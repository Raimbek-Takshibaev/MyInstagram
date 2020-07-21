using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyInstagram.Context;
using MyInstagram.Models;
using MyInstagram.Models.ModelViews;
using MyInstagram.Services;

namespace MyInstagram.Controllers
{
    public class AccountController : Controller
    {
        private EmailService _emailService;
        private UserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private UploadService _fileUpload;
        private IHostEnvironment _environment;
        private MyInstagramContext _db;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            UploadService fileUpload, IHostEnvironment environment, MyInstagramContext db, UserService userService, EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileUpload = fileUpload;
            _environment = environment;
            _db = db;
            _userService = userService;
            _emailService = emailService;
        }
        private string Load(AccountRegisterModelView model)
        {
             string newUserFile = $"{model.Nickname}_{model.Email}";
             string path = Path.Combine(_environment.ContentRootPath + $"\\wwwroot\\UserFiles\\{newUserFile}");
             string photoPath = $"UserFiles/{newUserFile}/{model.Avatar.FileName}";
             if (!System.IO.Directory.Exists($"wwwroot/UserFiles/{newUserFile}"))
             {
                 System.IO.Directory.CreateDirectory($"wwwroot/UserFiles/{newUserFile}");
             }
             _fileUpload.Upload(path, model.Avatar.FileName, model.Avatar);
             return photoPath;
        }
        async Task<List<User>> GetSubscribes(User user)
        {
            List<User> users = new List<User>();
            foreach (string id in user.SubscribesIds)
            {
                users.Add(await _userManager.FindByIdAsync(id));
            }
            return users;
        }
        async Task<List<User>> GetSubscribers(User user)
        {
            List<User> users = new List<User>();
            foreach (string id in user.SubscribersIds)
            {
                users.Add(await _userManager.FindByIdAsync(id));
            }
            return users;
        }
        async Task<List<Publication>> GetPublications(User user)
        {
            List<Publication> publications = new List<Publication>();
            foreach (string id in user.PublicationIds)
            {
                publications.Add(await _db.Publications.FindAsync(id));
            }
            return publications;
        }
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 300)]
        public async Task<IActionResult> Details()
        {
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            AccountDetailsModelView model = new AccountDetailsModelView();
            model.Subscribers = await GetSubscribers(user);
            model.Subscribes = await GetSubscribes(user);
            model.Publications = await GetPublications(user);
            model.User = user;
            return View(model);
        }
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2000)]
        public IActionResult Register()
        {
            return View();
        }
        private string GetUserData(User user)
        {
            string data = $"<p><b>Логин</b> - {user.UserName}</p><p><b>Почта</b> - {user.Email}</p><p><b>Описание</b> - {user.Description}</p>";
            switch (user.Sex)
            {
                case Sex.Male:
                    data += "<p><b>Пол</b> - мужской</p";
                    break;
                case Sex.Female:
                    data += "<p><b>Пол</b> - женскийй</p";
                    break;
            }
            if (!String.IsNullOrEmpty(user.NameSurname))
                data += $"<p><b>Имя и фамилия</b> - {user.NameSurname}</p>";
            if (!String.IsNullOrEmpty(user.NameSurname))
                data += $"<p><b>Имя и фамилия</b> - {user.NameSurname}</p>";
            if (!String.IsNullOrEmpty(user.PhoneNumber))
                data += $"<p><b>Имя и фамилия</b> - {user.PhoneNumber}</p>";
            return data;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRegisterModelView model)
        {
            if (model.Avatar is null)
            {
                ModelState.AddModelError("", "Вы должны иметь аватар");
            }
            else if (ModelState.IsValid)
            {
                User user = new User
                {
                    Email = model.Email,
                    UserName = model.Nickname,
                    NameSurname = model.NameSurname,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    AvatarPath = Load(model),
                    Publications = new List<Publication>(),
                    SubscribesIds = new List<string>(),
                    SubscribersIds = new List<string>(),
                    PublicationIds = new List<string>()
                };
                if (model.Male)
                    user.Sex = Sex.Male;
                else if (model.Female)
                    user.Sex = Sex.Female;
                else
                    user.Sex = Sex.Empty;            
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _userService.AddUserToCache(user);
                    await _signInManager.SignInAsync(user, false);
                    await _emailService.SendEmailAsync(
                        email: user.Email,
                        subject: "Поздравляем с регистрацией!",
                        message: $"<h5>Ваши данные:</h5>{GetUserData(user)}"
                        );
                    return RedirectToAction("Details", new { login = user.UserName });
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 2000)]
        public IActionResult Login()
        {
            return View();
        }
        [Authorize]
        public async Task<bool> GetDetailsEmailAjax()
        {
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            await _emailService.SendEmailAsync(
                        email: user.Email,
                        subject: "Ваши данные",
                        message: $"<h5>Ваши данные:</h5>{GetUserData(user)}"
                        );
            return true;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            AccountEditModelView model = new AccountEditModelView(user); 
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AccountEditModelView model)
        {          
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(User.Identity.Name);
                user.Description = model.Description;
                user.NameSurname = model.NameSurname;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                await _emailService.SendEmailAsync(
                         email: user.Email,
                         subject: "Ваши данные были изменены",
                         message: $"<h5>Ваши данные:</h5>{GetUserData(user)}"
                         );
                return RedirectToAction("Details");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginModelView model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Authetificator);
                if (user is null)
                    user = await _userManager.FindByNameAsync(model.Authetificator);
                if(user is null)
                {
                    ModelState.AddModelError("", "Не корректный пароль и(или) аутентификатор");
                    return View(model);
                }
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    true,
                    false
                    );
                if (result.Succeeded)
                {
                    _userService.AddUserToCache(user);
                    return RedirectToAction("Details");
                }
                ModelState.AddModelError("", "Не корректный пароль и(или) аутентификатор");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}