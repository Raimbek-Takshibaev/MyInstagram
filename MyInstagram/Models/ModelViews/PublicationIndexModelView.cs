using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models.ModelViews
{
    public class PublicationIndexModelView
    {
        public List<Publication> Publications { get; set; }
        public User User { get; set; }
    }
}
