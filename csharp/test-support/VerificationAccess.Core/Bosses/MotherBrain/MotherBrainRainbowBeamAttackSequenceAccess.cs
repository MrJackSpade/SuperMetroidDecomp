namespace SuperMetroid.Core.Bosses.MotherBrain;
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
            self.State.RequestWalkForward(unchecked((ushort)(self.State.BodyPosition.X + 0x0010)), MotherBrainWalkSpeed.ReallySlow);
            self.Encounter.FinishOffPhase = MotherBrainFinishOffPhase.AttackUntilSamusIsLow;
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
            SuperMetroid.Core.Enemies.RoomEnemyPlace brain = self.Encounter.Head;
            brain.Health = damage >= brain.Health
                ? (ushort)0
                : unchecked((ushort)(brain.Health - damage));
        }
    }
}
