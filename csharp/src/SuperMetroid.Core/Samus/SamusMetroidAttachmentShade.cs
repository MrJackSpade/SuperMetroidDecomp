namespace SuperMetroid.Core.Samus;

/// <summary>
/// The suit shade a draining Metroid or the Baby Metroid makes Samus flash through next (#1470): attachment starts on
/// the Speed Booster glow, and each update alternates it with the normal suit until the attacker lets go.
/// </summary>
public enum SamusMetroidAttachmentShade
{
    /// <summary>No Metroid is draining Samus: the alternation leaves the shade alone.</summary>
    Inactive,

    /// <summary>The next update shows the Speed Booster glow.</summary>
    SpeedBoostNext,

    /// <summary>The next update restores the normal suit shade.</summary>
    NormalSuitNext,
}
