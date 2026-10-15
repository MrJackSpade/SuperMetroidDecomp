namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values of <see cref="EndingLogoPalettePointerDefinitions"/> that only verification reads.</summary>
internal static class EndingLogoPalettePointerDefinitionsConstants
{
    /// <summary>$8B:E5E7, sixteen pairs of reverse-copy bank-$8C palette pointers.</summary>
    public const int NativeTableAddress = 0x8be5e7;

    /// <summary>Provides the native ending logo palette pointer table for imported palette checks.</summary>
    extension(EndingLogoPalettePointerDefinitions)
    {
        /// <inheritdoc cref="EndingLogoPalettePointerDefinitionsConstants.NativeTableAddress"/>
        internal static int NativeTableAddress => EndingLogoPalettePointerDefinitionsConstants.NativeTableAddress;
    }
}
