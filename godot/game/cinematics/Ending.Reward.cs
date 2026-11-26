using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The last of the ending: Samus as the clear time has earned her being seen, in her armour, with her helmet off,
/// or out of her suit. She makes her gesture, leaps off the top of the screen, falls back, lands and fires at the
/// view. Out of the flash her emblem comes together; the share of items found is typed, and then her sign-off.
/// </summary>
/// <remarks>
/// The data names her timelines ("suitless_idle", "suitless_legs_idle", "suited_idle", "helmet_idle", "head_idle";
/// "hair_release", "hair_release_legs", "gesture_body", "gesture_arm", "gesture_helmet", "gesture_head", of which the
/// ones that lead give the cue "jump"; "suitless_jump", "suited_jump", "helmet_jump", "head_jump", "falling" and
/// "landing", with the cues "ready_head", "launch_head", "launch", "fly", "head_fly" and "shoot"), the picture of her
/// shot ("shot_floor") and its sound ("shot_sound"), the emblem's timelines ("logo_upper_s", "logo_lower_s", "logo_upper_circle", which gives the
/// cue "grey_out", and "logo_lower_circle") and picture ("logo"), and her "last_words": by the name of each
/// ("ending_item_percentage", "ending_final_message") the font it is typed in and how far down the screen its first
/// line is, each line lying in the middle of the screen; under "percentage" the font and place of the figures
/// of the share of items found, which end in a per cent sign three cells along; and under "japanese_subtitle" the
/// picture shown, where its corner lies, under those figures while they are held, in the "languages" that show it.
/// </remarks>
public partial class Ending
{
    private enum RewardKind
    {
        Suitless,
        Helmetless,
        Armoured,
    }

    /// <summary>Where each part of her stands.</summary>
    private static readonly (int X, int Y) SuitlessAt = (120, 136), SuitedBodyAt = (120, 152), HelmetAt = (124, 108), HeadAt = (121, 107);

    /// <summary>How fast she leaves the ground, how much faster she falls each update, how high she goes before she falls back, and where she lands.</summary>
    private const double LaunchSpeed = -16, JumpGravity = 7.0 / 32;
    private const int FallStartY = -80, LandingY = 136;

    /// <summary>How long her words wait before they are typed, how long each letter takes, and how long the share of items is held.</summary>
    private const int TypingDelay = 64, TypingLetterUpdates = 4, PercentageUpdates = 128;

    private CinematicSprite? _jumpBody;
    private CinematicSprite? _jumpHead;
    private bool _bodyRising, _bodyFalling, _headRising;
    private double _jumpSpeed;
    private double _bodyY, _headY;
    private int _shotSize;
    private int _shotAngle;
    private bool _shotHolding;
    private int _shotAge;
    private int _whiteLeft;
    private readonly List<(CinematicSprite Half, int LandX, int LandY, int Way, int Speed, bool Settled)> _logoHalves = [];
    private readonly List<CinematicSprite> _logoCircles = [];
    private Sprite2D? _logoPicture;
    private int _logoFade = -1;
    private PixelText? _lastWords;
    private PixelText? _percentage;
    private Sprite2D? _subtitle;
    private bool _percentageShown;

    /// <summary>Which reward the clear time has earned.</summary>
    private RewardKind Reward => ClearHours < 3 ? RewardKind.Suitless : ClearHours < 10 ? RewardKind.Helmetless : RewardKind.Armoured;

    /// <summary>Puts Samus up as she waits.</summary>
    private void SpawnIdleSamus()
    {
        ClearActors();
        switch (Reward)
        {
            case RewardKind.Suitless:
                Spawn("samus", "suitless_idle", SuitlessAt.X, SuitlessAt.Y, Motion.Still);
                Spawn("samus", "suitless_legs_idle", SuitlessAt.X, SuitlessAt.Y, Motion.Still);
                break;
            case RewardKind.Helmetless:
                Spawn("samus", "suited_idle", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                Spawn("samus", "head_idle", HeadAt.X, HeadAt.Y, Motion.Still);
                break;
            default:
                Spawn("samus", "suited_idle", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                Spawn("samus", "helmet_idle", HelmetAt.X, HelmetAt.Y, Motion.Still);
                break;
        }
    }

    /// <summary>She begins her gesture, with nothing behind her but the stars.</summary>
    private void BeginGesture()
    {
        ClearActors();
        (_waitingBackdrop ?? throw NotEntered()).Visible = false;
        ShowPanel("", 0);
        switch (Reward)
        {
            case RewardKind.Suitless:
                Spawn("samus", "hair_release", SuitlessAt.X, SuitlessAt.Y, Motion.Still);
                Spawn("samus", "hair_release_legs", SuitlessAt.X, SuitlessAt.Y, Motion.Still);
                break;
            case RewardKind.Helmetless:
                Spawn("samus", "gesture_head", HeadAt.X, HeadAt.Y, Motion.Still);
                Spawn("samus", "gesture_body", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                Spawn("samus", "gesture_arm", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                break;
            default:
                Spawn("samus", "gesture_helmet", HelmetAt.X, HelmetAt.Y, Motion.Still);
                Spawn("samus", "gesture_body", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                Spawn("samus", "gesture_arm", SuitedBodyAt.X, SuitedBodyAt.Y, Motion.Still);
                break;
        }

        _phase = Phase.Gesture;
    }

    /// <summary>Plays one update of Samus's reward and sign-off.</summary>
    private void StepReward()
    {
        switch (_phase)
        {
            case Phase.Gesture:
                bool jumps = false;
                foreach (Actor actor in _actors.ToList())
                {
                    foreach (string cue in actor.Sprite.Step())
                    {
                        if (cue != "jump")
                            throw new InvalidOperationException($"Samus's gesture gives the cue \"{cue}\", which the ending does not know.");
                        jumps = true;
                    }
                }

                if (jumps)
                    BeginJump();
                break;

            case Phase.Jump:
                if (!StepJump())
                    break;
                Session.Sounds.Request(Data().Text("shot_sound"), channel: "weapons", priority: SoundPriority.Essential);
                ShowFloorPicture("shot_floor", new Vector2(104, 112), repeats: false);
                (_floorX, _floorY, _shotSize, _shotAngle, _shotHolding, _shotAge, _wait) = (-4, -8, 2304, 0, false, 0, 180);
                _phase = Phase.Shot;
                break;

            case Phase.Shot:
                StepShot();
                break;

            case Phase.WhiteFlash:
                (_white ?? throw NotEntered()).Step();
                StepJump();
                if (--_whiteLeft <= 0)
                    BeginLogo();
                break;

            case Phase.Logo:
                if (StepLogo())
                    BeginLastWords("ending_item_percentage", Phase.ItemPercentage);
                break;

            case Phase.ItemPercentage:
                if (!StepLastWords())
                    break;
                if (!_percentageShown)
                {
                    // The share of items found is put up beside its words and held.
                    PixelText percentage = _percentage ?? throw NotEntered();
                    percentage.Text = $"{ItemsFound,3}%";
                    percentage.Visible = true;
                    (_subtitle ?? throw NotEntered()).Visible = Data().Get("last_words").Get("japanese_subtitle").List("languages")
                        .Any(language => language.AsText() == Session.Options.Language);
                    (_percentageShown, _wait) = (true, PercentageUpdates);
                    break;
                }

                if (--_wait <= 0)
                {
                    // The line under the share goes as its hold ends.
                    (_subtitle ?? throw NotEntered()).Visible = false;
                    (_phase, _wait) = (Phase.ItemPercentageDrop, 40);
                }

                break;

            case Phase.ItemPercentageDrop:
                // Those words drop away down the screen, and her sign-off is typed in their place.
                (_lastWords ?? throw NotEntered()).Position += new Vector2(0, 2);
                (_percentage ?? throw NotEntered()).Position += new Vector2(0, 2);
                if (--_wait <= 0)
                    BeginLastWords("ending_final_message", Phase.FinalMessage);
                break;

            case Phase.FinalMessage:
                StepLastWords();
                break;

            default:
                throw new InvalidOperationException($"The ending's reward has no phase {_phase}.");
        }

        _floorSize = _shotSize;
        _floorAngle = _shotAngle;
    }

    private void BeginJump()
    {
        ClearActors();
        Node2D sprites = _sprites ?? throw NotEntered();
        (_jumpHead, _bodyRising, _bodyFalling, _headRising, _jumpSpeed) = (null, false, false, false, 0);
        if (Reward != RewardKind.Suitless)
        {
            (int x, int y) = Reward == RewardKind.Helmetless ? HeadAt : HelmetAt;
            _jumpHead = CinematicSprite.Of(
                Session.Content, _timelines.Get(Reward == RewardKind.Helmetless ? "head_jump" : "helmet_jump"), PixelPosition.FromWhole(x, y));
            sprites.AddChild(_jumpHead);
            _headY = y;
        }

        (int bodyX, int bodyY) = Reward == RewardKind.Suitless ? SuitlessAt : SuitedBodyAt;
        _jumpBody = CinematicSprite.Of(
            Session.Content, _timelines.Get(Reward == RewardKind.Suitless ? "suitless_jump" : "suited_jump"), PixelPosition.FromWhole(bodyX, bodyY));
        sprites.AddChild(_jumpBody);
        _bodyY = bodyY;
        _phase = Phase.Jump;
    }

    /// <summary>
    /// Plays one update of the leap and returns whether she fired. Her head and body share one speed: while both
    /// are rising it grows twice as fast.
    /// </summary>
    private bool StepJump()
    {
        bool shoots = false;
        CinematicSprite body = _jumpBody ?? throw NotEntered();
        if (_jumpHead is { IsOver: false } head)
        {
            if (_headRising)
            {
                _jumpSpeed += JumpGravity;
                _headY += _jumpSpeed;
                head.Place = PixelPosition.FromWhole(head.Place.X.Whole, (int)Math.Floor(_headY));
                head.Visible = _headY >= FallStartY;
            }

            foreach (string cue in head.Step())
                shoots |= OnJumpCue(cue);
        }

        if (_bodyRising || _bodyFalling)
        {
            _jumpSpeed += JumpGravity;
            _bodyY += _jumpSpeed;
            if (_bodyRising && _bodyY < FallStartY)
            {
                (_bodyRising, _bodyFalling) = (false, true);
                body.Play(_timelines.Get("falling"));
            }
            else if (_bodyFalling && _bodyY >= LandingY)
            {
                (_bodyFalling, _bodyY) = (false, LandingY);
                body.Play(_timelines.Get("landing"));
            }

            body.Place = PixelPosition.FromWhole(body.Place.X.Whole, (int)Math.Floor(_bodyY));
        }

        foreach (string cue in body.Step())
            shoots |= OnJumpCue(cue);
        return shoots;
    }

    private bool OnJumpCue(string cue)
    {
        CinematicSprite? head = _jumpHead;
        switch (cue)
        {
            case "ready_head":
                PlaceHead(head, Reward == RewardKind.Helmetless ? (120, 120) : (118, 120));
                return false;
            case "launch_head":
                PlaceHead(head, Reward == RewardKind.Helmetless ? (121, 116) : (120, 114));
                return false;
            case "launch":
                _jumpSpeed = LaunchSpeed;
                return false;
            case "fly":
                _bodyRising = true;
                return false;
            case "head_fly":
                _headRising = true;
                return false;
            case "shoot":
                return true;
            default:
                throw new InvalidOperationException($"Samus's leap gives the cue \"{cue}\", which the ending does not know.");
        }
    }

    private void PlaceHead(CinematicSprite? head, (int X, int Y) at)
    {
        if (head is null)
            return;
        head.Place = PixelPosition.FromWhole(at.X, at.Y);
        head.ShowPlace();
        _headY = at.Y;
    }

    /// <summary>Her shot comes at the view, spinning and swelling until it fills it; it is held, and then all is white.</summary>
    private void StepShot()
    {
        StepJump();
        _shotAge++;
        (_floor ?? throw NotEntered()).Modulate = new Color(1, 1, 1, Math.Min(1f, _shotAge / 32f));
        (_jumpBody ?? throw NotEntered()).Tint(new Color(1, 1, 1, 1 - Math.Clamp((_shotAge - 16) / 32f, 0f, 1f)));
        if (!_shotHolding)
        {
            _shotAngle -= 8;
            _shotSize -= 64;
            if (_shotSize < 24)
                (_shotSize, _shotHolding) = (24, true);
            return;
        }

        if (--_wait > 0)
            return;
        ScreenFade white = _white ?? throw NotEntered();
        white.FadeOut(1);
        white.Step();
        white.FadeIn(32);
        (_floor ?? throw NotEntered()).Visible = false;
        (_phase, _whiteLeft) = (Phase.WhiteFlash, 32);
    }

    /// <summary>The two halves of her emblem set out from the corners of the screen.</summary>
    private void BeginLogo()
    {
        _jumpBody?.QueueFree();
        _jumpHead?.QueueFree();
        (_jumpBody, _jumpHead) = (null, null);
        Node2D sprites = _sprites ?? throw NotEntered();
        _logoHalves.Clear();
        _logoCircles.Clear();
        _logoHalves.Add((LogoPart(sprites, "logo_upper_s", 274, -25), 138, 111, -1, 8, false));
        _logoHalves.Add((LogoPart(sprites, "logo_lower_s", -10, 263), 126, 127, 1, 8, false));
        _logoCircles.Add(LogoPart(sprites, "logo_upper_circle", 129, 110));
        _logoCircles.Add(LogoPart(sprites, "logo_lower_circle", 135, 128));
        _logoPicture = new Sprite2D
        {
            Name = "Logo", Centered = false, Modulate = new Color(1, 1, 1, 0), ZIndex = -1, Texture = Session.Content.LoadTexture(Data().Text("logo")),
        };
        sprites.AddChild(_logoPicture);
        _logoFade = -1;
        _phase = Phase.Logo;
    }

    private CinematicSprite LogoPart(Node2D sprites, string timeline, int x, int y)
    {
        CinematicSprite part = CinematicSprite.Of(Session.Content, _timelines.Get(timeline), PixelPosition.FromWhole(x, y));
        sprites.AddChild(part);
        return part;
    }

    /// <summary>
    /// Plays one update of the emblem and returns whether it is whole: its halves fly in, faster and faster, and
    /// land; its circles appear; and then its pieces give way, a step at a time, to its finished picture.
    /// </summary>
    private bool StepLogo()
    {
        if (_logoFade >= 0)
        {
            _logoFade++;
            float shown = _logoFade / 16f;
            (_logoPicture ?? throw NotEntered()).Modulate = new Color(1, 1, 1, shown);
            foreach (CinematicSprite piece in _logoHalves.Select(half => half.Half).Concat(_logoCircles))
                piece.Tint(new Color(1, 1, 1, 1 - shown));
            if (_logoFade < 16)
                return false;
            foreach (CinematicSprite piece in _logoHalves.Select(half => half.Half).Concat(_logoCircles))
                piece.QueueFree();
            _logoHalves.Clear();
            _logoCircles.Clear();
            return true;
        }

        for (int index = 0; index < _logoHalves.Count; index++)
        {
            (CinematicSprite half, int landX, int landY, int way, int speed, bool settled) = _logoHalves[index];
            if (!settled)
            {
                half.Place += PixelOffset.FromWhole(speed * way, -speed * way);
                bool reached = way < 0 ? half.Place.X.Whole <= landX : half.Place.X.Whole >= landX;
                if (reached)
                    half.Place = PixelPosition.FromWhole(landX, landY);
                _logoHalves[index] = (half, landX, landY, way, reached ? speed : speed + 2, reached);
            }

            half.Step();
        }

        foreach (CinematicSprite circle in _logoCircles)
        {
            foreach (string cue in circle.Step())
            {
                if (cue != "grey_out")
                    throw new InvalidOperationException($"The emblem gives the cue \"{cue}\", which the ending does not know.");
                _logoFade = 0;
            }
        }

        return false;
    }

    /// <summary>Starts typing one of her last pieces of text.</summary>
    private void BeginLastWords(string words, Phase phase)
    {
        CanvasLayer picture = _picture ?? throw NotEntered();
        _lastWords?.QueueFree();
        _percentage?.QueueFree();
        DataNode lastWords = Data().Get("last_words"), line = lastWords.Get(words), figures = lastWords.Get("percentage");
        _lastWords = new PixelText
        {
            Name = "LastWords", Font = PixelFont.Read(Session.Content, line.Text("font")), Position = new Vector2(0, line.Int("y")), Size = new Vector2(256, 0),
            HorizontalAlignment = HorizontalAlignment.Center, Text = Session.Text.Get(words), VisibleCharacters = 0,
        };
        picture.AddChild(_lastWords);
        _percentage = new PixelText
        {
            Name = "Percentage", Font = PixelFont.Read(Session.Content, figures.Text("font")), Position = new Vector2(figures.Int("x"), figures.Int("y")), Visible = false,
        };
        picture.AddChild(_percentage);
        _subtitle?.QueueFree();
        DataNode subtitle = lastWords.Get("japanese_subtitle");
        _subtitle = new Sprite2D
        {
            Name = "Subtitle", Centered = false, Texture = Session.Content.LoadTexture(subtitle.Text("picture")),
            Position = new Vector2(subtitle.Int("x"), subtitle.Int("y")), TextureFilter = CanvasItem.TextureFilterEnum.Nearest, Visible = false,
        };
        picture.AddChild(_subtitle);
        (_typed, _typeWait, _percentageShown, _phase) = (0, TypingDelay, false, phase);
    }

    /// <summary>Types the next letter when it is due, and returns whether the text is all there.</summary>
    private bool StepLastWords()
    {
        PixelText words = _lastWords ?? throw NotEntered();
        if (_typed >= words.Text.Length)
            return true;
        if (--_typeWait > 0)
            return false;
        _typed++;
        while (_typed < words.Text.Length && char.IsControl(words.Text[_typed]))
            _typed++;
        words.VisibleCharacters = _typed;
        _typeWait = TypingLetterUpdates;
        return false;
    }
}
