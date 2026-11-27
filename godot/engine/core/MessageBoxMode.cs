using Engine.Input;
using Engine.Ui;

namespace Engine.Core;

/// <summary>
/// A message being shown. The mode it was asked for in is kept beneath it and does not run. The game names how its
/// messages are shown (<see cref="GameDefinition.MessageBoxFile"/>): each is a picture through a window that opens
/// and closes, as its <see cref="MessageBoxStyle"/> says. The answer is given to whoever asked as the mode beneath
/// comes back.
/// </summary>
/// <remarks>
/// A message goes through these, an update at a time: nothing showing for the style's updates before
/// opening; the window opening from nothing by the style's step, the first update of it showing nothing, until it
/// is fully open; for a message that is held, its hold; then waiting. A plain message waits until any action at all
/// is held, a new press or not. A question looks at the player's presses only every so many updates, and a press
/// is new if it was not held at the look before (at first, on the update the message was asked for): confirm
/// answers with the choice showing, cancel answers no whatever shows, and otherwise one of the style's move actions
/// shows the other choice, whose sound is heard on the next update, which looks at nothing. The window then closes
/// from fully open, beginning on the update the message was dismissed, to nothing; and after the style's updates
/// after closing, the mode beneath comes back. A question that names what follows a yes (<see cref="AfterYes"/>)
/// and is answered yes sounds its sound on the update its window shows nothing, waits, and brings up its second
/// message the same way before the mode beneath comes back.
/// </remarks>
public partial class MessageBoxMode : GameMode
{
    /// <summary>What a message shown through a window is doing.</summary>
    private enum Phase
    {
        BeforeOpening,
        Opening,
        Held,
        Waiting,
        Closing,
        BeforeFollowing,
        AfterClosing,
    }

    private GameMode _returnsTo;
    private readonly string _message;
    private readonly Action<bool>? _answered;

    /// <summary>The message showing now: the one asked for, or the one that follows a yes to it.</summary>
    private string _shown;

    /// <summary>Whether the message showing now asks yes or no.</summary>
    private bool _asks;

    /// <summary>How many updates the message showing now stays up before it can be dismissed.</summary>
    private int _hold;
    private MessageBox? _box;
    private MessageBoxStyle? _style;
    private bool _yes = true;

    /// <summary>A plain message, named <paramref name="message"/> in the game's words, laid over <paramref name="returnsTo"/>.</summary>
    public MessageBoxMode(GameMode returnsTo, string message)
    {
        ArgumentNullException.ThrowIfNull(returnsTo);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _returnsTo = returnsTo;
        _message = message;
        _shown = message;
    }

    /// <summary>A message that asks yes or no, giving the answer to <paramref name="answered"/> as it is dismissed.</summary>
    public MessageBoxMode(GameMode returnsTo, string message, Action<bool> answered) : this(returnsTo, message)
    {
        ArgumentNullException.ThrowIfNull(answered);
        _answered = answered;
        _asks = true;
    }

    /// <summary>Music played while the message shows, by name; the music there was comes back after. Empty for none.</summary>
    public string Fanfare { get; init; } = "";

    /// <summary>How many updates the message stays up before it can be dismissed.</summary>
    public int HoldUpdates { get; init; }

    /// <summary>For a question, what follows a yes before the mode beneath comes back; null for nothing.</summary>
    public MessageAfterYes? AfterYes { get; init; }

    /// <summary>
    /// Where the message's picture is loaded from, for a game that keeps its messages as pictures: the room's own
    /// pictures, since a message is drawn as the room's set of graphics draws it.
    /// </summary>
    public Content.ContentLibrary? Pictures { get; init; }

    /// <summary>
    /// What the player held on the update the message was asked for. A question shown through a window takes a
    /// press as new against it at its first look.
    /// </summary>
    public GameInput HeldBefore { get; init; } = GameInput.None;

    /// <summary>
    /// Whether this message opens as another hands back, with play still covered: it then asks for no silence of
    /// its own, whatever its style says.
    /// </summary>
    public bool FollowsAnother { get; init; }

    private Phase _phase;
    private int _wait;
    private int _radius;
    private int _nextRadius;
    private bool _choiceMoved;
    private bool _answer;
    private HashSet<GameAction> _heldAtLastLook = [];

    /// <summary>The pause running while nothing shows, an entry for each of its updates, and how many of them have been played.</summary>
    private IReadOnlyList<MessagePauseUpdate> _pause = [];
    private int _pauseAt;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    /// <summary>
    /// A message holds what it covers still, and the clock with it: its updates are not counted, the one that hands
    /// back among them.
    /// </summary>
    public override bool CountsOnClock => false;

    /// <summary>
    /// A message shown through a window does the sounds' work itself while it covers play: the music gets no turn
    /// as its updates open.
    /// </summary>
    public override bool GivesMusicItsTurn => false;

    /// <summary>
    /// What closes the update just played. Shown through a window, an update gives the music a turn and the sounds
    /// one, unless it is one of the pauses' updates, which do as the style says, or the update that shows a
    /// question's choice moved, which gives neither.
    /// </summary>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <inheritdoc/>
    public override GameMode? Keeps => _returnsTo;

    /// <summary>The name of the message being shown.</summary>
    public string Message => _message;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _box = new MessageBox { Name = "MessageBox" };
        AddChild(_box);
        _hold = HoldUpdates;
        string file = Session.Definition.MessageBoxFile;
        if (file.Length == 0)
            throw new InvalidOperationException("The game's definition names no file that says how its messages are shown.");
        _style = MessageBoxStyle.Read(Session.Content.ReadData(file));
        _heldAtLastLook = HeldOf(HeldBefore, _style);
        BeginOpening(_style.BeforeOpening);

        // What play asked to hear on the update that asked for the message was asked for first.
        if (_style.SilencesSounds && !FollowsAnother)
            Session.Sounds.StopAll();
        ShowMessage();
        if (Fanfare.Length > 0)
            Session.Sounds.RequestFanfare(Fanfare);
    }

    /// <summary>
    /// Shows the message as it now stands: its picture, of the answer chosen and of the button bound, through the
    /// window as far as it is open.
    /// </summary>
    private void ShowMessage()
    {
        MessageBox box = _box ?? throw NotShown();
        MessageBoxStyle style = _style ?? throw NotShown();
        Content.ContentLibrary pictures = Pictures ?? Session.Content;
        string picture = style.PicturePath(_shown, _asks ? _yes : null, Session.Options.ControlOf);
        box.Present(pictures.LoadTexture(picture), style.CentreY, _radius);
    }

    private static InvalidOperationException NotShown() => new("The message has not been shown.");

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        _returnsTo.StepCovered();
        return StepWindow(input, _style ?? throw NotShown());
    }

    /// <summary>
    /// Has the message lie over <paramref name="mode"/> and hand control back to it, in place of the mode it was made
    /// over: for a mode that is itself laid over that one and is not done, as a pause is over play while it darkens
    /// the picture. That mode then takes back for the one beneath it (<see cref="GameMode.TakeBack"/>) and plays what
    /// of it goes on beneath the message.
    /// </summary>
    public void HandBackTo(GameMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        _returnsTo = mode;
    }

    /// <summary>Turns from the question answered yes to the message that follows it, sounding its sound.</summary>
    private void Follow(MessageAfterYes after)
    {
        if (after.Sound.Length > 0)
            Session.Sounds.Request(after.Sound, channel: "menu");
        (_shown, _asks, _hold) = (after.Message, false, after.HoldUpdates);
    }

    /// <summary>One update of a message shown through a window.</summary>
    private GameMode StepWindow(GameInput input, MessageBoxStyle style)
    {
        _closing = new ClosingSoundWork(MusicTurn: true, SoundTurns: 1);
        switch (_phase)
        {
            case Phase.BeforeOpening:
                PlayPauseUpdate();
                if (--_wait == 0)
                    _phase = Phase.Opening;
                return this;

            case Phase.Opening:
                _radius = _nextRadius;
                ShowMessage();
                if (_radius < style.FullRadius)
                    _nextRadius += style.RadiusStep;
                else if (_hold > 0)
                    (_phase, _wait) = (Phase.Held, _hold);
                else
                    (_phase, _wait) = (Phase.Waiting, style.FirstReadUpdates);
                return this;

            case Phase.Held:
                if (--_wait == 0)
                    (_phase, _wait) = (Phase.Waiting, style.FirstReadUpdates);
                return this;

            case Phase.Waiting:
                if (!_asks)
                    return input.AnyHeld ? BeginClosing(style) : this;
                if (_choiceMoved)
                {
                    // The update that shows the moved choice asks for its sound and gives neither music nor sounds a turn.
                    _choiceMoved = false;
                    if (style.MoveSound.Length > 0)
                        Session.Sounds.Request(style.MoveSound, channel: "menu");
                    _closing = ClosingSoundWork.None;
                    return this;
                }

                if (_wait > 0)
                {
                    _wait--;
                    return this;
                }

                _wait = style.UpdatesBetweenReads;
                HashSet<GameAction> held = HeldOf(input, style);
                var pressed = new HashSet<GameAction>(held.Where(action => !_heldAtLastLook.Contains(action)));
                _heldAtLastLook = held;
                if (pressed.Contains(GameAction.Confirm))
                {
                    _answer = _yes;
                    return BeginClosing(style);
                }

                if (pressed.Contains(GameAction.Cancel))
                {
                    _answer = false;
                    return BeginClosing(style);
                }

                if (style.MoveActions.Any(pressed.Contains))
                {
                    _yes = !_yes;
                    _choiceMoved = true;
                    ShowMessage();
                }

                return this;

            case Phase.Closing:
                return StepClosing(style);

            case Phase.BeforeFollowing:
                if (--_wait == 0)
                    BeginOpening(AfterYes?.BeforeOpeningUpdates ?? throw new InvalidOperationException("Nothing follows this question."));
                return this;

            case Phase.AfterClosing:
                PlayPauseUpdate();
                return --_wait > 0 ? this : Finish(input);

            default:
                throw new InvalidOperationException($"A message is doing {_phase}, which is nothing it can do.");
        }
    }

    /// <summary>The actions a question looks at that <paramref name="input"/> holds.</summary>
    private static HashSet<GameAction> HeldOf(GameInput input, MessageBoxStyle style) =>
        [.. style.MoveActions.Append(GameAction.Confirm).Append(GameAction.Cancel).Where(input.IsHeld)];

    /// <summary>Begins a message from a shut window, with a pause of nothing, an entry an update, before it begins to open.</summary>
    private void BeginOpening(IReadOnlyList<MessagePauseUpdate> pause)
    {
        BeginOpening(pause.Count);
        (_pause, _pauseAt) = (pause, 0);
    }

    /// <summary>
    /// Begins a message from a shut window, with so many updates of nothing before it begins to open, each giving
    /// the music a turn and the sounds one.
    /// </summary>
    private void BeginOpening(int updatesBefore)
    {
        (_radius, _nextRadius) = (0, 0);
        (_phase, _wait) = updatesBefore > 0 ? (Phase.BeforeOpening, updatesBefore) : (Phase.Opening, 0);
        (_pause, _pauseAt) = ([], 0);
    }

    /// <summary>
    /// One update of a pause of nothing does what its entry says of the sounds' work, and opens the update of what
    /// lies beneath if it says so. A pause with no entries gives the music a turn and the sounds one on each update.
    /// </summary>
    private void PlayPauseUpdate()
    {
        if (_pauseAt >= _pause.Count)
            return;
        MessagePauseUpdate update = _pause[_pauseAt++];
        _closing = new ClosingSoundWork(update.MusicTurn, update.SoundTurns);
        if (update.OpensRoom)
            _returnsTo.StepBeneath();
    }

    /// <summary>Begins closing the window; its first closing update is this one.</summary>
    private GameMode BeginClosing(MessageBoxStyle style)
    {
        RestoreMusic();
        _nextRadius = style.FullRadius;
        _phase = Phase.Closing;
        return StepClosing(style);
    }

    /// <summary>Shows the window one step further closed; once it has shown nothing, the wait after closing begins.</summary>
    private GameMode StepClosing(MessageBoxStyle style)
    {
        _radius = _nextRadius;
        ShowMessage();
        _nextRadius -= style.RadiusStep;
        if (_nextRadius >= 0)
            return this;
        if (_asks && _answer && AfterYes is { } after)
        {
            Follow(after);
            if (after.WaitUpdates > 0)
                (_phase, _wait) = (Phase.BeforeFollowing, after.WaitUpdates);
            else
                BeginOpening(after.BeforeOpeningUpdates);
            return this;
        }

        (_phase, _wait) = (Phase.AfterClosing, style.AfterClosing.Count);
        (_pause, _pauseAt) = (style.AfterClosing, 0);
        return this;
    }

    /// <summary>Says the message's fanfare has had its time: a flow that has not brought the music back by its own count does so.</summary>
    private void RestoreMusic()
    {
        if (Fanfare.Length > 0)
            Session.Sounds.EndFanfare();
    }

    /// <summary>
    /// Gives the answer to whoever asked, and control back to the mode beneath, which does within this update
    /// whatever of its own it had left undone and says what is in control next.
    /// </summary>
    private GameMode Finish(GameInput input)
    {
        _answered?.Invoke(_answer);
        return _returnsTo.TakeBack(input);
    }
}
