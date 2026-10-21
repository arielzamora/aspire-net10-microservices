using CsvHelper;
using MasterNet.Application.Interfaces;
using MasterNet.Domain.Abstractions;
using System.Globalization;
using System.Text;

namespace MasterNet.Infrastructure.Reports;

public class ReportService<T> : IReportService<T>
    where T : BaseEntity
{
    public Task<MemoryStream> GetCsvReport(List<T> records)
    {
        // Create stream we will return (do not dispose it here)
        var memoryStream = new MemoryStream();

        // Use StreamWriter that leaves the underlying stream open when disposed
        using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
        using (var csvWriter = new CsvWriter(textWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(records);
            textWriter.Flush();
        }

        // Reset position so caller can read from start
        memoryStream.Seek(0, SeekOrigin.Begin);

        return Task.FromResult(memoryStream);
    }
}

