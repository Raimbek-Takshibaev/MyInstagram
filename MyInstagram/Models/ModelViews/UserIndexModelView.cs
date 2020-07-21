using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class UserIndexModelView
    {
        public User User { get; set; }
        public List<User> Users { get; set; }
    }
}
