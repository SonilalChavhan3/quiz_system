namespace quiz_system.Models.QuizModel
{
    public class UserQuizResultsViewModel
    {
        public int AttemptId { get; set; }
        public string SectionName { get; set; }
        public DateTime AttemptedAt { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public string Status { get; set; }
    }
}
