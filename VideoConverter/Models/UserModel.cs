using System.Collections.Generic;

namespace VideoConverter.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public List<FileModel> Files { get; set; }
    }
}
