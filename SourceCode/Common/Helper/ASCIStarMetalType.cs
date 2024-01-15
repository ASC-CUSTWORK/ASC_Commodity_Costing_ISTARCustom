using ASCISTARCustom.Common.Descriptor;
using PX.Objects.IN;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace ASCISTARCustom.Common.Helper
{
    public static class ASCIStarMetalType
    {
        /// <summary>
        /// Determines if the given metal type is gold or not, based on the defined list of gold metal types.
        /// </summary>
        /// <param name="metalType">The metal type to check.</param>
        /// <returns>True if the metal type is gold, false if it is not, or null if it is not defined in the list.</returns>
        public static bool IsGold(string metalType)
        {
            switch (metalType?.ToUpper())
            {
                case ASCIStarConstants.MetalType.Type_24K: return true;
                case ASCIStarConstants.MetalType.Type_23K: return true;
                case ASCIStarConstants.MetalType.Type_22K: return true;
                case ASCIStarConstants.MetalType.Type_21K: return true;
                case ASCIStarConstants.MetalType.Type_20K: return true;
                case ASCIStarConstants.MetalType.Type_19K: return true;
                case ASCIStarConstants.MetalType.Type_18K: return true;
                case ASCIStarConstants.MetalType.Type_17K: return true;
                case ASCIStarConstants.MetalType.Type_16K: return true;
                case ASCIStarConstants.MetalType.Type_15K: return true;
                case ASCIStarConstants.MetalType.Type_14K: return true;
                case ASCIStarConstants.MetalType.Type_13K: return true;
                case ASCIStarConstants.MetalType.Type_12K: return true;
                case ASCIStarConstants.MetalType.Type_11K: return true;
                case ASCIStarConstants.MetalType.Type_10K: return true;
                case ASCIStarConstants.MetalType.Type_09K: return true;
                case ASCIStarConstants.MetalType.Type_08K: return true;
                case ASCIStarConstants.MetalType.Type_07K: return true;
                case ASCIStarConstants.MetalType.Type_06K: return true;

                case ASCIStarConstants.MetalType.Type_24F: return true;
                case ASCIStarConstants.MetalType.Type_23F: return true;
                case ASCIStarConstants.MetalType.Type_22F: return true;
                case ASCIStarConstants.MetalType.Type_21F: return true;
                case ASCIStarConstants.MetalType.Type_20F: return true;
                case ASCIStarConstants.MetalType.Type_19F: return true;
                case ASCIStarConstants.MetalType.Type_18F: return true;
                case ASCIStarConstants.MetalType.Type_17F: return true;
                case ASCIStarConstants.MetalType.Type_16F: return true;
                case ASCIStarConstants.MetalType.Type_15F: return true;
                case ASCIStarConstants.MetalType.Type_14F: return true;
                case ASCIStarConstants.MetalType.Type_13F: return true;
                case ASCIStarConstants.MetalType.Type_12F: return true;
                case ASCIStarConstants.MetalType.Type_11F: return true;
                case ASCIStarConstants.MetalType.Type_10F: return true;
                case ASCIStarConstants.MetalType.Type_09F: return true;
                case ASCIStarConstants.MetalType.Type_08F: return true;
                case ASCIStarConstants.MetalType.Type_07F: return true;
                case ASCIStarConstants.MetalType.Type_06F: return true;
                default: return false;
            }
        }

        /// <summary>
        /// Determines if the given metal type is silver or not, based on the defined list of silver metal types.
        /// </summary>
        /// <param name="metalType">The metal type to check.</param>
        /// <returns>True if the metal type is silver, false if it is not silver, and null if the metal type is not defined in the list.</returns>
        public static bool IsSilver(string metalType)
        {
            switch (metalType?.ToUpper())
            {
                case ASCIStarConstants.MetalType.Type_FSS: return true;
                case ASCIStarConstants.MetalType.Type_SSS: return true;
                default: return false;
            }
        }

        /// <summary>
        /// Determines if the given metal type is a mix, based on a predefined list of gold and silver types.
        /// This method identifies pure gold, a mix of gold and silver, other types of mixes, as well as cases where the metal type is undefined or not a mix.
        /// </summary>
        /// <param name="metalType">The metal type to check.</param>
        /// <returns>
        /// Returns a string that represents the type of mix:
        /// - 'Type_MixedGold' for pure gold (both metal types are gold).
        /// - 'Type_MixedDefault' for a standard mix of gold and silver.
        /// - 'Type_MixedUndefined' if the metal type is undefined or not a mix.
        /// </returns>
        public static string GetMixedTypeValue(string metalType)
        {
            if (metalType is null) return ASCIStarConstants.MixedMetalType.Type_MixedUndefined;

            metalType = metalType.ToUpper();
            var metalTypes = metalType.Split('-');

            if (metalTypes.Length == 2)
            {
                var firstMetalType = metalTypes[0];
                var secondMetalType = metalTypes[1];

                var isFirstMetalTypeGold = IsGold(firstMetalType);
                var isSecondMetalTypeGold = IsGold(secondMetalType);

                var isFirstMetalTypePredefined = isFirstMetalTypeGold || IsSilver(firstMetalType);
                var isSecondMetalTypePredefined = isSecondMetalTypeGold || IsSilver(secondMetalType);

                if (isFirstMetalTypeGold && isSecondMetalTypeGold)
                {
                    return ASCIStarConstants.MixedMetalType.Type_MixedGold;
                }

                if (isFirstMetalTypePredefined && isSecondMetalTypePredefined)
                {
                    return ASCIStarConstants.MixedMetalType.Type_MixedDefault;
                }
                
            }

            return ASCIStarConstants.MixedMetalType.Type_MixedUndefined;
        }

        ///<summary>
        ///Returns the gold value based on the provided metal type.
        ///Throws an exception if the metal type is null.
        ///</summary>
        ///<param name="metalType">The metal type for which the gold value is being retrieved.</param>
        ///<returns>The gold value for the provided metal type.</returns>
        public static decimal GetMultFactorConvertTOZtoGram(string metalType)
        {
            if (metalType == null) return decimal.Zero;

            if (IsGold(metalType)) return GetGoldTypeValue(metalType) / 24.0m / ASCIStarConstants.TOZ2GRAM_31_10348.value;

            if (IsSilver(metalType)) return GetSilverTypeValue(metalType) / ASCIStarConstants.TOZ2GRAM_31_10348.value;

            return decimal.Zero;
        }

        ///<summary>
        ///Returns the gold value based on the provided metal type.
        ///Throws an exception if the metal type is null.
        ///</summary>
        ///<param name="metalType">The metal type for which the gold value is being retrieved.</param>
        ///<returns>The gold value for the provided metal type.</returns>
        public static decimal GetGoldTypeValue(string metalType)
        {
            if (metalType == null) return 24.0m;

            switch (metalType)
            {
                case ASCIStarConstants.MetalType.Type_24K: return 24.000000m;
                case ASCIStarConstants.MetalType.Type_22K: return 22.000000m;
                case ASCIStarConstants.MetalType.Type_20K: return 20.000000m;
                case ASCIStarConstants.MetalType.Type_18K: return 18.000000m;
                case ASCIStarConstants.MetalType.Type_16K: return 16.000000m;
                case ASCIStarConstants.MetalType.Type_14K: return 14.000000m;
                case ASCIStarConstants.MetalType.Type_12K: return 12.000000m;
                case ASCIStarConstants.MetalType.Type_10K: return 10.000000m;
                case ASCIStarConstants.MetalType.Type_08K: return 8.000000m;
                case ASCIStarConstants.MetalType.Type_06K: return 6.000000m;
                default: return 24.0m;
            }
        }

        ///<summary>
        /// Returns the corresponding silver type value based on the given metalType parameter.
        /// Throws a PXException if the metalType is null.
        ///</summary>
        ///<param name="metalType">String value representing the metal type. </param>
        ///<returns>Decimal value representing the silver type value. </returns>
        public static decimal GetSilverTypeValue(string metalType)
        {
            if (metalType == null) return 1.0m;

            switch (metalType)
            {
                case ASCIStarConstants.MetalType.Type_FSS: return 1.081080m;
                case ASCIStarConstants.MetalType.Type_SSS: return 1.000000m;
                default: return 1.0m;
            }
        }

        /// <summary>
        /// Returns InventoryID that corespond to base inventory item of metal, base InventoryItem - InventoryCD in DB: Gold - 24k, Silver - SSS    
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="metalType">String value representing the metal type. </param>
        /// <returns>int? value of InventoryID from InventoryItem table.</returns>
        public static int? GetCommodityInventoryByMetalType(PXGraph graph, string metalType)
        {
            string inventoryCD = string.Empty;
            bool isGold = ASCIStarMetalType.IsGold(metalType);
            bool isSilver = ASCIStarMetalType.IsSilver(metalType);

            if (isGold) inventoryCD = "24K";
            if (isSilver) inventoryCD = "SSS";

            return GetInventoryItemByInvenctoryCD(graph, inventoryCD)?.InventoryID;
        }

        public static InventoryItem GetInventoryItemByInvenctoryCD(PXGraph graph, string inventoryCD) =>
          SelectFrom<InventoryItem>.Where<InventoryItem.inventoryCD.IsEqual<P.AsString>>.View.Select(graph, inventoryCD)?.TopFirst;
    }
}
