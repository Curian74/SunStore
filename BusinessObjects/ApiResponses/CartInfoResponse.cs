using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ApiResponses
{
    public class CartInfoResponse
    {
        public int? Quantity {  get; set; }
        public double? Price { get; set; }
        public double? UnitPrice { get; set; }
        public double? Total {  get; set; }
    }
}
