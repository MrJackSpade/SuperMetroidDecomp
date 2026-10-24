using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Runs Ridley's body and breakup programs and applies their cues.</summary>
[RoomLifetime]
internal sealed class RidleyBodyAnimation
{
    /// <summary>Queues roars, tail impacts, and other enemy-bank sounds.</summary>
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
    /// Runs Ridley's body program where the enemy instruction interpreter would: the frame expires on the update its
    /// native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceRidleyBodyAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        AnimationProgramCursor<RidleyBodyProgram> cursor = state.BodyProgram;
        // Ridley's body lists load no loop counter.
        EnemyProgramAnimation.Run(slot, RidleyAnimations.Programs, ref cursor, RidleyBodyFrameDefinitions.ShownFrames.Of,
            (cue, continuation) => ApplyRidleyBodyCue(slot, state, samus, cue));
        state.BodyProgram = cursor;
    }

    /// <summary>Carries out one cue of Ridley's body program, returning the program a goto cue continues at.</summary>
    internal RidleyBodyProgram? ApplyRidleyBodyCue(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, RidleyBodyCue cue)
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
                // $A6:E4E2 stores 8 to the Ridley timer at $7E:7800 before the goto,
                // cutting the fireball hover short.
                state.FunctionTimer = WrappingCountdown.Start(8);
                return goToIfCeresAndSamusLowEnergy.Target;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.Roar }:
                state.Roaring = true;
                _audio.QueueEnemySound(RidleySoundDefinitions.Roar, maximumQueued: 6);
                return null;
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.ClearRoaringFlag }:
                state.Roaring = false;
                return null;
            case RidleyBodyCue.SetFeetDistance setFeetDistance:
                state.FeetDistanceIndex = setFeetDistance.FeetDistanceIndex;
                return null;
            case RidleyBodyCue.Move move:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + move.Displacement.X)));
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + move.Displacement.Y)));
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
                liftoff.Velocity = liftoff.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-352));
                return null;
            }
            case RidleyBodyCue.Command { Kind: RidleyBodyCueKind.StartLiftoff }:
            {
                // The shared roar/liftoff list hands control to the real fight at
                // $B2F3 and supplies the initial upward 8.8 velocity in the same tick.
                RidleyEnemyState norfairLiftoff = _ridleyEncounter.RequireNorfairRidley(slot);
                norfairLiftoff.Function = RidleyAiFunction.NorfairEnterArena;
                norfairLiftoff.Velocity = norfairLiftoff.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-352));
                return null;
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyBodyCueKind)} {cue}.");
        }
    }

    /// <summary>Runs Ceres and Lower Norfair Ridley's body as typed programs in place of their bank-$A6 lists (#641).</summary>
    internal void AdvanceRidleyBodyTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        AdvanceRidleyBodyAnimation(slot, context.Samus);

    /// <summary>
    /// Runs a breakup fragment's program where the enemy instruction interpreter would: its pose shows for one update
    /// and then holds, raising the new-frame flag when shown.
    /// </summary>
    internal void AdvanceRidleyBreakupAnimation(RoomEnemySlot fragment)
    {
        if (!EnemyProgramAnimation.Expire(fragment))
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            $"Ridley breakup fragment in slot {fragment.SlotIndex} requires Ridley's encounter state.");
        if (!state.BreakupPrograms.TryGetValue(fragment.SlotIndex, out AnimationProgramCursor<RidleyBreakupFrame> cursor))
            throw new InvalidOperationException($"Ridley breakup fragment in slot {fragment.SlotIndex} has no program.");
        EnemyProgramAnimation.Run(fragment, RidleyBreakupAnimations.Programs, ref cursor, RidleyBreakupFrameDefinitions.ShownFrames.Of,
            (cue, continuation) => throw new InvalidOperationException("Ridley's breakup programs publish no cues."));
        state.BreakupPrograms[fragment.SlotIndex] = cursor;
    }
}
