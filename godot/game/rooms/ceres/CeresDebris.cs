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
        // The hall sets up no wait of its own: its first piece waits by what the shaft's turn, or its own count the
        // last time it was here, left; and it leaves the updates before its next piece as it goes.
        CeresDebrisCount count = room.Session.Service<CeresDebrisCount>();
        if (elapsed == 0)
            _wait = count.FirstWait;
        bool falls = --_wait <= 0;
        if (falls)
            _wait = Interval;
        count.LeftByHall(_wait - 1);
        if (!falls)
            return false;
        RandomDraw draw = room.Session.Random.Last;
        string piece = draw.Part(15, 1) == 1 ? DarkPiece : LightPiece;
        ShotType shot = ShotType.Read(room.Content.ReadData(piece), room.Content);
        var at = PixelPosition.FromWhole(Columns[draw.Part(0, 4)], StartsAtY);
        Projectile fallen = shot.Launch(at, new PixelVelocity(Pixels.Zero, shot.Speed), HitSide.Enemies, null, this);
        // The cartridge draws a piece behind Samus and all else standing in the room; its dust is drawn in front of her.
        if (room.Spawn(fallen))
            RoomDepth.Stand(fallen, Standing.Rearmost);
        return false;
    }
}
