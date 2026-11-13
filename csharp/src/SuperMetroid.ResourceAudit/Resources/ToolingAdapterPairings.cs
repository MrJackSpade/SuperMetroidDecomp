using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Animation;
using SuperMetroid.Tooling;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// The development-tool program-catalog adapters of a compilation, each paired with the shipped program it describes
/// through its <see cref="ToolingForAttribute"/> (#1732), never through its name. An adapter is a class implementing a
/// catalog contract the program audits read (<see cref="InstructionProgramCatalog"/>'s contracts); one with no pairing is
/// listed so the audit fails on it rather than auditing it under a guessed owner.
/// </summary>
internal sealed class ToolingAdapterPairings
{
    /// <summary>The catalog contracts whose implementers are program adapters.</summary>
    private static readonly string[] Contracts =
    [
        ContractName(typeof(IInstructionProgramCatalog)), ContractName(typeof(IPresentationOperandCatalog)),
        ContractName(typeof(ISinglePresentationOperand)), ContractName(typeof(IDeclaredProgramBank)),
    ];

    /// <summary>The pairing attribute's full name.</summary>
    private static readonly string Pairing = ContractName(typeof(ToolingForAttribute));

    /// <summary>Each adapter, by the shipped program its pairing names.</summary>
    private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> byOwner = new(SymbolEqualityComparer.Default);

    /// <summary>Each adapter's shipped program.</summary>
    private readonly Dictionary<INamedTypeSymbol, INamedTypeSymbol> owners = new(SymbolEqualityComparer.Default);

    /// <summary>The adapters with no <see cref="ToolingForAttribute"/> pairing.</summary>
    private readonly List<INamedTypeSymbol> unpaired = [];

    /// <summary>Finds every program-catalog adapter declared in <paramref name="compilation"/> and pairs it.</summary>
    internal ToolingAdapterPairings(Compilation compilation)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel semantic = compilation.GetSemanticModel(tree);
            foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (semantic.GetDeclaredSymbol(declaration) is not INamedTypeSymbol type || !seen.Add(type) ||
                    !type.AllInterfaces.Any(contract => Contracts.Contains(contract.ToDisplayString(), StringComparer.Ordinal)))
                    continue;
                AttributeData? pairing = type.GetAttributes().SingleOrDefault(attribute =>
                    attribute.AttributeClass?.ToDisplayString() == Pairing);
                if (pairing?.ConstructorArguments.SingleOrDefault().Value is not INamedTypeSymbol owner)
                {
                    unpaired.Add(type);
                    continue;
                }
                owners.Add(type, owner);
                if (!byOwner.TryGetValue(owner, out List<INamedTypeSymbol>? adapters))
                    byOwner.Add(owner, adapters = []);
                adapters.Add(type);
            }
        }
    }

    /// <summary>The shipped programs some adapter describes.</summary>
    internal IEnumerable<INamedTypeSymbol> Owners => byOwner.Keys;

    /// <summary>The adapters with no <see cref="ToolingForAttribute"/> pairing, which the audit fails on.</summary>
    internal IReadOnlyList<INamedTypeSymbol> Unpaired => unpaired;

    /// <summary>Whether <paramref name="type"/> is a program-catalog adapter, paired or not.</summary>
    internal bool IsAdapter(INamedTypeSymbol type) => owners.ContainsKey(type) || unpaired.Contains(type, SymbolEqualityComparer.Default);

    /// <summary>The shipped program <paramref name="adapter"/>'s pairing names, or null when it has none.</summary>
    internal INamedTypeSymbol? OwnerOf(INamedTypeSymbol adapter) => owners.GetValueOrDefault(adapter);

    /// <summary>The source declarations of every adapter paired with <paramref name="owner"/>.</summary>
    internal IEnumerable<ClassDeclarationSyntax> AdapterDeclarations(INamedTypeSymbol owner) =>
        byOwner.GetValueOrDefault(owner)?.SelectMany(adapter => adapter.DeclaringSyntaxReferences)
            .Select(reference => (ClassDeclarationSyntax)reference.GetSyntax()) ?? [];

    /// <summary>The source declarations of every adapter paired with the shipped program named <paramref name="owner"/>.</summary>
    internal IEnumerable<ClassDeclarationSyntax> AdapterDeclarations(string owner) =>
        byOwner.Where(pair => pair.Key.Name == owner).SelectMany(pair => AdapterDeclarations(pair.Key));

    /// <summary>The name Roslyn displays for <paramref name="type"/>, as the audits compare symbols by.</summary>
    private static string ContractName(Type type) =>
        type.FullName ?? throw new InvalidOperationException($"{type.Name} has no full name.");
}
