using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Runs both Ridleys' body animation and his breakup pieces' animations, and applies their cues (#2229).</summary>
[RoomLifetime]
internal sealed class RidleyBodyAnimation
{
    /// <summary>Queues roars, tail impacts, and other enemy sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal RidleyBodyAnimation(EnemyAudioQueue audio, RidleyProjectiles ridleyProjectiles)
    {
        _audio = audio;
        _ridleyProjectiles = ridleyProjectiles;
    }

    /// <summary>
    /// Plays Ridley's body animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and the clock notes each newly shown frame.
    /// </summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="owner">The encounter that owns the body, which handles the cues left to it (#2230).</param>
    internal void Advance(RoomEnemyPlace place, RidleyEncounterState ridley, SamusState samus, IRidleyBodyOwner owner)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<RidleyBodySegment> playback = ridley.Flight.BodyPlayback;
        EnemyAnimationPlayback.Play(place, RidleyAnimations.Segments, ref playback,
            cue => ApplyCue(place, ridley, samus, cue, owner) is { } target
                ? new AnimationContinuation<RidleyBodySegment>(RidleyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        ridley.Flight.BodyPlayback = playback;
    }

    /// <summary>What <c>ApplyCue</c> comes to for each <see cref="RidleyBodyCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<RidleyBodyCueKind, Func<RoomEnemyPlace, SamusState, IRidleyBodyOwner, RidleyFlightState, RidleyBodySegment?>> ApplyCueForRidleyBodyCueKind => field ??= new(new Dictionary<RidleyBodyCueKind, Func<RoomEnemyPlace, SamusState, IRidleyBodyOwner, RidleyFlightState, RidleyBodySegment?>>
    {
        [RidleyBodyCueKind.MayCutFireballingShort] = (place, samus, owner, flight) => owner.CutFireballingShort(samus),
        [RidleyBodyCueKind.Roar] = (place, samus, owner, flight) =>
        {
            flight.Roaring = true;
            _audio.QueueEnemySound(RidleySoundDefinitions.Roar, priority: SoundRequestPriority.Normal);
            return null;
        },
        [RidleyBodyCueKind.StopRoaring] = (place, samus, owner, flight) =>
        {
            flight.Roaring = false;
            return null;
        },
        [RidleyBodyCueKind.FaceForward] = (place, samus, owner, flight) =>
        {
            flight.FacingDirection = RidleyFacing.Turning;
            return null;
        },
        [RidleyBodyCueKind.AimFireball] = (place, samus, owner, flight) =>
        {
            RidleyProjectiles.CalculateRidleyFireballVelocity(place, flight, samus);
            return null;
        },
        [RidleyBodyCueKind.SpawnFireballWithAfterburn] = (place, samus, owner, flight) =>
        {
            _ridleyProjectiles.SpawnRidleyFireball(place, flight, afterburns: RidleyFireballDefinitions.AfterburnStages);
            return null;
        },
        [RidleyBodyCueKind.SpawnFireballWithoutAfterburn] = (place, samus, owner, flight) =>
        {
            _ridleyProjectiles.SpawnRidleyFireball(place, flight, afterburns: 0);
            return null;
        },
        [RidleyBodyCueKind.LiftOff] = (place, samus, owner, flight) =>
        {
            // The take-off hands control to the encounter's flight and starts his rise in the same update.
            owner.LiftOff();
            flight.Velocity = flight.Velocity.WithY(RidleyFlightDefinitions.LiftoffRise);
            return null;
        },
    });

    /// <summary>Carries out one cue of Ridley's body animation, returning the segment whose animation a continue-at cue continues in.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="cue">The cue.</param>
    /// <param name="owner">The encounter that owns the body, which handles the lift-off and the fireball cut (#2230).</param>
    private RidleyBodySegment? ApplyCue(RoomEnemyPlace place, RidleyEncounterState ridley, SamusState samus, RidleyBodyCue cue, IRidleyBodyOwner owner)
    {
        RidleyFlightState flight = ridley.Flight;
        return cue.Match(new RidleyBodyCueCases<RidleyBodySegment?>(
            Move: move =>
            {
                place.Position += move.Displacement;
                return null;
            },
            SetClawHeight: setClawHeight =>
            {
                flight.ClawHeight = setClawHeight.Height;
                return null;
            },
            Flip: flip =>
            {
                RidleyTail.MirrorRidleyTail(ridley.Tail);
                flight.FacingDirection = flip.Facing == HorizontalDirection.Left ? RidleyFacing.Left : RidleyFacing.Right;
                return null;
            },
            Signal: signal =>
            {
                return ApplyCueForRidleyBodyCueKind[signal.Kind](place, samus, owner, flight);
            }));
    }

    /// <summary>
    /// Shows a breakup fragment's picture where the shared enemy animation runs (#2235): a still picture, which the
    /// fragment shows on the update its clock first runs out, for
    /// <see cref="RidleyBreakupFrameDefinitions.PictureShowingUpdates"/>; it then rests on it, its clock stopped (#2454).
    /// </summary>
    internal static void AdvanceRidleyBreakupAnimation(RoomEnemyPlace fragment)
    {
        if (!fragment.AnimationClock.Expire())
            return;
        RidleyExplosionFragmentState state = fragment.RequireSpeciesState<RidleyExplosionFragmentState>();
        if (state.ShowsPicture)
        {
            fragment.AnimationClock.Stop();
            return;
        }
        fragment.Frame = state.Picture;
        fragment.AnimationClock.Show(RidleyBreakupFrameDefinitions.PictureShowingUpdates);
        state.ShowsPicture = true;
    }
}
