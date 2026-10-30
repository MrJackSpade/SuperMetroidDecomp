
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="CeresSourceDefinitions.Door"/> that only verification reads.</summary>
internal static class CeresSourceDefinitionsDoorConstants
{
        /// <summary>$A6:F918, the light platform strip; the two $A6:F900 transfer records name it and the dark strip after it.</summary>
        internal const int PlatformFrames = 0xa6f918;
        /// <summary>The bytes between the light and dark platform strips: one byte per floor cell.</summary>
        internal const int PlatformFrameBytes = 4;
}
