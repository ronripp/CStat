using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CStat.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<CStatUser> _signInManager;
        private readonly UserManager<CStatUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<CStatUser> userManager,
            SignInManager<CStatUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new CStatUser { UserName = Input.Email, Email = Input.Email };
             
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    return RedirectToPage("./Logout");

                // RJR TEMP REMOVE     _logger.LogInformation("User created a new account with password.");
                // RJR TEMP REMOVE 
                // RJR TEMP REMOVE     var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // RJR TEMP REMOVE     code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // RJR TEMP REMOVE     var callbackUrl = Url.Page(
                // RJR TEMP REMOVE         "/Account/ConfirmEmail",
                // RJR TEMP REMOVE         pageHandler: null,
                // RJR TEMP REMOVE         values: new { area = "Identity", userId = user.Id, code = code },
                // RJR TEMP REMOVE         protocol: Request.Scheme);
                // RJR TEMP REMOVE 
                // RJR TEMP REMOVE     await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                // RJR TEMP REMOVE         $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                // RJR TEMP REMOVE 
                // RJR TEMP REMOVE     if (_userManager.Options.SignIn.RequireConfirmedAccount)
                // RJR TEMP REMOVE     {
                // RJR TEMP REMOVE         return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                // RJR TEMP REMOVE     }
                // RJR TEMP REMOVE     else
                // RJR TEMP REMOVE     {
                // RJR TEMP REMOVE         await _signInManager.SignInAsync(user, isPersistent: false);
                // RJR TEMP REMOVE         return LocalRedirect(returnUrl);
                // RJR TEMP REMOVE     }
                // RJR TEMP REMOVE }
                // RJR TEMP REMOVE foreach (var error in result.Errors)
                // RJR TEMP REMOVE {
                // RJR TEMP REMOVE     ModelState.AddModelError(string.Empty, error.Description);

                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
