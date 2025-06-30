using BusinessObjects.Models;

public class BillDetailDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public OrderDto Order { get; set; }
    public string? CustomerName { get; set; }
    public string? Phone { get; set; }
    public string Address { get; set; }
    public string Status { get; set; }
    public string? Note { get; set; }
    public string Payment { get; set; }
    public int VoucherPercent { get; set; }
    public string Shipper { get; set; }
    public string VoucherCode { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public string ProductOptionName { get; set; }
    public string ProductOptionSize { get; set; }
    public int? Quantity { get; set; }
    public double? Price { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public DateTime? DateTime { get; set; }
    public double? TotalPrice { get; set; }
}

