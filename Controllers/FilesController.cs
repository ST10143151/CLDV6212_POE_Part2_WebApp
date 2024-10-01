using ABC_Retailers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

//[Authorize]
public class FilesController : Controller
{
    private readonly AzureFileShareService _fileShareService;

    public FilesController(AzureFileShareService fileShareService)
    {
        _fileShareService = fileShareService;
    }

    public async Task<IActionResult> Index()
    {
        List<FileModel> files;
        try
        {
            files = await _fileShareService.ListFilesAsync("uploads");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Failed to load files: {ex.Message}";
            files = new List<FileModel>();
        }

        return View(files);
    }

    [HttpPost]
public async Task<IActionResult> Upload(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        ModelState.AddModelError("file", "Please select a file to upload.");
        return await Index();  
    }

    try
    {
        // Upload the file locally or to your application
        using (var stream = file.OpenReadStream())
        {
            string directoryName = "uploads";  
            string fileName = file.FileName;   

            // Call your file upload service (e.g., Azure File Share service)
            await _fileShareService.UploadFileAsync(directoryName, fileName, stream);

            // Once file is uploaded locally, call Azure Function with HTTP POST to notify or process the file
            var functionResponse = await CallAzureFunctionAsync(fileName);
            if (functionResponse.IsSuccessStatusCode)
            {
                TempData["Message"] = $"File '{file.FileName}' uploaded successfully!";
            }
            else
            {
                TempData["Message"] = $"File upload to Azure Function failed: {functionResponse.ReasonPhrase}";
            }
        }
    }
    catch (Exception ex)
    {
        TempData["Message"] = $"File upload failed: {ex.Message}";
    }

    return RedirectToAction("Index");
}

private readonly IHttpClientFactory _httpClientFactory;

public FilesController(AzureFileShareService fileShareService, IHttpClientFactory httpClientFactory)
{
    _fileShareService = fileShareService;
    _httpClientFactory = httpClientFactory;
}

private async Task<HttpResponseMessage> CallAzureFunctionAsync(string fileName)
{
    using (var client = _httpClientFactory.CreateClient())
    {
        // URL of your Azure Function
        string azureFunctionUrl = "http://localhost:7071/api/UploadFile";

        // Creating the HTTP POST request
        var content = new StringContent(JsonConvert.SerializeObject(new { FileName = fileName }), Encoding.UTF8, "application/json");

        // Send the POST request to the Azure Function
        return await client.PostAsync(azureFunctionUrl, content);
    }
}



    // Handle file download
    [HttpGet]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name cannot be null or empty.");
        }

        try
        {
            var fileStream = await _fileShareService.DownloadFileAsync("uploads", fileName);

            if (fileStream == null)
            {
                return NotFound($"File '{fileName}' not found.");
            }

            return File(fileStream, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error downloading file: {ex.Message}");
        }
    }
}
