namespace quiz_system.Models
{
    public class UserQuizResult
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Username of the user
        public string QuestionText { get; set; }
        public int SectionId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
