using Engine.Core;
using Engine.Input;
using Godot;

namespace Game.Menus;

/// <summary>
/// The title screen. It shows the title and its music until confirm is pressed, and then goes to the choice of
/// saved game.
/// </summary>
public partial class TitleMode : GameMode
{
    /// <summary>The picture of the title, as a path within the assets tree; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "menus/title.png";

    /// <summary>The music of the title, by name.</summary>
    [Export]
    public string Music { get; set; } = Data.Music.TitleScreen;

    /// <summary>The name, in the game's words, of what it says under the title.</summary>
    [Export]
    public string Prompt { get; set; } = "title_prompt";

    /// <inheritdoc/>
    protected override void Enter()
    {
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        if (Picture.Length > 0)
            layer.AddChild(new TextureRect { Name = "Title", Texture = Session.Content.LoadTexture(Picture) });
        var prompt = new Label { Name = "Prompt", Text = Session.Text.Get(Prompt), HorizontalAlignment = HorizontalAlignment.Center };
        prompt.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
        layer.AddChild(prompt);
        Session.Sounds.RequestMusic(Music);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input) =>
        input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause) ? new FileSelectMode() : this;
}
