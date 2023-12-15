using PX.Data;
using PX.Objects.CR;
using System;

namespace ASCJewelryLibrary.CR.CacheExt
{
    [PXCacheName("CRLocation Extension")]
    [Serializable]
    public class ASCJCRLocationExt : PXCacheExtension<Location>
    {
        public static bool IsActive() => true;

        #region UsrASCJDCLocation
        [PXDBInt]
        [PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, NotEqual<Current<Location.locationID>>>>>),
            SubstituteKey = typeof(Location.locationCD))]
        [PXUIField(DisplayName = "DC Location")]
        public virtual int? UsrASCJDCLocation { get; set; }
        public abstract class usrASCJDCLocation : PX.Data.BQL.BqlInt.Field<usrASCJDCLocation> { }
        #endregion

    }
}