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
    /// <param name="place">Ridley's body.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="owner">The encounter that owns the body, which handles the cues left to it (#2230).</param>
    internal void AdvanceRidleyBodyAnimation(RoomEnemyPlace place, SamusState samus, IRidleyBodyOwner owner)
    {
        if (!place.AnimationClock.Expire())
            return;
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(place);
        AnimationPlayback<RidleyBodySegment> playback = state.BodyPlayback;
        EnemyAnimationPlayback.Play(place, RidleyAnimations.Segments, ref playback, RidleyBodyFrameDefinitions.ShownFrames.Of,
            cue => ApplyRidleyBodyCue(place, state, samus, cue, owner) is { } target
                ? new AnimationContinuation<RidleyBodySegment>(RidleyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.BodyPlayback = playback;
    }

    /// <summary>The speed Ridley starts rising at as he lifts off: 352/256 of a pixel an update.</summary>
    private static readonly SignedEightEight LiftoffRise = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-352);

    /// <summary>Carries out one cue of Ridley's body animation, returning the segment whose animation a continue-at cue continues in.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="cue">The cue.</param>
    /// <param name="owner">The encounter that owns the body, which handles the lift-off and the fireball cut (#2230).</param>
    internal RidleyBodySegment? ApplyRidleyBodyCue(RoomEnemyPlace place, RidleyEnemyState state, SamusState samus, RidleyBodyCue cue, IRidleyBodyOwner owner)
    {
        switch (cue)
        {
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.MayCutFireballingShort }:
                return owner.CutFireballingShort(samus);
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
            case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.LiftOff }:
                // The take-off hands control to the encounter's flight and starts his rise in the same update.
                owner.LiftOff();
                state.Velocity = state.Velocity.WithY(LiftoffRise);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyBodyCueKind)} {cue}.");
        }
    }

    /// <summary>Runs Ceres and Lower Norfair Ridley's body as typed animations (#641).</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="context">The animation pass.</param>
    /// <param name="owner">The encounter that owns the body (#2230).</param>
    internal void AdvanceRidleyBodyTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context, IRidleyBodyOwner owner) =>
        AdvanceRidleyBodyAnimation(place, context.Samus, owner);

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
