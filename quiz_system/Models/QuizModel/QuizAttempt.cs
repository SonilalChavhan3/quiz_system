namespace quiz_system.Models.QuizModel
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Authenticated User ID
        public int SectionId { get; set; }
        public DateTime AttemptedAt { get; set; } // Timestamp for the attempt

        // Navigation property
        public Section Section { get; set; }
        public ICollection<UserQuizResult> UserQuizResults { get; set; }
    }
}
