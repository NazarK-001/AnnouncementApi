using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnnouncementApi.Controllers;
using AnnouncementApi.Data;
using AnnouncementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AnnouncementApi.AnnouncementApi.Tests
{
    public class AnnouncementControllerTests
    {
        private AnnouncementDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AnnouncementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            var context = new AnnouncementDbContext(options);

            context.Announcements.AddRange(new[]
            {
                new AnnouncementModel { Title = "Free Movie Night", Description = "Comedy movie in the park", DateAdded = DateTime.Now },
                new AnnouncementModel { Title = "Tech Meetup", Description = "Meet other developers", DateAdded = DateTime.Now },
            });

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAnnouncementById_ReturnsCorrectAnnouncement()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AnnouncementsController(context);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<object>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var responseValue = okResult.Value;

            var json = JsonSerializer.Serialize(responseValue);
            var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            Assert.Equal(1, root.GetProperty("Id").GetInt32());
            Assert.Equal("Free Movie Night", root.GetProperty("Title").GetString());
        }

        [Fact]
        public async Task GetAnnouncements_ReturnsAllAnnouncements()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AnnouncementsController(context);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<AnnouncementModel>>>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<AnnouncementModel>>(okResult.Value);
            Assert.Equal(2, list.Count()); 
        }

        [Fact]
        public async Task PostAnnouncement_AddsSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AnnouncementsController(context);
            var newAnnouncement = new AnnouncementModel
            {
                Title = "Yoga Workshop",
                Description = "Morning yoga in the park",
                DateAdded = DateTime.Now
            };

            // Act
            var result = await controller.Create(newAnnouncement);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var announcement = Assert.IsType<AnnouncementModel>(createdAt.Value);
            Assert.Equal("Yoga Workshop", announcement.Title);
            Assert.Equal(3, context.Announcements.Count());
        }

    }
}
