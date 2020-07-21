using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models
{
    public enum Sex
    {
        Empty,
        Male,
        Female
    }
    public class User : IdentityUser
    {
        public string AvatarPath { get; set; }
        public string NameSurname { get; set; }
        public string Description { get; set; }
        public Sex Sex { get; set; }
        public List<string> SubscribesIds { get; set; }
        public List<string> SubscribersIds { get; set; }
        public List<string> PublicationIds { get; set; }
        public List<Publication> Publications { get; set; }
    }
}
