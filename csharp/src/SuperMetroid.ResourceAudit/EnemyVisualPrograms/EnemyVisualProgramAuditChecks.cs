using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>Negative fixtures for #1166's requested static completeness gate.</summary>
internal static class EnemyVisualProgramAuditChecks
{
    /// <summary>A one-frame list laid out as Kzan's native list at $A6:8B29: its frame's duration word.</summary>
    private const ushort FixtureFrame = 35625;

    /// <summary>The fixture frame's visual operand, two bytes after its duration.</summary>
    private const ushort FixtureOperand = FixtureFrame + 2;

    /// <summary>The fixture list's terminal sleep, after the four-byte frame.</summary>
    private const ushort FixtureSleep = FixtureFrame + 4;

    /// <summary>Checks owner discovery and independently reports missing selector, artwork, bank, or projectile binding.</summary>
    internal static void Run()
    {
        var syntax = CSharpSyntaxTree.ParseText("""
            static class SingleInstructionProgramDefinitions { const ushort PresentationWord = 1; }
            static class UnknownInstructionProgramDefinitions { }
            static class RenamedCatalog { public static ushort ReadWord(ushort p) => p; }
            class RoomEnemySystem {
                ushort ReadEnemyInstructionMechanicsWord(ushort p) => RenamedCatalog.ReadWord(p);
            }
            """);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("VisualAuditFixture", [syntax],
            platforms.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var owners = EnemyVisualProgramAudit.DiscoverOwners(compilation);
        Require(owners.SetEquals(["SingleInstructionProgramDefinitions", "UnknownInstructionProgramDefinitions", "RenamedCatalog"]),
            "single-operand, unknown and renamed interpreter owners must all be inventoried");
        var missingDeclarations = new Dictionary<ushort, ushort>
        {
            [FixtureFrame] = 1,
            [FixtureSleep] = (ushort)CommonEnemyInstruction.Sleep,
        };
        Require(EnemyVisualProgramAudit.InterleavedOperands(missingDeclarations)
                .SequenceEqual([FixtureOperand]),
            "mechanics gaps expose Kzan's operand even without its presentation declaration");
        missingDeclarations.Remove(FixtureSleep);
        Require(EnemyVisualProgramAudit.InterleavedOperands(missingDeclarations)
                .SequenceEqual([FixtureOperand]),
            "terminal frame operands cannot escape the independent inventory");

        EnemySpritemapDefinition frame = EnemyCompositionSchemaTooling.Family("kzan_").Single();
        ushort operand = FixtureOperand;
        var exports = new ResourceIndex();
        exports.Add(ResourceDomains.EnemyDisplay, ResourceIndex.Address(frame.Bank, frame.Pointer));
        var missingSelector = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(frame.Bank, operand, null, "Kzan", "fixture", exports, missingSelector);
        Require(missingSelector.Findings.Single().Domain == ResourceDomains.CompiledSelector,
            "existing artwork must not conceal a missing selector");
        var missingArt = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(frame.Bank, operand, frame.Pointer, "Kzan", "fixture", new ResourceIndex(), missingArt);
        Require(missingArt.Findings.Single().Domain == ResourceDomains.EnemyDisplay,
            "existing selector must not conceal missing artwork");
        var complete = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(frame.Bank, operand, frame.Pointer, "Kzan", "fixture", exports, complete);
        Require(complete.Findings.Count == 0, "complete selector/artwork resolves");
        var wrongBank = new ResourceIndex();
        wrongBank.Add(ResourceDomains.EnemyDisplay, ResourceIndex.Address(EnemyCompositionSchemaTooling.Family("polyp_").Single().Bank, frame.Pointer));
        var wrongBankReport = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(frame.Bank, operand, frame.Pointer, "Kzan", "fixture", wrongBank, wrongBankReport);
        Require(wrongBankReport.Findings.Count == 1, "matching pointer in another bank cannot satisfy artwork");

        var projectileExports = new ResourceIndex();
        projectileExports.Add(ResourceDomains.EnemyProjectileProgram,
            ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, EnemyProjectileProgramFrameSources.Operand(PolypRockFrameDefinitions.ShownProjectileFrames[new(0)])));
        var boundProjectile = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(ResourceBanks.EnemyProjectilePrograms,
            EnemyProjectileProgramFrameSources.Operand(PolypRockFrameDefinitions.ShownProjectileFrames[new(0)]), null, "PolypRock", "fixture", projectileExports, boundProjectile);
        Require(boundProjectile.Findings.Count == 0, "installed projectile operands do not require a redundant pointer lookup");
        var missingProjectile = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(ResourceBanks.EnemyProjectilePrograms,
            EnemyProjectileProgramFrameSources.Operand(PolypRockFrameDefinitions.ShownProjectileFrames[new(0)]), null, "PolypRock", "fixture", new ResourceIndex(), missingProjectile);
        Require(missingProjectile.Findings.Count == 1, "missing projectile binding/selector is rejected");

        Console.WriteLine("Enemy visual audit contracts: owner discovery, missing selector/artwork, bank isolation, and projectile binding pass.");
    }

    /// <summary>Fails the audit fixture with its specific contract description.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Enemy visual audit contract failed: " + message);
    }
}
