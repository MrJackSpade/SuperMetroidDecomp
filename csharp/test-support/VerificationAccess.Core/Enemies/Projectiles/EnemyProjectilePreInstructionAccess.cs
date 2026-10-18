using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification's native view of enemy-projectile pre-instructions: the bank-$86 routine word each identity names.</summary>
internal static class EnemyProjectilePreInstructionAccess
{
    /// <summary>The bank-$86 routine word of <paramref name="preInstruction"/>.</summary>
    internal static ushort Word(EnemyProjectilePreInstruction preInstruction) => (ushort)preInstruction;

    /// <summary>The pre-instruction whose bank-$86 routine is <paramref name="word"/>.</summary>
    internal static EnemyProjectilePreInstruction FromWord(ushort word) =>
        ClosedNativeWords.Decode<EnemyProjectilePreInstruction>(word, "enemy-projectile pre-instruction");

    /// <summary>Native views of a projectile slot.</summary>
    extension(RoomEnemyProjectileSlot self)
    {
        /// <summary>The bank-$86 routine word of the slot's pre-instruction.</summary>
        internal ushort PreInstructionWord
        {
            get => Word(self.PreInstruction);
            set => self.PreInstruction = FromWord(value);
        }
    }
}
