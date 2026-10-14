using SuperMetroid.Core.Hardware;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Programs;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>
/// Inventories immutable program definitions and their renderer dependencies without
/// executing an interpreter, loading a ROM, or sampling gameplay paths.
/// </summary>
internal static class EnemyVisualProgramAudit
{
    /// <summary>Records how many presentation operands a compiled instruction owner exposes.</summary>
    internal sealed record ProgramRow(int Operands);

    /// <summary>Inventories compiled enemy visual programs and writes their resource findings as JSON.</summary>
    internal static int Run(string root, string output)
    {
        var report = new AuditReport();
        ResourceIndex exports = DefinitionAudit.CollectEnemyVisuals(root, report);
        CSharpCompilation compilation = ConsumerAudit.CreateCompilation(root);
        EnemyVisualProgramRoutingContracts.Inspect(compilation, report);
        var declarations = compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()).ToArray();
        HashSet<string> owners = DiscoverOwners(compilation);
        var rows = new List<ProgramRow>();
        foreach (string owner in owners.Order(StringComparer.Ordinal))
        {
            ClassDeclarationSyntax[] parts = declarations.Where(type => type.Identifier.ValueText == owner).ToArray();
            ClassDeclarationSyntax[] adapterParts = declarations.Where(type => type.Identifier.ValueText == owner + "Tooling").ToArray();
            // Owners live in their domain namespaces (#626); the declaration names its own.
            string? ownerNamespace = parts.Select(part => part.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString()).FirstOrDefault(name => name is not null);
            Type? type = ownerNamespace is null ? null : ConsumerAudit.ProgramOwnerType(ownerNamespace + "." + owner);
            if (type is null || parts.Length == 0)
            {
                report.Gap(ResourceDomains.CompiledSelector, owner, "source inventory", "Program has no matched compiled/source owner.");
                continue;
            }
            string source = parts[0].SyntaxTree.FilePath;
            var catalog = InstructionProgramCatalog.Of(type);
            int? bank = ResolveBank(catalog, [.. parts, .. adapterParts], compilation);
            var addresses = new HashSet<ushort>();
            string shape = "unrecognized";
            if (catalog.PresentationOperands is { } indexed)
            {
                shape = "indexed";
                addresses.UnionWith(indexed);
            }
            else if (catalog.SinglePresentationOperand is { } single)
            {
                shape = "single";
                addresses.Add(single);
            }
            else if (type == typeof(EnemyProjectileInstructionMechanicsDefinitions))
            {
                shape = "typed-projectile-frames";
                foreach (var frame in EnemyProjectileInstructionMechanicsDefinitions.VisualFrames)
                    addresses.Add(frame.OperandAddress);
            }
            else if (EnemyVisualProgramSpecializations.IsControlOnly(type))
                shape = "control-only";

            Dictionary<ushort, ushort> words = ReadWords(catalog);
            // A compiled duration followed by a hole (including at the catalog end)
            // independently exposes interleaved operands, including missing declarations.
            addresses.UnionWith(InterleavedOperands(words));

            if (shape == "unrecognized")
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "No explicit presentation disposition for this program shape.");
            if (words.Count == 0)
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "No independently enumerable mechanics-word domain.");
            if (bank is null)
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "Program bank has no unambiguous source declaration.");
            else
                foreach (ushort operand in addresses.Order())
                    Require(type, (byte)bank.Value, operand, source, exports, report);
            rows.Add(new(addresses.Count));
        }
        EnemyVisualTypedProgramAudit.Inspect(exports, report, rows);
        report.Sort();
        string destination = Path.GetFullPath(output);
        Directory.CreateDirectory((Path.GetDirectoryName(destination) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(destination).")));
        File.WriteAllText(destination, JsonSerializer.Serialize(new { programs = rows, audit = report }, AuditReport.JsonOptions));
        Console.WriteLine($"Enemy visual static audit: {rows.Count} program owners, {rows.Sum(row => row.Operands)} visual references; {report.MissingResourceCount} missing resources, {report.UnresolvedCount} unresolved. No ROM or gameplay.");
        foreach (AuditFinding finding in report.Findings.DistinctBy(item => (item.Owner, item.Resource, item.Message)))
            Console.WriteLine($"{finding.Code} {finding.Owner} {finding.Resource}: {finding.Message}");
        return report.Findings.Count == 0 ? 0 : 1;
    }

    /// <summary>Finds presentation operands immediately after duration words at gaps in compiled mechanics.</summary>
    internal static IEnumerable<ushort> InterleavedOperands(Dictionary<ushort, ushort> words) =>
        words.Where(word => word.Value < 0x8000 && !words.ContainsKey(unchecked((ushort)(word.Key + 2))))
            .Select(word => unchecked((ushort)(word.Key + 2)));

    /// <summary>Finds program definition types and interpreter dependencies, including renamed owners.</summary>
    internal static HashSet<string> DiscoverOwners(CSharpCompilation compilation)
    {
        var owners = new HashSet<string>(compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()).Select(type => type.Identifier.ValueText)
            .Where(name => name.EndsWith("InstructionProgramDefinitions", StringComparison.Ordinal)), StringComparer.Ordinal);
        // Seed independently from interpreter dependencies, including renamed types.
        foreach (var method in compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()).Where(method => method.Identifier.ValueText is
                "ReadEnemyInstructionMechanicsWord" or "ReadEnemyProjectileInstructionMechanicsWord"))
        {
            SemanticModel model = compilation.GetSemanticModel(method.SyntaxTree);
            foreach (var call in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol target)
                    throw new InvalidDataException("Unbound interpreter dependency: " + call);
                if (target.ContainingType.Name == nameof(RoomEnemySystem)) continue;
                if (target.Name.Contains("Read", StringComparison.Ordinal) &&
                    target.Name.Contains("Word", StringComparison.Ordinal)) owners.Add(target.ContainingType.Name);
            }
        }
        return owners;
    }

    /// <summary>Enumerates a program's compiled mechanics words.</summary>
    private static Dictionary<ushort, ushort> ReadWords(InstructionProgramCatalog catalog)
    {
        var result = new Dictionary<ushort, ushort>();
        foreach (var word in catalog.MechanicsWords ?? [])
            result.Add(word.Address, word.Value);
        return result;
    }

    /// <summary>Resolves a program's SNES bank from its declaration or unambiguous source guards.</summary>
    private static int? ResolveBank(InstructionProgramCatalog catalog, ClassDeclarationSyntax[] parts, CSharpCompilation compilation)
    {
        if (catalog.DeclaredBank is { } declared)
            return declared <= byte.MaxValue ? declared : SnesAddressMath.BankNumber(declared);
        var banks = new HashSet<int>();
        foreach (var part in parts)
        {
            SemanticModel model = compilation.GetSemanticModel(part.SyntaxTree);
            foreach (var guard in part.Members.OfType<MethodDeclarationSyntax>().Where(method =>
                method.Identifier.ValueText is "IsCompiledMechanicsByte" or "TryGetPresentationWord"))
            foreach (var comparison in guard.DescendantNodes().OfType<BinaryExpressionSyntax>().Where(expression =>
                expression.IsKind(SyntaxKind.EqualsExpression) || expression.IsKind(SyntaxKind.NotEqualsExpression)))
            foreach (var side in new[] { comparison.Left, comparison.Right })
                if (model.GetConstantValue(side) is { HasValue: true, Value: int value } &&
                    SnesAddressMath.OffsetInBank(value) == 0 && SnesAddressMath.BankNumber(value) is >= 0x80 and <= 0xbf)
                    banks.Add(SnesAddressMath.BankNumber(value));
        }
        return banks.Count == 1 ? banks.Single() : null;
    }

    /// <summary>Resolves a presentation operand through its program-specific selector and checks the exported resource.</summary>
    private static void Require(Type type, byte bank, ushort operand, string source, ResourceIndex exports, AuditReport report)
    {
        string owner = type.Name;
        if (bank == ResourceBanks.EnemyProjectilePrograms)
        {
            ushort? direct = SkreeMetareeParticleVisualDefinitions.TryResolve(operand, out ushort particle)
                ? particle
                : CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort target) ? target : null;
            RequireResolvedOperand(bank, operand, direct, owner, source, exports, report);
            return;
        }
        ushort pointer;
        if (!CompiledEnemyVisualSelectors.TryGet(bank, operand, out pointer))
        {
            RequireResolvedOperand(bank, operand, null, owner, source, exports, report);
            return;
        }
        RequireResolvedOperand(bank, operand, pointer, owner, source, exports, report);
    }

    /// <summary>Records a selected visual resource, or reports an operand with no installed selector.</summary>
    internal static void RequireResolvedOperand(byte bank, ushort operand, ushort? pointer, string owner,
        string source, ResourceIndex exports, AuditReport report)
    {
        string key = ResourceIndex.Address(bank, operand);
        if (bank == ResourceBanks.EnemyProjectilePrograms)
            ProjectileDefinitionAudit.RequireFrame(operand, pointer, owner, source, exports, report);
        else if (pointer is ushort selected)
            report.Require(ResourceDomains.EnemyDisplay, owner, ResourceIndex.Address(bank, selected), source, exports);
        else
        {
            report.ReferenceCount++;
            report.Findings.Add(new(AuditReport.Missing, ResourceDomains.CompiledSelector, owner, key, source,
                "Instruction visual operand has no compiled selector or specialized resolver."));
        }
        report.Consumers.Add(new(owner, source,             pointer is ushort target ? ResourceIndex.Address(bank == ResourceBanks.EnemyProjectilePrograms
                ? ResourceBanks.ProjectileOamAndPaletteFx : bank, target) : "installed operand binding or missing selector"));
    }
}
