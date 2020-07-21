using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyInstagram.Context;
using MyInstagram.Models;

namespace MyInstagram.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private MyInstagramContext _db;

        public CommentsController(MyInstagramContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string commentText, string pubId)
        {
            if (pubId is null)
                return NotFound();
            if (String.IsNullOrEmpty(commentText))
                return RedirectToAction("Details", "Publications", new { pubId = pubId});
            Comment comment = new Comment();
            comment.CommentText = commentText;
            comment.PublicationId = pubId;
            comment.AuthorId = _db.Users.FirstOrDefault(p => p.UserName == User.Identity.Name).Id;
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            Publication pub = _db.Publications.FirstOrDefault(p => p.Id == pubId);
            pub.CommentIds.Add(comment.Id);
            _db.Entry(pub).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", "Publications", new { pubId = pubId });
        }
    }
}