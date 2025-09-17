using Stride.Core;
using Stride.Core.Mathematics;

namespace StrideExamples.Community.StrideUI.Grid.Core;

[DataContract]
public sealed class CubeData
{
    [DataMember()]
    public List<SimpleVector> CubePositions {get; set;} = [];

    public void AddPosition(Vector3 vector) 
        => CubePositions.Add(new SimpleVector(vector.X, vector.Y, vector.Z));
}