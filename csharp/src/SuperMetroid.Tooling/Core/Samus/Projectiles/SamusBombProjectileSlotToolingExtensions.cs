namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="SamusBombProjectileSlot"/>.</summary>
internal static class SamusBombProjectileSlotToolingExtensions
{
    /// <summary>Exposes the native program address the slot's cursor stands for to tools and verification.</summary>
    extension(SamusBombProjectileSlot self)
    {
        /// <summary>The bank-$93 instruction pointer at <c>$0C4A + 2*Index</c> the cursor stands for; zero marks an inactive slot.</summary>
        internal int InstructionPointer => SamusProjectileProgramAddresses.Address(self.Cursor);
    }
}
