using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quiz_system.Models;

namespace quiz_system.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly QuizContext _context;

        public AdminController(QuizContext context)
        {
            _context = context;
        }

        // View for managing sections
        public IActionResult ManageSections()
        {
            var sections = _context.Sections.ToList();
            return View(sections);
        }

        // Add a new section
        public IActionResult CreateSection()
        {
            Section section = new Section();
            return View(section);
        }

        [HttpPost]
        public IActionResult CreateSection(Section section)
        {
            if (ModelState.IsValid)
            {
                _context.Sections.Add(section);
                _context.SaveChanges();
                return RedirectToAction(nameof(ManageSections));
            }

            return View(section);
        }

        // Edit an existing section
        public IActionResult EditSection(int id)
        {
            var section = _context.Sections.Find(id);
            if (section == null)
            {
                return NotFound();
            }
            return View(section);
        }

        [HttpPost]
        public IActionResult EditSection(int id, Section section)
        {
            if (id != section.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(section);
                _context.SaveChanges();
                return RedirectToAction(nameof(ManageSections));
            }

            return View(section);
        }

        // Delete a section
        public IActionResult DeleteSection(int id)
        {
            var section = _context.Sections.Find(id);
            if (section == null)
            {
                return NotFound();
            }

            _context.Sections.Remove(section);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageSections));
        }

        // View for managing questions under a section
        public IActionResult ManageQuestions(int sectionId)
        {
            var section = _context.Sections.Include(s => s.Questions).FirstOrDefault(s => s.Id == sectionId);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // Add a new question under a section
        public IActionResult CreateQuestion(int sectionId)
        {
            var section = _context.Sections.Find(sectionId);
            if (section == null)
            {
                return NotFound();
            }

            var question = new Question { SectionId = sectionId };
            return View(question);
        }

        [HttpPost]
        public IActionResult CreateQuestion(Question question)
        {
            if (TryValidateModel(question, nameof(question.QuestionText)) &&
            TryValidateModel(question, nameof(question.OptionA)) &&
            TryValidateModel(question, nameof(question.OptionB)) &&
            TryValidateModel(question, nameof(question.OptionC)) &&
            TryValidateModel(question, nameof(question.OptionD)) &&
            TryValidateModel(question, nameof(question.CorrectAnswer)))
            {
                _context.Questions.Add(question);
                _context.SaveChanges();
                return RedirectToAction(nameof(ManageQuestions), new { sectionId = question.SectionId });
            }

            return View(question);
        }

        // Edit an existing question
        [HttpGet]
        public IActionResult EditQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(int id, Question question)
        {
            if (id != question.Id)
            {
                return BadRequest();
            }
            // Check if the SectionId exists in the Sections table
            var sectionExists = await _context.Sections
                .AnyAsync(s => s.Id == question.SectionId);
            if (!sectionExists)
            {
                // If the SectionId is not valid, return an error message
                ModelState.AddModelError("SectionId", "The selected section does not exist.");
                return View(question);
            }
            if (ModelState.IsValid)
            {
                var questionObj = await _context.Questions.FirstOrDefaultAsync(s => s.Id == question.Id);
                if (questionObj != null)
                {
                    questionObj.QuestionText = question.QuestionText;
                    questionObj.OptionA= question.OptionA;
                    questionObj.OptionB= question.OptionB;
                    questionObj.OptionC= question.OptionC;
                    questionObj.OptionD= question.OptionD;
                    questionObj.CorrectAnswer= question.CorrectAnswer;
                    _context.Questions.Update(questionObj);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageQuestions), new { sectionId = question.SectionId });

                }
                else
                {
                    ModelState.AddModelError("", "The selected question does not exist and not updated.");
                    return View(question);
                }
            }

            return View(question);
        }


        // Delete a question
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageQuestions), new { sectionId = question.SectionId });
        }
    }
}
