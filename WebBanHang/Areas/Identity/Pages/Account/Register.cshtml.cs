// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebBanHang.Models;

namespace WebBanHang.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private const string Role_Customer = "Customer";
        private const string Role_Company = "Company";
        private const string Role_Admin = "Admin";
        private const string Role_Employee = "Employee";

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
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
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Địa chỉ")]
            public string Address { get; set; }

            [Required]
            [Display(Name = "Tuổi")]
            public string Age { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Vai trò")]
            public string Role { get; set; }

            public IEnumerable<SelectListItem> RoleList { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Nhập lại mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            await CreateRolesIfNotExistAsync();

            Input = new InputModel
            {
                RoleList = GetRoleList()
            };

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            await CreateRolesIfNotExistAsync();

            // Gán lại RoleList để nếu đăng ký lỗi thì form vẫn hiện dropdown Role.
            Input.RoleList = GetRoleList();

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FullName = Input.FullName;
                user.Address = Input.Address;
                user.Age = Input.Age;
                user.Email = Input.Email;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Nếu không chọn vai trò thì mặc định là Customer.
                    if (string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Role_Customer);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private async Task CreateRolesIfNotExistAsync()
        {
            if (!await _roleManager.RoleExistsAsync(Role_Customer))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role_Customer));
            }

            if (!await _roleManager.RoleExistsAsync(Role_Company))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role_Company));
            }

            if (!await _roleManager.RoleExistsAsync(Role_Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role_Admin));
            }

            if (!await _roleManager.RoleExistsAsync(Role_Employee))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role_Employee));
            }
        }

        private IEnumerable<SelectListItem> GetRoleList()
        {
            return _roleManager.Roles
                .Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name
                })
                .ToList();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
