using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LogAtc.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(ILogger<RegisterModel> logger)
    {
        _logger = logger;
    }
}
