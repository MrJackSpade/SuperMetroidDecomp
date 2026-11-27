using Engine.Content;
using Engine.Input;

namespace Engine.Ui;

/// <summary>
/// How a game shows its messages when each is a picture: where the picture lies, the window about its middle line
/// that opens before it is read and closes after, when a question reads the player's answer, and which of a
/// message's pictures names the button the player has bound to an action.
/// </summary>
/// <remarks>
/// The data file is a group of:
/// <list type="bullet">
/// <item><c>pictures</c>: the folder of the messages' pictures, with one folder in it for each set of graphics; a
/// message's picture there is named as the message is, with <c>_yes</c> or <c>_no</c> after it for the answer a
/// question shows chosen.</item>
/// <item><c>centre_y</c>: how far down the screen the window's middle line is, which the picture's own middle lies
/// on.</item>
/// <item><c>full_radius</c>, <c>radius_step</c>: how many lines above and below its middle the open window shows,
/// and by how many it grows or shrinks an update.</item>
/// <item><c>before_opening</c>, <c>after_closing</c>: the updates on which nothing shows before the window begins
/// to open and after it has closed, one entry for each; play goes on after the last of the second. Each entry is
/// a group saying what that update does while the message covers play: <c>music</c>, whether the music waiting
/// gets a turn; <c>sound_turns</c>, how many turns the sounds waiting get; and <c>opens_room</c>, whether the room
/// beneath opens an update of its own (its liquid, its sequences' and its projectiles' openings).</item>
/// <item><c>silences_sounds</c>: whether every sound is asked to stop as a message is shown.</item>
/// <item><c>first_read_updates</c>, <c>updates_between_reads</c>: for a question, how many updates pass between the
/// window being open and the first look at what the player presses, and between one look and the next.</item>
/// <item><c>move_actions</c>: the actions that move a question's choice to the other answer.</item>
/// <item><c>move_sound</c>: the sound of the choice moving, heard the update after it moves.</item>
/// <item><c>names_action</c>: a group giving, for each message that names a button, the action whose control it
/// names. Such a message has a picture for each control of <c>controls</c>, named with <c>_</c> and that control's
/// <c>picture</c> after the message's name.</item>
/// <item><c>controls</c>: the controls a message can name, each a group of <c>control</c>, its name, and
/// <c>picture</c>. The one the action is on is the one shown.</item>
/// </list>
/// </remarks>
public sealed class MessageBoxStyle
{
    private readonly List<(string Control, string Picture)> _controls = [];
    private readonly Dictionary<string, string> _namesAction = [];

    private MessageBoxStyle()
    {
    }

    /// <summary>The folder of the messages' pictures.</summary>
    public string PictureFolder { get; private init; } = "";

    /// <summary>How far down the screen the window's middle line is, in pixels.</summary>
    public int CentreY { get; private init; }

    /// <summary>How many lines above and below its middle the open window shows.</summary>
    public int FullRadius { get; private init; }

    /// <summary>By how many lines the window grows or shrinks an update.</summary>
    public int RadiusStep { get; private init; }

    /// <summary>What each update does on which nothing shows before the window begins to open, in order.</summary>
    public IReadOnlyList<MessagePauseUpdate> BeforeOpening { get; private init; } = [];

    /// <summary>What each update does on which nothing shows after the window has closed, in order; play goes on after the last.</summary>
    public IReadOnlyList<MessagePauseUpdate> AfterClosing { get; private init; } = [];

    /// <summary>Whether every sound is asked to stop as a message is shown.</summary>
    public bool SilencesSounds { get; private init; }

    /// <summary>How many updates pass between a question's window being open and the first look at the player's presses.</summary>
    public int FirstReadUpdates { get; private init; }

    /// <summary>How many updates pass between one look at the player's presses and the next.</summary>
    public int UpdatesBetweenReads { get; private init; }

    /// <summary>The actions that move a question's choice to the other answer.</summary>
    public IReadOnlyList<GameAction> MoveActions { get; private init; } = [];

    /// <summary>The sound of a question's choice moving.</summary>
    public string MoveSound { get; private init; } = "";

    /// <summary>Reads a style from its data file.</summary>
    /// <exception cref="ContentException">A value is missing or cannot be what it must be.</exception>
    public static MessageBoxStyle Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var style = new MessageBoxStyle
        {
            PictureFolder = data.Text("pictures"),
            CentreY = data.Int("centre_y"),
            FullRadius = data.Int("full_radius"),
            RadiusStep = data.Int("radius_step"),
            BeforeOpening = [.. data.List("before_opening").Select(MessagePauseUpdate.Read)],
            AfterClosing = [.. data.List("after_closing").Select(MessagePauseUpdate.Read)],
            SilencesSounds = data.Flag("silences_sounds"),
            FirstReadUpdates = data.Int("first_read_updates"),
            UpdatesBetweenReads = data.Int("updates_between_reads"),
            MoveActions = [.. data.List("move_actions").Select(action => new GameAction(action.AsText()))],
            MoveSound = data.Text("move_sound"),
        };
        if (style.FullRadius <= 0 || style.RadiusStep <= 0 || style.FullRadius % style.RadiusStep != 0)
            throw data.Problem("has a window that does not open to its full height in whole steps");
        if (style.AfterClosing.Count == 0)
            throw data.Problem("has no update after its window has closed for play to go on after");
        foreach (DataNode control in data.List("controls"))
            style._controls.Add((control.Text("control"), control.Text("picture")));
        foreach ((string message, DataNode action) in data.Get("names_action").Entries())
            style._namesAction.Add(message, action.AsText());
        return style;
    }

    /// <summary>
    /// The path of the picture of <paramref name="message"/> as it shows now, within the assets tree, with the
    /// placeholder for the set of graphics in it.
    /// </summary>
    /// <param name="message">The message, by name.</param>
    /// <param name="answer">For a question, the answer that shows chosen; null for a message that asks nothing.</param>
    /// <param name="controlOf">The control an action is on, by the action's name.</param>
    /// <exception cref="InvalidOperationException">The message names an action that is on a control it has no picture for.</exception>
    public string PicturePath(string message, bool? answer, Func<string, string> controlOf)
    {
        ArgumentNullException.ThrowIfNull(controlOf);
        string name = message;
        if (_namesAction.TryGetValue(message, out string? action))
        {
            string control = controlOf(action);
            int shown = _controls.FindIndex(pictured => pictured.Control == control);
            if (shown < 0)
                throw new InvalidOperationException($"The message \"{message}\" names the button of the action \"{action}\", which is on the control \"{control}\": the message has no picture for it.");
            name += "_" + _controls[shown].Picture;
        }

        if (answer is { } yes)
            name += yes ? "_yes" : "_no";
        return $"{PictureFolder}/{ContentLibrary.GraphicsPlaceholder}/{name}.png";
    }
}
