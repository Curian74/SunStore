using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel.DataModels
{
    public class CreateVoucherViewModel
    {
        public List<SelectListItem>? Users { get; set; }
        public int UserId { get; set; }

        public string? Code { get; set; }

        public int Vpercent { get; set; }
        [Range(0, int.MaxValue)]
        public int? Quantity { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<int>? UserIds { get; set; }
    }
}
