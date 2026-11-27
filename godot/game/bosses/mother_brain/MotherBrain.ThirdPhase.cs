using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Mother Brain's last form and her death. Come back to life, she inches forward spitting rings and bombs; each
/// shot of the hyper beam costs her ground, and one she cannot afford throws her head back and drives her off. With
/// her health gone she backs away bursting, stumbles forward, and her body fades from under her brain, which
/// drops, greys, tips over and crumbles to dust. Then the alarm: the room shakes, the way out is blown open, and
/// the countdown begins.
/// </summary>
/// <remarks>
/// Her brain plays "bomb_phase3", "four_onion_rings_phase3", "hyper_beam_recoil", "dying_drool", "decapitated" and
/// "corpse" here. Her data gives "death_explosions", groups of four places from her body where she bursts;
/// "escape_door_dust", the places of the dust at the way out; "escape_door_fragments", the pieces thrown from it,
/// each with how far "down" from the door it starts and its "rise"; and "escape_door_patch", the terrain patch that
/// opens it. As the alarm starts she tells what listens "escape" and what writes the warning out to "start"; once
/// that has done, or she is told "escape_text_done", she starts the escape's countdown.
/// </remarks>
public partial class MotherBrain : ITriggerTarget
{
    /// <summary>The music of the escape, by name.</summary>
    [Export]
    public string EscapeMusic { get; set; } = "";

    /// <summary>The effect of the dust her corpse crumbles into.</summary>
    [Export, DataValue("corpse_dust_effect")]
    public string CorpseDustEffect { get; set; } = "";

    /// <summary>The sound of her corpse crumbling.</summary>
    [Export, DataValue("corpse_dust_sound")]
    public string CorpseDustSound { get; set; } = "";

    /// <summary>The effect that shows a piece thrown from the way out.</summary>
    [Export, DataValue("door_fragment_effect")]
    public string DoorFragmentEffect { get; set; } = "";

    /// <summary>Where the way out is, in the room's pixels.</summary>
    [Export]
    public Vector2I EscapeDoorPlace { get; set; } = new(16, 128);

    private enum Third
    {
        RecoverMakeSomeDistance,
        RecoverSetupForFighting,
        FightingMain,
        FightingAttackCooldown,
    }

    private enum ThirdWalk
    {
        Inactive,
        TryToInchForward,
        RetreatQuickly,
        RetreatSlowly,
    }

    private enum ThirdNeck
    {
        Inactive,
        Normal,
        StartRecoilRecovery,
        RecoilRecovery,
        StartHyperBeamRecoil,
        HyperBeamRecoil,
    }

    private enum Death
    {
        MoveToBackOfRoom,
        IdleWhilstExploding,
        StumbleToMiddleOfRoom,
        BrainComesAway,
        ExplodesBeforeBodyFades,
        FadeOutBody,
        FinalFewExplosions,
        RealizeDecapitation,
        BrainFallsToGround,
        BrainSettles,
        LiesStillBeforeGreying,
        FadeToGrey,
        CorpseTipsOver,
        CorpseRotsAway,
        PauseBeforeEscape,
        HoldBeforeEscape,
        StartEscape,
        TypeOutEscapeText,
        EscapeDoorBursts,
        BlowUpEscapeDoor,
        KeepEarthquakeGoing,
    }

    private sealed record DoorFragment(int Down, Pixels Rise);

    private sealed class FlyingFragment(Effect sprite, PixelVelocity speed)
    {
        public Effect Sprite { get; } = sprite;

        public PixelVelocity Speed { get; set; } = speed;

        public int Left { get; set; } = 33;
    }

    /// <summary>How fast her neck turns as it bobs in her last form.</summary>
    private const int NeckBobbing = 128;

    /// <summary>What a shot of the hyper beam costs her advance.</summary>
    private const int HyperBeamCost = 266;

    /// <summary>How many rows of her corpse crumble away.</summary>
    private const int CorpseRows = 48;

    private readonly List<PixelOffset[]> _deathExplosions = [];
    private readonly List<PixelPosition> _escapeDoorDust = [];
    private readonly List<DoorFragment> _doorFragments = [];
    private readonly List<FlyingFragment> _flyingFragments = [];
    private readonly List<(Effect Blast, PixelOffset From)> _bodyBlasts = [];
    private string _escapeDoorPatch = "";
    private Third _third;
    private ThirdWalk _thirdWalk;
    private ThirdNeck _thirdNeck;
    private int _thirdNeckTimer;
    private int _thirdTargetX;
    private Death _death;
    private int _burstWait = 1;
    private int _nextBurst;
    private int _nextDoorDust;
    private int _fadeStep;
    private int _fadeWait;
    private Pixels _brainFall;
    private bool _brainDetached;
    private Color _headTint = Colors.White;
    private RowCrumble? _rot;
    private bool _escapeTextDone;

    private void ReadDeathValues(DataNode data)
    {
        _deathExplosions.Clear();
        _deathExplosions.AddRange(data.List("death_explosions").Select(group =>
            group.Items().Select(place => PixelOffset.FromWhole(place.Int("x"), place.Int("y"))).ToArray()));
        if (_deathExplosions.Any(group => group.Length != 4))
            throw data.Problem("gives a group of \"death_explosions\" that is not four places");
        _escapeDoorDust.Clear();
        _escapeDoorDust.AddRange(data.List("escape_door_dust").Select(place => PixelPosition.FromWhole(place.Int("x"), place.Int("y"))));
        _doorFragments.Clear();
        _doorFragments.AddRange(data.List("escape_door_fragments").Select(piece => new DoorFragment(piece.Int("down"), piece.Distance("rise"))));
        _escapeDoorPatch = data.Text("escape_door_patch");
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what == "escape_text_done")
            _escapeTextDone = true;
        else
            throw new InvalidOperationException($"Mother Brain was told \"{what}\", which she does not know.");
    }

    // ---- Her last form ----

    private bool StepThirdPhase()
    {
        switch (_third)
        {
            case Third.RecoverMakeSomeDistance:
                _form = Form.ThirdPhase;
                (_third, _timer) = (Third.RecoverSetupForFighting, 33);
                WalkBackward(Position.X.Whole - 14, "really_fast");
                return false;

            case Third.RecoverSetupForFighting:
                if (--_timer > 0)
                    return false;
                (_third, _thirdNeck, _thirdWalk) = (Third.FightingMain, ThirdNeck.Normal, ThirdWalk.TryToInchForward);
                return true;

            case Third.FightingMain:
                if (_headHealth == 0)
                {
                    (_late, _death) = (Late.Dying, Death.MoveToBackOfRoom);
                    return false;
                }

                uint roll = Random.Last.Number & 0xFFFF;
                StepThirdNeck();
                StepThirdWalk(roll);
                if (_pose != Pose.Standing || _thirdNeck == ThirdNeck.HyperBeamRecoil || roll < 32768)
                    return false;
                PlayHead((roll & 0xFF) < 128 ? "bomb_phase3" : "four_onion_rings_phase3");
                (_third, _timer) = (Third.FightingAttackCooldown, 65);
                return false;

            case Third.FightingAttackCooldown:
                if (--_timer <= 0)
                    _third = Third.FightingMain;
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's last form has no step {_third}.");
        }
    }

    /// <summary>
    /// Her walking in her last form. Left alone she earns her way to a step forward and then keeps coming; with her
    /// advance spent by Samus's shots she backs off fast, then a little more, and starts again.
    /// </summary>
    private void StepThirdWalk(uint roll)
    {
        if (_pose != Pose.Standing)
            return;
        while (true)
        {
            int x = Position.X.Whole;
            switch (_thirdWalk)
            {
                case ThirdWalk.Inactive:
                    return;

                case ThirdWalk.TryToInchForward:
                    if (_walkCredit == 0)
                    {
                        (_thirdTargetX, _thirdWalk) = (x - 14, ThirdWalk.RetreatQuickly);
                        continue;
                    }

                    _walkCredit += 32;
                    if (_walkCredit < 256)
                        return;
                    _thirdTargetX = x + 1;
                    if (x >= 128)
                    {
                        _walkCredit = 128;
                        return;
                    }

                    WalkForward(_thirdTargetX, (roll & 2) != 0 ? "medium" : "fast");
                    return;

                case ThirdWalk.RetreatQuickly:
                    if (!WalkBackward(_thirdTargetX, "really_fast"))
                        return;
                    (_thirdTargetX, _thirdWalk) = (x - 14, ThirdWalk.RetreatSlowly);
                    return;

                case ThirdWalk.RetreatSlowly:
                    if (!WalkBackward(_thirdTargetX, "fast"))
                        return;
                    (_walkCredit, _thirdWalk, _thirdTargetX) = (64, ThirdWalk.TryToInchForward, x + 1);
                    return;

                default:
                    throw new InvalidOperationException($"Mother Brain's last walk has no step {_thirdWalk}.");
            }
        }
    }

    /// <summary>Her neck in her last form: it bobs, and after the hyper beam throws her head back, swings down again for a volley.</summary>
    private void StepThirdNeck()
    {
        while (true)
        {
            switch (_thirdNeck)
            {
                case ThirdNeck.Inactive:
                    return;

                case ThirdNeck.Normal:
                    (_neckTurn, _lowerMove, _upperMove) = (NeckBobbing, NeckMove.BobDown, NeckMove.BobUp);
                    _thirdNeck = ThirdNeck.Inactive;
                    return;

                case ThirdNeck.StartRecoilRecovery:
                    (_neckMoves, _neckTurn, _lowerMove, _upperMove) = (true, NeckFast, NeckMove.Lower, NeckMove.Lower);
                    (_thirdNeck, _thirdNeckTimer) = (ThirdNeck.RecoilRecovery, 17);
                    continue;

                case ThirdNeck.RecoilRecovery:
                    if (--_thirdNeckTimer > 0)
                        return;
                    PlayHead("four_onion_rings_phase3");
                    _thirdNeck = ThirdNeck.Normal;
                    return;

                case ThirdNeck.StartHyperBeamRecoil:
                    _neckMoves = true;
                    PlayHead("hyper_beam_recoil");
                    _brainShake = 50;
                    (_neckTurn, _lowerMove, _upperMove) = (NeckRecoil, NeckMove.Raise, NeckMove.Raise);
                    (_thirdNeck, _thirdNeckTimer) = (ThirdNeck.HyperBeamRecoil, 12);
                    continue;

                case ThirdNeck.HyperBeamRecoil:
                    if (--_thirdNeckTimer > 0)
                        return;
                    _neckTurn = NeckBobbing;
                    _thirdNeck = ThirdNeck.StartRecoilRecovery;
                    return;

                default:
                    throw new InvalidOperationException($"Mother Brain's neck has no step {_thirdNeck} in her last form.");
            }
        }
    }

    // ---- Her death ----

    private bool StepDeath(Player samus)
    {
        switch (_death)
        {
            case Death.MoveToBackOfRoom:
                (_bodyTouches, _brainTouches, _neckTouches) = (false, false, false);
                foreach (Collider part in GetChildren().OfType<Collider>())
                    part.Enabled = false;
                if (!WalkBackward(40, "medium"))
                    return false;
                (_death, _timer) = (Death.IdleWhilstExploding, 129);
                return true;

            case Death.IdleWhilstExploding:
                Burst(mixed: false);
                if (--_timer <= 0)
                    _death = Death.StumbleToMiddleOfRoom;
                return false;

            case Death.StumbleToMiddleOfRoom:
                Burst(mixed: false);
                if (!WalkForward(96, "really_fast"))
                    return false;
                PlayHead("dying_drool");
                (_lowerMove, _upperMove, _neckTurn) = (NeckMove.Lower, NeckMove.Lower, NeckFast);
                (_death, _timer) = (Death.BrainComesAway, 33);
                return false;

            case Death.BrainComesAway:
                Burst(mixed: false);
                if (--_timer > 0)
                    return false;
                (_lowerMove, _upperMove) = (NeckMove.None, NeckMove.None);
                (_drools, _breathes, _showsHealth) = (false, false, false);
                (_brainDetached, _headTint) = (true, Colors.White);
                _burstWait = 1;
                _death = Death.ExplodesBeforeBodyFades;
                return true;

            case Death.ExplodesBeforeBodyFades:
                Burst(mixed: true);
                (_fadeStep, _fadeWait) = (0, 1);
                _death = Death.FadeOutBody;
                return true;

            case Death.FadeOutBody:
                Burst(mixed: true);
                // Her body flickers as it fades.
                Visual?.SetShown(Updates % 2 != 0);
                if (--_fadeWait > 0)
                    return false;
                _fadeWait = 17;
                _tint = new Color(1f, 1f, 1f, 1f - _fadeStep / 16f);
                if (++_fadeStep <= 16)
                    return false;
                Visual?.SetShown(false);
                (_death, _timer) = (Death.FinalFewExplosions, 17);
                return false;

            case Death.FinalFewExplosions:
                Burst(mixed: true);
                if (--_timer <= 0)
                    _death = Death.RealizeDecapitation;
                return false;

            case Death.RealizeDecapitation:
                PlayHead("decapitated");
                _onNeck = false;
                _brainFall = Pixels.Zero;
                _death = Death.BrainFallsToGround;
                return true;

            case Death.BrainFallsToGround:
                _brainFall += Pixels.FromNumber(0.125);
                int fallen = _head.Y.Whole + _brainFall.Whole;
                if (fallen < 196)
                {
                    _head = new PixelPosition(_head.X, Pixels.FromWhole(fallen));
                    return false;
                }

                Room.Shake(1, 20, ShakeWay.Both, ShakeReach.Tiles);
                _head = new PixelPosition(_head.X, Pixels.FromWhole(196));
                (_death, _timer) = (Death.BrainSettles, 6);
                return false;

            case Death.BrainSettles:
                if (--_timer <= 0)
                    (_death, _timer) = (Death.LiesStillBeforeGreying, 33);
                return false;

            case Death.LiesStillBeforeGreying:
                if (--_timer > 0)
                    return false;
                (_fadeStep, _fadeWait) = (0, 1);
                _death = Death.FadeToGrey;
                return false;

            case Death.FadeToGrey:
                if (--_fadeWait > 0)
                    return false;
                _fadeWait = DrainedGreyStepUpdates;
                _headTint = GreyTint(_fadeStep);
                if (++_fadeStep <= GreySteps)
                    return false;
                PlayHead("corpse");
                (_death, _timer) = (Death.CorpseTipsOver, 257);
                return false;

            case Death.CorpseTipsOver:
                if (--_timer > 0)
                    return false;
                _rot = null;
                _death = Death.CorpseRotsAway;
                return false;

            case Death.CorpseRotsAway:
                if (RotCorpse())
                    return false;
                _headShown = false;
                Room.Session.Sounds.RequestMusic("");
                Compatibility.CartridgeSounds.Of(Room.Session).AskMusicSet("ridley_and_draygon_battles");
                (_death, _timer) = (Death.PauseBeforeEscape, 21);
                return true;

            case Death.PauseBeforeEscape:
                if (--_timer > 0)
                    return false;
                _head = PixelPosition.FromWhole(0, 0);
                (_death, _timer) = (Death.HoldBeforeEscape, 7);
                return false;

            case Death.HoldBeforeEscape:
                if (--_timer > 0)
                    return false;
                (_death, _timer) = (Death.StartEscape, 2);
                return true;

            case Death.StartEscape:
                if (--_timer > 0)
                    return false;
                if (EscapeMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(EscapeMusic);
                Room.ChangeShake(2, ShakeWay.Both, ShakeReach.Tiles);
                Room.RunShakeOn();
                _escapeTextDone = false;
                foreach (NodePath path in ToldWhenEscapeBegins)
                {
                    (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"Mother Brain is to tell \"{path}\" that her alarm begins, but that is nothing that can be told."))
                        .Triggered("start");
                }

                _death = Death.TypeOutEscapeText;
                return false;

            case Death.TypeOutEscapeText:
                KeepShaking();
                // She waits until she is told the warning has been written out, or until what writes it has done.
                if (!_escapeTextDone && ToldWhenEscapeBegins.Any(path => GetNode(path) is RoomSequence { IsRunning: true }))
                    return false;
                // The door's dust goes on from the wait her last bursts left.
                (_death, _timer) = (Death.EscapeDoorBursts, 33);
                return false;

            case Death.EscapeDoorBursts:
                KeepShaking();
                BurstAtEscapeDoor();
                if (--_timer > 0)
                    return false;
                if (samus.State is DrainedState)
                    samus.Release();
                Room.Session.Progress.Record(StoryEvents.EscapeStarted);
                Room.MarkGone(Name);
                Room.Session.Service<Escape>().Begin(Escape.ZebesTime);
                _death = Death.BlowUpEscapeDoor;
                return false;

            case Death.BlowUpEscapeDoor:
                KeepShaking();
                foreach (DoorFragment piece in _doorFragments)
                {
                    PixelPosition at = PixelPosition.FromWhole(EscapeDoorPlace.X, EscapeDoorPlace.Y + piece.Down);
                    if (DoorFragmentEffect.Length > 0 && LeaveHoldingPlace(DoorFragmentEffect, at) is { } fragment)
                        _flyingFragments.Add(new FlyingFragment(fragment, new PixelVelocity(Pixels.FromWhole(5), piece.Rise)));
                }

                Room.Terrain.ApplyPatch(_escapeDoorPatch);
                _death = Death.KeepEarthquakeGoing;
                return false;

            case Death.KeepEarthquakeGoing:
                KeepShaking();
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's death has no step {_death}.");
        }
    }

    /// <summary>
    /// The room does not stop shaking once the alarm has started: whenever something has given its shake a length
    /// and that has run out, it shakes on again with no length, with whatever look the room's shake has by then.
    /// </summary>
    private void KeepShaking()
    {
        if (Room.ShakeStrength == 0)
            Room.RunShakeOn();
    }

    /// <summary>
    /// She bursts every so often at the next group of places in turn: smoke at two of them early on; later, at all
    /// four, smoke mostly, a small blast sometimes and now and then a big one.
    /// </summary>
    private void Burst(bool mixed)
    {
        if (--_burstWait > 0 || _deathExplosions.Count == 0)
            return;
        _burstWait = mixed ? 9 : 17;
        PixelOffset[] group = _deathExplosions[_nextBurst];
        _nextBurst = (_nextBurst + 1) % _deathExplosions.Count;
        for (int place = 0; place < (mixed ? 4 : 2); place++)
        {
            string effect = SmokeEffect;
            if (mixed)
            {
                int roll = Random.Draw().Part(13, 3);
                effect = roll < 2 ? ExplosionEffect : roll < 7 ? SmokeEffect : BigExplosionEffect;
            }

            // The blasts of her death lie behind everything that stands in the room, as the cartridge draws them.
            if (effect.Length > 0 && LeaveHoldingPlace(effect, Position + group[place], Standing.Hindmost) is { } blast)
                _bodyBlasts.Add((blast, group[place]));
            Sound(ExplosionSound, SoundPriority.Low);
        }
    }

    /// <summary>Her bursts stay on her body as it moves.</summary>
    private void StepBodyBlasts()
    {
        _bodyBlasts.RemoveAll(blast => !IsInstanceValid(blast.Blast) || Room.IsLeaving(blast.Blast));
        foreach ((Effect blast, PixelOffset from) in _bodyBlasts)
            blast.Position = Position + from;
    }

    /// <summary>
    /// Her corpse crumbles from the bottom up, a row at a time, each leaving a puff of dust beside it. Returns whether
    /// any of it is left.
    /// </summary>
    private bool RotCorpse()
    {
        _rot ??= new RowCrumble(CorpseRows);
        _rot.Step(() =>
        {
            Leave(CorpseDustEffect, _head + PixelOffset.FromWhole(-16 + Random.Last.Part(0, 5), 16));
            if (Room.Session.EnemyPasses % 8 == 0)
                Sound(CorpseDustSound);
        });
        GetNodeOrNull<ActorVisual>("Head")?.ShowRows(_rot.Rows);
        return !_rot.IsOver;
    }

    /// <summary>Dust and blasts go off at the way out, at its places in turn.</summary>
    private void BurstAtEscapeDoor()
    {
        if (--_burstWait > 0 || _escapeDoorDust.Count == 0)
            return;
        _burstWait = 5;
        PixelPosition at = _escapeDoorDust[_nextDoorDust];
        _nextDoorDust = (_nextDoorDust + 1) % _escapeDoorDust.Count;
        Leave((Random.Draw().Number & 0xFFFF) < 16384 ? SmokeEffect : ExplosionEffect, at);
        Sound(ExplosionSound, SoundPriority.Low);
    }

    /// <summary>The pieces of the way out fly into the room, slowing and falling, and end in dust.</summary>
    private void StepFlyingFragments()
    {
        Pixels drag = Pixels.FromNumber(1 / 16.0), gravity = Pixels.FromNumber(0.125);
        foreach (FlyingFragment piece in _flyingFragments.ToList())
        {
            Pixels across = piece.Speed.X.Abs() - drag;
            across = across.IsNegative ? Pixels.Zero : piece.Speed.X.IsNegative ? -across : across;
            piece.Speed = new PixelVelocity(across, piece.Speed.Y + gravity);
            piece.Sprite.Position += piece.Speed.OverOneUpdate;
            if (--piece.Left > 0)
                continue;
            // The piece is gone before its dust is left, which may so take the place it held.
            Room.Remove(piece.Sprite);
            _flyingFragments.Remove(piece);
            Leave(DustEffect, piece.Sprite.Position - PixelOffset.FromWhole(0, 4));
        }
    }
}
