using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Mother Brain's fight on her feet. She stretches, and then thinks: unhurt she mostly paces, now and then attacking;
/// worn down she attacks more, or backs off to fire the beam from her hand. Her attacks are a volley of rings, a
/// laser from her eye, and a bouncing bomb, which she may back off and crouch to throw; which she picks depends on
/// whether Samus is in the air and how nearly level she is. Shots at her head stop her advance.
/// </summary>
/// <remarks>
/// Her shots are "ring", "bomb", "laser", "drool", "dying_drool", "charge_particle", "hand_beam_charge" and
/// "hand_beam"; the glow at her hand lays the beam's pieces down as its timeline says. Her brain plays
/// "stretching_phase2", "neutral_phase2", "four_onion_rings_phase2", "bomb_phase2" and "laser" here.
/// </remarks>
public partial class MotherBrain
{
    /// <summary>The sound of her footfall, by name.</summary>
    [Export, DataValue("footstep_sound")]
    public string FootstepSound { get; set; } = "";

    /// <summary>The sound of the beam from her hand.</summary>
    [Export, DataValue("hand_beam_sound")]
    public string HandBeamSound { get; set; } = "";

    /// <summary>The sound of the laser from her eye.</summary>
    [Export, DataValue("laser_sound")]
    public string LaserSound { get; set; } = "";

    /// <summary>The sound of her gathering her great beam.</summary>
    [Export, DataValue("charge_sound")]
    public string ChargeSound { get; set; } = "";

    /// <summary>The effect of the little breath at her mouth, by the data file of its timeline.</summary>
    [Export, DataValue("small_breath_effect")]
    public string SmallBreathEffect { get; set; } = "";

    /// <summary>The effect of the big breath at her mouth.</summary>
    [Export, DataValue("big_breath_effect")]
    public string BigBreathEffect { get; set; } = "";

    /// <summary>Where her mouth is from her head.</summary>
    public static PixelOffset MouthOffset { get; } = PixelOffset.FromWhole(10, 16);

    private enum Fight
    {
        StretchingShakeHead,
        StretchingBringHeadUp,
        StretchingFinish,
        Thinking,
        TryAttack,
        BombDecideWalking,
        BombWalkingBack,
        BombCrouch,
        BombFired,
        BombStandUp,
        LaserAimQuickly,
        LaserAimSlowlyAndFire,
        LaserFinish,
        HandBeam,
    }

    private enum Attack
    {
        Choose,
        Cooldown,
        End,
    }

    private enum HandBeam
    {
        BackUp,
        WaitForBombs,
        Firing,
        Finish,
    }

    private static readonly PixelOffset[] DroolMouthPoints =
    [
        PixelOffset.FromWhole(14, 18), PixelOffset.FromWhole(8, 23), PixelOffset.FromWhole(10, 19),
        PixelOffset.FromWhole(11, 25), PixelOffset.FromWhole(12, 18), PixelOffset.FromWhole(6, 20),
    ];

    private readonly List<Projectile> _bombs = [];
    private Fight _fight;
    private Attack _attack;
    private HandBeam _handBeam;
    private int _attackCooldown;
    private int _bodyTargetX;
    private int _ringAngle;
    private int _nextDroolPoint;
    private Effect? _smallBreath;
    private PixelPosition _handBeamAt;
    private PixelVelocity _handBeamSpeed;
    private int _handBeamAngle;

    /// <summary>How many of her bombs are still bouncing.</summary>
    private int BombCount
    {
        get
        {
            _bombs.RemoveAll(bomb => !IsInstanceValid(bomb) || bomb.HasEnded);
            return _bombs.Count;
        }
    }

    private void StepSecondPhase(Player samus)
    {
        switch (_fight)
        {
            case Fight.StretchingShakeHead:
                if (--_timer > 0)
                    break;
                PlayHead("stretching_phase2");
                (_fight, _neckTurn, _timer) = (Fight.StretchingBringHeadUp, NeckSlow, 257);
                goto case Fight.StretchingBringHeadUp;

            case Fight.StretchingBringHeadUp:
                if (--_timer > 0)
                    break;
                (_lowerMove, _upperMove) = (NeckMove.BobDown, NeckMove.BobUp);
                (_fight, _timer) = (Fight.StretchingFinish, 65);
                goto case Fight.StretchingFinish;

            case Fight.StretchingFinish:
                if (--_timer > 0)
                    break;
                _breathes = true;
                _fight = Fight.Thinking;
                break;

            case Fight.Thinking:
                Think();
                break;

            case Fight.TryAttack:
                switch (_attack)
                {
                    case Attack.Choose:
                        ChooseAttack(samus);
                        break;
                    case Attack.Cooldown:
                        if (--_attackCooldown <= 0)
                            _attack = Attack.End;
                        break;
                    case Attack.End:
                        (_attack, _fight) = (Attack.Choose, Fight.Thinking);
                        break;
                    default:
                        throw new InvalidOperationException($"Mother Brain's attack has no step {_attack}.");
                }

                break;

            case Fight.BombDecideWalking:
                DecideBombWalk();
                break;

            case Fight.BombWalkingBack:
                if (WalkBackward(_bodyTargetX, "medium"))
                    DecideBombCrouch();
                break;

            case Fight.BombCrouch:
                if (TryCrouch())
                    ThrowBomb();
                break;

            case Fight.BombFired:
                if (--_timer > 0)
                    break;
                _fight = TryStand() ? Fight.Thinking : Fight.BombStandUp;
                break;

            case Fight.BombStandUp:
                if (TryStand())
                    _fight = Fight.Thinking;
                break;

            case Fight.LaserAimQuickly:
                AimLaserQuickly(samus);
                break;

            case Fight.LaserAimSlowlyAndFire:
                if (--_timer > 0)
                    break;
                _neckTurn = NeckSteady;
                PlayHead("laser");
                (_fight, _timer) = (Fight.LaserFinish, 17);
                break;

            case Fight.LaserFinish:
                if (--_timer > 0)
                    break;
                (_lowerMove, _upperMove) = (NeckMove.BobUp, NeckMove.BobUp);
                _fight = Fight.Thinking;
                Think();
                break;

            case Fight.HandBeam:
                StepHandBeam();
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's fight has no step {_fight}.");
        }
    }

    /// <summary>Decides what to do next: with her health gone, the end of this part of her fight; otherwise to pace, attack, or fire from her hand.</summary>
    private void Think()
    {
        if (_headHealth == 0)
        {
            BeginLateFight();
            return;
        }

        if (_pose != Pose.Standing)
            return;
        uint roll = Random.Last.Number & 0xFFFF;
        if (_headHealth >= FullStrengthHealth)
        {
            if (roll < 4096)
                _fight = Fight.TryAttack;
            else
                Pace();
            return;
        }

        if (roll < 8192)
            Pace();
        else if (roll >= 40960)
            _fight = Fight.TryAttack;
        else
            _fight = Fight.HandBeam;
    }

    /// <summary>
    /// Her pacing. Each turn of thought earns her a little toward a step forward; a shot at her head takes it away, and
    /// with none left she backs off.
    /// </summary>
    private void Pace()
    {
        if (_pose != Pose.Standing)
            return;
        int x = Position.X.Whole;
        if (_walkCredit == 0)
        {
            _walkCredit = 1;
            if (x >= 48)
            {
                Play("walk_backward_fast", restart: true);
                return;
            }
        }
        else
        {
            _walkCredit += 6;
            if (_walkCredit >= 256)
            {
                StepForward();
                return;
            }

            if (x >= 48)
                return;
        }

        if ((Random.Last.Number & 0xFFF) >= 4032)
            StepForward();
    }

    private void StepForward()
    {
        _walkCredit = 128;
        if (Position.X.Whole < 128)
            Play("walk_forward_medium", restart: true);
    }

    /// <summary>Walks forward toward a place. Returns whether she is there, or can go no further.</summary>
    private bool WalkForward(int targetX, string speed)
    {
        int x = Position.X.Whole;
        if (x > targetX)
            return true;
        if (_pose != Pose.Standing)
            return false;
        if (x >= 128)
            return true;
        Play($"walk_forward_{speed}", restart: true);
        return false;
    }

    /// <summary>Walks backward toward a place. Returns whether she is there, or can go no further.</summary>
    private bool WalkBackward(int targetX, string speed)
    {
        int x = Position.X.Whole;
        if (x <= targetX)
            return true;
        if (_pose != Pose.Standing)
            return false;
        if (x < 48)
            return true;
        Play($"walk_backward_{speed}", restart: true);
        return false;
    }

    private bool TryCrouch()
    {
        if (_pose == Pose.Crouched)
            return true;
        if (_pose == Pose.Standing)
            Play("crouch_slow", restart: true);
        return false;
    }

    private bool TryStand()
    {
        switch (_pose)
        {
            case Pose.Standing:
                return true;
            case Pose.Crouched:
                Play("stand_after_crouch_fast", restart: true);
                return false;
            case Pose.Leaning:
                Play("stand_after_leaning", restart: true);
                return false;
            default:
                return false;
        }
    }

    /// <summary>Picks an attack by where Samus is and by chance, and then rests a while.</summary>
    private void ChooseAttack(Player samus)
    {
        _attackCooldown = 64;
        _attack = Attack.Cooldown;
        int roll = Random.Last.Part(0, 8);
        if (!samus.IsOnGround)
        {
            // Samus in the air gets rings or the laser.
            if (roll >= 128)
                PlayHead("four_onion_rings_phase2");
            else
                AimLaserQuickly(samus);
            return;
        }

        if (roll >= 128 && BombCount < 1)
        {
            _fight = Fight.BombDecideWalking;
            DecideBombWalk();
            return;
        }

        // Nearly level with her head, Samus mostly gets the laser; otherwise it is an even choice.
        bool near = Math.Abs(_head.Y.Whole + 4 - samus.Position.Y.Whole) < 32;
        int choice = near ? (roll < 16 ? 0 : roll < 32 ? 1 : roll < 208 ? 2 : 3) : roll / 64;
        switch (choice)
        {
            case 0:
                PlayHead("neutral_phase2");
                break;
            case 1:
                PlayHead("four_onion_rings_phase2");
                break;
            case 2:
                AimLaserQuickly(samus);
                break;
            default:
                if (BombCount < 1)
                {
                    _fight = Fight.BombDecideWalking;
                    DecideBombWalk();
                }

                break;
        }
    }

    /// <summary>Before a bomb she nearly always backs off a little way first.</summary>
    private void DecideBombWalk()
    {
        uint roll = Random.Last.Number & 0xFFFF;
        if (roll < 65408)
        {
            int targetX = roll < 24576 ? 96 : 64;
            if (targetX < Position.X.Whole)
            {
                _bodyTargetX = targetX;
                if (!WalkBackward(targetX, "medium"))
                {
                    _fight = Fight.BombWalkingBack;
                    return;
                }
            }
        }

        DecideBombCrouch();
    }

    /// <summary>As often as not she throws the bomb standing; otherwise she crouches first.</summary>
    private void DecideBombCrouch()
    {
        if ((Random.Draw().Number & 0x8000) == 0)
        {
            ThrowBomb();
            return;
        }

        _fight = Fight.BombCrouch;
        if (TryCrouch())
            ThrowBomb();
    }

    private void ThrowBomb()
    {
        PlayHead("bomb_phase2");
        (_fight, _timer) = (Fight.BombFired, 45);
    }

    /// <summary>Swings her head toward Samus's height, fast, before the laser.</summary>
    private void AimLaserQuickly(Player samus)
    {
        NeckMove move = _head.Y.Whole >= samus.Position.Y.Whole ? NeckMove.Raise : NeckMove.Lower;
        (_lowerMove, _upperMove, _neckTurn) = (move, move, NeckQuick);
        (_fight, _timer) = (Fight.LaserAimSlowlyAndFire, 5);
    }

    /// <summary>The beam from her hand: she backs right off, waits for her bombs to be gone, and fires.</summary>
    private void StepHandBeam()
    {
        switch (_handBeam)
        {
            case HandBeam.BackUp:
                if (!WalkBackward(40, "slow"))
                    break;
                (_lowerMove, _upperMove) = (NeckMove.Raise, NeckMove.Lower);
                _handBeam = HandBeam.WaitForBombs;
                break;

            case HandBeam.WaitForBombs:
                if (BombCount != 0)
                    break;
                Play("hand_beam", restart: true);
                _handBeam = HandBeam.Firing;
                break;

            case HandBeam.Firing:
                break;

            case HandBeam.Finish:
                PlayHead("neutral_phase2");
                (_lowerMove, _upperMove) = (NeckMove.BobDown, NeckMove.BobUp);
                _handBeam = HandBeam.BackUp;
                _fight = Fight.Thinking;
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's hand beam has no step {_handBeam}.");
        }
    }

    /// <summary>Gathers the beam at her hand and aims it at Samus.</summary>
    private void BeginHandBeam()
    {
        _handBeamAt = Position + PixelOffset.FromWhole(64, -48);
        PixelPosition samus = Room.Player.Position;
        // The angle is kept from straight down toward the right: the cartridge's aim, from whole pixels to whole
        // pixels, a quarter turn on.
        int aim = CartridgeAngles.AimAngle(samus.X.Whole - _handBeamAt.X.Whole, samus.Y.Whole - _handBeamAt.Y.Whole);
        _handBeamAngle = Turns.Wrap(aim + Turns.PerTurn / 4);
        _handBeamSpeed = AlongHeading(In256ths(Shot("hand_beam").Speed), _handBeamAngle);
        ((MotherBrainHandBeam)Fire("hand_beam_charge", _handBeamAt, PixelVelocity.Zero)).FiredBy(this);
    }

    /// <summary>
    /// A speed in 256ths of a pixel split along a heading from straight down toward the right, as the cartridge splits
    /// it: across by the heading's sine, down by the sine a quarter turn on.
    /// </summary>
    private static PixelVelocity AlongHeading(int speedIn256ths, int heading) => new(
        From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading)),
        From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading + Turns.PerTurn / 4)));

    /// <summary>A distance in the 256ths of a pixel the cartridge holds a speed in, what is finer dropped.</summary>
    private static int In256ths(Pixels distance) => distance.Whole * 256 + distance.Fraction / 256;

    /// <summary>A distance from a count of 256ths of a pixel.</summary>
    private static Pixels From256ths(int count) => Pixels.FromParts(count >> 8, (count & 255) * 256);

    /// <summary>
    /// Lays down the next piece of the beam from her hand: a step further along its line and knocked a little off it
    /// by chance. One that lands outside the fighting floor blows up instead.
    /// </summary>
    internal void LayHandBeamPiece()
    {
        _handBeamAt += _handBeamSpeed.OverOneUpdate;
        int turn = Turns.Wrap(_handBeamAngle + Random.Draw().Part(0, 8));
        int knock = Random.Draw().Part(8, 3);
        PixelPosition at = _handBeamAt + AlongHeading(knock * 256, turn).OverOneUpdate;
        if (at.Y.Whole < 34 || at.Y.Whole >= 206 || at.X.Whole < 2 || at.X.Whole >= 238)
        {
            Leave(BigExplosionEffect, at);
            Sound(ExplosionSound, SoundPriority.Low);
            Room.Shake(2, 10);
            return;
        }

        // Each piece lays down the next in its turn, as the glow at her hand does.
        ((MotherBrainHandBeam)Fire("hand_beam", at, PixelVelocity.Zero)).FiredBy(this);
    }

    /// <summary>Acts on a cue of her brain's timeline.</summary>
    private void OnHeadCue(string cue, Player samus)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "sound":
                Sound(parts[1]);
                break;

            case "shake":
                _brainShake = 50;
                break;

            case "drool":
                if (!_drools)
                    break;
                PixelOffset point = DroolMouthPoints[_nextDroolPoint];
                _nextDroolPoint = (_nextDroolPoint + 1) % DroolMouthPoints.Length;
                ((MotherBrainDrool)Fire(_neckTurn < NeckDyingDrool ? "drool" : "dying_drool", _head + point, PixelVelocity.Zero)).HangFrom(this, point);
                break;

            case "big_breath":
                if (BigBreathEffect.Length > 0)
                    LeaveHoldingPlace(BigBreathEffect, _head + PixelOffset.FromWhole(6, 16));
                break;

            case "neck":
                _neckMoves = parts[1] == "on";
                break;

            case "aim_rings":
                AimRings(samus.Position);
                break;

            case "ring":
                var ring = (MotherBrainRing)Fire("ring", _head + MouthOffset, AlongHeading(In256ths(Shot("ring").Speed), _ringAngle));
                ring.HeldBy(this);
                break;

            case "laser":
                _neckMoves = false;
                Fire("laser", _head + PixelOffset.FromWhole(16, 4), new PixelVelocity(Shot("laser").Speed, Pixels.Zero));
                Sound(LaserSound);
                break;

            case "bomb":
                var bomb = (MotherBrainBomb)Fire("bomb", PixelPosition.FromWhole(_head.X.Whole + 12, _head.Y.Whole + 16), new PixelVelocity(Pixels.FromNumber(0.875), Pixels.FromWhole(1)));
                bomb.Flames = Number(parts[1]);
                _bombs.Add(bomb);
                break;

            case "charge":
                _breathes = false;
                Sound(ChargeSound);
                break;

            case "charge_particle":
                ((MotherBrainDrool)Fire("charge_particle", _head, PixelVelocity.Zero)).HangFrom(this, PixelOffset.FromWhole(0, 0));
                break;

            case "maybe_hold":
                // Nearly always she holds her look a while before the next.
                if (Random.Last.Part(12, 4) < 15)
                    PlayHead(parts[1]);
                break;

            case "maybe_hold_late":
                // Her damaged head holds its look a little less often.
                if ((Random.Last.Number & 0xFFF) < 3776)
                    PlayHead(parts[1]);
                break;

            case "maybe_drool":
                if ((Random.Last.Number & 0xFFF) >= 4064)
                    PlayHead(parts[1]);
                break;

            default:
                OnLateFightHeadCue(parts, samus);
                break;
        }
    }

    /// <summary>
    /// Aims her rings at a place: at it if it lies ahead of and a little below or above her mouth, and otherwise as near
    /// as she can turn them. Angles here are in 256ths of a turn from straight down toward the right.
    /// </summary>
    private void AimRings(PixelPosition target)
    {
        // The cartridge aims from whole pixels to whole pixels, a quarter turn on so as to count from straight down.
        PixelPosition mouth = _head + MouthOffset;
        int angle = Turns.Wrap(CartridgeAngles.AimAngle(target.X.Whole - mouth.X.Whole, target.Y.Whole - mouth.Y.Whole) + Turns.PerTurn / 4);
        _ringAngle = angle is >= 16 and < 72 ? angle : angle is >= 72 and < 192 ? 72 : 16;
    }
}
