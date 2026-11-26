using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>Mother Brain's seeming death in her tank, the fall of its pipes, and the rising of her body.</summary>
/// <remarks>
/// Her data lists "tube_drops", the pipes that fall in turn: each the "effect" that shows it, where it starts ("x",
/// "y"), how far down it falls ("floor"), the terrain "patch" that clears where it stood, how many updates pass
/// before the next ("wait"), and whether it is the "main" one, which carries her brain down with it and waits a
/// "delay" before it falls, or one from the "top", which hangs above the tank and falls without a trail. It also
/// lists "fake_death_explosions", "ascent_dust" and "sealed_wall_dust", the places of the blasts of her fall, the
/// dust of her rising and the dust of the wall shut behind Samus, and "sealed_wall_patches", "ceiling_patch" and
/// "mural_patches", which it names. What is listed in <see cref="ToldWhenSealed"/> is told "close" as the wall shuts.
/// </remarks>
public partial class MotherBrain
{
    /// <summary>How many updates each step of a change of her colours takes when she seems to die.</summary>
    [Export, DataValue("fake_death_grey_step_updates")]
    public int FakeDeathGreyStepUpdates { get; set; } = 9;

    /// <summary>How many steps a change of her colours takes.</summary>
    [Export, DataValue("grey_steps")]
    public int GreySteps { get; set; } = 8;

    /// <summary>How many updates each step of her colours coming back takes.</summary>
    [Export, DataValue("from_grey_step_updates")]
    public int FromGreyStepUpdates { get; set; } = 5;

    /// <summary>The sound of the dust of her rising, by name.</summary>
    [Export, DataValue("ascent_dust_sound")]
    public string AscentDustSound { get; set; } = "";

    private enum Descent
    {
        InitialPause,
        PauseBeforeLock,
        PauseBeforeMusic,
        PauseBeforeUnlock,
        PauseBeforeFlash,
        FadeToGrey,
        CollapseTubes,
    }

    private enum Ascent
    {
        DrawMural,
        SetUpBody,
        SetUpBrain,
        PauseForSuspense,
        PrepareForRising,
        WaitBeforePause,
        ContinuePausing,
        StartRising,
        Rise,
        WaitUntilStanding,
        ColoursReturn,
    }

    /// <summary>What is told "close" as the wall is shut behind Samus: the part of the room the view is then kept out of.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenSealed { get; set; } = [];

    private sealed record TubeDrop(string Effect, int X, int Y, int Floor, string Patch, int Wait, bool Main, int Delay, bool Top);

    /// <summary>How far below a pipe from the top its dust shows as it starts to fall, in pixels.</summary>
    private const int TopTubeDustBelow = 8;

    private sealed class FallingTube(Effect sprite, TubeDrop drop)
    {
        public Effect Sprite { get; } = sprite;

        public TubeDrop Drop { get; } = drop;

        public Pixels Speed { get; set; }

        public int Delay { get; set; }

        public int SmokeWait { get; set; }

        public int SmokePuff { get; set; }
    }

    private static readonly int[] TubeSmokeAcross = [-8, 2, -4, 6];

    private readonly List<TubeDrop> _tubeDrops = [];
    private readonly List<FallingTube> _fallingTubes = [];

    /// <summary>
    /// The falling pipe that holds the cartridge's first free enemy place, if one does. The pipes that fall as
    /// enemies each take the first free place as they are made, and with her Zebetites gone that is always the same
    /// one: a pipe made while another holds it takes some other.
    /// </summary>
    private FallingTube? _inFirstPlace;

    /// <summary>
    /// The part of a pixel, down, that what last held that place left there. A place is given only whole pixels by
    /// what takes it, and a pipe that bursts is removed and not killed, so each pipe that takes the place starts with
    /// what the last left, and the giant Metroid that takes it last of all arrives with it.
    /// </summary>
    internal int PartOfAPixelLeftInFirstPlace { get; private set; }
    private readonly List<PixelPosition> _fakeDeathExplosions = [];
    private readonly List<PixelPosition> _ascentDust = [];
    private readonly List<PixelPosition> _sealedWallDust = [];
    private readonly List<string> _sealedWallPatches = [];
    private readonly List<string> _muralPatches = [];
    private string _ceilingPatch = "";
    private Descent _descent;
    private Ascent _ascent;
    private int _nextTube;
    private bool _tubeFallen;
    private int _tubeWait;
    private int _explosionWait;
    private int _nextExplosion;
    private int _nextMural;
    private int _greyStep;
    private int _greyWait;
    private bool _flashing;

    private void ReadFakeDeathValues(DataNode data)
    {
        _tubeDrops.Clear();
        _tubeDrops.AddRange(data.List("tube_drops").Select(drop => new TubeDrop(
            drop.Text("effect"), drop.Int("x"), drop.Int("y"), drop.Int("floor"), drop.Text("patch"), drop.Int("wait"), drop.Has("main") && drop.Flag("main"),
            drop.Has("delay") ? drop.Int("delay") : 0, drop.Has("top") && drop.Flag("top"))));
        foreach ((List<PixelPosition> places, string name) in new[]
                 {
                     (_fakeDeathExplosions, "fake_death_explosions"), (_ascentDust, "ascent_dust"), (_sealedWallDust, "sealed_wall_dust"),
                 })
        {
            places.Clear();
            places.AddRange(data.List(name).Select(place => PixelPosition.FromWhole(place.Int("x"), place.Int("y"))));
        }

        foreach ((List<string> patches, string name) in new[] { (_sealedWallPatches, "sealed_wall_patches"), (_muralPatches, "mural_patches") })
        {
            patches.Clear();
            patches.AddRange(data.List(name).Select(patch => patch.AsText()));
        }

        _ceilingPatch = data.Text("ceiling_patch");
        ReadLateFightValues(data);
    }

    /// <summary>Begins her seeming death: the way back is walled off.</summary>
    private void BeginDescent()
    {
        foreach (PixelPosition dust in _sealedWallDust)
            Leave(DustEffect, dust);
        foreach (string patch in _sealedWallPatches)
            Room.Terrain.ApplyPatch(patch);
        foreach (NodePath path in ToldWhenSealed)
        {
            (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"Mother Brain is to tell \"{path}\" that her room is shut, but that is nothing that can be told."))
                .Triggered("close");
        }

        _stage = Stage.Descent;
        _descent = Descent.InitialPause;
    }

    private void StepDescent(Player samus)
    {
        if (_flashing)
            _tint = Updates % 4 < 2 ? new Color(1.6f, 1.6f, 1.6f) : _greyStep > 0 ? GreyTint(_greyStep) : Colors.White;
        switch (_descent)
        {
            case Descent.InitialPause:
                (_descent, _timer) = (Descent.PauseBeforeLock, 65);
                goto case Descent.PauseBeforeLock;

            case Descent.PauseBeforeLock:
                if (--_timer > 0)
                    break;

                // The cartridge's lock here stops her as she is, in the air or mid-stride, and letting go has her go on.
                samus.HoldAsTheyAre();
                (_descent, _timer) = (Descent.PauseBeforeMusic, 33);
                goto case Descent.PauseBeforeMusic;

            case Descent.PauseBeforeMusic:
                if (--_timer > 0)
                    break;
                Room.Session.Sounds.RequestMusic("");
                Compatibility.CartridgeSounds.Of(Room.Session).AskMusicSet("mother_brain");
                (_descent, _timer) = (Descent.PauseBeforeUnlock, 13);
                goto case Descent.PauseBeforeUnlock;

            case Descent.PauseBeforeUnlock:
                if (--_timer > 0)
                    break;
                if (samus.State is HeldState)
                    samus.Release();
                (_descent, _timer) = (Descent.PauseBeforeFlash, 9);
                goto case Descent.PauseBeforeFlash;

            case Descent.PauseBeforeFlash:
                if (--_timer > 0)
                    break;
                _flashing = true;
                Room.Liquid?.HeadFor(_liquidDrain.Height, _liquidDrain.Speed, _liquidDrain.Delay);
                (_nextTube, _tubeFallen, _tubeWait) = (0, false, 1);
                (_inFirstPlace, PartOfAPixelLeftInFirstPlace) = (null, 0);
                (_greyStep, _greyWait) = (0, 1);
                Room.Terrain.ApplyPatch(_ceilingPatch);
                _descent = Descent.FadeToGrey;
                break;

            case Descent.FadeToGrey:
                if (--_greyWait <= 0)
                {
                    _greyWait = FakeDeathGreyStepUpdates;
                    if (++_greyStep >= GreySteps)
                        _descent = Descent.CollapseTubes;
                }

                StepTubeCollapse();
                StepFakeDeathExplosions();
                break;

            case Descent.CollapseTubes:
                StepTubeCollapse();
                StepFakeDeathExplosions();
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's fall has no step {_descent}.");
        }

        StepFallingTubes();
    }

    /// <summary>A blast goes off every few updates, at places that come round in turn; as often as not it is only smoke.</summary>
    private void StepFakeDeathExplosions()
    {
        if (_explosionWait > 0 && --_explosionWait > 0)
            return;
        _explosionWait = 9;
        if (_fakeDeathExplosions.Count == 0)
            return;
        PixelPosition at = _fakeDeathExplosions[_nextExplosion];
        _nextExplosion = (_nextExplosion + 1) % _fakeDeathExplosions.Count;
        Leave((Random.Last.Number & 0xFFFF) < 16384 ? SmokeEffect : ExplosionEffect, at);
        Sound(ExplosionSound);
    }

    /// <summary>The pipes of her tank let go one after another; where each stood is cleared the update after it falls.</summary>
    private void StepTubeCollapse()
    {
        if (_nextTube >= _tubeDrops.Count)
            return;
        TubeDrop drop = _tubeDrops[_nextTube];
        if (!_tubeFallen)
        {
            if (_nextTube > 0 && --_tubeWait > 0)
                return;
            _tubeFallen = true;

            // The pipes about the top of her tank are shots on the cartridge: each holds one of the room's places, and
            // one the room has no place for does not fall, nor raise its dust.
            PixelPosition from = PixelPosition.FromWhole(drop.X, drop.Y);
            if ((drop.Top ? LeaveHoldingPlace(drop.Effect, from) : LeaveEffect(drop.Effect, from)) is not { } sprite)
                return;
            var tube = new FallingTube(sprite, drop) { Delay = drop.Delay };

            // The pipes about the top of her tank are shots on the cartridge and hold no enemy's place.
            if (!drop.Top && _inFirstPlace is null)
            {
                _inFirstPlace = tube;
                tube.Sprite.Position = new PixelPosition(tube.Sprite.Position.X, tube.Sprite.Position.Y.WithFraction(PartOfAPixelLeftInFirstPlace));
            }

            _fallingTubes.Add(tube);
            if (drop.Top)
                Leave(DustEffect, PixelPosition.FromWhole(drop.X, drop.Y + TopTubeDustBelow));
            return;
        }

        Room.Terrain.ApplyPatch(drop.Patch);
        _tubeWait = drop.Wait;
        _tubeFallen = false;
        _nextTube++;
    }

    /// <summary>Lets the falling pipes fall. The main one carries her brain down; where it lands, her body is waiting.</summary>
    private void StepFallingTubes()
    {
        Pixels gravity = Pixels.FromNumber(0.0234375);
        foreach (FallingTube tube in _fallingTubes.ToList())
        {
            if (tube.Delay > 0 && --tube.Delay > 0)
                continue;
            tube.Speed += gravity;
            tube.Sprite.Position += new PixelOffset(Pixels.Zero, tube.Speed);
            int y = tube.Sprite.Position.Y.Whole;
            bool landed;
            if (tube.Drop.Main)
            {
                tube.Sprite.Visible = y < 244;
                _head = new PixelPosition(_head.X, Pixels.FromWhole(y - 56));
                landed = _head.Y.Whole >= 196;
            }
            else
            {
                landed = y >= tube.Drop.Floor;
            }

            if (!landed)
            {
                // Only the pipes that stood about the tank trail dust as they fall.
                if (tube.Drop.Top || (tube.SmokeWait > 0 && --tube.SmokeWait > 0))
                    continue;
                tube.SmokeWait = 9;
                tube.SmokePuff = (tube.SmokePuff + 1) % TubeSmokeAcross.Length;
                Leave(DustEffect, PixelPosition.FromWhole(tube.Sprite.Position.X.Whole + TubeSmokeAcross[tube.SmokePuff], 208));
                continue;
            }

            if (tube.Drop.Main)
                LandBrain();
            // A pipe from the top ends in smoke, quietly; the others burst.
            Leave(tube.Drop.Top ? SmokeEffect : ExplosionEffect, tube.Sprite.Position);
            if (!tube.Drop.Top)
                Sound(ExplosionSound);
            if (ReferenceEquals(tube, _inFirstPlace))
                (_inFirstPlace, PartOfAPixelLeftInFirstPlace) = (null, tube.Sprite.Position.Y.Fraction);
            Room.Remove(tube.Sprite);
            _fallingTubes.Remove(tube);
        }
    }

    /// <summary>Her brain comes to rest on the floor, and the room is still.</summary>
    private void LandBrain()
    {
        _flashing = false;
        _tint = GreyTint(GreySteps);
        Room.Shake(3, 32);
        _head = new PixelPosition(_head.X, Pixels.FromWhole(196));
        Position = PixelPosition.FromWhole(StandingPlace.X, StandingPlace.Y);
        (_lowerNeck, _upperNeck, _neckTurn) = (72 * 256, 80 * 256, NeckSteady);
        _stage = Stage.Ascent;
        _ascent = Ascent.DrawMural;
        _nextMural = 0;
    }

    private void StepAscent()
    {
        StepFallingTubes();
        switch (_ascent)
        {
            case Ascent.DrawMural:
                // The wall behind her shows through, two rows an update.
                for (int row = 0; row < 2 && _nextMural < _muralPatches.Count; row++)
                    Room.Terrain.ApplyPatch(_muralPatches[_nextMural++]);
                if (_nextMural >= _muralPatches.Count)
                    _ascent = Ascent.SetUpBody;
                break;

            case Ascent.SetUpBody:
                _ascent = Ascent.SetUpBrain;
                break;

            case Ascent.SetUpBrain:
                _onNeck = true;
                _bodyTouches = false;
                _headHealth = SecondPhaseHealth;
                (_ascent, _timer) = (Ascent.PauseForSuspense, 129);
                goto case Ascent.PauseForSuspense;

            case Ascent.PauseForSuspense:
                if (--_timer > 0)
                    break;
                (_ascent, _timer) = (Ascent.PrepareForRising, 33);
                goto case Ascent.PrepareForRising;

            case Ascent.PrepareForRising:
                if (--_timer > 0)
                    break;
                _headShown = false;
                PlayHead("whole_head_mouth_closed");
                (_ascent, _timer) = (Ascent.WaitBeforePause, 12);
                goto case Ascent.WaitBeforePause;

            case Ascent.WaitBeforePause:
                if (--_timer > 0)
                    break;
                (_ascent, _timer) = (Ascent.ContinuePausing, 257);
                goto case Ascent.ContinuePausing;

            case Ascent.ContinuePausing:
                if (--_timer > 0)
                    break;
                Position = PixelPosition.FromWhole(StandingPlace.X, StandingPlace.Y);
                (_bodyTouches, _brainTouches, _neckTouches) = (true, true, true);
                _ascent = Ascent.StartRising;
                break;

            case Ascent.StartRising:
                Play("crouched", restart: true);
                Visual?.SetShown(true);
                _headShown = true;
                Position = PixelPosition.FromWhole(StandingPlace.X, StandingPlace.Y);
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                Room.Shake(1, 256);
                (_neckTurn, _neckMoves, _lowerMove, _upperMove) = (NeckRising, true, NeckMove.Raise, NeckMove.Lower);
                _ascent = Ascent.Rise;
                break;

            case Ascent.Rise:
                if (Room.Session.Clock.Updates % 4 != 0)
                    break;
                if (_ascentDust.Count > 0)
                {
                    PixelPosition at = NextBurstOf(_ascentDust);
                    Leave(Random.Last.Part(8, 1) == 1 ? BigDustEffect : DustEffect, at);
                    Sound(AscentDustSound);
                }

                Position -= PixelOffset.FromWhole(0, 2);
                if (Position.Y.Whole > RisenY)
                    break;
                Position = new PixelPosition(Position.X, Pixels.FromWhole(RisenY));
                Room.Shake(0, 0);
                Play("stand_after_crouch_slow", restart: true);
                _ascent = Ascent.WaitUntilStanding;
                break;

            case Ascent.WaitUntilStanding:
                if (_pose != Pose.Standing)
                    break;
                (_greyStep, _greyWait) = (GreySteps, 1);
                _ascent = Ascent.ColoursReturn;
                break;

            case Ascent.ColoursReturn:
                if (--_greyWait > 0)
                    break;
                _greyWait = FromGreyStepUpdates;
                _tint = GreyTint(--_greyStep);
                if (_greyStep > 0)
                    break;
                _form = Form.SecondPhase;
                _drools = true;
                (_lowerMove, _upperMove, _neckTurn) = (NeckMove.Lower, NeckMove.Lower, NeckFast);
                _stage = Stage.SecondPhase;
                (_fight, _timer) = (Fight.StretchingShakeHead, 24);
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's rising has no step {_ascent}.");
        }
    }

    /// <summary>Her colour a number of steps of the way to grey.</summary>
    private Color GreyTint(int step) =>
        Colors.White.Lerp(new Color(0.45f, 0.45f, 0.45f), Math.Clamp((float)step / Math.Max(1, GreySteps), 0f, 1f));
}
