using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Phantoon: the ghost that haunts the Wrecked Ship. It fades in among a ring of flames, then drifts in a figure of
/// eight dropping flames, unseen to shots, until it opens its eye; only then can it be hurt. Hurt, it turns solid and
/// swoops at Samus; left alone, it shuts its eye and comes back with a rain of flames. A Super Missile in the eye
/// enrages it into throwing rings of flame from the top of the room. Dead, it bursts and fades, and the ship's power
/// comes back.
/// </summary>
/// <remarks>
/// Its body plays "body". The visual "Eye" plays "eye_open", whose cue "begin_tracking" opens it to harm;
/// "eye_close"; "eye_close_and_pick", whose cue "pick_pattern" chooses what it does next; "eyeball_centred"; and
/// "eye_looking_" with one of the eight ways. The visual "Mouth" plays "mouth_idle" and "mouth_spawn_flame", whose
/// cue "flame" drops a flame; "Tentacles" plays "tentacles". Its hurtbox "Eye" is its eye and "Body" the rest of it.
/// Its data gives its figure of eight under "path", a digit a step as on a number pad, and the places its rains fall
/// from under "rain_patterns". Its shots are "flame" and "starting_flame". All its places are in the room's own
/// pixels: its room is a single screen.
/// </remarks>
public partial class Phantoon : Enemy
{
    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The music that plays once it is dead, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>How many steps a fade in or out takes; it takes one every other update.</summary>
    [Export, DataValue("fade_steps")]
    public int FadeSteps { get; set; } = 12;

    /// <summary>The harm, in one opening of its eye, that makes it shut and go.</summary>
    [Export, DataValue("round_damage")]
    public int RoundDamage { get; set; } = 300;

    /// <summary>How many things it drops when it dies.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The effect of a big explosion, by the data file of its timeline.</summary>
    [Export, DataValue("big_explosion_effect")]
    public string BigExplosionEffect { get; set; } = "";

    /// <summary>The effect of a small explosion.</summary>
    [Export, DataValue("small_explosion_effect")]
    public string SmallExplosionEffect { get; set; } = "";

    /// <summary>The sound of a flame, by name.</summary>
    [Export, DataValue("flame_sound")]
    public string FlameSound { get; set; } = "";

    /// <summary>Its cry when shot, by name.</summary>
    [Export, DataValue("shot_cry")]
    public string ShotCry { get; set; } = "";

    /// <summary>Its cry as it dies.</summary>
    [Export, DataValue("dying_cry")]
    public string DyingCry { get; set; } = "";

    /// <summary>The sound of a wave of its rage.</summary>
    [Export, DataValue("rage_sound")]
    public string RageSound { get; set; } = "";

    /// <summary>The sound of an explosion on it as it dies.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    private const int Screen = 256;

    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];
    private static readonly int[] VulnerableWindows = [60, 30, 15, 30, 60, 30, 15, 60];
    private static readonly int[] EyeClosedWaits = [720, 60, 360, 720, 360, 60, 360, 720];
    private static readonly int[] RainHidingWaits = [60, 120, 30, 60, 30, 60, 30, 30];
    private static readonly int[][] FlameBursts = [[32, 32, 32, 32, 32], [16, 16, 16], [48, 48, 48, 48, 48, 48, 48], [32, 16, 32, 64, 32, 64, 16]];

    // The explosions of its death: where from its middle, whether big, and the wait after; the first five come once.
    private static readonly (int X, int Y, bool Big, int Wait)[] DeathExplosions =
    [
        (0, 0, true, 16), (32, -32, true, 16), (-32, 32, true, 16), (-32, -32, true, 16), (32, 32, true, 32),
        (-32, -8, true, 8), (0, 0, false, 8), (32, -8, true, 8), (0, 0, false, 8), (0, 24, false, 8), (0, 48, true, 8), (-24, 24, false, 8), (24, 24, false, 8),
    ];

    private enum Phase
    {
        WaitingOnEye,
        SpawnStartingFlames,
        WaitBeforeStartingFlames,
        StartingFlamesOrbit,
        FadeInAtStart,
        PickFirstPattern,
        FigureEight,
        EyeTracks,
        BecomeSolidAndSwoop,
        Swooping,
        FadeOutWhileSwooping,
        HiddenWait,
        PickNextAppearance,
        FadeInBeforeFigureEight,
        BecomeSolidAfterRain,
        FadeInDuringRain,
        TrackDuringRain,
        FadeOutDuringRain,
        SpawnRain,
        FadeOutBeforeFirstRain,
        FadeOutBeforeRage,
        HiddenBeforeRage,
        FadeInForRage,
        Enraged,
        FadeOutAfterRage,
        FatalSwoop,
        DyingFades,
        DyingExplosions,
        DyingFadeOut,
        Dead,
    }

    private readonly List<(int X, int Y)> _path = [];
    private readonly List<(int Place, int X, int Y)> _rainPatterns = [];
    private readonly Dictionary<string, TimelinePlayer> _parts = new(StringComparer.Ordinal);
    private Phase _phase = Phase.SpawnStartingFlames;
    private int _wait;
    private int _eyeClosedWait;
    private int _roundDamage;
    private bool _swoopAfterWindow;
    private bool _nextIsRain;
    private bool _dying;

    private int _pathPlace;
    private bool _pathBackward;
    private Pixels _pathSpeed;
    private int _pathStage;

    private int[] _burst = [];
    private int _burstAt = -1;
    private int _burstWait;

    private int _fade;
    private float _alpha;
    private int _startingFlame;
    private int _rageWaves;
    private int _swoopTargetX;
    private bool _swoopTargetReturning;
    private int _fadeCycles;
    private int _explosion;
    private int _explosionPasses;

    /// <summary>Whether its opening ring of flames has been told to close in on it.</summary>
    public bool StartingFlamesClose => _phase == Phase.StartingFlamesOrbit;

    /// <summary>The middle of the circles its flames turn in.</summary>
    public PixelPosition FlameCentre => PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole + 16);

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) =>
        _phase is not (Phase.WaitingOnEye or Phase.DyingFades or Phase.DyingExplosions or Phase.DyingFadeOut or Phase.Dead) && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _path.Clear();
        foreach (char step in data.Text("path"))
        {
            _path.Add(step switch
            {
                '1' => (-1, 1), '2' => (0, 1), '3' => (1, 1), '4' => (-1, 0), '6' => (1, 0), '7' => (-1, -1), '8' => (0, -1), '9' => (1, -1),
                _ => throw data.Problem($"has a path step '{step}' that no way on a number pad stands for"),
            });
        }

        _rainPatterns.Clear();
        _rainPatterns.AddRange(data.List("rain_patterns").Select(pattern => (pattern.Int("path_place"), pattern.Int("x"), pattern.Int("y"))));
        if (_path.Count == 0 || _rainPatterns.Count != 8)
            throw data.Problem("must give a path and eight rain patterns");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _wait = 120;
        Play("body");
        PlayPart("Eye", "eye_close");
        PlayPart("Mouth", "mouth_idle");
        PlayPart("Tentacles", "tentacles");
        SetVisible(0);
        SetHurtAreas(eye: false, body: false, touch: false);
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        // Bombs and their blasts never reach it.
        bool bomb = hit.Carries(new HitKind(HitKinds.Bomb)) || hit.Carries(new HitKind(HitKinds.PowerBomb));
        return bomb ? reaction with { DamageTimes = 0 } : reaction;
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part)
    {
        Sound(ShotCry);
        int damage = base.ReactionTo(hit, part).DamageFrom(hit.Damage);
        bool window = _phase is Phase.EyeTracks or Phase.TrackDuringRain;
        bool swooping = _phase == Phase.Swooping;
        if (!window && !swooping)
            return;
        if (damage >= RoundDamage && hit.Carries(new HitKind(HitKinds.SuperMissile)))
        {
            CloseAfterDamage(Phase.FadeOutBeforeRage);
            return;
        }

        _roundDamage += damage;
        if (_roundDamage >= RoundDamage)
        {
            if (swooping)
                _wait = 1;
            else
                CloseAfterDamage(Phase.FadeOutWhileSwooping);
            return;
        }

        if (!window)
            return;
        Random.Draw();
        if (!_swoopAfterWindow)
        {
            _swoopAfterWindow = true;
            _wait = Math.Min(_wait, 16);
        }
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        Sound(ShotCry);
        _dying = true;
        _phase = _phase is Phase.Swooping or Phase.FadeOutWhileSwooping ? Phase.FatalSwoop : Phase.DyingFades;
        _pathBackward = false;
        _fade = 0;
        SetHurtAreas(eye: false, body: false, touch: false);
        return false;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        long clock = Room.Session.Clock.Updates;
        switch (_phase)
        {
            case Phase.SpawnStartingFlames:
                if (!Ends())
                    break;
                // Eight flames, one every half second, an eighth of a turn apart.
                var first = (PhantoonFlame)Fire("starting_flame", FlameCentre, PixelVelocity.Zero);
                first.OrbitAtStart(this, _startingFlame * (Turns.PerTurn / 8));
                Sound(FlameSound);
                _wait = 30;
                if (++_startingFlame < 8)
                    break;
                _roundDamage = 0;
                _phase = Phase.WaitBeforeStartingFlames;
                break;

            case Phase.WaitBeforeStartingFlames:
                if (Ends())
                {
                    _wait = 240;
                    _phase = Phase.StartingFlamesOrbit;
                }

                break;

            case Phase.StartingFlamesOrbit:
                if (!Ends())
                    break;
                _phase = Phase.FadeInAtStart;
                _wait = 120;
                _fade = 0;
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                break;

            case Phase.FadeInAtStart:
                FadeIn(clock);
                if (_wait > 0)
                    _wait--;
                if (_wait == 0 && _fade >= FadeSteps)
                {
                    _phase = Phase.PickFirstPattern;
                    _wait = 30;
                }

                break;

            case Phase.PickFirstPattern:
                if (!Ends())
                    break;
                _eyeClosedWait = EyeClosedWaits[(int)(clock / 2 % 4)];
                _phase = Phase.FigureEight;
                _pathStage = 0;
                if (Random.Draw().Part(0, 1) == 1)
                {
                    _pathSpeed = Pixels.Zero;
                    _pathBackward = true;
                    _pathPlace = _path.Count - 1;
                }
                else
                {
                    _pathSpeed = Pixels.FromWhole(1);
                    _pathBackward = false;
                    _pathPlace = 0;
                }

                break;

            case Phase.FigureEight:
                StepFigureEight();
                StepFlameBursts();
                if (--_eyeClosedWait > 0)
                    break;
                _phase = Phase.WaitingOnEye;
                PlayPart("Eye", "eye_open");
                _nextIsRain = false;
                for (int flame = 0; flame < 8; flame++)
                    ((PhantoonFlame)Fire("flame", FlameCentre, PixelVelocity.Zero)).Spiral(this, 224 - flame * (Turns.PerTurn / 8));
                break;

            case Phase.EyeTracks:
                if (Ends())
                {
                    _roundDamage = 0;
                    if (!_swoopAfterWindow)
                    {
                        _phase = Phase.WaitingOnEye;
                        SetHurtAreas(eye: false, body: false, touch: false);
                        PlayPart("Eye", "eye_close_and_pick");
                        _nextIsRain = true;
                        break;
                    }

                    _swoopAfterWindow = false;
                    _wait = 60;
                    _phase = Phase.BecomeSolidAndSwoop;
                }

                LookAtSamus();
                break;

            case Phase.BecomeSolidAndSwoop:
                LookAtSamus();
                BeginSwoop();
                SetVisible(1);
                SetHurtAreas(eye: true, body: true, touch: true);
                break;

            case Phase.Swooping:
                LookAtSamus();
                Swoop(Target.Y.Whole - 48);
                if (!Ends())
                    break;
                _phase = Phase.FadeOutWhileSwooping;
                SetHurtAreas(eye: false, body: false, touch: false);
                PlayPart("Eye", "eye_close");
                _fade = 0;
                _roundDamage = 0;
                break;

            case Phase.FadeOutWhileSwooping:
                Swoop(Target.Y.Whole - 48);
                if (FadeOut(clock))
                {
                    _phase = Phase.HiddenWait;
                    _wait = 120;
                }

                break;

            case Phase.HiddenWait:
                if (Ends())
                    _phase = Phase.PickNextAppearance;
                break;

            case Phase.PickNextAppearance:
                bool right = Random.Draw().Part(0, 1) == 1;
                _pathPlace = right ? 136 : 399;
                Position = PixelPosition.FromWhole(right ? 208 : 48, 96);
                _pathBackward = false;
                _pathSpeed = Pixels.FromWhole(1);
                _nextIsRain = false;
                SecondRoundSettings(clock);
                _phase = Phase.FadeInBeforeFigureEight;
                _fade = 0;
                break;

            case Phase.FadeInBeforeFigureEight:
                if (FadeIn(clock))
                    _phase = Phase.FigureEight;
                break;

            case Phase.BecomeSolidAfterRain:
                _fade = 0;
                SetHurtAreas(eye: true, body: true, touch: false);
                PlayPart("Eye", "eyeball_centred");
                _phase = Phase.FadeInDuringRain;
                break;

            case Phase.FadeInDuringRain:
                if (!FadeIn(clock))
                    break;
                SetHurtAreas(eye: true, body: true, touch: true);
                _phase = Phase.TrackDuringRain;
                _wait = 90;
                break;

            case Phase.TrackDuringRain:
                if (!Ends())
                    break;
                _roundDamage = 0;
                if (_swoopAfterWindow)
                {
                    _swoopAfterWindow = false;
                    _nextIsRain = true;
                    BeginSwoop();
                    break;
                }

                _phase = Phase.FadeOutDuringRain;
                _fade = 0;
                SetHurtAreas(eye: false, body: false, touch: false);
                PlayPart("Eye", "eye_close");
                break;

            case Phase.FadeOutDuringRain:
                if (FadeOut(clock))
                {
                    _phase = Phase.SpawnRain;
                    _wait = RainHidingWaits[Random.Draw().Part(0, 3)];
                }

                break;

            case Phase.SpawnRain:
                if (!Ends())
                    break;
                (int place, int x, int y) = _rainPatterns[Random.Draw().Part(0, 3)];
                _pathPlace = place;
                Position = PixelPosition.FromWhole(x, y);
                _pathBackward = false;
                _phase = Phase.BecomeSolidAfterRain;
                Rain();
                break;

            case Phase.FadeOutBeforeFirstRain:
                FadeOut(clock);
                StepFigureEight();
                StepFlameBursts();
                if (--_eyeClosedWait > 0)
                    break;
                _swoopAfterWindow = false;
                _phase = Phase.BecomeSolidAfterRain;
                // The first rain falls from the middle if it is in the left half of the room, and from the right otherwise.
                (_, int rainX, int rainY) = _rainPatterns[Position.X.Whole < Screen / 2 ? 0 : 2];
                Position = PixelPosition.FromWhole(rainX, rainY);
                Rain();
                break;

            case Phase.FadeOutBeforeRage:
                if (FadeOut(clock))
                {
                    _phase = Phase.HiddenBeforeRage;
                    _wait = 120;
                }

                break;

            case Phase.HiddenBeforeRage:
                if (!Ends())
                    break;
                _phase = Phase.FadeInForRage;
                Position = PixelPosition.FromWhole(Screen / 2, 32);
                _fade = 0;
                break;

            case Phase.FadeInForRage:
                if (FadeIn(clock))
                {
                    _phase = Phase.Enraged;
                    _wait = 4;
                    _fade = 0;
                }

                break;

            case Phase.Enraged:
                if (!Ends())
                    break;
                // Waves of flame fan out from it, turning one way and then the other.
                bool odd = _rageWaves % 2 == 1;
                for (int flame = 0; flame < (odd ? 8 : 7); flame++)
                {
                    int angle = odd ? Turns.PerTurn / 2 + flame * 16 : 112 - flame * 16;
                    ((PhantoonFlame)Fire("flame", PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole + 32), PixelVelocity.Zero)).Rage(this, angle, odd ? -2 : 2);
                }

                Sound(RageSound);
                if (++_rageWaves < 8)
                {
                    _wait = 128;
                    break;
                }

                PlayPart("Eye", "eye_close");
                _rageWaves = 0;
                _phase = Phase.FadeOutAfterRage;
                break;

            case Phase.FadeOutAfterRage:
                if (FadeOut(clock))
                {
                    _phase = Phase.HiddenWait;
                    _wait = 120;
                }

                break;

            case Phase.FatalSwoop:
                LookAtSamus();
                Swoop(112);
                if (Position.X.Whole is >= 96 and < 160)
                    _phase = Phase.DyingFades;
                break;

            case Phase.DyingFades:
                bool done = _fadeCycles % 2 == 1 ? FadeIn(clock) : FadeOut(clock);
                if (!done)
                    break;
                _fade = 0;
                if (++_fadeCycles < 10)
                    break;
                SetVisible(1);
                _phase = Phase.DyingExplosions;
                _wait = 15;
                break;

            case Phase.DyingExplosions:
                if (!Ends())
                    break;
                (int offsetX, int offsetY, bool big, int next) = DeathExplosions[_explosion];
                _wait = next;
                string effect = big ? BigExplosionEffect : SmallExplosionEffect;
                if (effect.Length > 0)
                    LeaveEffect(effect, PixelPosition.FromWhole(Position.X.Whole + offsetX, Position.Y.Whole + offsetY));
                Sound(ExplosionSound);
                if (++_explosion < DeathExplosions.Length)
                    break;
                _explosion = 5;
                if (++_explosionPasses < 3)
                    break;
                _phase = Phase.DyingFadeOut;
                _fade = 0;
                foreach (string part in new[] { "Eye", "Mouth", "Tentacles" })
                    GetNodeOrNull<ActorVisual>(part)?.SetShown(false);
                Sound(DyingCry);
                break;

            case Phase.DyingFadeOut:
                if (FadeOut(clock))
                {
                    _phase = Phase.Dead;
                    _wait = 61;
                }

                break;

            case Phase.Dead:
                if (!Ends())
                    break;
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(Drops, PixelPosition.FromWhole(64 + draw.Part(0, 7), 96 + draw.Part(8, 6)));
                }

                if (VictoryMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(VictoryMusic);
                Die();
                return;
        }

        StepParts();
    }

    /// <summary>Counts its wait down by one update, and returns whether that ended it.</summary>
    private bool Ends()
    {
        if (_wait <= 0)
            return false;
        return --_wait == 0;
    }

    /// <summary>Shuts its eye after it has taken enough, and sends it off by the given way out.</summary>
    private void CloseAfterDamage(Phase wayOut)
    {
        _phase = wayOut;
        _swoopAfterWindow = false;
        _roundDamage = 0;
        SetHurtAreas(eye: false, body: false, touch: false);
        PlayPart("Eye", "eye_close");
        _fade = 0;
    }

    /// <summary>Opens its eye to harm, for a time picked by chance.</summary>
    private void BeginTracking()
    {
        _swoopAfterWindow = false;
        SetHurtAreas(eye: true, body: false, touch: true);
        _wait = VulnerableWindows[Random.Draw().Part(0, 3)];
        _phase = Phase.EyeTracks;
        PlayPart("Eye", "eyeball_centred");
    }

    /// <summary>Sets up its next round: how long its eye stays shut, and which way round its figure of eight goes.</summary>
    private void SecondRoundSettings(long clock)
    {
        _wait = 60;
        _eyeClosedWait = EyeClosedWaits[Random.Draw().Part(0, 3)];
        _pathStage = 0;
        if (clock % 2 == 1)
        {
            if (!_pathBackward)
                _pathPlace = (_pathPlace + _path.Count - 1) % _path.Count;
            _pathSpeed = Pixels.Zero;
            _pathBackward = true;
        }
        else
        {
            if (_pathBackward)
                _pathPlace = (_pathPlace + 1) % _path.Count;
            _pathSpeed = Pixels.FromWhole(1);
            _pathBackward = false;
        }
    }

    /// <summary>Moves it one update along its figure of eight: easing in, surging, and settling, over and over.</summary>
    private void StepFigureEight()
    {
        Pixels slow = Pixels.FromWhole(3).Share(1, 128), fast = Pixels.FromWhole(1).Share(1, 16);
        int sign = _pathBackward ? -1 : 1;
        if (_pathStage == 0)
        {
            _pathSpeed += slow * sign;
            if (_pathSpeed.Whole * sign >= 2)
            {
                _pathSpeed = Pixels.FromWhole(_pathBackward ? 0 : 1);
                _pathStage = 1;
            }
        }
        else if (_pathStage == 1)
        {
            _pathSpeed += fast * sign;
            if (_pathSpeed.Whole * sign >= 7)
            {
                _pathSpeed = Pixels.FromWhole(_pathBackward ? -6 : 7);
                _pathStage = 2;
            }
        }
        else
        {
            _pathSpeed -= fast * sign;
            if (_pathBackward ? _pathSpeed.Whole >= 0 : _pathSpeed.Whole <= 0)
            {
                _pathSpeed = Pixels.FromWhole(_pathBackward ? 0 : 1);
                _pathStage = 0;
            }
        }

        // Only whole places are taken, and only along the way it is going.
        for (int step = _pathBackward ? -_pathSpeed.Whole : _pathSpeed.Whole; step > 0; step--)
        {
            (int x, int y) = _path[_pathPlace];
            if (_pathBackward)
            {
                Position -= PixelOffset.FromWhole(x, y);
                _pathPlace = (_pathPlace + _path.Count - 1) % _path.Count;
            }
            else
            {
                Position += PixelOffset.FromWhole(x, y);
                _pathPlace = (_pathPlace + 1) % _path.Count;
            }
        }
    }

    /// <summary>Moves on the bursts of flames it drops as it drifts: a burst picked by chance, then a long pause.</summary>
    private void StepFlameBursts()
    {
        if (--_burstWait > 0)
            return;
        if (_burstAt >= 0 && _burstAt < _burst.Length)
        {
            _burstAt++;
            _burstWait = _burstAt < _burst.Length ? _burst[_burstAt] : 180;
            PlayPart("Mouth", "mouth_spawn_flame");
            return;
        }

        _burst = FlameBursts[Random.Draw().Part(0, 2)];
        _burstAt = 0;
        _burstWait = _burst[0];
    }

    /// <summary>Lets eight flames hang in a row across the room, to fall one after another.</summary>
    private void Rain()
    {
        // The row starts one column on from the column it is over, and wraps round.
        int column = (Position.X.Whole - 48) / 20 + 1;
        for (int flame = 0; flame < 8; flame++)
        {
            int x = 48 + 20 * ((column + flame) % 9);
            ((PhantoonFlame)Fire("flame", PixelPosition.FromWhole(x, 40), PixelVelocity.Zero)).Rain(8 * (flame + 1));
        }
    }

    /// <summary>Starts a swoop.</summary>
    private void BeginSwoop()
    {
        Velocity = new PixelVelocity(Pixels.FromWhole(4), Pixels.FromWhole(4));
        _swoopTargetX = 0;
        _swoopTargetReturning = false;
        _phase = Phase.Swooping;
        _wait = 360;
    }

    /// <summary>Moves it one update of a swoop: pulled back and forth across the room after a spot that sweeps it, and toward a height.</summary>
    private void Swoop(int targetY)
    {
        if (_swoopTargetReturning)
        {
            _swoopTargetX -= 2;
            if (_swoopTargetX == 0)
                _swoopTargetReturning = false;
        }
        else
        {
            _swoopTargetX += 2;
            if (_swoopTargetX >= 256)
                _swoopTargetReturning = true;
        }

        Pixels across = Velocity.X, down = Velocity.Y;
        if (_swoopTargetX < Position.X.Whole)
        {
            if (across >= Pixels.FromNumber(-2047.0 / 256))
                across -= Pixels.FromNumber(0.125);
        }
        else if (across < Pixels.FromWhole(8))
        {
            across += Pixels.FromNumber(0.125);
        }

        if (targetY < Position.Y.Whole)
        {
            if (down >= Pixels.FromNumber(-1535.0 / 256))
                down -= Pixels.FromNumber(0.25);
        }
        else if (down < Pixels.FromWhole(6))
        {
            down += Pixels.FromNumber(0.25);
        }

        Velocity = new PixelVelocity(across, down);
        Position += new PixelOffset(across, down);
        Position = new PixelPosition(
            Position.X.Whole < -64 ? Position.X.WithWhole(-64) : Position.X.Whole >= 448 ? Position.X.WithWhole(448) : Position.X,
            Position.Y.Whole < 64 ? Position.Y.WithWhole(64) : Position.Y.Whole >= 216 ? Position.Y.WithWhole(216) : Position.Y);
    }

    /// <summary>Turns its eye to Samus: straight across or up and down when she is nearly level or nearly above, and slantwise otherwise.</summary>
    private void LookAtSamus()
    {
        PixelOffset to = Target - Position;
        int across = to.X.Whole, down = to.Y.Whole;
        string way = Math.Abs(down) < 32 ? (across < 0 ? "left" : "right")
            : Math.Abs(across) < 32 ? (down < 0 ? "up" : "down")
            : $"{(down < 0 ? "up" : "down")}_{(across < 0 ? "left" : "right")}";
        PlayPart("Eye", $"eye_looking_{way}");
    }

    /// <summary>Takes a step of fading in on the updates it fades on, and returns whether it is fully there.</summary>
    private bool FadeIn(long clock)
    {
        if (clock % 2 == 0 && _fade < FadeSteps)
            SetVisible((float)++_fade / FadeSteps);
        return _fade >= FadeSteps;
    }

    /// <summary>Takes a step of fading out on the updates it fades on, and returns whether it is fully gone.</summary>
    private bool FadeOut(long clock)
    {
        if (clock % 2 == 0 && _fade < FadeSteps)
            SetVisible(1f - (float)++_fade / FadeSteps);
        return _fade >= FadeSteps;
    }

    /// <inheritdoc/>
    protected override Color RestingTint => new(1, 1, 1, _alpha);

    /// <summary>Shows it as solid as <paramref name="alpha"/>, parts and all.</summary>
    private void SetVisible(float alpha)
    {
        _alpha = Math.Clamp(alpha, 0f, 1f);
        foreach (string part in _parts.Keys)
            GetNodeOrNull<ActorVisual>(part)?.Tint(new Color(1, 1, 1, _alpha));
    }

    /// <summary>Says which of it shots can land on, and whether its touch hurts.</summary>
    private void SetHurtAreas(bool eye, bool body, bool touch)
    {
        foreach (Collider collider in Engine.World.EntityNode.CollidersOf(this))
        {
            collider.Enabled = collider.Role == ColliderRole.Hitbox ? touch && !_dying
                : collider.Name == "Eye" ? eye && !_dying
                : body && !_dying;
        }
    }

    /// <summary>Starts one of its parts on a timeline, unless it is already playing it.</summary>
    private void PlayPart(string part, string timeline)
    {
        FrameTimeline wanted = Timelines.Get(timeline);
        if (!_parts.TryGetValue(part, out TimelinePlayer? player) || player.Timeline != wanted)
            _parts[part] = new TimelinePlayer(wanted);
    }

    /// <summary>Moves its parts' timelines on, and acts on their cues.</summary>
    private void StepParts()
    {
        foreach ((string part, TimelinePlayer player) in _parts.ToList())
        {
            foreach (string cue in player.Step())
            {
                switch (cue)
                {
                    case "begin_tracking":
                        BeginTracking();
                        break;
                    case "pick_pattern":
                        SecondRoundSettings(Room.Session.Clock.Updates);
                        _phase = _nextIsRain ? Phase.FadeOutBeforeFirstRain : Phase.FigureEight;
                        _fade = 0;
                        break;
                    case "flame":
                        ((PhantoonFlame)Fire("flame", PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole + 32), PixelVelocity.Zero)).Fall();
                        Sound(FlameSound);
                        break;
                }
            }

            GetNodeOrNull<ActorVisual>(part)?.Show(player.Current);
        }
    }
}
