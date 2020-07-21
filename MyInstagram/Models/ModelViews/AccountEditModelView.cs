using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class AccountEditModelView
    {
        public string NameSurname { get; set; }
        [Required(ErrorMessage = "Заполните поле почты")]
        [Remote(action: "CheckUserEmail", controller: "Validation", ErrorMessage = "Данная почта уже существует")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Заполните поле логина")]
        [Remote(action: "CheckUserLogin", controller: "Validation", ErrorMessage = "Данный логин уже существует")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Опишите ваш аккаунт")]
        [MinLength(5, ErrorMessage = "Минимум символов - 5")]
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public AccountEditModelView(User user)
        {
            UserName = user.UserName;
            Email = user.Email;
            NameSurname = user.NameSurname;
            Description = user.Description;
            PhoneNumber = user.PhoneNumber;
        }
    }
}
