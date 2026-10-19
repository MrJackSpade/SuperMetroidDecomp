namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One of Dead Torizo's asymmetric touch rectangles; the overlap test uses the magnitude of the edge facing Samus.</summary>
/// <param name="Left">Left edge relative to the corpse origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the corpse origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the corpse origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the corpse origin, in room pixels.</param>
internal readonly record struct DeadCorpseTouchHitbox(short Left, short Top, short Right, short Bottom);
