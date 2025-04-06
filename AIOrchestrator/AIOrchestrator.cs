namespace AIOrchestrator;

using AIOrchestrator.Support;

public class AIManager
{
    private readonly AIInstanceHandler _chatHandler = new();
    private readonly FilesHelper _filesHelper = new();
    private readonly ParquetHelper _parquetHelper = new();
    public async Task StartChatAsync()
    {
        // var datasetFilePath = this._filesHelper.GetDatasetFilePath(DatasetFiles.RecurvMedical);
        // var datasetString = await ParquetHelper.ReadParquetAsString(datasetFilePath);

        // this._chatHandler.SetDatasetContent(datasetString);

        await _chatHandler.ConversationAsync();
    }
}