using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quiz_system.Models;
using quiz_system.Models.QuizModel;
using static System.Collections.Specialized.BitVector32;

namespace quiz_system.Controllers
{
    public class QuizController : Controller
    {
        private readonly QuizContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public QuizController(QuizContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var userId = _userManager.GetUserId(User);

            var section = _context.Sections.Include(s => s.Questions).FirstOrDefault(s => s.Id == sectionId);
            if (section == null)
            {
                return NotFound();
            }

            if (section.Questions.Count != 0)
            {
                var newAttempt = new QuizAttempt
                {
                    UserId = userId,
                    SectionId = sectionId,
                    AttemptedAt = DateTime.UtcNow
                };
                _context.QuizAttempts.Add(newAttempt);
                _context.SaveChanges();
                // Load the first question of the section
                var question = section.Questions.First();
                QuizViewModel quizViewModel = new QuizViewModel();
                quizViewModel.SectionId = sectionId;
                quizViewModel.Question = question;
                quizViewModel.TimeLimitInMinutes = section.TimeLimitInMinutes;
                quizViewModel.Section = section;
                quizViewModel.CurrentQuestionNumber = 1;
                quizViewModel.TotalQuestions = section.Questions.Count;

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
            var userId = _userManager.GetUserId(User);
            var section = _context.Sections.Include(s => s.Questions).FirstOrDefault(s => s.Id == sectionId);
            var question = section.Questions.FirstOrDefault(q => q.Id == questionId);

            // Check if an active attempt exists for the same section
            var recentAttempt = _context.QuizAttempts
                .Where(a => a.UserId == userId && a.SectionId == sectionId)
                .OrderByDescending(a => a.AttemptedAt)
                .FirstOrDefault();
            // Save the answer in UserQuizResult
            var result = new UserQuizResult
            {
                UserId = userId,  // Replace with actual user ID logic
                QuizAttemptId = recentAttempt.Id,
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
                return RedirectToAction("QuizResult", new { sectionId = sectionId, sectionName = section.Name });
            }
            // Get the current question number (e.g., 1/10)
            int currentQuestionNumber = section.Questions.OrderBy(q => q.Id).ToList().IndexOf(nextQuestion) + 1;

            int totalQuestions = section.Questions.Count;
            return View("TakeQuiz", new QuizViewModel
            {
                SectionId = sectionId,
                Question = nextQuestion,
                TimeLimitInMinutes = section.TimeLimitInMinutes,
                Section = section,
                CurrentQuestionNumber = currentQuestionNumber,
                TotalQuestions = totalQuestions,
            });
        }

        // Action to show the results after the quiz
        public IActionResult QuizResult(int sectionId, string sectionName)
        {
            var userId = _userManager.GetUserId(User);

            try
            {

                var recentAttempt = _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.SectionId == sectionId)
                    .OrderByDescending(a => a.AttemptedAt)
                    .FirstOrDefault();


                var results = _context.UserQuizResults
                .Where(r => r.QuizAttemptId == recentAttempt.Id)
                .ToList();

                var correctAnswersCount = results.Count(r => r.IsCorrect);
                var totalQuestions = results.Count;

                ViewBag.SectionName = sectionName;
                ViewBag.CorrectAnswers = correctAnswersCount;
                ViewBag.TotalQuestions = totalQuestions;

                return View(results);
            }
            catch (Exception)
            {

                throw;
            }



        }

        public IActionResult UserQuizResults()
        {
            var userId = _userManager.GetUserId(User); // Get the authenticated user's ID

            // Fetch all the quiz attempts for the authenticated user
            var quizAttempts = _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Section) // Include section data (optional but useful for displaying section name)
                .Include(qa => qa.UserQuizResults) // Include related quiz results for each attempt
                .OrderByDescending(qa => qa.AttemptedAt) // Order by date descending
                .ToList();

            if (quizAttempts == null || !quizAttempts.Any())
            {
                // Handle the case where no quiz attempts are found
                TempData["Message"] = "No quiz results found.";
                return View();
            }

            return View(quizAttempts);
        }

        public IActionResult VievQuizResult(int sectionId, int attempId)
        {
            var userId = _userManager.GetUserId(User);
            var attempt = _context.QuizAttempts
                .Include(a => a.Section.Questions)
                .Include(a => a.UserQuizResults)
                .FirstOrDefault(a => a.UserId == userId && a.SectionId == sectionId && a.Id == attempId);

            if (attempt == null)
            {
                return NotFound();
            }

            // Pass the attempt and results to the view
            return View(attempt);
        }

    }
}
