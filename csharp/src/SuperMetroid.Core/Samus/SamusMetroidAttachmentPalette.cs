namespace SuperMetroid.Core.Samus;

/// <summary>
/// The suit colours a draining Metroid or the Baby Metroid makes Samus flash through next (#1470): attachment starts on
/// the Speed Booster colouring, and each update alternates it with the normal suit until the attacker lets go.
/// </summary>
public enum SamusMetroidAttachmentPalette
{
    /// <summary>No Metroid is draining Samus: the alternation leaves her colours alone.</summary>
    Inactive,

    /// <summary>The next update shows the Speed Booster colouring.</summary>
    SpeedBoostNext,

    /// <summary>The next update restores the normal suit colours.</summary>
    NormalSuitNext,
}
