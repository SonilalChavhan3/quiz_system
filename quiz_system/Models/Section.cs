using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace quiz_system.Models
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }  // Section name
        public int TimeLimitInMinutes { get; set; }  // Time limit for the section in minutes

        // Navigation property to Questions
        [ValidateNever]
        public ICollection<Question> Questions { get; set; }
    }
}
