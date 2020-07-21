using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class PublicationCreateViewModel
    {
        [Required(ErrorMessage = "Добавьте подпись")]
        public string Inscription { get; set; }
        public IFormFile Image { get; set; }
        public User User { get; set; }
    }
}
