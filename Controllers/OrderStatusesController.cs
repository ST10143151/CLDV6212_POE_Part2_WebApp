using ABC_Retailers.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ABC_Retailers.Services;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class OrderStatusesController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly QueueService _queueService;
    private readonly OrderService _orderService;
    private readonly HttpClient _httpClient;

    public OrderStatusesController(TableStorageService tableStorageService, QueueService queueService, OrderService orderService, HttpClient httpClient)
    {
        _tableStorageService = tableStorageService;
        _queueService = queueService;
        _orderService = orderService;
        _httpClient = httpClient;
    }

    // Action to display all order statuses
    public async Task<IActionResult> Index()
    {
        var orderstatuses = await _tableStorageService.GetAllOrderStatusesAsync();
        return View(orderstatuses);
    }

    // Action to render the registration form for new orders
    // Action to render the registration form for new orders
public async Task<IActionResult> Register()
{
    var customers = await _tableStorageService.GetAllCustomersAsync();
    var products = await _tableStorageService.GetAllProductsAsync();

    if (customers == null || customers.Count == 0)
    {
        ModelState.AddModelError("", "No customers found. Please add customers first.");
        return View(); 
    }

    if (products == null || products.Count == 0)
    {
        ModelState.AddModelError("", "No products found. Please add products first.");
        return View(); 
    }

    ViewData["Customers"] = customers;
    ViewData["Products"] = products;

    return View();
}

// Action to handle the submission of the registration form
[HttpPost]
public async Task<IActionResult> Register(OrderStatus orderstatus)
{
    if (ModelState.IsValid)
    {
        orderstatus.OrderStatus_Date = DateTime.SpecifyKind(orderstatus.OrderStatus_Date, DateTimeKind.Utc);
        orderstatus.PartitionKey = "OrderStatusesPartition";
        orderstatus.RowKey = Guid.NewGuid().ToString();
        await _tableStorageService.AddOrderStatusAsync(orderstatus);

        string message = $"New order by Customer {orderstatus.Customer_ID} for Product {orderstatus.Product_ID} at {orderstatus.OrderStatus_Location} on {orderstatus.OrderStatus_Date}";
        await _queueService.SendMessageAsync(message);

        return RedirectToAction("Index");
    }
    else
    {
        foreach (var error in ModelState)
        {
            Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
        }
    }

    var customers = await _tableStorageService.GetAllCustomersAsync();
    var products = await _tableStorageService.GetAllProductsAsync();
    ViewData["Customers"] = customers;
    ViewData["Products"] = products;

    return View(orderstatus);
}

[HttpPost]
    public async Task<IActionResult> Checkout(string queueName, string message)
    {
        var queryString = $"queueName={queueName}&message={message}";
        var response = await _httpClient.PostAsync($"https://<your-function-app-url>/api/ProcessQueueMessage?{queryString}", null);

        if (response.IsSuccessStatusCode)
        {
            return Ok("Message added to queue successfully.");
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error adding message to queue.");
        }
    }
    // Action to handle checkout process
    public async Task<IActionResult> Checkout()
    {
        await _orderService.CheckoutAsync();
        TempData["Message"] = "Your order has been placed successfully!";
        return RedirectToAction("Index", "Home");
    }

    // Action to handle the deletion of an order status
    [HttpPost]
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            TempData["Error"] = "Invalid order status identifier.";
            return RedirectToAction("Index");
        }

        try
        {
            await _tableStorageService.DeleteOrderStatusAsync(partitionKey, rowKey);
            TempData["Message"] = "Order status deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting order status: {ex.Message}";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }
}
