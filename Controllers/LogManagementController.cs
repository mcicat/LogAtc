using LogAtc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogAtc.Controllers;

[Authorize]
public class LogManagementController : Controller
{
    private readonly ILogger<LogManagementController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    //private readonly IConfiguration _configuration;
    private readonly string _rootDirectoryPath;

    public LogManagementController(ILogger<LogManagementController> logger,
                                   IWebHostEnvironment appEnvironment,
                                   UserManager<IdentityUser> userManager
                                   /*, IConfiguration configuration*/)
    {
        _logger = logger;
        _userManager = userManager;
        _rootDirectoryPath = Path.Combine(appEnvironment.WebRootPath, "LogStorage");
        //_configuration = configuration;
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
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password!))
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

        //List<Claim> authClaims =
        //[
        //    new Claim(ClaimTypes.Name, user.UserName!),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //];

        //var token = GetToken(authClaims);

        //return Ok(new
        //{
        //    token = new JwtSecurityTokenHandler().WriteToken(token),
        //    expiration = token.ValidTo
        //});

    }

    //private JwtSecurityToken GetToken(List<Claim> authClaims)
    //{
    //    SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
    //    JwtSecurityToken token = new(
    //        issuer: _configuration["JwtSettings:Issuer"],
    //        audience: _configuration["JwtSettings:Audience"],
    //        expires: DateTime.Now.AddMinutes(10),
    //        claims: authClaims,
    //        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
    //    return token;
    //}
}

public class LogModel
{
    [Required(ErrorMessage = "User Name is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "ZipFile is required")]
    public string? ZipFileBase64 { get; set; }
}
