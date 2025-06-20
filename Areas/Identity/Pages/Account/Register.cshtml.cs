//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using ServiceReference1; // Your SOAP service reference
//using SLNavyJobBank.Models;
//using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;

//namespace SLNavyJobBank.Areas.Identity.Pages.Account
//{
//    public class WebServiceSettings
//    {
//        public string BaseUrl { get; set; }
//        public string ApiKey { get; set; }
//    }

//    public class RegisterModel : PageModel
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly ILogger<RegisterModel> _logger;
//        private readonly WebServiceSettings _webServiceSettings;

//        // Injecting WebServiceSettings into the constructor
//        public RegisterModel(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            ILogger<RegisterModel> logger,
//            IOptions<WebServiceSettings> webServiceSettings)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _logger = logger;
//            _webServiceSettings = webServiceSettings.Value;
//        }

//        [BindProperty]
//        public InputModel Input { get; set; } = new();

//        public string? ReturnUrl { get; set; }

//        public class InputModel
//        {
//            [Required]
//            [Display(Name = "National Identity Card Number")]
//            [RegularExpression(@"^[0-9]{9}[vVxX]$|^[0-9]{12}$", ErrorMessage = "Please enter a valid NIC number.")]
//            public string NIC { get; set; } = string.Empty;

//            [Display(Name = "Full Name")]
//            [Required(ErrorMessage = "Full Name is required.")]
//            public string FullName { get; set; } = string.Empty;

//            [Display(Name = "Navy ID")]
//            [Required(ErrorMessage = "Navy ID is required.")]
//            public string NavyId { get; set; } = string.Empty;

//            [Display(Name = "Rank")]
//            [Required(ErrorMessage = "Rank is required.")]
//            public string Rank { get; set; } = string.Empty;

//            [Display(Name = "Email")]
//            [Required(ErrorMessage = "Email is required.")]
//            public string Email { get; set; } = string.Empty;


//            //[Required]
//            //[EmailAddress]
//            //[Display(Name = "")]
//            //public string Email { get; set; } = string.Empty;

//            [Required]
//            [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
//            [DataType(DataType.Password)]
//            [Display(Name = "Password")]
//            public string Password { get; set; } = string.Empty;

//            [DataType(DataType.Password)]
//            [Display(Name = "Confirm Password")]
//            [Compare("Password", ErrorMessage = "Passwords do not match.")]
//            public string ConfirmPassword { get; set; } = string.Empty;
//        }

//        public void OnGet(string? returnUrl = null)
//        {
//            ReturnUrl = returnUrl;
//        }

//        // OnPostAsync handles both the NIC lookup and registration
//        public async Task<IActionResult> OnPostAsync(string action, string? returnUrl = null)
//        {
//            returnUrl ??= Url.Content("~/");

//            if (action == "fetch")
//            {
//                return await HandleNicFetchAsync();
//            }

//            if (action == "register")
//            {
//                return await HandleUserRegistrationAsync(returnUrl);
//            }

//            return Page();
//        }

//        // Fetch NIC details from the SOAP service
//        private async Task<IActionResult> HandleNicFetchAsync()
//        {
//            // Clear all model state except NIC
//            var nicValue = Input.NIC;
//            ModelState.Clear();
//            Input = new InputModel { NIC = nicValue };

//            if (string.IsNullOrWhiteSpace(Input.NIC))
//            {
//                ModelState.AddModelError(nameof(Input.NIC), "NIC is required for lookup.");
//                return Page();
//            }

//            // Call the SOAP web service to get NIC details
//            var nicLookupResult = await LookupNicAsync(Input.NIC);

//            if (nicLookupResult.Success)
//            {
//                Input.FullName = nicLookupResult.FullName;
//                Input.NavyId = nicLookupResult.NavyId;
//                Input.Rank = nicLookupResult.Rank;

//                // Add success message
//                TempData["SuccessMessage"] = "Navy personnel details retrieved successfully.";
//            }
//            else
//            {
//                ModelState.AddModelError(string.Empty, nicLookupResult.ErrorMessage ?? "NIC not found or service unavailable.");
//            }

//            return Page();
//        }

//        // Call the SOAP service and fetch NIC details
//        private async Task<NicLookupResult> LookupNicAsync(string nic)
//        {
//            try
//            {
//                using var client = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
//                var result = await client.getSpecificMemberDetailsAsync(nic);

//                if (result != null && result.Length > 0)
//                {
//                    return new NicLookupResult
//                    {
//                        Success = true,
//                        FullName = result[0]?.FullName?.Trim(),
//                        NavyId = result[0]?.NavyId?.Trim(),
//                        Rank = result[0]?.Rank?.Trim(),
//                        Email = result[0]?.email?.Trim()
//                    };
//                }
//                else
//                {
//                    return new NicLookupResult
//                    {
//                        Success = false,
//                        ErrorMessage = "No personnel found with this NIC."
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during NIC lookup");
//                return new NicLookupResult
//                {
//                    Success = false,
//                    ErrorMessage = "An error occurred during the lookup."
//                };
//            }
//        }

//        private class NicLookupResult
//        {
//            public bool Success { get; set; }
//            public string FullName { get; set; } = string.Empty;
//            public string NavyId { get; set; } = string.Empty;
//            public string Rank { get; set; } = string.Empty;
//            public string? ErrorMessage { get; set; }
//            public string? Email { get; internal set; }
//        }

//        // Handle the user registration after NIC fetch
//        private async Task<IActionResult> HandleUserRegistrationAsync(string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return Page();
//            }

//            // Check if user with this email already exists
//            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
//            if (existingUser != null)
//            {
//                ModelState.AddModelError(nameof(Input.Email), "An account with this email already exists.");
//                return Page();
//            }

//            // Check if user with this NIC already exists
//            var existingNicUser = await _userManager.Users
//                .FirstOrDefaultAsync(u => u.NIC == Input.NIC);
//            if (existingNicUser != null)
//            {
//                ModelState.AddModelError(nameof(Input.NIC), "An account with this NIC already exists.");
//                return Page();
//            }

//            // Create a new user instance
//            var user = new ApplicationUser
//            {
//                UserName = Input.Email,
//                Email = Input.Email,
//                FullName = Input.FullName,
//                NIC = Input.NIC,
//                NavyId = Input.NavyId,
//                Rank = Input.Rank,
//                EmailConfirmed = false // Optionally, set email confirmation flag to true
//            };

//            // Attempt to create the user
//            var result = await _userManager.CreateAsync(user, Input.Password);

//            if (result.Succeeded)
//            {
//                _logger.LogInformation("User created a new account with password.");

//                // Sign in the user
//                await _signInManager.SignInAsync(user, isPersistent: false);

//                // Redirect to the return URL
//                return LocalRedirect(returnUrl);
//            }

//            // Handle registration failures
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }

//            return Page();
//        }
//    }
//}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ServiceReference1; // Your SOAP service reference
using SLNavyJobBank.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SLNavyJobBank.Areas.Identity.Pages.Account
{
    public class WebServiceSettings
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly WebServiceSettings _webServiceSettings;

        // Injecting WebServiceSettings into the constructor
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IOptions<WebServiceSettings> webServiceSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webServiceSettings = webServiceSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "National Identity Card Number")]
            [RegularExpression(@"^[0-9]{9}[vVxX]$|^[0-9]{12}$", ErrorMessage = "Please enter a valid NIC number.")]
            public string NIC { get; set; } = string.Empty;

            [Display(Name = "Full Name")]
            [Required(ErrorMessage = "Full Name is required.")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Navy ID")]
            [Required(ErrorMessage = "Navy ID is required.")]
            public string NavyId { get; set; } = string.Empty;

            [Display(Name = "Rank")]
            [Required(ErrorMessage = "Rank is required.")]
            public string Rank { get; set; } = string.Empty;

            [Display(Name = "Email")]
            [Required(ErrorMessage = "Email is required.")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        // Handles both the NIC lookup and registration
        public async Task<IActionResult> OnPostAsync(string action, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (action == "fetch")
            {
                return await HandleNicFetchAsync();
            }

            if (action == "register")
            {
                return await HandleUserRegistrationAsync(returnUrl);
            }

            return Page();
        }

        // Fetch NIC details from the SOAP service
        private async Task<IActionResult> HandleNicFetchAsync()
        {
            // Clear all model state except NIC
            var nicValue = Input.NIC;
            ModelState.Clear();
            Input = new InputModel { NIC = nicValue };

            if (string.IsNullOrWhiteSpace(Input.NIC))
            {
                ModelState.AddModelError(nameof(Input.NIC), "NIC is required for lookup.");
                return Page();
            }

            // Call the SOAP web service to get NIC details
            var nicLookupResult = await LookupNicAsync(Input.NIC);

            if (nicLookupResult.Success)
            {
                Input.FullName = nicLookupResult.FullName;
                Input.NavyId = nicLookupResult.NavyId;
                Input.Rank = nicLookupResult.Rank;
                Input.Email = nicLookupResult.Email; // Fetch the email as well

                // Add success message
                TempData["SuccessMessage"] = "Navy personnel details retrieved successfully.";
            }
            else
            {
                ModelState.AddModelError(string.Empty, nicLookupResult.ErrorMessage ?? "NIC not found or service unavailable.");
            }

            return Page();
        }

        // Call the SOAP service and fetch NIC details
        private async Task<NicLookupResult> LookupNicAsync(string nic)
        {
            try
            {
                using var client = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
                var result = await client.getSpecificMemberDetailsAsync(nic);

                if (result != null && result.Length > 0)
                {
                    return new NicLookupResult
                    {
                        Success = true,
                        FullName = result[0]?.FullName?.Trim(),
                        NavyId = result[0]?.NavyId?.Trim(),
                        Rank = result[0]?.Rank?.Trim(),
                        Email = result[0]?.email?.Trim()
                    };
                }
                else
                {
                    return new NicLookupResult
                    {
                        Success = false,
                        ErrorMessage = "No personnel found with this NIC."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during NIC lookup");
                return new NicLookupResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during the lookup."
                };
            }
        }

        private class NicLookupResult
        {
            public bool Success { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string NavyId { get; set; } = string.Empty;
            public string Rank { get; set; } = string.Empty;
            public string? ErrorMessage { get; set; }
            public string? Email { get; internal set; }
        }

        //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        //{
        //    returnUrl ??= Url.Content("~/");

        //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        //    if (ModelState.IsValid)
        //    {
        //        // Only find user by email
        //        var user = await _userManager.FindByEmailAsync(Input.Email);

        //        if (user == null)
        //        {
        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //            return Page();
        //        }

        //        var result = await _signInManager.PasswordSignInAsync(
        //            user.UserName, // sign in using username, even though found by email
        //            Input.Password,
        //            Input.RememberMe,
        //            lockoutOnFailure: false);

        //        if (result.Succeeded)
        //        {
        //            _logger.LogInformation("User logged in.");
        //            return LocalRedirect(returnUrl);
        //        }
        //        if (result.RequiresTwoFactor)
        //        {
        //            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        //        }
        //        if (result.IsLockedOut)
        //        {
        //            _logger.LogWarning("User account locked out.");
        //            return RedirectToPage("./Lockout");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //            return Page();
        //        }
        //    }

        //    return Page();
        //}


        // Handle the user registration after NIC fetch
        private async Task<IActionResult> HandleUserRegistrationAsync(string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(Input.Email), "An account with this email already exists.");
                return Page();
            }

            // Check if user with this NIC already exists
            var existingNicUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NIC == Input.NIC);
            if (existingNicUser != null)
            {
                ModelState.AddModelError(nameof(Input.NIC), "An account with this NIC already exists.");
                return Page();
            }

            // Create a new user instance
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                NormalizedUserName = Input.Email.ToUpperInvariant(),
                Email = Input.Email,
                NormalizedEmail = Input.Email.ToUpperInvariant(),
                FullName = Input.FullName,
                NIC = Input.NIC,
                NavyId = Input.NavyId,
                Rank = Input.Rank,
                EmailConfirmed = true
            };

            // Attempt to create the user
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect to the return URL
                return LocalRedirect(returnUrl);
            }

            // Handle registration failures
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}

