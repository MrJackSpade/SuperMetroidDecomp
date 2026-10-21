namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of a projectile slot's program position.</summary>
internal static class SamusProjectileSlotNative
{
    /// <summary>A slot's program position by bank-$93 address.</summary>
    extension(SamusProjectileSlot slot)
    {
        /// <summary>The bank-$93 instruction pointer the cursor stands for; zero marks the slot inactive.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(slot.Cursor);
    }
}
