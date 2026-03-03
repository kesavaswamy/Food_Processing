public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public List<OrderItem>? Items { get; set; }
}
