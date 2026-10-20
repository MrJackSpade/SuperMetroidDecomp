namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Which palette rows a Spore Spawn death-palette step writes.</summary>
internal enum SporeSpawnPaletteCopy
{
    /// <summary>Both the current and the target rows.</summary>
    CurrentAndTarget,

    /// <summary>Only the target rows.</summary>
    TargetOnly,
}
