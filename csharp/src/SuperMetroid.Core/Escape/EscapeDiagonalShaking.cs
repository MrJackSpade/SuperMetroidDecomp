using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// How an escape room's horizontal shaking turns diagonal now and then (#2287): on an update the chance takes, the room
/// shakes diagonally for <see cref="ZebesEscapeDefinitions.DiagonalFrames"/> updates, then horizontally again.
/// </summary>
/// <param name="Horizontal">The shake the room returns to.</param>
/// <param name="Diagonal">The shake the room turns to.</param>
/// <param name="TurnsDiagonal">The chance an update of horizontal shaking turns diagonal.</param>
public sealed partial record EscapeDiagonalShaking(EarthquakeShake Horizontal, EarthquakeShake Diagonal, RandomChance TurnsDiagonal);
