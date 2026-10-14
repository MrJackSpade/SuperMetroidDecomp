using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="IntroCinematicRomData.Palette.Regions"/> members production does not use.</summary>
internal static class IntroCinematicRomDataPaletteRegionsAccess
{
    /// <summary>Tests whether the intro palette region collection has any entries.</summary>
    extension(IntroCinematicRomData.Palette.Regions self)
    {
        /// <summary>Whether the palette region collection has zero entries.</summary>
        internal bool IsEmpty => self.Count == 0;
    }
}
