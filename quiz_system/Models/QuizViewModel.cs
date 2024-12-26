namespace quiz_system.Models
{
    public class QuizViewModel
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        public Question Question { get; set; } = new Question();
        public int TimeLimitInMinutes { get; set; }
        public int CurrentQuestionNumber { get; set; } // Current question number
        public int TotalQuestions { get; set; } // Total number of questions in the section
        public int RemainingTimeInSeconds { get; set; }
    }
}
