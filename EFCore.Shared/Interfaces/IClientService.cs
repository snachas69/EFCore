﻿using EFCore.Model;

namespace EFCore.Shared.Interfaces;
public interface IClientService
{
    Client? Add(Client client);
    void Delete(Client client);
    void Edit(int property, object value, Client client);
    Client? GetClientById(int id);
    List<Client> GetClientsByLastName(string lastName);
    Client? GetClientByPhoneNumber(string phoneNumber);
    Client? GetClientByEmail(string email);
    bool IsEmailInUse(string email);
    int LoadOrders(Client client);
}