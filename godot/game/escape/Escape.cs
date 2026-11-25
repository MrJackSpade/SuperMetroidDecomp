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
        InitialDelay,
        CountingAtCentre,
        MovingIntoPlace,
        InPlace,
        Expired,
        Stopped,
    }

    private (string Room, string EntryPoint)? _leavingFor;
    private bool _leftZebes;
    private Phase _phase;
    private int _wait;
    private PixelPosition _place;
    private bool _played;

    /// <summary>Whether the countdown is running or has run out.</summary>
    public bool IsRunning => _phase is not (Phase.Inactive or Phase.Stopped);

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

    /// <summary>Begins the countdown from <paramref name="hundredths"/> of a second.</summary>
    public void Begin(int hundredths)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hundredths);
        Remaining = hundredths;
        (_played, ShowsWhileHeld) = (false, false);
        _place = PixelPosition.FromWhole(CentreX, CentreY);
        (_phase, _wait) = (Phase.InitialDelay, InitialDelay);
    }

    /// <summary>Samus has left Zebes: the countdown stops, and after this update the ending follows.</summary>
    public void LeaveZebes() => _leftZebes = true;

    /// <summary>
    /// Samus has left Ceres: the countdown stops, and after this update the screen goes dark and the station is seen
    /// blowing up behind her, before play goes on in <paramref name="room"/>, entered at <paramref name="entryPoint"/>.
    /// </summary>
    public void Depart(string room, string entryPoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(room);
        _leavingFor = (room, entryPoint);
    }

    /// <summary>Stops the countdown where it stands, as when the escape is over.</summary>
    public void Stop() => _phase = Phase.Stopped;

    /// <inheritdoc/>
    public void BeginAgain()
    {
        _phase = Phase.Inactive;
        (_played, ShowsWhileHeld) = (false, false);
        _leavingFor = null;
        _leftZebes = false;
        Remaining = 0;
    }

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

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
    public GameMode? TakesOver(PlayingMode playing)
    {
        if (_leavingFor is { } landing)
        {
            _leavingFor = null;
            Stop();
            return new CeresDeparture(playing) { LandingRoom = landing.Room, LandingEntry = landing.EntryPoint };
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

        if (!IsExpired)
            return null;
        Stop();
        return new TimeUpMode(playing);
    }

    private void Count(long updates)
    {
        Remaining = Math.Max(0, Remaining - Cadence[(int)(updates % Cadence.Length)]);
        if (Remaining == 0)
            _phase = Phase.Expired;
    }
}
