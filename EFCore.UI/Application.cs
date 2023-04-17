using EFCore.Model;
using EFCore.Shared.Interfaces;
using EFCore.Shared.Utility;
using Spectre.Console;
using System.Reflection.Metadata.Ecma335;

namespace EFCore.UI;
internal class Application
{
    private readonly Menu mainMenu = new();
    private readonly ICategoryService categories;
    private readonly IClientService clients;
    private readonly IProductService products;
    private readonly IOrderService orders;
    private bool eventLoop = true;

    public Application(
        ICategoryService categoryService,
        IClientService clientService,
        IProductService productService,
        IOrderService orderService)
    {
        InitMainMenu();
        this.categories = categoryService;
        this.clients = clientService;
        this.products = productService;
        this.orders = orderService;
    }

    #region Menu Init
    public void InitMainMenu()
    {
        var orders = this.mainMenu.AddItem("Orders");
        this.mainMenu.AddItem("New order",this.NewOrder, orders);
        var sfo = this.mainMenu.AddItem("Search for an order", parent: orders);
        this.mainMenu.AddItem("Get all orders by date span", this.GetOrdersByDates, sfo);
        this.mainMenu.AddItem("Get all orders by client id", this.GetOrdersByClientId, sfo);
        this.mainMenu.AddItem("Find all orders by a product id", this.GetOrdersByProductId, sfo);
        this.mainMenu.AddItem("Show order details by order id", this.ShowOrderDetails, orders);
        this.mainMenu.AddItem("Delete an order", this.DeleteOrder, orders);                                     
        
        var products = this.mainMenu.AddItem("Products");
        this.mainMenu.AddItem("New product", this.NewProduct, products);
        var sfp = this.mainMenu.AddItem("Search for a product", parent: products);
        this.mainMenu.AddItem("Get a product by name or id", this.FindProductByNameOrId, sfp);
        this.mainMenu.AddItem("Get all the products by categories", this.GetAllProductsByCategory, sfp);
        this.mainMenu.AddItem("Edit product", this.EditProduct, products);
        this.mainMenu.AddItem("Delete product", this.DeleteProduct, products);

        var categories = this.mainMenu.AddItem("Categories");
        this.mainMenu.AddItem("New category", this.NewCategory, categories);
        this.mainMenu.AddItem("Show all categories", this.ShowAllCategories, categories);
        this.mainMenu.AddItem("Edit category", this.EditCategory, categories);
        this.mainMenu.AddItem("Delete category", this.DeleteCategory, categories);

        var clients = this.mainMenu.AddItem("Clients");
        this.mainMenu.AddItem("New client", this.NewClient, clients);
        var fc = this.mainMenu.AddItem("Find client", parent: clients);
        this.mainMenu.AddItem("Find client by last name or id", this.FindClientByLastNameOrId, fc);
        this.mainMenu.AddItem("Find client by email", this.FindClientByEmail, fc);
        this.mainMenu.AddItem("Find client by phone number", this.FindClientByPhone, fc);
        this.mainMenu.AddItem("Edit client info", this.EditClientInfo, clients);
        this.mainMenu.AddItem("Delete client", this.DeleteClient, clients); 

        this.mainMenu.AddItem("Exit", () => this.eventLoop = false);
    }
        
    #endregion

    #region Entry Point

    public void Run()
    {
        while(this.eventLoop)
        {
            try
            {
                this.mainMenu.Show()?.Invoke();
            }
            catch(InvalidDataException er)
            {
                Console.WriteLine(er.Message);
            }
            Console.WriteLine("Press any key to continue");
            _ = Console.ReadKey();
        }
    }

    #endregion

    #region Selection functionality

    private Product? SelectProduct()
    {
        Console.Write("Specify the product's id (or products's name to search for specific one): > ");
        string? productInfo = Console.ReadLine();
        if (string.IsNullOrEmpty(productInfo))
        {
            Console.WriteLine("  ---   No data to search for a product   ---");
            return null;
        }
        if (!int.TryParse(productInfo, out int productId))
        {
            List<Product> products = this.products.GetProductsByName(productInfo!);
            AnsiConsole.Clear();
            if (products.Count < 1)
            {
                Console.WriteLine("No such products found!");
                return null;
            }
            return AnsiConsole.Prompt(
                new SelectionPrompt<Product>()
                    .AddChoices(products)
                    .UseConverter(p => $"{p.Id} {p.Name} {p.Price}")
            );
        }
        return this.products.GetProductById(productId);
    }    

    private Client? SelectClient()
    {
        Console.Write("Specify the customer's id (or customer's last name to search for specific one): > ");
        string? clientInfo = Console.ReadLine();
        if (string.IsNullOrEmpty(clientInfo))
        {
            Console.WriteLine("  ---   No data to search for a client   ---");
            return null;
        }
        if (!int.TryParse(clientInfo, out int clientId))
        {
            List<Client> clients = this.clients.GetClientsByLastName(clientInfo!);
            AnsiConsole.Clear();
            if(clients.Count < 1)
            {
                Console.WriteLine("No such clients found!");
                return null;
            }
            return AnsiConsole.Prompt(
                new SelectionPrompt<Client>()
                    .AddChoices(clients)
                    .UseConverter(c => $"{c.Id} {c.LastName} {c.FirstName} {c.Email}")
            );
        }
        return this.clients.GetClientById(clientId);
    }    

    private Category? SelectCategory()
    {
        AnsiConsole.Clear();
        Console.Write("Specify the category id (or part of the category name to search for specific one): > ");
        string? catInfo = Console.ReadLine();
        if (string.IsNullOrEmpty(catInfo))
        {
            Console.WriteLine("  ---   No data to search for a category   ---");
            return null;
        }
        if (!int.TryParse(catInfo, out int catId))
        {
            List<Category> categories = this.categories.GetCategoriesByName(catInfo!);
            AnsiConsole.Clear();
            if (categories.Count < 1)
            {
                Console.WriteLine("No such categories found!");
                return null;
            }
            return AnsiConsole.Prompt(
                new SelectionPrompt<Category>()
                    .AddChoices(categories)
                    .UseConverter(c => $"{c.Id} {c.Name}")
            );
        }
        return this.categories.GetCategoryById(catId);
    }

    #endregion

    #region Menu Handlers

    #region Orders Section
    private void NewOrder()
    {
        Console.WriteLine("   --- Making a new order ---");
        Client? client = this.SelectClient() ?? throw new InvalidDataException("Unable to create an order without a client info");
        Console.WriteLine("Add products to the order:");
        var order = new Order()
        {
            IssueDateTime = DateTime.Now,
            Client = client
        };
        do
        {
            Product? current = this.SelectProduct();
            if (current is not null)
            {
                int quantity = AnsiConsole.Prompt(new TextPrompt<int>("Quantity > "));
                order.OrderItems.Add(new OrderItem()
                {
                    Price = current.Price,
                    Product = current,
                    Quantity = quantity
                });
            }
        } while (AnsiConsole.Confirm("Add more products?"));
        if (order.OrderItems.Count < 1)
            throw new InvalidDataException("Unable to make an order without any products in it");
        this.orders.Add(order);
        Console.WriteLine($"Added new order:" +
            $"\nID      : {order.Id}" +
            $"\nDate    : {order.IssueDateTime}" +
            $"\nClient  : {order.Client.LastName} {order.Client.FirstName}");
    }
    private void ShowOrderDetails()
    {
        Console.Write("Input order id: ");
        int orderId;
        if (!int.TryParse(Console.ReadLine(), out orderId))
        {
            Console.WriteLine($"Not a valid id");
            return;
        }
        Order? order = this.orders.FindById(orderId, true);
        if(order is null)
        {
            Console.WriteLine($"Unable to find an order by id [{orderId}]");
            return;
        }
        Console.WriteLine(  $"Id        : {order.Id}\n" +
                            $"Client    : {order.Client.LastName} {order.Client.FirstName}\n" +
                            $"Issue date: {order.IssueDateTime.Date}");
        if(order.OrderItems.Count < 1)
        {
            Console.WriteLine("No products in the order");
            return;
        }
        foreach(var item in order.OrderItems)
            Console.WriteLine($" > product: [ {item.Product.Name,-60} ] quantity: [ {item.Quantity} ] price: [ {item.Price} ]");
    }    
    private void GetOrdersByDates()
    {
        var startDate = ConsoleUtilities.ReadDateTime("Input start date: ");
        var endDate = ConsoleUtilities.ReadDateTime("Input end date: ");
        var result = this.orders.Search(o => o.IssueDateTime.IsBetweenIncluded(startDate,endDate), true);
        Console.WriteLine("All the orders made between specified dates:");
        foreach (Order order in result)
            Console.WriteLine($"Order id: [{order.Id,-4}] By client: [{order.Client.LastName,-12}{order.Client.FirstName,-12}] {order.IssueDateTime}");
    }
    private void GetOrdersByClientId()
    {
        var startId = ConsoleUtilities.ReadNumber("Input start ID: ");
        var endId = ConsoleUtilities.ReadNumber("Input end ID: ");
        var result = this.orders.Search(o => (o.Id >= startId && o.Id <= endId), true);
        Console.WriteLine("All the orders made between specified IDs:");
        foreach (Order order in result)
            Console.WriteLine($"Order id: [{order.Id,-4}] By client: [{order.Client.LastName,-12}{order.Client.FirstName,-12}] {order.IssueDateTime}");
    }
    private void GetOrdersByProductId()
    {
        var startId = ConsoleUtilities.ReadNumber("Input start ID: ");
        var endId = ConsoleUtilities.ReadNumber("Input end ID: ");
        var result = this.orders.Search(_ => true, true).Where(p => p.OrderItems.Count(i => i.Product.Id >= startId && i.Product.Id <= endId) != 0).ToArray();
        Console.WriteLine("All the orders made between specified product IDs:");
        foreach (var order in result)
            Console.WriteLine($"Order id: [{order.Id,-4}] By client: [{order.Client.LastName,-12}{order.Client.FirstName,-12}] {order.IssueDateTime}");
    }    
    private void DeleteOrder()
    {
        var id = ConsoleUtilities.ReadNumber("Input an ID: ");
        this.orders.Delete(this.orders?.FindById(id, true) ?? new Order());
        Console.WriteLine("The order has been successfully deleted");
    }
    #endregion

    #region Products Section

    private void NewProduct()
    {
        Console.WriteLine("   --- Making a new product ---");
        var product = new Product()
        {
            Name = AnsiConsole.Prompt(new TextPrompt<string>("Product name: ")),
            Description = AnsiConsole.Prompt(new TextPrompt<string>("Product description: ")),
            Price = AnsiConsole.Prompt(new TextPrompt<decimal>("Product price: "))
        };
        while (AnsiConsole.Confirm("Do you want to add a category to the product?"))
        {
            Category? category = this.SelectCategory();
            if (category is not null)
            {
                product.Categories.Add(category);
                Console.WriteLine($"[{product.Name}] has been assigned the category [{category.Name}]");
            }            
        }
        this.products.Add(product);
    }

    private void FindProductByNameOrId()
    {
        Product? product = this.SelectProduct();
        if(product is null)
        {
            Console.WriteLine("No products found");
            return;
        }
        if (product.Categories.Count < 1)
            this.products.LoadCategories(product);
        Console.WriteLine($"Id            : {product.Id}\n" +
                          $"Name          : {product.Name}\n" +
                          $"Description   : {product.Description}\n" +
                          $"Price         : {product.Price}\n" +
                          $"Categories    : {
                           (product.Categories.Count < 1 ? "<NO CATEGORIES>" : string.Join(", ",product.Categories.Select(c => c.Name).ToList()))}");
    }

    private void GetAllProductsByCategory()
    {
        Category? category = this.SelectCategory();
        if (category is null)
        {
            Console.WriteLine("No category found");
            return;
        }
        if (category.Products.Count < 1)
            this.categories.LoadProducts(category);
        foreach(Product product in category.Products)
            Console.WriteLine($"product id: [ {product.Id,-4} ] name: [ {product.Name,-40} ] price: [ {product.Price} ]");
    }

    private void EditProduct()
    {
        var product = this.SelectProduct();

        Console.WriteLine("What do you want to edit?\n" +
            "[1] => Name\n" +
            "[2] => Description\n" +
            "[3] => Price\n" +
            "[Anything else] => Quit\n");
        Console.Write("Choose: ");
        if (int.TryParse(Console.ReadLine(), out int result) && result > 0 && result < 3) 
        {
            Product updateData = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
            updateData.Categories.AddRange(product.Categories);
            switch (result)
            {
                case 1:
                    Console.Write("Enter the new product name: ");
                    updateData.Name = Console.ReadLine();
                    break;
                case 2:
                    Console.Write("Enter the new description: ");
                    updateData.Description = Console.ReadLine();
                    break;
                case 3:
                    updateData.Price = ConsoleUtilities.ReadDecimalNumber("Enter the new price: ");
                    break;
            }

            this.products.Edit(updateData);
            Console.WriteLine("The product data has been successfully updated");
        }
    }

    private void DeleteProduct()
    {
        this.products.Delete(this.SelectProduct());
        Console.WriteLine("The product has been successfully deleted");
    }

    #endregion

    #region Categories Section

    private void NewCategory()
    {
        Console.WriteLine("   --- Making a new category ---");
        Console.Write("Category name: ");
        string catName = Console.ReadLine()!;
        this.categories.Add(catName);        
    }

    private void ShowAllCategories()
    {
        List<Category> categories = this.categories.GetCategoriesByName("");
        foreach (Category category in categories)
            Console.WriteLine($"ID: {category.Id,-5} | Name: {category.Name, -20}");
    }

    private void EditCategory()
    {
        var category = this.SelectCategory();
        Console.Write("Enter the new name of the selected category: ");
        string? name = Console.ReadLine();
        Category updatedData = new Category() { Id = category.Id, Name = name };
        updatedData.Products.AddRange(category.Products);
        this.categories.Edit(updatedData);
        Console.WriteLine("The product data has been successfully updated");
    }

    private void DeleteCategory()
    {
        this.categories.Delete(this.SelectCategory()?.Name ?? string.Empty);
        Console.WriteLine("The catogry has been successfully deleted");
    }

    #endregion

    #region Clients Section

    private void NewClient()
    {
        Console.WriteLine("   --- Adding a new client ---");
        Console.Write("Last name: ");
        string? lastName = Console.ReadLine();
        if (string.IsNullOrEmpty(lastName))
            throw new InvalidDataException("Last name can not be empty");
        Console.Write("First name: ");
        string? firstName = Console.ReadLine();
        if (string.IsNullOrEmpty(firstName))
            throw new InvalidDataException("First name can not be empty");
        Console.Write("Email: ");
        string? email = Console.ReadLine();
        if (string.IsNullOrEmpty(email))
            throw new InvalidDataException("Email can not be empty");
        if (this.clients.IsEmailInUse(email))
            throw new InvalidDataException("This email is already in use");
        Console.Write("Phone: ");
        string? phone = Console.ReadLine();
        this.clients.Add(new()
        {
            LastName = lastName!,
            FirstName = firstName!,
            Email = email!,
            Phone = string.IsNullOrEmpty(phone) ? null : phone
        });
    }
    private void DeleteClient()
    {
        this.clients.Delete(this.SelectClient() ?? new Client());
        Console.WriteLine("The client has been successfully deleted");
    }
    private void EditClientInfo()
    {
        var client = this.SelectClient();

        Console.WriteLine("What do you want to edit?\n" +
            "[1] => First Name\n" +
            "[2] => Last Name\n" +
            "[3] => Email\n" +
            "[4] => Phone\n" +
            "[Anything else] => Quit\n");
        Console.Write("Choose: ");
        if (int.TryParse(Console.ReadLine(), out int result) && result > 0 && result < 4)
        {
            Client? updateData = new()
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone
            };
            updateData.Orders.AddRange(client.Orders);
            switch (result)
            {
                case 1:
                    updateData.FirstName = ConsoleUtilities.ReadNames("Enter the new first name: ");
                    break;
                case 2:
                    updateData.LastName = ConsoleUtilities.ReadNames("Enter the new last name: ");
                    break;
                case 3:
                    updateData.Email = ConsoleUtilities.ReadEmail("Enter the new email: ");
                    break;
                case 4:
                    updateData.Phone = ConsoleUtilities.ReadPhoneNumber("Enter the new phone number: ");
                    break;
            }

            this.clients.Edit(updateData);

            Console.WriteLine("The product data has been successfully updated");
        }
        Console.WriteLine("Error");
    }
    private void FindClientByPhone()
    {
        Client? client = this.clients.GetClientByPhoneNumber(ConsoleUtilities.ReadPhoneNumber("Input phone number: "));
        if (client is null)
        {
            Console.WriteLine("No clients found");
            return;
        }
        if (client.Orders.Count < 1)
            this.clients.LoadOrders(client);
        Console.WriteLine($"Id                 : {client.Id}\n" +
                          $"First Name         : {client.FirstName}\n" +
                          $"Las Name           : {client.LastName}\n" +
                          $"Email              : {client.Email}\n" +
                          $"Phone              : {client.Phone}\n" +
                          $"Order Issue Dates  : {(client.Orders.Count < 1 ? "<NO ORDERS>" : string.Join(", ", client.Orders.Select(c => c.IssueDateTime.ToShortDateString()).ToList()))}");
    }
    private void FindClientByEmail()
    {
        Client? client = this.clients.GetClientByEmail(ConsoleUtilities.ReadEmail("Input phone number: "));
        if (client is null)
        {
            Console.WriteLine("No clients found");
            return;
        }
        if (client.Orders.Count < 1)
            this.clients.LoadOrders(client);
        Console.WriteLine($"Id                 : {client.Id}\n" +
                          $"First Name         : {client.FirstName}\n" +
                          $"Las Name           : {client.LastName}\n" +
                          $"Email              : {client.Email}\n" +
                          $"Phone              : {client.Phone}\n" +
                          $"Order Issue Dates  : {(client.Orders.Count < 1 ? "<NO ORDERS>" : string.Join(", ", client.Orders.Select(c => c.IssueDateTime.ToShortDateString()).ToList()))}");
    }
    private void FindClientByLastNameOrId()
    {
        Client? client = SelectClient();
        if (client is null)
        {
            Console.WriteLine("No clients found");
            return;
        }
        if (client.Orders.Count < 1)
            this.clients.LoadOrders(client);
        Console.WriteLine($"Id                 : {client.Id}\n" +
                          $"First Name         : {client.FirstName}\n" +
                          $"Las Name           : {client.LastName}\n" +
                          $"Email              : {client.Email}\n" +
                          $"Phone              : {client.Phone}\n" +
                          $"Order Issue Dates  : {(client.Orders.Count < 1 ? "<NO ORDERS>" : string.Join(", ", client.Orders.Select(c => c.IssueDateTime.ToShortDateString()).ToList()))}");
    }

    #endregion

    #endregion
}
