using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VideoConverter.Models;
using Xabe.FFmpeg;

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
        public IActionResult Converter(string fileList)
        {
            if (fileList == null)
            {
                return Redirect("Index");
            }
            
            var model = new UserModel();

            List<Dictionary<string, string>>? filesDict = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(fileList);

            model.Files = filesDict.Select(p => new FileModel { Name = p["key"], Extension = p["value"] }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<bool> Upload(IFormFile inputFile)
        {
            if (inputFile == null)
            {
                return false;
            }
            
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", 
                Path.GetFileName(inputFile.FileName));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            try
            {
                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = inputFile.OpenReadStream())
                {
                    await uploadedFile.CopyToAsync(localFile);
                }
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        public async Task<string> Convert(FileModel file)
        {          
            string inputFile = Path.Combine(Directory.GetCurrentDirectory(), 
                "Uploads", file.Name);
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), 
                "Converted", Path.ChangeExtension(file.Name, file.Extension));

            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault()
                ?.SetCodec(AudioCodec.mp3);
            IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault()
                ?.SetCodec(VideoCodec.mjpeg);

            if (System.IO.File.Exists(outputFile))
            {
                System.IO.File.Delete(outputFile);
            }

            await FFmpeg.Conversions.New()
                .AddStream(audioStream, videoStream)
                .SetOutput(outputFile)
                .Start();

            return Path.GetFileName(outputFile);
        }

        [HttpPost]
        public async Task<IActionResult> Download(string filename)
        {
            //if (fileList == null)
            //{
            //    return Redirect("Index");
            //}
            //
            //var model = new UserModel();
            //
            //model.Files = JsonSerializer.Deserialize<List<FileModel>>(fileList);

            if (filename == null)
            {
                return Content("filename is null");
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Converted", 
                Path.GetFileName(filename));
            
            if (filename == null || System.IO.File.Exists(path) == false)
            {
                return Content($"{filename} not found");
            }

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
