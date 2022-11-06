using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoConverter.Models;

namespace VideoConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadPage(IFormFile inputFile)
        {
            if (inputFile != null)
            {
                //?
                var fileName = Path.GetFileName(inputFile.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = inputFile.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                }
            }

            ViewBag.Message = "File is successfully uploaded";

            var item = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Uploads"))
                                .Where(f => System.IO.Path.GetFileName(f) == inputFile.FileName)
                                .FirstOrDefault();
            var model = new FileModel { Name = System.IO.Path.GetFileName(item), Path = item };

            return View(model);
        }

        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
                return Content("Filename is not available");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        // Get content type
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".avi", "video/x-msvideo"},
                {".mp4", "video/mp4"},
                {".mpeg", "video/mpeg"},
                {".webm", "video/webm"}
            };
        }
        //!!!!!

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
