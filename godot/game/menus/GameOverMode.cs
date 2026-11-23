using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The game over screen. It comes up out of black, asks whether to continue, and fades out again: yes goes back to
/// the saved game, no to the title.
/// </summary>
/// <remarks>Its words are "game_over", "game_over_continue", "yes" and "no".</remarks>
public partial class GameOverMode : GameMode
{
    /// <summary>The music of the screen, by name.</summary>
    [Export]
    public string Music { get; set; } = Data.Music.StatueHall;

    /// <summary>The picture behind it, as a path within the assets tree; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "menus/game_over.png";

    /// <summary>The sound of the mark moving, by name.</summary>
    [Export]
    public string MoveSound { get; set; } = "";

    /// <summary>How many updates it takes to fade in, and to fade out.</summary>
    [Export]
    public int FadeUpdates { get; set; } = 32;

    private ChoiceList? _list;
    private ScreenFade? _fade;
    private bool? _continues;

    /// <inheritdoc/>
    protected override void Enter()
    {
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        if (Picture.Length > 0)
            layer.AddChild(new TextureRect { Name = "Backdrop", Texture = Session.Content.LoadTexture(Picture) });
        var column = new VBoxContainer { Name = "Column" };
        column.SetAnchorsPreset(Control.LayoutPreset.Center);
        layer.AddChild(column);
        column.AddChild(new Label { Name = "Heading", Text = Session.Text.Get("game_over") });
        column.AddChild(new Label { Name = "Question", Text = Session.Text.Get("game_over_continue") });
        _list = new ChoiceList { Name = "List" };
        column.AddChild(_list);
        _list.Present([Session.Text.Get("yes"), Session.Text.Get("no")]);
        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        _fade.FadeIn(FadeUpdates);
        Session.Sounds.RequestMusic(Music);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        ScreenFade fade = _fade ?? throw new InvalidOperationException("The game over screen has not been shown.");
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

        switch (_list!.Step(input))
        {
            case ChoiceList.Outcome.Moved:
                if (MoveSound.Length > 0)
                    Session.Sounds.Request(MoveSound, channel: "menu");
                break;
            case ChoiceList.Outcome.Confirmed:
                _continues = _list.Chosen == 0;
                fade.FadeOut(FadeUpdates);
                break;
        }

        return this;
    }
}
