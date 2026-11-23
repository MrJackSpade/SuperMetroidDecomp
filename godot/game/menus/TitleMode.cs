using Engine.Core;
using Engine.Input;
using Godot;

namespace Game.Menus;

/// <summary>
/// The title screen. It shows the title and its music until confirm is pressed, and then goes to the choice of
/// saved game. Left alone for a while it shows one of its demos, each in turn, and comes back.
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

    /// <summary>The files of its demos, as paths within the assets tree, shown in turn.</summary>
    [Export]
    public string[] Demos { get; set; } = [];

    /// <summary>How many updates it is left alone before it shows a demo.</summary>
    [Export]
    public int DemoAfterUpdates { get; set; } = 1200;

    /// <summary>Which demo is shown next; it goes on from where the last title screen left off.</summary>
    private static int _nextDemo;

    private int _idle;

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
    public override GameMode Step(GameInput input)
    {
        if (input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause))
            return new FileSelectMode();
        if (Demos.Length == 0 || ++_idle < DemoAfterUpdates)
            return this;
        string demo = Demos[_nextDemo % Demos.Length];
        _nextDemo = (_nextDemo + 1) % Demos.Length;
        Session.BeginAgain();
        return AttractDemoMode.Of(Session.Content, demo);
    }
}
