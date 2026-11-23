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
        _regrow = Countdown.Of(Type.RegrowsAfter);
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

        if (!Broken || Type.RegrowsAfter == 0)
            return false;
        if (_regrow.IsRunning)
        {
            _regrow = _regrow.Stepped();
            return false;
        }

        if (!spaceIsClear)
            return false;
        Broken = false;
        return true;
    }
}
