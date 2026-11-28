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

    /// <summary>Whether the block is giving way under something that stood on it, or since a line latched onto it.</summary>
    private bool _stoodOn;

    /// <summary>How many updates it has been broken for.</summary>
    private int _brokenFor;

    /// <summary>Whether the block is solid right now.</summary>
    public bool IsSolid => Type.Solid && (!Broken || Type.StaysSolid || (Type.BlocksWhileCrumbling && ShowsChange()));

    /// <summary>
    /// Whether the last picture of its breaking showed on the update before this one: the update a type that has a
    /// breaking records its fact on. True through one update, the one its timers were moved on to.
    /// </summary>
    internal bool HasShownItsBreaking => Broken && Type.Breaking is { } breaking && _brokenFor == breaking.Steps.Sum(step => step.Updates);

    /// <summary>Shows the block for what it is.</summary>
    internal void Reveal() => Hidden = false;

    /// <summary>Breaks the block and starts its regrowth, if it regrows.</summary>
    internal void Break()
    {
        Broken = true;
        Hidden = false;
        _stoodOn = false;
        _brokenFor = 0;
        _regrow = Countdown.Of(Type.RegrowsAfter);
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
            string own = Hidden && _seenFor == 0 ? Type.HiddenPicture : Type.IntactPictureOf(part);
            return own.Length > 0 ? own : tilePicture;
        }

        if (Type.Breaking is { } breaking && StepAt(breaking, _brokenFor) is { } crumbling)
            return Type.BreakingPictureOf(crumbling, part);
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
    private static string? PictureAt(FrameTimeline timeline, int updates) =>
        StepAt(timeline, updates) is { } step ? timeline.Steps[step].Picture : null;

    /// <summary>The place of the step a timeline is at so many updates in; null once it is over.</summary>
    private static int? StepAt(FrameTimeline timeline, int updates)
    {
        for (int index = 0; index < timeline.Steps.Count; index++)
        {
            if (updates < timeline.Steps[index].Updates)
                return index;
            updates -= timeline.Steps[index].Updates;
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

    /// <summary>Tells the block a line has latched onto it. One that lasts only so long once latched starts giving way.</summary>
    internal void LatchOnto()
    {
        if (Broken || _stoodOn || Type.Contact != BlockContact.LatchOnce || Type.LatchedUpdates == 0)
            return;
        _stoodOn = true;
        _giveWay = Countdown.Of(Type.LatchedUpdates);
    }

    /// <summary>How many more updates it is shown for what it is because a revealing ability saw it.</summary>
    private int _seenFor;

    /// <summary>Shows the block for what it is for as long as a revealing ability goes on seeing it.</summary>
    internal void SeeBySight() => _seenFor = 2;

    /// <summary>Moves the block's timers on by one update. Returns whether its solidity or its look may have changed.</summary>
    /// <param name="spaceIsClear">Whether nothing occupies the block's space, so that it may regrow.</param>
    internal bool Step(bool spaceIsClear)
    {
        // Seen, it shows for what it is through this update; unseen the next, it looks like plain terrain again.
        bool wasSeen = _seenFor > 0;
        if (wasSeen)
            _seenFor--;
        return StepTimers(spaceIsClear) | wasSeen;
    }

    private bool StepTimers(bool spaceIsClear)
    {
        if (_stoodOn)
        {
            // The update it is first stood on, or latched onto, is the first of those it lasts: it breaks as the
            // last of them ends, and is gone from the next.
            _giveWay = _giveWay.Stepped();
            if (!_giveWay.IsOver)
                return false;
            Break();
            return true;
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
