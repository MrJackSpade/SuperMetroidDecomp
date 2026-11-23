using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// A warning typed out across the screen a letter at a time, as when a self-destruct begins. Once it is all there
/// and has been held a moment, it is taken away and what is listed is told so.
/// </summary>
public partial class TypedWarning : RoomSequence
{
    /// <summary>The warning, by its name in the game's words.</summary>
    [Export]
    public string Message { get; set; } = "";

    /// <summary>How many updates each letter takes.</summary>
    [Export]
    public int UpdatesPerLetter { get; set; } = 2;

    /// <summary>The sound of a letter appearing, by name.</summary>
    [Export]
    public string LetterSound { get; set; } = "";

    /// <summary>How many updates the whole warning stays up.</summary>
    [Export]
    public int HoldUpdates { get; set; } = 64;

    /// <summary>The things told once it is over.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenDone { get; set; } = [];

    /// <summary>What they are told.</summary>
    [Export]
    public string Tells { get; set; } = "done";

    private TypedText? _text;
    private int _held;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
        {
            var layer = new CanvasLayer { Name = "Warning", Layer = 5 };
            AddChild(layer);
            _text = new TypedText { Name = "Words", UpdatesPerCharacter = UpdatesPerLetter, Sound = LetterSound };
            _text.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            layer.AddChild(_text);
            _text.Type(room.Session.Text.Get(Message));
            _held = 0;
        }

        TypedText text = _text ?? throw new InvalidOperationException("The warning is not being typed.");
        if (!text.IsDone)
        {
            text.Step(room.Session.Sounds);
            return false;
        }

        if (++_held < HoldUpdates)
            return false;
        text.GetParent().QueueFree();
        _text = null;
        foreach (NodePath path in ToldWhenDone)
        {
            (GetNode(path) as ITriggerTarget
                ?? throw new InvalidOperationException($"The warning \"{Name}\" is to tell \"{path}\" that it is over, but that is nothing that can be told."))
                .Triggered(Tells);
        }

        return true;
    }
}
