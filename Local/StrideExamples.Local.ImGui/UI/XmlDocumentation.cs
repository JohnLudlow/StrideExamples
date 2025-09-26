using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Xml;
using FFmpeg.AutoGen;

namespace StrideExamples.Local.ImGui.UI;

public class XmlDocumentation
{
  static ConcurrentDictionary<Assembly, XmlDocument> _documents = [];
  static ConcurrentDictionary<MemberInfo, CachedDocumentation> _documentation = [];


  static bool TryGetDocumentation(MemberInfo member, out CachedDocumentation documentation)
  {
    if (!_documentation.TryGetValue(member, out documentation))
    {
      var asm = default(Assembly);
      if (member is Type t)
        asm = t.Assembly;
      else
        asm = member.DeclaringType.Assembly;

      if (!_documents.TryGetValue(asm, out var document))
      {
        var filepath = asm.Location;
        const string localPrefix = "file:///";
        if (filepath.StartsWith(localPrefix, StringComparison.InvariantCulture))
        {
          filepath = filepath[localPrefix.Length..];
          filepath = Path.ChangeExtension(filepath, ".xml");

          var streamReader = default(TextReader);

          try
          {
            streamReader = new StreamReader(filepath);
          }
          catch (FileNotFoundException)
          {
            streamReader = null;
          }

          if (streamReader is not null)
          {
            document = new XmlDocument();
            document.Load(streamReader);
          }
          else
            document = null;
        }
        else
        {
          document = null;
        }

        _documents.TryAdd(asm, document);
      }

      if (document is null)
        documentation = null;
      else
      {
        var fullname = default(string);
        switch (member)
        {
          case MethodInfo methodInfo:
            var parameters = "";

            foreach (var parameterInfo in methodInfo.GetParameters())
            {
              if (parameters.Length > 0)
                parameters += ",";

              parameters += parameterInfo.ParameterType.FullName;
            }

            if (parameters.Length > 0)
              parameters += $"({parameters})";

            fullname = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}{parameters}";

            break;

          case Type type:
            fullname = $"T:{type.FullName}";
            break;

          default:
            fullname = $"{member.MemberType.ToString()[0]}:{member.DeclaringType}.{member.Name}";
            break;
        }

        if (document["doc"]?["members"]?.SelectSingleNode($"member[@name='{fullname}']") is XmlElement element)
        {
          documentation = new CachedDocumentation(element);
        }
        else
        {
          documentation = null;
        }
      }
    }

    return false;
  }

  public class CachedDocumentation
  {
    private readonly Lock _lock = new();
    private string _cleanSummary;

    public XmlElement Element { get; }

    public string CleanSummary
    {
      get
      {
        lock (_lock)
        {
          return _cleanSummary ??= GetCleanSummary();
        }
      }
    }

    public CachedDocumentation(XmlElement elem)
    {
      Element = elem;
    }

    string GetCleanSummary()
    {
      var rawString = Element?.SelectSingleNode("summary")?.InnerXml;
      if (rawString is null) return string.Empty;

      rawString = WebUtility.HtmlDecode(rawString);
      rawString = rawString.Replace("<see cref=", "").Replace("/>", "");

      var final = "";
      foreach (var lines in rawString.Split(['\n'], StringSplitOptions.None))
      {
        var cleanLine = lines.Trim();

        if (string.IsNullOrWhiteSpace(final))
        {
          final += $"\n{cleanLine}";
        }
        else
          final = cleanLine;
      }

      return "";
    }
  }
}