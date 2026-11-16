using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Runs Ridley's body and breakup animations and applies their cues.</summary>
[RoomLifetime]
internal sealed class RidleyBodyAnimation
{
    /// <summary>Queues roars, tail impacts, and other enemy sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal RidleyBodyAnimation(EnemyAudioQueue audio, RidleyEncounter ridleyEncounter, RidleyProjectiles ridleyProjectiles)
    {
        _audio = audio;
        _ridleyEncounter = ridleyEncounter;
        _ridleyProjectiles = ridleyProjectiles;
    }

    /// <summary>
    /// Plays Ridley's body animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceRidleyBodyAnimation(RoomEnemyPlace place, SamusState samus)
    {
        if (!place.AnimationClock.Expire())
            return;
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(place);
        AnimationPlayback<RidleyBodySegment> playback = state.BodyPlayback;
        EnemyAnimationPlayback.Play(place, RidleyAnimations.Segments, ref playback, RidleyBodyFrameDefinitions.ShownFrames.Of,
            cue => ApplyRidleyBodyCue(place, state, samus, cue) is { } target
                ? new AnimationContinuation<RidleyBodySegment>(RidleyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.BodyPlayback = playback;
    }

    /// <summary>Carries out one cue of Ridley's body animation, returning the segment whose animation a continue-at cue continues in.</summary>
    internal RidleyBodySegment? ApplyRidleyBodyCue(RoomEnemyPlace place, RidleyEnemyState state, SamusState samus, RidleyBodyCue cue)
    {
        switch (cue)
        {
            case RidleyBodyCue.GoToUnlessFacingLeft goToUnlessFacingLeft:
                return state.FacingDirection != RidleyFacing.Left ? goToUnlessFacingLeft.Target : null;
            case RidleyBodyCue.GoToIfCeresAndSamusLowEnergy goToIfCeresAndSamusLowEnergy:
                if (place.DefinitionId == EnemyDefinitionId.Ridley)
                    return null;
                if (samus.Health >= 30)
                    return null;
                // The fireball animation stores 8 in the Ridley timer before it continues elsewhere,
                // cutting the fireball hover short.
                state.PhaseTimer = 8;
                return goToIfCeresAndSamusLowEnergy.Target;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.Roar }:
                state.Roaring = true;
                _audio.QueueEnemySound(RidleySoundDefinitions.Roar, priority: SoundRequestPriority.Normal);
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.ClearRoaringFlag }:
                state.Roaring = false;
                return null;
            case RidleyBodyCue.SetClawHeight setClawHeight:
                state.ClawHeight = setClawHeight.Height;
                return null;
            case RidleyBodyCue.Move move:
                place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + move.Displacement.X);
                place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + move.Displacement.Y);
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.FaceForward }:
                state.FacingDirection = RidleyFacing.Turning;
                return null;
            case RidleyBodyCue.Flip flip:
                RidleyTail.MirrorRidleyTail(state);
                state.FacingDirection = flip.Facing == HorizontalDirection.Left ? RidleyFacing.Left : RidleyFacing.Right;
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.AimFireball }:
                _ridleyProjectiles.CalculateRidleyFireballVelocity(place, samus);
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.SpawnFireballWithAfterburn }:
                _ridleyProjectiles.SpawnRidleyFireball(place, afterburns: 3);
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.SpawnFireballWithoutAfterburn }:
                _ridleyProjectiles.SpawnRidleyFireball(place, afterburns: 0);
                return null;
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.CeresStartLiftoff }:
            {
                RidleyEnemyState liftoff = _ridleyEncounter.RequireCeresRidley(place);
                liftoff.Phase = RidleyPhase.CeresLiftoffAccelerating;
                liftoff.Velocity = liftoff.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-352));
                return null;
            }
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.StartLiftoff }:
            {
                // The shared roar/liftoff animation hands control to the real fight at
                // the fight's start and supplies the initial upward 1/256-pixel velocity in the same tick.
                RidleyEnemyState norfairLiftoff = _ridleyEncounter.RequireNorfairRidley(place);
                norfairLiftoff.Phase = RidleyPhase.NorfairEnterArena;
                norfairLiftoff.Velocity = norfairLiftoff.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-352));
                return null;
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyBodyCueKind)} {cue}.");
        }
    }

    /// <summary>Runs Ceres and Lower Norfair Ridley's body as typed animations (#641).</summary>
    internal void AdvanceRidleyBodyTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        AdvanceRidleyBodyAnimation(place, context.Samus);

    /// <summary>
    /// Runs a breakup fragment's animation where the shared enemy animation runs: its pose shows for one update
    /// and then holds, raising the new-frame flag when shown.
    /// </summary>
    internal static void AdvanceRidleyBreakupAnimation(RoomEnemyPlace fragment)
    {
        if (!fragment.AnimationClock.Expire())
            return;
        RidleyExplosionFragmentState state = fragment.RequireSpeciesState<RidleyExplosionFragmentState>();
        AnimationPlayback<RidleyBreakupFrame> playback = state.Playback;
        EnemyAnimationPlayback.Play(fragment, RidleyBreakupAnimations.Segments, ref playback, RidleyBreakupFrameDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException("Ridley's breakup animations raise no events."));
        state.Playback = playback;
    }
}
