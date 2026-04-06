namespace OrderItemsReserver;

public class OrderRequest
{
    public int OrderId { get; set; }
    public List<OrderItemDetail> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string BuyerId { get; set; } = string.Empty;
}

public class OrderItemDetail
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public string ItemName { get; set; } = string.Empty;
}
