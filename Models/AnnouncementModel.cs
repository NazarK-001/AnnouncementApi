using System;
using System.ComponentModel.DataAnnotations;

namespace AnnouncementApi.Models
{
    public class AnnouncementModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
