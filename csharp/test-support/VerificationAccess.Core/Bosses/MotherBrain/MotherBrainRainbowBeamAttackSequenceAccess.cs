using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;

/// <summary>Verification access to <see cref="MotherBrainRainbowBeamAttackSequence"/> members production does not use.</summary>
internal static class MotherBrainRainbowBeamAttackSequenceAccess
{
    /// <summary>Exposes corpse processing and focused finish-off, damage, neck, and draw-timing hooks on the live sequence.</summary>
    extension(MotherBrainRainbowBeamAttackSequence self)
    {

        /// <summary>
        /// Starts at the low-health handoff in `$A9:BB1A`, including its immediate really-slow
        /// forward-walk request. This is the exact debugger entry point reached after the `$BB06`
        /// decision timer; it does not skip the later `$BD45` health-selection loop.
        /// </summary>
        internal void StartFinishOffSequence()
        {
            self.RequestWalkForwardReallySlow(unchecked((ushort)(self.Body.Position.X + 0x0010)));
            self.BabyMetroidTileTransferIndex = 0;
            self.BabyMetroidSpawned = false;
            self.Phase = MotherBrainRainbowBeamAttackPhase.FinishSamusOff;
        }

        /// <summary>
        /// Applies damage already calculated by the ordinary enemy-shot engine. Bank `$A0` owns
        /// beam/item damage multipliers, invulnerability, projectile deletion, and hit flashing;
        /// this actor owns only the resulting health word and the phase-three zero-health branch.
        /// Keeping that boundary explicit lets a live projectile producer call the real actor
        /// without duplicating the room enemy system's translated generic damage routine.
        /// </summary>
        internal void ApplyCalculatedBrainDamage(ushort damage)
        {
            // Generic enemy damage saturates at zero. A host subtraction with ushort wrapping
            // would resurrect a nearly dead boss, so perform the borrow test before the write.
            self.BrainHealth = damage >= self.BrainHealth
                ? (ushort)0
                : unchecked((ushort)(self.BrainHealth - damage));
        }

        /// <summary>
        /// Executes the movement half of <c>$A9:9072-$91B7</c> on Mother Brain's later brain
        /// enemy slot. Call this after the body AI/body instruction stage and before the still
        /// later Baby slot, matching the retail increasing-slot enemy loop.
        /// </summary>
        internal void StepNeckMovement(ISnesAddressSpace bus, SamusState samus)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            if (self.NeckMovementEnabled)
            {
                SnesAngle lowerAngle = self.LowerNeckAngle;
                SnesAngle upperAngle = self.UpperNeckAngle;
                MotherBrainNeckMovement lowerIndex = self.LowerNeckMovementIndex;
                MotherBrainNeckMovement upperIndex = self.UpperNeckMovementIndex;
                MotherBrainNeckKinematics.StepAngles(
                    ref lowerAngle,
                    ref upperAngle,
                    ref lowerIndex,
                    ref upperIndex,
                    self.NeckAngleDelta,
                    self.BrainPosition.Y,
                    samus.Position.Y.Whole);
                self.LowerNeckAngle = lowerAngle;
                self.UpperNeckAngle = upperAngle;
                self.LowerNeckMovementIndex = lowerIndex;
                self.UpperNeckMovementIndex = upperIndex;
            }

            self.BrainPosition = MotherBrainNeckKinematics.CalculateGeometry(self.Body.Position, self.LowerNeckAngle, self.UpperNeckAngle,
                MotherBrainNeckJointDefinitions.InitialDistances)[MotherBrainNeckJointDefinitions.BrainAnchor];
        }

        /// <summary>
        /// Executes the body-owned shake countdown consumed while the later graphics hook draws
        /// Mother Brain's brain at <c>$A9:9382-$939A</c>. Call after all enemy slots, matching the
        /// renderer: `$BE96` can then observe zero and reseed fifty on the following frame.
        /// </summary>
        internal ushort StepBrainShakeForDraw()
        {
            if (!self.BrainMainShakeTimer.IsZero)
                self.BrainMainShakeTimer = self.BrainMainShakeTimer.Decrement().Remaining;
            return unchecked((ushort)(self.BrainMainShakeTimer.RawValue & 6));
        }
    }
}
