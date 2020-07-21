using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class AccountDetailsModelView
    {
        public User User { get; set; }
        public User ShowingUser { get; set; }
        public List<User> Subscribes { get; set; }
        public List<User> Subscribers { get; set; }
        public List<Publication> Publications { get; set; }
    }
}
