namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Where Samus stands from Phantoon for one eye check, and the direction the eye then looks.</summary>
/// <param name="X">Samus's horizontal offset from Phantoon.</param>
/// <param name="Y">Samus's vertical offset from Phantoon.</param>
/// <param name="Direction">The direction the eye looks.</param>
/// <param name="CartridgeSelector">The position of that direction's gaze in the cartridge's eye table, the oracle.</param>
internal readonly record struct EyeTarget(short X, short Y, CompassDirection Direction, int CartridgeSelector);
