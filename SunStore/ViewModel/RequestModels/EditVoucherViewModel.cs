using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel.RequestModels
{
    public class EditVoucherViewModel
    {
        public List<SelectListItem>? Users { get; set; }
        public int UserId { get; set; }

        public int VoucherId { get; set; }

        [Required]
        public string? Code { get; set; }

        [Required]
        public int Vpercent { get; set; }

        [Range(0, int.MaxValue)]
        [Required]
        public int? Quantity { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        public List<int>? UserIds { get; set; }
    }
}
