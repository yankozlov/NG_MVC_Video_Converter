using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoConverter.Services
{
    public class FileCleanupService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            string convertedPath = Path.Combine(Directory.GetCurrentDirectory(), "Converted");

            if (Directory.Exists(uploadsPath))
            {
                foreach (var file in Directory.GetFiles(uploadsPath))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (Directory.Exists(convertedPath))
            {
                foreach (var file in Directory.GetFiles(convertedPath))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(convertedPath);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();

            return Task.CompletedTask;
        }
    }
}
