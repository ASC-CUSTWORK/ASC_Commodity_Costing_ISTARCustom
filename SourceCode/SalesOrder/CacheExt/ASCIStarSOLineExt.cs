using PX.Data;
using PX.Objects.CR;
using PX.Objects.SO;

namespace ASCISTARCustom.SalesOrder.CacheExt
{
    public class ASCIStarSOLineExt : PXCacheExtension<SOLine>
    {
        public static bool IsActive() => true;

        #region UsrEndLocation
        [PXDBInt]
        [PXUIField(DisplayName = "End Location")]
        [PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>>>), SubstituteKey = typeof(Location.locationCD))]
        public virtual int? UsrEndLocation { get; set; }
        public abstract class usrEndLocation : PX.Data.BQL.BqlInt.Field<usrEndLocation> { }
        #endregion
    }
}