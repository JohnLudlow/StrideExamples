using System.Collections.Concurrent;
using System.Reflection;
using Stride.Core;

namespace StrideExamples.Local.ImGui.UI;

public class Inspector : BaseWindow
{
  [Flags]
  public enum Filter : uint
  {
    Fields = 1,
    Properties = Fields << 1,
    SubTypes = Properties << 1,
    Public = SubTypes << 1,
    NonPublic = Public << 1,
    Static = NonPublic << 1,
    Instance = Static << 1,
    Inherited = Instance << 1,
  }

  private static readonly Filter[] _filterValues = Enum.GetValues<Filter>();
  private const float DummyWidth = 19;
  private const float Indentation2 = DummyWidth + 8;

  public delegate bool ValueHandler(string label, ref object value);
  public static ConcurrentDictionary<Type, ValueHandler> ValueHandlers => [];
  private static List<Inspector> _inspectors = [];
  private Dictionary<Type, TypeCache> _cachedTypeData = [];
  private HashSet<int> _openId = [];
  private WeakReference<object?> _target = new(null);

  // TODO: Refactor!
  // Settings
  /// <summary> Is this interface returned by <see cref="FindFreeInspector"/> </summary>
  public bool Locked = false;
  /// <summary> Show specialized interface to handle IEnumerable types </summary>
  public bool EnumerableView = true;
  /// <summary>
  /// For <see cref="Target"/> of type <see cref="System.Type"/>, return the content of 'static type.*' instead of 'typeof(type).*'
  /// </summary>
  public bool TypeAsStatic = true;

  public Filter MemberFilter
  {
    get => _memberFilter;
    set
    {
      if (_memberFilter == value) return;

      _memberFilter = value;
      _cachedTypeData.Clear();
    }
  }


  Filter _memberFilter = Filter.Public | Filter.Inherited | Filter.Properties | Filter.Fields | Filter.Instance;

  public object Target
  {
    get
    {
      if (_target.TryGetTarget(out var source))
        return _target;
      // TODO: Resolve warning
      return null;
    }
    set
    {
      if (_target == value)
        return;

      _target.SetTarget(value);
      _openId.Clear();
    }
  }

  WeakReference<object> _dicAddCommandTarget = new(null);
  (object key, object value) _dicAddCommandData;

  public Inspector(IServiceRegistry registry) : base(registry)
  {
    _inspectors.Add(this);
  }

  public static Inspector FindFreeInspector(IServiceRegistry services)
  {
    foreach (var inspector in _inspectors)
    {
      if (!inspector.Locked)
        return inspector;
    }

    return new Inspector(services);
  }

  protected override void OnDestroy()
  {
    _inspectors.Remove(this);
  }

  protected override void OnDraw(bool collapsed)
  {
    if (collapsed) return;

    Hexa.NET.ImGui.ImGui.Checkbox("Locked", ref Locked);
    using (UserInterfaceExtension.UCombo("Filter", MemberFilter.ToString(), out var open))
    {
      if (open)
      {
        foreach (var o in _filterValues)
        {
          var selected = (MemberFilter & o) == o;

          if (Hexa.NET.ImGui.ImGui.Selectable(o.ToString(), selected))
          {
            if (selected)
            {
              MemberFilter &= ~o;
            }
            else
            {
              MemberFilter |= o;
            }
          }
        }
      }
    }

    Hexa.NET.ImGui.ImGui.Checkbox("Enumerable view", ref EnumerableView);
    Hexa.NET.ImGui.ImGui.SameLine();
    Hexa.NET.ImGui.ImGui.Checkbox("Type as static ref", ref TypeAsStatic);
    Hexa.NET.ImGui.ImGui.Spacing();
    Hexa.NET.ImGui.ImGui.TextUnformatted($"Inspecting [{Target ?? "null"}]");
    Hexa.NET.ImGui.ImGui.SameLine();

    using (UserInterfaceExtension.Child())
    {
      if (Target != null)
        DrawMembers(Target, Target.GetType().GetHashCode());
    }
  }

  private bool DrawMembers(object target, int hashcodeSource)
  {
    if (target is null) return false;

    var type = TypeAsStatic && target is Type ? (Type)target : target.GetType();
    var members = GetTypeData(type).FilteredMembers;

    var hasChanged = false;

    using (UserInterfaceExtension.UIndent(Indentation2))
    {
      foreach (var member in members)
      {
        // TODO: Refactor!!!!!
        var value = default(object);
        var readOnly = default(bool);

        {
          try
          {
            if (member is FieldInfo fi)
            {
              value = fi.GetValue(target);
              readOnly = fi.IsInitOnly;
            }
            else if (member is PropertyInfo pi && pi.CanRead)
            {
              value = pi.GetValue(target);
              readOnly = !pi.CanWrite;
            }
            else if (member is Type asType)
            {
              value = asType;
              readOnly = true;
            }
            else
              throw new NotImplementedException($"UI handler for type {member.GetType()} not implemented");
          }
          catch (Exception e)
          {
            value = $"x Exception: {e.Message}";
            readOnly = true;
          }
        }
        // TODO: Implement
      }
    }

    return false;
  }

  TypeCache GetTypeData(Type t)
  {
    // TODO: refactor
    var output = default(TypeCache);

    if (_cachedTypeData.TryGetValue(t, out output))
    {
      return output;
    }

    output = new(t, MemberFilter);
    _cachedTypeData.Add(t, output);
    return output;
  }

  internal sealed class TypeCache
  {
    // TODO: Refactor!
    public readonly MemberInfo[] FilteredMembers;
    public readonly (Type key, Type value, MethodInfo getKey, MethodInfo getValue)? AsDictionary;
    public readonly Type AsList;
    public readonly (bool flags, Array values)? asEnum;
    private readonly Type _type;
    private readonly Filter _filter;

    public TypeCache(Type t, Filter filter)
    {
      _type = t;
      _filter = filter;
      // TODO: implement XMLDocumentation
    }

    private static IEnumerable<MemberInfo> GetAllMembers(Type t)
    {
      foreach (var member in t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
      {
        yield return member;
      }

      while (t is not null)
      {
        foreach (var member in t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
          yield return member;
        }

        t = t.BaseType;
      }
    }
  }  
}