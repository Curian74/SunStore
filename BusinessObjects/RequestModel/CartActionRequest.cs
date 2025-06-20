using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.RequestModel
{
    public class CartActionRequest
    {
        public int? CustomerId { get; set; }
        public int ProductOptionId { get; set; }
        public int Quantity { get; set; }
    }

}
