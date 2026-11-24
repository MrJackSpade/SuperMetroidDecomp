using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.World;
using Godot;

namespace Game.Menus;

/// <summary>
/// The title screen. It shows the title over its backdrop, with its music, until confirm is pressed, and then goes
/// to the choice of saved game. Left alone for a while it shows its demos, a set of them at a time, and comes back.
/// </summary>
/// <remarks>
/// Its data names its "music"; its "backdrop" colour; its "floor", the picture behind the title, with how much of it
/// there is to each pixel of the screen ("floor_per_pixel"), the point of it that stays put ("floor_centre") and how
/// far it is moved ("floor_scroll"); its "pictures", each with where its corner goes; how many updates it is left
/// alone before it shows a demo ("demo_after_updates"); and its "demo_sets", each a list of the files of its demos.
/// </remarks>
public partial class TitleMode : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/title.json";

    private DataNode? _data;
    private int _idle;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        var floor = new TurningFloor { Name = "Floor", Repeats = false, Picture = _data.Text("floor") };
        layer.AddChild(floor);
        floor.Setup(Session.Content, new Vector2I(256, 224));
        floor.Turn(0, _data.Get("floor_per_pixel").AsPixels().ToDisplay(), Place(_data.Get("floor_centre")), Place(_data.Get("floor_scroll")));
        foreach (DataNode picture in _data.List("pictures"))
        {
            layer.AddChild(new Sprite2D
            {
                Texture = Session.Content.LoadTexture(picture.Text("picture")), Centered = false, Position = Place(picture),
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            });
        }

        Session.Sounds.RequestMusic(_data.Text("music"));

        static Vector2 Place(DataNode at) => new(at.Int("x"), at.Int("y"));
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        DataNode data = _data ?? throw new InvalidOperationException("The title has not been shown.");
        if (input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause))
            return new FileSelectMode();
        IReadOnlyList<DataNode> sets = data.List("demo_sets");
        if (sets.Count == 0 || ++_idle < data.Int("demo_after_updates"))
            return this;
        string demo = Session.Service<DemoSchedule>().FromTitle([.. sets.Select(set => (IReadOnlyList<string>)[.. set.Items().Select(file => file.AsText())])]);
        Session.BeginAgain();
        return AttractDemoMode.Of(Session.Content, demo);
    }
}
