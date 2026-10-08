using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Negative fixtures for #1166's requested static completeness gate.</summary>
internal static class EnemyVisualProgramAuditChecks
{
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
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var compilation = CSharpCompilation.Create("VisualAuditFixture", [syntax],
            platforms.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var owners = EnemyVisualProgramAudit.DiscoverOwners(compilation);
        Require(owners.SetEquals(["SingleInstructionProgramDefinitions", "UnknownInstructionProgramDefinitions", "RenamedCatalog"]),
            "single-operand, unknown and renamed interpreter owners must all be inventoried");
        var missingDeclarations = new Dictionary<ushort, ushort>
        {
            [KzanInstructionProgramDefinitions.Idle] = 1,
            [unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4))] = CommonEnemyInstructionCodes.Sleep,
        };
        Require(EnemyVisualProgramAudit.InterleavedOperands(missingDeclarations)
                .SequenceEqual([KzanInstructionProgramDefinitionsTooling.PresentationWord]),
            "mechanics gaps expose Kzan's operand even without its presentation declaration");
        missingDeclarations.Remove(unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4)));
        Require(EnemyVisualProgramAudit.InterleavedOperands(missingDeclarations)
                .SequenceEqual([KzanInstructionProgramDefinitionsTooling.PresentationWord]),
            "terminal frame operands cannot escape the independent inventory");

        var frame = SingleFrameEnemyVisualDefinitions.Kzan;
        ushort operand = KzanInstructionProgramDefinitionsTooling.PresentationWord;
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
        wrongBank.Add(ResourceDomains.EnemyDisplay, ResourceIndex.Address(SingleFrameEnemyVisualDefinitions.Polyp.Bank, frame.Pointer));
        var wrongBankReport = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(frame.Bank, operand, frame.Pointer, "Kzan", "fixture", wrongBank, wrongBankReport);
        Require(wrongBankReport.Findings.Count == 1, "matching pointer in another bank cannot satisfy artwork");

        var projectileExports = new ResourceIndex();
        projectileExports.Add(ResourceDomains.EnemyProjectileProgram,
            ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, PolypRockInstructionProgramDefinitions.PresentationWord));
        var boundProjectile = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(ResourceBanks.EnemyProjectilePrograms,
            PolypRockInstructionProgramDefinitions.PresentationWord, null, "PolypRock", "fixture", projectileExports, boundProjectile);
        Require(boundProjectile.Findings.Count == 0, "installed projectile operands do not require a redundant pointer lookup");
        var missingProjectile = new AuditReport();
        EnemyVisualProgramAudit.RequireResolvedOperand(ResourceBanks.EnemyProjectilePrograms,
            PolypRockInstructionProgramDefinitions.PresentationWord, null, "PolypRock", "fixture", new ResourceIndex(), missingProjectile);
        Require(missingProjectile.Findings.Count == 1, "missing projectile binding/selector is rejected");

        const string reviewed = "control-only\n";
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(reviewed)));
        Require(EnemyVisualProgramSpecializations.SourceMatches("control-only\r\n", hash), "line endings preserve a disposition");
        Require(!EnemyVisualProgramSpecializations.SourceMatches(reviewed + "new frame", hash), "changed custom/control-only source revokes its disposition");
        Console.WriteLine("Enemy visual audit contracts: owner discovery, missing selector/artwork, bank isolation, projectile binding, and disposition revocation pass.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Enemy visual audit contract failed: " + message);
    }
}
