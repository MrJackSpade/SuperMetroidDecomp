namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One pickup request emitted after Samus shoots a Bomb/Golden Torizo Chozo orb. The orb's drop step
/// selects one of two enemy definitions by area; the initialized Torizo variant
/// is the equivalent concrete authority in this room-scoped runtime.
/// </summary>
public readonly record struct TorizoOrbDropRequest();
