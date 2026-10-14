namespace SuperMetroid.Core.Frontend;

/// <summary>Four palette boxes belonging to one binary special-setting row.</summary>
/// <param name="EnabledTop">Page-relative byte offset of the enabled-choice top row, covering <see cref="GameOptionsRomData.SpecialToggles.PaletteRegionByteCount"/> bytes.</param>
/// <param name="EnabledBottom">Page-relative byte offset of the enabled-choice bottom row, one $40-byte tilemap row below its top.</param>
/// <param name="DisabledTop">Page-relative byte offset of the disabled-choice top row, seven tilemap words to the right of the enabled choice.</param>
/// <param name="DisabledBottom">Page-relative byte offset of the disabled-choice bottom row, one $40-byte tilemap row below its top.</param>
public readonly record struct GameOptionsToggleLayout(
    int EnabledTop,
    int EnabledBottom,
    int DisabledTop,
    int DisabledBottom);
