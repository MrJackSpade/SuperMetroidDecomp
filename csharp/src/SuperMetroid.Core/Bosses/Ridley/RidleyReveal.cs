using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The reveal both Ridleys open with (#2227, #2229): Ceres Ridley as he appears in the space station and Lower Norfair
/// Ridley as he appears in his lair. He waits for Samus's door and his encounter's delay, his eyes fade in from the dark,
/// then his body one shade at a time; the final body shade ends his ignoring Samus's touch and starts the Ridley and
/// Draygon battle music; he roars, holds as his encounter decides, and takes off. His lift-off ends the reveal.
/// </summary>
[RoomLifetime]
internal sealed class RidleyReveal
{
    /// <summary>Samus's passage through a door, which holds his opening wait.</summary>
    private readonly DoorPassage _doorPassage;
    /// <summary>Receives the battle music the finished body fade starts.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the reveal from the collaborators it uses.</summary>
    internal RidleyReveal(DoorPassage doorPassage, EnemyAudioQueue audio)
    {
        _doorPassage = doorPassage;
        _audio = audio;
    }

    /// <summary>Runs the phase of the reveal Ridley is in, before he lifts off.</summary>
    /// <typeparam name="TState">The encounter's state.</typeparam>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="encounter">What the encounter decides in the reveal.</param>
    internal void Run<TState>(RoomEnemyPlace place, TState ridley, IRidleyRevealEncounter<TState> encounter)
        where TState : RidleyEncounterState
    {
        switch (ridley.RevealPhase)
        {
            case RidleyRevealPhase.WaitForDoorTransition:
                // The wait holds even though enemy animations run during the fade.
                if (_doorPassage.IsUnderWay)
                    return;
                ridley.RevealPhase = RidleyRevealPhase.InitialDelay;
                ridley.PhaseTimer = encounter.InitialDelay;
                TickInitialDelay(ridley);
                return;

            case RidleyRevealPhase.InitialDelay:
                TickInitialDelay(ridley);
                return;

            case RidleyRevealPhase.FadeInEyes:
                TickEyeFadeIn(ridley);
                return;

            case RidleyRevealPhase.FadeInBody:
                TickBodyFadeIn(place, ridley);
                return;

            case RidleyRevealPhase.WaitBeforeRoar:
                if (--ridley.PhaseTimer < 0)
                {
                    RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.OpeningRoar);
                    ridley.PhaseTimer = encounter.RoarHold;
                    ridley.RevealPhase = RidleyRevealPhase.WaitBeforeLiftoff;
                }
                return;

            case RidleyRevealPhase.WaitBeforeLiftoff:
                if (encounter.TakesOff(place, ridley))
                    TakeOff(place, ridley);
                return;

            case RidleyRevealPhase.TakingOff:
                ridley.Flight.Velocity = EightEightVector<RoomPixels>.Zero;
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(RidleyRevealPhase)} {ridley.RevealPhase}.");
        }
    }

    /// <summary>Advances the opening hold before Ridley's eye fade begins.</summary>
    private static void TickInitialDelay(RidleyEncounterState ridley)
    {
        if (--ridley.PhaseTimer >= 0)
            return;

        ridley.RevealPhase = RidleyRevealPhase.FadeInEyes;
        ridley.PhaseTimer = 0;
    }

    /// <summary>
    /// Advances Ridley's eye fade an update on each AI call: its sixteen shades, then its hold; once it has held, his body
    /// fade starts.
    /// </summary>
    internal static void TickEyeFadeIn(RidleyEncounterState ridley)
    {
        // The game's increment-and-test advances once per AI call for every reachable
        // counter value, resetting that timer to zero immediately afterward.
        if (++ridley.PhaseTimer == 0)
            return;
        ridley.PhaseTimer = 0;

        if (ridley.EyeFade.IsComplete)
        {
            ridley.RevealPhase = RidleyRevealPhase.FadeInBody;
            ridley.MovementAnimationEnabled = true;
            return;
        }

        // Each of the fade's first updates shows its next, lighter shade; its hold keeps the last, his own eye colours.
        if (ridley.EyeFade.NextShade is { } shade)
            ridley.Colours.ShowEyeFade(shade);
        ridley.EyeFade = ridley.EyeFade.Next();
    }

    /// <summary>
    /// Reveals Ridley's body one shade every second update; after the final shade he stops ignoring Samus, waits before his
    /// opening roar, and the battle music starts.
    /// </summary>
    internal void TickBodyFadeIn(RoomEnemyPlace place, RidleyEncounterState ridley)
    {
        if (++ridley.PhaseTimer < 2)
            return;
        ridley.PhaseTimer = 0;

        // Each step of the body fade shows the next shade, from black to the full body colours.
        if (ridley.BodyFade.IsComplete)
            throw new InvalidOperationException($"Ridley body-fade shade {ridley.BodyFade.Shade} is outside the installed shades.");
        ridley.Colours.ShowBodyFade(ridley.BodyFade.Shade);
        ridley.BodyFade = ridley.BodyFade.Next();
        if (!ridley.BodyFade.IsComplete)
            return;

        // After the final shade Ridley stops ignoring Samus, so the ordinary touch check meets his body again; he waits
        // five updates, then begins the opening-roar animation.
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        ridley.RevealPhase = RidleyRevealPhase.WaitBeforeRoar;
        ridley.PhaseTimer = 4;
        // The fade queues the battle music immediately after the final body shade. It
        // intentionally continues after Ridley retreats, so omitting this single publication
        // silences both scenes.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
    }

    /// <summary>
    /// Starts Ridley's take-off: his take-off animation, whose lift-off ends the reveal, with his wings primed, every tail
    /// link enabled and his tail in its neutral swing.
    /// </summary>
    private static void TakeOff(RoomEnemyPlace place, RidleyEncounterState ridley)
    {
        RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.TransitionToFlying);
        ridley.Flight.WingAnimationTimer = 8;
        ridley.Flight.WingAnimationTimerDelta = 8;
        foreach (RidleyTailLink link in ridley.Tail.Links)
            link.Active = true;
        ridley.Tail.Action = RidleyTailAction.Neutral;
        ridley.RevealPhase = RidleyRevealPhase.TakingOff;
    }
}
