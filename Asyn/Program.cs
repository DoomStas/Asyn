using System.Diagnostics;
using System.Globalization;

namespace Asyn;

public class SaleRecord
{
    public DateTime Date { get; set; }
    public string Region { get; set; }
    public string Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalSum => Price * Quantity;
}

class Program
{
    private static readonly string FilePath = "sales.csv";
    private static readonly string OutputPath = "filtered_sales.csv";

    static async Task Main(string[] args)
    {
        //1.Generation file
        GenerateSalesFileIfNotExist(500_000);

        using var cts = new CancellationTokenSource();
        Console.WriteLine("Press Ctrl+C to exit");
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Exiting...");
            cts.Cancel();
            e.Cancel = true;
        };

        Console.WriteLine("Loading file in fone");

        Task<List<SaleRecord>> loadTask = Task.Run(() => LoadSalesData(FilePath, cts.Token));

        Console.WriteLine("Loading file... - enter parameters: ");

        try
        {
            Console.Write("Region: ");
            string regionFilter = Console.ReadLine();

            Console.Write("Min sum: ");
            string minSumInput = Console.ReadLine();
            decimal minSum = string.IsNullOrWhiteSpace(minSumInput) ? 0: decimal.Parse(minSumInput, CultureInfo.InvariantCulture);

            Console.Write("Date from (yyyy-mm-dd): ");
            string dateFromInput = Console.ReadLine();
            DateTime dateFrom = string.IsNullOrWhiteSpace(dateFromInput)
                ? DateTime.MinValue
                : DateTime.ParseExact(dateFromInput, "yyyy-MM-dd", null);

            Console.Write("Date to (yyyy-MM-dd): ");
            string dateToInput = Console.ReadLine();
            DateTime dateTo = string.IsNullOrWhiteSpace(dateToInput)
                ? DateTime.MaxValue
                : DateTime.ParseExact(dateToInput, "yyyy-MM-dd", null);

            Console.WriteLine("Waiting for the background file loading to complete...");
            List<SaleRecord> allSales = await loadTask;

            Console.WriteLine($"File loaded successfully: {allSales.Count} records");

            var sw = Stopwatch.StartNew();

            List<SaleRecord> filteredSales = allSales.FindAll(s =>
                (string.IsNullOrWhiteSpace(regionFilter) ||
                 s.Region.Equals(regionFilter, StringComparison.OrdinalIgnoreCase)) &&
                s.TotalSum >= minSum &&
                s.Date >= dateFrom &&
                s.Date <= dateTo);

            Console.WriteLine($"Records found: {filteredSales.Count}");

            await SaveFilteredDataAsync(OutputPath, filteredSales, cts.Token);
            sw.Stop();
            Console.WriteLine($"Result saved to: {OutputPath}");
            Console.WriteLine($"Filtration and saving completed in: {sw.Elapsed}");
        }

        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    private static List<SaleRecord> LoadSalesData(string path, CancellationToken cancellationToken)
    {
        var list = new List<SaleRecord>(500_000);
        using var reader = new StreamReader(path);

        string header = reader.ReadLine();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string line = reader.ReadLine();
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

    private static async Task SaveFilteredDataAsync(string path, List<SaleRecord> data,
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

    private static void GenerateSalesFileIfNotExist(int rowCount)
    {
        if (File.Exists(FilePath)) return;
        Console.WriteLine($"[Generator] File {FilePath} not found. Generation {rowCount} rows");
        
        string[] regions = { "Київ", "Львів", "Одеса", "Харків", "Дніпро" };
        string[] products = { "Ноутбук", "Мишка", "Клавіатура", "Монітор", "Навушники" };
        var rand =  new Random();

        using (var writer = new StreamWriter(FilePath))
        {
            writer.WriteLine("Date,Region,Product,Quantity,Price");
            for (int i = 0; i < rowCount; i++)
            {
                DateTime date = DateTime.Today.AddDays(-rand.Next(1, 365));
                string region = regions[rand.Next(regions.Length)];
                string product = products[rand.Next(products.Length)];
                int quantity = rand.Next(1, 10);
                decimal price = rand.Next(300, 45000);
                
                writer.WriteLine($"{date:yy-MM-dd},{region},{product},{quantity},{price}");
            }
        }
        Console.WriteLine($"[Generator] File {FilePath} generated successfully.");
    }

}