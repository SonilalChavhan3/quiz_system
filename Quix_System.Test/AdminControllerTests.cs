using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using quiz_system.Controllers;
using quiz_system.Models;
using quiz_system.Models.QuizModel;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Security.Claims;

namespace quiz_system.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerInMemoryTests : IDisposable
    {
        private QuizContext _context;
        private AdminController _controller;
        private bool _disposed = false;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<QuizContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid().ToString())
                .Options;

            _context = new QuizContext(options);

            // Seed test data
            _context.Sections.AddRange(
                new Section { Id = 1, Name = "Section 1" },
                new Section { Id = 2, Name = "Section 2" }
            );

            _context.Questions.AddRange(
                new Question
                {
                    Id = 1,
                    SectionId = 1,
                    QuestionText = "Q1",
                    OptionA = "A",
                    OptionB = "B",
                    OptionC = "C",
                    OptionD = "D",
                    CorrectAnswer = "A"
                },
                new Question
                {
                    Id = 2,
                    SectionId = 1,
                    QuestionText = "Q2",
                    OptionA = "A",
                    OptionB = "B",
                    OptionC = "C",
                    OptionD = "D",
                    CorrectAnswer = "B"
                }
            );

            _context.SaveChanges();

            _controller = new AdminController(_context);

            // Mock user identity for authorization
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context?.Dispose();
                _controller?.Dispose();
                _disposed = true;
            }
        }

        [Test]
        public void ManageSections_ReturnsViewWithSections()
        {
            // Act
            var result = _controller.ManageSections() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<List<Section>>());
            var model = result.Model as List<Section>;
            Assert.That(model, Has.Count.EqualTo(2));
        }

        [Test]
        public void CreateSection_Get_ReturnsViewWithNewSection()
        {
            // Act
            var result = _controller.CreateSection() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<Section>());
        }

        [Test]
        public void CreateSection_Post_ValidModel_AddsSectionAndRedirects()
        {
            // Arrange
            var initialCount = _context.Sections.Count();
            var newSection = new Section { Name = "New Section" };

            // Act
            var result = _controller.CreateSection(newSection) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("ManageSections"));
            Assert.That(_context.Sections.Count(), Is.EqualTo(initialCount + 1));
        }

        [Test]
        public void CreateSection_Post_InvalidModel_ReturnsViewWithSection()
        {
            // Arrange
            var section = new Section { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.CreateSection(section) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(section));
        }

        [Test]
        public void EditSection_Get_ValidId_ReturnsViewWithSection()
        {
            // Act
            var result = _controller.EditSection(1) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<Section>());
            var model = result.Model as Section;
            Assert.That(model.Id, Is.EqualTo(1));
            Assert.That(model.Name, Is.EqualTo("Section 1"));
        }

        [Test]
        public void EditSection_Get_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = _controller.EditSection(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public void EditSection_Post_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var section = new Section { Id = 2, Name = "Updated Section" };

            // Act
            var result = _controller.EditSection(1, section) as BadRequestResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void DeleteSection_ValidId_DeletesSectionAndRedirects()
        {
            // Arrange
            var initialCount = _context.Sections.Count();

            // Act
            var result = _controller.DeleteSection(1) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("ManageSections"));
            Assert.That(_context.Sections.Count(), Is.EqualTo(initialCount - 1));
            Assert.That(_context.Sections.Find(1), Is.Null);
        }

        [Test]
        public void DeleteSection_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeleteSection(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ManageQuestions_ValidSectionId_ReturnsViewWithSection()
        {
            // Act
            var result = _controller.ManageQuestions(1) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<Section>());
            var model = result.Model as Section;
            Assert.That(model.Id, Is.EqualTo(1));
            Assert.That(model.Questions, Has.Count.EqualTo(2));
        }

        [Test]
        public void ManageQuestions_InvalidSectionId_ReturnsNotFound()
        {
            // Act
            var result = _controller.ManageQuestions(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CreateQuestion_Get_ValidSectionId_ReturnsViewWithQuestion()
        {
            // Act
            var result = _controller.CreateQuestion(1) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<Question>());
            var model = result.Model as Question;
            Assert.That(model.SectionId, Is.EqualTo(1));
        }

        [Test]
        public void CreateQuestion_Get_InvalidSectionId_ReturnsNotFound()
        {
            // Act
            var result = _controller.CreateQuestion(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

       

        [Test]
        public async Task EditQuestion_Post_ValidModel_UpdatesQuestionAndRedirects()
        {
            // Arrange
            var question = new Question
            {
                Id = 1,
                SectionId = 1,
                QuestionText = "Updated Question",
                OptionA = "Updated A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectAnswer = "B"
            };

            // Act
            var result = await _controller.EditQuestion(1, question) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("ManageQuestions"));

            var updatedQuestion = _context.Questions.Find(1);
            Assert.That(updatedQuestion.QuestionText, Is.EqualTo("Updated Question"));
            Assert.That(updatedQuestion.OptionA, Is.EqualTo("Updated A"));
            Assert.That(updatedQuestion.CorrectAnswer, Is.EqualTo("B"));
        }

        [Test]
        public void DeleteQuestion_ValidId_DeletesQuestionAndRedirects()
        {
            // Arrange
            var initialCount = _context.Questions.Count();
            var questionToDelete = _context.Questions.Find(1);

            // Act
            var result = _controller.DeleteQuestion(1) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("ManageQuestions"));
            Assert.That(result.RouteValues["sectionId"], Is.EqualTo(1));
            Assert.That(_context.Questions.Count(), Is.EqualTo(initialCount - 1));
            Assert.That(_context.Questions.Find(1), Is.Null);
        }

        [Test]
        public void DeleteQuestion_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeleteQuestion(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Controller_HasAuthorizeAttributeWithSuperAdminRole()
        {
            // Arrange
            var controllerType = typeof(AdminController);
            var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                                                  .FirstOrDefault() as AuthorizeAttribute;

            // Assert
            Assert.That(authorizeAttribute, Is.Not.Null);
            Assert.That(authorizeAttribute.Roles, Is.EqualTo("SuperAdmin"));
        }
    }
}