namespace AIOrchestrator.Support;

using System.Text;
using Parquet;

public class ParquetHelper
{
    private const string _separator = "\n\n\n\n\n\n";

    public static async Task<string> ReadParquetAsStringAsync(string filePath)
    {
        var options = new ParquetOptions { TreatByteArrayAsString = true };

        using (Stream fileStream = File.OpenRead(filePath))
        {
            using (var reader = await ParquetReader.CreateAsync(fileStream))
            {
                StringBuilder stringBuilder = new();
                for (var i = 0; i < reader.RowGroupCount; i++)
                {
                    var group = await reader.ReadEntireRowGroupAsync(i);
                    foreach (var column in group)
                    {
                        stringBuilder.AppendLine(
                            string.Join(
                                ", ",
                                ((IEnumerable<object>)column.Data).Select(d =>
                                    d?.ToString() + _separator
                                )
                            )
                        );
                    }
                }

                return stringBuilder.ToString();
            }
        }
    }
}
