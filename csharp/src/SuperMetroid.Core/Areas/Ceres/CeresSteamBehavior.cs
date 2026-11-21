using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Ceres's steam vents: each puffs its plume after its own random wait, and the plume touches Samus by the rectangle its
/// frame shows. A room places its vents through <see cref="CeresSteamFactory"/>, with the motions its scenery gives them:
/// only the turning elevator shaft's let a vent ride the shaft's turn (#2480).
/// </summary>
/// <remarks>
/// The steam cannot be destroyed (#2019): its definition takes no damage from any weapon or contact
/// (<see cref="EnemyVulnerabilityDefinitions.Invulnerable"/>) and withholds its plume from shots, so its health never
/// leaves the definition's.
/// </remarks>
/// <param name="room">The room's services, whose random numbers stagger the vents.</param>
/// <param name="collision">The common contact resolution.</param>
[RoomLifetime]
internal sealed class CeresSteamBehavior(EnemyRoomBinding room, EnemyCollision collision)
{
    /// <summary>Room services that supply the live RNG used to stagger vent activation.</summary>
    private readonly EnemyRoomBinding _room = room;
    /// <summary>Common contact resolution used after steam's extended hitbox selects its touch reaction.</summary>
    private readonly EnemyCollision _collision = collision;

    /// <summary>
    /// Plays a vent's typed animation in place of the shared enemy animation. Only a dormant vent's animation holds: it
    /// hides the vent on its first update and rests there. The vent stays hidden for its drawn wait and then goes on
    /// where a rest ends (<see cref="CeresSteamAnimations.ActivationAfter"/>, #2480), appearing and showing its puff's
    /// first picture on the wait's last update.
    /// </summary>
    internal static void AdvanceCeresSteamAnimation(RoomEnemyPlace place, CeresSteamEnemyState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<CeresSteamSegment> playback = state.Playback;
        EnemyAnimationPlayback.PlayApplyingCues(place, CeresSteamAnimations.Segments, ref playback, (CeresSteamCueKind cue) => CarryOut(place, cue));
        if (place.AnimationClock.IsStopped)
        {
            place.AnimationClock.HoldFor(state.UpdatesToActivation);
            playback = CeresSteamAnimations.ActivationAfter[playback.Animation.Phases[playback.Place.Phase].Segment];
        }
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of a vent's animation; the play runs on after each.</summary>
    private static void CarryOut(RoomEnemyPlace place, CeresSteamCueKind cue)
    {
        switch (cue)
        {
            case CeresSteamCueKind.Vanish:
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                return;
            case CeresSteamCueKind.Appear:
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return;
            default:
                throw new InvalidOperationException($"Undefined Ceres steam cue {cue}.");
        }
    }

    /// <summary>Initializes the Ceres steam, drawn where the motion <paramref name="motions"/> give its variant puts it.</summary>
    /// <exception cref="InvalidDataException">The room cannot place a vent of the variant's action.</exception>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal CeresSteamEnemyState InitializeCeresSteam(CeresSteamSpawnSettings ceresSteamSpawnSettings, RoomEnemyPlace place, CeresSteamVentMotions motions)
    {
        CeresSteamInitialization initialization = CeresSteamInitializations.Initialization(
            ceresSteamSpawnSettings.Variant);

        // The vent animates, which shows and hides its plume, and collides by its frames' plume rectangles, which no
        // radius can describe.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.CollidesByFrameHitboxes = true;
        place.AnimationClock.Restart();

        // Each vent waits a random number of updates, at least one (CeresSteamDefinitions.ActivationDelay), before its
        // first plume, hidden by its dormant animation, so the vents puff out of step.
        var state = new CeresSteamEnemyState(Ensure.NotNull(motions).MotionOf(initialization.Action))
        {
            UpdatesToActivation = _room.Random.Next(CeresSteamDefinitions.ActivationDelay) + 1,
            Playback = new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation),
        };
        return state;
    }

    /// <summary>Runs the Ceres steam's main AI: the vent's drawing follows its motion.</summary>
    internal static void RunCeresSteamMainAi(RoomEnemyPlace place, CeresSteamEnemyState ceresSteamEnemyState) =>
        ceresSteamEnemyState.Motion.Move(place);

    /// <summary>Handles contact only for the steam's authored extended hitbox reaction.</summary>
    internal void ReactToCeresSteamTouch(EnemyTouchContext context)
    {
        // Every visible steam frame stores the steam's touch reaction in its authored hitbox.
        // The last two dispersal frames have no plume and touch nothing. Steam touches Samus only where a frame's
        // authored hitbox carries the touch reaction.
        if (!context.UsesFrameHitboxes || context.HitboxTouch != CeresSteamReactions.Touch)
        {
            throw new InvalidDataException(
                $"Ceres steam touches through its frames' hitboxes with {CeresSteamReactions.Touch}; " +
                $"frame hitboxes={context.UsesFrameHitboxes}, touch={context.HitboxTouch}, " +
                $"frame={context.Place.Frame}.");
        }

        // Definition damage is zero: contact still installs the common invincibility and knockback, while the steam's
        // definition takes no damage from any weapon or contact, so the Screw Attack or the Speed Booster cannot destroy it.
        _collision.ResolveOrdinaryTouch(context);
    }

    /// <summary>The reaction of the plume rectangle the frame shown has, when the probe overlaps it.</summary>
    /// <exception cref="InvalidDataException">The frame shown is not one of the steam's frames.</exception>
    internal static EnemyHitboxContact FindHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        // Only the frame's plume rectangle damages Samus, whatever its installed picture shows; a hidden plume has none.
        if (enemy.Frame is not { } frame || !CeresSteamCollisionDefinitions.Plumes.TryGetValue(frame, out EnemyHitboxRectangle? plume))
            throw new InvalidDataException($"Ceres steam frame {enemy.Frame} has no plume collision.");
        if (plume is not { } rectangle)
            return EnemyHitboxContact.Missed;

        // The probe compares the edges by their signed distances, so an edge past the room's edge needs no wrapping.
        Position16<RoomPixels> vent = enemy.Position.Whole;
        if (!probe.Overlaps(vent.X + rectangle.Left, vent.Y + rectangle.Top, vent.X + rectangle.Right, vent.Y + rectangle.Bottom))
            return EnemyHitboxContact.Missed;
        return EnemyHitboxContact.Struck(probe.SelectsShot
            ? CeresSteamCollisionDefinitions.ShotReaction
            : CeresSteamCollisionDefinitions.TouchReaction);
    }
}
