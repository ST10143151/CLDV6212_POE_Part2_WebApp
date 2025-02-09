using ABC_Retailers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

//[Authorize]
public class FilesController : Controller
{
    private readonly AzureFileShareService _fileShareService;
    private readonly HttpClient _httpClient;


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
    public async Task<IActionResult> Upload(string shareName, string fileName, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is missing.");
        }

        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent(shareName), "shareName");
            content.Add(new StringContent(fileName), "fileName");
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var response = await _httpClient.PostAsync("https://<your-function-app-url>/api/UploadFile", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok("File uploaded successfully.");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error uploading file.");
            }
        }
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
            using (var stream = file.OpenReadStream())
            {
                string directoryName = "uploads";  
                string fileName = file.FileName;   

                await _fileShareService.UploadFileAsync(directoryName, fileName, stream);
            }

            TempData["Message"] = $"File '{file.FileName}' uploaded successfully!";
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"File upload failed: {ex.Message}";
        }

        return RedirectToAction("Index"); 
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