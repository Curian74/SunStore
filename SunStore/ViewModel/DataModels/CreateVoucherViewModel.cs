using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel.DataModels
{
    public class CreateVoucherViewModel
    {
        public List<SelectListItem>? Users { get; set; }
        public int UserId { get; set; }

        [Required]
        public string? Code { get; set; }

        [Required]
        public int Vpercent { get; set; }

        public int? Quantity { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        public List<int>? UserIds { get; set; }
    }
}
