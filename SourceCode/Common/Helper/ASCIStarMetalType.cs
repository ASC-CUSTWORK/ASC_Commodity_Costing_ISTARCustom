using ASCISTARCustom.Common.Descriptor;
using PX.Data;

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
            return GetMetalType(metalType) == true;
        }

        /// <summary>
        /// Determines if the given metal type is silver or not, based on the defined list of silver metal types.
        /// </summary>
        /// <param name="metalType">The metal type to check.</param>
        /// <returns>True if the metal type is silver, false if it is not silver, and null if the metal type is not defined in the list.</returns>
        public static bool IsSilver(string metalType)
        {
            return GetMetalType(metalType) == false;
        }

        /// <summary>
        /// Determines if a given metal type is valid based on a list of acceptable metal types.
        /// </summary>
        /// <param name="metalType">The metal type to check.</param>
        /// <returns>True if the metal type is gold, false if metal type is silver, and null if it cannot be determined.</returns>
        public static bool? GetMetalType(string metalType)
        {
            if (metalType == null)
            {
                return null;
            }

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

                case ASCIStarConstants.MetalType.Type_FSS: return false;
                case ASCIStarConstants.MetalType.Type_SSS: return false;

                default: return null;
            }
        }

        ///<summary>
        ///Returns the gold value based on the provided metal type.
        ///Throws an exception if the metal type is null.
        ///</summary>
        ///<param name="metalType">The metal type for which the gold value is being retrieved.</param>
        ///<returns>The gold value for the provided metal type.</returns>
        public static decimal GetMultFactorConvertTOZtoGram(string metalType)
        {
            if (metalType == null)
                return decimal.Zero;

            switch (metalType)
            {
                case ASCIStarConstants.MetalType.Type_24K: return 24.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_22K: return 22.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_20K: return 20.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_18K: return 18.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_16K: return 16.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_14K: return 14.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_12K: return 12.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_10K: return 10.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_08K: return 8.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_06K: return 6.000000m / 24.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_SSS: return 1.000000m / 31.10348m;
                case ASCIStarConstants.MetalType.Type_FSS: return 1.081080m / 31.10348m;
                default: return decimal.Zero;
            }
        }

        ///<summary>
        ///Returns the gold value based on the provided metal type.
        ///Throws an exception if the metal type is null.
        ///</summary>
        ///<param name="metalType">The metal type for which the gold value is being retrieved.</param>
        ///<returns>The gold value for the provided metal type.</returns>
        public static decimal GetGoldTypeValue(string metalType)
        {
            if (metalType == null)
                return decimal.Zero;

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
                default: return decimal.Zero;
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
            if (metalType == null)
                return decimal.Zero;

            switch (metalType)
            {
                case ASCIStarConstants.MetalType.Type_FSS: return 1.081080m;
                case ASCIStarConstants.MetalType.Type_SSS: return 1.000000m;
                default: return decimal.Zero;
            }
        }
    }
}
