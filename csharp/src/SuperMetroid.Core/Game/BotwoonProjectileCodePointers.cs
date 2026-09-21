namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$86 code and shared-program pointers used by Botwoon's projectiles.</summary>
internal static class BotwoonProjectileCodePointers
{
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Main</c> at $86:EA98.</summary>
    internal const ushort BodyMainFunction = 0xea98;
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_SetDelay</c> at $86:EAF4.</summary>
    internal const ushort BodyBeginDyingFunction = 0xeaf4;
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Delay</c> at $86:EB04.</summary>
    internal const ushort BodyDyingDelayFunction = 0xeb04;
    /// <summary><c>Function_EnemyProjectile_BotwoonsBody_Dying_Falling</c> at $86:EB1F.</summary>
    internal const ushort BodyFallingFunction = 0xeb1f;
    /// <summary><c>RTS_86EB93</c>, the inert state after a body segment lands.</summary>
    internal const ushort BodyLandedFunction = 0xeb93;
    /// <summary>
    /// $86:EB8F, the pre-catalog managed landed sentinel serialized by older debugger states.
    /// New execution installs the cartridge's $86:EB93 RTS; restore retains this alias only
    /// so an in-flight historical Botwoon save does not fail its dispatcher.
    /// </summary>
    internal const ushort LegacyBodyLandedFunction = 0xeb8f;
    /// <summary><c>InstList_EnemyProj_MiscDust_1D_BigExplosion</c> at $86:E208.</summary>
    internal const ushort BodyLandedInstruction =
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBigDeathExplosionInitial;
}
