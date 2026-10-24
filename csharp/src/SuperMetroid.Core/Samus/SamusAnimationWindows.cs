namespace SuperMetroid.Core.Samus;

/// <summary>
/// Where Samus's compiled animation regions sit in their banks' sixteen-bit windows (#1411), read from the player's
/// cartridge at install. A restored animation index reading past its program reads on through the window: other compiled
/// data, the words naming the programs, or the bank's low half, which aliases the floor update residue.
/// </summary>
/// <param name="ChargeFlarePrograms">Where the charge flare's main-flare program starts.</param>
/// <param name="PoseDelayPrograms">Where the pose delay programs start.</param>
/// <param name="RunningCadences">Where the running cadences start, at the word naming the ordinary stream.</param>
public sealed record SamusAnimationWindows(int ChargeFlarePrograms, int PoseDelayPrograms, int RunningCadences);
