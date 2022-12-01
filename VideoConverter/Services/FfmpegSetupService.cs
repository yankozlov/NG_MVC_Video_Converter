using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace VideoConverter.Services
{
    public class FfmpegSetupService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
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
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
