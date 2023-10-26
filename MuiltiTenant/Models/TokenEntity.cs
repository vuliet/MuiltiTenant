using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MuiltiTenant.Models
{
    [Index(nameof(Hash))]
    public class TokenEntity
    {
        [Key]
        public long Id { get; set; }
        public string Hash { get; set; }
        public string Data { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
