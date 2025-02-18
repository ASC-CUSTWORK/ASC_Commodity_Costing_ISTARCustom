using ASCISTARCustom.Common.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace ASCISTARCustom.PO.DAC
{
    [Serializable]
    [PXCacheName(_cacheName)]
    public class ASCiStarVendorDuty : AuditSystemFields, IBqlTable
    {
        private const string _cacheName = "ASCiStarVendorDuty";


        //#region RecordID
        //[PXDBIdentity(IsKey = true)]
        //public int? RecordID { get; set; }
        //public abstract class recordID : BqlInt.Field<recordID> { }

        //#endregion

        #region BranchID
        //[PXDBInt()]
        [Branch(IsKey = true)]
        [PXUIField(DisplayName = "Branch")]
        //[PXSelector(typeof(Branch.branchID), typeof(Branch.branchCD), typeof(Branch.contactName), SubstituteKey = typeof(Branch.branchCD))]
        public int? Branch { get; set; }
        public abstract class branch : PX.Data.BQL.BqlInt.Field<branch> { }
        #endregion

        #region UOM
        [INUnit(DisplayName = "UOM")]
        [PXCustomizeSelectorColumns(typeof(INUnit.fromUnit), typeof(INUnit.toUnit), typeof(INUnit.unitMultDiv))]
        //[PXDBString(6, IsKey = true)]
        //[PXUIField(DisplayName = "UOM")]
        //[PXSelector(
        //    typeof(Search<INUnit.fromUnit>), typeof(INUnit.toUnit), typeof(INUnit.unitMultDiv))]
        public string UOM { get; set; }
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        #endregion

        #region Freight
        [PXDBDecimal(4, MinValue = -100, MaxValue = 100)]
        [PXDefault(TypeCode.Decimal, "0.000000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Freight")]
        public decimal? Freight { get; set; }
        public abstract class freight : PX.Data.BQL.BqlDecimal.Field<freight> { }
        #endregion

        #region EffectiveDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Effective Date")]
        public DateTime? EffectiveDate { get; set; }
        public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
        #endregion


        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
