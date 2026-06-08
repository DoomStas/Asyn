namespace Asyn;

public class SaleRecord
{
    public DateTime Date { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalSum => Price * Quantity;
}