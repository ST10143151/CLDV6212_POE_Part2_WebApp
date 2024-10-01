using ABC_Retailers.Models;
using ABC_Retailers.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ProductsController : Controller
{
    private readonly BlobService _blobService;
    private readonly TableStorageService _tableStorageService;
    private readonly CartService _cartService;

    public ProductsController(BlobService blobService, TableStorageService tableStorageService, CartService cartService)
    {
        _blobService = blobService;
        _tableStorageService = tableStorageService;
        _cartService = cartService;
    }

    [HttpGet]
    public IActionResult AddProduct()
    {
        return View();
    }

    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
        {
            return BadRequest("Invalid product identifier.");
        }

        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);

        if (product == null)
        {
            return NotFound("Product not found.");
        }

        return View(product);
    }

    /*[HttpPost]
    public async Task<IActionResult> AddProduct(Product product, IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
            product.ImageUrl = imageUrl; // Assuming Product has an ImageUrl property
        }

        if (ModelState.IsValid)
        {
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddProductAsync(product);
            return RedirectToAction("Index");
        }
        return View(product);
    }*/
    /*[HttpPost]
public async Task<IActionResult> AddProduct(Product product, IFormFile file)
{
    if (file != null && file.Length > 0)
    {
        // Upload the file to Blob storage and get the URL
        using var stream = file.OpenReadStream();
        var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
        product.ImageUrl = imageUrl;
    }

    if (ModelState.IsValid)
    {
        product.PartitionKey = "ProductsPartition";
        product.RowKey = Guid.NewGuid().ToString();
        await _tableStorageService.AddProductAsync(product);
        return RedirectToAction("Index");
    }
    return View(product);
}*/

    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product, IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
            product.ImageUrl = imageUrl;
        }

        if (ModelState.IsValid)
        {
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            await _tableStorageService.AddProductAsync(product);
            return RedirectToAction("Index");
        }
        return View(product);
    }




    [HttpPost]
    public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
    {
        if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
        {
            await _blobService.DeleteBlobAsync(product.ImageUrl);
        }

        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        return View(products);
    }

    public async Task<IActionResult> AddToCart(string partitionKey, string rowKey)
    {
        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);

        if (product != null)
        {
            var cartItem = new CartItem
            {
                ProductId = product.RowKey,
                ProductName = product.Product_Name,
                Price = 100m, // Assume price is available somewhere; hardcoded here for simplicity
                Quantity = 1
            };

            _cartService.AddToCart(cartItem);
            TempData["Message"] = $"{product.Product_Name} has been added to your cart!";
        }

        return RedirectToAction("Index");
    }

    public IActionResult Cart()
    {
        var cart = _cartService.GetCart();
        return View(cart);
    }
}
