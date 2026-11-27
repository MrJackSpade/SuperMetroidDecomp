using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Game.Menus;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The ending. The gunship climbs out through Zebes's clouds; the planet cracks, glows and blows apart; the gunship
/// flies out of the blast and away, and the mission's result and clear time are typed over it. The credits roll.
/// Then, among shooting stars, Samus is seen waiting, shows herself as the clear time has earned, leaps and fires at
/// the screen; her emblem comes together, the share of items found is typed, and she signs off.
/// </summary>
/// <remarks>
/// It is written across three files: this one is its order of events, its floor and the things it shows; the others
/// are the credits and waiting, and Samus's reward and sign-off. Its data names the timelines of what it shows, its
/// pictures, and the names of its words.
/// </remarks>
public partial class Ending : Cinematic
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "cinematics/ending.json";

    /// <summary>The hours of play the game was cleared in.</summary>
    public int ClearHours { get; set; }

    /// <summary>The minutes of play past those hours.</summary>
    public int ClearMinutes { get; set; }

    /// <summary>How many of the hundred things to find were found.</summary>
    public int ItemsFound { get; set; }

    /// <summary>Whether the animals were let out during the escape.</summary>
    public bool AnimalsEscaped { get; set; }

    private enum Phase
    {
        Setup,
        WaitingForEscapeMusic,
        FadingInSideClouds,
        SideClouds,
        FadingOutSideClouds,
        FadingInTopAndBottomClouds,
        TopAndBottomClouds,
        FadingOutTopAndBottomClouds,
        FadingInExplosion,
        ExplosionCrossfade,
        ExplosionHold,
        ExplosionAnimation,
        WaitingForCreditsMusic,
        WaitingForCreditsMusicToCome,
        PlanetEscapeFast,
        PlanetEscapeSlow,
        PlanetEscapeAccelerating,
        CompletionText,
        FadingOutToCredits,
        Credits,
        PostCreditsBlank,
        PostCreditsFadeIn,
        ShootingStars,
        WaitingBackdrop,
        WaitingSamus,
        RewardReveal,
        Gesture,
        Jump,
        Shot,
        WhiteFlash,
        Logo,
        ItemPercentage,
        ItemPercentageDrop,
        FinalMessage,
    }

    private enum Motion
    {
        Still,
        SideCloudFromRight,
        SideCloudFromLeft,
        CloudDown,
        CloudUp,
        ExplosionLayer,
        FlyawayStars,
        FinaleExit,
        AnimalPod,
    }

    /// <summary>One thing the ending shows, with the way it moves.</summary>
    private sealed class Actor(string role, CinematicSprite sprite, Motion motion)
    {
        public string Role => role;
        public CinematicSprite Sprite => sprite;
        public Motion Motion => motion;
        public bool Released { get; set; }
        public bool Moving { get; set; }
        public Pixels Speed { get; set; }

        /// <summary>Colours of its own it is shown in, given it once it shows its first picture; null once given, or for none.</summary>
        public ColourChange? Colours { get; set; }
    }

    /// <summary>How many updates the set-up waits out before the update that sets the clouds up.</summary>
    private const int SetupWaits = 9;

    /// <summary>How many turns of the music's queue each of its pieces of music waits once at its front.</summary>
    private const int MusicWait = 14;

    /// <summary>How many updates a quick fade of the whole screen takes, and how many steps it goes in.</summary>
    private const int FadeUpdates = 30, FadeLevels = 15;

    /// <summary>How many steps a wash of white over the whole screen clears in: the cartridge's thirty-one levels of added white.</summary>
    private const int WashLevels = 31;

    /// <summary>The name the floor's colours are changed under for the stage of the ending it is shown in.</summary>
    private const string FloorColours = "Stage";

    /// <summary>The name the exploding planet's colours are changed under: its own, and then its fade's.</summary>
    private const string PlanetColours = "Planet";

    /// <summary>The points of the floor that stay put as it turns: among the clouds and over the planet, and behind the gunship.</summary>
    private static readonly Vector2 AtmosphereCentre = new(128, 128), GunshipCentre = new(56, 24);

    /// <summary>How the gunship is shaken from side to side as it leaves fast, and then slowly.</summary>
    private static readonly double[] FastShake = [0.5, 0.5, 0.5, 0.5, -0.5, -0.5, 0.5, 0.5, 0.5, 0.5, -0.5, -0.5, 0.5, 0.5, -0.5, -0.5];
    private static readonly double[] SlowShake = [1, 1, 1, -1, -1, 1, 1, -1];

    private readonly List<Actor> _actors = [];
    private DataNode? _data;
    private TimelineSet _timelines = TimelineSet.None;
    private Phase _phase;
    private int _wait;
    private int _setupWaited;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;
    private int _frame;

    private ScreenFade? _fade;
    private ScreenFade? _white;
    private CanvasLayer? _front;
    private CanvasLayer? _picture;
    private ColorRect? _backdrop;
    private TurningFloor? _floor;
    private Node2D? _sprites;
    private Sprite2D? _explosionPicture;
    private Sprite2D? _explosionGlow;
    private readonly Dictionary<string, PixelText> _resultTexts = [];

    private double _floorX, _floorY;
    private int _floorSize = 256;
    private int _floorAngle;
    private Vector2 _floorCentre = AtmosphereCentre;
    private int _crossfade;

    /// <summary>The fade of the exploding planet's colours, from its cue until the planet goes; none before or after.</summary>
    private ColourSteps? _planetFade;

    /// <summary>
    /// The other steps of colours playing on what the ending shows, in the order they began: each from the update the
    /// old game starts it until the scene it plays in ends or what it is shown on goes.
    /// </summary>
    private readonly List<ColourSteps> _colourSteps = [];

    /// <summary>The gunship's coming out of the blast's white, and the planet's afterglow, while they play.</summary>
    private ColourSteps? _gunshipEmergence, _afterglowGlow;
    private int _shake;
    private bool _slowShake;
    private double _planetSpeed;
    private int _whiteHold;
    private string _typing = "";
    private int _typed;
    private int _typeWait;
    private int _digitsShown;

    /// <summary>The steps of white the wash over the screen has left, how many updates each takes, and how many until the next.</summary>
    private int _washLeft, _washPace, _washWait;

    /// <inheritdoc/>
    protected override bool CanBeSkipped => false;

    /// <inheritdoc/>
    public override bool GivesMusicItsTurn => !WaitsOutDisplayFrame;

    /// <inheritdoc/>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <summary>
    /// Whether the update finishes a wait of the set-up for a frame of the display, as every update of the old set-up
    /// after its first does, the one that sets the clouds up included: such an update gives the music and the sounds no turn.
    /// </summary>
    private bool WaitsOutDisplayFrame => _phase == Phase.Setup && _setupWaited > 0;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _timelines = TimelineSet.ReadFrom(_data);
        // The colour behind everything lies on a layer of its own beneath the picture. The stars, the shooting stars
        // and the emblem's picture lie at depths below the rest of the picture's layer, and a colour laid in the same
        // layer would hide them.
        var behind = new CanvasLayer { Name = "Behind", Layer = 0 };
        AddChild(behind);
        _backdrop = new ColorRect { Name = "Backdrop", Color = Colors.Black };
        _backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        behind.AddChild(_backdrop);
        _picture = new CanvasLayer { Name = "Picture", Layer = 1 };
        AddChild(_picture);
        _floor = new TurningFloor { Name = "Floor", Visible = false };
        _picture.AddChild(_floor);
        _explosionPicture = new Sprite2D { Name = "Explosion", Centered = false, Visible = false };
        _picture.AddChild(_explosionPicture);
        _sprites = new Node2D { Name = "Sprites" };
        _picture.AddChild(_sprites);
        _explosionGlow = new Sprite2D
        {
            Name = "Glow", Centered = false, Visible = false, Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add },
        };
        _picture.AddChild(_explosionGlow);
        foreach ((string words, DataNode line) in _data.Get("result_texts").Entries())
        {
            var text = new PixelText
            {
                Name = words, Font = PixelFont.Read(Session.Content, line.Text("font")), Position = new Vector2(line.Int("x"), line.Int("y")),
                Text = Session.Text.Get(words), VisibleCharacters = 0, Visible = false,
            };
            _picture.AddChild(text);
            _resultTexts[words] = text;
        }
        _white = new ScreenFade { Name = "White", Layer = 3, Colour = Colors.White };
        AddChild(_white);

        // What passes in front of the white over the screen: the gunship as it flies away, and her shot after it.
        _front = new CanvasLayer { Name = "Front", Layer = 4 };
        AddChild(_front);
        _fade = new ScreenFade { Name = "Fade", Layer = 5, Levels = FadeLevels };
        AddChild(_fade);
        _fade.FadeOut(1);
        _fade.Step();
        (_phase, _setupWaited) = (Phase.Setup, 0);
    }

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        // Every update of the old credits draws a random number first, but for one that finishes a wait for a frame of
        // the display.
        if (!WaitsOutDisplayFrame)
            Session.OpenUpdate();
        _closing = WaitsOutDisplayFrame ? ClosingSoundWork.None : ClosingSoundWork.Usual;
        if (_phase == Phase.Setup)
        {
            // The old set-up waits out nine updates and sets the clouds up on the tenth.
            if (_setupWaited < SetupWaits)
            {
                _setupWaited++;
                return false;
            }

            // The update that sets the clouds up goes on, as the old ending's does, and counts as one of its updates.
            SetUpSideClouds();
            Session.Sounds.RequestMusic("");
            Compatibility.CartridgeSounds.Of(Session).AskMusicSet("zebes_explosion");
            Session.Sounds.RequestMusic(Music.ZebesExplosion, MusicWait);
            _phase = Phase.WaitingForEscapeMusic;
        }
        else
        {
            // Steps of colours begun on an earlier update take theirs before anything else of the update; one begun
            // on this update shows its first step as it begins.
            foreach (ColourSteps steps in _colourSteps)
                steps.Step();
            if (_phase < Phase.Credits)
                StepEscape();
            else if (_phase < Phase.Gesture)
                StepCreditsAndWaiting();
            else
                StepReward();
        }

        if (_phase >= Phase.PostCreditsBlank)
            StepShootingStars();
        _frame++;
        ShowFloor();
        return false;
    }

    /// <inheritdoc/>
    protected override GameMode Next() => new TitleMode();

    /// <summary>Puts up something the data has a timeline for, at a place, with the way it moves.</summary>
    private Actor Spawn(string role, string timeline, int x, int y, Motion motion, bool behind = false)
    {
        CinematicSprite sprite = CinematicSprite.Of(Session.Content, _timelines.Get(timeline), PixelPosition.FromWhole(x, y));
        sprite.Name = role;
        sprite.ZIndex = behind ? -1 : 0;
        (_sprites ?? throw NotEntered()).AddChild(sprite);
        var actor = new Actor(role, sprite, motion);
        _actors.Add(actor);
        return actor;
    }

    private void ClearActors()
    {
        foreach (Actor actor in _actors)
            actor.Sprite.QueueFree();
        _actors.Clear();
    }

    /// <summary>
    /// Moves and plays everything on show in order, acting on the cues its timelines give. Something a cue puts up
    /// after the one playing plays later in the same update; something put in ahead of it waits for the next.
    /// </summary>
    private void StepActors()
    {
        for (int place = 0; place < _actors.Count; place++)
        {
            Actor actor = _actors[place];
            bool stays = Move(actor);
            if (stays)
            {
                foreach (string cue in actor.Sprite.Step())
                    OnCue(cue);
                stays = !actor.Sprite.IsOver;
                if (actor.Colours is { } colours && colours.On(actor.Sprite).Count > 0)
                {
                    colours.ShowOn(actor.Sprite, actor.Role);
                    actor.Colours = null;
                }
            }

            place = _actors.IndexOf(actor);
            if (place < 0)
                throw new InvalidOperationException($"The ending's {actor.Role} was taken off the stage by its own update.");
            if (stays)
                continue;
            _actors.RemoveAt(place--);
            actor.Sprite.QueueFree();
        }
    }

    /// <summary>Moves one thing by its way of moving, and returns whether it is still there.</summary>
    private bool Move(Actor actor)
    {
        CinematicSprite sprite = actor.Sprite;
        switch (actor.Motion)
        {
            case Motion.Still:
                return true;

            case Motion.SideCloudFromRight:
            case Motion.SideCloudFromLeft:
                // The clouds at the sides hang until the view has drawn back far enough, then slide past.
                if (!actor.Released)
                {
                    actor.Released = _floorSize >= 96;
                    return true;
                }

                sprite.Place += actor.Motion == Motion.SideCloudFromRight ? PixelOffset.FromWhole(-1, -2) : PixelOffset.FromWhole(1, 2);
                return true;

            case Motion.CloudDown:
            case Motion.CloudUp:
                if (!actor.Released)
                {
                    actor.Released = _floorSize < 176;
                    return true;
                }

                sprite.Place += PixelOffset.FromWhole(0, actor.Motion == Motion.CloudDown ? 1 : -1);
                return true;

            case Motion.ExplosionLayer:
                // What lies over the planet goes when the planet does.
                return _actors.Any(other => other.Role == "exploding_planet");

            case Motion.FlyawayStars:
                if (_phase == Phase.CompletionText)
                    return false;
                if (actor.Released && !actor.Moving && _phase == Phase.PlanetEscapeFast)
                    (actor.Moving, actor.Speed) = (true, Pixels.FromNumber(0.25));
                else if (actor.Moving)
                {
                    actor.Speed -= Pixels.FromNumber(1.0 / 2048);
                    sprite.Place = new PixelPosition(sprite.Place.X + actor.Speed, sprite.Place.Y);
                }

                return true;

            case Motion.FinaleExit:
                return _phase != Phase.CompletionText;

            case Motion.AnimalPod:
                sprite.Place = new PixelPosition(sprite.Place.X + Pixels.FromWhole(1), sprite.Place.Y + Pixels.FromNumber(1.0 / 512));
                return sprite.Place.X.Whole < 272;

            default:
                throw new InvalidOperationException($"The ending has no way of moving named {actor.Motion}.");
        }
    }

    /// <summary>
    /// Shows the floor's picture for a shot, level and at its own size until it is set otherwise, repeated beyond its
    /// edges or with nothing beyond them as <paramref name="repeats"/> says.
    /// </summary>
    private void ShowFloorPicture(string name, Vector2 centre, bool repeats)
    {
        TurningFloor floor = _floor ?? throw NotEntered();
        // A picture shows its own colours until it is given others.
        ColourChange.None.ShowOn(floor, FloorColours);
        floor.Picture = Data().Text(name);
        floor.Repeats = repeats;
        floor.Setup(Session.Content, new Vector2I(256, 224));
        floor.Visible = true;
        _floorCentre = centre;
    }

    /// <summary>Shows the floor as the view now has it. At its very nearest one point of it fills the screen.</summary>
    private void ShowFloor()
    {
        TurningFloor floor = _floor ?? throw NotEntered();
        if (!floor.Visible)
            return;
        floor.Turn(
            _floorAngle * Math.Tau / 256, Math.Max(_floorSize, 1) / 256.0, _floorCentre,
            new Vector2((float)Math.Floor(_floorX), (float)Math.Floor(_floorY)));
    }

    private void SetFloor(double x, double y, int size, int angle)
    {
        // Setting the floor's place keeps the part of a pixel it had across.
        _floorX = x + (_floorX - Math.Floor(_floorX));
        (_floorY, _floorSize, _floorAngle) = (y, size, angle);
    }

    /// <summary>Washes the screen fully white; it clears a step on the first update it is stepped and every <paramref name="pace"/> updates after.</summary>
    private void BeginWhiteWash(int pace)
    {
        (_washLeft, _washPace, _washWait) = (WashLevels, pace, 1);
        (_white ?? throw NotEntered()).Cover(WashLevels, WashLevels);
    }

    /// <summary>Plays one update of the wash of white, which stays clear once it has cleared.</summary>
    private void StepWhiteWash()
    {
        if (_washLeft == 0 || --_washWait > 0)
            return;
        (_washLeft, _washWait) = (_washLeft - 1, _washPace);
        (_white ?? throw NotEntered()).Cover(_washLeft, WashLevels);
    }

    /// <summary>
    /// Begins the steps of colours the data names, shown on <paramref name="on"/> under <paramref name="source"/>: the
    /// first shows at once, and each later update steps them before anything else.
    /// </summary>
    private ColourSteps BeginColourSteps(string data, string source, params IRecoloured[] on)
    {
        var steps = new ColourSteps(Data().Get(data), source);
        foreach (IRecoloured target in on)
            steps.ShowOn(target);
        steps.Step();
        _colourSteps.Add(steps);
        return steps;
    }

    /// <summary>Ends steps of colours, taking their colours back from what they are shown on.</summary>
    private void StopColourSteps(ColourSteps steps)
    {
        steps.Stop();
        _colourSteps.Remove(steps);
    }

    /// <summary>Ends every step of colours still playing, as the old game does at the start of a new scene.</summary>
    private void StopAllColourSteps()
    {
        foreach (ColourSteps steps in _colourSteps)
            steps.Stop();
        _colourSteps.Clear();
    }

    private ScreenFade Fade() => _fade ?? throw NotEntered();

    private DataNode Data() => _data ?? throw NotEntered();

    private static InvalidOperationException NotEntered() => new("The ending has not begun.");

    /// <summary>
    /// How many of the hundred things to find an inventory holds: each tank and expansion, and each piece of
    /// equipment and beam that is found rather than given.
    /// </summary>
    public static int CountItems(Engine.Actors.Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        int count = inventory.Capacity(Amounts.Energy) / 100 + inventory.Capacity(Amounts.ReserveEnergy) / 100
            + inventory.Capacity(Amounts.Missiles) / 5 + inventory.Capacity(Amounts.SuperMissiles) / 5 + inventory.Capacity(Amounts.PowerBombs) / 5;
        string[] found =
        [
            Equipment.VariaSuit, Equipment.SpringBall, Equipment.MorphBall, Equipment.ScrewAttack, Equipment.GravitySuit,
            Equipment.HiJumpBoots, Equipment.SpaceJump, Equipment.Bombs, Equipment.SpeedBooster, Equipment.GrappleBeam,
            Equipment.XrayScope, Equipment.WaveBeam, Equipment.IceBeam, Equipment.Spazer, Equipment.PlasmaBeam, Equipment.ChargeBeam,
        ];
        count += found.Count(inventory.Owns);
        return Math.Clamp(count, 0, 100);
    }
}
