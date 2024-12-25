using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quiz_system.Models;

namespace quiz_system.Controllers
{
    public class UserController : Controller
    {
        private readonly QuizContext _context;

        public UserController(QuizContext context)
        {
            _context = context;
        }

        // Step 1: User enters their name
        [HttpGet]
        public IActionResult EnterName()
        {
            return View();
        }

        // Step 2: User submits their name and proceeds to quiz
        [HttpPost]
        public IActionResult StartQuiz(UserInfo userInfo)
        {
            if (string.IsNullOrEmpty(userInfo.Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
                return View("EnterName");
            }

            // Store user's name in TempData or session for the quiz session
            HttpContext.Session.SetString("UserName", userInfo.Name);

            return RedirectToAction("Index", "Quiz");
        }
    }
}
