namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Which colour copies a Spore Spawn death-palette step writes.</summary>
internal enum SporeSpawnPaletteCopy
{
    /// <summary>Both the current and the target colours.</summary>
    CurrentAndTarget,

    /// <summary>Only the target colours.</summary>
    TargetOnly,
}
