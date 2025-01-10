namespace quiz_system.Models.QuizModel
{
    public class SectionQuizResult
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public int SectionId { get; set; }
        public int AttemptId { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
