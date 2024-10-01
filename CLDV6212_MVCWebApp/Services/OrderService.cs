using ABC_Retailers.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class OrderService
{
    private readonly TableStorageService _tableStorageService;
    private readonly CartService _cartService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderService(TableStorageService tableStorageService, CartService cartService, IHttpContextAccessor httpContextAccessor)
    {
        _tableStorageService = tableStorageService;
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task CheckoutAsync()
    {
        var cart = _cartService.GetCart();
        if (cart.Count == 0) return;  // If cart is empty, exit

        var username = _httpContextAccessor.HttpContext.Session.GetString("Username");

        // Assuming you have a way to retrieve the Customer_ID based on the username
        int customerId = await GetCustomerIdByUsernameAsync(username);
        if (customerId == 0)
        {
            throw new InvalidOperationException("Invalid customer. Cannot proceed with checkout.");
        }

        foreach (var item in cart)
        {
            if (!int.TryParse(item.ProductId, out int productId))
            {
                throw new InvalidOperationException("Invalid Product ID. Cannot convert to an integer.");
            }

            var orderStatus = new OrderStatus
            {
                PartitionKey = "OrderStatusesPartition",
                RowKey = Guid.NewGuid().ToString(),
                Customer_ID = customerId,  // Customer_ID is an int
                Product_ID = productId,    // Product_ID is an int
                OrderStatus_Location = "Online", // Default location for online orders
                OrderStatus_Date = DateTime.UtcNow,
            };

            await _tableStorageService.AddOrderStatusAsync(orderStatus);
        }

        _cartService.ClearCart(); // Clear the cart after successful checkout
    }

    private async Task<int> GetCustomerIdByUsernameAsync(string username)
    {
        var customer = await _tableStorageService.GetCustomerByUsernameAsync(username);
        return customer?.Customer_Id ?? 0; // Return 0 if customer not found
    }
}
