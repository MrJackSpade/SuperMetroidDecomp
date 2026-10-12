using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Matches SetEnemyProjectileVisualOperand's declared routing: an installed
/// operand frame wins; otherwise the compiled selector names direct bank-$8D OAM.
/// Neither a missing operand binding nor a matching ID in another bank proves
/// that the actual production draw resource is missing or present.
/// </summary>
internal static class ProjectileDefinitionAudit
{
    public static void RequireFrame(ushort operand, string owner, string source,
        ResourceIndex exports, AuditReport report)
    {
        byte bank = ResourceBanks.EnemyProjectilePrograms;
        ushort? direct = SkreeMetareeParticleVisualDefinitions.TryResolve(operand, out ushort particle)
            ? particle
            : CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort pointer) ? pointer : null;
        RequireFrame(operand, direct, owner, source, exports, report);
    }

    internal static void RequireFrame(ushort operand, ushort? direct, string owner, string source,
        ResourceIndex exports, AuditReport report)
    {
        string key = ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, operand);
        if (exports.Contains(ResourceDomains.EnemyProjectileProgram, key))
            report.Require(ResourceDomains.EnemyProjectileProgram, owner, key, source, exports);
        else if (direct is ushort pointer)
            report.Require(ResourceDomains.EnemyProjectileSprite, owner,
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer), source, exports);
        else
            report.Require(ResourceDomains.CompiledSelector, owner, key, source, exports);
    }
}
