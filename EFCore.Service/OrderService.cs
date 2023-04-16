using EFCore.DAL;
using EFCore.Model;
using EFCore.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Service;
public class OrderService : IOrderService
{
    private readonly DataContext context;

    public OrderService(DataContext context)
    {
        this.context = context;
    }

    public Order? Add(Order order)
    {
        this.context.Order.Add(order);
        this.context.SaveChanges();
        return order;
    }

    public void Delete(Order order)
    {
        this.context.Order.Remove(order);
        this.context.SaveChanges();
    }

    public List<Order> Search(Func<Order, bool> filter, bool loadRalatedData = false)
        => (loadRalatedData) ? this.context.Order.Include(p => p.Client).Where(filter).ToList() : this.context.Order.Where(filter).ToList();

    public Order? FindById(int orderId, bool loadRalatedData = false)
        => (loadRalatedData) ?
                this.context.Order
                    .Include(o => o.Client)
                    .Include(o => o.OrderItems)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefault(o => o.Id == orderId) :
                this.context.Order.Find(orderId);

}
