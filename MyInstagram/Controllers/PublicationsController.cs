using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyInstagram.Context;
using MyInstagram.Models;
using MyInstagram.Models.ModelViews;
using MyInstagram.Services;

namespace MyInstagram.Controllers
{
    [Authorize]
    public class PublicationsController : Controller
    {
        private UserService _userService;
        private MyInstagramContext _db;
        private IHostEnvironment _environment;
        private UploadService _uploadService;

        public PublicationsController(MyInstagramContext db, IHostEnvironment environment,
            UploadService uploadService, UserService userService)
        {
            _db = db;
            _environment = environment;
            _uploadService = uploadService;
            _userService = userService;
        }

        public string Load(PublicationCreateViewModel model, User user)
        {
            string userFile = $"{user.UserName}_{user.Email}";
            string imageFileName = Guid.NewGuid().ToString();
            string path = Path.Combine(_environment.ContentRootPath + $"\\wwwroot\\UserFiles\\{userFile}\\{imageFileName}");
            string photoPath = $"UserFiles/{userFile}/{imageFileName}/{model.Image.FileName}";
            Directory.CreateDirectory($"wwwroot/UserFiles/{userFile}/{imageFileName}");
            _uploadService.Upload(path, model.Image.FileName, model.Image);
            return photoPath;
        }
        private List<Comment> GetComments(Publication pub)
        {
            List<Comment> comments = new List<Comment>();
            IQueryable<Comment> commentsContext = _db.Comments.Include(p => p.Author);
            foreach (string Id in pub.CommentIds)
            {
                comments.Add(commentsContext.FirstOrDefault(p => p.Id == Id));
            }
            comments.OrderByDescending(p => p.CreationDate);
            return comments;
        }
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 20)]
        public async Task<IActionResult> Index()
        {
            IQueryable<User> users = _db.Users.AsNoTracking();
            IQueryable<Publication> publications = _db.Publications.AsNoTracking().Include(p => p.Author).Include(p => p.Comments);
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            List<Publication> pubs = new List<Publication>();
            foreach (string id in user.SubscribesIds)
            {
                User gettingUser = await users.FirstOrDefaultAsync(p => p.Id == id);
                foreach (string pubId in gettingUser.PublicationIds)
                {
                    pubs.Add(await publications.FirstOrDefaultAsync(p => p.Id == pubId));
                }
            }
            pubs.OrderByDescending(p => p.CreationDate);
            for (int i = 0; i < pubs.Count; i++)
            {
                pubs[i].Comments = GetComments(pubs[i]);
            }
            PublicationIndexModelView model = new PublicationIndexModelView { User = user, Publications = pubs };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> LikeOrUnlike(string pubId)
        {
            if (pubId is null)
                return NotFound();
            Publication pub = await _db.Publications.FirstOrDefaultAsync(p => p.Id == pubId);
            if (pub is null)
                return NotFound();
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            if (pub.LikesIds.Any(p => p == user.Id))
                pub.LikesIds.Remove(user.Id);
            else
                pub.LikesIds.Add(user.Id);
            _db.Entry(pub).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Json(pub.LikesIds.Count);
        }
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 30)]
        public IActionResult Details(string pubId)
        {
            if (pubId is null)
                return NotFound();
            Publication pub = _db.Publications.Include(p => p.Author).Include(p => p.Comments).FirstOrDefault(p => p.Id == pubId);
            if (pub is null)
                return NotFound();
            pub.Comments = GetComments(pub);
            return View(new PublicationDetailsModelView
            {
                Publication = pub,
                User = _db.Users.FirstOrDefault(p => p.UserName == User.Identity.Name)
            });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string pubId)
        {
            if (pubId is null)
                return NotFound();
            Publication pub = await _db.Publications.FirstOrDefaultAsync(p => p.Id == pubId);
            if (pub is null)
                return NotFound();
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            if (user.Id != pub.AuthorId)
                return BadRequest();
            _db.Entry(pub).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            user.PublicationIds.Remove(pub.Id);
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            _userService.AddUserToCache(user);
            return Json(true);
        }
        [HttpPost]
        public async Task<IActionResult> Update(string pubId, string inscription)
        {
            if (pubId is null || String.IsNullOrEmpty(inscription))
                return NotFound();
            User user = await _userService.GetUserByUserName(User.Identity.Name);
            Publication pub = await _db.Publications.FirstOrDefaultAsync(p => p.Id == pubId);
            if (pub is null)
                return NotFound();
            if (pub.AuthorId != user.Id)
                return BadRequest();
            pub.Inscription = inscription;
            _db.Entry(pub).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Json(true);
        }
        public IActionResult Create()
        {
            PublicationCreateViewModel model = new PublicationCreateViewModel
            {
                User = _db.Users.FirstOrDefault(p => p.UserName == User.Identity.Name)
            };
            return View(model);
        }
        [ActionName("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(PublicationCreateViewModel model)
        {
            if (model.Image is null)
                ModelState.AddModelError("", "В публикации должно быть фото");
            else if (model.Image.ContentType != "image/jpg" && model.Image.ContentType != "image/png" && model.Image.ContentType != "image/jpeg")
                ModelState.AddModelError("", "В публикации должно быть фото");
            else if (ModelState.IsValid)
            {
                Publication publication = new Publication();
                User user = await _userService.GetUserByUserName(User.Identity.Name);
                publication.AuthorId = user.Id;
                publication.Inscription = model.Inscription;
                publication.ImagePath = Load(model, user);
                publication.CreationDate = DateTime.Now;
                publication.Id = Guid.NewGuid().ToString();
                publication.LikesIds = new List<string>();
                publication.CommentIds = new List<string>();
                await _db.Publications.AddAsync(publication);
                await _db.SaveChangesAsync();
                Publication pub = _db.Publications.FirstOrDefault(p => p.ImagePath == publication.ImagePath);
                user.PublicationIds.Add(pub.Id);
                _db.Entry(user).State = EntityState.Detached;
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                _userService.AddUserToCache(user);
                return RedirectToAction("Details", "Account");
            }
            model.User = _db.Users.FirstOrDefault(p => p.UserName == User.Identity.Name);
            return View(model);
        }
    }
}