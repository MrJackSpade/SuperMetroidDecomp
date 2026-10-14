namespace SuperMetroid.Core.Game;

/// <summary>
/// One pickup request emitted after Samus shoots a Bomb/Golden Torizo Chozo orb. Opcode
/// <c>$86:AB8A</c> selects one of two enemy headers by area; the initialized Torizo variant
/// is the equivalent concrete authority in this room-scoped runtime.
/// </summary>
public readonly record struct TorizoOrbDropRequest();
