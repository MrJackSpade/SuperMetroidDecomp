namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification's native view of the charge-flare compositions: the bank-$93 address of each.</summary>
internal static class ChargeFlareSpriteNative
{
    /// <summary>Native views of the flare compositions.</summary>
    extension(ChargeFlareSpriteDefinitions)
    {
        /// <summary>The bank-$93 address of each flare composition, in family order.</summary>
        internal static IReadOnlyList<ushort> NativePointers => ProjectileSpriteNative.FlareAddresses;
    }
}
