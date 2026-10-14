using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.Kraid"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersKraidConstants
{
    /// <summary>Kraid ceiling-rock X-position words at $A7:ACB3.</summary>
    public const int CeilingRockXWords = 0xa7acb3;
    /// <summary>Kraid combat timer word at $A7:974A.</summary>
    public const int CombatTimerWord = 0xa7974a;
    /// <summary>Kraid death explosion Y/function records at $A7:C5E7.</summary>
    public const int DeathExplosionRecords = 0xa7c5e7;
    /// <summary>Kraid death initial timer word at $A7:9764.</summary>
    public const int DeathInitialTimerWord = 0xa79764;
    /// <summary>Fake Kraid's ordinary enemy-population record at $A1:A0EA.</summary>
    public const int FakeKraidPopulationRecord = 0xa1a0ea;
    /// <summary>Kraid hitbox left-coordinate records at $A7:B163.</summary>
    public const int HitboxLeftWords = 0xa7b163;
    /// <summary>Kraid hitbox top-coordinate records at $A7:B165.</summary>
    public const int HitboxTopWords = 0xa7b165;
    /// <summary>Initial nail spritemap pointer word at $A7:8B0C.</summary>
    public const int InitialNailSpritemapWord = 0xa78b0c;
    /// <summary>Kraid roar/growth initial timer words at $A7:96D2.</summary>
    public const int InitialTimerWords = 0xa796d2;
    /// <summary>$A7:BE46, Function_KraidNail_Initialize.downwardsVelocityPointers; selected when the sibling's Y velocity is nonnegative.</summary>
    public const int NailDownwardVelocityPointers = 0xa7be46;
    /// <summary>Kraid fingernail position-offset words at $A7:BF1D.</summary>
    public const int NailPositionOffsetWords = 0xa7bf1d;
    /// <summary>$A7:BE3E, Function_KraidNail_Initialize.upwardsVelocityPointers; selected when the sibling's Y velocity is negative.</summary>
    public const int NailUpwardVelocityPointers = 0xa7be3e;
    /// <summary>Eight ordered Kraid room-population records at $A1:9EB5.</summary>
    public const int PopulationRecords = 0xa19eb5;
    /// <summary>Kraid spat-rock X velocity words at $A7:BC65.</summary>
    public const int RockXVelocityWords = 0xa7bc65;
    /// <summary>Kraid second-phase movement record table at $A7:BA7D.</summary>
    public const int SecondPhaseMovementRecords = 0xa7ba7d;

    /// <summary>Provides Kraid's native combat, population, projectile, hitbox, and death table pointers for behavior checks.</summary>
    extension(EnemyRomTablePointers.Kraid)
    {
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.CeilingRockXWords"/>
        internal static int CeilingRockXWords => EnemyRomTablePointersKraidConstants.CeilingRockXWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.CombatTimerWord"/>
        internal static int CombatTimerWord => EnemyRomTablePointersKraidConstants.CombatTimerWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.DeathExplosionRecords"/>
        internal static int DeathExplosionRecords => EnemyRomTablePointersKraidConstants.DeathExplosionRecords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.DeathInitialTimerWord"/>
        internal static int DeathInitialTimerWord => EnemyRomTablePointersKraidConstants.DeathInitialTimerWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.FakeKraidPopulationRecord"/>
        internal static int FakeKraidPopulationRecord => EnemyRomTablePointersKraidConstants.FakeKraidPopulationRecord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.HitboxLeftWords"/>
        internal static int HitboxLeftWords => EnemyRomTablePointersKraidConstants.HitboxLeftWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.HitboxTopWords"/>
        internal static int HitboxTopWords => EnemyRomTablePointersKraidConstants.HitboxTopWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.InitialNailSpritemapWord"/>
        internal static int InitialNailSpritemapWord => EnemyRomTablePointersKraidConstants.InitialNailSpritemapWord;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.InitialTimerWords"/>
        internal static int InitialTimerWords => EnemyRomTablePointersKraidConstants.InitialTimerWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailDownwardVelocityPointers"/>
        internal static int NailDownwardVelocityPointers => EnemyRomTablePointersKraidConstants.NailDownwardVelocityPointers;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailPositionOffsetWords"/>
        internal static int NailPositionOffsetWords => EnemyRomTablePointersKraidConstants.NailPositionOffsetWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.NailUpwardVelocityPointers"/>
        internal static int NailUpwardVelocityPointers => EnemyRomTablePointersKraidConstants.NailUpwardVelocityPointers;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.PopulationRecords"/>
        internal static int PopulationRecords => EnemyRomTablePointersKraidConstants.PopulationRecords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.RockXVelocityWords"/>
        internal static int RockXVelocityWords => EnemyRomTablePointersKraidConstants.RockXVelocityWords;
        /// <inheritdoc cref="EnemyRomTablePointersKraidConstants.SecondPhaseMovementRecords"/>
        internal static int SecondPhaseMovementRecords => EnemyRomTablePointersKraidConstants.SecondPhaseMovementRecords;
    }
}
