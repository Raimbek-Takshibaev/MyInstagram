using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models
{
    public class Publication
    {
        public string Id { get; set; }
        public string ImagePath { get; set; }
        public string Inscription { get; set; }
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }
        public List<string> LikesIds{ get; set; }
        public List<string> CommentIds { get; set; }
        public List<Comment> Comments { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
