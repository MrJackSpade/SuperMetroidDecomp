namespace SuperMetroid.Core.Game;

/// <summary>One side of a two-stream bank-$90 projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Room-pixel trail-sprite position.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
    /// <summary>Updates remaining before the current trail instruction advances.</summary>
    public NativeCountdown InstructionTimer { get; internal set; }
    /// <summary>Bank-$90 pointer to the current trail instruction.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>Packed tile number and OBJ attributes drawn for this trail side.</summary>
    public ushort TileNumberAttributes { get; internal set; }

    /// <summary>Resets the sprite position, timer, instruction pointer, and OBJ attributes for one trail side.</summary>
    internal void ClearFields()
    {
        Position = Position16<RoomPixels>.Zero;
        InstructionTimer = NativeCountdown.Start(0);
        InstructionPointer = 0;
        TileNumberAttributes = 0;
    }
}
