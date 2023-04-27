using PX.Data;
using PX.Objects.CR;

namespace ASCISTARCustom
{
    public class ASCIStarCRLocationExt : PXCacheExtension<PX.Objects.CR.Location>
    {
        #region Static Method
        public static bool IsActive()
        {
            return true;
        }
        #endregion


        #region UsrDCLocation
        [PXDBInt]
        [PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, NotEqual<Current<Location.locationID>>>>>), SubstituteKey = typeof(Location.locationCD))]
        [PXUIField(DisplayName = "DC Location")]
        public virtual int? UsrDCLocation { get; set; }
        public abstract class usrDCLocation : PX.Data.BQL.BqlInt.Field<usrDCLocation> { }
        #endregion

    }
}