using EFCore.DAL;
using EFCore.Model;
using EFCore.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Windows.Markup;

namespace EFCore.Service;
public class ClientService : IClientService
{
    private readonly DataContext context;

    public ClientService(DataContext context)
    {
        this.context = context;
    }

    public Client? GetClientByPhoneNumber(string phoneNumber)
        => this.context.Client.Where(c => c.Phone == phoneNumber).FirstOrDefault();

    public List<Client> GetClientsByLastName(string lastName)
        => this.context.Client.Where(c => c.LastName.ToLower().Contains(lastName.ToLower())).ToList();

    public Client? GetClientById(int id) => this.context.Client.Find(id);

    public bool IsEmailInUse(string email) => this.context.Client.Where(c => c.Email == email).Any();
    public Client? Add(Client client)
    {
        Client? added;
        try
        {
            added = this.context.Client.Add(client).Entity;
            this.context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            added = null;
        }
        return added;
    }

    public void Delete(Client client)
    {
        this.context.Client.Remove(client);
        this.context.SaveChanges();
    }

    public Client? GetClientByEmail(string email)
        => this.context.Client.Where(c => c.Email == email).FirstOrDefault();

    public int LoadOrders(Client client)
    {
        this.context.Client.Entry(client).Collection(p => p.Orders).Load();
        return client.Orders.Count;
    }

    public void Edit(int property, object value, Client client)
    {
        switch (property)
        {
            case 1: //First Name
                {
                    Client c = this.context.Client.Where(p => p.Equals(value)).First();
                    c.FirstName = value as string ?? client.FirstName;
                }
                break;
            case 2: //Last Name
                {
                    Client c = this.context.Client.Where(p => p.Equals(value)).First();
                    c.LastName = value as string ?? client.LastName;
                }
                break;
            case 3: //Email
                {
                    Client c = this.context.Client.Where(p => p.Equals(value)).First();
                    c.Email = value as string ?? client.Email;
                }
                break;
            case 4: //Phone
                {
                    Client c = this.context.Client.Where(p => p.Equals(value)).First();
                    c.Phone = value as string ?? client.Phone;
                }
                break;
        }
        this.context.SaveChanges();
    }
}
