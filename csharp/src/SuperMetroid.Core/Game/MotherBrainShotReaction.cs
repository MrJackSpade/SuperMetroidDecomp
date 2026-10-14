namespace SuperMetroid.Core.Game;

/// <summary>
/// Ports the walk-counter arithmetic of Mother Brain's phase-two/three shot reaction
/// <c>$A9:B562-$B5C4</c>, independent of which object currently owns the counter.
/// </summary>
internal static class MotherBrainShotReaction
{
    /// <summary>Updates phase-two/three walk credit after a shot and signals recoil only on form-four Hyper Beam underflow.</summary>
    public static MotherBrainShotReactionResult Resolve(
        ushort form,
        MotherBrainProjectileType projectileType,
        ushort walkCounter)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot smuggle a larger value past it.
        if ((uint)projectileType > 7)
            throw new ArgumentOutOfRangeException(nameof(projectileType));

        // The table returns two for beams, one for missiles/supers, and zero for every
        // remaining projectile class. Form four gives only reaction type two the special
        // Hyper Beam path; ordinary phase-two beams continue through the generic branch.
        ushort reactionType = projectileType switch
        {
            MotherBrainProjectileType.Beam => 2,
            MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile => 1,
            MotherBrainProjectileType.PowerBomb or MotherBrainProjectileType.ClassFour or
                MotherBrainProjectileType.Bomb or MotherBrainProjectileType.ClassSix or
                MotherBrainProjectileType.BeamExplosion => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(projectileType), projectileType, "Undefined projectile class."),
        };
        if (form == 4 && reactionType == 2)
        {
            // `$B5A9`: BPL keeps the nonnegative remainder without recoil, which is why
            // sustained Hyper Beam fire first consumes accumulated walk credit. On underflow
            // the counter is replaced by zero, not the wrapped subtraction.
            ushort candidate = unchecked((ushort)(walkCounter - 266));
            return unchecked((short)candidate) >= 0
                ? new MotherBrainShotReactionResult(candidate, HyperBeamRecoil: false)
                : new MotherBrainShotReactionResult(0, HyperBeamRecoil: true);
        }

        // DEC turns reaction one into zero, sending either missile kind directly to the
        // zero label. Reaction zero wraps to `$FFFF`; reaction two outside form four leaves
        // one. Both nonzero cases subtract `$0100` and clamp signed underflow to zero.
        reactionType = NativeWordCounter.Decrement(reactionType).Value;
        if (reactionType == 0)
            return new MotherBrainShotReactionResult(0, HyperBeamRecoil: false);

        ushort genericCandidate = unchecked((ushort)(walkCounter - 256));
        return new MotherBrainShotReactionResult(
            unchecked((short)genericCandidate) >= 0 ? genericCandidate : (ushort)0,
            HyperBeamRecoil: false);
    }
}
