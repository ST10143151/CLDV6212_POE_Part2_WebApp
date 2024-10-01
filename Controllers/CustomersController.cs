using ABC_Retailers.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


[Authorize]
public class CustomersController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly HttpClient _httpClient;

    public CustomersController(TableStorageService tableStorageService, HttpClient httpClient)
    {
        _tableStorageService = tableStorageService;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _tableStorageService.GetAllCustomersAsync();
        return View(customers);
    }

    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(string tableName, string partitionKey, string rowKey, string data)
    {
        var queryString = $"tableName={tableName}&partitionKey={partitionKey}&rowKey={rowKey}&data={data}";
        var response = await _httpClient.PostAsync($"https://<your-function-app-url>/api/StoreTableInfo?{queryString}", null);

        if (response.IsSuccessStatusCode)
        {
            return Ok("Data stored in table successfully.");
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error storing data in table.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer customer)
    {
        customer.PartitionKey = "CustomersPartition";
        customer.RowKey = Guid.NewGuid().ToString();

        await _tableStorageService.AddCustomerAsync(customer);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
        if (customer == null)
        {
            return NotFound();
        }
        return View(customer);
    }
}
