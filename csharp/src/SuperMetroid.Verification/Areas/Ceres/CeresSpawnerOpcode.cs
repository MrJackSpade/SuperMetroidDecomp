namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Private instruction sets of native cinematic lists that verification interprets as an
/// oracle but production does not execute through the shared sprite interpreter.
/// </summary>
internal enum CeresSpawnerOpcode : ushort
{
    /// <summary>$8B:C404, the Ceres explosion spawner's initial burst.</summary>
    SpawnInitial = 0xc404,

    /// <summary>$8B:C50C, the Ceres explosion spawner's final burst.</summary>
    SpawnFinal = 0xc50c,
}
