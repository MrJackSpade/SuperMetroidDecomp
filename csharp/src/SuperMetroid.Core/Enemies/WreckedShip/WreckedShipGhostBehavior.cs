using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship ghost species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class WreckedShipGhostBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Coven];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new WreckedShipGhostEntity(occupant, place, common));

    /// <summary>Initializes the Wrecked Ship ghost.</summary>
    internal static void InitializeWreckedShipGhost(RoomEnemyPlace place)
    {
        // The ghost starts animating, unseen and untouchable.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        place.AnimationClock.Restart();

        var state = new WreckedShipGhostEnemyState(WreckedShipGhostAction.InitialInvisibleDelay)
        {
            Playback = new(WreckedShipGhostAnimations.StartingWith(WreckedShipGhostSegment.Floating), AnimationPlace.StartOfAnimation),
            InvisibleDelay = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.InitialDelay),
        };

        // The ghost starts dark: its colours have settled to black long before its first wait
        // (WreckedShipGhostAppearanceDefinitions.InitialDelay) expires, so the first white ramp begins from black rather than from the ghost's own colours.
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
    /// Counts down the invisible delay (<see cref="WreckedShipGhostAppearanceDefinitions.InitialDelay"/> when the ghost is
    /// placed, <see cref="WreckedShipGhostAppearanceDefinitions.ReappearanceDelay"/> after each vanishing); on the update
    /// it runs out the ghost starts tracking Samus.
    /// </summary>
    private static void RunWreckedShipGhostInitialDelay(WreckedShipGhostEnemyState state)
    {
        if (!state.InvisibleDelay.StepEnds())
            return;

        state.Action = WreckedShipGhostAction.TrackingSamusForSpawn;
    }

    /// <summary>
    /// Tracks Samus for the spawn: she stays still while she lies within the box around where she was, from its lower
    /// corner up to, but not including, its upper corner.
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
            if (state.StablePositionTimer.StepEnds())
            {
                // Remaining inside the tiny box for its still duration means "stationary" on both
                // axes and therefore selects the central spawn offset.
                state.SamusMotion = new(SamusAxisMotion.Still, SamusAxisMotion.Still);
                SpawnWreckedShipGhost(place, state, samusPosition);
                return;
            }
        }
        else
        {
            state.StablePositionTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.StillPositionDuration);
            Offset16<RoomPixels> motion = samusPosition - state.PreviousSamusPosition;
            SamusAxisMotion horizontal = AxisMotion(motion.X);

            if (horizontal != state.SamusMotion.Horizontal)
            {
                state.SamusMotion = state.SamusMotion with { Horizontal = horizontal };
                state.StableDirectionTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration);
            }
            else
            {
                SamusAxisMotion vertical = AxisMotion(motion.Y);

                if (vertical != state.SamusMotion.Vertical)
                {
                    state.SamusMotion = state.SamusMotion with { Vertical = vertical };
                    state.StableDirectionTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration);
                }
                else
                {
                    if (state.StableDirectionTimer.StepEnds())
                    {
                        SpawnWreckedShipGhost(place, state, samusPosition);
                        return;
                    }
                }
            }
        }

        // The tracker publishes a fresh box from one pixel before the position up to, not including, one pixel after it at
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

    /// <summary>
    /// Installs the direction-selected relative position and begins brightening, its flicker's first step to start on the
    /// first update it brightens.
    /// </summary>
    internal static void SpawnWreckedShipGhost(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        Position16<RoomPixels> samusPosition)
    {
        place.Position = place.Position.WithWhole(samusPosition + WreckedShipGhostAppearance.SpawnOffset(state.SamusMotion));
        state.Action = WreckedShipGhostAction.BrighteningAndFlickering;
        state.FlickerTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.FlickerFirstStepDelay);
        state.NextFlickerStep = WreckedShipGhostFlickerStep.First;
        state.StablePositionTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.StillPositionDuration);
        state.StableDirectionTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration);
    }

    /// <summary>Brightens the ghost toward its white flash, flickering as it brightens; once no colour changes, it fades to its own colours.</summary>
    internal static void RunWreckedShipGhostBrightening(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(place, state);

        // The ghost brightens as WreckedShipGhostFade reproduces it.
        WreckedShipGhostFadeStep step = state.Fade.Brightened();
        state.Fade = step.Next;
        int colorsChanged = step.Changes;
        ShowFade(place, state);
        if (colorsChanged != 0)
            return;

        state.Action = WreckedShipGhostAction.FadingToGhostColours;
        state.Fade = state.Fade.Materializing(WreckedShipGhostFade.GhostColours);
    }

    /// <summary>Fades to the ghost's own colours, with the simultaneous flicker and per-channel fade.</summary>
    private static void RunWreckedShipGhostColourFade(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostFadeTowardTarget(place, state);
        AdvanceWreckedShipGhostFlicker(place, state);
        if (state.IsFlickering || componentsChanged != 0)
            return;

        // Clearing intangibility and invisibility makes the fully materialized ghost visible and eligible for the
        // ordinary touch and shot responses authored by its enemy definition.
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        state.Action = WreckedShipGhostAction.BobbingWhileVisible;
        state.BobbingOriginY = place.Position.Y.Whole;
        state.VisibleTimer = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.VisibleDuration);
        state.VerticalVelocity = WreckedShipGhostAppearanceDefinitions.BobStartSpeed;
        state.SamusMotion = new(SamusAxisMotion.Still, SamusAxisMotion.Still);
        state.PreviousSamusPosition = samus.Position.Whole;
        state.SamusLowerBound = samus.Position.Whole;
        state.SamusUpperBound = samus.Position.Whole;
    }

    /// <summary>Bobs the ghost up and down while it is visible (<see cref="WreckedShipGhostAppearanceDefinitions.VisibleDuration"/>).</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        place.Position = place.Position.AdvanceY(state.VerticalVelocity);

        // The ghost appears above a Samus who has been rising, by its spawn offset, and Samus can rise nearly to the top of
        // the Wrecked Ship's main shaft and save room, so a ghost can bob a few pixels either side of the room's
        // top edge; it turns back toward where it appeared the short way, as DistanceAcrossRoomEdge.LiesBefore reads it.
        SignedSixteenSixteen acceleration = WreckedShipGhostAppearanceDefinitions.BobAcceleration.Toward(
            DistanceAcrossRoomEdge.LiesBefore(place.Position.Y.Whole, state.BobbingOriginY) ? VerticalDirection.Down : VerticalDirection.Up);
        state.VerticalVelocity += acceleration;

        if (!state.VisibleTimer.StepEnds())
            return;

        state.Action = WreckedShipGhostAction.WaitingForWhiteFade;
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        state.Fade = state.Fade.Vanishing();
    }

    /// <summary>Finishes whitening, hides, and starts its wait to reappear (<see cref="WreckedShipGhostAppearanceDefinitions.ReappearanceDelay"/>) again.</summary>
    private static void RunWreckedShipGhostWhiteFade(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostFadeTowardTarget(place, state) != 0)
            return;

        state.Action = WreckedShipGhostAction.InitialInvisibleDelay;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.InvisibleDelay = UpdateCountdown.For(WreckedShipGhostAppearanceDefinitions.ReappearanceDelay);
    }

    /// <summary>
    /// Advances the flicker: when the current step's updates run out, the next step starts and may show the ghost; past the
    /// last step the flicker ends and every later update keeps the ghost visible for this cycle.
    /// </summary>
    internal static void AdvanceWreckedShipGhostFlicker(
        RoomEnemyPlace place,
        WreckedShipGhostEnemyState state)
    {
        if (state.IsFlickering)
        {
            if (!state.FlickerTimer.StepEnds())
                return;

            if (state.NextFlickerStep is not { } step)
                return;

            state.FlickerTimer = UpdateCountdown.For(step.Duration);
            state.NextFlickerStep = step.Next;
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
    internal static void AdvanceWreckedShipGhostTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, WreckedShipGhostAnimations.Segments,
            static (WreckedShipGhostEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Wrecked Ship ghost's cue-free animation raised {cue}."));

    /// <summary>Runs the Wrecked Ship ghost's main AI.</summary>
    internal static void RunWreckedShipGhostMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWreckedShipGhostMain(place, place.RequireSpeciesState<WreckedShipGhostEnemyState>(), frame.Samus);
    }
}
