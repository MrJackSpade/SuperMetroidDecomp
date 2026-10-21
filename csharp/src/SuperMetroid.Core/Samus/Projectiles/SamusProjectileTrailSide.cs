namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One side of a two-stream bank-$90 projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Room-pixel trail-sprite position.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
    /// <summary>Updates remaining before the current trail instruction advances.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>Bank-$90 pointer to the current trail instruction.</summary>
    public int InstructionPointer { get; internal set; }
    /// <summary>Packed tile number and OBJ attributes drawn for this trail side.</summary>
    public SnesObjAttributeWord TileNumberAttributes { get; internal set; }
    /// <summary>
    /// Room-pixel position the side shows this frame: <see cref="Position"/> when the side last advanced. `$90:B6A9`
    /// draws each side right after advancing it, so a sibling's later move command shows only on the next frame.
    /// </summary>
    public Position16<RoomPixels> DisplayedPosition { get; internal set; }

    /// <summary>Resets the sprite position, timer, instruction pointer, and OBJ attributes for one trail side.</summary>
    internal void ClearFields()
    {
        Position = Position16<RoomPixels>.Zero;
        InstructionTimer = WrappingCountdown.Start(0);
        InstructionPointer = 0;
        TileNumberAttributes = default;
        DisplayedPosition = Position16<RoomPixels>.Zero;
    }
}
