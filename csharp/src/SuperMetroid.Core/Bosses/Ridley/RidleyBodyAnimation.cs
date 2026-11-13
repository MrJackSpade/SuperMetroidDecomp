using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
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
    internal void AdvanceRidleyBodyAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!slot.AnimationClock.Expire())
            return;
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        AnimationPlayback<RidleyBodySegment> playback = state.BodyPlayback;
        // Ridley's body lists load no loop counter.
        EnemyAnimationPlayback.Play(slot, RidleyAnimations.Segments, ref playback, RidleyBodyFrameDefinitions.ShownFrames.Of,
            cue => ApplyRidleyBodyCue(slot, state, samus, cue) is { } target
                ? new AnimationContinuation<RidleyBodySegment>(RidleyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.BodyPlayback = playback;
    }

    /// <summary>Carries out one cue of Ridley's body animation, returning the segment whose animation a continue-at cue continues in.</summary>
    internal RidleyBodySegment? ApplyRidleyBodyCue(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, RidleyBodyCue cue)
    {
        switch (cue)
        {
            case RidleyBodyCue.GoToUnlessFacingLeft goToUnlessFacingLeft:
                return state.FacingDirection != RidleyFacing.Left ? goToUnlessFacingLeft.Target : null;
            case RidleyBodyCue.GoToIfCeresAndSamusLowEnergy goToIfCeresAndSamusLowEnergy:
                if (slot.DefinitionId == EnemyDefinitionId.Ridley)
                    return null;
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball branch requires the active Samus actor.");
                }
                if (samus.Health >= 30)
                    return null;
                // The fireball list stores 8 to the Ridley timer before it continues elsewhere,
                // cutting the fireball hover short.
                state.FunctionTimer = 8;
                return goToIfCeresAndSamusLowEnergy.Target;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.Roar }:
                state.Roaring = true;
                _audio.QueueEnemySound(RidleySoundDefinitions.Roar, priority: SoundRequestPriority.Normal);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.ClearRoaringFlag }:
                state.Roaring = false;
                return null;
            case RidleyBodyCue.SetFeetDistance setFeetDistance:
                state.FeetDistanceIndex = setFeetDistance.FeetDistanceIndex;
                return null;
            case RidleyBodyCue.Move move:
                slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + move.Displacement.X);
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + move.Displacement.Y);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.FaceForward }:
                state.FacingDirection = RidleyFacing.Turning;
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.FlipLeft }:
                RidleyTail.MirrorRidleyTail(state);
                state.FacingDirection = RidleyFacing.Left;
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.FlipRight }:
                RidleyTail.MirrorRidleyTail(state);
                state.FacingDirection = RidleyFacing.Right;
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.AimFireball }:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball aim requires the active Samus actor.");
                }
                _ridleyProjectiles.CalculateRidleyFireballVelocity(slot, samus);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.SpawnFireballWithAfterburn }:
                _ridleyProjectiles.SpawnRidleyFireball(slot, afterburns: 3);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.SpawnFireballWithoutAfterburn }:
                _ridleyProjectiles.SpawnRidleyFireball(slot, afterburns: 0);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.CeresStartLiftoff }:
            {
                RidleyEnemyState liftoff = _ridleyEncounter.RequireCeresRidley(slot);
                liftoff.Function = RidleyAiFunction.CeresLiftoffAccelerating;
                liftoff.Velocity = liftoff.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-352));
                return null;
            }
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.StartLiftoff }:
            {
                // The shared roar/liftoff list hands control to the real fight at
                // the fight's start and supplies the initial upward 8.8 velocity in the same tick.
                RidleyEnemyState norfairLiftoff = _ridleyEncounter.RequireNorfairRidley(slot);
                norfairLiftoff.Function = RidleyAiFunction.NorfairEnterArena;
                norfairLiftoff.Velocity = norfairLiftoff.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-352));
                return null;
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyBodyCueKind)} {cue}.");
        }
    }

    /// <summary>Runs Ceres and Lower Norfair Ridley's body as typed animations in place of their lists (#641).</summary>
    internal void AdvanceRidleyBodyTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context) =>
        AdvanceRidleyBodyAnimation(slot, context.Samus);

    /// <summary>
    /// Runs a breakup fragment's animation where the shared enemy animation runs: its pose shows for one update
    /// and then holds, raising the new-frame flag when shown.
    /// </summary>
    internal static void AdvanceRidleyBreakupAnimation(RoomEnemySlot fragment)
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
