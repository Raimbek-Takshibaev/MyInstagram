using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class AccountRegisterModelView
    {
        [Required(ErrorMessage = "Заполните поле почты")]
        [Remote(action: "CheckUserEmail", controller: "Validation", ErrorMessage = "Данная почта уже существует")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Заполните поле логина")]
        [Remote(action: "CheckUserLogin", controller: "Validation", ErrorMessage = "Данный логин уже существует")]       
        public string Nickname { get; set; }
        public string NameSurname { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Заполните поле пароля")]
        [MinLength(5, ErrorMessage = "Минимум символов - 5")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Опишите ваш аккаунт")]
        [MinLength(5, ErrorMessage = "Минимум символов - 5")]
        public string Description { get; set; }
        public string  PhoneNumber { get; set; }
        public bool Male { get; set; }
        public bool Female { get; set; } 
        public IFormFile Avatar { get; set; }
    }
}
