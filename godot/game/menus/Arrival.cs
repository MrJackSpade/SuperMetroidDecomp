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
    private int _waitsLeft;
    private int _faded;
    private ScreenFade? _black;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    /// <summary>The arrival into <paramref name="playing"/>, the black held for <paramref name="waits"/> updates first.</summary>
    public Arrival(PlayingMode playing, int waits)
    {
        ArgumentNullException.ThrowIfNull(playing);
        ArgumentOutOfRangeException.ThrowIfNegative(waits);
        _playing = playing;
        _waitsLeft = waits;
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
        _black = new ScreenFade { Name = "Black", Layer = 20 };
        AddChild(_black);
        _black.Cover(Levels, Levels);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        ScreenFade black = _black ?? throw new InvalidOperationException("The arrival has not begun.");
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
        black.Cover(Levels - _faded / UpdatesALevel, Levels);
        _faded++;
        return _faded < Levels * UpdatesALevel ? this : _playing;
    }
}
