using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game;

/// <summary>
/// Samus's death, from the update after her energy ran out to the game over screen, as the cartridge plays it.
/// <list type="number">
/// <item>One last update of play, time standing still for everything but her, after which she holds her beam and a
/// hurt's flicker ends.</item>
/// <item>Everything but her goes black, a part an update, play still going on beneath with time standing still; the
/// first of these updates changes nothing, and the update after the last part ends it: every sound is told to
/// stop, the music stops, the death's set of music is asked for and its piece behind it.</item>
/// <item>Nothing plays while the music's queue empties: she is shown as she was.</item>
/// <item>She takes up her death's pose, whose pictures move on for a while; then her colours flash between her
/// death's first two colourings; then, on the flash's last update, her suit begins to break up, each picture in the
/// colouring its step names, and from the update after its second picture first shows everything but her whitens,
/// a colour an update, to rest on its brightest but one. The update after the last picture shows the brightest and
/// nothing of her.</item>
/// <item>The screen goes black, the update's random number drawn on each update of it, and the game over screen
/// follows.</item>
/// </list>
/// What is not her is covered by a colour laid over the whole screen, and she is shown over that cover as her own
/// picture shows her.
/// </summary>
/// <remarks>
/// Its data names the parts everything goes black in ("fade_parts"); the set of music and the piece asked for and
/// the turns of the music's queue the piece waits ("music_set", "music", "music_wait"); the updates the pose's
/// first picture shows ("pose_first_updates"); how long the pose is shown before the flash and how long the flash
/// lasts ("before_flash_updates", "flash_updates"); the updates before the flash's first colouring
/// ("flash_first_wait"), its colourings in turn, each a step of her death's colours shown for so many updates
/// ("flash"), and the step shown on its last update ("flash_last_shade"); the colours everything whitens through
/// ("whiteout"), the one it rests on ("whiteout_resting") and the last ("whiteout_last"); and the updates the last
/// black-out waits before each level ("final_fade_wait"). Her timelines give the pose ("dying_right",
/// "dying_left": a ball's picture and then the upright one, which a death not begun in a ball starts on) and the
/// suit's breaking up ("death_explosion_right", "death_explosion_left"), each step of which names the step of her
/// death's colours it is shown in with the cue "shade:" and its number.
/// </remarks>
public partial class DeathMode : GameMode
{
    /// <summary>The data file of how her death is played.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "samus/death.json";

    private const string ShadeCue = "shade:";

    /// <summary>The layer of the cover over everything but her: above the HUD, beneath the screen's darkness.</summary>
    private const int CoverLayer = 20;

    /// <summary>
    /// The terms play goes on beneath on: nothing follows from her empty energy again, the pause is refused, no
    /// service takes play, and the updates do not count toward the play time.
    /// </summary>
    private static readonly PlayTerms TimeStandingStill = new()
    {
        PlayerMayDie = false, PlayerMayBeSaved = false, MayPause = false, ServicesMayTakeOver = false, CountsPlayTime = false,
    };

    private enum Stage
    {
        LastPass,
        BlackingOut,
        WaitingForMusic,
        BeforeFlash,
        Flashing,
        BreakingUp,
        FinalBlackOut,
    }

    private readonly PlayingMode _playing;
    private DataNode? _data;
    private ScreenFade? _cover;
    private ActorVisual? _figure;
    private Stage _stage = Stage.LastPass;

    /// <summary>How many steps the black-out of everything but her has taken.</summary>
    private int _fadeSteps;

    /// <summary>Her death's pose: its pictures, the one shown and the updates it has left.</summary>
    private FrameTimeline? _pose;
    private int _poseStep;
    private int _poseLeft;

    /// <summary>The updates left of the wait before the flash, and then of the flash.</summary>
    private int _left;

    /// <summary>Where the flash stands in its colourings, none shown yet at minus one, and the updates the one shown has left.</summary>
    private int _flashStep = -1;
    private int _flashLeft;

    /// <summary>Her suit breaking up: its pictures, the one shown and the updates it has left.</summary>
    private FrameTimeline? _breaking;
    private int _breakStep;
    private int _breakLeft;

    /// <summary>Whether everything but her whitens, and the colour it shows next.</summary>
    private bool _whitens;
    private int _whiteout;

    /// <summary>Samus's death over <paramref name="playing"/>, on the update after her energy ran out.</summary>
    public DeathMode(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _cover = new ScreenFade { Name = "Cover", Layer = CoverLayer };
        AddChild(_cover);
        var above = new CanvasLayer { Name = "Samus", Layer = CoverLayer + 1 };
        AddChild(above);
        _figure = new ActorVisual { Name = "Figure" };
        above.AddChild(_figure);
        _figure.Bind(Session.Content);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        var samus = (Samus)Session.Player;
        DataNode data = _data ?? throw NotEntered();
        ScreenFade cover = _cover ?? throw NotEntered();
        switch (_stage)
        {
            case Stage.LastPass:
            {
                if (_playing.PlayBeneath(input, TimeStandingStill) is { } elsewhere)
                    return elsewhere;

                // The black-out begins, changing nothing yet, and she is made ready.
                cover.Cover(0, data.Int("fade_parts"));
                samus.SettleForDeath();
                _stage = Stage.BlackingOut;
                return this;
            }

            case Stage.BlackingOut:
            {
                if (_playing.PlayBeneath(input, TimeStandingStill) is { } elsewhere)
                    return elsewhere;
                int parts = data.Int("fade_parts");
                if (!Session.Service<Compatibility.SharedFadeCount>().TakeStep(parts))
                {
                    // The first step shows everything as it was, and each after it a part darker, to black. When the
                    // black-out is over is by the count every fade shares, which a fade cut short before it may
                    // have left part way.
                    cover.Cover(Math.Min(_fadeSteps++, parts), parts);
                }
                else
                {
                    cover.Cover(parts, parts);

                    // The update after the last part: every library is told to stop, which no number of sounds
                    // waiting turns away, and the death's music is asked for behind the stop and its set.
                    Session.Sounds.StopAll(SoundPriority.Essential);
                    Session.Sounds.RequestMusic("");
                    Compatibility.CartridgeSounds.Of(Session).AskMusicSet(data.Text("music_set"));
                    Session.Sounds.RequestMusic(data.Text("music"), data.Int("music_wait"));
                    _stage = Stage.WaitingForMusic;
                }

                ShowFigure(samus, shows: true);
                return this;
            }

            case Stage.WaitingForMusic:
            {
                // Nothing plays and she is shown as she was. Once the music's queue is empty her death begins, to
                // be shown from the next update.
                if (!Session.Sounds.HasQueuedMusic)
                    BeginDying(samus, data);
                return this;
            }

            case Stage.BeforeFlash:
            {
                // Her pose's pictures move on only through this wait.
                FrameTimeline pose = _pose ?? throw NotEntered();
                if (--_poseLeft <= 0 && _poseStep < pose.Steps.Count - 1)
                    _poseLeft = pose.Steps[++_poseStep].Updates;
                Show(samus, pose.Steps[_poseStep], shows: true);
                if (--_left == 0)
                    (_stage, _left) = (Stage.Flashing, data.Int("flash_updates"));
                return this;
            }

            case Stage.Flashing:
            {
                if (--_left == 0)
                {
                    // The flash's last update shows its last colouring, and her suit begins to break up: its first
                    // picture is shown on this update, which counts as the first of that picture's.
                    FrameTimeline breaking = _breaking ?? throw NotEntered();
                    samus.ShowDeathColours(data.Int("flash_last_shade"));
                    (_breakStep, _breakLeft, _whitens, _whiteout) = (0, breaking.Steps[0].Updates, false, 0);
                    _stage = Stage.BreakingUp;
                    return StepBreakingUp(samus, data);
                }

                if (--_flashLeft == 0)
                {
                    IReadOnlyList<DataNode> flash = data.List("flash");
                    _flashStep = (_flashStep + 1) % flash.Count;
                    _flashLeft = flash[_flashStep].Int("updates");
                    samus.ShowDeathColours(flash[_flashStep].Int("shade"));
                }

                Show(samus, null, shows: true);
                return this;
            }

            case Stage.BreakingUp:
            {
                // Everything but her shows the whiteout's next colour, and rests on the one its data says.
                if (_whitens)
                {
                    ShowWhiteout(samus, data, _whiteout);
                    _whiteout = Math.Min(_whiteout + 1, data.Int("whiteout_resting"));
                }

                return StepBreakingUp(samus, data);
            }

            case Stage.FinalBlackOut:
            {
                // The last black-out plays nothing: the update's random number is drawn, and the screen darkens.
                Session.OpenUpdate();
                return Session.Darkness.Darken() ? new Menus.GameOverMode() : this;
            }

            default:
                throw new InvalidOperationException($"Samus's death has no stage {_stage}.");
        }
    }

    /// <summary>
    /// Her death begins: she takes up its pose, on the picture a ball begins it on or else its upright one, and the
    /// wait before the flash and the flash's own first wait begin.
    /// </summary>
    private void BeginDying(Samus samus, DataNode data)
    {
        samus.BeginDeathPose();
        string side = samus.Facing == Facing.Right ? "right" : "left";
        _pose = samus.Timelines.Get($"dying_{side}");
        _breaking = samus.Timelines.Get($"death_explosion_{side}");
        _poseStep = samus.Form == Player.Crawling ? 0 : _pose.Steps.Count - 1;
        _poseLeft = data.Int("pose_first_updates");
        _left = data.Int("before_flash_updates");
        (_flashStep, _flashLeft) = (-1, data.Int("flash_first_wait"));
        _stage = Stage.BeforeFlash;
    }

    /// <summary>
    /// One update of her suit breaking up. The picture shown moves on as its updates run out, in the colouring its
    /// step names, and the whitening begins from the update after the second picture first shows. The update after
    /// the last picture shows the whiteout's last colour and nothing of her, and the last black-out begins.
    /// </summary>
    private GameMode StepBreakingUp(Samus samus, DataNode data)
    {
        FrameTimeline breaking = _breaking ?? throw NotEntered();
        if (--_breakLeft == 0)
        {
            if (++_breakStep == breaking.Steps.Count)
            {
                ShowWhiteout(samus, data, data.Int("whiteout_last"));
                Show(samus, null, shows: false);
                Session.Darkness.Restart(0, data.Int("final_fade_wait"));
                _stage = Stage.FinalBlackOut;
                return this;
            }

            TimelineStep next = breaking.Steps[_breakStep];
            _breakLeft = next.Updates;
            _whitens = true;
            samus.ShowDeathColours(ShadeOf(next));
        }

        Show(samus, breaking.Steps[_breakStep], shows: true);
        return this;
    }

    /// <summary>The step of her death's colours a picture of the breaking up names.</summary>
    private static int ShadeOf(TimelineStep step)
    {
        string cue = step.Cues.FirstOrDefault(cue => cue.StartsWith(ShadeCue, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"The picture {step.Picture} of Samus's death names no step of her death's colours.");
        return int.Parse(cue[ShadeCue.Length..], System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Everything but her shows a colour of the whiteout, and her beams' colours whiten with it.</summary>
    private void ShowWhiteout(Samus samus, DataNode data, int step)
    {
        ScreenFade cover = _cover ?? throw NotEntered();
        cover.Colour = new Color(data.List("whiteout")[step].AsText());
        cover.Cover(1, 1);
        samus.ShowDeathWhiteout(step);
    }

    /// <summary>Shows her as her death has her: a step of one of her timelines, or as she was last shown with none given; or nothing.</summary>
    private void Show(Samus samus, TimelineStep? step, bool shows)
    {
        samus.GetNode<SamusVisual>("Visual").ShowDying(samus, step, shows);
        ShowFigure(samus, shows);
    }

    /// <summary>Shows over the cover what her own picture shows, where she is on the screen.</summary>
    private void ShowFigure(Samus samus, bool shows)
    {
        ActorVisual figure = _figure ?? throw NotEntered();
        samus.GetNode<SamusVisual>("Visual").ShowEchoOn(samus, figure, shows);
        figure.Position = new Vector2(samus.Position.X.Whole - Session.Camera.Left, samus.Position.Y.Whole - Session.Camera.Top);
    }

    private static InvalidOperationException NotEntered() => new("Samus's death is stepped before it is entered.");
}
