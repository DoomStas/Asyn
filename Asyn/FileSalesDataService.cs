using System.Globalization;
namespace Asyn;

public class FileSalesDataService
{
    public async Task<List<SaleRecord>> LoadSalesDataAsync(string path, CancellationToken cancellationToken)
    {
        var list = new List<SaleRecord>(500_000);
        using var reader = new StreamReader(path);

        await reader.ReadLineAsync(cancellationToken);
        
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string line = await reader.ReadLineAsync(cancellationToken);
            string[] parts = line.Split(',');

            var record = new SaleRecord
            {
                Date = DateTime.ParseExact(parts[0].Trim(), "yyyy-MM-dd", null),
                Region = parts[1].Trim(),
                Product = parts[2].Trim(),
                Quantity = int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture),
                Price = decimal.Parse(parts[4].Trim(), CultureInfo.InvariantCulture),
            };
            list.Add(record);
        }

        return list;
    }

    public async Task SaveFilteredDataAsync(string path, List<SaleRecord> data,
        CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(path);
        await writer.WriteLineAsync("Date,Region,Product,Quantity,Price");
        
        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string line = $"{item.Date:yyyy-MM-dd},{item.Region},{item.Product},{item.Quantity},{item.Price}";
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}