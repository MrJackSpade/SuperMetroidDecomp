namespace SuperMetroid.Core.Samus;

/// <summary>Wall-jump animation boundaries used by the cartridge's spin-sound command.</summary>
internal static class SamusSpinSoundFrames
{
    /// <summary>$90:F451, SamusCommand_1C: first wall-jump frame selecting Space Jump audio.</summary>
    public const int SpaceJump = 13;
    /// <summary>$90:F44C, SamusCommand_1C: first wall-jump frame selecting Screw Attack audio.</summary>
    public const int ScrewAttack = 23;
}
