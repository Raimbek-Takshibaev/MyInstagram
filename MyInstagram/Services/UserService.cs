using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyInstagram.Context;
using MyInstagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Services
{
    public class UserService
    {
        private MyInstagramContext db;
        private IMemoryCache cache;
        public UserService(MyInstagramContext context, IMemoryCache memoryCache)
        {
            db = context;
            cache = memoryCache;
        }
        public void AddUserToCache(User user)
        {
            cache.Set(user.Id, user, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });
        }

        public async Task<User> GetUser(string id)
        {
            User user = null;
            if (!cache.TryGetValue(id, out user))
            {
                user = await db.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    cache.Set(user.Id, user,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));
                }
            }
            return user;
        }
        public async Task<User> GetUserByUserName(string userName)
        {
            User user = null;
            if (!cache.TryGetValue(userName, out user))
            {
                user = await db.Users.FirstOrDefaultAsync(p => p.UserName == userName);
                if (user != null)
                {
                    cache.Set(user.UserName, user,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));
                }
            }
            return user;
        }
    }
}
