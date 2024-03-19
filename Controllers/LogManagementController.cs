using LogAtc.Data;
using LogAtc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace LogAtc.Controllers;

[Authorize]
public class LogManagementController : Controller
{
    private readonly ILogger<LogManagementController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _rootDirectoryPath;

    public LogManagementController(ILogger<LogManagementController> logger,
                                   IWebHostEnvironment appEnvironment,
                                   UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        _rootDirectoryPath = Path.Combine(appEnvironment.WebRootPath, "LogStorage");
    }

    public IActionResult Index()
    {
        if (!Directory.Exists(_rootDirectoryPath))
        {
            return View(Enumerable.Empty<(string, string)>());
        }
        var files = Directory.EnumerateFiles(_rootDirectoryPath)
                             .Where(x => x.EndsWith(".zip"))
                             .Select(x=> (Path.GetFileName(x),x.Replace(_rootDirectoryPath, "LogStorage/")))
                             .ToList();
        return View(files);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [Route("[controller]/LogZipFile")]
    [AllowAnonymous]
    public async Task<IActionResult> LogZipFile([FromBody] LogModel model)
    {
        if (model is null)
        {
            return BadRequest();
        }
        var user = await _userManager.FindByNameAsync(model.Username!);
        if (user == null 
            || user.ApiPassword is null
            || user.ApiPassword != model.ApiPassword)
        {
            return Unauthorized();
        }
        if (!Directory.Exists(_rootDirectoryPath))
        {
            Directory.CreateDirectory(_rootDirectoryPath);
        }

        string zipFilename = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + Guid.NewGuid() + ".zip";
        System.IO.File.WriteAllBytes(Path.Combine(_rootDirectoryPath, zipFilename), Convert.FromBase64String(model.ZipFileBase64!));

        return Ok();
    }
}

public class LogModel
{
    [Required(ErrorMessage = "User Name is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? ApiPassword { get; set; }

    [Required(ErrorMessage = "ZipFile is required")]
    public string? ZipFileBase64 { get; set; }
}
