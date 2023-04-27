using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCISTARCustom.PDS.Interfaces
{
    public interface IASCIStarCostRollup
    {
        string UsrCostRollupType { get; set; }
        decimal? UsrExtCost { get; set; }
    }
}
