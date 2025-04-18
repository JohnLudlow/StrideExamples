using Stride.Core.Mathematics;
using StrideExamples.StrideUI.Grid.Core;
using System.Runtime.Serialization;

namespace StrideExamples.StrideUI.Grid.Managers;

public class CubeDataManager
{
    private const string CubeDataFileName = "StrideExampleCubeData.yaml";
    private readonly DataSaver<CubeData> _dataSaver;

    public CubeDataManager()
    {
        _dataSaver = new DataSaver<CubeData>()
        {
            Data = new CubeData(),
            FileName = CubeDataFileName
        };
    }

    public async Task<List<Vector3>> LoadDataAsync()
    {
        var isSuccessful = await _dataSaver.TryLoadAsync();

        if (!isSuccessful) return [];

        return _dataSaver.Data.CubePositions.ConvertAll(s => new Vector3(s.X, s.Y, s.Y));
    }

    public void DeleteData() => _dataSaver.Delete();

    public async Task SaveDataAsync() => await _dataSaver.SaveAsync();

    public void UpdatePositions(List<Vector3> positions)
    {
        _dataSaver.Data.CubePositions.Clear();

        foreach(var position in positions)
            _dataSaver.Data.AddPosition(position);        
    }
}