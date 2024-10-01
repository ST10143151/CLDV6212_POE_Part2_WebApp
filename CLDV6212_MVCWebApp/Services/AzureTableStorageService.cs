using Azure;
using Azure.Data.Tables;
using ABC_Retailers.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TableStorageService
{
    private readonly TableClient _tableClient;
    private readonly TableClient _customerTableClient;
    private readonly TableClient _orderStatusTableClient;

    public TableStorageService(string connectionString)
    {
        _tableClient = new TableClient(connectionString, "Product");
        _customerTableClient = new TableClient(connectionString, "Customer");
        _orderStatusTableClient = new TableClient(connectionString, "OrderStatus");
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();

        await foreach (var product in _tableClient.QueryAsync<Product>())
        {
            products.Add(product);
        }

        return products;
    }

    public async Task AddProductAsync(Product product)
    {
        if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _tableClient.AddEntityAsync(product);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding product to Table Storage", ex);
        }
    }

    public async Task DeleteProductAsync(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var customers = new List<Customer>();

        await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
        {
            customers.Add(customer);
        }

        return customers;
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _customerTableClient.AddEntityAsync(customer);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding customer to Table Storage", ex);
        }
    }

    public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
    {
        await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task AddOrderStatusAsync(OrderStatus orderStatus)
    {
        if (string.IsNullOrEmpty(orderStatus.PartitionKey) || string.IsNullOrEmpty(orderStatus.RowKey))
        {
            throw new ArgumentException("PartitionKey and RowKey must be set.");
        }

        try
        {
            await _orderStatusTableClient.AddEntityAsync(orderStatus);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error adding order status to Table Storage", ex);
        }
    }

    public async Task<List<OrderStatus>> GetAllOrderStatusesAsync()
    {
        var orderStatuses = new List<OrderStatus>();

        await foreach (var orderStatus in _orderStatusTableClient.QueryAsync<OrderStatus>())
        {
            orderStatuses.Add(orderStatus);
        }

        return orderStatuses;
    }

    public async Task<Customer?> GetCustomerByUsernameAsync(string username)
    {
        try
        {
            var queryResult = _customerTableClient.QueryAsync<Customer>(c => c.Email == username);
            var enumerator = queryResult.AsPages().GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
            {
                var firstCustomer = enumerator.Current.Values.FirstOrDefault();
                return firstCustomer;
            }
            return null;
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Azure Table Storage request failed: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteOrderStatusAsync(string partitionKey, string rowKey)
    {
        try
        {
            await _orderStatusTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Error deleting order status from Table Storage", ex);
        }
    }
}
