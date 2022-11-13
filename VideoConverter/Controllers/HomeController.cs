using Microsoft.AspNetCore.Hosting;
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
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace VideoConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment _appEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnvironment)
        {
            _logger = logger;
            _appEnvironment = appEnvironment;

            string ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg");

            if (Directory.Exists(ffmpegPath) == false)
            {
                Directory.CreateDirectory(ffmpegPath);
            }
            if ((Directory.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe"))
                && Directory.Exists(Path.Combine(ffmpegPath, "ffprobe.exe"))) == false)
            {
                FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath)
                    .GetAwaiter().GetResult();
            }
            FFmpeg.SetExecutablesPath(ffmpegPath);   
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        async public Task<IActionResult> UploadPage(IFormFile inputFile)
        {
            bool isUploaded = false;
            if (inputFile != null)
            {
                isUploaded = await Upload(inputFile);
            }
            
            if (!isUploaded)
            {
                ViewBag.Message = "File is not uploaded";
                ViewBag.Success = false;
                return View();
            }

            ViewBag.Message = "File is successfully uploaded";
            ViewBag.Success = true;

            var item = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Uploads"))
                                .Where(f => System.IO.Path.GetFileName(f) == inputFile.FileName)
                                .FirstOrDefault();
            var model = new FileModel { Name = Path.GetFileName(item), IsConverted = false };

            return View(model);
        }

        public async Task<bool> Upload(IFormFile inputFile)
        {
            var fileName = Path.GetFileName(inputFile.FileName);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

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
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(FileModel model)
        {          
            string inputFile = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", model.Name);
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "Converted", 
                Path.ChangeExtension(model.Name, "avi"));

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

            ViewBag.Success = true;

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Download(string filename)
        {

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Converted", 
                Path.ChangeExtension(filename, "avi"));
            
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
