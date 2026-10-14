using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RoomPaletteFxDefinitions"/> that only verification reads.</summary>
internal static class RoomPaletteFxDefinitionsConstants
{
    /// <summary>$8D:E194, first definition in the contiguous cinematic/Samus group.</summary>
    public const ushort CinematicDefinitionsBegin = 0xe194;
    /// <summary>$8D:E200, final definition in the contiguous cinematic/Samus group.</summary>
    public const ushort CinematicDefinitionsEnd = 0xe200;
    /// <summary>Each bank-$8D palette-FX definition stores a four-byte program header.</summary>
    public const int DefinitionByteCount = 4;
    /// <summary>$8D:F745, first definition in the contiguous room-effect group.</summary>
    public const ushort RoomDefinitionsBegin = 0xf745;
    /// <summary>$8D:F7A5, final definition in the contiguous room-effect group.</summary>
    public const ushort RoomDefinitionsEnd = 0xf7a5;
    /// <summary>$8D:F761, Norfair Samus-in-heat palette-FX owner selected by area lists.</summary>
    public const ushort SamusInHeat = 0xf761;
    /// <summary>$8D:FFC9, first definition in the contiguous Tourian escape group.</summary>
    public const ushort TourianDefinitionsBegin = 0xffc9;
    /// <summary>$8D:FFED, final definition in the contiguous Tourian escape group.</summary>
    public const ushort TourianDefinitionsEnd = 0xffed;

    /// <summary>Provides palette effect definition ranges and record width for room, Tourian, and cinematic decoding.</summary>
    extension(RoomPaletteFxDefinitions)
    {
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.CinematicDefinitionsBegin"/>
        internal static ushort CinematicDefinitionsBegin => RoomPaletteFxDefinitionsConstants.CinematicDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.CinematicDefinitionsEnd"/>
        internal static ushort CinematicDefinitionsEnd => RoomPaletteFxDefinitionsConstants.CinematicDefinitionsEnd;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.DefinitionByteCount"/>
        internal static int DefinitionByteCount => RoomPaletteFxDefinitionsConstants.DefinitionByteCount;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.RoomDefinitionsBegin"/>
        internal static ushort RoomDefinitionsBegin => RoomPaletteFxDefinitionsConstants.RoomDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.RoomDefinitionsEnd"/>
        internal static ushort RoomDefinitionsEnd => RoomPaletteFxDefinitionsConstants.RoomDefinitionsEnd;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.SamusInHeat"/>
        internal static ushort SamusInHeat => RoomPaletteFxDefinitionsConstants.SamusInHeat;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.TourianDefinitionsBegin"/>
        internal static ushort TourianDefinitionsBegin => RoomPaletteFxDefinitionsConstants.TourianDefinitionsBegin;
        /// <inheritdoc cref="RoomPaletteFxDefinitionsConstants.TourianDefinitionsEnd"/>
        internal static ushort TourianDefinitionsEnd => RoomPaletteFxDefinitionsConstants.TourianDefinitionsEnd;
    }
}
