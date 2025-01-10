using Microsoft.AspNetCore.Mvc.Rendering;

namespace quiz_system.Models.QuizModel
{
    public class FindUserResultsViewModel
    {
        public string SelectedUserId { get; set; }
        public string SearchEmail { get; set; }
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
    }
}
