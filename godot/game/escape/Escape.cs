using Engine.Core;
using Engine.Shared;
using Game.Cinematics;

namespace Game;

/// <summary>
/// The escape countdown. Once begun it shows in the middle of the screen, starts to count, slides to its corner and
/// counts down to nothing; with none left, time is up. It does not know which room the player is in or why it was
/// begun.
/// </summary>
public sealed class Escape : ISessionService
{
    /// <summary>How long Samus has to leave Ceres station, in hundredths of a second.</summary>
    public const int CeresTime = 1 * 60 * 100;

    /// <summary>How long Samus has to leave the planet, in hundredths of a second.</summary>
    public const int ZebesTime = 3 * 60 * 100;

    /// <summary>Where its readout is while it waits in the middle of the screen, and where it settles, in screen pixels.</summary>
    private const int CentreX = 128, CentreY = 128, SettledX = 220, SettledY = 48;

    /// <summary>How many updates it waits before it counts, and then counts in the middle before it moves.</summary>
    private const int InitialDelay = 16, CentreCounting = 80;

    /// <summary>
    /// How many hundredths of a second each update of a run of 128 takes off: the game's sixty updates a second, as
    /// nearly as whole hundredths allow.
    /// </summary>
    private static readonly int[] Cadence =
    [
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
    ];

    private static readonly Pixels RightwardSpeed = Pixels.FromNumber(7 / 8.0);
    private static readonly Pixels UpwardSpeed = Pixels.FromNumber(193 / 256.0);

    private enum Phase
    {
        Inactive,
        Starting,
        InitialDelay,
        CountingAtCentre,
        MovingIntoPlace,
        InPlace,
        Expired,
        Stopped,
    }

    /// <summary>How many parts the white-out that follows time running out whitens the screen in.</summary>
    public const int WhiteOutParts = 9;

    /// <summary>
    /// How many updates are left of a harder spell of the escape's shaking. It is the escape's and no room's: a spell
    /// goes on through a door into the next of the escape's rooms that shakes harder by turns, standing still through
    /// any room between that does not, unless a room starts its shaking afresh.
    /// </summary>
    public int HarderShakeLeft { get; set; }

    private (string Room, string EntryPoint)? _leavingFor;
    private bool _leftZebes;
    private bool _timeUpDue;
    private int _whiteOutSteps;

    /// <summary>How many of its <see cref="WhiteOutParts"/> parts time running out has whitened the screen by.</summary>
    public int Whitened => Math.Clamp(_whiteOutSteps - 1, 0, WhiteOutParts);
    private Phase _phase;
    private int _wait;
    private PixelPosition _place;
    private bool _played;

    /// <summary>Whether the countdown is running or has run out. One begun is not running until it starts.</summary>
    public bool IsRunning => _phase is not (Phase.Inactive or Phase.Starting or Phase.Stopped);

    /// <summary>
    /// Whether its readout shows: while it runs, on the updates it plays. It does not play, or show, while Samus is
    /// in another's hands, unless whoever has her says it shows as it stands (<see cref="ShowsWhileHeld"/>).
    /// </summary>
    public bool IsShown => IsRunning && (_played || ShowsWhileHeld);

    /// <summary>Whether the readout shows as it stands while Samus is in another's hands; whoever takes her so says.</summary>
    public bool ShowsWhileHeld { get; set; }

    /// <summary>Whether time is up.</summary>
    public bool IsExpired => _phase == Phase.Expired;

    /// <summary>How much time is left, in hundredths of a second.</summary>
    public int Remaining { get; private set; }

    /// <summary>The minutes left.</summary>
    public int Minutes => Remaining / 6000;

    /// <summary>The seconds left within the minute.</summary>
    public int Seconds => Remaining / 100 % 60;

    /// <summary>The hundredths left within the second.</summary>
    public int Hundredths => Remaining % 100;

    /// <summary>Where its readout is on the screen, in pixels.</summary>
    public (int X, int Y) ScreenPlace => (_place.X.Whole, _place.Y.Whole);

    /// <summary>
    /// Begins the countdown from <paramref name="hundredths"/> of a second. It starts on the next update of its own,
    /// which may be this one, and its first wait is counted from the update after.
    /// </summary>
    public void Begin(int hundredths)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hundredths);
        Remaining = hundredths;
        (_played, ShowsWhileHeld) = (false, false);
        (_timeUpDue, _whiteOutSteps) = (false, 0);
        _place = PixelPosition.FromWhole(CentreX, CentreY);
        _phase = Phase.Starting;
    }

    /// <summary>Samus has left Zebes: the countdown stops, and after this update the ending follows.</summary>
    public void LeaveZebes() => _leftZebes = true;

    /// <summary>
    /// The elevator of Ceres has Samus: after this update her departure begins, at whose end the station is seen
    /// blowing up behind her, before play goes on in <paramref name="room"/>, entered at <paramref name="entryPoint"/>.
    /// </summary>
    public void Depart(string room, string entryPoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(room);
        _leavingFor = (room, entryPoint);
    }

    /// <summary>Stops the countdown where it stands, as when the escape is over.</summary>
    public void Stop() => (_phase, _timeUpDue) = (Phase.Stopped, false);

    /// <inheritdoc/>
    public void BeginAgain()
    {
        _phase = Phase.Inactive;
        (_played, ShowsWhileHeld) = (false, false);
        _leavingFor = null;
        _leftZebes = false;
        (_timeUpDue, _whiteOutSteps) = (false, 0);
        Remaining = 0;
        HarderShakeLeft = 0;
    }

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        // Time up is asked for on the update the countdown runs out. Once the planet's escape is on it is asked for
        // again on every update of play that finds the countdown run out, so a time up that a door or a refill from
        // her reserves took play from comes back; on Ceres it does not.
        _timeUpDue = IsExpired && session.Progress.Has(Data.StoryEvents.EscapeStarted);

        // Only an update Samus plays in her own hands is one of the countdown's: held for a refill from her reserves,
        // dying, riding an elevator, putting on a suit or aboard her ship, it neither waits, moves nor counts, so it
        // cannot run out while she is held.
        _played = !session.Player.State.InAnothersHands;
        if (!_played)
            return;
        long updates = session.Clock.Updates;
        switch (_phase)
        {
            case Phase.Inactive:
            case Phase.Expired:
            case Phase.Stopped:
                break;

            case Phase.Starting:
                // Starting takes no step of the first wait.
                (_phase, _wait) = (Phase.InitialDelay, InitialDelay);
                break;

            case Phase.InitialDelay:
                // The first wait never counts, even on the update that ends it.
                if (--_wait <= 0)
                    (_phase, _wait) = (Phase.CountingAtCentre, CentreCounting);
                break;

            case Phase.CountingAtCentre:
                if (--_wait <= 0)
                    _phase = Phase.MovingIntoPlace;
                Count(updates);
                break;

            case Phase.MovingIntoPlace:
                Pixels across = _place.X + RightwardSpeed, down = _place.Y - UpwardSpeed;
                bool thereAcross = across.Whole >= SettledX, thereDown = down.Whole < SettledY;
                _place = new PixelPosition(thereAcross ? Pixels.FromWhole(SettledX) : across, thereDown ? Pixels.FromWhole(SettledY) : down);
                if (thereAcross && thereDown)
                    _phase = Phase.InPlace;
                Count(updates);
                break;

            case Phase.InPlace:
                Count(updates);
                break;

            default:
                throw new InvalidOperationException($"The escape countdown has no phase {_phase}.");
        }
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => _timeUpDue || _leftZebes || _leavingFor is not null;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing)
    {
        // Time up comes before her leaving either place, as on the cartridge. The countdown stays run out, and shown,
        // until the time up has gone black and stops it.
        if (_timeUpDue)
        {
            _timeUpDue = false;
            return new TimeUpMode(playing);
        }

        if (_leftZebes)
        {
            // What the ending tells of her is taken down now, while she is still here to be asked.
            _leftZebes = false;
            Stop();
            GameSession session = playing.Room.Session;
            long minutes = session.Progress.PlayUpdates / 3600;
            return new Ending
            {
                ClearHours = (int)Math.Min(99, minutes / 60),
                ClearMinutes = (int)(minutes % 60),
                ItemsFound = Ending.CountItems(playing.Room.Player.Inventory),
                AnimalsEscaped = session.Progress.Has(Data.StoryEvents.AnimalsEscaped),
            };
        }

        // The countdown still stands, and shows, beneath the departure, which stops it once the screen is black.
        if (_leavingFor is { } landing)
        {
            _leavingFor = null;
            return new CeresDeparture(playing) { LandingRoom = landing.Room, LandingEntry = landing.EntryPoint };
        }

        return null;
    }

    /// <summary>
    /// Takes one step of the white-out that follows time running out, and returns whether the screen was white
    /// already, when no step is taken and the white-out is over. Its first step leaves the screen as it was and each
    /// of the next <see cref="WhiteOutParts"/> whitens it by a part. Whatever takes play from the time up leaves
    /// the white-out where it stood. When it is over is by <paramref name="count"/>, the count every fade shares.
    /// </summary>
    public bool StepWhiteOut(Compatibility.SharedFadeCount count)
    {
        ArgumentNullException.ThrowIfNull(count);
        if (count.TakeStep(WhiteOutParts))
        {
            _whiteOutSteps = WhiteOutParts + 1;
            return true;
        }

        _whiteOutSteps++;
        return false;
    }

    private void Count(long updates)
    {
        Remaining = Math.Max(0, Remaining - Cadence[(int)(updates % Cadence.Length)]);
        if (Remaining == 0)
            (_phase, _timeUpDue) = (Phase.Expired, true);
    }
}
