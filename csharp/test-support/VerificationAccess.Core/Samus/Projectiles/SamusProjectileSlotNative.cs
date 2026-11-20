namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of a projectile slot's program position and timer.</summary>
internal static class SamusProjectileSlotNative
{
    /// <summary>A slot's program position by bank-$93 address.</summary>
    extension(SamusProjectile slot)
    {
        /// <summary>The bank-$93 instruction pointer the cursor stands for; zero marks the slot inactive.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(slot.Playback);

        /// <summary>
        /// The type the native slot's type word records (#2558): the projectile's own while it holds its place, otherwise
        /// the cleared word's, an uncharged Power beam's.
        /// </summary>
        internal SuperMetroid.Core.Progression.SamusProjectileType RecordedType => slot.IsOccupied
            ? slot.Type
            : SuperMetroid.Core.Progression.SamusProjectileType.Beam(SuperMetroid.Core.Progression.SamusBeamCombination.Power, charged: false);

        /// <summary>Where the native snapshot's per-slot word arrays hold this projectile's place: twice its ordinal.</summary>
        internal int SlotTableOffset => slot.Place * sizeof(ushort);

        /// <summary>
        /// The native timer the slot's animation stands for (#2458): the updates its frame has left, zero for an ended slot,
        /// or null while it holds a still picture, which the native timer only re-shows (#2347).
        /// </summary>
        internal int? NativeFrameHold => slot.Animation is not { } animation ? 0 : animation.IsResting ? null : animation.UpdatesLeft;
    }

    /// <summary>Exposes the native program address and timer a bomb slot's animation stands for to verification.</summary>
    extension(SamusBomb slot)
    {
        /// <summary>The bank-$93 instruction pointer at <c>$0C4A + 2*Index</c> the cursor stands for; zero marks an inactive slot.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(slot.Playback);

        /// <summary>The native timer the bomb's animation stands for (#2458): the updates its frame has left, zero for an ended slot.</summary>
        internal int NativeFrameHold => slot.Animation?.UpdatesLeft ?? 0;
    }
}
