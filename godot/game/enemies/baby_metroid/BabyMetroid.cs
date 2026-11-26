using Engine.Actors;
using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The giant Metroid of Tourian. When Samus comes into its room the room closes behind her. It falls on a
/// Sidehopper and drains it, then turns on her: it hovers, then chases, clamps on and drains her nearly to
/// nothing, slowing her as it does. Then it knows her. It lets go, hovers at her shoulders, stays near her a
/// while as she gets up, and flies off. Shots and a spinning Samus only push it about.
/// </summary>
/// <remarks>
/// Its timelines are "normal", "latched_on", "finish_draining" and "remorse", whose cue "maybe_cry" starts it again
/// as often as not and otherwise sounds its cry. A picture's path holds "{shade}" where the name of a set of its
/// pictures goes: there is a set for each beat of its membrane, named with an underscore and the beat's number from
/// nought. Its data names the terrain patches that close and open its room.
/// </remarks>
public partial class BabyMetroid : Enemy
{
    /// <summary>A plasma beam stops on it, as the cartridge's does: it is one of the few a beam does not pass through.</summary>
    public override bool Stops(Hit hit) => hit.Carries(new HitKind(Game.Data.HitKinds.Beam));

    /// <summary>The Sidehopper it feeds on.</summary>
    [Export]
    public NodePath Victim { get; set; } = new();

    /// <summary>What is told "close" as its room closes and "open" as it opens again.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldOfRoom { get; set; } = [];

    /// <summary>The music of its attack, by name.</summary>
    [Export]
    public string AttackMusic { get; set; } = "";

    /// <summary>The music once it has let Samus go, by name.</summary>
    [Export]
    public string AfterwardMusic { get; set; } = "";

    /// <summary>
    /// How far across the room its arena starts, in pixels; it wakes once the view's left edge is at or left of this,
    /// and puts the view's left edge there as it does.
    /// </summary>
    [Export]
    public int ArenaLeft { get; set; } = 512;

    /// <summary>The terrain patch that walls its arena in.</summary>
    [Export, DataValue("walls_up_patch")]
    public string WallsUpPatch { get; set; } = "";

    /// <summary>The terrain patch that opens its arena again.</summary>
    [Export, DataValue("walls_down_patch")]
    public string WallsDownPatch { get; set; } = "";

    /// <summary>Its cry while it is calm.</summary>
    [Export, DataValue("cry_sound")]
    public string CrySound { get; set; } = "";

    /// <summary>Its cry while it feeds.</summary>
    [Export, DataValue("feeding_cry_sound")]
    public string FeedingCrySound { get; set; } = "";

    /// <summary>Its cry as it flies from Samus's back.</summary>
    [Export, DataValue("fly_away_cry_sound")]
    public string FlyAwayCrySound { get; set; } = "";

    /// <summary>Its cry of remorse.</summary>
    [Export, DataValue("remorse_cry_sound")]
    public string RemorseCrySound { get; set; } = "";

    /// <summary>How many updates each beat of its membrane lasts while it is calm.</summary>
    [Export, DataValue("calm_pulse_updates")]
    public int CalmPulseUpdates { get; set; } = 11;

    /// <summary>How many updates each beat of its membrane lasts while it feeds.</summary>
    [Export, DataValue("feeding_pulse_updates")]
    public int FeedingPulseUpdates { get; set; } = 2;

    /// <summary>How many beats its membrane goes round.</summary>
    [Export, DataValue("pulse_beats")]
    public int PulseBeats { get; set; } = 8;

    /// <summary>The beat, counted from nought, on which it may cry.</summary>
    [Export, DataValue("cry_beat")]
    public int CryBeat { get; set; } = 5;

    /// <summary>How many times its membrane comes to that beat for each cry.</summary>
    [Export, DataValue("beats_a_cry")]
    public int BeatsACry { get; set; } = 4;

    private enum Doing
    {
        Dormant,
        WaitForView,
        BeginEntranceDelay,
        EntranceDelay,
        FlyToSidehopper,
        ChaseSidehopper,
        AttachToSidehopper,
        DrainSidehopper,
        ActivateSidehopperCorpse,
        RiseAfterFeeding,
        HoverNearSamus,
        ChaseSamus,
        BeginDrainingSamus,
        DrainSamus,
        BeginPostDrainPause,
        PostDrainPause,
        RiseAfterDrainingSamus,
        FlyBesideSamus,
        BeginExit,
        Exit,
        HoldSamusBeforeRelease,
        ReleasedFollow,
    }

    private static readonly PixelOffset[] ShakeOffsets = [PixelOffset.FromWhole(0, 0), PixelOffset.FromWhole(-1, 1), PixelOffset.FromWhole(0, -1), PixelOffset.FromWhole(1, 1)];
    private static readonly PixelOffset OnItsPrey = PixelOffset.FromWhole(0, -32);
    private static readonly Pixels Kick = Pixels.FromWhole(4);

    private Doing _doing;
    private int _timer;
    private bool _reactsToSamus;
    private bool _cries;
    private bool _feeding;
    private int _pulseWait;
    private int _pulseBeat;
    private int _criesWait;
    private int _lock;
    private int _hover;
    private bool _besideLeft;
    private int _sidehopperDrain;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (!_reactsToSamus)
            return HitResult.Absorbed;
        StopFollowing();
        // Whatever hit it, a bomb as much as a shot, it is knocked away from what holds the first of Samus's shots'
        // places, and from the room's corner when nothing holds it; harder the more the hit would have hurt.
        double strength = Math.Min(hit.Damage / 32.0, 15 / 16.0);
        PushAwayFrom(((Samus)Room.Player).FirstShotStands, Pixels.FromNumber(strength));
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_reactsToSamus)
            return;
        Player samus = Room.Player;
        StopFollowing();
        if (samus.State is SpinJumpingState && samus.Position.X.Whole >= ArenaLeft)
        {
            PushAwayFrom(samus.Position, Pixels.FromNumber(0.25));
            return;
        }

        if (_doing == Doing.ChaseSamus)
        {
            if (!CloseOn(samus.Position + OnItsPrey))
                return;
            Play("latched_on", restart: true);
            (_feeding, _reactsToSamus) = (true, false);
            Velocity = PixelVelocity.Zero;
            _doing = Doing.BeginDrainingSamus;
        }
        else if (_doing == Doing.HoverNearSamus)
        {
            _doing = Doing.ChaseSamus;
        }
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Velocity = PixelVelocity.Zero;
        (_doing, _reactsToSamus, _cries, _feeding) = (Doing.WaitForView, false, false, false);
        (_pulseWait, _pulseBeat, _criesWait, _lock, _hover) = (1, 0, BeatsACry, 0, 0);
        Play("normal");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue != "maybe_cry")
            throw new InvalidOperationException($"The giant Metroid's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        if (Random.Last.Part(15, 1) == 0)
            Play("remorse", restart: true);
        else
            Sound(RemorseCrySound);
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step) => step.Picture.Replace("{shade}", $"_{_pulseBeat}", StringComparison.Ordinal);

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        while (Step(samus))
        {
        }

        if (IsDead)
            return;
        Position += Velocity.OverOneUpdate;
        StepMembrane();
    }

    /// <summary>Plays one step of what it is doing. Returns whether the next step is to be played at once.</summary>
    private bool Step(Player samus)
    {
        PixelOffset shake = ShakeOffsets[Updates / 2 % 4];
        switch (_doing)
        {
            case Doing.Dormant:
                Velocity = PixelVelocity.Zero;
                return false;

            case Doing.WaitForView:
                if (Room.Session.Camera.View.Left > ArenaLeft)
                    return false;
                // The old one writes the view's left edge to its arena's edge as it wakes, keeping its fraction.
                Room.Session.Camera.MoveLeftEdgeTo(ArenaLeft);
                TellRoom("close");
                Room.Terrain.ApplyPatch(WallsUpPatch);
                (_doing, _cries) = (Doing.BeginEntranceDelay, true);
                return false;

            case Doing.BeginEntranceDelay:
                (_doing, _timer) = (Doing.EntranceDelay, 465);
                return true;

            case Doing.EntranceDelay:
                if (--_timer > 0)
                    return false;
                if (AttackMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(AttackMusic);
                _doing = Doing.FlyToSidehopper;
                return true;

            case Doing.FlyToSidehopper:
                var rendezvous = PixelPosition.FromWhole(584, 74);
                FlyToward(rendezvous, 1);
                if (IsOver(rendezvous, 1, 1))
                    _doing = Doing.ChaseSidehopper;
                return false;

            case Doing.ChaseSidehopper:
                DeadSidehopper prey = Prey();
                FlyToward(prey.Position + OnItsPrey, 1);
                PixelRect preyBody = BodyOf(prey);
                if (IsOver(prey.Position, preyBody.Width / 2, preyBody.Height / 2))
                    _doing = Doing.AttachToSidehopper;
                return false;

            case Doing.AttachToSidehopper:
                PixelPosition latch = Prey().Position + OnItsPrey;
                if (!CloseOn(latch))
                    return false;
                Velocity = PixelVelocity.Zero;
                Position = latch;
                Play("latched_on", restart: true);
                (_doing, _feeding, _reactsToSamus, _sidehopperDrain) = (Doing.DrainSidehopper, true, false, 320);
                return false;

            case Doing.DrainSidehopper:
                Position = Prey().Position + shake + OnItsPrey;
                if (--_sidehopperDrain > 0)
                    return false;
                _doing = Doing.ActivateSidehopperCorpse;
                Play("finish_draining", restart: true);
                _feeding = false;
                return false;

            case Doing.ActivateSidehopperCorpse:
                Prey().BeginDraining();
                (_doing, _timer) = (Doing.RiseAfterFeeding, 193);
                return true;

            case Doing.RiseAfterFeeding:
                FlyToward(PixelPosition.FromWhole(Position.X.Whole, 104), 16);
                if (--_timer > 0)
                    return false;
                (_doing, _reactsToSamus) = (Doing.HoverNearSamus, true);
                TellRoom("open");
                Room.Terrain.ApplyPatch(WallsDownPatch);
                return false;

            case Doing.HoverNearSamus:
                BuildLock(samus, 8);
                if (_lock >= 256 || samus.Position.X.Whole < ArenaLeft)
                {
                    _doing = Doing.ChaseSamus;
                    return false;
                }

                FlyToward(HoverTarget(samus.Position), 6);
                return false;

            case Doing.ChaseSamus:
                FlyToward(samus.Position + OnItsPrey, 1);
                return false;

            case Doing.BeginDrainingSamus:
                // Clamped on, it has her flash for as long as it drains her.
                (samus as Samus)?.SetDrainFlash(true);
                if (samus is Samus withheldFrom)
                    withheldFrom.ScopeWithheld = true;
                _doing = Doing.DrainSamus;
                return true;

            case Doing.DrainSamus:
                DrainSamus(samus, shake);
                return false;

            case Doing.BeginPostDrainPause:
                (_doing, _timer) = (Doing.PostDrainPause, 121);
                return true;

            case Doing.PostDrainPause:
                if (--_timer > 0)
                    return false;
                (_doing, _timer) = (Doing.RiseAfterDrainingSamus, 193);
                return true;

            case Doing.RiseAfterDrainingSamus:
                FlyToward(PixelPosition.FromWhole(samus.Position.X.Whole, 104), 16);
                if (--_timer > 0)
                    return false;
                Sound(FlyAwayCrySound);
                (_doing, _besideLeft, _timer) = (Doing.FlyBesideSamus, true, 89);
                Play("latched_on", restart: true);
                return true;

            case Doing.FlyBesideSamus:
                FlyToward(_besideLeft
                    ? PixelPosition.FromWhole(samus.Position.X.Whole - 64, 100)
                    : PixelPosition.FromWhole(samus.Position.X.Whole + 96, 104), 16);
                if (--_timer > 0)
                    return false;
                if (_besideLeft)
                {
                    (_besideLeft, _timer) = (false, 89);
                    return true;
                }

                (_doing, _timer) = (Doing.HoldSamusBeforeRelease, 257);
                Play("remorse", restart: true);
                return false;

            case Doing.BeginExit:
                Sound(RemorseCrySound);
                _doing = Doing.Exit;
                return true;

            case Doing.Exit:
                var exit = PixelPosition.FromWhole(-128, 64);
                FlyToward(exit, 16);
                if (!IsOver(exit, 8, 8))
                    return false;
                Vanish();
                return false;

            case Doing.HoldSamusBeforeRelease:
                if (--_timer <= 0)
                {
                    // She gets to her feet and is her own again.
                    if (samus.State is DrainedState drained)
                        drained.Put(DrainedState.Pose.GettingUp);
                    (_reactsToSamus, _doing) = (true, Doing.ReleasedFollow);
                }

                FollowSamus(samus);
                return false;

            case Doing.ReleasedFollow:
                if (FollowSamus(samus))
                    _doing = Doing.BeginExit;
                return false;

            default:
                throw new InvalidOperationException($"The giant Metroid has no step {_doing}.");
        }
    }

    /// <summary>Clamped on Samus it takes her energy every update, slowing her and her fall, until she has almost none.</summary>
    private void DrainSamus(Player samus, PixelOffset shake)
    {
        Inventory inventory = samus.Inventory;
        int energy = inventory.Amount(Inventory.Energy);
        if (energy < 2)
        {
            samus.SpeedHalvings = 0;
            _doing = Doing.BeginPostDrainPause;
            Velocity = PixelVelocity.Zero;
            Play("finish_draining", restart: true);
            (_feeding, _cries) = (false, false);

            // Her flash ends, and as she is let fall her suit's colours show.
            (samus as Samus)?.SetDrainFlash(false);
            if (samus is Samus givenBack)
                givenBack.ScopeWithheld = false;
            samus.ChangeState(new DrainedState(DrainedState.Pose.Kneeling, canStand: false));
            Room.Session.Sounds.RequestMusic(AfterwardMusic);
            return;
        }

        samus.Weapons.WaitAtLeast(8);
        samus.SpeedHalvings = 2;
        // Rising or falling, a speed of four pixels or more an update is cut to two, and for that update it sits a
        // pixel to the right.
        // The cut goes by the cartridge's reading of her speed, which reads one worn past rest, at the top of a rise or
        // in a moonfall, as below four. The pixel to the right goes by the speed's whole pixels as a plain count, which
        // such a speed, held at the top of its range, is far above.
        bool right = Game.Compatibility.CartridgeSpeedReadings.WholeAsCount(samus.HeldSpeedUpOrDown) >= 4;
        if (samus.MovesUpOrDownAtLeast(4))
        {
            Pixels speed = samus.Velocity.Y.Abs();
            samus.Velocity = samus.Velocity.WithY(samus.Velocity.Y.IsNegative ? -speed.WithWhole(2) : speed.WithWhole(2));
        }

        Position = samus.Position + shake + PixelOffset.FromWhole(right ? 1 : 0, -20);
        int taken = inventory.IsOn(Equipment.VariaSuit) ? 2 : 4;
        inventory.Lose(Inventory.Energy, taken >= energy ? energy - 1 : taken);
    }

    /// <summary>
    /// Stays by Samus once it has let her go. Returns whether it is time to leave: it has kept right over her long
    /// enough, or she has gone far to the left.
    /// </summary>
    private bool FollowSamus(Player samus)
    {
        BuildLock(samus, 2);
        Velocity = GradualFlight.Toward(Position, Velocity, HoverTarget(samus.Position + PixelOffset.FromWhole(0, -18)), 8, Kick, Room.Session.Camera.View);
        return _lock >= 1024 || samus.Position.X.Whole < 128;
    }

    /// <summary>It grows surer of her the longer it is right over her, and less sure when it is not.</summary>
    private void BuildLock(Player samus, int reach) =>
        _lock = Math.Abs(Position.X.Whole - samus.Position.X.Whole) < reach ? _lock + 2 : Math.Max(0, _lock - 1);

    /// <summary>Where it hovers: usually high over the place, and now and then, for a moment, at the place itself.</summary>
    private PixelPosition HoverTarget(PixelPosition followed)
    {
        if (_hover > 0)
        {
            _hover--;
            return followed;
        }

        if ((Random.Last.Number & 0xFFF) >= 4064)
            _hover = 32;
        return new PixelPosition(followed.X, Pixels.FromWhole(80));
    }

    /// <summary>Once it has let Samus go, anything she does to it sends it on its way.</summary>
    private void StopFollowing()
    {
        if (_doing == Doing.ReleasedFollow)
            _doing = Doing.BeginExit;
    }

    /// <summary>Its membrane beats, slowly or fast; every few beats it cries.</summary>
    private void StepMembrane()
    {
        // The membrane waits while a fade's count runs, as through a door's fade, when its turns go on.
        if (Room.Session.Service<SharedFadeCount>().IsPastStart)
            return;
        if (--_pulseWait > 0)
            return;
        _pulseWait = _feeding ? FeedingPulseUpdates : CalmPulseUpdates;
        _pulseBeat = (_pulseBeat + 1) % PulseBeats;
        if (_pulseBeat != CryBeat || !_cries || --_criesWait > 0)
            return;
        _criesWait = BeatsACry;
        Sound(_feeding ? FeedingCrySound : CrySound);
    }

    /// <summary>The Sidehopper it feeds on.</summary>
    private DeadSidehopper Prey() =>
        GetNodeOrNull(Victim) as DeadSidehopper
        ?? throw new InvalidOperationException($"The giant Metroid \"{Name}\" names no Sidehopper to feed on.");

    private static PixelRect BodyOf(Enemy enemy) =>
        enemy.GetChildren().OfType<Collider>().FirstOrDefault(part => part.Role == ColliderRole.Body)?.Rect
        ?? throw new InvalidOperationException($"The enemy \"{enemy.Name}\" has no body collider.");

    /// <summary>Tells whatever listens that its room closes or opens.</summary>
    private void TellRoom(string what)
    {
        foreach (NodePath path in ToldOfRoom)
            (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"The giant Metroid is to tell \"{path}\" of its room, but that is nothing that can be told.")).Triggered(what);
    }

    /// <summary>Draws it toward a place.</summary>
    private void FlyToward(PixelPosition target, int divisor) =>
        Velocity = GradualFlight.Toward(Position, Velocity, target, divisor, Kick, Room.Session.Camera.View);

    /// <summary>Whether it is over something at <paramref name="place"/> reaching that far from its middle.</summary>
    private bool IsOver(PixelPosition place, int reachAcross, int reachDown)
    {
        PixelRect body = BodyOf(this);
        return Math.Abs(place.X.Whole - Position.X.Whole) < body.Width / 2 + reachAcross + 1
            && Math.Abs(place.Y.Whole - Position.Y.Whole) < body.Height / 2 + reachDown + 1;
    }

    /// <summary>
    /// Knocks it away from something at <paramref name="source"/>. As on the cartridge: the grapple's angle to it between
    /// whole pixels, clockwise from up, taken backward, and the speed in 256ths of a pixel along that angle's sine across
    /// and along its sine a quarter turn on down.
    /// </summary>
    private void PushAwayFrom(PixelPosition source, Pixels speed)
    {
        int away = Turns.Wrap(-CartridgeAngles.AngleClockwiseFromUp(source.X.Whole - Position.X.Whole, source.Y.Whole - Position.Y.Whole));
        int speedIn256ths = speed.Whole * 256 + speed.Fraction / 256;
        Velocity = new PixelVelocity(
            Velocity.X + FromTwoFiftySixths(CartridgeAngles.SpeedAlong(speedIn256ths, away)),
            Velocity.Y + FromTwoFiftySixths(CartridgeAngles.SpeedAlong(speedIn256ths, away + Turns.PerTurn / 4)));
    }

    /// <summary>A distance given in 256ths of a pixel.</summary>
    private static Pixels FromTwoFiftySixths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 0xFF) << 8);

    /// <summary>
    /// Closes the last of the way to a place, gaining speed toward it and stopping dead as it gets there. Returns
    /// whether it is there, across and down.
    /// </summary>
    private bool CloseOn(PixelPosition target)
    {
        (bool thereAcross, Pixels across) = Close(Position.X.Whole, target.X.Whole, Velocity.X);
        (bool thereDown, Pixels down) = Close(Position.Y.Whole, target.Y.Whole, Velocity.Y);
        Velocity = new PixelVelocity(across, down);
        return thereAcross && thereDown;

        static (bool There, Pixels Speed) Close(int at, int target, Pixels speed)
        {
            Pixels gain = Pixels.FromWhole(2), most = Pixels.FromWhole(5);
            int apart = at - target;
            if (apart == 0)
                return (true, speed);
            Pixels next = apart >= 0 ? (speed - gain < -most ? -most : speed - gain) : (speed + gain > most ? most : speed + gain);
            int then = at + next.Whole - target;
            bool crossed = apart >= 0 ? then <= 0 : then >= 0;
            return (crossed, crossed ? Pixels.Zero : next);
        }
    }
}
