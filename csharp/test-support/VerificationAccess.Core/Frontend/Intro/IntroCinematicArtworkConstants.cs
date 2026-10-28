namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Sizes of the opening cinematic's character artwork that only verification reads.</summary>
internal static class IntroCinematicArtworkConstants
{
    /// <summary>The bytes of one four-bit character.</summary>
    private const int FourBitCharacterBytes = 32;

    /// <summary>The cinematic sprites' 288 four-bit characters.</summary>
    public const int ObjectCharacterBytes = 288 * FourBitCharacterBytes;
}
