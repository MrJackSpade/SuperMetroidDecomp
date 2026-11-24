using Engine.Shared;

namespace Engine.World;

/// <summary>One block in a room's terrain: where it is, what type it is, and its state right now.</summary>
public sealed class Block
{
    internal Block(BlockType type, PixelRect rect)
    {
        Type = type;
        Rect = rect;
        Hidden = type.StartsHidden;
    }

    /// <summary>The block's type.</summary>
    public BlockType Type { get; }

    /// <summary>The pixels the block covers, in the room.</summary>
    public PixelRect Rect { get; }

    /// <summary>Whether the block is broken: gone until it regrows or its room is set up again.</summary>
    public bool Broken { get; private set; }

    /// <summary>Whether the block still looks like plain terrain.</summary>
    public bool Hidden { get; private set; }

    /// <summary>The updates left before a broken block regrows.</summary>
    private Countdown _regrow = Countdown.Over;

    /// <summary>The updates left before a block that is being stood on breaks.</summary>
    private Countdown _giveWay = Countdown.Over;

    /// <summary>Whether the block is giving way under something that stood on it.</summary>
    private bool _stoodOn;

    /// <summary>How many updates it has been broken for.</summary>
    private int _brokenFor;

    /// <summary>Whether it has broken since this was last asked.</summary>
    private bool _justBroke;

    /// <summary>Whether the block is solid right now.</summary>
    public bool IsSolid => !Broken && Type.Solid;

    /// <summary>Shows the block for what it is.</summary>
    internal void Reveal() => Hidden = false;

    /// <summary>Breaks the block and starts its regrowth, if it regrows.</summary>
    internal void Break()
    {
        Broken = true;
        Hidden = false;
        _stoodOn = false;
        _brokenFor = 0;
        _justBroke = true;
        _regrow = Countdown.Of(Type.RegrowsAfter);
    }

    /// <summary>Whether it has broken since this was last asked.</summary>
    internal bool TakeJustBroke()
    {
        bool broke = _justBroke;
        _justBroke = false;
        return broke;
    }

    /// <summary>
    /// The picture one of its tiles shows now, given the tile's own and its place in the block, row by row: the tile's
    /// own or its type's while it stands, and while it is broken the pictures of its breaking and then, as it is about
    /// to come back, of its regrowing.
    /// </summary>
    public string PictureNow(string tilePicture, int part)
    {
        if (!Broken)
        {
            string own = Hidden ? Type.HiddenPicture : Type.IntactPictureOf(part);
            return own.Length > 0 ? own : tilePicture;
        }

        if (Type.Breaking is { } breaking && PictureAt(breaking, _brokenFor) is { } crumbling)
            return crumbling;
        if (Type.RegrowsAfter > 0 && Type.Regrowing is { } regrowing)
        {
            int left = Type.RegrowsAfter - _brokenFor;
            int length = regrowing.Steps.Sum(step => step.Updates);
            if (left <= length && left > 0 && PictureAt(regrowing, length - left) is { } returning)
                return returning;
        }

        return "";
    }

    /// <summary>Whether what it shows may have changed this update: it is breaking, or about to come back.</summary>
    private bool ShowsChange()
    {
        if (Type.Breaking is { } breaking && _brokenFor <= breaking.Steps.Sum(step => step.Updates))
            return true;
        return Type.RegrowsAfter > 0 && Type.Regrowing is { } regrowing
            && Type.RegrowsAfter - _brokenFor <= regrowing.Steps.Sum(step => step.Updates);
    }

    /// <summary>The picture a timeline shows so many updates in; null once it is over.</summary>
    private static string? PictureAt(FrameTimeline timeline, int updates)
    {
        foreach (TimelineStep step in timeline.Steps)
        {
            if (updates < step.Updates)
                return step.Picture;
            updates -= step.Updates;
        }

        return null;
    }

    /// <summary>Tells the block something is standing on it.</summary>
    internal void StandOn()
    {
        if (Broken || !Type.BreaksWhenStoodOn || _stoodOn)
            return;
        _stoodOn = true;
        _giveWay = Countdown.Of(Type.StoodOnUpdates);
    }

    /// <summary>Moves the block's timers on by one update. Returns whether its solidity changed.</summary>
    /// <param name="spaceIsClear">Whether nothing occupies the block's space, so that it may regrow.</param>
    internal bool Step(bool spaceIsClear)
    {
        if (_stoodOn)
        {
            if (_giveWay.IsOver)
            {
                Break();
                return true;
            }

            _giveWay = _giveWay.Stepped();
            return false;
        }

        if (!Broken)
            return false;
        _brokenFor++;
        if (Type.RegrowsAfter == 0)
            return ShowsChange();
        if (_regrow.IsRunning)
        {
            _regrow = _regrow.Stepped();
            return ShowsChange();
        }

        if (!spaceIsClear)
            return false;
        Broken = false;
        Hidden = Type.HidesAgain && Type.StartsHidden;
        return true;
    }
}
