using EFCore.DAL;
using EFCore.Model;
using EFCore.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Service;
public class ProductService : IProductService
{
    private readonly DataContext context;

    public ProductService(DataContext context)
    {
        this.context = context;
    }

    public List<Product> GetProductsByName(string productName)
        => this.context.Product.Where(p => p.Name.ToLower().Contains(productName.ToLower())).ToList();

    public Product? GetProductById(int id) => this.context.Product.Find(id);

    public Product Add(Product product)
    {
        this.context.Product.Add(product);
        this.context.SaveChanges();
        return product;
    }

    public int LoadCategories(Product product)
    {
        this.context.Product.Entry(product).Collection(p => p.Categories).Load();
        return product.Categories.Count;
    }

    public List<Product> Search(Func<Product, bool> filter, bool loadRalatedData = false)
        => (loadRalatedData) ? this.context.Product.Include(p => p.Categories).Where(filter).ToList() : this.context.Product.Where(filter).ToList();

    public void Delete(Product? product)
    {
        this.context.Product.Remove(product ?? new Product());
        this.context.SaveChanges();
    }

    public void Edit(Product updatedData)
    {
        this.context.Update(updatedData);
        this.context.SaveChanges();
    }
}
