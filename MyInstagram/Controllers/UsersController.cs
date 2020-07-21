using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyInstagram.Context;
using MyInstagram.Models;
using MyInstagram.Models.ModelViews;
using MyInstagram.Services;

namespace MyInstagram.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private UserService _userService;
        private MyInstagramContext _db;
        public UsersController(MyInstagramContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }
        bool IsWordContains(string sentence, string keyWord)
        {
            string[] words = sentence.Split(new char[] { ' ' });
            foreach (string item in words)
            {
                if (item == keyWord)
                    return true;
            }
            return false;
        }
        public async Task<IActionResult> FindUsers(string keyWord)
        {
            List<User> users = new List<User>();
            User[] userSelector = _db.Users.ToArray();
            foreach (User user in userSelector)
            {
                if (IsWordContains(user.UserName, keyWord))
                    users.Add(user);
                else if (IsWordContains(user.Email, keyWord))
                    users.Add(user);
                else if (IsWordContains(user.Description, keyWord))
                    users.Add(user);
                else if (IsWordContains(user.NameSurname, keyWord))
                { 
                    if(!String.IsNullOrEmpty(keyWord) && !String.IsNullOrEmpty(user.NameSurname))
                        users.Add(user);
                }
                    
            }
            UserIndexModelView model = new UserIndexModelView()
            {
                User = await _db.Users.FirstOrDefaultAsync(p => p.UserName == User.Identity.Name),
                Users = users
            };
            return View("Index" , model);
        }
        async Task<List<User>> GetSubscribes(User user)
        {
            List<User> users = new List<User>();
            foreach (string id in user.SubscribesIds)
            {
                users.Add(await _db.Users.FirstOrDefaultAsync(p => p.Id == id));
            }
            return users;
        }
        async Task<List<User>> GetSubscribers(User user)
        {
            List<User> users = new List<User>();
            foreach (string id in user.SubscribersIds)
            {
                users.Add(await _db.Users.FirstOrDefaultAsync(p => p.Id == id));
            }
            return users;
        }
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 100)]
        public async Task<IActionResult> Details(string userName)
        {
            if (userName is null)
                return NotFound();
            IQueryable<User> users = _db.Users;
            User user = await _userService.GetUserByUserName(userName);
            if (user is null)
                return NotFound();
            AccountDetailsModelView model = new AccountDetailsModelView();
            model.Publications = new List<Publication>();
            IQueryable<Publication> pubs = _db.Publications.Include(p => p.Comments);
            foreach (string id in user.PublicationIds)
            {
                model.Publications.Add(await pubs.FirstOrDefaultAsync(p => p.Id == id));
            }            
            model.ShowingUser = user;
            model.User = await users.FirstOrDefaultAsync(p => p.UserName == User.Identity.Name);
            model.Subscribers = await GetSubscribers(user);
            model.Subscribes = await GetSubscribes(user);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Subscribe(string subscribeTo)
        {
            if (subscribeTo is null)
                return NotFound();
            User subscribeToUser = await _userService.GetUser(subscribeTo);
            if (subscribeToUser is null)
                return NotFound();
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            if(user.UserName == subscribeToUser.UserName || user.SubscribesIds.Any(p => p == subscribeToUser.Id))
                return BadRequest();
            subscribeToUser.SubscribersIds.Add(user.Id);
            _db.Entry(subscribeToUser).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            user.SubscribesIds.Add(subscribeToUser.Id);
            await _db.SaveChangesAsync();
            return Json(subscribeToUser.SubscribersIds.Count);
        }
        [HttpPost]
        public async Task<IActionResult> UnSubscribe(string unSubscribeTo)
        {
            if (unSubscribeTo is null)
                return NotFound();
            User unSubscribeToUser = await _userService.GetUser(unSubscribeTo);
            if (unSubscribeToUser is null)
                return NotFound();
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            if (user.UserName == unSubscribeToUser.UserName || !user.SubscribesIds.Any(p => p == unSubscribeToUser.Id))
                return BadRequest();         
            unSubscribeToUser.SubscribersIds.Remove(user.Id);
            _userService.AddUserToCache(unSubscribeToUser);
            _db.Entry(unSubscribeToUser).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            user.SubscribesIds.Remove(unSubscribeToUser.Id);
            _userService.AddUserToCache(user);
            await _db.SaveChangesAsync();
            return Json(unSubscribeToUser.SubscribersIds.Count);
        }
    }
}