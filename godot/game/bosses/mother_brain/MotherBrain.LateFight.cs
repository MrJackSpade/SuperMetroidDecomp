using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// What follows once Mother Brain's second health is gone. She gathers a great beam and fires it: it throws Samus
/// to the far wall, pins her there and drains her energy and her ammunition, and lets her drop. With Samus still
/// strong she does it again; with Samus weak she toys with her, and then gathers a last beam, which the giant
/// Metroid takes instead. It drains her until she stands grey and lifeless, heals Samus, and shields her when
/// Mother Brain wakes and turns her rings on it.
/// </summary>
/// <remarks>
/// Her brain plays "charging_rainbow_beam", "firing_rainbow_beam", "two_onion_rings_phase2", "dying_drool",
/// "stretching_phase3", "attacking_baby_metroid" and "four_onion_rings_phase3" here, with the cues
/// "aim_rings_at_baby", "count_baby_attack", "reset_baby_attacks" and "baby_attack_sound". Her "Beam" is a polygon
/// drawn as the beam. Her data gives "rainbow_explosions", the places from Samus of the blasts of the beam.
/// </remarks>
public partial class MotherBrain
{
    /// <summary>The scene of the giant Metroid that comes to Samus's aid.</summary>
    [Export]
    public PackedScene? Baby { get; set; }

    /// <summary>
    /// Her room's colours with its lights out, and at each step of their coming back, as her data gives them
    /// ("room_lights": "out", and the list "back", whose last stays).
    /// </summary>
    private Engine.Ui.ColourChange _lightsOut = Engine.Ui.ColourChange.None;
    private readonly List<Engine.Ui.ColourChange> _lightsBack = [];

    /// <summary>The name her room's lights are shown under.</summary>
    private const string RoomLights = "MotherBrainLights";

    /// <summary>What is told to "start" as the alarm of her death begins: the room sequence that writes the warning out, which she waits for.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenEscapeBegins { get; set; } = [];

    /// <summary>The sound of her great beam while it fires, by name.</summary>
    [Export, DataValue("rainbow_beam_sound")]
    public string RainbowBeamSound { get; set; } = "";

    /// <summary>The sound that takes the place of her great beam's when it stops.</summary>
    [Export, DataValue("rainbow_beam_stop_sound")]
    public string RainbowBeamStopSound { get; set; } = "";

    /// <summary>The sound that takes the place of her gathering her beam when she has gathered it.</summary>
    [Export, DataValue("charge_stop_sound")]
    public string ChargeStopSound { get; set; } = "";

    /// <summary>Her cry as she attacks the giant Metroid.</summary>
    [Export, DataValue("attack_cry_sound")]
    public string AttackCrySound { get; set; } = "";

    /// <summary>With at least this much energy Samus can get up after the beam.</summary>
    [Export, DataValue("samus_stands_energy")]
    public int SamusStandsEnergy { get; set; } = 700;

    /// <summary>With less than this much energy after the beam, she is finished off rather than fired on again.</summary>
    [Export, DataValue("finish_off_energy")]
    public int FinishOffEnergy { get; set; } = 400;

    /// <summary>Her colour once the Metroid has drained her beam out of her, before she greys.</summary>
    [Export]
    public Color DrainedTint { get; set; } = new(0.8f, 0.8f, 0.85f);

    /// <summary>How many updates each step of her colours takes as she is drained grey and as she comes back.</summary>
    [Export, DataValue("drained_grey_step_updates")]
    public int DrainedGreyStepUpdates { get; set; } = 17;

    private enum Late
    {
        RainbowBeam,
        FinishingOff,
        Drained,
        Reviving,
        ThirdPhase,
        Dying,
    }

    private enum Rainbow
    {
        ExtendNeck,
        StartCharging,
        RetractNeck,
        WaitForCharge,
        ExtendNeckDown,
        StartFiring,
        MoveSamusTowardWall,
        PinSamusToWall,
        StartDrainingSamus,
        DrainingSamus,
        FinishFiring,
        LetSamusFall,
        WaitForSamusToLand,
        LowerHead,
        DecideNextAction,
    }

    private enum FinishOff
    {
        AttackUntilSamusIsLow,
        StandUp,
        Admire,
        ChargeFinalBeam,
        BringInBaby,
        FireFinalBeam,
        FinalBeamHolding,
    }

    private enum Drain
    {
        TakenAback,
        RegainBalance,
        FiringRainbowBeam,
        RainbowBeamRunOut,
        MoveToBackOfRoom,
        SlumpOnceNeckIsRaised,
        WaitBeforeGreying,
        TransitionToGrey,
    }

    private enum Revival
    {
        InanimateGrey,
        ShowSignsOfLife,
        TransitionFromGrey,
        WakeUp,
        WakeUpStretch,
        WalkUpToBaby,
        PrepareNeckForBabyDeath,
        FinishPreparingForBabyDeath,
        MurderBabyAttack,
        MurderBabyAttackCooldown,
        PrepareForFinalBabyAttack,
        ExecuteFinalBabyAttack,
        FinalBabyAttackHolding,
    }

    /// <summary>One stage of her stagger as the Metroid drains her: how she walks, how fast her neck turns, and how long she then pauses.</summary>
    private sealed record Stagger(string Speed, int NeckTurn, int Pause, bool Forward, bool BeamRunsOut = false);

    private static readonly Stagger[] Staggers =
    [
        new("really_fast", 1280, 16, Forward: true), new("really_fast", 1280, 16, Forward: false),
        new("medium", 512, 32, Forward: true), new("medium", 512, 32, Forward: false),
        new("slow", 192, 48, Forward: true), new("slow", 192, 48, Forward: false),
        new("really_slow", 64, 64, Forward: true, BeamRunsOut: true), new("really_slow", 64, 64, Forward: false),
    ];

    /// <summary>Where Samus is thrown to and held by the beam, in the room's pixels.</summary>
    private const int BeamWallX = 235, BeamCeiling = 48, BeamFloor = 192, BeamMiddleY = 124;

    /// <summary>The narrowest and widest the beam gets and how fast it widens, in halves of a step of angle.</summary>
    private const int BeamNarrowest = 4, BeamWidest = 24, BeamWidening = 3;

    private readonly List<PixelOffset> _rainbowExplosions = [];
    private readonly List<(Effect Blast, PixelOffset From)> _followingBlasts = [];
    private Late _late;
    private Rainbow _rainbow;
    private FinishOff _finishOff;
    private Drain _drain;
    private Revival _revival;
    private int _beamWidth;

    /// <summary>The way the beam points, in 256ths of a turn from straight down toward the right.</summary>
    private int _beamAngle;

    private int _beamExplosionWait;
    private int _nextBeamExplosion;
    private bool _beamSounding;
    private PixelVelocity _samusFall;
    private bool _babyCalled;
    private MotherBrainBaby? _baby;
    private int _babyAttacks;
    private bool _isCorpse;
    private bool _hasRevived;
    private bool _showsHealth;
    private bool _rainbowColours;
    private int _rainbowColour;
    private int _stagger;
    private int _staggerPause;
    private string _staggerSpeed = "really_fast";

    /// <summary>Whether the Metroid has drained the life from her; she stays so until she wakes.</summary>
    public bool IsDrainedLifeless => _isCorpse || _hasRevived;

    private void ReadLateFightValues(DataNode data)
    {
        _rainbowExplosions.Clear();
        _rainbowExplosions.AddRange(data.List("rainbow_explosions").Select(place => PixelOffset.FromWhole(place.Int("x"), place.Int("y"))));
        ReadDeathValues(data);
    }

    /// <summary>Her second health is gone: she begins to gather her great beam.</summary>
    private void BeginLateFight()
    {
        _stage = Stage.LateFight;
        _late = Late.RainbowBeam;
        _babyCalled = false;
        BeginExtendingNeck();
    }

    private void StepLateFight(Player samus)
    {
        _rainbowColours = false;
        StepFollowingBlasts(samus);
        StepBodyBlasts();
        StepFlyingFragments();
        bool again;
        do
        {
            again = _late switch
            {
                Late.RainbowBeam => StepRainbow(samus),
                Late.FinishingOff => StepFinishOff(samus),
                Late.Drained => StepDrain(),
                Late.Reviving => StepRevival(),
                Late.ThirdPhase => StepThirdPhase(),
                Late.Dying => StepDeath(samus),
                _ => throw new InvalidOperationException($"Mother Brain's late fight has no stage {_late}."),
            };
        }
        while (again && !IsDead);

        if (IsDead)
            return;
        StepLateColours();
        ShowBeam();
    }

    /// <summary>What a hit on her head does to her advance in the late fight.</summary>
    private void OnLateFightHeadHit(Hit hit, bool missile)
    {
        bool beam = hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Charged)]);
        if (_form != Form.ThirdPhase || !beam)
        {
            _walkCredit = missile ? 0 : Math.Max(0, _walkCredit - 256);
            return;
        }

        // The hyper beam costs her more than a step; without that much to lose, it throws her head back.
        if (_walkCredit >= HyperBeamCost)
        {
            _walkCredit -= HyperBeamCost;
            return;
        }

        _walkCredit = 0;
        (_thirdNeck, _timer) = (ThirdNeck.StartHyperBeamRecoil, 1);
    }

    /// <summary>Acts on the cues of her brain's timelines that only her late fight gives.</summary>
    private void OnLateFightHeadCue(string[] parts, Player samus)
    {
        switch (parts[0])
        {
            case "aim_rings_at_baby":
                if (_baby is not { } baby || !IsInstanceValid(baby) || baby.IsDead)
                    throw new InvalidOperationException("Mother Brain aims her rings at the giant Metroid, but it is not here.");
                AimRings(baby.Position);
                break;

            case "count_baby_attack":
                _babyAttacks = Math.Min(_babyAttacks + 1, 12);
                break;

            case "reset_baby_attacks":
                _babyAttacks = 0;
                break;

            case "baby_attack_sound":
                // Her eleventh volley in a row is silent.
                if (_babyAttacks != 11)
                    Sound(AttackCrySound);
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's brain timeline \"{_headPlaying}\" gives the cue \"{string.Join(':', parts)}\", which she does not know.");
        }
    }

    // ---- The great beam ----

    private void BeginExtendingNeck()
    {
        PlayHead("neutral_phase2");
        (_neckTurn, _neckMoves, _lowerMove, _upperMove) = (NeckSlow, true, NeckMove.BobDown, NeckMove.BobUp);
        (_rainbow, _timer) = (Rainbow.StartCharging, 257);
    }

    /// <summary>Draws her head back.</summary>
    private void RetractHead() =>
        (_neckTurn, _neckMoves, _lowerMove, _upperMove) = (NeckRising, true, NeckMove.Raise, NeckMove.Lower);

    private bool StepRainbow(Player samus)
    {
        switch (_rainbow)
        {
            case Rainbow.ExtendNeck:
                BeginExtendingNeck();
                return false;

            case Rainbow.StartCharging:
                if (--_timer > 0)
                    return false;
                PlayHead("charging_rainbow_beam");
                _rainbow = Rainbow.RetractNeck;
                return true;

            case Rainbow.RetractNeck:
                if (!WalkBackward(40, "really_slow"))
                    return false;
                RetractHead();
                (_rainbow, _timer) = (Rainbow.WaitForCharge, 257);
                return true;

            case Rainbow.WaitForCharge:
                if (--_timer > 0)
                    return false;
                Sound(ChargeStopSound);
                _rainbow = Rainbow.ExtendNeckDown;
                return true;

            case Rainbow.ExtendNeckDown:
                samus.Weapons.WaitAtLeast(8);
                (_lowerMove, _upperMove, _neckTurn) = (NeckMove.Lower, NeckMove.Lower, NeckFast);
                (_rainbow, _timer) = (Rainbow.StartFiring, 17);
                return true;

            case Rainbow.StartFiring:
                WidenBeam();
                AimBeam(samus);
                // She holds her fire while one of Samus's power bombs is going off.
                if (!Room.Projectiles.OfType<PowerBomb>().Any() && --_timer <= 0)
                    StartActiveBeam(samus);
                return false;

            case Rainbow.MoveSamusTowardWall:
                ContinueBeam(samus, widening: true);
                if (ThrowSamusTowardWall(samus))
                    (_rainbow, _timer) = (Rainbow.PinSamusToWall, 1);
                return false;

            case Rainbow.PinSamusToWall:
                ContinueBeam(samus, widening: true);
                ThrowSamusTowardWall(samus);
                if (--_timer > 0)
                    return false;
                Room.Shake(3, 8);
                _rainbow = Rainbow.StartDrainingSamus;
                return false;

            case Rainbow.StartDrainingSamus:
                (_rainbow, _timer) = (Rainbow.DrainingSamus, 300);
                Room.Shake(3, 299);
                return true;

            case Rainbow.DrainingSamus:
                ContinueBeam(samus, widening: true);
                DrainSamus(samus);
                if (samus.State is not DrainedState)
                    return false;
                MoveSamusDown(samus, samus.Position.Y.Whole <= BeamMiddleY ? Pixels.FromNumber(0.25) : Pixels.FromNumber(-0.25));
                if (--_timer <= 0)
                    _rainbow = Rainbow.FinishFiring;
                return false;

            case Rainbow.FinishFiring:
                ContinueBeam(samus, widening: false);
                _beamWidth -= BeamWidening;
                if (_beamWidth >= BeamNarrowest)
                    return false;
                _beamWidth = BeamNarrowest;
                _samusFall = new PixelVelocity(Pixels.FromWhole(-1), Pixels.Zero);
                StopBeamSound();
                _tint = Colors.White;
                Room.Shake(0, 0);
                samus.Weapons.WaitAtLeast(8);
                _rainbow = Rainbow.LetSamusFall;
                return false;

            case Rainbow.LetSamusFall:
                if (samus.State is DrainedState falling)
                {
                    falling.Put(DrainedState.Pose.Kneeling);
                    falling.IsCarried = true;
                }

                _rainbow = Rainbow.WaitForSamusToLand;
                return true;

            case Rainbow.WaitForSamusToLand:
                if (DropSamus(samus))
                {
                    if (samus.State is DrainedState landed)
                        landed.IsCarried = false;
                    _rainbow = Rainbow.LowerHead;
                }

                return false;

            case Rainbow.LowerHead:
                (_rainbow, _timer) = (Rainbow.DecideNextAction, 129);
                return false;

            case Rainbow.DecideNextAction:
                if (--_timer > 0)
                    return false;
                if (samus.Inventory.Amount(Inventory.Energy) >= FinishOffEnergy)
                {
                    _rainbow = Rainbow.ExtendNeck;
                    return false;
                }

                WalkForward(Position.X.Whole + 16, "really_slow");
                _babyCalled = false;
                (_late, _finishOff) = (Late.FinishingOff, FinishOff.AttackUntilSamusIsLow);
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's great beam has no step {_rainbow}.");
        }
    }

    /// <summary>Whether the great beam is firing.</summary>
    private bool BeamIsFiring => _late == Late.RainbowBeam && _rainbow is >= Rainbow.MoveSamusTowardWall and <= Rainbow.FinishFiring;

    /// <summary>The beam takes hold of Samus.</summary>
    private void StartActiveBeam(Player samus)
    {
        PlayHead("firing_rainbow_beam");
        _beamWidth = BeamNarrowest;
        (_nextBeamExplosion, _beamExplosionWait) = (0, 1);
        _beamSounding = false;
        (_neckTurn, _neckMoves, _lowerMove, _upperMove) = (NeckSlow, true, NeckMove.BobDown, NeckMove.BobUp);
        samus.ChangeState(new DrainedState(DrainedState.Pose.Pinned, samus.Inventory.Amount(Inventory.Energy) >= SamusStandsEnergy));
        _rainbow = Rainbow.MoveSamusTowardWall;
    }

    private void WidenBeam() => _beamWidth = Math.Min(BeamWidest, _beamWidth + BeamWidening);

    /// <summary>Turns the beam toward Samus.</summary>
    private void AimBeam(Player samus)
    {
        // The cartridge reads the way to Samus from whole pixels, clockwise from straight up as its grapple beam does,
        // and counts the beam's angle back from a half turn.
        PixelPosition from = _head + PixelOffset.FromWhole(16, 4);
        int bearing = CartridgeAngles.AngleClockwiseFromUp(samus.Position.X.Whole - from.X.Whole, samus.Position.Y.Whole - from.Y.Whole);
        _beamAngle = Turns.Wrap(Turns.PerTurn / 2 - bearing);
    }

    /// <summary>What the beam does every update it fires: its sound, her colours, its aim, and a blast on Samus every few updates.</summary>
    private void ContinueBeam(Player samus, bool widening)
    {
        if (!_beamSounding)
        {
            _beamSounding = true;
            if (RainbowBeamSound.Length > 0)
                Room.Session.Sounds.Request(RainbowBeamSound, channel: "beam");
        }

        _rainbowColours = true;
        if (widening)
            WidenBeam();
        AimBeam(samus);
        if (--_beamExplosionWait > 0 || _rainbowExplosions.Count == 0)
            return;
        _beamExplosionWait = 9;
        PixelOffset from = _rainbowExplosions[_nextBeamExplosion];
        _nextBeamExplosion = (_nextBeamExplosion + 1) % _rainbowExplosions.Count;
        if (ExplosionEffect.Length > 0 && LeaveHoldingPlace(ExplosionEffect, samus.Position + from) is { } blast)
            _followingBlasts.Add((blast, from));
        Sound(ExplosionSound);
    }

    private void StopBeamSound()
    {
        _beamSounding = false;
        if (RainbowBeamStopSound.Length > 0)
            Room.Session.Sounds.Request(RainbowBeamStopSound, channel: "beam");
    }

    /// <summary>The blasts of the beam stay on Samus as she is thrown about.</summary>
    private void StepFollowingBlasts(Player samus)
    {
        _followingBlasts.RemoveAll(blast => !IsInstanceValid(blast.Blast) || Room.IsLeaving(blast.Blast));
        foreach ((Effect blast, PixelOffset from) in _followingBlasts)
            blast.Position = samus.Position + from;
    }

    /// <summary>Throws Samus along the beam toward the far wall. Returns whether she is against it.</summary>
    private bool ThrowSamusTowardWall(Player samus)
    {
        if (samus.State is not DrainedState)
            return true;
        Pixels speed = Pixels.FromWhole(16);
        if (MoveSamusAcross(samus, speed))
            return true;
        // Down by the push's speed along the beam's sine a quarter turn on, in 256ths of a pixel.
        MoveSamusDown(samus, From256ths(CartridgeAngles.SpeedAlong(In256ths(speed), _beamAngle + Turns.PerTurn / 4)));
        return false;
    }

    /// <summary>Moves Samus across by the beam's push. Returns whether she has reached the wall.</summary>
    private static bool MoveSamusAcross(Player samus, Pixels by)
    {
        PixelPosition moved = samus.Position + new PixelOffset(by, Pixels.Zero);
        // Every way out of the push tells the view she stood where it leaves her: the view sees no whole pixel of it.
        if (moved.X.Whole >= BeamWallX)
        {
            samus.Position = new PixelPosition(Pixels.FromWhole(BeamWallX), samus.Position.Y);
            samus.Room.Session.Camera.TakePlayerToHaveStoodAtX(BeamWallX);
            return true;
        }

        samus.Position = moved;
        samus.Room.Session.Camera.TakePlayerToHaveStoodAtX(moved.X.Whole);
        return false;
    }

    /// <summary>Moves Samus up or down by the beam's push. Returns whether she has reached the ceiling or the floor.</summary>
    private static bool MoveSamusDown(Player samus, Pixels by)
    {
        PixelPosition moved = samus.Position + new PixelOffset(Pixels.Zero, by);
        bool high = moved.Y.Whole < BeamCeiling;
        if (high || moved.Y.Whole >= BeamFloor)
        {
            samus.Position = new PixelPosition(samus.Position.X, Pixels.FromWhole(high ? BeamCeiling : BeamFloor));
            samus.Room.Session.Camera.TakePlayerToHaveStoodAtY(high ? BeamCeiling : BeamFloor);
            return true;
        }

        samus.Position = moved;
        samus.Room.Session.Camera.TakePlayerToHaveStoodAtY(moved.Y.Whole);
        return false;
    }

    /// <summary>The beam takes Samus's energy every update, and her missiles and bombs as it goes.</summary>
    private void DrainSamus(Player samus)
    {
        Inventory inventory = samus.Inventory;
        int energy = inventory.Amount(Inventory.Energy);
        int taken = inventory.IsOn(Equipment.VariaSuit) ? 1 : 2;
        inventory.Lose(Inventory.Energy, Math.Min(energy, taken));
        if (Room.Session.EnemyPasses % 4 == 0)
        {
            inventory.Lose(Amounts.Missiles, Math.Min(1, inventory.Amount(Amounts.Missiles)));
            inventory.Lose(Amounts.SuperMissiles, Math.Min(1, inventory.Amount(Amounts.SuperMissiles)));
        }

        inventory.Lose(Amounts.PowerBombs, Math.Min(1, inventory.Amount(Amounts.PowerBombs)));
        if (inventory.Amount(Inventory.Energy) == 0)
            samus.Sap(new Hit(0, HitSide.Enemies, HitKind.Contact) { Source = this });
    }

    /// <summary>Lets Samus drop from the wall, drifting back from it. Returns whether she has landed.</summary>
    private bool DropSamus(Player samus)
    {
        if (samus.State is not DrainedState)
            return true;
        Pixels across = _samusFall.X + Pixels.FromNumber(1 / 128.0);
        _samusFall = _samusFall.WithX(across.IsNegative ? across : Pixels.Zero);
        MoveSamusAcross(samus, _samusFall.X);
        _samusFall = _samusFall.WithY(_samusFall.Y + Pixels.FromNumber(3 / 32.0));
        return MoveSamusDown(samus, _samusFall.Y);
    }

    /// <summary>Shows the beam as a wedge from her mouth, as wide as it has grown.</summary>
    private void ShowBeam()
    {
        if (GetNodeOrNull<Polygon2D>("Beam") is not { } beam)
            return;
        beam.Visible = BeamIsFiring;
        if (!beam.Visible)
            return;
        PixelOffset mouth = _head + PixelOffset.FromWhole(14, 5) - Position;
        int half = _beamWidth / 2 / 2;
        Pixels reach = Pixels.FromWhole(512);
        Vector2 Edge(int angle) => new(
            mouth.X.Whole + Turns.Sine(Turns.Wrap(angle), reach).Whole,
            mouth.Y.Whole + Turns.Cosine(Turns.Wrap(angle), reach).Whole);
        beam.Polygon = [new Vector2(mouth.X.Whole, mouth.Y.Whole), Edge(_beamAngle - half), Edge(_beamAngle + half)];
        beam.Color = Color.FromHsv(_rainbowColour % 12 / 12f, 0.5f, 1f);
    }

    /// <summary>Her colours: every colour in turn while her beam fires, and by her health once she has come back.</summary>
    private void StepLateColours()
    {
        if (_rainbowColours)
        {
            if (Updates % 4 >= 2)
            {
                _rainbowColour++;
                _tint = Color.FromHsv(_rainbowColour % 12 / 12f, 0.5f, 1.2f);
            }

            return;
        }

        if (_showsHealth && _hasRevived)
            _tint = HealthTint();
    }

    /// <summary>Her colour by how hurt she is in her last form: paler and redder as her health goes.</summary>
    private Color HealthTint() => _headHealth switch
    {
        >= 9000 => Colors.White,
        >= 5400 => new Color(1f, 0.9f, 0.9f),
        >= 1800 => new Color(1f, 0.75f, 0.75f),
        _ => new Color(1f, 0.6f, 0.6f),
    };

    // ---- Finishing Samus off ----

    private bool StepFinishOff(Player samus)
    {
        switch (_finishOff)
        {
            case FinishOff.AttackUntilSamusIsLow:
                ToyWithSamus(samus);
                return false;

            case FinishOff.StandUp:
                if (!TryStand())
                    return false;
                (_finishOff, _timer) = (FinishOff.Admire, 17);
                return true;

            case FinishOff.Admire:
                if (--_timer > 0)
                    return false;
                PlayHead("stretching_phase2");
                (_finishOff, _timer) = (FinishOff.ChargeFinalBeam, 257);
                return false;

            case FinishOff.ChargeFinalBeam:
                if (--_timer > 0)
                    return false;
                PlayHead("charging_rainbow_beam");
                (_finishOff, _timer) = (FinishOff.BringInBaby, 4);
                return true;

            case FinishOff.BringInBaby:
                if (--_timer > 0)
                    return false;
                RetractHead();
                CallBaby();
                (_finishOff, _timer) = (FinishOff.FireFinalBeam, 257);
                return false;

            case FinishOff.FireFinalBeam:
                if (--_timer > 0)
                    return false;
                PlayHead("firing_rainbow_beam");
                (_lowerMove, _upperMove, _neckTurn) = (NeckMove.Lower, NeckMove.Lower, NeckFast);
                Sound(ChargeStopSound);
                _rainbowColour = 0;
                _finishOff = FinishOff.FinalBeamHolding;
                return false;

            case FinishOff.FinalBeamHolding:
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's finishing of Samus has no step {_finishOff}.");
        }
    }

    /// <summary>
    /// With Samus on her knees she mostly watches, now and then leaning down over her or standing again, and now and
    /// then spitting rings or a bomb, though never what would kill. Once four rings would nearly do it, she stops.
    /// </summary>
    private void ToyWithSamus(Player samus)
    {
        int energy = samus.Inventory.Amount(Inventory.Energy);
        if (samus.EnergyLostTo(HitOf("ring")) * 4 + 20 >= energy)
        {
            _finishOff = FinishOff.StandUp;
            return;
        }

        uint roll = Random.Last.Number & 0xFFFF;
        if ((roll & 0xFFF) < 4000)
        {
            if (Updates % 32 == 0)
                MaybeChangePosture(roll);
            return;
        }

        bool bomb = samus.EnergyLostTo(HitOf("bomb")) + 20 < energy && (roll & 0xFFF) >= 4080;
        PlayHead(bomb ? "bomb_phase2" : "two_onion_rings_phase2");
    }

    /// <summary>The hit of one of her shots.</summary>
    private Hit HitOf(string shot)
    {
        ShotType type = Shot(shot);
        return new Hit(type.Damage, HitSide.Enemies, [.. type.Kinds]) { Source = this };
    }

    /// <summary>One time in four she changes how she stands: leaning down if upright, and upright if leaning.</summary>
    private void MaybeChangePosture(uint roll)
    {
        if ((roll & 0xFF) < 192)
            return;
        if (_pose == Pose.Standing)
            Play("lean_down", restart: true);
        else if (_pose == Pose.Leaning)
            TryStand();
    }

    /// <summary>Brings the giant Metroid into the room.</summary>
    private void CallBaby()
    {
        _babyCalled = true;
        if (Baby is null)
            throw new InvalidOperationException("Mother Brain has no scene for the giant Metroid that comes to Samus's aid.");
        _baby = Baby.Instantiate() as MotherBrainBaby
            ?? throw new InvalidOperationException("The scene Mother Brain has for the giant Metroid does not have one at its root.");
        _baby.ComeFor(this);
        Room.Spawn(_baby);
    }

    // ---- What the giant Metroid tells her ----

    /// <summary>The Metroid has flown into her face: she stumbles back a step.</summary>
    internal void StumbleFromBaby() => WalkBackward(Position.X.Whole - 1, "really_fast");

    /// <summary>The Metroid has latched on and begun to drain her.</summary>
    internal void BeDrainedByBaby()
    {
        (_late, _drain) = (Late.Drained, Drain.TakenAback);
        _beamSounding = true;
        if (RainbowBeamSound.Length > 0)
            Room.Session.Sounds.Request(RainbowBeamSound, channel: "beam");
    }

    /// <summary>The Metroid, hurt, has let go of Samus: she backs off for her last volley at it.</summary>
    internal void PrepareForLastBabyAttack() => (_late, _revival) = (Late.Reviving, Revival.PrepareForFinalBabyAttack);

    /// <summary>The Metroid is coming at her: she fires her last volley at it.</summary>
    internal void FireLastBabyAttack() => (_late, _revival) = (Late.Reviving, Revival.ExecuteFinalBabyAttack);

    /// <summary>The Metroid is dead and Samus has its gift: her last form's fight begins.</summary>
    internal void BeginThirdPhase()
    {
        (_late, _third) = (Late.ThirdPhase, Third.RecoverMakeSomeDistance);
        _babyCalled = false;
        _baby = null;
    }

    /// <summary>Puts her room's lights out: the room's own pictures in the colours her data gives for it.</summary>
    internal void PutLightsOut() => Room.Recolour(RoomLights, _lightsOut);

    /// <summary>How many steps her room's lights come back by.</summary>
    internal int LightsBackSteps => _lightsBack.Count;

    /// <summary>Shows one step of her room's lights coming back; the last stays.</summary>
    internal void ShowLightsBack(int step) => Room.Recolour(RoomLights, _lightsBack[step]);

    // ---- Drained by the giant Metroid ----

    private bool StepDrain()
    {
        switch (_drain)
        {
            case Drain.TakenAback:
                _form = Form.Drained;
                (_lowerMove, _upperMove, _neckTurn) = (NeckMove.Raise, NeckMove.Raise, NeckTakenAback);
                (_drain, _timer) = (Drain.RegainBalance, 49);
                return true;

            case Drain.RegainBalance:
                _rainbowColours = true;
                if (--_timer > 0)
                    return false;
                (_stagger, _staggerPause, _staggerSpeed) = (0, 0, "really_fast");
                (_lowerMove, _upperMove) = (NeckMove.BobDown, NeckMove.BobUp);
                _drain = Drain.FiringRainbowBeam;
                return false;

            case Drain.FiringRainbowBeam:
                if (_brainShake == 0)
                    _brainShake = 50;
                _rainbowColours = true;
                StepStagger();
                Stagger stage = Staggers[_stagger];
                (_staggerSpeed, _neckTurn) = (stage.Speed, stage.NeckTurn);
                if (!stage.BeamRunsOut)
                    return false;
                // Her beam gives out, and she is left pale.
                _rainbowColours = false;
                _tint = DrainedTint;
                StopBeamSound();
                _drain = Drain.RainbowBeamRunOut;
                return false;

            case Drain.RainbowBeamRunOut:
                if (!StepStagger())
                    return false;
                (_neckTurn, _lowerMove, _upperMove) = (NeckSlow, NeckMove.Raise, NeckMove.Raise);
                PlayHead("dying_drool");
                _drain = Drain.MoveToBackOfRoom;
                return true;

            case Drain.MoveToBackOfRoom:
                if (!WalkBackward(40, "really_slow"))
                    return false;
                _drain = Drain.SlumpOnceNeckIsRaised;
                _upperMove = NeckMove.None;
                return true;

            case Drain.SlumpOnceNeckIsRaised:
                if (_lowerMove != NeckMove.None || _upperMove != NeckMove.None)
                    return false;
                _drools = false;
                if (_pose != Pose.Standing)
                    return false;
                Play("crouch_fast", restart: true);
                (_drain, _timer) = (Drain.WaitBeforeGreying, 65);
                return false;

            case Drain.WaitBeforeGreying:
                if (--_timer > 0)
                    return false;
                // Her first step toward grey shows a step's length from now, counting this update.
                (_greyStep, _greyWait) = (0, DrainedGreyStepUpdates);
                _drain = Drain.TransitionToGrey;
                return true;

            case Drain.TransitionToGrey:
                // Each step shows for its updates, the last as well: she lies grey only on the update after the last
                // has had them.
                if (--_greyWait > 0)
                    return false;
                if (_greyStep < GreySteps)
                {
                    _greyWait = DrainedGreyStepUpdates;
                    _tint = DrainedTint.Lerp(GreyTint(GreySteps), (float)++_greyStep / GreySteps);
                    return false;
                }

                _headHealth = ThirdPhaseHealth;
                _isCorpse = true;
                _breathes = false;
                _form = Form.SecondPhase;
                (_late, _revival) = (Late.Reviving, Revival.InanimateGrey);
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's draining has no step {_drain}.");
        }
    }

    /// <summary>
    /// Her stagger: forward and back across the room, more slowly each time, with a pause at each end. Returns
    /// whether the last of it is done.
    /// </summary>
    private bool StepStagger()
    {
        if (_staggerPause > 0)
        {
            if (--_staggerPause > 0)
                return false;
            if (_stagger + 1 >= Staggers.Length)
                return true;
            _stagger++;
            return false;
        }

        Stagger stage = Staggers[_stagger];
        bool reached = stage.Forward ? WalkForward(72, _staggerSpeed) : WalkBackward(40, _staggerSpeed);
        if (reached)
            _staggerPause = stage.Pause;
        return false;
    }

    // ---- Coming back to life ----

    private bool StepRevival()
    {
        switch (_revival)
        {
            case Revival.InanimateGrey:
                (_revival, _timer) = (Revival.ShowSignsOfLife, 769);
                return false;

            case Revival.ShowSignsOfLife:
                if (--_timer > 0)
                    return false;
                (_breathes, _drools) = (true, true);
                (_greyStep, _greyWait) = (GreySteps, 225);
                _revival = Revival.TransitionFromGrey;
                return true;

            case Revival.TransitionFromGrey:
                // Each step shows for its updates, the last as well: she wakes only on the update after the last has
                // had them.
                if (--_greyWait > 0)
                    return false;
                if (_greyStep > 0)
                {
                    _greyWait = DrainedGreyStepUpdates;
                    _tint = GreyTint(--_greyStep);
                    return false;
                }

                _revival = Revival.WakeUp;
                return true;

            case Revival.WakeUp:
                if (!TryStand())
                    return false;
                (_lowerMove, _upperMove, _neckTurn, _neckMoves) = (NeckMove.Lower, NeckMove.Lower, NeckFast, true);
                (_revival, _timer) = (Revival.WakeUpStretch, 17);
                return true;

            case Revival.WakeUpStretch:
                if (--_timer > 0)
                    return false;
                PlayHead("stretching_phase3");
                (_revival, _timer) = (Revival.WalkUpToBaby, 129);
                return true;

            case Revival.WalkUpToBaby:
                if (_timer > 0 && --_timer > 0)
                    return false;
                WalkForward(80, "fast");
                if (Position.X.Whole <= 80)
                    return false;
                (_isCorpse, _hasRevived, _showsHealth) = (false, true, true);
                _revival = Revival.PrepareNeckForBabyDeath;
                return false;

            case Revival.PrepareNeckForBabyDeath:
                _babyAttacks = 0;
                (_neckMoves, _lowerMove, _upperMove, _neckTurn) = (true, NeckMove.BobDown, NeckMove.BobUp, NeckSlow);
                _revival = Revival.FinishPreparingForBabyDeath;
                return true;

            case Revival.FinishPreparingForBabyDeath:
                if (!TryStand())
                    return false;
                _revival = Revival.MurderBabyAttack;
                WalkForward(80, "really_slow");
                return true;

            case Revival.MurderBabyAttack:
                uint roll = Random.Last.Number & 0xFFFF;
                MaybeChangePosture(roll);
                if (roll < 32768)
                    return false;
                PlayHead(_babyCalled ? "attacking_baby_metroid" : "four_onion_rings_phase3");
                (_revival, _timer) = (Revival.MurderBabyAttackCooldown, 65);
                return false;

            case Revival.MurderBabyAttackCooldown:
                if (--_timer <= 0)
                    _revival = Revival.MurderBabyAttack;
                return false;

            case Revival.PrepareForFinalBabyAttack:
                TryStand();
                WalkBackward(64, "fast");
                return false;

            case Revival.ExecuteFinalBabyAttack:
                PlayHead("attacking_baby_metroid");
                _revival = Revival.FinalBabyAttackHolding;
                return false;

            case Revival.FinalBabyAttackHolding:
                return false;

            default:
                throw new InvalidOperationException($"Mother Brain's coming back has no step {_revival}.");
        }
    }
}
