using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The end of the opening: Samus's gunship flying to the space colony. It is seen from in front, rushing at the
/// view through the stars until it fills it; then, after a flash of white, from behind, shrinking toward the station
/// among drifting asteroids; and then the colony's name is typed out and the screen goes dark.
/// </summary>
/// <remarks>
/// The data names the gunship's pictures ("front_floor", "rear_floor"), the "flight_backdrop" colour, the timeline
/// and place of the "stars" it rushes through, what is seen from behind it ("rear_view": each thing's name, which
/// is the name of its timeline and place, how far it drifts each update, and whether it drifts round a loop), the
/// name of the caption's words ("caption"), the font they are typed in ("caption_font"), and the picture of the
/// caption's second wording ("caption_second_picture"), with the places "caption" and "caption_second".
/// </remarks>
public partial class Intro
{
    private enum Flight
    {
        AskingForMusic,
        WaitingForMusic,
        FlyingIntoCamera,
        FlyingTowardCeres,
        Caption,
        FadingOut,
        Finished,
    }

    /// <summary>How many updates pass between the caption's letters, how long it is held, and how long its second wording takes to follow.</summary>
    private const int CaptionLetterUpdates = 16, CaptionHoldUpdates = 128;

    /// <summary>The point of the gunship's picture that stays put as it turns.</summary>
    private static readonly Vector2 FlightCentre = new(56, 24);

    /// <summary>How wide the loop is that what drifts round one drifts round.</summary>
    private const int DriftLoopWidth = 512;

    private readonly List<CinematicSprite> _rearView = [];
    private Flight _flight;
    private TurningFloor? _gunship;
    private CinematicSprite? _stars;
    private Node2D? _flightSprites;
    private ScreenFade? _flash;
    private PixelText? _caption;
    private Sprite2D? _captionSecond;
    private double _flightScrollX, _flightScrollY;
    private double _starsSpeed;
    private int _flightSize;
    private int _flightAngle;
    private int _captionShown;
    private int _captionWait;

    /// <summary>Sets up the flight, in the dark; it asks for its music on its first update.</summary>
    private void BeginFlight()
    {
        var layer = new CanvasLayer { Name = "Flight", Layer = 3 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(Data().Text("flight_backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        _flightSprites = new Node2D { Name = "Sprites" };
        layer.AddChild(_flightSprites);
        _stars = Sprite("stars", "stars");
        _flightSprites.AddChild(_stars);
        _gunship = new TurningFloor { Name = "Gunship", Repeats = false, Picture = Data().Text("front_floor") };
        layer.AddChild(_gunship);
        _gunship.Setup(Session.Content, new Vector2I(256, 224));
        DataNode captionAt = Data().Get("places").Get("caption");
        _caption = new PixelText
        {
            Name = "Caption", Font = PixelFont.Read(Session.Content, Data().Text("caption_font")), Position = new Vector2(captionAt.Int("x"), captionAt.Int("y")),
            Text = Session.Text.Get(Data().Text("caption")), VisibleCharacters = 0, Visible = false,
        };
        layer.AddChild(_caption);
        _captionSecond = Picture("CaptionSecond", "caption_second_picture", "caption_second");
        _captionSecond.Visible = false;
        layer.AddChild(_captionSecond);
        _flash = new ScreenFade { Name = "Flash", Layer = 4, Colour = Colors.White, Levels = FadeLevels };
        AddChild(_flash);

        (_flightScrollX, _flightScrollY, _flightSize, _flightAngle, _starsSpeed) = (-72, -104, 512, 224, -4);
        _flight = Flight.AskingForMusic;
        ShowGunship();
    }

    /// <summary>The flicker of the gunship's engine, from the update the view from behind is set up; none before.</summary>
    private ColourSteps? _engineFlicker;

    /// <summary>The blinking of the station's navigation lights, from the same update; none before.</summary>
    private ColourSteps? _navigationLights;

    /// <summary>Plays one update of the flight and returns whether it is over.</summary>
    private bool StepFlight()
    {
        ScreenFade fade = _fade ?? throw NotEntered();
        CinematicSprite stars = _stars ?? throw NotEntered();
        switch (_flight)
        {
            case Flight.AskingForMusic:
                // The old flight asks for its music on the update after the opening's fade reaches black, not on that update.
                Compatibility.CartridgeSounds.Of(Session).AskMusicSet("ceres");
                Session.Sounds.RequestMusic(Music.CeresFlight, SceneMusicWait);
                _flight = Flight.WaitingForMusic;
                break;

            case Flight.WaitingForMusic:
                // The flight is shown once the music asked for has all been carried out; the stars play on while it waits.
                if (Session.Sounds.HasQueuedMusic)
                    break;
                fade.FadeIn(1);
                fade.Step();
                _flight = Flight.FlyingIntoCamera;

                // The old flight's rush already runs on the update it finds the music ready, before the gunship draws back.
                Rush(stars);
                break;

            case Flight.FlyingIntoCamera:
                if (_flightSize >= 32)
                {
                    _flightSize -= 16;
                }
                else
                {
                    TurnToRearView();
                    break;
                }

                Rush(stars);
                break;

            case Flight.FlyingTowardCeres:
                (_flash ?? throw NotEntered()).Step();
                _flightScrollX -= 0.125;
                if (_flightSize < 3072)
                    _flightSize += 16;
                else if (_flightSize < 8192)
                    _flightSize += 32;
                else
                    BeginCaption();
                break;

            case Flight.Caption:
                StepCaption(fade);
                break;

            case Flight.FadingOut:
                fade.Step();
                if (fade.IsDone)
                    _flight = Flight.Finished;
                break;

            case Flight.Finished:
                return true;

            default:
                throw new InvalidOperationException($"The flight to the colony has no stage {_flight}.");
        }

        if (_flight is Flight.FlyingTowardCeres or Flight.Caption or Flight.FadingOut)
        {
            foreach (CinematicSprite seen in _rearView)
                seen.Step();

            // The engine's flicker and the station's lights take their turn after what is seen has taken its own,
            // from the update the view from behind is set up.
            _engineFlicker?.Step();
            _navigationLights?.Step();
        }
        else if (_flight != Flight.Finished)
        {
            stars.Step();
        }

        ShowGunship();
        return false;
    }

    /// <summary>The stars rush past, faster and faster, and the gunship with them.</summary>
    private void Rush(CinematicSprite stars)
    {
        _starsSpeed += 0.5;
        stars.Place += new PixelOffset(Pixels.FromNumber(_starsSpeed), Pixels.FromNumber(_starsSpeed));
        _flightScrollX += _starsSpeed;
        _flightScrollY += _starsSpeed;
    }

    /// <summary>The view cuts, in a flash of white, to behind the gunship as it makes for the station.</summary>
    private void TurnToRearView()
    {
        (_flightScrollX, _flightScrollY, _flightAngle) = (-32, -128, 32);
        (_stars ?? throw NotEntered()).Visible = false;
        TurningFloor gunship = _gunship ?? throw NotEntered();
        gunship.Picture = Data().Text("rear_floor");
        gunship.Setup(Session.Content, new Vector2I(256, 224));
        Node2D sprites = _flightSprites ?? throw NotEntered();

        // The gunship's engine flickers on its picture, and the station's lights blink on what the data says is lit.
        _engineFlicker = new ColourSteps(Data().Get("engine_flicker"), "EngineFlicker");
        _engineFlicker.ShowOn(gunship);
        _navigationLights = new ColourSteps(Data().Get("navigation_lights"), "NavigationLights");

        // The data lists what is seen from the nearest to the farthest; the farthest is laid down first.
        foreach (DataNode thing in Data().List("rear_view").Reverse())
        {
            string name = thing.Text("name");
            CinematicSprite seen = Sprite(name, name);
            seen.Velocity = new PixelVelocity(thing.Distance("drift"), Pixels.Zero);
            seen.LoopWidth = thing.Flag("loops") ? DriftLoopWidth : 0;
            sprites.AddChild(seen);
            _rearView.Add(seen);
            if (!thing.Flag("lit"))
                continue;
            foreach (IRecoloured visual in seen.FindChildren("*", owned: false).OfType<IRecoloured>())
                _navigationLights.ShowOn(visual);
        }

        ScreenFade flash = _flash ?? throw NotEntered();
        flash.FadeOut(1);
        flash.Step();
        flash.FadeIn(FadeLevels);
        _flight = Flight.FlyingTowardCeres;
    }

    /// <summary>The gunship is lost in the distance, and the colony's name begins to be typed.</summary>
    private void BeginCaption()
    {
        (_gunship ?? throw NotEntered()).Visible = false;
        (_caption ?? throw NotEntered()).Visible = true;
        (_flight, _captionShown, _captionWait) = (Flight.Caption, 0, 1);
    }

    private void StepCaption(ScreenFade fade)
    {
        if (--_captionWait > 0)
            return;
        PixelText caption = _caption ?? throw NotEntered();
        string words = caption.Text;
        if (_captionShown < words.Length)
        {
            // A letter at a time, a space coming with the letter after it.
            _captionShown++;
            while (_captionShown < words.Length && char.IsWhiteSpace(words[_captionShown - 1]))
                _captionShown++;
            caption.VisibleCharacters = _captionShown;
            _captionWait = CaptionLetterUpdates;
            if (_captionShown < words.Length)
                return;
            (_captionSecond ?? throw NotEntered()).Visible = IsSubtitled;

            _captionWait += CaptionHoldUpdates;
            return;
        }

        fade.FadeOut(FadeUpdates);
        _flight = Flight.FadingOut;
    }

    /// <summary>Shows the gunship as the view now has it.</summary>
    private void ShowGunship()
    {
        TurningFloor gunship = _gunship ?? throw NotEntered();
        if (!gunship.Visible)
            return;
        gunship.Turn(
            _flightAngle * Math.Tau / 256, Math.Max(_flightSize, 1) / 256.0, FlightCentre,
            new Vector2((float)Math.Floor(_flightScrollX), (float)Math.Floor(_flightScrollY)));
    }
}
