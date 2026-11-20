namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// One loop of a falling Choot's path: the offsets from the falling origin it has still to show, one per update. Each
/// falling update shows the next, and the update that finds none left ends the loop, so no update lies past the path.
/// </summary>
public sealed class ChootFallLoop
{
    /// <summary>The offsets of the whole loop.</summary>
    private readonly ReadOnlyMemory<Offset16<RoomPixels>> _loop;

    /// <summary>The offsets of the loop it has not yet shown.</summary>
    private ReadOnlyMemory<Offset16<RoomPixels>> _unshown;

    /// <summary>Starts a loop that shows <paramref name="loop"/>'s offsets in turn.</summary>
    private ChootFallLoop(ReadOnlyMemory<Offset16<RoomPixels>> loop)
    {
        _loop = loop;
        _unshown = loop;
    }

    /// <summary>A loop with no offset left to show, which the next falling update ends.</summary>
    internal static ChootFallLoop Ended() => new(ReadOnlyMemory<Offset16<RoomPixels>>.Empty);

    /// <summary>A loop of <paramref name="fallingPath"/> that shows none of its offsets yet.</summary>
    internal static ChootFallLoop Of(ChootFallingPath fallingPath) => new(ChootFalls.LoopOf(fallingPath));

    /// <summary>How many of the loop's offsets it has shown: the update of the loop the next falling update takes.</summary>
    public int Age => _loop.Length - _unshown.Length;

    /// <summary>Shows the loop's next offset, or answers false when it has shown every offset and the loop ends.</summary>
    /// <param name="offset">The offset from the falling origin the update shows.</param>
    internal bool TryShowNext(out Offset16<RoomPixels> offset)
    {
        if (_unshown.IsEmpty)
        {
            offset = default;
            return false;
        }

        offset = _unshown.Span[0];
        _unshown = _unshown[1..];
        return true;
    }
}
