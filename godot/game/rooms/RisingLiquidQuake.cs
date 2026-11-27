using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The quake of rising lava and acid: on every update the room's liquid, where it is one of these, waited to move or
/// moved, the room's shake is kept going and, unless the room's quakes are silent, the quake's rumble is heard in its
/// own uneven rhythm.
/// </summary>
/// <remarks>The rumble's rhythm is the one every lasting quake has (<see cref="QuakeRumble"/>).</remarks>
public partial class RisingLiquidQuake : RoomSequence
{
    /// <summary>The liquids that quake, by their types' names.</summary>
    [Export]
    public string[] Liquids { get; set; } = ["lava", "acid"];

    /// <summary>The rumble, by name.</summary>
    [Export]
    public string Sound { get; set; } = "";

    /// <summary>Whether the room's quakes are silent: it shakes, and no rumble is heard.</summary>
    [Export]
    public bool Silent { get; set; }

    /// <summary>How hard the room shakes, across.</summary>
    [Export]
    public int ShakeStrength { get; set; } = 2;

    /// <summary>The updates of shaking each such update keeps to come at the least: a power of two.</summary>
    [Export]
    public int ShakeAtLeast { get; set; } = 32;

    private readonly QuakeRumble _rumble = new();

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            _rumble.StartOver();
        if (room.Liquid is not { IsAstir: true } liquid || !Liquids.Contains(liquid.Type.Name))
            return false;
        if (!Silent && Sound.Length > 0)
            _rumble.Step(room, Sound);

        room.SustainShake(ShakeStrength, ShakeAtLeast, ShakeWay.Across, ShakeReach.Everything);
        return false;
    }
}
