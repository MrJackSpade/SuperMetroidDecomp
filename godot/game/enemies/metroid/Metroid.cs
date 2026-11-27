using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Metroid: it homes in on Samus, clamps onto her head, and drains her energy for as long as it holds. Shots only
/// knock it back; ice freezes it once enough has landed, and only missiles hurt it while it is frozen. A bomb set off
/// while it holds her shakes it loose.
/// </summary>
/// <remarks>
/// Its timelines are "chasing", whose cue "cry" sounds one of its cries, and "draining"; "electricity" and "shell"
/// are for the visuals of those names. Its data lists its eight cries under "cries". Its reactions are what it takes
/// while frozen; unfrozen it takes nothing.
/// </remarks>
public partial class Metroid : Enemy
{
    /// <summary>How much ice damage freezes it.</summary>
    [Export]
    public int IceDamageToFreeze { get; set; }

    /// <summary>How far above Samus's middle it aims and clamps on, in pixels.</summary>
    [Export, DataValue("aim_above")]
    public int AimAbove { get; set; } = 8;

    /// <summary>How near that spot it must be, in pixels across and down, to clamp on.</summary>
    [Export, DataValue("latch_distance")]
    public int LatchDistance { get; set; } = 8;

    /// <summary>The fastest it flies, in pixels an update.</summary>
    [Export, DataValue("fastest_chase")]
    public int FastestChase { get; set; } = 3;

    /// <summary>How many updates it stays frozen.</summary>
    [Export, DataValue("frozen_updates")]
    public int FrozenUpdates { get; set; } = 400;

    /// <summary>The energy it drains each update it touches Samus, in hundredths of a point, before her suit lessens it.</summary>
    [Export, DataValue("drain_hundredths")]
    public int DrainHundredths { get; set; } = 75;

    /// <summary>How many updates pass between the sounds of it feeding.</summary>
    [Export, DataValue("feeding_sound_every")]
    public int FeedingSoundEvery { get; set; } = 8;

    /// <summary>The least energy Samus must have for its feeding to be heard.</summary>
    [Export, DataValue("feeding_sound_least_energy")]
    public int FeedingSoundLeastEnergy { get; set; } = 30;

    /// <summary>The sound of it feeding, by name.</summary>
    [Export, DataValue("feeding_sound")]
    public string FeedingSound { get; set; } = "";

    /// <summary>The sound of a shot knocking it back, by name.</summary>
    [Export, DataValue("recoil_sound")]
    public string RecoilSound { get; set; } = "";

    /// <summary>How many things it drops when it is destroyed, scattered about where it was, beside its death's own drop.</summary>
    [Export, DataValue("drop_count")]
    public int DropCount { get; set; } = 5;

    /// <summary>How many updates an ice shot that lands on it, unfrozen, makes it flash.</summary>
    [Export, DataValue("ice_flash_updates")]
    public int IceFlashUpdates { get; set; } = 4;

    /// <summary>How many updates it flashes for on each turn it is frozen while its count of escape moves runs on.</summary>
    [Export, DataValue("escape_flash_updates")]
    public int EscapeFlashUpdates { get; set; } = 2;

    private static readonly (int X, int Y)[] EscapeMoves = [(2, 0), (0, 2), (-2, 0), (0, -2)];

    private enum Moving
    {
        Homing,
        Closing,
        Attached,
        Escaping,
    }

    private readonly List<string> _cries = [];
    private Moving _moving;
    private int _escapeMovesLeft;
    private int _iceLeft;
    private TimelinePlayer? _electricity;
    private TimelinePlayer? _shell;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _cries.Clear();
        _cries.AddRange(data.List("cries").Select(cry => cry.AsText()));
        if (_cries.Count != 8)
            throw data.Problem("must list eight cries");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _iceLeft = IceDamageToFreeze;
        Play("chasing");
        _electricity = new TimelinePlayer(Timelines.Get("electricity"));
        _shell = new TimelinePlayer(Timelines.Get("shell"));
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "cry")
            Sound(_cries[Random.Draw().Part(0, 3)]);
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        // Unfrozen, nothing hurts it; what a shot does to it then is its own affair.
        Reaction reaction = base.ReactionTo(hit, part);
        return IsFrozen ? reaction : reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };
    }

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (IsFrozen)
            return;
        bool bomb = hit.Carries(new HitKind(HitKinds.Bomb)) || hit.Carries(new HitKind(HitKinds.PowerBomb));
        if (_moving == Moving.Attached)
        {
            if (!bomb)
                return;
            _escapeMovesLeft = EscapeMoves.Length;
            _moving = Moving.Escaping;
            (Room.Player as Samus)?.SetDrainFlash(false);
            Play("chasing", restart: true);
            return;
        }

        // It is thrown back from what holds the first of her shots' places, whatever struck it, a bomb as much as a
        // shot, and from the room's corner when nothing holds that place; harder the further off that is.
        PixelPosition from = ((Samus)Room.Player).FirstShotStands;
        Velocity = new PixelVelocity(
            Pixels.FromWhole(Position.X.Whole - from.X.Whole).Share(1, 8),
            Pixels.FromWhole(Position.Y.Whole - from.Y.Whole).Share(1, 8));
        _moving = Moving.Homing;
        Play("chasing", restart: true);
        if (hit.Carries(new HitKind(HitKinds.Ice)))
        {
            FlashFor(IceFlashUpdates);
            if (hit.Damage >= _iceLeft)
            {
                _iceLeft = 0;
                BeginStatus(Frozen, FrozenUpdates);
                return;
            }

            _iceLeft -= hit.Damage;
        }

        Sound(RecoilSound);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        Player samus = Room.Player;
        int aimY = samus.Position.Y.Whole - AimAbove;
        if (samus.BodyPowers.Any(power => power.IsActive))
        {
            // Samus's body, when it is a weapon, throws it off rather than hurting it.
            if (_moving == Moving.Attached)
                return;
            Velocity = new PixelVelocity(
                Pixels.FromWhole(Position.X.Whole - samus.Position.X.Whole).Share(1, 4),
                Pixels.FromWhole(Position.Y.Whole - aimY).Share(1, 4));
            _moving = Moving.Homing;
            Play("chasing", restart: true);
            return;
        }

        if (_moving != Moving.Escaping)
        {
            if (Room.Session.EnemyPasses % FeedingSoundEvery == FeedingSoundEvery - 1
                && samus.Inventory.Amount(Inventory.Energy) >= FeedingSoundLeastEnergy)
                Sound(FeedingSound);
            samus.AddOngoingDamage("metroid", DrainHundredths);
        }

        if (_moving is Moving.Attached or Moving.Escaping)
            return;
        _moving = Moving.Closing;
        if (Math.Abs(Position.X.Whole - samus.Position.X.Whole) >= LatchDistance || Math.Abs(Position.Y.Whole - aimY) >= LatchDistance)
            return;
        _moving = Moving.Attached;

        // Fastened on, it has her flash for as long as it holds her.
        (Room.Player as Samus)?.SetDrainFlash(true);
        Play("draining", restart: true);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        int aimX = Target.X.Whole, aimY = Target.Y.Whole - AimAbove;
        Pixels fastest = Pixels.FromWhole(FastestChase);
        switch (_moving)
        {
            case Moving.Homing:
                // It is pulled toward its aim, harder the further off it is.
                Fly(
                    Limited(Velocity.X - Pull(Position.X.Whole - aimX), fastest),
                    Limited(Velocity.Y - Pull(Position.Y.Whole - aimY), fastest));
                break;

            case Moving.Closing:
                Fly(Pixels.FromWhole(Closing(aimX, Position.X.Whole)), Pixels.FromWhole(Closing(aimY, Position.Y.Whole)));
                break;

            case Moving.Attached:
                // Its whole pixels are held on her; the part of a pixel it had stays.
                Position = new PixelPosition(Position.X.WithWhole(aimX), Position.Y.WithWhole(aimY));
                Velocity = PixelVelocity.Zero;
                break;

            case Moving.Escaping:
                (int x, int y) = EscapeMoves[^_escapeMovesLeft];
                Position += PixelOffset.FromWhole(x, y);
                Velocity = PixelVelocity.Zero;
                if (--_escapeMovesLeft == 0)
                {
                    _moving = Moving.Homing;
                    Play("chasing", restart: true);
                }

                break;

            default:
                throw new InvalidOperationException($"Undefined Metroid movement {_moving}.");
        }

        Step(_electricity, "Electricity");
        Step(_shell, "Shell");
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Frozen, it does not move, but a count of escape moves it was still making goes on, and on each turn it does it
    /// flashes; a count run out changes nothing more.
    /// </remarks>
    protected override void WhileFrozen()
    {
        if (_escapeMovesLeft == 0)
            return;
        _escapeMovesLeft--;
        FlashFor(EscapeFlashUpdates);
    }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        (Room.Player as Samus)?.SetDrainFlash(false);

        // Beside its death's own drop it leaves its full count more, scattered about, each placed by a fresh random number.
        for (int drop = 0; drop < DropCount; drop++)
        {
            RandomDraw draw = Random.Draw();
            Drop(Drops, PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + PixelOffset.FromWhole(draw.Part(0, 5) - 16, draw.Part(8, 5) - 16));
        }
    }

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        // Frozen, its electricity and shell stand still on their first pictures.
        if (status != Frozen)
            return;
        _electricity = Restarted(_electricity, "Electricity");
        _shell = Restarted(_shell, "Shell");
    }

    /// <summary>Flies one update at the given speeds, down and then across, each stopped dead by whatever it meets.</summary>
    private void Fly(Pixels across, Pixels down)
    {
        Velocity = new PixelVelocity(Velocity.X, down);
        if (Room.Mover.MoveDown(MovingBody(), down, Moves).Blocked)
            Velocity = Velocity.WithY(Pixels.Zero);
        Velocity = Velocity.WithX(across);
        if (Room.Mover.MoveAcross(MovingBody(), across, MovesAcross).Blocked)
            Velocity = Velocity.WithX(Pixels.Zero);
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Metroid \"{Name}\" has no body collider.");

    /// <summary>One of its outer visuals' timelines started again and shown from its first step.</summary>
    private TimelinePlayer? Restarted(TimelinePlayer? player, string visual)
    {
        if (player is null)
            return null;
        var restarted = new TimelinePlayer(player.Timeline);
        GetNodeOrNull<ActorVisual>(visual)?.Show(restarted.Shown);
        return restarted;
    }

    /// <summary>The pull toward its aim across a distance: a 1024th of the distance, counted in 256ths of a pixel.</summary>
    internal static Pixels Pull(int distance) => Pixels.FromWhole(distance / 4).Share(1, 256);

    /// <summary>A speed held within the fastest either way.</summary>
    internal static Pixels Limited(Pixels speed, Pixels fastest) => speed >= fastest ? fastest : speed < -fastest ? -fastest : speed;

    /// <summary>The whole-pixel speed it closes at along one axis: flat out toward its aim, the distance counted round a 256-pixel span.</summary>
    private int Closing(int aim, int at)
    {
        int apart = (((aim - at) % 256) + 256) % 256;
        if (apart >= 128)
            apart -= 256;
        return Math.Clamp(32 * apart, -FastestChase, FastestChase);
    }

    /// <summary>Moves one of its outer visuals on by one update.</summary>
    private void Step(TimelinePlayer? player, string visual)
    {
        if (player is null || GetNodeOrNull<ActorVisual>(visual) is not { } shown)
            return;
        player.Step();
        shown.Show(player.Shown);
    }
}
