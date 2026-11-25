using Engine.Shared;

namespace Engine.Collision;

/// <summary>
/// What terrain makes of a move tried only where it ends: whether it stops a body whose leading edge would end where
/// it was asked about, and where that leaves the body.
/// </summary>
/// <param name="Stopped">Whether the terrain stops the move.</param>
/// <param name="Back">
/// How many pixels back from where it would have ended the edge comes to rest; null when the body does not move at
/// all.
/// </param>
/// <param name="KeepsFraction">
/// Whether the body keeps the part of a pixel it had, as one stopped by sloped ground under its middle does; a body
/// stopped by anything else rests at the end of its pixel that lies against what stopped it.
/// </param>
public readonly record struct EdgeStop(bool Stopped, int? Back, bool KeepsFraction)
{
    /// <summary>The tile of terrain that stopped the move, in room pixels; null when none did, or what stopped it is no tile.</summary>
    public PixelRect? Tile { get; init; }

    /// <summary>
    /// Whether the stop leaves the body held by sloped or stepped ground, as a move down onto a step does: the body's
    /// owner then presses down to it by a pixel, not by its reach.
    /// </summary>
    public bool PutsOnSlope { get; init; }

    /// <summary>Nothing stops the move.</summary>
    public static EdgeStop None => default;
}
