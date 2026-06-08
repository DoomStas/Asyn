using System.Diagnostics;
using System.Globalization;

namespace Asyn;
class Program
{
    private static readonly string FilePath = "sales.csv";
    private static readonly string OutputPath = "filtered_sales.csv";

    static async Task Main(string[] args)
    {
        //1.Generation file
        SalesGenerator.GenerateSalesFileIfNotExist(FilePath,500_000);

        using var cts = new CancellationTokenSource();
        Console.WriteLine("Press Ctrl+C to exit");
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Exiting...");
            cts.Cancel();
            e.Cancel = true;
        };

        Console.WriteLine("Loading file in fone");

        var dataService = new FileSalesDataService();
        Task<List<SaleRecord>> loadTask = dataService.LoadSalesDataAsync(FilePath, cts.Token);

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

            await dataService.SaveFilteredDataAsync(OutputPath, filteredSales, cts.Token);
            sw.Stop();
            Console.WriteLine($"Result saved to: {OutputPath}");
            Console.WriteLine($"Filtration and saving completed in: {sw.Elapsed}");
        }

        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}