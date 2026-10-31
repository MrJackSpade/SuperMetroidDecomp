namespace SuperMetroid.Core.Samus;

/// <summary>
/// Where the charge flare's animation programs sit in their bank's sixteen-bit window (#1411), read from the player's
/// cartridge at install. A charge-flare index wrapping below its program reads the words naming the programs, whose
/// values are these window positions.
/// </summary>
/// <param name="ChargeFlarePrograms">Where the charge flare's main-flare program starts.</param>
public sealed record SamusAnimationWindows(int ChargeFlarePrograms);
