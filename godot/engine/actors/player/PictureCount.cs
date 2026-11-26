using System;
using System.Collections.Generic;

namespace Engine.Actors;

/// <summary>
/// The count of one of the player's timed changes: a turn, a change of posture, a jump's gathering, a landing's
/// settling. A change shows its pictures one after another and lasts as long as they do. Each picture is held for
/// its own length and for as much longer as the player's pictures are held as it begins
/// (<see cref="Player.PictureHold"/>), so a change the player is in as they enter or leave a liquid, or as something
/// takes hold of them, goes on from its next picture by where they then are.
/// </summary>
public readonly record struct PictureCount
{
    private readonly IReadOnlyList<int>? _lengths;
    private readonly int _picture;
    private readonly int _left;

    private PictureCount(IReadOnlyList<int> lengths, int picture, int left) => (_lengths, _picture, _left) = (lengths, picture, left);

    /// <summary>A count with nothing left.</summary>
    public static PictureCount Over => default;

    /// <summary>
    /// The count of pictures held for <paramref name="lengths"/> updates each, in order, begun where
    /// <paramref name="owner"/> now is; with no pictures it is over at once.
    /// </summary>
    /// <param name="owner">The player whose pictures they are.</param>
    /// <param name="lengths">How many updates each picture is held, at least one.</param>
    /// <param name="asChangeBegins">
    /// Whether the first picture begins a change of the player's, as all but a count taken up within what they were
    /// already doing do.
    /// </param>
    public static PictureCount Begin(Player owner, IReadOnlyList<int> lengths, bool asChangeBegins = true)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(lengths);
        foreach (int length in lengths)
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        return lengths.Count == 0 ? default : new(lengths, 0, lengths[0] + owner.PictureHold(asChangeBegins));
    }

    /// <summary>Whether any of it is left.</summary>
    public bool IsRunning => _left > 0;

    /// <summary>Whether none of it is left.</summary>
    public bool IsOver => _left <= 0;

    /// <summary>Whether it is on its last update: the next step ends it.</summary>
    public bool IsOnLastUpdate => _lengths is { } lengths && _picture == lengths.Count - 1 && _left == 1;

    /// <summary>
    /// The count one update on. A picture whose time is up gives way to the next, held as long as
    /// <paramref name="owner"/>'s pictures are held on this update; after the last, the count is over.
    /// </summary>
    public PictureCount Stepped(Player owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (_lengths is not { } lengths || _left <= 0)
            return this;
        if (_left > 1)
            return new(lengths, _picture, _left - 1);
        int next = _picture + 1;
        return next < lengths.Count ? new(lengths, next, lengths[next] + owner.PictureHold(asChangeBegins: false)) : default;
    }
}
