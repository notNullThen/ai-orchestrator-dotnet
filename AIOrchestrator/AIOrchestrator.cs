namespace AIOrchestrator;

using AIOrchestrator.Support;

public class AIManager
{
    private readonly FilesHelper _filesHelper = new();
    public async Task StartAsync()
    {
        var datasetFilePath = _filesHelper.GetDatasetFilePath(DatasetFiles.RecurvMedical);
        var datasetString = await ParquetHelper.ReadParquetAsStringAsync(datasetFilePath);
    }
}