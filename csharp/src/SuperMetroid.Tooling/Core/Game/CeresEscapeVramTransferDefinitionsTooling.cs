namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresEscapeVramTransferDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeVramTransferDefinitionsTooling
{
    /// <summary>Gets the ordered read-only view of the compiled Ceres escape VRAM transfer records for tooling audits.</summary>
    internal static IReadOnlyList<CeresEscapeVramTransferDefinition> All => CeresEscapeVramTransferDefinitions.Records;
}
