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
/// <param name="BySlopeUnderMiddle">
/// Whether what stopped the move is sloped ground that holds things by their middle, met under the body's middle:
/// where that leaves the body is the move's to say (<see cref="MoveOptions.RestsAgainstSlopes"/>). A body stopped
/// by anything else rests at the end of its pixel that lies against what stopped it.
/// </param>
public readonly record struct EdgeStop(bool Stopped, int? Back, bool BySlopeUnderMiddle)
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
