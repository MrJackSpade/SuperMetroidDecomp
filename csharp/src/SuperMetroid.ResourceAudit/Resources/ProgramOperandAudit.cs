using SuperMetroid.AssetExtraction.Cartridge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Animation;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Checks declared presentation operands against the compiled selector registry.
/// New program catalogs are discovered from their declaration contract, not from
/// a hand-maintained list of enemy kinds or paths reached during gameplay.
/// </summary>
/// <param name="exports">The installed and compiled resources an operand must resolve to.</param>
/// <param name="report">The report each operand requirement and gap is recorded in.</param>
/// <param name="adapters">The compilation's program-catalog adapters, paired with their shipped programs by attribute.</param>
internal sealed class ProgramOperandAudit(ResourceIndex exports, AuditReport report, ToolingAdapterPairings adapters)
{
    /// <summary>Shipped program owners already audited across source parts and tooling adapters.</summary>
    private readonly HashSet<string> inspected = new(StringComparer.Ordinal);
    /// <summary>Number of presentation operands resolved for coverage reporting.</summary>
    private int references;

    /// <summary>Finds a program's operand contract and verifies each declared target has an exported visual resource.</summary>
    public void Inspect(ClassDeclarationSyntax declaration, SemanticModel semantic)
    {
        if (!declaration.Members.OfType<PropertyDeclarationSyntax>()
            .Any(property => property.Identifier.ValueText == "PresentationWordCount")) return;
        if (semantic.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol) return;
        string adapterSource = declaration.SyntaxTree.FilePath + ":" +
            (declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
        // The operand contract lives in a development adapter, which names the shipped program it describes through its
        // [ToolingFor] pairing (#1732); that pairing, not a type-name convention, makes it a sprite instruction selector.
        if (!adapters.IsAdapter(symbol))
        {
            report.Gap(ResourceDomains.CompiledSelector, symbol.ToDisplayString(), adapterSource,
                "Presentation operands are declared outside a program-catalog adapter; requires a domain-specific adapter.");
            return;
        }
        if (adapters.OwnerOf(symbol) is not { } shipped)
        {
            report.Gap(ResourceDomains.CompiledSelector, symbol.ToDisplayString(), adapterSource,
                "Program-catalog adapter has no [ToolingFor] pairing with the shipped program it describes.");
            return;
        }
        symbol = shipped;
        declaration = (ClassDeclarationSyntax)shipped.DeclaringSyntaxReferences[0].GetSyntax();
        semantic = semantic.Compilation.GetSemanticModel(declaration.SyntaxTree);
        string owner = symbol.ToDisplayString();
        if (!inspected.Add(owner)) return;
        string source = declaration.SyntaxTree.FilePath + ":" +
            (declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
        if (owner == typeof(MotherBrainRoomFlashDefinitions).FullName)
        {
            // This finite program selects installed color rows, not OAM operands.
            MotherBrainRoomFlashAudit.Inspect(source, exports, report);
            return;
        }
        if (symbol.Name.Contains("Palette", StringComparison.Ordinal))
        {
            report.Gap(ResourceDomains.CompiledSelector, owner, source,
                "Presentation operand kind is not an enemy/sprite instruction selector; requires a domain-specific adapter.");
            return;
        }
        Type? metadata = ConsumerAudit.ProgramOwnerType(owner);
        InstructionProgramCatalog? catalog = metadata is null ? null : InstructionProgramCatalog.Of(metadata);
        if (catalog?.PresentationOperands is not { } operands)
        {
            report.Gap(ResourceDomains.CompiledSelector, owner, source,
                "Presentation operand catalog has no recognized count/address metadata contract.");
            return;
        }
        int? bank = catalog.DeclaredBank is { } declared
            ? declared <= byte.MaxValue ? declared : SnesAddressMath.BankNumber(declared)
            : null;
        // Existing catalogs often encode their bank only in the source ownership
        // guard. Read its compiler-resolved constants, never probe possible banks
        // by calling gameplay code or infer a bank from a coincidentally matching ID.
        var guardBanks = new HashSet<int>();
        // The owner's ownership guard may live in its development adapter.
        IEnumerable<ClassDeclarationSyntax> adapterParts = adapters.AdapterDeclarations(symbol);
        foreach (var part in symbol.DeclaringSyntaxReferences.Select(reference => (ClassDeclarationSyntax)reference.GetSyntax())
                     .Concat(adapterParts))
        {
            SemanticModel partModel = semantic.Compilation.GetSemanticModel(part.SyntaxTree);
            foreach (var guard in part.Members.OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ValueText is "IsCompiledMechanicsByte" or "TryGetPresentationWord"))
            foreach (var comparison in guard.DescendantNodes().OfType<BinaryExpressionSyntax>())
            {
                if (!comparison.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression) &&
                    !comparison.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NotEqualsExpression)) continue;
                foreach (var side in new[] { comparison.Left, comparison.Right })
                {
                    if (partModel.GetConstantValue(side) is { HasValue: true, Value: int value } &&
                        SnesAddressMath.OffsetInBank(value) == 0 && SnesAddressMath.BankNumber(value) is >= 0x80 and <= 0xbf)
                        guardBanks.Add(SnesAddressMath.BankNumber(value));
                }
            }
        }
        if (bank is null && guardBanks.Count == 1) bank = guardBanks.Single();
        if (bank is null)
        {
            report.Gap(ResourceDomains.CompiledSelector, owner, source,
                "Program bank is not uniquely declared by a Bank constant or source ownership guard.");
            return;
        }
        foreach (ushort address in operands)
        {
            if (bank.Value == ResourceBanks.EnemyProjectilePrograms)
                ProjectileDefinitionAudit.RequireFrame(address, owner, source, exports, report);
            else
                report.Require(ResourceDomains.CompiledSelector, owner,
                    ResourceIndex.Address(bank.Value, address), source, exports);
            references++;
        }
    }

    /// <summary>Records the operand count against available selector and projectile exports.</summary>
    public void Complete() => report.Coverage.Add(new("compiled-presentation-operands", references,
        exports.Count(ResourceDomains.CompiledSelector) + exports.Count(ResourceDomains.EnemyProjectileProgram) +
        exports.Count(ResourceDomains.EnemyProjectileSprite)));
}
