namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Named bank-$8C indirect-data records compared by cinematic draw code.</summary>
internal static class CinematicCodePointersIndirectData
{
    /// <summary>Bank-8C blank-space list at $8C:D67D used by intro text rendering.</summary>
    public const ushort IntroTextSpace = 0xd67d;
    /// <summary>Bank-8C first intro story-page background list at $8C:C383.</summary>
    public const ushort IntroTextPage1 = 0xc383;
    /// <summary>Bank-8C second intro story-page background list at $8C:C797.</summary>
    public const ushort IntroTextPage2 = 0xc797;
    /// <summary>Bank-8C third intro story-page background list at $8C:CB45.</summary>
    public const ushort IntroTextPage3 = 0xcb45;
    /// <summary>Bank-8C fourth intro story-page background list at $8C:CE33.</summary>
    public const ushort IntroTextPage4 = 0xce33;
    /// <summary>Bank-8C fifth intro story-page background list at $8C:D15D.</summary>
    public const ushort IntroTextPage5 = 0xd15d;
    /// <summary>Bank-8C sixth intro story-page background list at $8C:D511.</summary>
    public const ushort IntroTextPage6 = 0xd511;
}
