using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChatMeService.Data;
using ChatMeService.Models;
using ChatMeService.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChatMeService.Controllers
{
    /// <summary>
    /// Accounts API
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin,User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountController : ControllerBase
    //public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="context"></param>
        public AccountController(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ILogger<AccountController> logger,
                IConfiguration config,
                ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
            db = context;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Token</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorModel.BadRequest(Request, ModelState));
            }

            if (ModelState.IsValid)
            {
                return await GetTokenAsync(model);
            }

            return BadRequest(new ErrorModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Login Failed",
                Method = Request.Method,
                URL = Request.Path
            });
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Token</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (db.Users.Any(u => u.Email == model.Email))
            {
                return BadRequest(new ErrorModel
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Method = Request.Method,
                    URL = Request.Path,
                    Message = "User Exist! Try to login or try another email address.",
                });
            }

            if (model.Email != null && model.Password != null && model.Password == model.ConfirmPassword)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, AddDateTime = DateTime.Now };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (db.Roles.Any())
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    return await GetTokenAsync(new LoginViewModel { Email = model.Email, Password = model.Password });
                }

                AddErrors(result);
            }

            return BadRequest(new ErrorModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Method = Request.Method,
                URL = Request.Path,
                Message = "Registration Failed",
            });
        }

        /// <summary>
        /// Register By Phone
        /// </summary>
        /// <returns>Verification Code</returns>
        /// <param name="model">Phone Number View Model</param>
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/RegisterByPhone")]
        public async Task<IActionResult> RegisterByPhone([FromBody] RegisterByPhoneViewModel model)
        {
            ApplicationUser user;

            if (!db.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                user = new ApplicationUser { UserName = model.PhoneNumber, PhoneNumber = model.PhoneNumber, AddDateTime = DateTime.Now };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with phone number.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    AddErrors(result);

                    return BadRequest(ErrorModel.BadRequest(Request, ModelState));
                }
            }
            else
            {
                user = db.Users.SingleOrDefault(u => u.PhoneNumber == model.PhoneNumber);
            }

            //var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            //await _SMSSender.SendSMSAsync(user.PhoneNumber, $"{code} is your verification code in {_config["Title"]}", false);

            //if (_config["Development"] == "True")
            //{
            //    return Ok(new { Status = "Verification code sent!", Code = code });
            //}

            return Ok(new { Status = "Verification code sent!" });
        }

        /// <summary>
        /// Login By Phone
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Token</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/LoginByPhone")]
        public async Task<IActionResult> LoginByPhone([FromBody] LoginByPhoneViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.PhoneNumber == model.PhoneNumber);

                if (user == null)
                {
                    return NotFound(new ErrorModel
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Method = Request.Method,
                        URL = Request.Path,
                        Message = "User Not Found",
                    });
                }

                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.VerificationCode);

                if (!result.Succeeded)
                {
                    AddErrors(result);

                    return BadRequest(ErrorModel.BadRequest(Request, ModelState));
                }

                var token = await GetToken(user);

                if (token != null)
                {
                    return Ok(token);
                }
            }

            return BadRequest(new ErrorModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Method = Request.Method,
                URL = Request.Path,
                Message = "Login Failed"
            });
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        /// <param name="model"></param>
        [HttpPost]
        [Route("/api/ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorModel.BadRequest(Request, ModelState));
            }

            var user = ApplicationUser.Get(db, User);

            _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

            return Ok("Password Changed Successfully");
        }

        /// <summary>
        /// Forgots Password
        /// </summary>
        /// <returns></returns>
        /// <param name="model">Forgot Password Model</param>
        [HttpPost]
        [AllowAnonymous]
        [Route("/api/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorModel.BadRequest(Request, ModelState));
            }
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new ErrorModel
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Method = Request.Method,
                    URL = Request.Path,
                    Message = "User Not Found!"
                });
            }

            //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            //
            //var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            //
            //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
            //     $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            ////return RedirectToAction(nameof(ForgotPasswordConfirmation));

            return Ok("Password reset sent to your email.");
        }

        /// <summary>
        /// Resets Password
        /// </summary>
        /// <returns></returns>
        /// <param name="model">Reset Password Model</param>
        [HttpPost]
        [Route("/api/ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorModel.BadRequest(Request, ModelState));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new ErrorModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Method = Request.Method,
                    URL = Request.Path,
                    Message = "User Not Found!",
                });
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                return Ok("Password Reset Successfully");
            }

            return BadRequest(ErrorModel.BadRequest(Request));
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<IActionResult> GetTokenAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (result.Succeeded)
                {
                    return Ok(await GetToken(user));
                }

                return BadRequest(new ErrorModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Method = Request.Method,
                    URL = Request.Path,
                    Message = "Wrong email or password!"
                });
            }

            return NotFound(new ErrorModel
            {
                StatusCode = HttpStatusCode.NotFound,
                Method = Request.Method,
                URL = Request.Path,
                Message = "User Not Found"
            });
        }

        private async Task<object> GetToken(ApplicationUser user)
        {
            user.LastLoginDateTime = DateTime.Now;
            db.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var r in roles)
            {
                claims.Add(new Claim("roles", r));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

            return new { Status = "Success", Token = new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo };
        }
        #endregion
    }
}