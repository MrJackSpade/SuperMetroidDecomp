using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Game.Data;
using Godot;

namespace Game.Menus;

/// <summary>
/// The game over screen. It stays black while the room's sounds stop, it is set up (the larva starting, the music
/// stopping and the title's set of music loading) and the music's queue empties, and then it comes up out of black with
/// its own music: its page, with the larva crying over its egg, and a cursor beside yes or no. Item select, up and down move the cursor; confirming fades the
/// screen out, the larva holding its look, and yes goes back to the saved game by the map of where it was saved,
/// no to the title. A yes for the game saved on arriving at Ceres starts it at once, with no fade; one for the game
/// saved as Ceres was left fades out and takes it up with Ceres blowing up. Its fades go a level of brightness an update.
/// </summary>
/// <remarks>
/// Its data names its "music", its "backdrop" colour, its "page" picture, the sound of the cursor moving
/// ("move_sound"), the updates it stays black before it is set up ("entry_black_updates"), the levels of brightness its fades go through
/// ("fade_levels"), the updates the larva holds its
/// look once an answer is given ("answer_hold_updates"), its timelines "baby" (which gives the cues "sound:" and a
/// sound's name), "egg" and "cursor", and the "places" of the "baby" (the egg lies there too) and of the cursor
/// beside each answer ("cursor_yes", "cursor_no").
/// </remarks>
public partial class GameOverMode : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/game_over.json";

    private const string SoundCue = "sound:";

    private DataNode? _data;
    private CinematicSprite? _baby;
    private CinematicSprite? _egg;
    private CinematicSprite? _cursor;
    private ScreenFade? _fade;
    private bool _yes = true;
    private bool? _continues;
    private SaveMapMode? _map;
    private int _levels;
    private int _holdLeft;
    private int _blackUpdates;
    private bool _setUp;
    private bool _setUpWaitedOut;
    private bool _musicAsked;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        TimelineSet timelines = TimelineSet.ReadFrom(_data);
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        layer.AddChild(new Sprite2D
        {
            Name = "Page", Centered = false, Texture = Session.Content.LoadTexture(_data.Text("page")), TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        });
        _baby = CinematicSprite.Of(Session.Content, timelines.Get("baby"), Place("baby"));
        _egg = CinematicSprite.Of(Session.Content, timelines.Get("egg"), Place("baby"));
        _cursor = CinematicSprite.Of(Session.Content, timelines.Get("cursor"), Place("cursor_yes"));
        layer.AddChild(_baby);
        layer.AddChild(_egg);
        layer.AddChild(_cursor);
        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        // Its fades go a level of brightness an update, by the engine's own stepped fade.
        _levels = _data.Int("fade_levels");
        _fade.Levels = _levels;
        _fade.FadeIn(_levels);
    }

    /// <inheritdoc/>
    public override bool GivesMusicItsTurn => !WaitsOutDisplayFrame;

    /// <inheritdoc/>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // Every update of the old game over screen draws a random number first and gives the sounds their turns, but for
        // one that finishes a wait for a frame of the display.
        if (!WaitsOutDisplayFrame)
            Session.OpenUpdate();
        _closing = WaitsOutDisplayFrame ? ClosingSoundWork.None : ClosingSoundWork.Usual;
        ScreenFade fade = _fade ?? throw NotShown();
        if (_blackUpdates < Data().Int("entry_black_updates"))
        {
            // Every sound stops on its first update.
            if (_blackUpdates++ == 0)
                Session.Sounds.StopAll();
            return this;
        }

        if (!_setUp)
        {
            // Its set-up stops the music, loads the title's set and starts the larva, this update counting as the first
            // of its first look.
            _setUp = true;
            Session.Sounds.RequestMusic("");
            Compatibility.CartridgeSounds.Of(Session).AskMusicSet("title_sequence");
            StepBaby();
            return this;
        }

        if (!_setUpWaitedOut)
        {
            // The old game waits out the set-up's update before it looks at the music.
            _setUpWaitedOut = true;
            return this;
        }

        if (!_musicAsked)
        {
            // Nothing moves while the music's queue empties; once it has, the screen asks for its own and fades in.
            if (Session.Sounds.HasQueuedMusic)
                return this;
            _musicAsked = true;
            Session.Sounds.RequestMusic(Data().Text("music"));
            return this;
        }

        StepSprites();
        if (_continues is { } continues)
        {
            fade.Step();
            if (!fade.IsDone)
                return this;
            if (!continues)
                return new TitleMode();

            // A saved game shows where it was saved before it starts again, as from the options; the one saved as
            // Ceres was left is taken up with Ceres blowing up.
            return _map ?? GameLoad.OfSlot(appears: false);
        }

        if (!fade.IsDone)
        {
            fade.Step();
            return this;
        }

        if (input.WasPressed(Actions.ItemSelect) || input.WasPressed(GameAction.Up) || input.WasPressed(GameAction.Down))
        {
            _yes = !_yes;
            CinematicSprite cursor = _cursor ?? throw NotShown();
            cursor.Place = Place(_yes ? "cursor_yes" : "cursor_no");
            cursor.ShowPlace();
            Session.Sounds.Request(Data().Text("move_sound"), channel: "menu");
        }
        else if (input.WasPressed(GameAction.Confirm))
        {
            if (_yes)
            {
                _map = SaveMapMode.Of(Session);
                // The game saved on arriving at Ceres starts on this update, with no hold and no fade; the one saved as
                // Ceres was left fades out first, as one the map shows does.
                if (_map is null && !GameLoad.SavedLeavingCeres(Session))
                    return GameLoad.OfSlot(appears: false);
            }

            _continues = _yes;
            fade.FadeOut(_levels);
            _holdLeft = Data().Int("answer_hold_updates");
        }

        return this;
    }

    /// <summary>
    /// Whether the update finishes a wait for a frame of the display: in the old game the last of the black updates
    /// (after the sounds are stopped) and the one after the set-up.
    /// </summary>
    private bool WaitsOutDisplayFrame => _blackUpdates == Data().Int("entry_black_updates") - 1 || (_setUp && !_setUpWaitedOut);

    /// <summary>Plays one update of the larva, its egg and the cursor.</summary>
    private void StepSprites()
    {
        StepBaby();
        (_egg ?? throw NotShown()).Step();
        (_cursor ?? throw NotShown()).Step();
    }

    /// <summary>Plays one update of the larva; its cries are heard as its timeline reaches them.</summary>
    private void StepBaby()
    {
        // Once an answer is given the larva holds the look it has for the old game's hold, through the fade.
        bool holds = _continues is not null && _holdLeft-- > 0;
        IReadOnlyList<string> cues = holds ? [] : (_baby ?? throw NotShown()).Step();
        foreach (string cue in cues)
        {
            if (!cue.StartsWith(SoundCue, StringComparison.Ordinal))
                throw new InvalidOperationException($"The larva's timeline gives the cue \"{cue}\", which the game over screen does not know.");
            Session.Sounds.Request(cue[SoundCue.Length..], channel: "menu");
        }
    }

    private PixelPosition Place(string name)
    {
        DataNode at = Data().Get("places").Get(name);
        return PixelPosition.FromWhole(at.Int("x"), at.Int("y"));
    }

    private DataNode Data() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The game over screen has not been shown.");
}
