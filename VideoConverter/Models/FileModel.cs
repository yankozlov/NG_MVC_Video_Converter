using Microsoft.AspNetCore.Http;

namespace VideoConverter.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsConverted { get; set; }
    }
}
