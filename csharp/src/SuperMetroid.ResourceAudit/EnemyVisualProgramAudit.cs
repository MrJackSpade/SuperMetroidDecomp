using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Inventories immutable program definitions and their renderer dependencies without
/// executing an interpreter, loading a ROM, or sampling gameplay paths.
/// </summary>
internal static class EnemyVisualProgramAudit
{
    internal sealed record ProgramRow(int Operands);

    internal static int Run(string root, string output)
    {
        EnemyVisualProgramSpecializations.GuardCustomLayouts(root);
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
            Type? type = ConsumerAudit.ProgramOwnerType("SuperMetroid.Core.Game." + owner);
            if (type is null || parts.Length == 0)
            {
                report.Gap(ResourceDomains.CompiledSelector, owner, "source inventory", "Program has no matched compiled/source owner.");
                continue;
            }
            string source = parts[0].SyntaxTree.FilePath;
            var catalog = InstructionProgramCatalog.Of(type);
            int? bank = ResolveBank(type, catalog, [.. parts, .. adapterParts], compilation);
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
            else if (type == typeof(DeadTourianCorpseInstructionProgramDefinitions))
            {
                shape = "corpse-programs";
                for (int i = 0; i < DeadTourianCorpseInstructionProgramDefinitions.ProgramCount; i++)
                    addresses.Add(DeadTourianCorpseInstructionProgramDefinitionsTooling.PresentationWordAddress(i));
            }
            else if (type == typeof(SkreeMetareeInstructionProgramDefinitions))
            {
                shape = "two-families";
                foreach (bool metaree in new[] { false, true })
                for (int i = 0; i < SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(metaree); i++)
                    addresses.Add(SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(metaree, i));
            }
            else if (type == typeof(CeresBabyInstructionProgramDefinitions))
            {
                shape = "typed-sprites-and-palettes";
                for (int i = 0; i < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount; i++)
                    addresses.Add(CeresBabyInstructionProgramDefinitions.SpritemapOperandAddress(i));
            }
            else if (type == typeof(EnemyProjectileInstructionMechanicsDefinitions))
            {
                shape = "typed-projectile-frames";
                foreach (var frame in EnemyProjectileInstructionMechanicsDefinitions.VisualFrames)
                    addresses.Add(frame.OperandAddress);
            }
            else if (EnemyVisualProgramSpecializations.TryOperands(type, addresses))
                shape = "specialized-layout";
            else if (EnemyVisualProgramSpecializations.IsReviewedControlOnly(type, root))
                shape = "control-only";

            Dictionary<ushort, ushort> words = ReadWords(type, catalog);
            // A compiled duration followed by a hole (including at the catalog end)
            // independently exposes interleaved operands, including missing declarations.
            addresses.UnionWith(InterleavedOperands(words));

            if (shape == "unrecognized")
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "No explicit presentation disposition for this program shape.");
            if (words.Count == 0 && !EnemyVisualProgramSpecializations.IsMotherBrain(type))
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "No independently enumerable mechanics-word domain.");
            if (bank is null)
                report.Gap(ResourceDomains.CompiledSelector, owner, source, "Program bank has no unambiguous source declaration.");
            else
                foreach (ushort operand in addresses.Order())
                    Require(type, (byte)bank.Value, operand, source, exports, report);
            rows.Add(new(addresses.Count));
        }
        EnemyVisualTypedProgramAudit.Inspect(root, compilation, exports, report, rows);
        report.Sort();
        string destination = Path.GetFullPath(output);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, JsonSerializer.Serialize(new { programs = rows, audit = report }, AuditReport.JsonOptions));
        Console.WriteLine($"Enemy visual static audit: {rows.Count} program owners, {rows.Sum(row => row.Operands)} visual references; {report.MissingResourceCount} missing resources, {report.UnresolvedCount} unresolved. No ROM or gameplay.");
        foreach (AuditFinding finding in report.Findings.DistinctBy(item => (item.Owner, item.Resource, item.Message)))
            Console.WriteLine($"{finding.Code} {finding.Owner} {finding.Resource}: {finding.Message}");
        return report.Findings.Count == 0 ? 0 : 1;
    }

    internal static IEnumerable<ushort> InterleavedOperands(Dictionary<ushort, ushort> words) =>
        words.Where(word => word.Value < 0x8000 && !words.ContainsKey(unchecked((ushort)(word.Key + 2))))
            .Select(word => unchecked((ushort)(word.Key + 2)));

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

    private static Dictionary<ushort, ushort> ReadWords(Type type, InstructionProgramCatalog catalog)
    {
        var result = new Dictionary<ushort, ushort>();
        if (type == typeof(MotherBrainBodyInstructionProgramDefinitions))
            return MotherBrainBodyInstructionProgramDefinitionsTooling.AllWords.ToDictionary(word => word.Address, word => word.Word);
        if (type == typeof(SkreeMetareeInstructionProgramDefinitions))
        {
            foreach (bool metaree in new[] { false, true })
            for (int i = 0; i < SkreeMetareeInstructionProgramDefinitionsTooling.MechanicsWordCount(metaree); i++)
            {
                var word = SkreeMetareeInstructionProgramDefinitions.MechanicsWord(metaree, i);
                result.Add(word.Address, word.Value);
            }
            return result;
        }
        if (type == typeof(MotherBrainHandBeamInstructionProgramDefinitions))
        {
            for (int i = 0; i < MotherBrainHandBeamInstructionProgramDefinitionsTooling.NativeWordCount; i++)
            {
                var word = MotherBrainHandBeamInstructionProgramDefinitionsTooling.NativeWord(i);
                result.Add(word.Address, word.Value);
            }
            return result;
        }
        foreach (var word in catalog.MechanicsWords ?? [])
            result.Add(word.Address, word.Value);
        return result;
    }

    private static int? ResolveBank(Type type, InstructionProgramCatalog catalog, ClassDeclarationSyntax[] parts, CSharpCompilation compilation)
    {
        if (EnemyVisualProgramSpecializations.IsMotherBrain(type)) return MotherBrainVisualDefinitions.Bank;
        if (catalog.DeclaredBank is { } declared)
            return declared <= byte.MaxValue ? declared : declared >> 16;
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
                    (value & 0xffff) == 0 && value >> 16 is >= 0x80 and <= 0xbf)
                    banks.Add(value >> 16);
        }
        return banks.Count == 1 ? banks.Single() : null;
    }

    private static void Require(Type type, byte bank, ushort operand, string source, ResourceIndex exports, AuditReport report)
    {
        string owner = type.Name;
        if (bank == ResourceBanks.EnemyProjectilePrograms)
        {
            ushort? direct = operand switch
            {
                SkreeMetareeParticleVisualDefinitions.SkreeOperand => SkreeMetareeParticleVisualDefinitions.SkreeComposition,
                SkreeMetareeParticleVisualDefinitions.MetareeOperand => SkreeMetareeParticleVisualDefinitions.MetareeComposition,
                _ => CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort target) ? target : null,
            };
            RequireResolvedOperand(bank, operand, direct, owner, source, exports, report);
            return;
        }
        ushort pointer;
        if (EnemyVisualProgramSpecializations.TryResolve(type, operand, out pointer)) { }
        else if (type == typeof(DeadTourianCorpseInstructionProgramDefinitions))
            pointer = DeadTourianCorpseVisualDefinitions.FrameAt(operand);
        else if (type == typeof(SkreeMetareeInstructionProgramDefinitions))
            pointer = EnemySpritemapDefinitions.SkreeMetareeFrameAt(
                Enumerable.Range(0, SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(true))
                    .Any(i => SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(true, i) == operand), operand);
        else if (!CompiledEnemyVisualSelectors.TryGet(bank, operand, out pointer))
        {
            RequireResolvedOperand(bank, operand, null, owner, source, exports, report);
            return;
        }
        RequireResolvedOperand(bank, operand, pointer, owner, source, exports, report);
    }

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
