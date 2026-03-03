using System.Collections.Concurrent;

public static class OrdersStore
{
    private static ConcurrentDictionary<string, Order> _orders = new();

    public static void Save(Order order) => _orders[order.Id] = order;
    public static Order? Get(string id) => _orders.TryGetValue(id, out var o) ? o : null;
}

public class Order
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "inr";
    public List<OrderItem> Items { get; set; } = new();
    public bool Paid { get; set; }
}

public class OrderItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Qty { get; set; }
}
