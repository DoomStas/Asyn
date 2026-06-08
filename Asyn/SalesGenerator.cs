namespace Asyn;

public class SalesGenerator
{
    public static void GenerateSalesFileIfNotExist(string FilePath, int rowCount)
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