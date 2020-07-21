using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyInstagram.Services
{
    public class UploadService
    {
        public async void Upload(string path, string fileName, IFormFile file)
        {
            using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}
