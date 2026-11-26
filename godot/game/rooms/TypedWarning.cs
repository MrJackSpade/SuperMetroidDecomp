using Engine.Content;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// A warning typed out across the screen a letter at a time, as when a self-destruct begins. Its typing begins the
/// update after it is started. Once it is all there and the wait after its last letter has passed, what is listed
/// is told so; the words stay written for as long as the room does. A second wording, in the languages that have
/// one, is put up once the typing has gone on a while, and stays as the words do.
/// </summary>
/// <remarks>
/// How a warning is laid out and typed is a data file named for it in <see cref="LayoutFolder"/>: the "font" its
/// words are typed in, where their first letter's cell lies on the screen ("x", "y"), how many updates each letter
/// takes ("updates_per_letter"), how many letters at the start are followed by the next at once
/// ("undelayed_letters"), how many letters appear for each sound ("letters_per_sound"), the sound of a letter
/// appearing ("letter_sound"), and its "second_wordings", each under a name with the "languages" it is shown in, its
/// "pictures", each with where it goes, and how many updates of typing pass before it shows ("shown_after").
/// </remarks>
public partial class TypedWarning : RoomSequence
{
    /// <summary>The warning, by its name in the game's words.</summary>
    [Export]
    public string Message { get; set; } = "";

    /// <summary>The folder, within the assets tree, of the files that say how each warning is laid out.</summary>
    [Export]
    public string LayoutFolder { get; set; } = "text/typed";

    /// <summary>The warning's second wording, by its name in the warning's layout; empty for none.</summary>
    [Export]
    public string Subtitle { get; set; } = "";

    private TypedText? _text;
    private Node2D? _subtitle;
    private int _subtitleAfter;

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
                Sound = layout.Text("letter_sound"), LettersPerSound = layout.Int("letters_per_sound"),
            };
            layer.AddChild(_text);
            _text.Type(room.Session.Text.Get(Message));
            _subtitle = SecondWording(room, layout);
            if (_subtitle is not null)
                layer.AddChild(_subtitle);
            return false;
        }

        TypedText text = _text ?? throw new InvalidOperationException("The warning is not being typed.");
        text.Step(room.Session.Sounds);
        if (_subtitle is not null && elapsed == _subtitleAfter)
            _subtitle.Visible = true;
        return text.IsOver;
    }

    /// <summary>The second wording's pictures, hidden until their time; null when there is none, or none in the language read.</summary>
    private Node2D? SecondWording(Room room, DataNode layout)
    {
        if (Subtitle.Length == 0)
            return null;

        // The second wording is pictures, shown only in the languages that have one.
        DataNode second = layout.Get("second_wordings").Get(Subtitle);
        if (!second.List("languages").Any(language => language.AsText() == room.Session.Options.Language))
            return null;
        _subtitleAfter = second.Int("shown_after");
        var shown = new Node2D { Name = "Subtitle", Visible = false };
        foreach (DataNode line in second.List("pictures"))
        {
            shown.AddChild(new Sprite2D
            {
                Texture = room.Content.LoadTexture(line.Text("picture")), Centered = false, Position = new Vector2(line.Int("x"), line.Int("y")),
            });
        }

        return shown;
    }
}
