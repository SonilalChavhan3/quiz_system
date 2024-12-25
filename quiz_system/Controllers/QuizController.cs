using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quiz_system.Models;
using static System.Collections.Specialized.BitVector32;

namespace quiz_system.Controllers
{
    public class QuizController : Controller
    {
        private readonly QuizContext _context;

        public QuizController(QuizContext context)
        {
            _context = context;
        }

        // Action to display the available sections (topics)
        public IActionResult Index()
        {
            var sections = _context.Sections.ToList();
            return View(sections);
        }

        // Action to take a quiz from a section
        public IActionResult TakeQuiz(int sectionId)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("EnterName","User");
            }
            var section = _context.Sections.Include(s => s.Questions).FirstOrDefault(s => s.Id == sectionId);
            if (section == null)
            {
                return NotFound();
            }

            if (section.Questions.Count!= 0)
            {

                // Load the first question of the section
                var question = section.Questions.First();
                QuizViewModel quizViewModel = new QuizViewModel();
                quizViewModel.SectionId = sectionId;
                quizViewModel.Question = question;
                quizViewModel.TimeLimitInMinutes = section.TimeLimitInMinutes;
                quizViewModel.Section = section;

                return View(quizViewModel);

            }
            else
            {
                ModelState.AddModelError("", "The selected section does not have any question please try after some time ");
                return RedirectToAction(nameof(Index));
            }
        }

        // Action to navigate to the next question
        [HttpPost]
        public IActionResult NextQuestion(int sectionId, int questionId, string selectedAnswer)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("EnterName");
            }
            var section = _context.Sections.Include(s => s.Questions).FirstOrDefault(s => s.Id == sectionId);
            var question = section.Questions.FirstOrDefault(q => q.Id == questionId);

            // Save the answer in UserQuizResult
            var result = new UserQuizResult
            {
                UserId = userName,  // Replace with actual user ID logic
                SectionId = sectionId,
                QuestionId = questionId,
                SelectedAnswer = selectedAnswer,
                CorrectAnswer = question.CorrectAnswer,
                IsCorrect = selectedAnswer == question.CorrectAnswer,
                QuestionText = question.QuestionText,
            };
            _context.UserQuizResults.Add(result);
            _context.SaveChanges();

            // Get the next question in the section
            var nextQuestion = section.Questions.FirstOrDefault(q => q.Id > questionId);
            if (nextQuestion == null)
            {
                return RedirectToAction("QuizResult", new { sectionId = sectionId });
            }

            return View("TakeQuiz", new QuizViewModel
            {
                SectionId = sectionId,
                Question = nextQuestion,
                TimeLimitInMinutes = section.TimeLimitInMinutes,
                Section = section
            });
        }

        // Action to show the results after the quiz
        public IActionResult QuizResult(int sectionId)
        {
            var userName = HttpContext.Session.GetString("UserName"); ;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("EnterName");
            }
            //var results = _context.UserQuizResults.Where(r => r.SectionId == sectionId).ToList();
            //return View(results);

            var results = _context.UserQuizResults
           .Where(r => r.UserId == userName && r.SectionId == sectionId)
           .ToList();

            var correctAnswersCount = results.Count(r => r.IsCorrect);
            var totalQuestions = results.Count;

            ViewBag.UserName = userName;
            ViewBag.CorrectAnswers = correctAnswersCount;
            ViewBag.TotalQuestions = totalQuestions;
            // Optionally clear the session if you don't want to keep userName in session anymore
            HttpContext.Session.Remove("UserName"); // This removes only the "UserName" from the session
                                                    // or
                                                    // HttpContext.Session.Clear(); // This clears all session data

            return View(results);
        }
    }
}
