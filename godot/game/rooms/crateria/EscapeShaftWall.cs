using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The wall of the old Tourian shaft that gives way in the escape. Told to "open", it leaves a small blast about the
/// wall's middle, thrown a little way by chance, asks for the blast's sound, and opens the wall's blocks. A trigger
/// tells it as Samus passes the point the cartridge watches for.
/// </summary>
/// <remarks>
/// The blast holds one of the room's places for what its enemies launch and leave, as the cartridge's does. With none
/// free it is not made: nothing is then drawn of chance and nothing is heard, and the wall opens all the same.
/// </remarks>
public partial class EscapeShaftWall : Node, ITriggerTarget, IPlaced
{
    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>The patch of the room's terrain that opens the wall.</summary>
    [Export]
    public string Patch { get; set; } = "";

    /// <summary>The middle of the wall, where the blast is left before chance throws it.</summary>
    [Export]
    public Vector2I BlastAt { get; set; }

    /// <summary>How far the blast may be thrown: the width and the height of the spread, each a power of two, about the wall's middle.</summary>
    [Export]
    public Vector2I BlastSpread { get; set; } = new(8, 16);

    /// <summary>The data file of the blast's timeline.</summary>
    [Export(PropertyHint.File)]
    public string BlastEffect { get; set; } = "";

    /// <summary>The blast's sound, by name.</summary>
    [Export]
    public string BlastSound { get; set; } = "";

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what != "open")
            throw new ArgumentException($"The escape shaft's wall can be told to \"open\", not \"{what}\".", nameof(what));
        Room room = GetParent() as Room ?? throw new InvalidOperationException($"The escape shaft's wall \"{Name}\" is not in a room.");

        // The blast first, as the cartridge makes it before it opens the wall; chance throws it once it has its place.
        Effect blast = Effect.Of(FrameTimeline.Read(room.Content.ReadData(BlastEffect)), PixelPosition.FromWhole(BlastAt.X, BlastAt.Y), holdsPlace: true);
        if (room.Spawn(blast))
        {
            blast.Position += room.Session.Random.Scatter(BlastSpread.X, BlastSpread.Y, 0, 0);
            room.Session.Sounds.Request(BlastSound, channel: "enemies");
        }

        room.Terrain.ApplyPatch(Patch);
    }
}
