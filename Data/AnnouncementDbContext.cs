using AnnouncementApi.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AnnouncementApi.Data
{
    public class AnnouncementDbContext : DbContext
    {
        public AnnouncementDbContext(DbContextOptions<AnnouncementDbContext> options) : base(options) { }

        public DbSet<AnnouncementModel> Announcements { get; set; }
    }
}
