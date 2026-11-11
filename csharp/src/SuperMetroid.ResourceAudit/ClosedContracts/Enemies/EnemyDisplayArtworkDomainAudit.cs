using SuperMetroid.ResourceAudit.Resources;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Checks that a constant display lookup names a frame of the installed schema it reads.</summary>
internal static class EnemyDisplayArtworkDomainAudit
{
    /// <summary>
    /// Reports a constant display lookup whose frame is absent from the reviewed frame domain it reads: the ordinary
    /// compositions' frames, or every installed frame and the empty frame for the frame displays gameplay draws by.
    /// </summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        string type = operation.TargetMethod.ContainingType.ToDisplayString();
        if (operation.TargetMethod.Name != "TryGetDisplay" ||
            type != typeof(EnemySpritemapCatalog).FullName && type != typeof(EnemyFrameDisplays).FullName) return null;
        if (ConsumerAudit.EnemyFrameKey(operation) is not { } name) return null;
        bool ordinary = EnemySpritemapDefinitions.Frames.Any(frame => frame.Frame.ToString() == name);
        bool owned = type == typeof(EnemySpritemapCatalog).FullName
            ? ordinary
            : ordinary || EnemyExtendedFrameDefinitions.Frames.Any(frame => frame.Frame.ToString() == name) ||
                name == nameof(EnemyFrameId.Empty);
        return owned ? null : $"Known display identity {name} is outside the reviewed installed frame domain.";
    }
}
