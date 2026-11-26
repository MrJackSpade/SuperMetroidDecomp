using Engine.Content;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// A warning typed out across the screen a letter at a time, as when a self-destruct begins. Its typing begins the
/// update after it is started. Once it is all there and the wait after its last letter has passed, what is listed
/// is told so; the words stay written for as long as the room does.
/// </summary>
/// <remarks>
/// How a warning is laid out and typed is a data file named for it in <see cref="LayoutFolder"/>: the "font" its
/// words are typed in, where their first letter's cell lies on the screen ("x", "y"), how many updates each letter
/// takes ("updates_per_letter"), how many letters at the start are followed by the next at once
/// ("undelayed_letters"), and how many letters appear for each sound ("letters_per_sound").
/// </remarks>
public partial class TypedWarning : RoomSequence
{
    /// <summary>The warning, by its name in the game's words.</summary>
    [Export]
    public string Message { get; set; } = "";

    /// <summary>The folder, within the assets tree, of the files that say how each warning is laid out.</summary>
    [Export]
    public string LayoutFolder { get; set; } = "text/typed";

    /// <summary>The sound of a letter appearing, by name.</summary>
    [Export]
    public string LetterSound { get; set; } = "";

    private TypedText? _text;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
        {
            // A warning typed before is written over.
            if (_text?.GetParent() is { } before)
            {
                RemoveChild(before);
                before.QueueFree();
            }

            var layer = new CanvasLayer { Name = "Warning", Layer = 5 };
            AddChild(layer);
            DataNode layout = room.Session.Content.ReadData($"{LayoutFolder}/{Message}.json");
            _text = new TypedText
            {
                Name = "Words", Font = PixelFont.Read(room.Session.Content, layout.Text("font")), Position = new Vector2(layout.Int("x"), layout.Int("y")),
                UpdatesPerCharacter = layout.Int("updates_per_letter"), UndelayedCharacters = layout.Int("undelayed_letters"),
                Sound = LetterSound, LettersPerSound = layout.Int("letters_per_sound"),
            };
            layer.AddChild(_text);
            _text.Type(room.Session.Text.Get(Message));
            return false;
        }

        TypedText text = _text ?? throw new InvalidOperationException("The warning is not being typed.");
        text.Step(room.Session.Sounds);
        return text.IsOver;
    }
}
