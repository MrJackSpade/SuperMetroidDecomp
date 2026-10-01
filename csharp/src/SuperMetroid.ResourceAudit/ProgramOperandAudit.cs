using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Checks declared presentation operands against the compiled selector registry.
/// New program catalogs are discovered from their declaration contract, not from
/// a hand-maintained list of enemy kinds or paths reached during gameplay.
/// </summary>
internal sealed class ProgramOperandAudit(ResourceIndex exports, AuditReport report)
{
    private readonly HashSet<string> inspected = new(StringComparer.Ordinal);
    private int references;

    public void Inspect(ClassDeclarationSyntax declaration, SemanticModel semantic)
    {
        if (!declaration.Members.OfType<PropertyDeclarationSyntax>()
            .Any(property => property.Identifier.ValueText == "PresentationWordCount")) return;
        if (semantic.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol) return;
        string owner = symbol.ToDisplayString();
        if (!inspected.Add(owner)) return;
        string source = declaration.SyntaxTree.FilePath + ":" +
            (declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
        if (owner == typeof(MotherBrainRoomPaletteProgramDefinitions).FullName)
        {
            // This finite program selects installed color rows, not OAM operands.
            MotherBrainRoomFlashAudit.Inspect(source, exports, report);
            return;
        }
        if (!symbol.Name.EndsWith("InstructionProgramDefinitions", StringComparison.Ordinal) ||
            symbol.Name.Contains("Palette", StringComparison.Ordinal))
        {
            report.Gap(ResourceDomains.CompiledSelector, owner, source,
                "Presentation operand kind is not an enemy/sprite instruction selector; requires a domain-specific adapter.");
            return;
        }
        Type? metadata = typeof(RoomEnemySystem).Assembly.GetType(owner);
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        PropertyInfo? countProperty = metadata?.GetProperty("PresentationWordCount", flags);
        MethodInfo? addressMethod = metadata?.GetMethod("PresentationWordAddress", flags, [typeof(int)]);
        if (countProperty is null || addressMethod is null || addressMethod.ReturnType != typeof(ushort))
        {
            report.Gap(ResourceDomains.CompiledSelector, owner, source,
                "Presentation operand catalog has no recognized count/address metadata contract.");
            return;
        }
        int? bank = null;
        if (metadata!.GetField("Bank", flags) is { IsLiteral: true } bankField)
        {
            int value = Convert.ToInt32(bankField.GetRawConstantValue());
            bank = value <= byte.MaxValue ? value : value >> 16;
        }
        // Existing catalogs often encode their bank only in the source ownership
        // guard. Read its compiler-resolved constants, never probe possible banks
        // by calling gameplay code or infer a bank from a coincidentally matching ID.
        var guardBanks = new HashSet<int>();
        foreach (SyntaxReference syntax in symbol.DeclaringSyntaxReferences)
        {
            var part = (ClassDeclarationSyntax)syntax.GetSyntax();
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
                        (value & 0xffff) == 0 && value >> 16 is >= 0x80 and <= 0xbf)
                        guardBanks.Add(value >> 16);
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
        int count = (int)countProperty.GetValue(null)!;
        for (int index = 0; index < count; index++)
        {
            ushort address = (ushort)addressMethod.Invoke(null, [index])!;
            if (bank.Value == ResourceBanks.EnemyProjectilePrograms)
                ProjectileDefinitionAudit.RequireFrame(address, owner, source, exports, report);
            else
                report.Require(ResourceDomains.CompiledSelector, owner,
                    ResourceIndex.Address(bank.Value, address), source, exports);
            references++;
        }
    }

    public void Complete() => report.Coverage.Add(new("compiled-presentation-operands", references,
        exports.Count(ResourceDomains.CompiledSelector) + exports.Count(ResourceDomains.EnemyProjectileProgram) +
        exports.Count(ResourceDomains.EnemyProjectileSprite)));
}
