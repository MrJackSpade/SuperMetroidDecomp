namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the intro flashback's Rinka spawner actor.</summary>
internal enum IntroRinkaSpawnerInstruction : ushort
{
    /// <summary><c>Instruction_Spawn_IntroRinkas_0_1</c> at $8B:BA21.</summary>
    SpawnRinkas0And1 = 0xba21,

    /// <summary><c>Instruction_Spawn_IntroRinkas_2_3</c> at $8B:BA36.</summary>
    SpawnRinkas2And3 = 0xba36,
}
