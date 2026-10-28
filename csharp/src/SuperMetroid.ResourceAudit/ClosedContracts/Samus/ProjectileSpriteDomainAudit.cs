using SuperMetroid.ResourceAudit.Resources;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Checks that a constant projectile composition lookup names a composition the catalog installs.</summary>
internal static class ProjectileSpriteDomainAudit
{
    /// <summary>Reports a constant composition lookup outside the reviewed projectile composition domain.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "Frame" ||
            operation.TargetMethod.ContainingType.ToDisplayString() != typeof(InstalledProjectileSprites).FullName) return null;
        if (ConsumerAudit.ProjectileSpriteKey(operation) is not { } name) return null;
        return ProjectileSpriteDefinitions.All.Any(sprite => sprite.Name == name) || ChargeFlareSpriteDefinitions.All.Any(sprite => sprite.Name == name)
            ? null
            : $"Known projectile composition {name} is outside the reviewed installed composition domain.";
    }
}
