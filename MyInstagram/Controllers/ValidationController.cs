using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyInstagram.Context;

namespace MyInstagram.Controllers
{
    public class ValidationController : Controller
    {
        private MyInstagramContext _db;
        public ValidationController(MyInstagramContext db)
        {
            _db = db;
        }
        public bool CheckUserEmail(string email)
        {
            return !_db.Users.Any(p => p.Email == email);
        }
        public bool CheckUserLogin(string nickname)
        {
            return !_db.Users.Any(p => p.UserName == nickname);
        }
    }
}