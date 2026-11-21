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
internal sealed partial class RidleyReveal
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
                ridley.PhaseTimer.LastFor(encounter.InitialDelayUpdates);
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
                if (ridley.PhaseTimer.StepRunsOut())
                {
                    RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.OpeningRoar);
                    encounter.BeginRoarHold(ridley);
                    ridley.RevealPhase = RidleyRevealPhase.WaitBeforeLiftoff;
                }
                return;

            case RidleyRevealPhase.WaitBeforeLiftoff:
                if (encounter.TakesOff(place, ridley))
                    TakeOff(place, ridley);
                return;

            case RidleyRevealPhase.TakingOff:
                ridley.Flight.Velocity = PixelVelocity<RoomPixels>.Zero;
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(RidleyRevealPhase)} {ridley.RevealPhase}.");
        }
    }

    /// <summary>Advances the opening hold before Ridley's eye fade begins.</summary>
    private static void TickInitialDelay(RidleyEncounterState ridley)
    {
        if (ridley.PhaseTimer.StepKeepsRunning())
            return;

        ridley.RevealPhase = RidleyRevealPhase.FadeInEyes;
    }

    /// <summary>
    /// Plays Ridley's eye fade an update on each AI update (#2409): its shades, then its hold; on the update it rests,
    /// his body fade starts.
    /// </summary>
    internal static void TickEyeFadeIn(RidleyEncounterState ridley)
    {
        // Each of the fade's first updates shows its next, lighter shade; its hold keeps the last, his own eye colours.
        if (ridley.EyeFade.StepToNewFrame() is { } image)
            ridley.Colours.ShowEyeFade(image);
        if (!ridley.EyeFade.IsResting)
            return;

        ridley.RevealPhase = RidleyRevealPhase.FadeInBody;
        ridley.Motion = RidleyMotion.Running;
    }

    /// <summary>
    /// Plays Ridley's body fade an update on each AI update (#2409), each shade for its hold from black to his own
    /// colours; as the final shade shows he stops ignoring Samus, waits before his opening roar, and the battle music starts.
    /// </summary>
    internal void TickBodyFadeIn(RoomEnemyPlace place, RidleyEncounterState ridley)
    {
        if (ridley.BodyFade.StepToNewFrame() is { } image)
            ridley.Colours.ShowBodyFade(image);
        if (!ridley.BodyFade.IsResting)
            return;

        // After the final shade Ridley stops ignoring Samus, so the ordinary touch check meets his body again; he waits,
        // then begins the opening-roar animation.
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        ridley.RevealPhase = RidleyRevealPhase.WaitBeforeRoar;
        ridley.PhaseTimer.LastFor(RidleyShadeDefinitions.BeforeRoarUpdates);
        // The battle music starts right after the final body shade and plays on after Ridley retreats.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
    }

    /// <summary>
    /// Starts Ridley's take-off: his take-off animation, whose lift-off ends the reveal, with his wings primed, every tail
    /// link enabled and his tail in its neutral swing.
    /// </summary>
    private static void TakeOff(RoomEnemyPlace place, RidleyEncounterState ridley)
    {
        RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.TransitionToFlying);
        ridley.Flight.Wings.Countdown.Restart(RidleyWingDefinitions.TakeOffCountdown);
        foreach (RidleyTailLink link in ridley.Tail.Links)
            link.Activate();
        ridley.Tail.Action = RidleyTailAction.Neutral;
        ridley.RevealPhase = RidleyRevealPhase.TakingOff;
    }
}
