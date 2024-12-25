namespace quiz_system.Models
{
    public class QuizResultViewModel
    {
        public string UserName { get; set; }
        public int TotalQuestions { get; set; }
        public string CorrectAnswers { get; set; }
        public List<Question> Questions { get; set; }
    }
}
