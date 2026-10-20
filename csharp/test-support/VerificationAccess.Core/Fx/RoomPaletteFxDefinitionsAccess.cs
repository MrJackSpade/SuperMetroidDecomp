
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomPaletteFxDefinitions"/> members production does not use.</summary>
internal static class RoomPaletteFxDefinitionsAccess
{
    /// <summary>Calculates each area's palette-FX list pointer from the bank-$83 table's $20-byte stride.</summary>
    extension(RoomPaletteFxDefinitions)
    {
        /// <summary>Calculates a native palette-FX area-list identity for index0..7.</summary>
        /// <remarks>$83:AC46 has eight pointers to AC66+20h*area. Each area owns an
        /// eight-word palette list followed by an eight-word animated-tile list.
        /// Includes the debug area; invalid indices retain the former span rejection.
        /// Independently verified against NTSC J/U v1.0 and pinned bank_83.asm
        /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
        internal static ushort NativeAreaListPointer(int areaIndex)
        {
            if ((uint)areaIndex >= RoomPaletteFxDefinitions.AreaCount) throw new ArgumentOutOfRangeException(nameof(areaIndex));
            return (ushort)(0xac66 + areaIndex * 0x20);
        }
    }
}
