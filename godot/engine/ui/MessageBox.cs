using Godot;

namespace Engine.Ui;

/// <summary>
/// A box showing a message, with a yes and a no to choose between when the message asks. It shows; the mode that
/// presents it decides when it is dismissed, and it grants nothing and saves nothing itself. A message is its words,
/// or, for a game that keeps its messages as pictures, a picture shown across the middle of the screen.
/// </summary>
public partial class MessageBox : CanvasLayer
{
    private PixelText? _words;
    private PixelText? _choice;
    private Sprite2D? _picture;

    /// <summary>The font its words are drawn in.</summary>
    public PixelFont? Font { get; set; }

    /// <summary>The message being shown.</summary>
    public string Message { get; private set; } = "";

    /// <summary>Whether the message asks yes or no.</summary>
    public bool AsksYesOrNo { get; private set; }

    /// <summary>Whether yes is the choice showing.</summary>
    public bool YesIsChosen { get; private set; } = true;

    /// <summary>Shows <paramref name="message"/>.</summary>
    public void Present(string message, bool asksYesOrNo)
    {
        ArgumentNullException.ThrowIfNull(message);
        Message = message;
        AsksYesOrNo = asksYesOrNo;
        if (_words is null)
        {
            var panel = new PanelContainer { Name = "Panel" };
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            var column = new VBoxContainer { Name = "Column" };
            _words = new PixelText { Name = "Words", Font = Font, HorizontalAlignment = HorizontalAlignment.Center };
            _choice = new PixelText { Name = "Choice", Font = Font, HorizontalAlignment = HorizontalAlignment.Center };
            column.AddChild(_words);
            column.AddChild(_choice);
            panel.AddChild(column);
            AddChild(panel);
        }

        _words.Text = message;
        Choose(true);
    }

    /// <summary>Shows a message that is a picture, the middle of it <paramref name="centreY"/> pixels down the screen.</summary>
    public void Present(Texture2D picture, int centreY)
    {
        ArgumentNullException.ThrowIfNull(picture);
        if (_picture is null)
        {
            _picture = new Sprite2D { Name = "Picture", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
            AddChild(_picture);
        }

        _picture.Texture = picture;
        Vector2 screen = GetViewport()?.GetVisibleRect().Size ?? new Vector2(picture.GetWidth(), 0);
        _picture.Position = new Vector2(Mathf.Floor((screen.X - picture.GetWidth()) / 2), centreY - picture.GetHeight() / 2);
    }

    /// <summary>Shows yes, or no, as the choice.</summary>
    public void Choose(bool yes)
    {
        YesIsChosen = yes;
        if (_choice is null)
            return;
        _choice.Visible = AsksYesOrNo;
        _choice.Text = yes ? "> YES    NO" : "  YES  > NO";
    }
}
