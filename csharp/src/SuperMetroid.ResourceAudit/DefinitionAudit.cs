using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Evaluates immutable declaration catalogs, not enemy AI or animation timelines.
/// Required selector targets are independent of the export lists under comparison.
/// </summary>
internal static class DefinitionAudit
{
    public static ResourceIndex Collect(string root, AuditReport report)
    {
        var exports = new ResourceIndex();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            string key = ResourceIndex.Address(frame.Bank, frame.Pointer);
            exports.Add(ResourceDomains.EnemySimple, key);
            exports.Add(ResourceDomains.EnemyDisplay, key);
        }
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            string key = ResourceIndex.Address(frame.Bank, frame.Pointer);
            exports.Add(ResourceDomains.EnemyExtended, key);
            exports.Add(ResourceDomains.EnemyDisplay, key);
        }
        CompiledEnemyDisplayAudit.Install(root, exports, report);
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameDefinitions.All)
            exports.Add(ResourceDomains.EnemyProjectileProgram, ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, frame.OperandAddress));
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
            exports.Add(ResourceDomains.EnemyProjectileSprite, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
        // This is an explicit zero-part definition, not a missing art fallback.
        exports.Add(ResourceDomains.EnemyProjectileSprite,
            ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, EnemyProjectileSpritemapDefinitions.BlankSpritemap));
        foreach (ushort pointer in ProjectileSpriteDefinitions.NativePointers)
            exports.Add(ResourceDomains.SamusProjectile, ResourceIndex.Address(ResourceBanks.SamusProjectiles, pointer));

        int enemyReferences = 0, projectileReferences = 0;
        for (int index = 0; index < CompiledEnemyVisualSelectors.Count; index++)
        {
            CompiledEnemyVisualSelector selector = CompiledEnemyVisualSelectors.At(index);
            exports.Add(ResourceDomains.CompiledSelector,
                ResourceIndex.Address(selector.Address >> 16, selector.Address & 0xffff));
            int bank = selector.Address >> 16;
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
        PaletteDefinitionAudit.Run(root, exports, report);
        MotherBrainRoomFlashAudit.Install(exports);
        // Samus projectile bindings are imported visual data rather than compiled
        // selectors. Enumerating exported sprites alone cannot prove their input
        // mappings complete; the consumer inventory must retain that boundary.
        report.Coverage.Add(new(ResourceDomains.SamusProjectile, 0, exports.Count(ResourceDomains.SamusProjectile)));
        return exports;
    }

    private static string SelectorSource(string root, int bank, int address)
    {
        bool calculated = CompiledEnemyVisualSelectors.IsCalculatedSelector(address);
        string calculatedMethod = AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address)
            ? "AlcoonFrameAt" : "AtomicFrameAt";
        string relative = calculated
            ? "csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs"
            : $"csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank{bank:X2}.Definitions.cs";
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
