using SuperMetroid.ResourceAudit.Resources;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>
/// Checks that a constant projectile or charge-flare composition lookup names a composition its provider installs: the
/// projectile sprites hold every projectile and flare composition, the charge-flare sprites the flare's alone (#1624).
/// </summary>
internal static class ProjectileSpriteDomainAudit
{
    /// <summary>Reports a constant composition lookup outside its provider's reviewed composition domain.</summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.Name != "Frame") return null;
        string provider = operation.TargetMethod.ContainingType.ToDisplayString();
        bool flareOnly = provider == typeof(ChargeFlareSprites).FullName;
        if (!flareOnly && provider != typeof(InstalledProjectileSprites).FullName) return null;
        if (ConsumerAudit.ProjectileSpriteKey(operation) is not { } name) return null;
        bool installed = ChargeFlareSpriteDefinitions.All.Any(sprite => sprite.Name == name) ||
            (!flareOnly && ProjectileSpriteDefinitions.All.Any(sprite => sprite.Name == name));
        return installed
            ? null
            : flareOnly
                ? $"Known charge-flare composition {name} is outside the reviewed flare composition domain."
                : $"Known projectile composition {name} is outside the reviewed installed composition domain.";
    }
}
