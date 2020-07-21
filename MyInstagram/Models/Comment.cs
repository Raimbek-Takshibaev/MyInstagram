using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string CommentText { get; set; }
        public string PublicationId { get; set; }
        public Publication Publication { get; set; }
        public string AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreationDate { get; set; }
        public Comment()
        {
            Id = Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
        }
    }
}
