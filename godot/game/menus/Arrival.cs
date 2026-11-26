using Engine.Core;
using Engine.Input;
using Engine.Ui;

namespace Game.Menus;

/// <summary>
/// What follows a game's load (<see cref="GameLoad"/>), laid over the play it made. The screen is held black for the
/// load's waits, on which nothing plays: no random number is drawn and neither the music nor the sounds get a turn.
/// Then play goes on beneath it while the screen comes up from black, a level every other update, and when it is
/// full play is its own.
/// </summary>
public partial class Arrival : GameMode
{
    /// <summary>
    /// How many updates the black is held after a load: sixty for the music, and seven more, or sixteen more for
    /// the landing on Zebes after Ceres.
    /// </summary>
    public const int OrdinaryWaits = 67, LandingWaits = 76;

    /// <summary>How many levels the screen comes up in, and how many updates each takes: the first of them waits, the second steps.</summary>
    private const int Levels = 15, UpdatesALevel = 2;

    /// <summary>
    /// The terms play goes on beneath the fade on, as the cartridge has it: Samus with no energy left neither begins
    /// to die nor is saved by her reserves, the pause is refused, and nothing takes play elsewhere.
    /// </summary>
    private static readonly PlayTerms BeneathTheFade = new() { PlayerMayDie = false, PlayerMayBeSaved = false, MayPause = false, ServicesMayTakeOver = false };

    private readonly PlayingMode _playing;
    private readonly bool _savesFirstArrival;
    private readonly bool _samusAppears;
    private int _waitsLeft;
    private int _faded;
    private bool _begun;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    /// <summary>
    /// The arrival into <paramref name="playing"/>, the black held for <paramref name="waits"/> updates first.
    /// <paramref name="savesFirstArrival"/> says the game is saved as play is made, to resume in the game's first room
    /// at its first entry point, as a new game's arrival on Ceres is. <paramref name="samusAppears"/> says Samus
    /// appears where play begins, as where a saved game is taken up: she is set appearing before play's first update.
    /// </summary>
    public Arrival(PlayingMode playing, int waits, bool savesFirstArrival = false, bool samusAppears = false)
    {
        ArgumentNullException.ThrowIfNull(playing);
        ArgumentOutOfRangeException.ThrowIfNegative(waits);
        _playing = playing;
        _waitsLeft = waits;
        _savesFirstArrival = savesFirstArrival;
        _samusAppears = samusAppears;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <summary>An update that only waits gives the music no turn.</summary>
    public override bool GivesMusicItsTurn => _waitsLeft == 0;

    /// <summary>An update that only waited gives the sounds none.</summary>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <inheritdoc/>
    protected override void Enter()
    {
        // The black is the screen's own darkness, which this brings up level by level.
        Session.Darkness.Show(Session.Darkness.Levels);
        _begun = true;

        // Play, made beneath this, has its room and its player set up by now: the game is saved as it stands.
        if (_savesFirstArrival)
            Session.Saves.Write(Session.Slot, SaveData.Capture(Session, Session.Definition.FirstRoom, Session.Definition.FirstEntryPoint));

        // Her appearance begins before play's first update, which is its first: the black's waits are none of it.
        if (_samusAppears)
            ((Samus)Session.Player).Appear();
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (!_begun)
            throw new InvalidOperationException("The arrival has not begun.");
        ScreenDarkness darkness = Session.Darkness;
        if (_waitsLeft > 0)
        {
            _waitsLeft--;
            _closing = ClosingSoundWork.None;
            return this;
        }

        // A whole update of play, shown at the level the screen had reached before this update's step.
        _closing = ClosingSoundWork.Usual;
        if (_playing.PlayBeneath(input, BeneathTheFade) is { } other)
            throw new InvalidOperationException($"Play beneath the arrival's fade handed over to {other.GetType().Name}; nothing play leads to is followed while the screen comes up.");
        int dark = Levels - _faded / UpdatesALevel;
        _faded++;
        if (_faded < Levels * UpdatesALevel)
        {
            darkness.Show(dark);
            return this;
        }

        // The last level is seen through this update, and play is its own from the next, fully lit, its fade at no pace.
        darkness.SetPace(0, 0);
        darkness.ShowThenLeave(dark, 0);
        return _playing;
    }
}
