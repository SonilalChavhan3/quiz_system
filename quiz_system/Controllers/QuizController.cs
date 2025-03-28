﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using quiz_system.Models;
using quiz_system.Models.QuizModel;
using static System.Collections.Specialized.BitVector32;

namespace quiz_system.Controllers
{
    [Authorize]
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
                quizViewModel.RemainingTimeInSeconds = section.TimeLimitInMinutes * 60;// Start full time
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

            // Calculate remaining time
            var elapsedTime = (DateTime.UtcNow - recentAttempt.AttemptedAt).TotalSeconds;
            var remainingTime = section.TimeLimitInMinutes * 60 - elapsedTime;
            if (remainingTime <= 0)
            {
                return RedirectToAction("QuizResult", new { sectionId = sectionId, sectionName = section.Name });
            }
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
                RemainingTimeInSeconds = (int)remainingTime // Pass remaining time
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
        [HttpPost]
        public IActionResult SubmitQuiz(int sectionId, string sectionName)
        {
            // Quiz processing logic
            var redirectUrl = Url.Action("QuizResult", "Quiz", new { sectionId = sectionId, sectionName = sectionName });
            return Json(new { redirectUrl });
        }

        public IActionResult UserQuizResults(string UserId = null)
        {
            string UserIdobj = String.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                UserIdobj = _userManager.GetUserId(User); // Get the authenticated user's ID
            }
            else
            {
                UserIdobj = UserId;
            }

            

            // Fetch all the quiz attempts for the authenticated user
            var quizAttempts = _context.QuizAttempts
                .Where(qa => qa.UserId == UserIdobj)
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

        public IActionResult ViewQuizResult(int sectionId, int attempId, string UserId = null)
        {
            string UserIdobj = String.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                UserIdobj = _userManager.GetUserId(User); // Get the authenticated user's ID
            }
            else
            {
                UserIdobj = UserId;
            }
            var attempt = _context.QuizAttempts
                .Include(a => a.Section.Questions)
                .Include(a => a.UserQuizResults)
                .FirstOrDefault(a => a.UserId == UserIdobj && a.SectionId == sectionId && a.Id == attempId);

            if (attempt == null)
            {
                return NotFound();
            }

            // Pass the attempt and results to the view
            return View(attempt);
        }
        public IActionResult GetQuestionDetails(int questionId)
        {
            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                return NotFound();
            }

            return Json(new
            {
                questionText = question.QuestionText,
                optionA = question.OptionA,
                optionB = question.OptionB,
                optionC = question.OptionC,
                optionD = question.OptionD,
                correctAnswer = question.CorrectAnswer,
            });
        }
        public IActionResult FindUserResults()
        {
            // Fetch all unique user IDs from QuizAttempts
            var usersWithResultsIds = _context.QuizAttempts
                .Select(qa => qa.UserId)
                .Distinct()
                .ToList();

            // Fetch the corresponding user details from AspNetUsers
            var usersWithResults = _context.Users // Assuming _context.Users refers to AspNetUsers
                .Where(u => usersWithResultsIds.Contains(u.Id))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email // Use the Email field from AspNetUsers
                })
                .ToList();

            // Create the view model
            var model = new FindUserResultsViewModel
            {
                Users = usersWithResults
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult FindUserResults(FindUserResultsViewModel model)
        {
            if (!string.IsNullOrEmpty(model.SearchEmail))
            {
                // Find user by email
                var user = _userManager.Users.FirstOrDefault(u => u.Email == model.SearchEmail);
                if (user == null)
                {
                    TempData["Message"] = "No user found with the provided email address.";
                    return View(model);
                }

                // Redirect to the UserQuizResults method
                return RedirectToAction("UserQuizResults1", new { UserId = user.Id });
            }
            else if (!string.IsNullOrEmpty(model.SelectedUserId))
            {
                // Redirect to the UserQuizResults method
                return RedirectToAction("UserQuizResults1", new { UserId = model.SelectedUserId });
            }

            TempData["Message"] = "Please select a user or enter an email address.";
            return View(model);
        }


        public IActionResult UserQuizResults1(string UserId = null)
        {
            string UserIdobj = string.IsNullOrEmpty(UserId) ? _userManager.GetUserId(User) : UserId;

            var quizAttempts = _context.QuizAttempts
                .Where(qa => qa.UserId == UserIdobj)
                .Include(qa => qa.Section)
                .Include(qa => qa.UserQuizResults)
                .OrderByDescending(qa => qa.AttemptedAt)
                .ToList();

            var results = quizAttempts.Select(qa => new
            {
                SectionName = qa.Section.Name,
                TotalQuestions = qa.UserQuizResults.Count,
                SectionId=qa.Section.Id,
                AttemptId = qa.Id,
                UserId=qa.UserId,
                CorrectAnswers = qa.UserQuizResults.Count(r => r.IsCorrect),
                Percentage = qa.UserQuizResults.Count > 0
                    ? (qa.UserQuizResults.Count(r => r.IsCorrect) * 100) / qa.UserQuizResults.Count
                    : 0,
                Performance = qa.UserQuizResults.Count > 0
                    ? (qa.UserQuizResults.Count(r => r.IsCorrect) * 100) / qa.UserQuizResults.Count >= 75 ? "High" :
                      (qa.UserQuizResults.Count(r => r.IsCorrect) * 100) / qa.UserQuizResults.Count >= 50 ? "Average" :
                      "Low"
                    : "Low"
            }).ToList();


            if (!results.Any())
            {
                TempData["Message"] = "No quiz results found.";
                return View();
            }

            return View(results);
        }

    }
}
