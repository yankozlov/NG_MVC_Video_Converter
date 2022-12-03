using Microsoft.AspNetCore.Http;

namespace VideoConverter.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public string Status { get; set; }
    }
}
