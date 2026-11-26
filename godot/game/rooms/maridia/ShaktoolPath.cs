using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The watch kept on Shaktool's room until its path has been cleared. From its second update on, on every update:
/// while a power bomb of Samus's explodes it opens every screen of the room to the view, each held to its top, so
/// that she can follow what the blast has dug out; and once her place is past the far end of the path it is done,
/// which records that the path was cleared, and the room is as cleared from her next visit.
/// </summary>
public partial class ShaktoolPath : RoomSequence
{
    /// <summary>How far across the room Samus's place must have passed for the path to count as cleared, in pixels.</summary>
    [Export]
    public int ClearedPast { get; set; }

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        // Its first update only begins the watch.
        if (elapsed == 0)
            return false;
        if (room.Projectiles.OfType<PowerBomb>().Any(bomb => bomb.IsExploding))
        {
            int columns = Math.Max(1, (room.Terrain.WidthInPixels + Camera.Screen - 1) / Camera.Screen);
            int rows = Math.Max(1, (room.Terrain.HeightInPixels + Camera.Screen - 1) / Camera.Screen);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                    room.Session.ScrollCells.Write(column, row, ScreenScroll.OpenAlignedToTop);
            }
        }

        return room.Player.Position.X.Whole > ClearedPast;
    }
}
