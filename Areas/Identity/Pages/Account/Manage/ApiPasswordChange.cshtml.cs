// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using LogAtc.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LogAtc.Areas.Identity.Pages.Account.Manage;

public class ApiPasswordChangeModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ApiPasswordChangeModel> _logger;

    public ApiPasswordChangeModel(UserManager<ApplicationUser> userManager,
                                  ApplicationDbContext dbContext,
                                  ILogger<ApiPasswordChangeModel> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    [BindProperty]
    public string ApiPassword { get; set; }
   
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        ApiPassword = user.ApiPassword;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var userFromDb = _dbContext.Users.First(u => u.Id == user.Id);
        userFromDb.ApiPassword = ApiPassword;
        await _dbContext.SaveChangesAsync();

        StatusMessage = "API password changed.";
        return RedirectToPage();
    }
}
