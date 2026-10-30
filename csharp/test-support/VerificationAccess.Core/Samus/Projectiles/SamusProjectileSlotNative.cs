namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of a projectile slot's program position.</summary>
internal static class SamusProjectileSlotNative
{
    /// <summary>A slot's program position by bank-$93 address.</summary>
    extension(SamusProjectileSlot slot)
    {
        /// <summary>The bank-$93 instruction pointer the cursor stands for; zero marks the slot inactive.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(slot.Cursor);

        /// <summary>Where the native snapshot's per-slot word arrays hold this slot: twice its ordinal.</summary>
        internal int SlotTableOffset => slot.SlotIndex * sizeof(ushort);
    }

    /// <summary>Exposes the native program address a bomb slot's cursor stands for to verification.</summary>
    extension(SamusBombProjectileSlot slot)
    {
        /// <summary>The bank-$93 instruction pointer at <c>$0C4A + 2*Index</c> the cursor stands for; zero marks an inactive slot.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(slot.Cursor);
    }
}
