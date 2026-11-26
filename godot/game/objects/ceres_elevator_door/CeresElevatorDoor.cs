using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// The blocked way to Ceres's elevator. It stands shut until the station's self-destruct has begun; then it is
/// blown apart, blast after blast, and once it is gone the shaft is free to tilt and the elevator to take Samus.
/// While it is whole, and again once it is gone, it keeps the elevator's platform and the room's glow flickering.
/// </summary>
/// <remarks>
/// Its data names its picture, "blocked"; its blasts' effects, "explosion" and "smoke"; and its "explosion_sound".
/// The floor it is given has a part named "Platform".
/// </remarks>
public partial class CeresElevatorDoor : RoomObject
{
    /// <summary>The shaft's picture, whose platform and glow it keeps flickering.</summary>
    [Export]
    public NodePath Floor { get; set; } = new();

    /// <summary>The platform's two pictures.</summary>
    [Export]
    public Godot.Collections.Array<string> PlatformPictures { get; set; } = [];

    /// <summary>The colours the shaft glows through, in order; empty for none.</summary>
    [Export]
    public Godot.Collections.Array<Color> GlowTints { get; set; } = [];

    /// <summary>The picture of it shut.</summary>
    [Export, DataValue("blocked")]
    public string BlockedPicture { get; set; } = "";

    /// <summary>The effect of a blast, by the data file of its timeline.</summary>
    [Export, DataValue("explosion")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The effect of a burst of smoke.</summary>
    [Export, DataValue("smoke")]
    public string SmokeEffect { get; set; } = "";

    /// <summary>The sound of a blast, by name.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    private enum Phase
    {
        Intact,
        Exploding,
        Destroyed,
    }

    /// <summary>How many updates it takes to blow apart, and how many pass between blasts.</summary>
    private const int ExplodingUpdates = 49, BlastInterval = 5;

    /// <summary>Where its blasts go off, from where it stands, in turn.</summary>
    private static readonly PixelOffset[] Blasts =
    [
        PixelOffset.FromWhole(2, 12), PixelOffset.FromWhole(-2, 22), PixelOffset.FromWhole(0, 4), PixelOffset.FromWhole(-4, -8),
    ];

    private Phase _phase;
    private int _age;
    private int _left;
    private int _blastIn;
    private int _blast;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_phase, _age) = (Phase.Intact, 0);
        Show(BlockedPicture);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        switch (_phase)
        {
            case Phase.Intact:
                Flicker();
                if (Room.Session.Progress.Has(StoryEvents.CeresSelfDestructStarted))
                    (_phase, _left, _blastIn, _blast) = (Phase.Exploding, ExplodingUpdates, 1, 0);
                break;

            case Phase.Exploding:
                if (--_left == 0)
                {
                    Show("");
                    _phase = Phase.Destroyed;
                    Room.Session.Progress.Record(StoryEvents.CeresElevatorDoorDestroyed);
                    break;
                }

                if (--_blastIn > 0)
                    break;
                _blastIn = BlastInterval;
                PixelPosition at = Position + Blasts[_blast];
                _blast = (_blast + 1) % Blasts.Length;
                // The blast is one of the enemies' shots on the cartridge: it holds one of the room's places while it
                // plays, and is not made with none free, though it is heard all the same.
                string effect = Room.Session.Random.Draw().Part(14, 2) == 0 ? SmokeEffect : ExplosionEffect;
                if (effect.Length > 0)
                    Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace: true));
                if (ExplosionSound.Length > 0)
                    Room.Session.Sounds.Request(ExplosionSound, channel: "objects");
                break;

            case Phase.Destroyed:
                Flicker();
                break;

            default:
                throw new InvalidOperationException($"The elevator door has no phase {_phase}.");
        }

        _age++;
    }

    /// <summary>Shows the platform and the glow as they are this update.</summary>
    private void Flicker()
    {
        TurningFloor floor = GetNode(Floor) as TurningFloor
            ?? throw new InvalidOperationException($"The elevator door \"{Name}\" is to light \"{Floor}\", but that is no turning floor.");
        if (PlatformPictures.Count > 0)
            floor.ShowPart("Platform", PlatformPictures[_age / 2 % PlatformPictures.Count]);
        if (GlowTints.Count > 0)
            floor.Modulate = GlowTints[_age / 8 % GlowTints.Count];
    }

    private void Show(string picture)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(picture);
    }
}
