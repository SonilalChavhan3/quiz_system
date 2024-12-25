namespace quiz_system.Models
{
    public class QuizViewModel
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        public Question Question { get; set; } = new Question();
        public int TimeLimitInMinutes { get; set; }
    }
}
