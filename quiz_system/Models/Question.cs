using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace quiz_system.Models
{
    public class Question
    {
        [ValidateNever]
        public int Id { get; set; }
        public int SectionId { get; set; }  // Foreign key to Section
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }

        // Navigation property to Section
        [ValidateNever]
        public Section Section { get; set; }
    }
}
