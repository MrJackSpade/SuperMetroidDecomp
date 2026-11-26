using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The giant Metroid that comes to Samus's aid against Mother Brain. It swoops in as her last beam fires, latches
/// onto her head and drains her until she stands lifeless; then it flies to Samus and gives her energy back. When
/// Mother Brain wakes and fires on them it shields Samus until it can take no more, lets go, and charges; her last
/// volley kills it. As it falls Samus takes in what it leaves her, the hyper beam.
/// </summary>
/// <remarks>
/// It plays the timelines "idle_pulse", "draining_mother_brain" and "taking_fatal_blow". Mother Brain makes it and
/// tells it whose fight it has come to.
/// </remarks>
public partial class MotherBrainBaby : Enemy
{
    /// <summary>Its cry.</summary>
    [Export, DataValue("cry_sound")]
    public string CrySound { get; set; } = "";

    /// <summary>The cry it gives now and then as it comes to Samus.</summary>
    [Export, DataValue("remorse_cry_sound")]
    public string RemorseCrySound { get; set; } = "";

    /// <summary>The sound of its death.</summary>
    [Export, DataValue("death_sound")]
    public string DeathSound { get; set; } = "";

    /// <summary>The sound of the energy it gives Samus.</summary>
    [Export, DataValue("heal_sound")]
    public string HealSound { get; set; } = "";

    /// <summary>The sound of the blasts of its death.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>The effect of the dust it leaves as it lets go of Mother Brain.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of the blasts of its death.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The music that plays once it is dead, by name.</summary>
    [Export]
    public string SamusThemeMusic { get; set; } = "";

    /// <summary>How much of Mother Brain's rings it can take while it shields Samus.</summary>
    [Export, DataValue("shielding_health")]
    public int ShieldingHealth { get; set; } = 3200;

    /// <summary>Where it comes in, in the room's pixels.</summary>
    [Export]
    public Vector2I ArrivalPlace { get; set; } = new(320, 96);

    private enum Phase
    {
        DashOntoScreen,
        CurveTowardHead,
        GetInHerFace,
        LatchOntoMotherBrain,
        SetMotherBrainStumbling,
        BeginDraining,
        WaitForCorpse,
        StopDraining,
        LetGo,
        MoveToCeiling,
        MoveToSamus,
        LatchOntoSamus,
        HealSamus,
        ShieldSamus,
        ReleaseSamus,
        StareDown,
        FlyToRightEdge,
        MoveToChargeStart,
        BeginCharge,
        Charge,
        TakeFinalBlow,
        PlaySamusTheme,
        PrepareSamus,
        DeathSequence,
        PauseAfterFade,
        LetSamusGlow,
        LightsReturn,
    }

    private enum SamusGlow
    {
        None,
        WhenItHasFallenToHer,
        Slowing,
    }

    /// <summary>The way it takes to Samus: each place, and what turns it about when it strays out of view on the way.</summary>
    private static readonly (PixelPosition Target, Pixels Kick)[] Route =
    [
        (PixelPosition.FromWhole(160, 120), Pixels.FromNumber(1 / 16.0)), (PixelPosition.FromWhole(304, 122), Pixels.FromNumber(1 / 16.0)),
        (PixelPosition.FromWhole(192, 64), Pixels.FromNumber(1 / 16.0)), (PixelPosition.FromWhole(192, 112), Pixels.FromNumber(1 / 16.0)),
        (PixelPosition.FromWhole(224, 128), Pixels.FromNumber(1 / 16.0)), (PixelPosition.FromWhole(205, 144), Pixels.FromNumber(1 / 32.0)),
        (PixelPosition.FromWhole(204, 160), Pixels.FromNumber(1 / 32.0)), (PixelPosition.FromWhole(203, 176), Pixels.FromNumber(1 / 32.0)),
    ];

    private static readonly PixelOffset[] ShakeOffsets = [PixelOffset.FromWhole(0, 0), PixelOffset.FromWhole(-1, 1), PixelOffset.FromWhole(0, -1), PixelOffset.FromWhole(1, 1)];
    private static readonly PixelOffset[] ReleaseDust = [PixelOffset.FromWhole(-16, -8), PixelOffset.FromWhole(0, -16), PixelOffset.FromWhole(16, -8)];

    private static readonly PixelOffset[] DeathScatter =
    [
        PixelOffset.FromWhole(-20, 20), PixelOffset.FromWhole(16, -30), PixelOffset.FromWhole(30, -3), PixelOffset.FromWhole(14, -13), PixelOffset.FromWhole(-2, 18),
        PixelOffset.FromWhole(-2, -32), PixelOffset.FromWhole(-31, 8), PixelOffset.FromWhole(-4, -10), PixelOffset.FromWhole(19, 19), PixelOffset.FromWhole(-24, -24),
    ];

    private static readonly PixelOffset OnHerHead = PixelOffset.FromWhole(0, -24);
    private static readonly PixelOffset OnSamus = PixelOffset.FromWhole(0, -20);
    private static readonly Pixels Gentle = Pixels.FromNumber(1 / 16.0);
    private static readonly Pixels Hard = Pixels.FromWhole(4);

    /// <summary>How far it reaches from its middle, across and down.</summary>
    private const int Reach = 36;

    private MotherBrain? _mother;
    private Phase _phase;
    private int _timer;
    private int _health;
    private int _leg;
    private Pixels _speed;
    private int _heading;
    private int _ringShake;
    private int _pendingCries;
    private PixelPosition _struckAt;
    private int _fadeStep;
    private int _fadeWait;
    private int _blastWait;
    private int _nextBlast;
    private int _lightsShown;
    private SamusGlow _samusGlow;
    private int _glowPart;
    private Color _tint = Colors.White;

    /// <summary>Whether it can take no more of Mother Brain's rings.</summary>
    public bool IsSpent => _health == 0;

    /// <inheritdoc/>
    protected override Color RestingTint => _tint;

    /// <summary>Says whose fight it has come to.</summary>
    public void ComeFor(MotherBrain mother) => _mother = mother;

    /// <summary>Whether it is in the way of something filling <paramref name="area"/>.</summary>
    public bool Covers(PixelRect area) =>
        Math.Abs(area.Left + area.Width / 2 - Position.X.Whole) < area.Width / 2 + Reach
        && Math.Abs(area.Top + area.Height / 2 - Position.Y.Whole) < area.Height / 2 + Reach;

    /// <summary>Takes one of Mother Brain's rings.</summary>
    public void TakeRing(int damage)
    {
        if (_health == 0)
            return;
        _pendingCries++;
        _ringShake = 16;
        FlashFor(16);
        _health = Math.Max(0, _health - damage);
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part) => HitResult.Absorbed;

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It does Samus no harm.
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Position = PixelPosition.FromWhole(ArrivalPlace.X, ArrivalPlace.Y);
        Velocity = PixelVelocity.Zero;
        _health = ShieldingHealth;
        (_phase, _timer) = (Phase.DashOntoScreen, 249);
        (_fadeStep, _fadeWait, _blastWait, _nextBlast) = (0, 1, 1, 0);
        Play("idle_pulse");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        MotherBrain mother = _mother is { } known && IsInstanceValid(known)
            ? known
            : throw new InvalidOperationException("The giant Metroid of Mother Brain's fight has not been told whose fight it is.");
        Player samus = Room.Player;
        PixelPosition head = mother.HeadPosition;

        if (_pendingCries != 0 && _phase is Phase.HealSamus or Phase.ShieldSamus)
        {
            _pendingCries = 0;
            Sound(CrySound);
        }

        // Once it touches her it closes the last of the way and settles on her.
        if (_phase == Phase.LatchOntoSamus && Touches(samus) && CloseOn(samus.Position + OnSamus, Gentle))
        {
            Velocity = PixelVelocity.Zero;
            _phase = Phase.HealSamus;
        }

        while (StepPhase(mother, samus, head))
        {
        }

        if (IsDead)
            return;
        Position += Velocity.OverOneUpdate;
        if (_ringShake > 0)
            _ringShake--;
    }

    /// <summary>Plays one step of what it is doing. Returns whether the next step is to be played at once.</summary>
    private bool StepPhase(MotherBrain mother, Player samus, PixelPosition head)
    {
        PixelOffset shake = ShakeOffsets[Updates / 2 % 4];
        switch (_phase)
        {
            case Phase.DashOntoScreen:
                if (--_timer > 0)
                    return false;
                (_heading, _speed) = (216 * 256, Pixels.FromWhole(10));
                (_phase, _timer) = (Phase.CurveTowardHead, 11);
                return true;

            case Phase.CurveTowardHead:
                Swoop(384, 176 * 256, Pixels.FromWhole(10));
                if (--_timer <= 0)
                    (_phase, _timer) = (Phase.GetInHerFace, 10);
                return false;

            case Phase.GetInHerFace:
                Swoop(1536, 130 * 256, Pixels.FromWhole(14));
                if (!IsAt(head, 4) && --_timer > 0)
                    return false;
                _phase = Phase.LatchOntoMotherBrain;
                PutSamus(samus, DrainedState.Pose.Standing);
                return false;

            case Phase.LatchOntoMotherBrain:
                FlyToward(head + OnHerHead, 16, Hard);
                if (IsAt(head + OnHerHead, 8))
                    _phase = Phase.SetMotherBrainStumbling;
                return false;

            case Phase.SetMotherBrainStumbling:
                mother.StumbleFromBaby();
                _phase = Phase.BeginDraining;
                return true;

            case Phase.BeginDraining:
                if (!CloseOn(head + OnHerHead, Pixels.FromWhole(2)))
                    return false;
                Velocity = PixelVelocity.Zero;
                Position = head + OnHerHead;
                Play("draining_mother_brain", restart: true);
                _phase = Phase.WaitForCorpse;
                mother.BeDrainedByBaby();
                return false;

            case Phase.WaitForCorpse:
                Position = head + shake + OnHerHead;
                if (mother.IsDrainedLifeless)
                    (_phase, _timer) = (Phase.StopDraining, 65);
                return false;

            case Phase.StopDraining:
                Position = head + OnHerHead;
                if (--_timer > 0)
                    return false;
                Play("idle_pulse", restart: true);
                (_phase, _timer) = (Phase.LetGo, 33);
                Velocity = PixelVelocity.Zero;
                return false;

            case Phase.LetGo:
                if (--_timer <= 0)
                {
                    foreach (PixelOffset dust in ReleaseDust)
                        Leave(DustEffect, head + dust);
                    _phase = Phase.MoveToCeiling;
                }

                RiseToCeiling(samus, head);
                return false;

            case Phase.MoveToCeiling:
                RiseToCeiling(samus, head);
                return false;

            case Phase.MoveToSamus:
                if ((Random.Last.Number & 0xFFF) >= 4000)
                    Sound(RemorseCrySound);
                (PixelPosition target, Pixels kick) = Route[_leg];
                FlyToward(target, 16, kick);
                if (!IsAt(target, 4))
                    return false;
                if (_leg + 1 < Route.Length)
                    _leg++;
                else
                    _phase = Phase.LatchOntoSamus;
                return false;

            case Phase.LatchOntoSamus:
                FlyToward(samus.Position + OnSamus, 16, Hard);
                return false;

            case Phase.HealSamus:
                Position = samus.Position + shake + OnSamus;
                Inventory inventory = samus.Inventory;
                if (inventory.Amount(Inventory.Energy) >= 81 && Room.Session.EnemyPasses % 8 == 0 && HealSound.Length > 0)
                    Room.Session.Sounds.Request(HealSound, channel: "pickups", priority: SoundPriority.Low);
                inventory.Add(Inventory.Energy, 1);
                if (!inventory.IsFull(Inventory.Energy))
                    return false;
                inventory.Fill(Amounts.ReserveEnergy);
                _phase = Phase.ShieldSamus;
                return false;

            case Phase.ShieldSamus:
                if (_ringShake > 0)
                {
                    PixelOffset jolt = ShakeOffsets[_ringShake / 2 % 4];
                    Position = samus.Position + jolt + jolt + OnSamus;
                }

                if (_health != 0)
                    return false;
                _health = 320;
                _phase = Phase.ReleaseSamus;
                return false;

            case Phase.ReleaseSamus:
                mother.PrepareForLastBabyAttack();
                _phase = Phase.StareDown;
                return true;

            case Phase.StareDown:
                var stare = PixelPosition.FromWhole(samus.Position.X.Whole - 4, 96);
                FlyToward(stare, 16, Gentle);
                if (IsAt(stare, 4))
                    _phase = Phase.FlyToRightEdge;
                return false;

            case Phase.FlyToRightEdge:
                var edge = PixelPosition.FromWhole(272, 64);
                FlyToward(edge, 16, Gentle);
                if (IsAt(edge, 4))
                    _phase = Phase.MoveToChargeStart;
                return false;

            case Phase.MoveToChargeStart:
                var start = PixelPosition.FromWhole(305, 160);
                FlyToward(start, 16, Gentle);
                if (!IsAt(start, 4))
                    return false;
                _health = 79;
                mother.FireLastBabyAttack();
                _phase = Phase.BeginCharge;
                return false;

            case Phase.BeginCharge:
                var aim = PixelPosition.FromWhole(290, 128);
                FlyToward(aim, 6, Hard);
                if (IsAt(aim, 4))
                    _phase = Phase.Charge;
                return false;

            case Phase.Charge:
                FlyToward(head + PixelOffset.FromWhole(0, -32), 4, Hard);
                if (_health != 0)
                    return false;
                Play("taking_fatal_blow", restart: true);
                Velocity = PixelVelocity.Zero;
                (_phase, _timer) = (Phase.TakeFinalBlow, 17);
                _struckAt = Position;
                Sound(DeathSound);
                mother.PutLightsOut();
                Room.Session.Sounds.RequestMusic("");
                return true;

            case Phase.TakeFinalBlow:
                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(1 / 256.0));
                Position = _struckAt + shake;
                if (--_timer > 0)
                    return false;
                Position = _struckAt;
                (_phase, _timer) = (Phase.PlaySamusTheme, 57);
                return true;

            case Phase.PlaySamusTheme:
                if (--_timer > 0)
                    return false;
                Compatibility.CartridgeSounds.Of(Room.Session).AskMusicSet("samus_theme_upper_crateria");
                if (SamusThemeMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(SamusThemeMusic);
                (_phase, _timer) = (Phase.PrepareSamus, 13);
                return true;

            case Phase.PrepareSamus:
                if (--_timer > 0)
                    return false;
                PutSamus(samus, DrainedState.Pose.Frozen);
                _samusGlow = SamusGlow.WhenItHasFallenToHer;
                _phase = Phase.DeathSequence;
                return false;

            case Phase.DeathSequence:
                StepSamusGlow(samus);
                // It sinks, slowing across.
                Pixels across = Velocity.X.Abs() - Pixels.FromNumber(0.125);
                across = across.IsNegative ? Pixels.Zero : Velocity.X.IsNegative ? -across : across;
                Velocity = new PixelVelocity(across, Velocity.Y + Pixels.FromNumber(1 / 128.0));
                if (FadeToBlack())
                {
                    Visual?.SetShown(false);
                    (_phase, _timer) = (Phase.PauseAfterFade, 132);
                    return false;
                }

                if (--_blastWait <= 0)
                {
                    _blastWait = 5;
                    Leave(ExplosionEffect, Position + DeathScatter[_nextBlast]);
                    _nextBlast = (_nextBlast + 1) % DeathScatter.Length;
                    Sound(ExplosionSound);
                }

                Visual?.SetShown(Updates % 2 != 0);
                return false;

            case Phase.PauseAfterFade:
                StepSamusGlow(samus);
                if (--_timer > 0)
                    return false;
                (_phase, _timer) = (Phase.LetSamusGlow, 177);
                return true;

            case Phase.LetSamusGlow:
                if (--_timer > 0)
                    return false;
                (_phase, _lightsShown) = (Phase.LightsReturn, 0);
                return true;

            case Phase.LightsReturn:
                // Each update shows the next step of the lights' coming back, the first on the update the glow's wait
                // ran out; the update after the last ends the scene.
                if (_lightsShown < mother.LightsBackSteps)
                {
                    mother.ShowLightsBack(_lightsShown++);
                    return false;
                }

                mother.BeginThirdPhase();
                // Samus stops glowing, gets to her feet, and has the hyper beam.
                if (samus.State is DrainedState drained)
                {
                    drained.Glows = false;
                    drained.Put(DrainedState.Pose.GettingUp);
                }

                DrainedState.GiveHyperBeam(samus);
                Vanish();
                return false;

            default:
                throw new InvalidOperationException($"The giant Metroid has no step {_phase}.");
        }
    }

    /// <summary>It rises from her head to the ceiling, and from there sets off for Samus, who sinks to her knees.</summary>
    private void RiseToCeiling(Player samus, PixelPosition head)
    {
        var target = new PixelPosition(head.X, Pixels.Zero);
        FlyToward(target, 16, Hard);
        if (!IsAt(target, 4))
            return;
        PutSamus(samus, DrainedState.Pose.Kneeling);
        (_phase, _leg) = (Phase.MoveToSamus, 0);
    }

    /// <summary>Samus begins to glow once it has fallen nearly to her, and the glow slows as it goes on.</summary>
    private void StepSamusGlow(Player samus)
    {
        if (samus.State is not DrainedState drained)
            return;
        switch (_samusGlow)
        {
            case SamusGlow.None:
                break;
            case SamusGlow.WhenItHasFallenToHer:
                if (Position.Y.Whole + 16 < samus.Position.Y.Whole)
                    break;
                (drained.Glows, drained.GlowHold) = (true, 1);
                _samusGlow = SamusGlow.Slowing;
                break;
            case SamusGlow.Slowing:
                _glowPart += 3;
                if (_glowPart < 256)
                    break;
                _glowPart -= 256;
                drained.GlowHold = Math.Min(drained.GlowHold + 1, 10);
                break;
            default:
                throw new InvalidOperationException($"The glow the giant Metroid gives Samus has no step {_samusGlow}.");
        }
    }

    /// <summary>Once it has sunk far enough it fades to black. Returns whether it has gone.</summary>
    private bool FadeToBlack()
    {
        if (Position.Y.Whole < 128)
            return false;
        if (--_fadeWait > 0)
            return false;
        _fadeWait = 9;
        float left = 1f - Math.Min(_fadeStep, 5) / 5f;
        _tint = new Color(left, left, left);
        return ++_fadeStep > 6;
    }

    /// <summary>Puts a drained Samus in a pose; one who had got up is drained again.</summary>
    private static void PutSamus(Player samus, DrainedState.Pose pose)
    {
        if (samus.State is DrainedState drained)
            drained.Put(pose);
        else
            samus.ChangeState(new DrainedState(pose, canStand: false));
    }

    /// <summary>Draws it toward a place.</summary>
    private void FlyToward(PixelPosition target, int divisor, Pixels wrongWayKick) =>
        Velocity = GradualFlight.Toward(Position, Velocity, target, divisor, wrongWayKick, Room.Session.Camera.View);

    /// <summary>Whether it is over a place, give or take <paramref name="reach"/> pixels beyond its own.</summary>
    private bool IsAt(PixelPosition place, int reach) =>
        Math.Abs(place.X.Whole - Position.X.Whole) <= reach + Reach && Math.Abs(place.Y.Whole - Position.Y.Whole) <= reach + Reach;

    /// <summary>Whether it touches Samus. One who has just been hurt it cannot touch.</summary>
    private bool Touches(Player samus)
    {
        if (samus.IsInvincible)
            return false;
        PixelRect body = samus.BodyRect;
        return Axis(samus.Position.X.Whole, Position.X.Whole, body.Width / 2) && Axis(samus.Position.Y.Whole, Position.Y.Whole, body.Height / 2);

        static bool Axis(int hers, int its, int herReach)
        {
            int apart = Math.Abs(hers - its);
            return apart < herReach || apart - herReach < Reach;
        }
    }

    /// <summary>
    /// Closes the last of the way to a place, gaining speed toward it and stopping dead as it gets there. Returns
    /// whether it is there, across and down.
    /// </summary>
    private bool CloseOn(PixelPosition target, Pixels gain)
    {
        (bool thereAcross, Pixels across) = Close(Position.X.Whole, target.X.Whole, Velocity.X, gain, stopsOnIt: false);
        (bool thereDown, Pixels down) = Close(Position.Y.Whole, target.Y.Whole, Velocity.Y, gain, stopsOnIt: true);
        Velocity = new PixelVelocity(across, down);
        return thereAcross && thereDown;

        static (bool There, Pixels Speed) Close(int at, int target, Pixels speed, Pixels gain, bool stopsOnIt)
        {
            Pixels most = Pixels.FromWhole(5);
            int apart = at - target;
            if (apart == 0 && stopsOnIt)
                return (true, speed);
            if (apart < 0)
            {
                speed = speed + gain > most ? most : speed + gain;
                if (at + speed.Whole < target)
                    return (false, speed);
            }
            else
            {
                speed = speed - gain < -most ? -most : speed - gain;
                if (at + speed.Whole - target > 0)
                    return (false, speed);
            }

            return (true, Pixels.Zero);
        }
    }

    /// <summary>
    /// Flies it along a heading that turns toward a heading and stops there, at a speed that moves toward a speed.
    /// Headings are in 256ths of a 256th of a turn, from straight down toward the right.
    /// </summary>
    private void Swoop(int turn, int target, Pixels speed)
    {
        Pixels step = Pixels.FromNumber(0.125);
        if (_speed != speed)
            _speed = speed < _speed ? (_speed - step < speed ? speed : _speed - step) : (_speed + step > speed ? speed : _speed + step);
        int full = Turns.PerTurn * 256;
        int turned = _heading - turn;
        int ahead = ((turned - target) % full + full + full / 2) % full - full / 2;
        _heading = ahead >= 0 ? ((turned % full) + full) % full : target;
        // Each part is the speed in 256ths of a pixel along the heading's coarse sine, as the cartridge splits it.
        int angle = Turns.Wrap(_heading >> 8);
        int speedIn256ths = _speed.Whole * 256 + _speed.Fraction / 256;
        Velocity = new PixelVelocity(
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, angle)),
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, angle + Turns.PerTurn / 4)));
    }

    /// <summary>A distance from a count of 256ths of a pixel.</summary>
    private static Pixels From256ths(int count) => Pixels.FromParts(count >> 8, (count & 255) * 256);

    /// <summary>Leaves an effect, if it is named.</summary>
    private void Leave(string effect, PixelPosition at)
    {
        if (effect.Length > 0)
            LeaveEffect(effect, at);
    }
}
