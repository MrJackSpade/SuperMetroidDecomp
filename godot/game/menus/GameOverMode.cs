using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The game over screen. It comes up out of black: its page, with the larva crying over its egg, and a cursor
/// beside yes or no. Up and down move the cursor; confirming fades the screen out, and yes goes back to the saved
/// game, no to the title.
/// </summary>
/// <remarks>
/// Its data names its "music", its "backdrop" colour, its "page" picture, the sound of the cursor moving
/// ("move_sound"), its timelines "baby" (which gives the cues "sound:" and a sound's name), "egg" and "cursor", and
/// the "places" of the "baby" (the egg lies there too) and of the cursor beside each answer ("cursor_yes",
/// "cursor_no").
/// </remarks>
public partial class GameOverMode : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/game_over.json";

    /// <summary>How many updates it takes to fade in, and to fade out.</summary>
    [Export]
    public int FadeUpdates { get; set; } = 32;

    private const string SoundCue = "sound:";

    private DataNode? _data;
    private CinematicSprite? _baby;
    private CinematicSprite? _egg;
    private CinematicSprite? _cursor;
    private ScreenFade? _fade;
    private bool _yes = true;
    private bool? _continues;

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
        _fade.FadeIn(FadeUpdates);
        Session.Sounds.RequestMusic(_data.Text("music"));
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        ScreenFade fade = _fade ?? throw NotShown();
        StepSprites();
        if (_continues is { } continues)
        {
            fade.Step();
            if (!fade.IsDone)
                return this;
            return continues ? new LoadingMode() : new TitleMode();
        }

        if (!fade.IsDone)
        {
            fade.Step();
            return this;
        }

        if (input.WasPressed(GameAction.Up) || input.WasPressed(GameAction.Down))
        {
            _yes = !_yes;
            CinematicSprite cursor = _cursor ?? throw NotShown();
            cursor.Place = Place(_yes ? "cursor_yes" : "cursor_no");
            cursor.ShowPlace();
            Session.Sounds.Request(Data().Text("move_sound"), channel: "menu");
        }
        else if (input.WasPressed(GameAction.Confirm))
        {
            _continues = _yes;
            fade.FadeOut(FadeUpdates);
        }

        return this;
    }

    /// <summary>Plays one update of the larva, its egg and the cursor; the larva's cries are heard as its timeline reaches them.</summary>
    private void StepSprites()
    {
        // Once an answer is given the larva holds the look it has while the screen fades.
        IReadOnlyList<string> cues = _continues is null ? (_baby ?? throw NotShown()).Step() : [];
        foreach (string cue in cues)
        {
            if (!cue.StartsWith(SoundCue, StringComparison.Ordinal))
                throw new InvalidOperationException($"The larva's timeline gives the cue \"{cue}\", which the game over screen does not know.");
            Session.Sounds.Request(cue[SoundCue.Length..], channel: "menu");
        }

        (_egg ?? throw NotShown()).Step();
        (_cursor ?? throw NotShown()).Step();
    }

    private PixelPosition Place(string name)
    {
        DataNode at = Data().Get("places").Get(name);
        return PixelPosition.FromWhole(at.Int("x"), at.Int("y"));
    }

    private DataNode Data() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The game over screen has not been shown.");
}
