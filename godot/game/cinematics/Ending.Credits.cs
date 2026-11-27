using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The middle of the ending: the credits rolling up the screen, and then, among shooting stars, Samus seen waiting,
/// first as a wavering glow, then still, while the result of the mission is shown and she is revealed.
/// </summary>
/// <remarks>
/// The data names the credits' picture ("credits_roll") and how far it scrolls before it is done ("credits_scroll"),
/// the colour shown behind everything after the credits ("post_credits_backdrop"), the pictures "waiting_backdrop",
/// "wavy_samus", "result_panel" and "copyright_panel", and the four pictures of a "shooting_star" from faintest to
/// fullest, with where those lie from the star's place ("shooting_star_offset").
/// </remarks>
public partial class Ending
{
    /// <summary>
    /// The wavering glow: each line of the screen shows the picture pushed sideways by its own whole number of pixels
    /// and scrolled up, added over what lies beneath, and only where the backdrop she waits against is drawn.
    /// </summary>
    private const string WaveCode = """
        shader_type canvas_item;
        render_mode blend_add;
        uniform sampler2D picture : repeat_enable, filter_nearest;
        uniform sampler2D backdrop : filter_nearest;
        uniform int shift[224];
        uniform int scroll = 0;
        void fragment() {
            int x = int(floor(UV.x * 256.0));
            int line = int(floor(UV.y * 224.0));
            vec2 size = vec2(textureSize(picture, 0));
            vec4 glow = texture(picture, (vec2(float(x + shift[line]), float(line + scroll)) + 0.5) / size);
            ivec2 under = textureSize(backdrop, 0);
            float drawn = x < under.x && line < under.y ? texture(backdrop, (vec2(float(x), float(line)) + 0.5) / vec2(under)).a : 0.0;
            COLOR = vec4(glow.rgb, glow.a * drawn);
        }
        """;

    /// <summary>The screen's lines, each pushed sideways by the wave on its own.</summary>
    private const int ScreenLines = 224;

    /// <summary>
    /// The wave: how far it pushes a line at most, in pixels; how many lines a whole turn of it takes; the angle at the
    /// top line on its first update, a step short of a whole turn; and how far that angle moves on each update after.
    /// </summary>
    private const int WaveReach = 64, WaveLines = 128, WaveFirstAngle = 255, WaveTurnSteps = 8;

    /// <summary>How many lines the picture is scrolled up on each update.</summary>
    private const int WaveScrollLines = 2;

    /// <summary>How long the screen stays dark after the credits.</summary>
    private const int BlankUpdates = 60;

    /// <summary>
    /// The screen then comes up a level on the second update and every fourth after it, so that it is fully lit on the
    /// fifty-eighth.
    /// </summary>
    private const int PostCreditsFadeFirstStep = 2, PostCreditsFadeStepUpdates = 4;

    /// <summary>How long the waiting backdrop takes to come in, how long it is watched, and how long the result is shown.</summary>
    private const int BackdropFadeUpdates = 32, BackdropUpdates = 180, ResultUpdates = 180;

    /// <summary>How long each half of Samus's reveal lasts, and how long the copyright is held between them.</summary>
    private const int RevealHalfUpdates = 64, CopyrightUpdates = 180;

    /// <summary>
    /// The shooting stars: how much speed each gains across and down every update, in 256ths of a pixel; how long each
    /// of its looks lasts as it brightens, or zero for one that stays faint; and how long it waits before it first sets out.
    /// </summary>
    private static readonly (int Across, int Down, int Period, int Delay)[] ShootingStars =
    [
        (16, -16, 6, 8), (12, -1, 6, 0), (13, -10, 6, 6), (8, -16, 6, 0), (2, -14, 6, 8), (16, -8, 6, 0), (2, -1, 0, 0), (1, -2, 0, 0),
        (0, -3, 0, 0), (2, -5, 32, 4), (15, 16, 6, 8), (2, 9, 8, 0), (12, 8, 6, 0), (16, 4, 6, 8), (8, 12, 8, 0), (2, 8, 8, 0),
        (2, 4, 0, 0), (2, 1, 0, 0), (-13, 13, 4, 8), (-14, 8, 4, 0), (-7, 14, 6, 0), (-5, 16, 4, 8), (-4, 5, 32, 0), (-2, 3, 0, 0),
        (-2, 4, 0, 0), (-3, 1, 0, 0), (-12, 2, 4, 0), (-6, 5, 32, 0), (-3, 8, 32, 0), (-14, -9, 4, 8), (-8, -12, 4, 0), (-6, -14, 6, 0),
        (-12, -16, 4, 8), (-8, -4, 32, 0), (-3, -6, 32, 0), (-7, -8, 32, 0), (-2, -3, 0, 0), (-4, -4, 0, 0), (-8, -2, 0, 0), (-8, -4, 0, 0),
    ];

    /// <summary>How long a shooting star flies unseen before it shows.</summary>
    private const int StarLeadIn = 32;

    private readonly List<(int X, int Y, int SpeedX, int SpeedY, int Age, int Delay)> _stars = [];
    private readonly List<Sprite2D> _starSprites = [];
    private Node2D? _starField;
    private Sprite2D? _credits;
    private Sprite2D? _waitingBackdrop;
    private ColorRect? _wavySamus;
    private Sprite2D? _panel;
    private double _scrolled;
    private int _age;
    private int _revealStep;
    private bool _copyrightShown;
    private bool _copyrightHolds;

    /// <summary>The credits begin: everything of the flight is put away and the roll starts from below the screen.</summary>
    private void SetUpCredits()
    {
        // The credits end every step of colours still playing.
        StopAllColourSteps();
        ClearActors();
        (_floor ?? throw NotEntered()).Visible = false;
        foreach (PixelText line in _resultTexts.Values)
            line.Visible = false;
        (_backdrop ?? throw NotEntered()).Color = Colors.Black;
        CanvasLayer picture = _picture ?? throw NotEntered();
        _credits = new Sprite2D { Name = "Credits", Centered = false, Position = new Vector2(0, 256), Texture = Session.Content.LoadTexture(Data().Text("credits_roll")) };
        picture.AddChild(_credits);
        _scrolled = 0;
        ScreenFade fade = Fade();
        fade.FadeIn(1);
        fade.Step();
        _phase = Phase.Credits;
    }

    /// <summary>Plays one update of the credits and of Samus waiting.</summary>
    private void StepCreditsAndWaiting()
    {
        ScreenFade fade = Fade();
        switch (_phase)
        {
            case Phase.Credits:
                _scrolled += 0.5;
                (_credits ?? throw NotEntered()).Position = new Vector2(0, 256 - (int)_scrolled);
                if ((int)_scrolled < Data().Int("credits_scroll"))
                    break;
                SetUpPostCredits(fade);
                (_phase, _wait) = (Phase.PostCreditsBlank, BlankUpdates);
                break;

            case Phase.PostCreditsBlank:
                if (--_wait > 0)
                    break;
                fade.Cover(FadeLevels, FadeLevels);
                (_phase, _age) = (Phase.PostCreditsFadeIn, 0);
                break;

            case Phase.PostCreditsFadeIn:
                _age++;
                int lit = _age < PostCreditsFadeFirstStep ? 0 : Math.Min(FadeLevels, 1 + (_age - PostCreditsFadeFirstStep) / PostCreditsFadeStepUpdates);
                fade.Cover(FadeLevels - lit, FadeLevels);
                if (lit < FadeLevels)
                    break;
                (_waitingBackdrop ?? throw NotEntered()).Visible = true;
                (_phase, _age) = (Phase.ShootingStars, 0);
                break;

            case Phase.ShootingStars:
                _age++;
                float brought = (float)_age / BackdropFadeUpdates;
                (_waitingBackdrop ?? throw NotEntered()).Modulate = new Color(brought, brought, brought);
                if (_age < BackdropFadeUpdates)
                    break;

                // Samus is seen over it from the update it has come in, still unpushed.
                (_phase, _age) = (Phase.WaitingBackdrop, 0);
                ShowWavySamus(_age);
                break;

            case Phase.WaitingBackdrop:
                // Samus wavers over the backdrop like something seen through heat, sliding up all the while.
                _age++;
                ShowWavySamus(_age);
                if (_age < BackdropUpdates)
                    break;
                // The result is shown alone: the backdrop and the stars are put away while it is held.
                (_wavySamus ?? throw NotEntered()).Visible = false;
                (_waitingBackdrop ?? throw NotEntered()).Visible = false;
                StarField().Visible = false;
                ShowPanel("result_panel", 72);
                (_phase, _wait) = (Phase.WaitingSamus, ResultUpdates);
                break;

            case Phase.WaitingSamus:
                if (--_wait > 0)
                    break;
                SpawnIdleSamus();
                (_phase, _wait, _revealStep, _age, _copyrightShown, _copyrightHolds) = (Phase.RewardReveal, RevealHalfUpdates, 0, 0, false, false);
                ShowRevealing(true);
                break;

            case Phase.RewardReveal:
                StepReveal();
                break;

            default:
                throw new InvalidOperationException($"The ending's credits have no phase {_phase}.");
        }
    }

    /// <summary>Sets up what follows the credits, in the dark: the stars, the backdrop Samus waits against, and her wavering picture.</summary>
    private void SetUpPostCredits(ScreenFade fade)
    {
        (_credits ?? throw NotEntered()).Visible = false;
        fade.FadeOut(1);
        fade.Step();

        // Everything after the credits lies over the colour of its own scenes, which shows wherever nothing is drawn:
        // behind the stars, Samus's shot and the white flash after it as much as anywhere.
        (_backdrop ?? throw NotEntered()).Color = new Color(Data().Text("post_credits_backdrop"));
        CanvasLayer picture = _picture ?? throw NotEntered();

        // The backdrop she waits against comes in from black, as its colours do.
        _waitingBackdrop = new Sprite2D
        {
            Name = "WaitingBackdrop", Centered = false, Visible = false, Modulate = new Color(0, 0, 0),
            Texture = Session.Content.LoadTexture(Data().Text("waiting_backdrop")),
        };
        picture.AddChild(_waitingBackdrop);
        var wave = new ShaderMaterial { Shader = new Shader { Code = WaveCode } };
        wave.SetShaderParameter("picture", Session.Content.LoadTexture(Data().Text("wavy_samus")));
        wave.SetShaderParameter("backdrop", _waitingBackdrop.Texture);
        _wavySamus = new ColorRect { Name = "WavySamus", Size = new Vector2(256, 224), Visible = false, Material = wave };
        picture.AddChild(_wavySamus);
        _panel = new Sprite2D { Name = "Panel", Centered = false, Visible = false };
        picture.AddChild(_panel);

        _stars.Clear();
        _starSprites.Clear();
        _starField = new Node2D { Name = "ShootingStars" };
        (_sprites ?? throw NotEntered()).AddChild(_starField);
        foreach ((_, _, _, int delay) in ShootingStars)
        {
            // A star that waits counts its wait, and one update more on which its wait runs out.
            _stars.Add((128 * 256, 128 * 256, 0, 0, 0, delay == 0 ? 0 : delay + 1));
            DataNode offset = Data().Get("shooting_star_offset");
            var sprite = new Sprite2D { Name = "ShootingStar", Centered = false, Offset = new Vector2(offset.Int("x"), offset.Int("y")), Visible = false, ZIndex = -2 };
            _starField.AddChild(sprite);
            _starSprites.Add(sprite);
        }
    }

    private Node2D StarField() => _starField ?? throw NotEntered();

    /// <summary>
    /// Shows Samus wavering, <paramref name="age"/> updates after the backdrop has come in: unpushed on its first two,
    /// and from then each line pushed by the cartridge's sine at the line's angle, cut toward nothing, as the
    /// cartridge's ripple is.
    /// </summary>
    private void ShowWavySamus(int age)
    {
        ColorRect samus = _wavySamus ?? throw NotEntered();
        samus.Visible = true;
        var shift = new int[ScreenLines];
        if (age >= 2)
        {
            int top = WaveFirstAngle + WaveTurnSteps * (age - 1);
            for (int line = 0; line < ScreenLines; line++)
                shift[line] = Compatibility.CartridgeAngles.ReachCutShort(top + line * Turns.PerTurn / WaveLines, WaveReach);
        }

        var wave = (ShaderMaterial)samus.Material;
        wave.SetShaderParameter("shift", shift);
        wave.SetShaderParameter("scroll", age * WaveScrollLines);
    }

    private void ShowPanel(string picture, int y)
    {
        Sprite2D panel = _panel ?? throw NotEntered();
        panel.Visible = picture.Length > 0;
        if (!panel.Visible)
            return;
        panel.Texture = Session.Content.LoadTexture(Data().Text(picture));
        panel.Position = new Vector2(0, y);
    }

    /// <summary>
    /// Plays one update of Samus's reveal: she comes in over the backdrop a step every fourth update; half-way the
    /// copyright is shown alone while everything is held, she going on playing unseen; then the rest, and she is ready to
    /// move.
    /// </summary>
    private void StepReveal()
    {
        StepActors();
        if (_copyrightHolds)
        {
            if (--_wait > 0)
                return;
            (_copyrightHolds, _wait) = (false, RevealHalfUpdates);
            ShowRevealing(true);
            return;
        }

        if (++_age % 4 == 0 && _revealStep < 32)
        {
            _revealStep++;
            ShowReveal();
        }

        if (--_wait > 0)
            return;
        if (!_copyrightShown)
        {
            (_copyrightShown, _copyrightHolds, _wait) = (true, true, CopyrightUpdates);
            ShowRevealing(false);
            return;
        }

        BeginGesture();
    }

    /// <summary>
    /// Shows the reveal, with the backdrop, Samus and the stars and no panel; or, while the copyright is held, the
    /// copyright alone.
    /// </summary>
    private void ShowRevealing(bool revealing)
    {
        ShowPanel(revealing ? "" : "copyright_panel", 96);
        StarField().Visible = revealing;
        foreach (Actor actor in _actors)
            actor.Sprite.Visible = revealing;
        (_waitingBackdrop ?? throw NotEntered()).Visible = revealing;
        if (revealing)
            ShowReveal();
    }

    /// <summary>Shows the reveal as far as it has got. Samus in her armour is there from the first, with nothing behind her.</summary>
    private void ShowReveal()
    {
        Sprite2D backdrop = _waitingBackdrop ?? throw NotEntered();
        if (Reward == RewardKind.Armoured)
        {
            backdrop.Visible = false;
            return;
        }

        // The backdrop goes to black as she comes in, as its colours do.
        float shown = _revealStep / 32f;
        backdrop.Modulate = new Color(1 - shown, 1 - shown, 1 - shown);
        foreach (Actor actor in _actors)
            actor.Sprite.Tint(new Color(1, 1, 1, shown));
    }

    /// <summary>
    /// Plays one update of the shooting stars: each waits its turn, then flies from the middle of the screen, gaining
    /// speed and brightening, and starts again from the middle when it has left the screen.
    /// </summary>
    private void StepShootingStars()
    {
        for (int index = 0; index < _stars.Count; index++)
        {
            (int x, int y, int speedX, int speedY, int age, int delay) = _stars[index];
            (int across, int down, int period, _) = ShootingStars[index];
            Sprite2D sprite = _starSprites[index];
            if (delay > 0)
            {
                // On the update its wait runs out it does not move, but its flight begins: it is a step into its lead-in.
                _stars[index] = (x, y, speedX, speedY, delay == 1 ? 1 : 0, delay - 1);
                continue;
            }

            // It gains speed by the look it had as the update began, and shows the look its flight has come to.
            int look = StarLook(age, period);
            int gain = look <= 0 ? 1 : 2;
            (speedX, speedY) = (speedX + across * gain, speedY + down * gain);
            (x, y) = (x + speedX, y + speedY);
            int shownX = x >> 8, shownY = y >> 8;
            if (shownX is < 4 or > 255 || shownY is < 4 or > 255)
            {
                _stars[index] = (128 * 256, 128 * 256, 0, 0, 0, 0);
                sprite.Visible = false;
                continue;
            }

            _stars[index] = (x, y, speedX, speedY, age + 1, 0);
            int shown = StarLook(age + 1, period);
            sprite.Visible = shown >= 0;
            if (shown < 0)
                continue;
            sprite.Texture = Session.Content.LoadTexture(Data().List("shooting_star")[shown].AsText());
            sprite.Position = new Vector2(shownX, shownY);
        }
    }

    /// <summary>
    /// A star's look once its flight has played <paramref name="age"/> updates: unseen through its lead-in, then faint,
    /// and for one that brightens, bright, dim and full, each for its period.
    /// </summary>
    private static int StarLook(int age, int period) =>
        age < StarLeadIn ? -1 : period == 0 ? 0 : Math.Min(3, (age - StarLeadIn) / period);
}
