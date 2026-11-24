using Engine.Input;
using Engine.Ui;

namespace Engine.Core;

/// <summary>
/// A message being shown. The mode it was asked for in is kept beneath it and does not run. Confirming dismisses a
/// plain message; a message that asks yes or no is answered by choosing and confirming, and the answer is given to
/// whoever asked.
/// </summary>
public partial class MessageBoxMode : GameMode
{
    private readonly GameMode _returnsTo;
    private readonly string _message;
    private readonly Action<bool>? _answered;
    private MessageBox? _box;
    private bool _yes = true;

    /// <summary>A plain message, named <paramref name="message"/> in the game's words, laid over <paramref name="returnsTo"/>.</summary>
    public MessageBoxMode(GameMode returnsTo, string message)
    {
        ArgumentNullException.ThrowIfNull(returnsTo);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _returnsTo = returnsTo;
        _message = message;
    }

    /// <summary>A message that asks yes or no, giving the answer to <paramref name="answered"/> as it is dismissed.</summary>
    public MessageBoxMode(GameMode returnsTo, string message, Action<bool> answered) : this(returnsTo, message)
    {
        ArgumentNullException.ThrowIfNull(answered);
        _answered = answered;
    }

    /// <summary>Music played while the message shows, by name; the music there was comes back after. Empty for none.</summary>
    public string Fanfare { get; init; } = "";

    /// <summary>How many updates the message stays up before it can be dismissed.</summary>
    public int HoldUpdates { get; init; }

    /// <summary>
    /// Where the message's picture is loaded from, for a game that keeps its messages as pictures: the room's own
    /// pictures, since a message is drawn as the room's set of graphics draws it.
    /// </summary>
    public Content.ContentLibrary? Pictures { get; init; }

    private string _musicBefore = "";
    private int _shownFor;

    /// <inheritdoc/>
    public override GameMode? Keeps => _returnsTo;

    /// <summary>The name of the message being shown.</summary>
    public string Message => _message;

    /// <summary>For a message that asks yes or no, whether yes is the choice at the moment.</summary>
    public bool YesIsChosen => _yes;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _box = new MessageBox { Name = "MessageBox", Font = Session.Font };
        AddChild(_box);
        ShowMessage();
        if (Fanfare.Length > 0)
        {
            _musicBefore = Session.Sounds.Music;
            Session.Sounds.RequestMusic(Fanfare);
        }
    }

    /// <summary>
    /// Shows the message as it now stands. A game that names a folder of message pictures has a picture for each
    /// message, named as the message is, and for one that asks yes or no a picture of each answer chosen, with
    /// "_yes" and "_no" after the name; any other game's messages are their words.
    /// </summary>
    private void ShowMessage()
    {
        MessageBox box = _box ?? throw new InvalidOperationException("The message has not been shown.");
        string folder = Session.Definition.MessagePictureFolder;
        if (folder.Length == 0)
        {
            box.Present(Session.Text.Get(_message), asksYesOrNo: _answered is not null);
            box.Choose(_yes);
            return;
        }

        string answer = _answered is null ? "" : _yes ? "_yes" : "_no";
        Content.ContentLibrary pictures = Pictures ?? Session.Content;
        box.Present(pictures.LoadTexture($"{folder}/{Content.ContentLibrary.GraphicsPlaceholder}/{_message}{answer}.png"), Session.Definition.MessageCentreY);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (_answered is not null && (input.WasPressed(GameAction.Left) || input.WasPressed(GameAction.Right)))
        {
            _yes = !_yes;
            ShowMessage();
        }

        if (_shownFor < HoldUpdates)
        {
            _shownFor++;
            return this;
        }

        if (!input.WasPressed(GameAction.Confirm))
            return this;
        if (Fanfare.Length > 0)
            Session.Sounds.RequestMusic(_musicBefore);
        _answered?.Invoke(_yes);
        return _returnsTo;
    }
}
