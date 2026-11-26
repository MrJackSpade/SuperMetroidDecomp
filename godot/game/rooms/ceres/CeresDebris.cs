using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// Pieces of Ceres's ceiling coming down once Ridley has gone: every few updates one drops from the top of the room
/// in a column chosen by chance, and breaks into dust where it lands.
/// </summary>
public partial class CeresDebris : RoomSequence
{
    /// <summary>The light piece, by the data file of its shot.</summary>
    [Export(PropertyHint.File)]
    public string LightPiece { get; set; } = "";

    /// <summary>The dark piece.</summary>
    [Export(PropertyHint.File)]
    public string DarkPiece { get; set; } = "";

    /// <summary>How many updates pass between one piece and the next.</summary>
    private const int Interval = 9;

    /// <summary>How far down the room a piece starts.</summary>
    private const int StartsAtY = 42;

    /// <summary>The columns a piece may fall in, by how far across the room.</summary>
    private static readonly int[] Columns = [80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 272, 304, 336, 368, 400];

    private int _wait;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            _wait = Interval;
        if (--_wait > 0)
            return false;
        _wait = Interval;
        RandomDraw draw = room.Session.Random.Last;
        string piece = draw.Part(15, 1) == 1 ? DarkPiece : LightPiece;
        ShotType shot = ShotType.Read(room.Content.ReadData(piece));
        var at = PixelPosition.FromWhole(Columns[draw.Part(0, 4)], StartsAtY);
        Projectile fallen = shot.Launch(at, new PixelVelocity(Pixels.Zero, shot.Speed), HitSide.Enemies, null, this);
        // The cartridge draws a piece behind Samus and all else standing in the room; its dust is drawn in front of her.
        if (room.Spawn(fallen))
            RoomDepth.Stand(fallen, Standing.Rearmost);
        return false;
    }
}
