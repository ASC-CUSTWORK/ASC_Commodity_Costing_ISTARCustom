using PX.Data;
using PX.Objects.CR;
using System;

namespace ASCISTARCustom.CR.CacheExt
{
    [PXCacheName("CRLocation Extension")]
    [Serializable]
    public class ASCIStarCRLocationExt : PXCacheExtension<Location>
    {
        public static bool IsActive() => true;

        #region UsrDCLocation
        [PXDBInt]
        [PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, NotEqual<Current<Location.locationID>>>>>),
            SubstituteKey = typeof(Location.locationCD))]
        [PXUIField(DisplayName = "DC Location")]
        public virtual int? UsrDCLocation { get; set; }
        public abstract class usrDCLocation : PX.Data.BQL.BqlInt.Field<usrDCLocation> { }
        #endregion

    }
}