namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>One shot of a Super Missile pair against Mother Brain's glass: the list it plays, its radius and whether it is the link.</summary>
/// <param name="List">The native list the shot plays.</param>
/// <param name="Radius">The shot's radius.</param>
/// <param name="IsLink">Whether the shot is the Super Missile's link rather than the missile its owner flies.</param>
internal readonly record struct MotherBrainGlassSuperMissilePair(ushort List, ushort Radius, bool IsLink);
