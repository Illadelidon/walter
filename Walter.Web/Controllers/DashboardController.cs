﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Walter.Core.DTO_s.User;
using Walter.Core.Services;
using Walter.Core.Validation.User;

namespace Walter.Web.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserService _userService;
        


        public DashboardController(UserService userService)
        {
            _userService = userService;
            
           
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous] // Method GET
        public async Task<IActionResult> SignIn()
        {
            var user = HttpContext.User.Identity.IsAuthenticated;
            if (user)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }


        [AllowAnonymous] // Method POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginUserDto model)
        {
            var validator = new LoginUserValidation();
            var validationResult = await validator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                ServiceResponse result = await _userService.LoginUserAsync(model);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.AuthError = result.Message;
                return View(model);
            }
            ViewBag.AuthError = validationResult.Errors[0];
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutUserAsync();
            return RedirectToAction(nameof(SignIn));
        }

        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return View(result.Payload);
        }
        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (result.Success)
            {
                return View(result.Payload);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(ChangePasswordDto model)
        {
            var validator = new ChangePasswordValidation();
            var validationResult = await validator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                var result = await _userService.ChangePasswordAsync(model);
                return RedirectToAction(nameof(GetAll));
            }
            else
            {
                return View(validationResult.Errors);
            }
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            var validator = new CreateUserValidation();
            var validationResult = await validator.ValidateAsync(model);
            if(validationResult.IsValid)
            {
                var result = await _userService.CreateAsync(model);
                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.AuthError = result.Payload;
                return View(model);
            }
            ViewBag.AuthError = validationResult.Errors[0];
            return View(model);
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var user = await _userService.GetByIdAsync(Id);
            return View(user.Payload);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _userService.ConfirmEmailAsync(userId, token);
            if (result.Success)
            {
                return Redirect(nameof(SignIn));
            }
            return Redirect(nameof(SignIn));
        }

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var result = await _userService.ForgotPasswordAsync(email);
            if (result.Success)
            {
                @ViewBag.AuthError = "Check your email.";
                return View(nameof(SignIn));
            }
            @ViewBag.AuthError = "Error sending reset data.";
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            @ViewBag.Email = email;
            @ViewBag.Token = token;
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ResetPasswordData(ResetPasswordDto model)
        {
            var result = await _userService.ResetPasswordAsync(model);
            if (result.Success)
            {
                @ViewBag.AuthError = result.Message;
                return View(nameof(SignIn));
            }
            @ViewBag.AuthError = result.Errors;
            return View();
        }

        public async Task<IActionResult> DeleteById(string id)
        {
            var result = await _userService.DeleteAsync(id);
            if(result.Success)
            {
                return View(nameof(Index));
            }
            return View(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _userService.GetByIdAsync(id);
            await GetRoles();
            if (result.Success)
            {
                return View(result.Payload);
            }
            return View();
        }

        private async Task GetRoles()
        {
            var result = await _userService.LoadRoles();
            @ViewBag.RoleList = new SelectList(
          (System.Collections.IEnumerable)result, nameof(IdentityRole.Id),
              nameof(IdentityRole.Name)
              );
        }
        [HttpPost]
        public async Task<IActionResult>Index(string FirstName,string LastName)
        {
            var userId= User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) 
            {
                return NotFound();
            }

            var result = await _userService.UpdateUserInfoAsync(userId, FirstName,LastName);

            if(result)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Помилка оновлення імені користувача.");
                return View();
            }
        }
        [HttpPost]
       public async Task <IActionResult>EditUsers(string id , string FirstName, string LastName)
        {
            if (string.IsNullOrWhiteSpace(id)||string.IsNullOrWhiteSpace(FirstName)||string.IsNullOrWhiteSpace(LastName))
            {
                return BadRequest("Некоректні дані");
            }
            var success = await _userService.UpdateUserInfoAsync(id, FirstName, LastName);

            if (success)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Помилка оновлення імені користувача.");
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult>AssignRole(string id,string role)
        {

             var selectedRoleId = Request.Form["RoleId"];

             role = await _userService.GetRoleNameById(selectedRoleId);

            



            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role))
            {
                return BadRequest("Некоректні дані");
            }
            var success = await _userService.AssignRoleAsync(id, role);

            if (success)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return NotFound("Неможливо змінити роль для цього користувача");
            }
        }
    }
}
