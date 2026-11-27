using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// Ceres blowing up. The station is seen from space, blasts breaking out over it as the view draws near, until one
/// great blast takes it. If Samus got away her gunship flies out of the blast, the view cuts to Zebes, coming clear
/// out of coarse blocks, its name comes up, and the gunship flies down to it; play then goes on where she lands. If
/// she was still aboard, the blast is the end of her.
/// </summary>
/// <remarks>
/// Its data names the timelines of what it shows ("large_asteroids", "small_asteroids", "vortex", "small_explosion",
/// "repeating_small_explosion", "large_explosion", "station_blast", "planet", "stars_upper_left", "stars_upper_right",
/// "stars_lower_left", "stars_lower_right", "title", "second_title"), where each starts ("places"), its pictures
/// ("station_floor", "explosion_floor", "approach_floor", "zebes"), its "backdrop" colour, the "blast_sound" of the
/// station going up, and the languages its second title is shown in ("second_title_languages"). The title's
/// timeline gives the cues "fade_in", "second_title", "fade_out" and "fly".
/// The great blast that takes the station is a power bomb's, seen as one is: on each update from the one after it
/// goes off, the oval it shows ("blast_ovals", how far across, nought for none) and the colour it adds to everything
/// inside it ("blast_glows", red, green and blue in one number), the oval "blast_height_parts" high for every
/// "blast_width_parts" across; it stays where the station was on the screen as the view moves.
/// </remarks>
public partial class CeresDestruction : Cinematic
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "cinematics/ceres_destruction.json";

    /// <summary>Whether Samus was still aboard when the station blew up.</summary>
    public bool SamusAboard { get; set; }

    /// <summary>The room play goes on in afterwards, when she got away.</summary>
    public string LandingRoom { get; set; } = "";

    /// <summary>The entry point she arrives at there.</summary>
    public string LandingEntry { get; set; } = "";

    private enum Phase
    {
        Opening,
        WaitingForMusic,
        FadingInAndDrifting,
        ApproachingExplosion,
        FlyingAwayFromExplosion,
        HoldingAfterExplosion,
        FadingOutCeres,
        SettingUpZebes,
        FadingInZebes,
        ClearingZebes,
        ShowingTitle,
        FlyingTowardZebes,
        TurningTowardZebes,
        ClosingInOnZebes,
        HoldingCloseToZebes,
        SlidingAway,
        Finished,
    }

    /// <summary>How many updates pass before the station is set up.</summary>
    private const int OpeningUpdates = 9;

    /// <summary>How many turns of the music's queue the station's piece of music waits once at its front.</summary>
    private const int MusicWait = 14;

    /// <summary>How many updates a fade takes, and how many steps it goes in.</summary>
    private const int FadeUpdates = 30, FadeLevels = 15;

    /// <summary>How many updates the title of Zebes takes to fade in, and again to fade out: eight looks of three.</summary>
    private const int TitleFadeUpdates = 24;

    /// <summary>How long the last blast is watched, and how long Zebes is held close.</summary>
    private const int ExplosionHoldUpdates = 192, ZebesHoldUpdates = 64;

    /// <summary>The point of each floor that stays put as it turns.</summary>
    private static readonly Vector2 CeresCentre = new(52, 48), ZebesCentre = new(56, 24);

    /// <summary>How many points the great blast's oval is drawn with.</summary>
    private const int BlastOvalPoints = 128;

    /// <summary>How far a blast is carried each update, as the gunship flies from it.</summary>
    private static readonly PixelVelocity BlastDrift = new(Pixels.FromNumber(0.25), Pixels.FromNumber(-0.0625));

    /// <summary>The first blasts over the station, each by where it goes off from the station's middle and how long it waits.</summary>
    private static readonly (int X, int Y, int Delay)[] FirstBlasts = [(16, -16, 1), (-16, 16, 16), (16, 16, 32), (-16, -16, 48), (0, 0, 64)];

    /// <summary>Where each of the blasts that follow, one after another, goes off.</summary>
    private static readonly (int X, int Y)[] FollowingBlasts = [(14, -8), (8, 12), (-16, 12), (-8, -14), (0, 0), (16, 14)];

    /// <summary>The last, larger blasts.</summary>
    private static readonly (int X, int Y, int Delay)[] LastBlasts = [(8, -4, 1), (12, 8, 4), (-8, -10, 8), (-12, 12, 16)];

    private readonly List<CinematicSprite> _spaceThings = [];
    private readonly List<CinematicSprite> _blasts = [];
    private readonly List<(CinematicSprite Sprite, Pixels Gain, bool Last)> _sliders = [];
    private readonly Dictionary<CinematicSprite, Pixels> _slideSpeeds = [];
    private CinematicSprite? _title;
    private CinematicSprite? _secondTitle;
    private TimelineSet _timelines = TimelineSet.None;
    private DataNode? _data;
    private Phase _phase;
    private int _wait;
    private int _counter;
    private double _scrollX = -44, _scrollY = -112;
    private int _size = 256;
    private int _angle;
    private bool _floorOverSprites;
    private int _blockWidth = 1;
    private int _spawnerUpdates;
    private int _titleFade = TitleFadeUpdates;
    private bool _titleFadesIn;
    /// <summary>How many updates the great blast has shown itself on; none while there is no blast.</summary>
    private int _blastUpdate;
    private bool _blasting;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    private TurningFloor? _floor;
    private Node2D? _sprites;
    private MosaicPicture? _zebes;
    private Polygon2D? _blastGlow;
    private ScreenFade? _fade;

    /// <inheritdoc/>
    protected override bool CanBeSkipped => false;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _timelines = TimelineSet.ReadFrom(_data);
        var layer = new CanvasLayer { Name = "Picture", Layer = 1 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        _zebes = new MosaicPicture { Name = "Zebes", Centered = false, Visible = false, Texture = Session.Content.LoadTexture(_data.Text("zebes")) };
        layer.AddChild(_zebes);
        _floor = new TurningFloor { Name = "Floor", Repeats = false, Visible = false };
        layer.AddChild(_floor);
        _sprites = new Node2D { Name = "Sprites" };
        layer.AddChild(_sprites);
        // The great blast adds its colour to everything inside its oval.
        var oval = new Vector2[BlastOvalPoints];
        float down = (float)_data.Int("blast_height_parts") / _data.Int("blast_width_parts");
        for (int point = 0; point < oval.Length; point++)
        {
            float angle = Mathf.Tau * point / oval.Length;
            oval[point] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * down);
        }

        _blastGlow = new Polygon2D
        {
            Name = "Blast", Polygon = oval, Visible = false, Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add },
        };
        layer.AddChild(_blastGlow);
        _fade = new ScreenFade { Name = "Fade", Layer = 2, Levels = FadeLevels };
        AddChild(_fade);
        _fade.FadeOut(1);
        _fade.Step();
        _phase = Phase.Opening;
    }

    /// <inheritdoc/>
    public override bool GivesMusicItsTurn => !WaitsOutDisplayFrame;

    /// <inheritdoc/>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <summary>
    /// Whether the update finishes one of the opening's waits for a frame of the display: each after the first, up to
    /// and with the one that sets the station up. Such an update gives the music and the sounds no turn.
    /// </summary>
    private bool WaitsOutDisplayFrame => Elapsed > 0 && Elapsed <= OpeningUpdates;

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        // Every update of the old destruction draws a random number first, but for those that finish its opening's waits
        // for a frame of the display.
        if (!WaitsOutDisplayFrame)
            Session.OpenUpdate();
        _closing = WaitsOutDisplayFrame ? ClosingSoundWork.None : ClosingSoundWork.Usual;
        if (elapsed < OpeningUpdates)
            return false;
        if (elapsed == OpeningUpdates)
            SetUpStation();
        else
            StepPhase();

        if (_phase <= Phase.FadingOutCeres)
            StepStation();

        // The lights and the flicker take their turn after what is seen has taken its own, from the update each begins on.
        _navigationLights?.Step();
        _engineFlicker?.Step();
        _counter++;
        ShowFloor();
        return _phase == Phase.Finished;
    }

    /// <inheritdoc/>
    protected override GameMode Next() =>
        SamusAboard ? new Menus.DeathBlackOut() : Menus.GameLoad.OfLanding(LandingRoom, LandingEntry);

    /// <summary>The blinking of the station's navigation lights on the floor, from the station's setup until the planet's.</summary>
    private ColourSteps? _navigationLights;

    /// <summary>The flicker of the gunship's engine, from the planet's setup; seen once the approach's floor is shown.</summary>
    private ColourSteps? _engineFlicker;

    private void SetUpStation()
    {
        ShowFloorPicture("station_floor");
        _navigationLights = new ColourSteps((_data ?? throw NotEntered()).Get("navigation_lights"), "NavigationLights");
        _navigationLights.ShowOn(Floor());
        Spawn(_spaceThings, "large_asteroids", Pixels.FromNumber(0.25), loops: true);
        Spawn(_spaceThings, "small_asteroids", Pixels.FromNumber(0.03125), loops: true);
        Spawn(_spaceThings, "vortex", Pixels.Zero, loops: false);
        Session.Sounds.RequestMusic("");
        Compatibility.CartridgeSounds.Of(Session).AskMusicSet("ceres");
        Session.Sounds.RequestMusic(SamusAboard ? Music.CeresExplosionWithSamusAboard : Music.CeresExplosion, MusicWait);
        (_phase, _spawnerUpdates) = (Phase.WaitingForMusic, 0);
    }

    private void StepPhase()
    {
        StepBlastGlow();
        ScreenFade fade = Fade();
        switch (_phase)
        {
            case Phase.WaitingForMusic:
                // The station fades in once the music asked for has all been carried out.
                if (Session.Sounds.HasQueuedMusic)
                    break;
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInAndDrifting;
                break;

            case Phase.FadingInAndDrifting:
                Drift();
                _size++;
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.ApproachingExplosion;
                break;

            case Phase.ApproachingExplosion:
                Drift();
                if (_size < 640)
                {
                    _size++;
                    break;
                }

                _size = 768;
                BlowUpStation();
                _phase = Phase.FlyingAwayFromExplosion;
                break;

            case Phase.FlyingAwayFromExplosion:
                _scrollX += 2;
                _angle--;
                if (_size < 16)
                    (_phase, _wait) = (Phase.HoldingAfterExplosion, ExplosionHoldUpdates);
                else
                    _size -= 16;
                break;

            case Phase.HoldingAfterExplosion:
                if (--_wait > 0)
                    break;
                fade.FadeOut(FadeUpdates);
                _phase = Phase.FadingOutCeres;
                break;

            case Phase.FadingOutCeres:
                fade.Step();
                if (!fade.IsDone)
                    break;
                if (SamusAboard)
                {
                    Session.Sounds.RequestMusic("");
                    _phase = Phase.Finished;
                    break;
                }

                // The planet is set up on the next update, which is black throughout.
                _phase = Phase.SettingUpZebes;
                break;

            case Phase.SettingUpZebes:
                SetUpZebes();
                _phase = Phase.FadingInZebes;
                break;

            case Phase.FadingInZebes:
                StepBlocks();
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.ClearingZebes;
                break;

            case Phase.ClearingZebes:
                if (StepBlocks())
                    SetUpApproach();
                break;

            case Phase.ShowingTitle:
                StepZebes(sliding: false);
                break;

            case Phase.FlyingTowardZebes:
                _scrollX -= 0.5;
                _scrollY += 0.125;
                if (_size < 1152)
                    _size += 4;
                else
                    _phase = Phase.TurningTowardZebes;
                StepZebes(sliding: false);
                break;

            case Phase.TurningTowardZebes:
                _scrollX -= 0.5;
                _scrollY += 0.125;
                if (Math.Floor(_scrollX) < -128)
                {
                    _phase = Phase.ClosingInOnZebes;
                }
                else
                {
                    _size += 16;
                    _angle--;
                }

                StepZebes(sliding: false);
                break;

            case Phase.ClosingInOnZebes:
                _scrollX += 0.125;
                _scrollY += 0.125;
                if (_size < 8192)
                {
                    _size += 32;
                }
                else
                {
                    // The gunship has gone down into the planet: only the planet and its stars are left.
                    Floor().Visible = false;
                    (_phase, _wait) = (Phase.HoldingCloseToZebes, ZebesHoldUpdates);
                }

                StepZebes(sliding: false);
                break;

            case Phase.HoldingCloseToZebes:
                StepZebes(sliding: false);
                if (--_wait <= 0)
                    _phase = Phase.SlidingAway;
                break;

            case Phase.SlidingAway:
                StepZebes(sliding: true);
                break;

            case Phase.Finished:
                break;

            default:
                throw new InvalidOperationException($"The destruction of Ceres has no phase {_phase}.");
        }
    }

    /// <summary>The station drifts across the view.</summary>
    private void Drift()
    {
        _scrollX -= 0.25;
        _scrollY += 0.0625;
    }

    /// <summary>Plays one update of what is seen round the station: the blasts breaking out over it, and what drifts by.</summary>
    private void StepStation()
    {
        // The blasts come by the clock: a first wave, six more one after another, and a last, larger wave.
        _spawnerUpdates++;
        if (_spawnerUpdates == 129)
        {
            foreach ((int x, int y, int delay) in FirstBlasts)
                SpawnBlast("small_explosion", x, y, delay);
        }
        else if (_spawnerUpdates > 198 && _spawnerUpdates <= 198 + 12 * FollowingBlasts.Length && (_spawnerUpdates - 198) % 12 == 0)
        {
            (int x, int y) = FollowingBlasts[(_spawnerUpdates - 198) / 12 - 1];
            SpawnBlast("repeating_small_explosion", x, y, 1);
        }
        else if (_spawnerUpdates == 198 + 12 * FollowingBlasts.Length + 3)
        {
            foreach ((int x, int y, int delay) in LastBlasts)
                SpawnBlast("large_explosion", x, y, delay);
        }

        foreach (List<CinematicSprite> sprites in new[] { _blasts, _spaceThings })
        {
            for (int index = sprites.Count - 1; index >= 0; index--)
            {
                CinematicSprite sprite = sprites[index];
                sprite.Step();
                if (!sprite.IsOver)
                    continue;
                sprite.QueueFree();
                sprites.RemoveAt(index);
            }
        }
    }

    private void SpawnBlast(string timeline, int x, int y, int delay)
    {
        var at = PixelPosition.FromWhole(
            (int)CeresCentre.X - (int)Math.Floor(_scrollX) + x, (int)CeresCentre.Y - (int)Math.Floor(_scrollY) + y);
        CinematicSprite blast = CinematicSprite.Of(Session.Content, _timelines.Get(timeline), at);
        blast.Velocity = BlastDrift;
        blast.Delay(delay - 1);
        Sprites().AddChild(blast);
        // Blasts are drawn under the small asteroids and the vortex, which were put up before them.
        Sprites().MoveChild(blast, 1);
        _blasts.Add(blast);
    }

    /// <summary>The station goes up in one great blast.</summary>
    private void BlowUpStation()
    {
        SpawnBlast("station_blast", 0, 0, 1);
        (_blasting, _blastUpdate) = (true, 0);
        Glow().Position = new Vector2(CeresCentre.X - (float)Math.Floor(_scrollX), CeresCentre.Y - (float)Math.Floor(_scrollY));
        Session.Sounds.Request(Data().Text("blast_sound"), channel: "explosions", priority: SoundPriority.Essential);
        if (SamusAboard)
            Floor().Visible = false;
        else
            ShowFloorPicture("explosion_floor");
    }

    /// <summary>Shows the great blast as it stands on this update, before anything else of the update; done when its glow has faded.</summary>
    private void StepBlastGlow()
    {
        if (!_blasting)
            return;
        Polygon2D glow = Glow();
        IReadOnlyList<DataNode> ovals = Data().List("blast_ovals"), glows = Data().List("blast_glows");
        if (_blastUpdate >= ovals.Count)
        {
            (_blasting, glow.Visible) = (false, false);
            return;
        }

        int across = ovals[_blastUpdate].AsInt(), colour = glows[_blastUpdate].AsInt();
        _blastUpdate++;
        glow.Visible = across > 0;
        glow.Scale = new Vector2(across, across);
        glow.Color = new Color(((colour >> 16) & 255) / 255f, ((colour >> 8) & 255) / 255f, (colour & 255) / 255f);
    }

    private void SetUpZebes()
    {
        foreach (CinematicSprite sprite in _spaceThings.Concat(_blasts))
            sprite.QueueFree();
        _spaceThings.Clear();
        _blasts.Clear();
        (_blasting, Glow().Visible) = (false, false);
        Floor().Visible = false;
        (_scrollX, _scrollY, _angle, _size) = (0, 0, 0, 256);
        // The station's lights end here, and the gunship's engine begins to flicker; it is seen on the approach's floor.
        _navigationLights?.Stop();
        _navigationLights = null;
        _engineFlicker = new ColourSteps((_data ?? throw NotEntered()).Get("engine_flicker"), "EngineFlicker");
        MosaicPicture zebes = _zebes ?? throw NotEntered();
        zebes.Visible = true;
        _blockWidth = 9;
        zebes.BlockWidth = _blockWidth;
        Fade().FadeIn(FadeUpdates);
    }

    /// <summary>Makes Zebes a little clearer every fourth update, and returns whether it has come clear.</summary>
    private bool StepBlocks()
    {
        if (_counter % 4 != 0)
            return false;
        _blockWidth = _blockWidth == 1 ? 16 : _blockWidth - 1;
        (_zebes ?? throw NotEntered()).BlockWidth = _blockWidth;
        return _blockWidth == 1;
    }

    /// <summary>The picture of Zebes gives way to the planet, its stars, its name and the gunship.</summary>
    private void SetUpApproach()
    {
        (_zebes ?? throw NotEntered()).Visible = false;
        _scrollX = 128 + (_scrollX - Math.Floor(_scrollX));
        _scrollY = -104 + (_scrollY - Math.Floor(_scrollY));
        (_angle, _size) = (32, 256);
        ShowFloorPicture("approach_floor");
        (_engineFlicker ?? throw NotEntered()).ShowOn(Floor());
        _floorOverSprites = true;
        _sliders.Add((Spawn(null, "planet", Pixels.Zero, loops: false), Pixels.FromNumber(0.25), false));
        foreach (string stars in new[] { "stars_upper_left", "stars_upper_right", "stars_lower_left", "stars_lower_right" })
            _sliders.Add((Spawn(null, stars, Pixels.Zero, loops: false), Pixels.FromNumber(0.125), stars == "stars_lower_right"));
        _title = Spawn(null, "title", Pixels.Zero, loops: false);
        _title.Tint(new Color(1, 1, 1, 0));
        Node floor = Floor();
        floor.GetParent().MoveChild(floor, -1);
        _phase = Phase.ShowingTitle;
    }

    /// <summary>Plays one update of what is seen round Zebes.</summary>
    private void StepZebes(bool sliding)
    {
        if (_title is { } title)
        {
            foreach (string cue in title.Step())
                OnTitleCue(cue);
            if (_titleFade < TitleFadeUpdates)
            {
                _titleFade++;
                float shown = (float)_titleFade / TitleFadeUpdates;
                var tint = new Color(1, 1, 1, _titleFadesIn ? shown : 1 - shown);
                title.Tint(tint);
                _secondTitle?.Tint(tint);
            }

            if (title.IsOver)
            {
                title.QueueFree();
                _title = null;
            }
        }

        if (_secondTitle is { } second)
        {
            second.Step();
            if (second.IsOver)
            {
                second.QueueFree();
                _secondTitle = null;
            }
        }

        for (int index = _sliders.Count - 1; index >= 0; index--)
        {
            (CinematicSprite sprite, Pixels gain, bool last) = _sliders[index];
            if (sliding)
            {
                // Everything slides up off the screen, faster and faster; when the last of it has gone, so has the scene.
                Pixels speed = _slideSpeeds.GetValueOrDefault(sprite) + gain;
                _slideSpeeds[sprite] = speed;
                sprite.Place = new PixelPosition(sprite.Place.X, sprite.Place.Y - speed);
                if (sprite.Place.Y.Whole < -128)
                {
                    if (last)
                    {
                        _phase = Phase.Finished;
                        return;
                    }

                    sprite.QueueFree();
                    _sliders.RemoveAt(index);
                    continue;
                }
            }

            sprite.Step();
        }
    }

    private void OnTitleCue(string cue)
    {
        switch (cue)
        {
            case "fade_in":
                (_titleFade, _titleFadesIn) = (0, true);
                break;
            case "second_title":
                if (!Data().List("second_title_languages").Any(language => language.AsText() == Session.Options.Language))
                    break;
                // It shows its first look on this update, stepped once with the title.
                _secondTitle = Spawn(null, "second_title", Pixels.Zero, loops: false);
                break;
            case "fade_out":
                (_titleFade, _titleFadesIn) = (0, false);
                break;
            case "fly":
                // The gunship comes in from behind the view, huge, and shrinks away toward the planet.
                _scrollX = 62 + (_scrollX - Math.Floor(_scrollX));
                _scrollY = -112 + (_scrollY - Math.Floor(_scrollY));
                (_angle, _size) = (32, 16);
                _phase = Phase.FlyingTowardZebes;
                break;
            default:
                throw new InvalidOperationException($"The title of Zebes gives the cue \"{cue}\", which the destruction of Ceres does not know.");
        }
    }

    /// <summary>Puts up something the data names, at the place the data gives it.</summary>
    private CinematicSprite Spawn(List<CinematicSprite>? into, string name, Pixels drift, bool loops)
    {
        DataNode place = (_data ?? throw NotEntered()).Get("places").Get(name);
        CinematicSprite sprite = CinematicSprite.Of(
            Session.Content, _timelines.Get(name), PixelPosition.FromWhole(place.Int("x"), place.Int("y")));
        sprite.Name = name;
        sprite.Velocity = new PixelVelocity(drift, Pixels.Zero);
        sprite.LoopWidth = loops ? 512 : 0;
        Sprites().AddChild(sprite);
        into?.Add(sprite);
        return sprite;
    }

    private void ShowFloorPicture(string name)
    {
        TurningFloor floor = Floor();
        floor.Picture = (_data ?? throw NotEntered()).Text(name);
        floor.Setup(Session.Content, new Vector2I(256, 224));
        floor.Visible = true;
    }

    /// <summary>Shows the floor as the view now has it. At its very nearest one point of it fills the screen.</summary>
    private void ShowFloor()
    {
        TurningFloor floor = Floor();
        if (!floor.Visible)
            return;
        floor.Turn(
            _angle * Math.Tau / 256, Math.Max(_size, 1) / 256.0, _floorOverSprites ? ZebesCentre : CeresCentre,
            new Vector2((float)Math.Floor(_scrollX), (float)Math.Floor(_scrollY)));
    }

    private DataNode Data() => _data ?? throw NotEntered();

    private ScreenFade Fade() => _fade ?? throw NotEntered();

    private TurningFloor Floor() => _floor ?? throw NotEntered();

    private Node2D Sprites() => _sprites ?? throw NotEntered();

    private Polygon2D Glow() => _blastGlow ?? throw NotEntered();

    private static InvalidOperationException NotEntered() => new("The destruction of Ceres has not begun.");
}
