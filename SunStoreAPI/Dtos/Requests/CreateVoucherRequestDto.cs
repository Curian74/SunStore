using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos.Requests
{
    public class CreateVoucherRequestDto
    {
        public string? Code { get; set; }

        public int Vpercent { get; set; }
        public int? Quantity { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<int>? UserIds { get; set; }
    }
}
