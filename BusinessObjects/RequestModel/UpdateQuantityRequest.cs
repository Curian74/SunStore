﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.RequestModel
{
    public class UpdateQuantityRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }

}
