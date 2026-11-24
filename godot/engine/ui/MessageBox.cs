using Godot;

namespace Engine.Ui;

/// <summary>
/// A box showing a message, with a yes and a no to choose between when the message asks. It shows; the mode that
/// presents it decides when it is dismissed, and it grants nothing and saves nothing itself.
/// </summary>
public partial class MessageBox : CanvasLayer
{
    private PixelText? _words;
    private PixelText? _choice;

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
