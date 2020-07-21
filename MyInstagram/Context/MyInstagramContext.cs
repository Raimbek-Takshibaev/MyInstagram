using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyInstagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Context
{
    public class MyInstagramContext : IdentityDbContext<User>
    {
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public MyInstagramContext(DbContextOptions<MyInstagramContext> options) :
            base(options)
        { }
    }
}
