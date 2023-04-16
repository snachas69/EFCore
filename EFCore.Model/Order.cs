namespace EFCore.Model;

public class Order
{
    public int Id { get; set; }
    public Client Client { get; set; } = null!;
    public DateTime IssueDateTime { get; set; }
    public List<OrderItem> OrderItems { get; } = new();    
}