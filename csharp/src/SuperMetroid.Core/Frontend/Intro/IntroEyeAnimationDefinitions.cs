namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Where the portrait eye is drawn, and where the cartridge keeps the eye's background-object lists and draw records.
/// The simulation runs <see cref="IntroEyeAnimations"/> and draws at <see cref="PortraitCell"/>; the list and record
/// addresses are read only by the eye artwork extraction and by the ROM oracle that checks the typed programs.
/// </summary>
internal static class IntroEyeAnimationDefinitions
{
    /// <summary>$8C:D5DF, the cartridge's blinking-eye list that <see cref="IntroEyeProgram.Blinking"/> replaces.</summary>
    internal const ushort StartPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>$8C:D613, the cartridge's final-page eye list that <see cref="IntroEyeProgram.FinalPageClosing"/> replaces.</summary>
    internal const ushort FinalPagePointer = CinematicCodePointers.BackgroundLists.SamusBlinkingPage6;
    /// <summary>$8C:D629, exclusive end after the final-page eye loop.</summary>
    internal const ushort EndPointer = 0xd629;
    /// <summary>$8C:D781, first of four 16-byte portrait-eye draw records, in <see cref="IntroEyeFrame"/> order.</summary>
    internal const ushort FrameStartPointer = 0xd781;
    /// <summary>Four frames referenced by the normal and final-page scripts.</summary>
    internal const int FrameCount = 4;
    /// <summary>Each native record holds a draw function, dimensions and six words.</summary>
    internal const int FrameStride = 0x10;
    /// <summary>Native portrait-eye rectangle size in BG tiles: three columns by two rows.</summary>
    internal static Extent16<TilemapCells> FrameSize => new(3, 2);
    /// <summary>The portrait tilemap cell at which every eye frame's top-left tile is drawn: column 17, row 13.</summary>
    internal static Position16<TilemapCells> PortraitCell => new(17, 13);
}
