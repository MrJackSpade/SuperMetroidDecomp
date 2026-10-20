using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Evaluates immutable declaration catalogs, not enemy AI or animation timelines.
/// Required selector targets are independent of the export lists under comparison.
/// </summary>
internal static class DefinitionAudit
{
    /// <summary>Collects enemy and projectile exports, then runs palette and Mother Brain flash audits.</summary>
    public static ResourceIndex Collect(string root, AuditReport report)
    {
        ResourceIndex exports = CollectEnemyVisuals(root, report);
        PaletteDefinitionAudit.Run(root, exports, report);
        MotherBrainRoomFlashAudit.Install(exports);
        report.Coverage.Add(new(ResourceDomains.SamusProjectile, 0, exports.Count(ResourceDomains.SamusProjectile)));
        return exports;
    }

    /// <summary>Indexes compiled enemy and projectile visual targets and reports unresolved consumer references.</summary>
    internal static ResourceIndex CollectEnemyVisuals(string root, AuditReport report)
    {
        var exports = new ResourceIndex();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            string key = ResourceIndex.Address(frame.Bank, frame.Pointer);
            exports.Add(ResourceDomains.EnemySimple, key);
            exports.Add(ResourceDomains.EnemyDisplay, key);
        }
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            string key = ResourceIndex.Address(frame.Bank, frame.Pointer);
            exports.Add(ResourceDomains.EnemyExtended, key);
            exports.Add(ResourceDomains.EnemyDisplay, key);
        }
        CompiledEnemyDisplayAudit.Install(root, exports, report);
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameSets.All)
            exports.Add(ResourceDomains.EnemyProjectileProgram, ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, frame.OperandAddress));
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
            exports.Add(ResourceDomains.EnemyProjectileSprite, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
        // This is an explicit zero-part definition, not a missing art fallback.
        exports.Add(ResourceDomains.EnemyProjectileSprite,
            ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, EnemyProjectileSpritemapDefinitions.BlankSpritemap));
        foreach (ushort pointer in ProjectileSpriteDefinitions.NativePointers)
            exports.Add(ResourceDomains.SamusProjectile, ResourceIndex.Address(ResourceBanks.SamusProjectiles, pointer));

        int enemyReferences = 0, projectileReferences = 0;
        for (int index = 0; index < CompiledEnemyVisualSelectorsTooling.Count; index++)
        {
            CompiledEnemyVisualSelector selector = CompiledEnemyVisualSelectorsTooling.At(index);
            exports.Add(ResourceDomains.CompiledSelector,
                ResourceIndex.Address(SnesAddressMath.BankNumber(selector.Address), SnesAddressMath.OffsetInBank(selector.Address)));
            int bank = SnesAddressMath.BankNumber(selector.Address);
            string source = SelectorSource(root, bank, selector.Address);
            if (bank == ResourceBanks.EnemyProjectilePrograms)
            {
                ProjectileDefinitionAudit.RequireFrame((ushort)selector.Address,
                    "CompiledEnemyVisualSelectors", source, exports, report);
                projectileReferences++;
            }
            else
            {
                report.Require(ResourceDomains.EnemyDisplay, "CompiledEnemyVisualSelectors",
                    ResourceIndex.Address(bank, selector.Pointer), source, exports);
                enemyReferences++;
            }
        }
        report.Coverage.Add(new(ResourceDomains.EnemyDisplay, enemyReferences, exports.Count(ResourceDomains.EnemyDisplay)));
        report.Coverage.Add(new("enemy-projectile-frames", projectileReferences,
            exports.Count(ResourceDomains.EnemyProjectileProgram) + exports.Count(ResourceDomains.EnemyProjectileSprite)));
        // Samus projectile bindings are imported visual data rather than compiled
        // selectors. Enumerating exported sprites alone cannot prove their input
        // mappings complete; the consumer inventory must retain that boundary.
        return exports;
    }

    /// <summary>Locates the declaration of a selector in generated tables or calculated visual definitions.</summary>
    private static string SelectorSource(string root, int bank, int address)
    {
        // Only the bank-$86 fireballs compute their selectors; every other bank's are literal generated entries.
        bool calculated = CompiledEnemyVisualSelectors.IsCalculatedSelector(address);
        const string calculatedMethod = "Spritemap";
        string relative = calculated
            ? AlcoonFireballFrameDefinitions.TryFrame((ushort)address, out _)
                ? "csharp/src/SuperMetroid.Tooling/Core/Enemies/Alcoon/Artwork/AlcoonFireballFrameToolingDefinitions.cs"
                : "csharp/src/SuperMetroid.Tooling/Core/Enemies/FuneNamihe/Artwork/FuneNamiheFireballFrameToolingDefinitions.cs"
            : $"csharp/src/SuperMetroid.Tooling/Core/Enemies/Artwork/CompiledEnemyVisualSelectors.Bank{bank:X2}.Definitions.Generated.cs";
        int line = 0;
        foreach (string text in File.ReadLines(Path.Combine(root, relative)))
        {
            line++;
            if (calculated ? text.Contains($"internal static ushort {calculatedMethod}", StringComparison.Ordinal)
                : text.Contains($"0x{address:X6}", StringComparison.OrdinalIgnoreCase)) return $"{relative}:{line}";
        }
        throw new InvalidDataException($"Compiled selector ${address:X6} has no matching source declaration.");
    }
}
