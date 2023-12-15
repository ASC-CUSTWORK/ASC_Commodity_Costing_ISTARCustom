using PX.Data;
using PX.Objects.CR;
using PX.Objects.SO;
using System;

namespace ASCJewelryLibrary.SO.CacheExt
{
    [Serializable]
    [PXCacheName("SOLine Extension")]
    public class ASCJSOLineExt : PXCacheExtension<SOLine>
    {
        public static bool IsActive() => true;

        #region UsrASCJEndLocation
        [PXDBInt]
        [PXUIField(DisplayName = "End Location")]
        [PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>>>), SubstituteKey = typeof(Location.locationCD))]
        public virtual int? UsrASCJEndLocation { get; set; }
        public abstract class usrASCJEndLocation : PX.Data.BQL.BqlInt.Field<usrASCJEndLocation> { }
        #endregion
    }
}