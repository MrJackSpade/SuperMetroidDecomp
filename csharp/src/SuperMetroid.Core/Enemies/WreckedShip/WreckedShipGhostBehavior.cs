using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship ghost species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WreckedShipGhostBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Coven];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new WreckedShipGhostEntity(place, common);

    /// <summary>Initializes the Wrecked Ship ghost.</summary>
    internal static void InitializeWreckedShipGhost(RoomEnemyPlace place)
    {
        // The initializer sets the animating, intangibility and invisibility properties. Here those mean
        // animation processing, exclusion from Samus interaction, and invisibility.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        place.AnimationClock.Restart();

        var state = new WreckedShipGhostEnemyState(WreckedShipGhostAction.InitialInvisibleDelay)
        {
            Playback = new(WreckedShipGhostAnimations.StartingWith(WreckedShipGhostSegment.Floating), AnimationPlace.StartOfAnimation),
            PhaseTimer = WreckedShipGhostAppearanceDefinitions.InitialDelay,
        };

        // The ghost starts dark: its colours have settled to black long before the 280-update appearance delay
        // expires, so the first white ramp begins from black rather than from the ghost's own colours.
        state.Fade = WreckedShipGhostFade.Dark;
        ShowFade(place, state);
        place.SetSpeciesState(state);
    }

    /// <summary>Shows the ghost in the shade its fading colours are in (#1512).</summary>
    private static void ShowFade(RoomEnemyPlace place, WreckedShipGhostEnemyState state) =>
        place.Shading = PictureShading.Shaded(WreckedShipGhostFadeShade.Of(state.Fade));

    /// <summary>Runs the Wrecked Ship ghost's actions.</summary>
    private static void RunWreckedShipGhostMain(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case WreckedShipGhostAction.BrighteningAndFlickering:
                RunWreckedShipGhostBrightening(place, state);
                break;
            case WreckedShipGhostAction.FadingToGhostColours:
                RunWreckedShipGhostColourFade(place, state, samus);
                break;
            case WreckedShipGhostAction.WaitingForWhiteFade:
                RunWreckedShipGhostWhiteFade(place, state);
                break;
            case WreckedShipGhostAction.BobbingWhileVisible:
                RunWreckedShipGhostBobbing(place, state);
                break;
            case WreckedShipGhostAction.InitialInvisibleDelay:
                RunWreckedShipGhostInitialDelay(state);
                break;
            case WreckedShipGhostAction.TrackingSamusForSpawn:
                RunWreckedShipGhostTracking(place, state, samus);
                break;
            default:
                throw new InvalidDataException(
                    $"Wrecked Ship ghost has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Counts down the invisible delay. A zero timer also advances immediately, matching the game's
    /// branch rather than wrapping zero to -1.
    /// </summary>
    private static void RunWreckedShipGhostInitialDelay(WreckedShipGhostEnemyState state)
    {
        state.PhaseTimer = Math.Max(0, state.PhaseTimer - 1);
        if (state.PhaseTimer != 0)
            return;

        state.PhaseTimer = 1;
        state.NextFlickerStep = 0;
        state.Action = WreckedShipGhostAction.TrackingSamusForSpawn;
    }

    /// <summary>
    /// Tracks Samus for the spawn. The one-pixel upper bound is exclusive in retail; this asymmetric
    /// comparison is deliberately not replaced by an absolute-distance convenience test.
    /// </summary>
    private static void RunWreckedShipGhostTracking(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        Position16<RoomPixels> samusPosition = samus.Position.Whole;
        Offset16<RoomPixels> fromLower = samusPosition - state.SamusLowerBound;
        Offset16<RoomPixels> fromUpper = samusPosition - state.SamusUpperBound;

        bool withinPreviousBox =
            fromLower.X >= 0 &&
            fromUpper.X < 0 &&
            fromLower.Y >= 0 &&
            fromUpper.Y < 0;

        if (withinPreviousBox)
        {
            if (--state.StablePositionTimer == 0)
            {
                // Remaining inside the tiny box for 64 frames means "stationary" on both
                // axes and therefore selects the central spawn offset.
                state.SamusMotion = new(SamusAxisMotion.Still, SamusAxisMotion.Still);
                SpawnWreckedShipGhost(place, state, samusPosition);
                return;
            }
        }
        else
        {
            state.StablePositionTimer = WreckedShipGhostAppearanceDefinitions.StillPositionDuration;
            Offset16<RoomPixels> motion = samusPosition - state.PreviousSamusPosition;
            SamusAxisMotion horizontal = AxisMotion(motion.X);

            if (horizontal != state.SamusMotion.Horizontal)
            {
                state.SamusMotion = state.SamusMotion with { Horizontal = horizontal };
                state.StableDirectionTimer = WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration;
            }
            else
            {
                SamusAxisMotion vertical = AxisMotion(motion.Y);

                if (vertical != state.SamusMotion.Vertical)
                {
                    state.SamusMotion = state.SamusMotion with { Vertical = vertical };
                    state.StableDirectionTimer = WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration;
                }
                else
                {
                    if (--state.StableDirectionTimer == 0)
                    {
                        SpawnWreckedShipGhost(place, state, samusPosition);
                        return;
                    }
                }
            }
        }

        // The tracker publishes a fresh asymmetric [position-1, position+1) box at
        // the end of every non-spawning frame.
        state.PreviousSamusPosition = samusPosition;
        state.SamusLowerBound = samusPosition - WreckedShipGhostAppearanceDefinitions.StillMargin;
        state.SamusUpperBound = samusPosition + WreckedShipGhostAppearanceDefinitions.StillMargin;
    }

    /// <summary>How Samus moved along one axis by <paramref name="movedBy"/> pixels.</summary>
    private static SamusAxisMotion AxisMotion(int movedBy) => movedBy switch
    {
        < 0 => SamusAxisMotion.TowardSmaller,
        0 => SamusAxisMotion.Still,
        _ => SamusAxisMotion.TowardLarger,
    };

    /// <summary>Installs the direction-selected relative position and begins brightening.</summary>
    internal static void SpawnWreckedShipGhost(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        Position16<RoomPixels> samusPosition)
    {
        place.Position = place.Position.WithWhole(samusPosition + WreckedShipGhostAppearance.SpawnOffset(state.SamusMotion));
        state.Action = WreckedShipGhostAction.BrighteningAndFlickering;
        state.StablePositionTimer = WreckedShipGhostAppearanceDefinitions.StillPositionDuration;
        state.StableDirectionTimer = WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration;
    }

    /// <summary>Brightens the ghost toward its white flash, flickering as it brightens; once no colour changes, it fades to its own colours.</summary>
    internal static void RunWreckedShipGhostBrightening(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(place, state);

        // The brightening gates only on red and does not saturate the others: a full green or blue carries into the
        // next channel, as the fade value reproduces.
        WreckedShipGhostFadeStep step = state.Fade.Brightened();
        state.Fade = step.Next;
        int colorsChanged = step.Changes;
        ShowFade(place, state);
        if (colorsChanged != 0)
            return;

        state.Action = WreckedShipGhostAction.FadingToGhostColours;
        state.Fade = state.Fade.Materializing(WreckedShipGhostAppearance.Colours);
    }

    /// <summary>Fades to the ghost's own colours, with the simultaneous flicker and per-channel fade.</summary>
    private static void RunWreckedShipGhostColourFade(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostFadeTowardTarget(place, state);
        AdvanceWreckedShipGhostFlicker(place, state);
        if (state.PhaseTimer != 0 || componentsChanged != 0)
            return;

        // Clearing intangibility and invisibility makes the fully materialized ghost visible and eligible for the
        // ordinary touch and shot responses authored by its enemy definition.
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        state.Action = WreckedShipGhostAction.BobbingWhileVisible;
        state.BobbingOriginY = place.Position.Y.Whole;
        state.PhaseTimer = WreckedShipGhostAppearanceDefinitions.VisibleDuration;
        state.NextFlickerStep = 0;
        state.VerticalVelocity = WreckedShipGhostAppearanceDefinitions.BobStartSpeed;
        state.SamusMotion = new(SamusAxisMotion.Still, SamusAxisMotion.Still);
        state.PreviousSamusPosition = samus.Position.Whole;
        state.SamusLowerBound = samus.Position.Whole;
        state.SamusUpperBound = samus.Position.Whole;
        state.StablePositionTimer = WreckedShipGhostAppearanceDefinitions.StillPositionDuration;
        state.StableDirectionTimer = WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration;
    }

    /// <summary>Runs the signed 1/65536-pixel oscillation and 120-frame visible lifetime.</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        place.Position = place.Position.AdvanceY(state.VerticalVelocity);

        SignedSixteenSixteen acceleration = CoordinateAxis.Precedes(place.Position.Y.Whole, state.BobbingOriginY)
            ? WreckedShipGhostAppearanceDefinitions.BobAcceleration
            : -WreckedShipGhostAppearanceDefinitions.BobAcceleration;
        state.VerticalVelocity += acceleration;

        if (--state.PhaseTimer != 0)
            return;

        state.Action = WreckedShipGhostAction.WaitingForWhiteFade;
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        state.Fade = state.Fade.Vanishing();
    }

    /// <summary>Finishes whitening, hides, and restarts the 120-frame delay.</summary>
    private static void RunWreckedShipGhostWhiteFade(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostFadeTowardTarget(place, state) != 0)
            return;

        state.Action = WreckedShipGhostAction.InitialInvisibleDelay;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.PhaseTimer = WreckedShipGhostAppearanceDefinitions.VisibleDuration;
    }

    /// <summary>
    /// Advances the flicker: when the current step's updates run out, the next step starts and may show the ghost; past the
    /// last step the flicker ends and every later call keeps the ghost visible for this cycle.
    /// </summary>
    internal static void AdvanceWreckedShipGhostFlicker(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        if (state.PhaseTimer != 0)
        {
            if (--state.PhaseTimer != 0)
                return;

            IReadOnlyList<WreckedShipGhostFlickerStep> steps = WreckedShipGhostAppearanceDefinitions.FlickerSteps;
            if (state.NextFlickerStep == steps.Count)
                return;

            WreckedShipGhostFlickerStep step = steps[state.NextFlickerStep];
            state.PhaseTimer = step.Duration;
            state.NextFlickerStep++;
            if (step.Visibility == WreckedShipGhostFlickerVisibility.Unchanged)
                return;
        }

        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
    }

    /// <summary>
    /// Steps the ghost's fade: every colour channel moves exactly one level toward its target per frame; the return value
    /// counts channel changes, not changed colours.
    /// </summary>
    internal static int StepWreckedShipGhostFadeTowardTarget(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        WreckedShipGhostFadeStep step = state.Fade.SteppedTowardTarget();
        state.Fade = step.Next;
        int componentsChanged = step.Changes;
        ShowFade(place, state);
        return componentsChanged;
    }

    /// <summary>Advances the Wrecked Ship ghost's typed animation.</summary>
    internal static void AdvanceWreckedShipGhostTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, WreckedShipGhostAnimations.Segments,
            static (WreckedShipGhostEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Wrecked Ship ghost's cue-free animation raised {cue}."));

    /// <summary>Runs the Wrecked Ship ghost's main AI.</summary>
    internal static void RunWreckedShipGhostMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWreckedShipGhostMain(place, place.RequireSpeciesState<WreckedShipGhostEnemyState>(), frame.Samus);
    }
}
