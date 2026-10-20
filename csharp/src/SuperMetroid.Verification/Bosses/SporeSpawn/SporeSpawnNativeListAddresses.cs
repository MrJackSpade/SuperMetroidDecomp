using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// The native bank-$A5 and bank-$86 lists the Spore Spawn ROM oracle (#1288) walks: the body and projectile list
/// entry points and the instructions the body lists use. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SporeSpawnNativeListAddresses
{
    /// <summary>Spore Spawn's instruction bank, $A5.</summary>
    internal const byte BodyBankNumber = 0xa5;

    /// <summary>Each typed body program's native entry point in bank $A5, in authored order.</summary>
    internal static readonly (SporeSpawnBodyProgram Program, ushort Entry)[] BodyEntries =
    [
        (SporeSpawnBodyProgram.InitialDead, 0xe6b9),
        (SporeSpawnBodyProgram.InitialAlive, 0xe6c7),
        (SporeSpawnBodyProgram.FightStarted, 0xe6d5),
        (SporeSpawnBodyProgram.OpenAndStop, 0xe6e3),
        (SporeSpawnBodyProgram.OpenPulse, 0xe715),
        (SporeSpawnBodyProgram.CloseAndMove, 0xe729),
        (SporeSpawnBodyProgram.Death, 0xe77d),
        (SporeSpawnBodyProgram.DyingExplosions, 0xe78d),
        (SporeSpawnBodyProgram.Hardening, 0xe7bd),
    ];

    /// <summary>The native body lists' extent, $A5:E6B9..E811, which production must never read.</summary>
    internal const int BodyListsStart = 0xa5e6b9;
    /// <summary>The first address past the native body lists.</summary>
    internal const int BodyListsEnd = 0xa5e811;

    /// <summary>Each bank-$A5 Spore Spawn instruction the body lists use: the cue it must become and its operand words.</summary>
    internal static readonly Dictionary<ushort, (NativeCueShape<SporeSpawnCue> Cue, int Operands)> BodyInstructions = new()
    {
        [0xe75f] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.IncreaseMaxXRadius)), 0),
        [0xe771] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.ClearDamagedFlag)), 0),
        [0xe82d] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.SetMaxXRadiusAndAngleDelta>(""), 2),
        [0xe872] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.SetSporeGeneration>(""), 1),
        [0xe87c] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.Harden)), 0),
        [0xe895] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.QueueSound>(""), 1),
        [0xe8b1] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.RequestDeathDrops)), 0),
        [0xe8ba] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.SetFunction>(""), 1),
        [0xe8ca] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.LoadDeathPalette>(""), 1),
        [0xe91c] = (NativeCueShape<SporeSpawnCue>.Of<SporeSpawnCue.LoadDeathTargetPalette>(""), 1),
        [0xe96e] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.SpawnHardeningDust)), 0),
        [0xe9b1] = (NativeCueShape<SporeSpawnCue>.Exactly(new SporeSpawnCue.Command(SporeSpawnCueKind.SpawnDyingExplosion)), 0),
    };

    /// <summary>Each typed projectile program's native entry point in bank $86.</summary>
    internal static readonly (SporeSpawnProjectileProgram Program, ushort Entry)[] ProjectileEntries =
    [
        (SporeSpawnProjectileProgram.SpawnerClosed, 0xdc00),
        (SporeSpawnProjectileProgram.SpawnerRelease, 0xdc06),
        (SporeSpawnProjectileProgram.Spore, 0xdc1e),
        (SporeSpawnProjectileProgram.Stalk, 0xdc2e),
        (SporeSpawnProjectileProgram.SporeShot, 0xdc34),
    ];

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static SporeSpawnBodyProgram? BodyProgramAt(ushort address) =>
        BodyEntries.Where(item => item.Entry == address).Select(item => (SporeSpawnBodyProgram?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort BodyEntry(SporeSpawnBodyProgram program) => BodyEntries.Single(item => item.Program == program).Entry;
}
