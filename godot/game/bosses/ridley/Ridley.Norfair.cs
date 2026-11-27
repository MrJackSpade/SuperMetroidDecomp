using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Ridley's fight in its lair in Norfair. Once it has shown itself and the lava has risen it picks its attacks by what Samus
/// is doing: a long swoop across the room; bouncing on its tail like a pogo stick, breathing fire every other
/// bounce; sweeping from wall to wall when she spin jumps; or lunging to seize her, carry her up and drop her. A
/// power bomb sends it lunging at once, or out of the blast's way. The more it is hurt the harder it flies. With its
/// health gone it fights on until a lunge takes hold of her or ten have missed; then it roars, blows apart, and the
/// lava drains away.
/// </summary>
/// <remarks>
/// As well as what both fights share it plays "death_roar_left" and "death_roar_right". Its data lists
/// "arena_colours", the changes of colours of the room's own pictures: "hidden", shown from the moment it is set up,
/// and the steps of the "reveal", the first shown on the first update of its roar's hold and each for
/// "arena_reveal_step_updates"; the last stays. Its "liquid" says where the room's liquid heads as the room comes out
/// of the dark ("rise") and as its death roar ends ("drain"): each a "height" in pixels down from the room's top, a
/// "speed" in pixels an update and a "delay" in updates. It lists
/// "health_bands", lowest first, each for health "below" a number, with how hard it flies when it hovers
/// ("hover_divisor") and lunges ("grab_divisor"), the "colours" of its pictures once it has shown itself, and its
/// four "pogo" launches, each with its speed "across" and "up" and the speed it gains each update rising ("rise_gain") and falling
/// ("fall_gain"); the last band, with no "below", is for when it is unhurt. It also lists "death_explosions", the
/// places from its middle its death's blasts go off in turn, and "breakup", the pieces it breaks into: each an
/// "effect", where it starts ("x", "y", for when it faces left) or the link of its "tail" it was, and how many
/// "updates" it lasts; in an effect's name "{side}" stands for the way it faces and "{tip}" for which of its tail
/// tip's pictures shows. A piece that has lasted its time goes off as "piece_end_effect" and may leave one of
/// "piece_drops". "scattered_drops" is the table of what it scatters when it is gone, and "drop_scatter" where: a
/// corner of the room ("x", "y") and, across and down, how many binary digits of a number drawn are added and from
/// which digit up.
/// </remarks>
public partial class Ridley
{
    /// <summary>The music once it is dead, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>How many updates the room takes to come out of the dark as it roars.</summary>
    [Export, DataValue("arena_reveal_updates")]
    public int ArenaRevealUpdates { get; set; } = 46;

    /// <summary>How many updates each step of the room's coming out of the dark shows.</summary>
    [Export, DataValue("arena_reveal_step_updates")]
    public int ArenaRevealStepUpdates { get; set; } = 3;

    /// <summary>The name the arena's colours are changed under.</summary>
    private const string ArenaColours = "RidleyArena";

    /// <summary>
    /// Where the room's liquid heads as the room comes out of the dark, and as his death roar ends: the height, in
    /// pixels down from the room's top, how far it moves an update, and how many updates it waits first.
    /// </summary>
    private (int Height, double Speed, int Delay) _liquidRise, _liquidDrain;

    /// <summary>The arena's own pictures hidden in black, and each step of their coming out of the dark.</summary>
    private ColourChange _arenaHidden = ColourChange.None;
    private readonly List<ColourChange> _arenaReveal = [];

    /// <summary>Below this health it no longer waits for Samus to be low in the room to bounce.</summary>
    [Export, DataValue("wounded_health")]
    public int WoundedHealth { get; set; } = 14400;

    /// <summary>How many lunges it makes with its health gone before it dies without taking hold of her.</summary>
    [Export, DataValue("death_lunges")]
    public int DeathLunges { get; set; } = 10;

    /// <summary>The effect of dust where its tail strikes the floor, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of one of the blasts of its death.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>What a piece of it goes off as when it has lasted its time, by the data file of its timeline.</summary>
    [Export, DataValue("piece_end_effect")]
    public string PieceEndEffect { get; set; } = "";

    /// <summary>The sound of its tail striking the floor, by name.</summary>
    [Export, DataValue("bounce_sound")]
    public string BounceSound { get; set; } = "";

    /// <summary>The sound of one of the blasts of its death.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>How many things it drops when it is gone.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>How far left of the view's corner the window in which it meets Samus begins, in pixels.</summary>
    [Export, DataValue("touch_margin_across")]
    public int TouchMarginAcross { get; set; } = 32;

    /// <summary>How far above the view's corner that window begins, in pixels.</summary>
    [Export, DataValue("touch_margin_down")]
    public int TouchMarginDown { get; set; } = 32;

    /// <summary>How wide that window is, in pixels.</summary>
    [Export, DataValue("touch_window_width")]
    public int TouchWindowWidth { get; set; } = 320;

    /// <summary>How tall that window is, in pixels.</summary>
    [Export, DataValue("touch_window_height")]
    public int TouchWindowHeight { get; set; } = 288;

    private enum Phase
    {
        EnterArena,
        SelectAttack,
        HoverSetup,
        Hover,
        SwoopSetup,
        SwoopMoveToStart,
        SwoopAimDown,
        SwoopAimSideways,
        SwoopAimUp,
        SwoopClimb,
        SwoopRecover,
        WallSweepSetup,
        SweepToWallBehind,
        SweepToWallAhead,
        PogoMoveToSide,
        PogoMoveToHeight,
        PogoFall,
        PogoRise,
        GrabApproach,
        PowerBombDodge,
        CarrySetup,
        CarryMoveToAnchor,
        CarryRise,
        CarryRelease,
        MoveToDeathSpot,
        DeathRoar,
        DeathExplosions,
        DeathBreakup,
        DeathWait,
        DeathFinish,
        DeathComplete,
    }

    private enum Mode
    {
        NotFighting,
        Fighting,
        PowerBombDodge,
        Dying,
    }

    private sealed record Pogo(Pixels Across, Pixels Up, Pixels RiseGain, Pixels FallGain);

    private sealed record Band(int Below, int HoverDivisor, int GrabDivisor, ColourChange Colours, IReadOnlyList<Pogo> Pogos);

    private sealed record Piece(string Effect, int X, int Y, int TailLink, int Updates);

    private static readonly PixelPosition ArenaEntry = PixelPosition.FromWhole(64, 256);
    private static readonly PixelPosition DeathSpot = PixelPosition.FromWhole(128, 328);

    private readonly List<Band> _bands = [];
    private readonly List<PixelOffset> _deathExplosions = [];
    private readonly List<Piece> _breakup = [];
    private readonly List<(Effect Sprite, PixelVelocity Speed, int Left, int Moved)> _pieces = [];
    private Phase _phase;
    private Mode _mode;
    private Band _band = null!;
    private int _timer;
    private int _swoopTimer;
    private int _revealLeft;
    private bool _holdsSamus;
    private PixelOffset _grabOffset;
    private PixelPosition _target;
    private int _pogoBounces;
    private Facing _pogoWay;
    private Pixels _pogoRiseGain;
    private Pixels _pogoFallGain;
    private int _hurtLag;
    private int _deathLungesMade;
    private int _explosionWait;
    private int _nextExplosion;
    private DropTable _scatteredDrops = DropTable.Nothing;
    private DropTable _pieceDrops = DropTable.Nothing;
    private Vector2I _dropCorner;
    private (int From, int Digits) _dropAcross, _dropDown;

    /// <summary>Reads what only its fight in Norfair needs.</summary>
    private void ReadNorfairValues(DataNode data)
    {
        DataNode arena = data.Get("arena_colours");
        _arenaHidden = ColourChange.Read(arena.Get("hidden"));
        static (int, double, int) Motion(DataNode motion) => (motion.Int("height"), motion.Distance("speed").ToNumber(), motion.Int("delay"));
        (_liquidRise, _liquidDrain) = (Motion(data.Get("liquid").Get("rise")), Motion(data.Get("liquid").Get("drain")));
        _arenaReveal.Clear();
        _arenaReveal.AddRange(arena.List("reveal").Select(ColourChange.Read));
        _bands.Clear();
        foreach (DataNode band in data.List("health_bands"))
        {
            List<Pogo> pogos = [.. band.List("pogo").Select(pogo => new Pogo(pogo.Distance("across"), pogo.Distance("up"), pogo.Distance("rise_gain"), pogo.Distance("fall_gain")))];
            if (pogos.Count != 4)
                throw band.Problem("must list four pogo launches");
            _bands.Add(new Band(band.Has("below") ? band.Int("below") : int.MaxValue, band.Int("hover_divisor"), band.Int("grab_divisor"), ColourChange.Read(band.Get("colours")), pogos));
        }

        if (_bands.Count == 0 || _bands[^1].Below != int.MaxValue)
            throw data.Problem("must list its health bands, the last of them for when it is unhurt");
        _deathExplosions.Clear();
        _deathExplosions.AddRange(data.List("death_explosions").Select(at => PixelOffset.FromWhole(at.Int("x"), at.Int("y"))));
        _breakup.Clear();
        _breakup.AddRange(data.List("breakup").Select(piece => new Piece(
            piece.Text("effect"), piece.Has("x") ? piece.Int("x") : 0, piece.Has("y") ? piece.Int("y") : 0, piece.Has("tail") ? piece.Int("tail") : -1, piece.Int("updates"))));
        static (int From, int Digits) Field(DataNode field) => (field.Int("from"), field.Int("digits"));
        _scatteredDrops = DropTable.Read(data.Get("scattered_drops"));
        _pieceDrops = data.Has("piece_drops") ? DropTable.Read(data.Get("piece_drops")) : DropTable.Nothing;
        DataNode scatter = data.Get("drop_scatter");
        _dropCorner = new Vector2I(scatter.Int("x"), scatter.Int("y"));
        _dropAcross = Field(scatter.Get("across"));
        _dropDown = Field(scatter.Get("down"));
    }

    /// <summary>How many of the map's rows his arena takes, from his room's own cell down: the cartridge marks them explored as he begins.</summary>
    private const int ArenaMapRows = 2;

    /// <summary>Sets it up for its fight in Norfair.</summary>
    private void EnterNorfair()
    {
        // His arena is marked explored as he begins, since the map explores nothing while he is there.
        for (int row = 0; row < ArenaMapRows; row++)
        {
            Room.Session.Progress.MarkExplored(Room.Area, Room.MapCell.X, Room.MapCell.Y + row);
            Compatibility.KeptExploredMaps.Of(Room.Session).MarkLive(Room.MapCell.X, Room.MapCell.Y + row);
        }
        // In Norfair it stands behind Samus, where its data has it in front of her as aboard Ceres. The cartridge
        // draws it in front of the room's tiles as well, which one order of drawing cannot show together with its
        // standing behind her: here it is drawn among the room's things, behind her.
        InFrontOfTiles = false;
        Standing = Standing.Behind;

        // The capsule is only in its claws aboard Ceres.
        GetNodeOrNull<Engine.Actors.Components.ActorVisual>("Baby")?.SetShown(false);
        Position = PixelPosition.FromWhole(96, 394);
        Facing = Facing.Right;
        PlayFacing("standing");
        _band = _bands[^1];

        // Its arena is hidden in black from the moment it is set up, so through the door's passage too.
        Room.Recolour(ArenaColours, _arenaHidden);
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit) => OnCeres && base.DiesAtOnce(hit);

    /// <summary>Whether it may take off yet in Norfair: once the room has come out of the dark.</summary>
    private bool NorfairMayTakeOff()
    {
        // A step of the room's colours shows on the first update of the hold and on every few after it.
        int held = ArenaRevealUpdates - _revealLeft;
        if (held % ArenaRevealStepUpdates == 0 && held / ArenaRevealStepUpdates < _arenaReveal.Count)
            Room.Recolour(ArenaColours, _arenaReveal[held / ArenaRevealStepUpdates]);
        if (--_revealLeft > 0)
            return false;
        // The room's liquid rises as the room comes out of the dark.
        Room.Liquid?.HeadFor(_liquidRise.Height, _liquidRise.Speed, _liquidRise.Delay);
        return true;
    }

    /// <summary>One update of it in Norfair; <paramref name="hurt"/> says its hurt holds, which is not as long as its flash.</summary>
    private void BehaveInNorfair(bool hurt)
    {
        Player samus = Room.Player;
        // Hurt, it acts only every other update, and loses its grip on how it carries Samus.
        bool acts = !hurt || Updates % 2 == 0;
        _hurtLag = hurt ? Math.Min(8, _hurtLag + 1) : Math.Max(0, _hurtLag - 4);

        // Its pieces move before it acts, so those it breaks into this update first move on the next, as the
        // cartridge's enemies made in a turn first take theirs on the next update.
        StepPieces();
        if (acts)
        {
            UpdateIntangibility();
            if (_mode is Mode.Fighting && !_holdsSamus && PowerBombIsOut())
            {
                // A power bomb sends it straight at Samus.
                Tail.Action = RidleyTail.Doing.Neutral;
                Tail.SwingStep = RidleyTail.SlowSwing;
                _phase = Phase.GrabApproach;
            }

            if (HasLiftedOff)
                StepFight(samus);
            else
                StepReveal();
            if (IsInMotion)
                StepBody();
        }

        if (_holdsSamus && (!hurt || _hurtLag >= 8))
            CarrySamus(samus);
        _band = _bands.First(band => Health.Current < band.Below);
    }

    /// <summary>Whether a power bomb of Samus's lies waiting or is going off.</summary>
    private bool PowerBombIsOut() => PowerBomb() is not null;

    private Projectile? PowerBomb() => Room.Projectiles.FirstOrDefault(shot =>
        shot.Side == HitSide.Player && !shot.HasEnded && shot.Hit.Carries(new HitKind(HitKinds.PowerBomb)));

    /// <summary>Out of the window in which it meets Samus it cannot touch or be touched.</summary>
    private void UpdateIntangibility()
    {
        if (_mode == Mode.Dying)
            return;
        if (_mode != Mode.NotFighting)
            IsIntangible = !IsInTouchWindow();
    }

    /// <summary>
    /// Whether it lies in the window in which it meets Samus: from its margins left of and above the view's corner, as
    /// wide and as tall as the window, the far edges outside, and nothing above or left of the room.
    /// </summary>
    private bool IsInTouchWindow()
    {
        Camera camera = Room.Session.Camera;
        int x = Position.X.Whole, y = Position.Y.Whole;
        int across = x + TouchMarginAcross - camera.Left, down = y + TouchMarginDown - camera.Top;
        return x >= 0 && y >= 0 && across >= 0 && down >= 0 && across < TouchWindowWidth && down < TouchWindowHeight;
    }

    /// <summary>One update of its fight.</summary>
    private void StepFight(Player samus)
    {
        bool right = Facing == Facing.Right && !IsTurning;
        switch (_phase)
        {
            case Phase.EnterArena:
                FlyToward(ArenaEntry, 2);
                if (!IsNear(ArenaEntry, 8, 8))
                    break;
                _mode = Mode.Fighting;
                SelectAttack(samus);
                break;

            case Phase.SelectAttack:
                SelectAttack(samus);
                break;

            case Phase.HoverSetup:
                (_phase, _timer) = (Phase.Hover, 129);
                goto case Phase.Hover;

            case Phase.Hover:
                if (--_timer <= 0)
                {
                    _phase = Phase.SelectAttack;
                    break;
                }

                PixelPosition hover = PixelPosition.FromWhole(Across(192, 96, 96), 256);
                FlyToward(hover, _band.HoverDivisor);
                if (IsNear(hover, 8, 8))
                    _phase = Phase.SelectAttack;
                break;

            case Phase.SwoopSetup:
                (_phase, SwoopHeading) = (Phase.SwoopMoveToStart, 0);
                goto case Phase.SwoopMoveToStart;

            case Phase.SwoopMoveToStart:
                PixelPosition start = PixelPosition.FromWhole(Across(192, 64, 64), 128);
                FlyToward(start, 15);
                if (!IsNear(start, 8, 8))
                    break;
                (_phase, _swoopTimer, SwoopHeading) = (Phase.SwoopAimDown, 33, 0);
                break;

            case Phase.SwoopAimDown:
                Swoop(32, 512, 4.5, Phase.SwoopAimSideways, 21);
                break;

            case Phase.SwoopAimSideways:
                if (Swoop(320, 64 * 256, 5, Phase.SwoopAimUp, 17))
                    Tail.WhipOrdered = true;
                break;

            case Phase.SwoopAimUp:
                Swoop(512, 120 * 256, 3, Phase.SwoopClimb, 33);
                break;

            case Phase.SwoopClimb:
                if (Swoop(1024, 120 * 256, 3, Phase.SwoopRecover, 33))
                    TurnToFaceRoom();
                break;

            case Phase.SwoopRecover:
                SetSwoopSpeed(0, false, 128 * 256, Pixels.FromNumber(1.75));
                if (--_swoopTimer > 0)
                    break;
                _phase = CanSeize(samus) ? Phase.GrabApproach : Phase.SelectAttack;
                break;

            case Phase.WallSweepSetup:
                Tail.LinkSpacing = 11;
                Tail.ExtensionSpeed = Pixels.FromNumber(1.5);
                _phase = Phase.SweepToWallBehind;
                _timer = Random.Last.Part(0, 5) + 33;
                goto case Phase.SweepToWallBehind;

            case Phase.SweepToWallBehind:
            case Phase.SweepToWallAhead:
                bool behind = _phase == Phase.SweepToWallBehind;
                FlyToward(PixelPosition.FromWhole(behind ? Across(192, 128, 64) : Across(64, 128, 192), Math.Min(samus.Position.Y.Whole, 352)), _band.HoverDivisor);
                Tail.WhipOrdered = true;
                if (samus.State is not SpinJumpingState)
                {
                    BeginPogo();
                    break;
                }

                if (Random.Last.Part(0, 8) >= 128 && !IsRoaring && !IsTurning)
                    PlayFacing("fireballing");
                if (--_timer > 0)
                    break;
                _phase = behind ? Phase.SweepToWallAhead : Phase.SweepToWallBehind;
                _timer = 129;
                TurnToFaceRoom();
                break;

            case Phase.PogoMoveToSide:
                if (Position.Y.Whole < 288)
                {
                    TurnToFaceRoom();
                    (_phase, _timer) = (Phase.PogoMoveToHeight, 33);
                    goto case Phase.PogoMoveToHeight;
                }

                FlyToward(PixelPosition.FromWhole(Across(176, 128, 96), 288), 16);
                break;

            case Phase.PogoMoveToHeight:
                FlyToward(PixelPosition.FromWhole(Position.X.Whole, 288), 16);
                if (--_timer > 0)
                    break;
                Tail.Action = RidleyTail.Doing.PogoSetup;
                StepPogoTail();
                _pogoWay = Velocity.X.IsNegative ? Facing.Left : Facing.Right;
                LaunchPogo();
                _phase = Phase.PogoFall;
                _timer = Random.Last.Part(0, 6) + 129;
                break;

            case Phase.PogoFall:
                if (CanSeize(samus) && ClawsReach(samus, 4, 4))
                {
                    // Samus under its claws as it comes down is snatched up.
                    Pixels lift = Velocity.Y.Abs() > Pixels.FromWhole(2) ? Velocity.Y.Abs() : Pixels.FromWhole(2);
                    Velocity = Velocity.WithY(-lift);
                    Tail.Action = RidleyTail.Doing.Neutral;
                    Tail.SwingStep = RidleyTail.SlowSwing;
                    Seize(samus);
                    break;
                }

                Pixels fall = Velocity.Y + _pogoFallGain;
                Velocity = Velocity.WithY(fall > Pixels.FromWhole(6) ? Pixels.FromWhole(6) : fall);
                if (!TailTouchesTerrain())
                    break;
                if (DustEffect.Length > 0)
                    LeaveHoldingPlace(DustEffect, Tail.Tip.Position + PixelOffset.FromWhole(0, 12));
                Sound(BounceSound);
                Room.Shake(2, 4, ShakeWay.Down);
                ChoosePogoWay(samus);
                LaunchPogo();
                foreach (RidleyTail.Link link in Tail.Links)
                {
                    link.Distance = link.Rest;
                    link.TargetDistance = Pixels.FromWhole(12);
                }

                Tail.Action = RidleyTail.Doing.Bouncing;
                if (++_pogoBounces >= 2)
                {
                    _pogoBounces = 0;
                    if (!IsTurning)
                        PlayFacing("fireballing");
                }

                _phase = Phase.PogoRise;
                break;

            case Phase.PogoRise:
                if (samus.Position.Y.Whole < 352 || --_timer <= 0)
                {
                    Tail.Action = RidleyTail.Doing.Neutral;
                    Tail.SwingStep = RidleyTail.SlowSwing;
                    _phase = Phase.SelectAttack;
                    break;
                }

                Pixels rise = Velocity.Y + _pogoRiseGain;
                if (rise.IsNegative)
                {
                    Velocity = Velocity.WithY(rise);
                    break;
                }

                Velocity = Velocity.WithY(Pixels.Zero);
                _phase = Phase.PogoFall;
                break;

            case Phase.GrabApproach:
                if (!CanSeize(samus))
                {
                    MissLunge();
                    break;
                }

                int past = Position.X.Whole - samus.Position.X.Whole;
                bool passed = right ? past >= 0 : past < 0;
                if (HitBounds || (passed && Math.Abs(past) >= 32) || Position.Y.Whole + 35 >= samus.Position.Y.Whole)
                {
                    MissLunge();
                    break;
                }

                FlyToward(samus.Position + PixelOffset.FromWhole(Facing == Facing.Left && !IsTurning ? -16 : 16, -4), _band.GrabDivisor);
                if (!ClawsReach(samus, 8, 12))
                    break;
                Velocity = Velocity.WithY(-Velocity.Y);
                Seize(samus);
                break;

            case Phase.PowerBombDodge:
                Dodge();
                break;

            case Phase.CarrySetup:
                BeginCarry();
                break;

            case Phase.CarryMoveToAnchor:
                FlyToward(_target, 16);
                if (--_timer <= 0)
                    (_phase, _timer) = (Phase.CarryRise, 33);
                break;

            case Phase.CarryRise:
                if (--_timer > 0)
                {
                    FlyToward(new PixelPosition(_target.X, Pixels.FromWhole(256)), 16);
                    break;
                }

                Tail.LinkSpacing = 8;
                Tail.ExtensionSpeed = Pixels.FromNumber(0.9375);
                LetSamusGo(samus);
                (_phase, _timer) = (Phase.CarryRelease, 65);
                break;

            case Phase.CarryRelease:
                if (--_timer > 0)
                {
                    FlyToward(PixelPosition.FromWhole(Across(176, 0, 80), 224), 16);
                    break;
                }

                Tail.LinkSpacing = 16;
                Tail.ExtensionSpeed = Pixels.FromNumber(0.9375);
                _phase = Phase.SelectAttack;
                break;

            case Phase.MoveToDeathSpot:
                StepToDeathSpot();
                break;

            case Phase.DeathRoar:
                FlyToward(DeathSpot, 16, Pixels.FromNumber(0.0625));
                if (--_timer > 0)
                    break;
                Velocity = PixelVelocity.Zero;
                Room.Liquid?.HeadFor(_liquidDrain.Height, _liquidDrain.Speed, _liquidDrain.Delay);
                (_explosionWait, _nextExplosion) = (1, 0);
                (_phase, _timer) = (Phase.DeathExplosions, 161);
                goto case Phase.DeathExplosions;

            case Phase.DeathExplosions:
                Explode();
                if (--_timer > 0)
                    break;
                if (_holdsSamus)
                    LetSamusGo(samus);
                _phase = Phase.DeathBreakup;
                Velocity = PixelVelocity.Zero;
                BreakUp();
                break;

            case Phase.DeathBreakup:
                Explode();
                IsInMotion = false;
                Visible = false;
                (_phase, _timer) = (Phase.DeathWait, 33);
                break;

            case Phase.DeathWait:
                if (--_timer <= 0)
                    (_phase, _timer) = (Phase.DeathFinish, 257);
                break;

            case Phase.DeathFinish:
                if (--_timer > 0)
                    break;
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(_scatteredDrops, PixelPosition.FromWhole(
                        _dropCorner.X + draw.Part(_dropAcross.From, _dropAcross.Digits), _dropCorner.Y + draw.Part(_dropDown.From, _dropDown.Digits)));
                }

                if (VictoryMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(VictoryMusic);
                _phase = Phase.DeathComplete;
                Die();
                break;

            case Phase.DeathComplete:
                break;

            default:
                throw new InvalidOperationException($"Ridley has no phase {_phase}.");
        }
    }

    /// <summary>Picks its next attack by what Samus is doing, and begins it at once.</summary>
    private void SelectAttack(Player samus)
    {
        int roll = Random.Draw().Part(0, 3);
        if (samus.State is SpinJumpingState)
        {
            _phase = Phase.WallSweepSetup;
        }
        else if (Health.IsEmpty)
        {
            _deathLungesMade++;
            _phase = Phase.GrabApproach;
        }
        else if (Health.Current < WoundedHealth)
        {
            _phase = roll < 4 ? Phase.PogoMoveToSide : Phase.SwoopSetup;
        }
        else if (samus.Position.Y.Whole >= 352)
        {
            _phase = Phase.PogoMoveToSide;
        }
        else if (CanSeize(samus))
        {
            _phase = roll < 6 ? Phase.GrabApproach : Phase.PogoMoveToSide;
        }
        else
        {
            _phase = roll < 4 ? Phase.PogoMoveToSide : Phase.SwoopSetup;
        }

        StepFight(samus);
    }

    /// <summary>A place across the room that depends on the way it faces: left, turning, or right.</summary>
    private int Across(int facingLeft, int turning, int facingRight) => IsTurning ? turning : Facing == Facing.Left ? facingLeft : facingRight;

    /// <summary>
    /// One update of a leg of its swoop: its heading turns, by 256ths of a step, toward an angle given for when it faces
    /// right, and its speed moves toward a speed. Returns whether the leg is over.
    /// </summary>
    private bool Swoop(int turn, int headingFacingRight, double speed, Phase next, int nextUpdates)
    {
        bool right = Facing != Facing.Left || IsTurning;
        SetSwoopSpeed(turn, !right, right ? headingFacingRight : -headingFacingRight, Pixels.FromNumber(speed));
        if (--_swoopTimer > 0)
            return false;
        (_phase, _swoopTimer) = (next, nextUpdates);
        return true;
    }

    /// <summary>Turns about, if it has its back to the middle of the room.</summary>
    private void TurnToFaceRoom()
    {
        if (IsTurning)
            return;
        bool rightHalf = Position.X.Whole >= 128;
        if (Facing == Facing.Left ? rightHalf : !rightHalf)
            return;
        Play(Facing == Facing.Left ? "turn_left_to_right" : "turn_right_to_left", restart: true);
    }

    /// <summary>Begins its bouncing on its tail.</summary>
    private void BeginPogo()
    {
        Tail.ExtensionSpeed = Pixels.FromNumber(0.9375);
        Tail.LinkSpacing = 16;
        Tail.Action = RidleyTail.Doing.Neutral;
        _phase = Phase.PogoMoveToSide;
    }

    /// <summary>Throws it up from a bounce, by one of the four launches its health allows.</summary>
    private void LaunchPogo()
    {
        Pogo pogo = _band.Pogos[Random.Last.Part(0, 2)];
        (_pogoRiseGain, _pogoFallGain) = (pogo.RiseGain, pogo.FallGain);
        Velocity = new PixelVelocity(_pogoWay.Toward(pogo.Across), -pogo.Up);
    }

    /// <summary>Picks the way its next bounce goes: back from an edge, and otherwise nearly always toward Samus.</summary>
    private void ChoosePogoWay(Player samus)
    {
        int x = Position.X.Whole;
        if (Velocity.X.IsZero)
            _pogoWay = x < 128 ? Facing.Right : Facing.Left;
        bool reverse;
        if (x < FlightBounds.Position.X)
        {
            reverse = _pogoWay == Facing.Left;
        }
        else if (x >= FlightBounds.End.X)
        {
            reverse = _pogoWay == Facing.Right;
        }
        else
        {
            Facing towardSamus = x < samus.Position.X.Whole ? Facing.Right : Facing.Left;
            bool away = (Random.Last.Number & 0xFFFF) < 1365;
            reverse = _pogoWay == towardSamus ? away : !away;
        }

        if (reverse)
            _pogoWay = _pogoWay.Opposite();
    }

    /// <summary>The tail's part in the pogo: it swings round to point down, bounces, draws up and stabs.</summary>
    private void StepPogoTail()
    {
        bool right = Facing == Facing.Right && !IsTurning;
        Tail.SetLimits(right);
        RidleyTail.Doing doing = Tail.Action;
        if (doing is RidleyTail.Doing.PogoSetup or RidleyTail.Doing.PointingDown or RidleyTail.Doing.Stabbing)
        {
            Tail.SwingStep = doing == RidleyTail.Doing.Stabbing ? RidleyTail.StabSwing : RidleyTail.WhipSwing;
            Tail.LeftwardTarget = right ? 0 : null;
            Tail.RightwardTarget = right ? null : 0;
        }

        bool restsFalling = false;
        if (Tail.IsAllMoving && Tail.LeftwardTarget is null && Tail.RightwardTarget is null &&
            (Random.Last.Part(0, 8) >= 240 || Math.Abs(Room.Player.Position.X.Whole - Position.X.Whole) < 128))
        {
            // It lashes a whole turn round, to come down pointing at the floor.
            Tail.LeftwardTarget = -Turns.PerTurn;
            Tail.SwingStep = RidleyTail.WhipSwing;
        }
        else if (Tail.IsAtRest)
        {
            restsFalling = !Velocity.Y.IsNegative;
            if (restsFalling && doing == RidleyTail.Doing.ReadyingStab)
            {
                foreach (RidleyTail.Link link in Tail.Links)
                {
                    link.TargetDistance = Pixels.FromWhole(10);
                    link.Angle = 0;
                }
            }

            Tail.Base.Activate();
            (Tail.LeftwardTarget, Tail.RightwardTarget) = (null, null);
            Tail.SwingStep = 0;
        }

        Tail.SwingLinks();
        bool moving = !Tail.IsAtRest;
        Tail.Action = doing switch
        {
            RidleyTail.Doing.PogoSetup => RidleyTail.Doing.PointingDown,
            RidleyTail.Doing.PointingDown => moving ? RidleyTail.Doing.PointingDown : RidleyTail.Doing.Bouncing,
            RidleyTail.Doing.Bouncing => restsFalling ? RidleyTail.Doing.ReadyingStab : RidleyTail.Doing.Bouncing,
            RidleyTail.Doing.ReadyingStab => restsFalling ? RidleyTail.Doing.Stabbing : RidleyTail.Doing.ReadyingStab,
            RidleyTail.Doing.Stabbing => RidleyTail.Doing.Stabbing,
            _ => throw new InvalidOperationException($"Ridley's tail has no part in the pogo called {doing}."),
        };
    }

    /// <summary>Whether the end of its tail has reached the floor.</summary>
    private bool TailTouchesTerrain()
    {
        if (Probe(Tail.Tip.Position + PixelOffset.FromWhole(0, 16)))
            return true;
        return Tail.Links.Skip(1).Take(Tail.Links.Count - 2).Reverse().Take(4).Any(link => Probe(link.Position + PixelOffset.FromWhole(0, 18)));
    }

    private bool Probe(PixelPosition at) => Room.Terrain.IsSolidIn(new PixelRect(at.X.Whole, at.Y.Whole, 1, 1), SlopeTreatment.Wall);

    /// <summary>
    /// Whether Samus is doing anything it can take hold of her in: not spinning, on the grapple beam (swinging, held
    /// in place by it or against a wall by it), damage boosting, or seized by Draygon.
    /// </summary>
    private static bool CanSeize(Player samus) =>
        samus.State is not (SpinJumpingState or GrapplingState or GrappleLockedState or GrappleWallGrabState or DamageBoostState or GrabbedState);

    /// <summary>Whether its claws are within a reach of Samus's body.</summary>
    private bool ClawsReach(Player samus, int across, int down)
    {
        PixelOffset fromClaw = samus.Position - ClawPosition;
        PixelRect body = samus.BodyRect;
        return Math.Abs(fromClaw.X.Whole) < body.Width / 2 + across && Math.Abs(fromClaw.Y.Whole) < body.Height / 2 + down;
    }

    /// <summary>Takes hold of Samus. With its health gone that is the end of it; otherwise it carries her off.</summary>
    private void Seize(Player samus)
    {
        PixelOffset fromClaw = samus.Position - ClawPosition;
        _grabOffset = PixelOffset.FromWhole(fromClaw.X.Whole, fromClaw.Y.Whole);
        _holdsSamus = true;
        // In its claws she is held as she is: her pose and speed kept, neither moving, animating nor acting.
        samus.HoldAsTheyAre();
        if (Health.IsEmpty)
        {
            BeginDying();
            StepToDeathSpot();
            return;
        }

        BeginCarry();
    }

    /// <summary>Makes for the middle of the room, where it dies.</summary>
    private void StepToDeathSpot()
    {
        FlyToward(DeathSpot, 16, Pixels.FromNumber(0.0625));
        if (IsNear(DeathSpot, 4, 4))
            BeginDeathRoar();
    }

    /// <summary>Begins carrying Samus up toward a side of the room.</summary>
    private void BeginCarry()
    {
        _target = PixelPosition.FromWhole(Across(64, 0, 208), Position.Y.Whole < 320 ? 256 : Position.Y.Whole - 64);
        (_phase, _timer) = (Phase.CarryMoveToAnchor, 33);
        FlyToward(_target, 16);
        _timer--;
    }

    /// <summary>Keeps Samus in its claws, drawing her in to them a little each update.</summary>
    private void CarrySamus(Player samus)
    {
        _grabOffset = PixelOffset.FromWhole(TowardNothing(_grabOffset.X.Whole), TowardNothing(_grabOffset.Y.Whole));
        samus.Position = ClawPosition + _grabOffset;

        static int TowardNothing(int offset) => offset > 0 ? Math.Max(0, offset - 4) : Math.Min(0, offset + 4);
    }

    /// <summary>Lets Samus go, with a lash of its tail after her.</summary>
    private void LetSamusGo(Player samus)
    {
        _holdsSamus = false;
        Tail.WhipOrdered = true;
        Tail.Action = RidleyTail.Doing.Neutral;
        if (samus.State is HeldState)
            samus.Release();
    }

    /// <summary>A lunge that did not take hold: with its health gone and its lunges spent it dies; otherwise it hovers, or gets out of a power bomb's way.</summary>
    private void MissLunge()
    {
        if (_deathLungesMade >= DeathLunges)
        {
            BeginDying();
            BeginDeathRoar();
            return;
        }

        if (PowerBombIsOut())
        {
            _phase = Phase.PowerBombDodge;
            Dodge();
            return;
        }

        _phase = Phase.HoverSetup;
        Tail.WhipOrdered = true;
    }

    /// <summary>Keeps to the corner of the room farthest from a power bomb until it is over.</summary>
    private void Dodge()
    {
        if (PowerBomb() is not { } bomb)
        {
            _mode = Mode.Fighting;
            _phase = _holdsSamus ? Phase.CarrySetup : Phase.SelectAttack;
            return;
        }

        _mode = Mode.PowerBombDodge;
        FlyToward(PixelPosition.FromWhole(bomb.Position.X.Whole >= 128 ? 80 : 192, bomb.Position.Y.Whole >= 256 ? 192 : 384), _band.HoverDivisor);
    }

    /// <summary>Begins its death, if its health is gone: it makes for the middle of the room.</summary>
    private void BeginDying()
    {
        if (!Health.IsEmpty || _mode == Mode.Dying)
            return;
        _mode = Mode.Dying;
        IsIntangible = true;
        _phase = Phase.MoveToDeathSpot;
    }

    private void BeginDeathRoar()
    {
        PlayFacing("death_roar");
        (_phase, _timer) = (Phase.DeathRoar, 33);
    }

    /// <summary>Sets off the blasts of its death, one every few updates, at places that come round in turn.</summary>
    private void Explode()
    {
        if (_explosionWait > 0 && --_explosionWait > 0)
            return;
        _explosionWait = 5;
        if (_deathExplosions.Count == 0)
            return;
        PixelOffset at = _deathExplosions[_nextExplosion];
        _nextExplosion = (_nextExplosion + 1) % _deathExplosions.Count;
        if (ExplosionEffect.Length > 0)
            LeaveHoldingPlace(ExplosionEffect, Position + at);
        Sound(ExplosionSound);
    }

    /// <summary>
    /// Leaves dust or a blast: on the cartridge each is one of the enemies' shots, which holds one of the room's places
    /// while it plays and is not made with none free.
    /// </summary>
    private void LeaveHoldingPlace(string effect, PixelPosition at) =>
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace: true));

    /// <summary>
    /// Breaks it into its pieces, each flung a little to one side. Caught turning to face out of the screen, it breaks
    /// up as though it faced right, as on the cartridge.
    /// </summary>
    private void BreakUp()
    {
        bool right = Facing == Facing.Right || IsTurning;
        foreach (Piece piece in _breakup)
        {
            // From the number drawn: none up to three sixteenths of a pixel from its fifth and sixth places, a whole
            // pixel or none from its ninth, and left or right from its highest.
            RandomDraw draw = Random.Draw();
            Pixels across = Pixels.FromNumber(draw.Part(4, 2) / 16.0 + draw.Part(8, 1));
            PixelPosition at = piece.TailLink >= 0
                ? Tail.Links[piece.TailLink].Position
                : Position + PixelOffset.FromWhole(right ? -piece.X : piece.X, piece.Y);
            // A piece starts on the whole pixel it is made at.
            Effect sprite = LeaveEffect(
                piece.Effect.Replace("{side}", right ? "right" : "left", StringComparison.Ordinal)
                    .Replace("{tip}", TailTipPicture.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal),
                PixelPosition.FromWhole(at.X.Whole, at.Y.Whole));
            // It is not seen until its first move, on the next update.
            sprite.Visible = false;
            _pieces.Add((sprite, new PixelVelocity(draw.Part(15, 1) == 1 ? -across : across, Pixels.Zero), piece.Updates, 0));
        }
    }

    /// <summary>
    /// Moves its pieces: they slow across, fall, show on their first move and every other one after, and each goes
    /// off as it makes its last.
    /// </summary>
    private void StepPieces()
    {
        Pixels drag = Pixels.FromNumber(0.015625);
        for (int index = _pieces.Count - 1; index >= 0; index--)
        {
            (Effect sprite, PixelVelocity speed, int left, int moved) = _pieces[index];
            sprite.Visible = moved % 2 == 0;
            Pixels across = speed.X.Abs() <= drag ? Pixels.Zero : speed.X.IsNegative ? speed.X + drag : speed.X - drag;
            speed = new PixelVelocity(across, speed.Y + drag);
            sprite.Position += speed.OverOneUpdate;
            if (--left > 0)
            {
                _pieces[index] = (sprite, speed, left, moved + 1);
                continue;
            }

            // It goes off as a small enemy does, and may leave something.
            Room.Spawn(Remains.Of(FrameTimeline.Read(Room.Content.ReadData(PieceEndEffect)), sprite.Position, _pieceDrops));
            Room.Remove(sprite);
            _pieces.RemoveAt(index);
        }
    }
}
