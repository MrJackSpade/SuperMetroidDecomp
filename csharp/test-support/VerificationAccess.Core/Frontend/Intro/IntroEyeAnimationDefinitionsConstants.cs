namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values and helpers of <see cref="IntroEyeAnimationDefinitions"/> that only verification reads.</summary>
internal static class IntroEyeAnimationDefinitionsConstants
{
    /// <summary>$8C:D5DF, the cartridge's blinking-eye list that <see cref="IntroEyeProgram.Blinking"/> replaces.</summary>
    internal const ushort StartPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>$8C:D613, the cartridge's final-page eye list that <see cref="IntroEyeProgram.FinalPageClosing"/> replaces.</summary>
    internal const ushort FinalPagePointer = CinematicCodePointers.BackgroundLists.SamusBlinkingPage6;
    /// <summary>$8C:D629, exclusive end after the final-page eye loop.</summary>
    internal const ushort EndPointer = 0xd629;
    /// <summary>Four frames referenced by the normal and final-page scripts.</summary>
    internal const int FrameCount = 4;
}
