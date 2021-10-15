using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExampleProject.Web.Pages
{
    [AllowAnonymous]
    public class React : PageModel
    {
        public void OnGet()
        {
        }
    }
}