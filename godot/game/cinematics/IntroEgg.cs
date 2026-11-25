using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The Metroid egg Samus finds, in the opening's flashback. When she comes near it rocks and hatches, its shell
/// flying apart; the larva rises out of it, shedding slime, hangs a while above her, and then follows her about,
/// crying. Once the egg's remains have lain long enough it asks for the next page, and it is gone once that has come.
/// </summary>
/// <remarks>
/// Its data names the timelines of its two visuals, "Egg" and "Baby": "intact", "hatching" (which gives the cues
/// "burst" and "next_page") and "baby"; the six pictures of its "shell" pieces and where a piece's picture lies from
/// the piece's place ("shell_offset"); how many updates a piece of shell's curve covers ("shell_curve_updates") and how
/// far a piece still falling past it falls on each update after it ("shell_fall_past_curve"); its effects "slime" (which loops)
/// and "slime_splash" by the data files of their timelines; and its "hatch_sound" and "cry_sound".
/// </remarks>
public partial class IntroEgg : RoomObject
{
    /// <summary>The sound of it hatching, by name.</summary>
    [Export, DataValue("hatch_sound")]
    public string HatchSound { get; set; } = "";

    /// <summary>The larva's cry.</summary>
    [Export, DataValue("cry_sound")]
    public string CrySound { get; set; } = "";

    /// <summary>The effect of a drop of slime, by the data file of its timeline.</summary>
    [Export, DataValue("slime")]
    public string SlimeEffect { get; set; } = "";

    /// <summary>The effect of a drop of slime landing.</summary>
    [Export, DataValue("slime_splash")]
    public string SlimeSplashEffect { get; set; } = "";

    private enum Baby
    {
        WaitingForHatch,
        Rising,
        Idling,
        Dancing,
    }

    /// <summary>How near Samus comes, across the room, before it hatches.</summary>
    private const int HatchX = 169;

    /// <summary>Where the larva sheds its slime, where slime and shell land, and how far above Samus it rises and then keeps.</summary>
    private const int SlimeShedY = 145, LandingY = 168, RiseAboveSamus = 32, DanceAboveSamus = 8;

    /// <summary>How long the larva hangs at the top of its rise, and for how long after it cries as it follows her.</summary>
    private const int HangUpdates = 128, CryingUpdates = 128;

    private static readonly Pixels Gain = Pixels.FromNumber(0.125), MostDown = Pixels.FromNumber(2.125), MostAcross = Pixels.FromNumber(2.5);

    /// <summary>How fast a thrown thing rises on each of its first updates; after them it falls an eighth of a pixel faster each update.</summary>
    private static readonly double[] LowThrow = [-2, -1.25, -1.375, -1.5, -1.625, -1.75, -1.875, -1, -0.25, -0.375, -0.5, -0.625, -0.75, -0.875];
    private static readonly double[] HighThrow = [-3, -2.25, -2.375, -2.5, -2.625, -2.75, -2.875, .. LowThrow];

    /// <summary>The shell's pieces: where each starts and how far across it flies each update.</summary>
    private static readonly (int X, int Y, double Drift)[] ShellPieces =
    [
        (108, 147, -0.75), (115, 147, 0.25), (105, 152, -0.5), (112, 150, -0.875), (118, 153, 0.5), (115, 155, 0.125),
    ];

    /// <summary>The drops of slime: how far across each flies each update, and whether it is thrown high.</summary>
    private static readonly (double Drift, bool High)[] SlimeDrops = [(-1, true), (-0.5, false), (1, true), (0.5, false)];

    /// <summary>How many updates a piece of shell's curve covers, after which it falls by the steps its data gives.</summary>
    private int _shellCurveUpdates;

    private readonly List<string> _shellPictures = [];
    private readonly List<Pixels> _shellFallPastCurve = [];
    private (int X, int Y) _shellOffset;
    private readonly List<(Effect Thing, double Drift, bool High, int Age, bool IsSlime)> _thrown = [];
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _egg;
    private TimelinePlayer? _babyLook;
    private Baby _baby;
    private PixelPosition _babyAt;
    private PixelVelocity _babySpeed;
    private bool _hatching;
    private bool _burst;
    private bool _shedSlime;
    private int _wait;
    private int _crying;

    /// <summary>Whether its remains have lain long enough that the page should turn.</summary>
    public bool WantsNextPage { get; private set; }

    /// <summary>Whether the larva is, this update, on the near side of Samus.</summary>
    public bool BabyIsInFront { get; private set; }

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
        _shellPictures.Clear();
        _shellPictures.AddRange(data.List("shell").Select(piece => piece.AsText()));
        if (_shellPictures.Count != ShellPieces.Length)
            throw data.Problem($"must name {ShellPieces.Length} pieces of shell");
        _shellOffset = (data.Get("shell_offset").Int("x"), data.Get("shell_offset").Int("y"));
        _shellCurveUpdates = data.Int("shell_curve_updates");
        _shellFallPastCurve.Clear();
        _shellFallPastCurve.AddRange(data.List("shell_fall_past_curve").Select(step => step.AsPixels()));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_hatching, _burst, _shedSlime, WantsNextPage, BabyIsInFront) = (false, false, false, false, false);
        (_baby, _babyAt, _babySpeed) = (Baby.WaitingForHatch, Position, PixelVelocity.Zero);
        _thrown.Clear();
        _egg = new TimelinePlayer(_timelines.Get("intact"));
        _babyLook = new TimelinePlayer(_timelines.Get("baby"));
        Visual("Egg").Show(_egg.Current);
        Visual("Baby").Show(_babyLook.Current);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        if (!_hatching && samus.Position.X.Whole < HatchX)
        {
            _hatching = true;
            _egg = new TimelinePlayer(_timelines.Get("hatching"));
        }

        TimelinePlayer egg = _egg ?? throw new InvalidOperationException("The egg is not in a room.");
        foreach (string cue in egg.Step())
        {
            switch (cue)
            {
                case "burst":
                    Burst();
                    break;
                case "next_page":
                    WantsNextPage = true;
                    break;
                default:
                    throw new InvalidOperationException($"The egg's timeline gives the cue \"{cue}\", which it does not know.");
            }
        }

        Visual("Egg").Show(egg.Current);
        StepBaby(samus);
        StepThrown();
    }

    /// <summary>Takes the egg and the larva out of the room, once the page has turned.</summary>
    public void Leave()
    {
        foreach ((Effect thing, _, _, _, _) in _thrown)
            Room.Remove(thing);
        _thrown.Clear();
        Room.Remove(this);
    }

    /// <summary>The shell flies apart.</summary>
    private void Burst()
    {
        _burst = true;
        for (int piece = 0; piece < ShellPieces.Length; piece++)
        {
            (int x, int y, double drift) = ShellPieces[piece];
            var still = new FrameTimeline([new TimelineStep(_shellPictures[piece], 1, []) { Offset = _shellOffset }], 0);
            Effect shell = Effect.Of(still, PixelPosition.FromWhole(x, y));
            Room.Spawn(shell);
            _thrown.Add((shell, drift, false, 0, false));
        }

        if (HatchSound.Length > 0)
            Room.Session.Sounds.Request(HatchSound, channel: "objects");
    }

    private void StepBaby(Player samus)
    {
        switch (_baby)
        {
            case Baby.WaitingForHatch:
                if (_burst)
                    (_baby, _babySpeed) = (Baby.Rising, PixelVelocity.Zero);
                break;

            case Baby.Rising:
                if (_babyAt.Y.Whole == SlimeShedY && !_shedSlime)
                {
                    _shedSlime = true;
                    foreach ((double drift, bool high) in SlimeDrops)
                    {
                        if (SlimeEffect.Length == 0)
                            continue;
                        Effect drop = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(SlimeEffect)), _babyAt);
                        Room.Spawn(drop);
                        _thrown.Add((drop, drift, high, 0, true));
                    }

                    Cry();
                }

                _babySpeed = _babySpeed.WithY(Toward(_babyAt.Y.Whole, samus.Position.Y.Whole - RiseAboveSamus, _babySpeed.Y, MostDown));
                _babyAt = new PixelPosition(_babyAt.X, _babyAt.Y + _babySpeed.Y);

                // It has passed the height it was making for and stopped: it hangs there.
                if (!_babySpeed.Y.IsNegative)
                    (_baby, _wait) = (Baby.Idling, HangUpdates);
                break;

            case Baby.Idling:
                if (--_wait > 0)
                    break;
                (_baby, _crying) = (Baby.Dancing, 0);
                _babySpeed = _babySpeed.WithY(Pixels.Zero);
                break;

            case Baby.Dancing:
                if (_crying < CryingUpdates && ++_crying % 64 == 0)
                    Cry();
                _babySpeed = new PixelVelocity(
                    Toward(_babyAt.X.Whole, samus.Position.X.Whole, _babySpeed.X, MostAcross), _babySpeed.Y);
                BabyIsInFront = !_babySpeed.X.IsNegative;
                _babyAt = new PixelPosition(_babyAt.X + _babySpeed.X, _babyAt.Y);
                _babySpeed = _babySpeed.WithY(Toward(_babyAt.Y.Whole, samus.Position.Y.Whole - DanceAboveSamus, _babySpeed.Y, MostDown));
                _babyAt = new PixelPosition(_babyAt.X, _babyAt.Y + _babySpeed.Y);
                break;

            default:
                throw new InvalidOperationException($"The larva has no way of moving named {_baby}.");
        }

        TimelinePlayer look = _babyLook ?? throw new InvalidOperationException("The egg is not in a room.");
        look.Step();
        ActorVisual baby = Visual("Baby");
        baby.Show(look.Current);
        PixelOffset from = _babyAt - Position;
        baby.Position = new Vector2(from.X.ToDisplay(), from.Y.ToDisplay());
        baby.ZIndex = BabyIsInFront ? 1 : -1;
    }

    /// <summary>The speed that takes something a step nearer a place, never past its limit either way.</summary>
    private static Pixels Toward(int from, int to, Pixels speed, Pixels limit)
    {
        if (to >= from)
            return speed + Gain > limit ? limit : speed + Gain;
        return speed - Gain < -limit ? -limit : speed - Gain;
    }

    /// <summary>Flies the shell's pieces and the slime: up at first, then falling, until they land.</summary>
    private void StepThrown()
    {
        for (int index = _thrown.Count - 1; index >= 0; index--)
        {
            (Effect thing, double drift, bool high, int age, bool isSlime) = _thrown[index];
            double[] rise = high ? HighThrow : LowThrow;
            Pixels down = age < rise.Length ? Pixels.FromNumber(rise[age])
                : !isSlime && age >= _shellCurveUpdates ? ShellStepPastCurve(age - _shellCurveUpdates)
                : Pixels.FromNumber(0.125 * (age - rise.Length));
            thing.Position += new PixelOffset(Pixels.FromNumber(drift), down);
            if (thing.Position.Y.Whole < LandingY)
            {
                _thrown[index] = (thing, drift, high, age + 1, isSlime);
                continue;
            }

            if (isSlime && SlimeSplashEffect.Length > 0)
                Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(SlimeSplashEffect)), thing.Position));
            Room.Remove(thing);
            _thrown.RemoveAt(index);
        }
    }

    /// <summary>
    /// How far a piece of shell falls on an update past the end of its curve: the cartridge reads its steps from the
    /// data after the curve, which its data file gives.
    /// </summary>
    private Pixels ShellStepPastCurve(int updatesPastCurve) =>
        updatesPastCurve < _shellFallPastCurve.Count
            ? _shellFallPastCurve[updatesPastCurve]
            : throw new InvalidOperationException($"A piece of shell is still falling {updatesPastCurve + 1} updates past its curve, which its data gives only {_shellFallPastCurve.Count} steps for.");

    private void Cry()
    {
        if (CrySound.Length > 0)
            Room.Session.Sounds.Request(CrySound, channel: "objects");
    }

    private ActorVisual Visual(string name) => GetNodeOrNull<ActorVisual>(name)
        ?? throw new InvalidOperationException($"The egg has no visual named \"{name}\".");
}
