namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Development-tool members of <see cref="CeresEscapeVramTransferDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeVramTransferDefinitionsTooling
{
    /// <summary>Read-only view of the complete compiled transfer sequence for development audits.</summary>
    internal static IReadOnlyList<CeresEscapeVramTransferDefinition> All => CeresEscapeVramTransferDefinitions.Records;
}
