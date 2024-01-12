using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCISTARCustom.Cost
{
    public class ApiResponseMessage
    {
        public const string Error = "Error";
        public const string Success = "Success";
        public const string Warning = "Warning";
        public string Status { get; set; }
        public string Message { get; set; }
        public decimal Price { get; set; }
    }
}
