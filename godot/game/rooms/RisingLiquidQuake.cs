using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The quake of rising lava and acid: on every update the room's liquid, where it is one of these, waited to move or
/// moved, the room's shake is kept going and, unless the room's quakes are silent, the quake's rumble is heard in its
/// own uneven rhythm.
/// </summary>
/// <remarks>
/// The rumble's rhythm is the one every lasting quake has (<see cref="QuakeRumble"/>). It counts with the liquid's
/// moves as each update opens, as the cartridge's does: through a door's passage too, though nothing is heard
/// there, and not while the room is held still. The cartridge lengthens its waits by the number as it stood before
/// the liquid stirred it on the same move, and the stir has changed its halves round since: the digits it goes by
/// are now the lowest of the high half.
/// </remarks>
public partial class RisingLiquidQuake : RoomSequence, IOpeningStep
{
    /// <summary>Where the digits the cartridge lengthens a wait by lie once the liquid has stirred the number: the high half's lowest.</summary>
    private const int StirredChanceAt = 8;

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
    protected override void OnStarted() => _rumble.StartOver();

    /// <inheritdoc/>
    public void StepOpening(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!Silent && Sound.Length > 0 && Quakes(room))
            _rumble.Step(room, Sound, StirredChanceAt);
    }

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (Quakes(room))
            room.SustainShake(ShakeStrength, ShakeAtLeast, ShakeWay.Across, ShakeReach.Everything);
        return false;
    }

    /// <summary>Whether the room's liquid is one that quakes and waited to move or moved on its last move.</summary>
    private bool Quakes(Room room) => room.Liquid is { IsAstir: true } liquid && Liquids.Contains(liquid.Type.Name);
}
