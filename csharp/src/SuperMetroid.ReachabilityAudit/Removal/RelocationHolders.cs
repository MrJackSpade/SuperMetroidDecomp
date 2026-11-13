using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Chooses the class each owner's relocated members go into. The target directory is compiled into its projects with
/// every other source those projects compile (test-support shared through <c>.projitems</c> among them), so a new class
/// named like one those projects already declare in the same namespace would not compile: the 2026-11-13 cleanup at
/// a7f8ec16e6 created VerificationAccess.Core's <c>SamusProjectileListLayoutAccess</c> beside the one
/// RepositoryInstallation.Verification declares (CS0101). Each candidate name, the preferred one and then the preferred
/// one followed by 2, 3 and so on, is tried in turn. A name no project compiling the target knows is used for a new
/// class. A name those projects know is merged into when it is one compatible class that exactly the target's projects
/// compile, so the moved code sees what it would see in the target and reaches no project the target does not. Any other
/// known name is passed over.
/// </summary>
internal sealed class RelocationHolders
{
    /// <summary>Every loaded project.</summary>
    private readonly IReadOnlyList<ProjectCompilation> projects;
    /// <summary>The projects that compile a source file in the target directory.</summary>
    private readonly IReadOnlyList<ProjectCompilation> targetProjects;
    /// <summary>The full names of the classes chosen in this run, each with the request it was chosen for.</summary>
    private readonly Dictionary<string, string> chosen = new(StringComparer.Ordinal);
    /// <summary>The holder chosen for each owner and kind.</summary>
    private readonly Dictionary<string, RelocationHolder> resolved = new(StringComparer.Ordinal);

    /// <summary>Finds the projects that compile the target directory.</summary>
    /// <exception cref="InvalidOperationException">No project compiles a file in the target directory.</exception>
    public RelocationHolders(LoadedSolution solution, string targetDirectory)
    {
        string prefix = Path.GetFullPath(targetDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        projects = solution.Projects;
        targetProjects = [.. projects.Where(project => project.Compilation.SyntaxTrees.Any(tree =>
            tree.FilePath.Length > 0 && Path.GetFullPath(tree.FilePath).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))];
        if (targetProjects.Count == 0)
            throw new InvalidOperationException($"No project compiles a file in {targetDirectory}, so code moved there would compile nowhere.");
    }

    /// <summary>
    /// The relocation's holder step: the class each owner's moved members of each kind go into, chosen before any reference
    /// names it. An owner implementing a moved interface gets an abstract adapter, and a members class of forwarders even
    /// when only instance members move.
    /// </summary>
    /// <param name="solution">The loaded solution.</param>
    /// <param name="moves">The declarations that move.</param>
    /// <param name="targetDirectory">The directory moved code goes into.</param>
    /// <param name="style">How moved members are packaged.</param>
    /// <param name="identity">Gives each source file its repository-relative path.</param>
    /// <exception cref="InvalidOperationException">An extension block would go into an abstract adapter, which cannot declare one.</exception>
    public static RelocationHolders ForMoves(LoadedSolution solution, RelocationMoves moves, string targetDirectory, RelocationStyle style,
        SymbolIdentity identity)
    {
        var holders = new RelocationHolders(solution, targetDirectory);
        var holderRequests = moves.All.Where(m => m.Placement != RelocationPlacement.WholeType)
            .GroupBy(m => (Owner: moves.OwnerKey(m.AdapterOwner), Kind: m.HolderKind))
            .OrderBy(g => g.Key.Owner, StringComparer.Ordinal).ThenBy(g => g.Key.Kind).ToList();
        foreach (var request in holderRequests)
        {
            var owner = request.First().AdapterOwner;
            // An owner implementing a moved interface gets an abstract adapter, and only a static class declares extension blocks.
            if (request.Key.Kind == RelocationHolderKind.Members && moves.ImplementsMovedInterface(owner) &&
                request.FirstOrDefault(m => m.Symbol is INamedTypeSymbol { IsExtension: true }) is { } block)
                throw new InvalidOperationException($"{block.Symbol.ToDisplayString()} cannot move: {owner.ToDisplayString()} implements a " +
                    "moved interface, so its moved members go into an abstract adapter class, which cannot declare an extension block.");
            string directory = Path.Combine(targetDirectory, request
                .Select(m => Path.GetDirectoryName(RelocationSymbols.TargetRelativePath(identity.Relative(m.Tree.FilePath), style.KeepsProjectDirectory)) ?? "")
                .Order(StringComparer.Ordinal).First());
            holders.Resolve(owner, request.Key.Owner, request.Key.Kind,
                request.Key.Kind == RelocationHolderKind.Extensions ? style.ExtensionsName(owner) : style.AdapterName(owner),
                directory, request.Key.Kind == RelocationHolderKind.Members && moves.ImplementsMovedInterface(owner),
                [.. request.Select(m => m.Symbol)]);
            // An owner implementing a moved interface gets a members class of forwarders even when only instance members move.
            if (request.Key.Kind == RelocationHolderKind.Extensions && moves.ImplementsMovedInterface(owner))
                holders.Resolve(owner, request.Key.Owner, RelocationHolderKind.Members, style.AdapterName(owner), directory, true, []);
        }
        return holders;
    }

    /// <summary>The holder <see cref="Resolve"/> chose for the owner's members of this kind.</summary>
    /// <exception cref="InvalidOperationException">No holder was resolved for them.</exception>
    public RelocationHolder Of(string ownerKey, RelocationHolderKind kind) =>
        resolved.TryGetValue(Request(ownerKey, kind), out RelocationHolder? holder) ? holder
            : throw new InvalidOperationException($"No {kind} holder was resolved for {ownerKey}.");

    /// <summary>Chooses, once, the class that receives <paramref name="moved"/>, members of <paramref name="owner"/>.</summary>
    /// <param name="owner">The type the members leave; the holder is in its namespace.</param>
    /// <param name="ownerKey">The owner's repository key.</param>
    /// <param name="kind">Whether the holder takes static members or instance extensions.</param>
    /// <param name="preferredName">The name the relocation style gives the holder.</param>
    /// <param name="directory">The directory a new holder's file goes in.</param>
    /// <param name="abstractHolder">Whether a new members holder is an abstract class implementing moved interfaces.</param>
    /// <param name="moved">The members that move into the holder.</param>
    /// <exception cref="InvalidOperationException">The class merged into already declares a member that moves into it.</exception>
    public RelocationHolder Resolve(INamedTypeSymbol owner, string ownerKey, RelocationHolderKind kind, string preferredName,
        string directory, bool abstractHolder, IReadOnlyCollection<ISymbol> moved)
    {
        string request = Request(ownerKey, kind);
        if (resolved.TryGetValue(request, out RelocationHolder? known))
            return known;
        string ns = owner.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() + "." : "";
        for (int suffix = 1; ; suffix++)
        {
            string name = suffix == 1 ? preferredName : preferredName + suffix.ToString(CultureInfo.InvariantCulture);
            if (chosen.ContainsKey(ns + name))
                continue;
            var declared = targetProjects.Select(project => project.Compilation.GetTypesByMetadataName(ns + name)).ToList();
            RelocationHolder holder;
            if (declared.All(types => types.IsEmpty))
                holder = new RelocationHolder(name, Path.GetFullPath(Path.Combine(directory, name + ".cs")));
            else if (MergeTarget(declared, kind, abstractHolder, kind == RelocationHolderKind.Members && moved.Any(IsPartialMember)) is { } existing)
            {
                string path = Path.GetFullPath(existing.DeclaringSyntaxReferences[0].SyntaxTree.FilePath);
                RejectDeclaredMembers(existing, path, kind, moved);
                holder = new RelocationHolder(name, path);
            }
            else
                continue;
            chosen[ns + name] = request;
            resolved[request] = holder;
            return holder;
        }
    }

    /// <summary>The key of one owner's holder of one kind.</summary>
    private static string Request(string ownerKey, RelocationHolderKind kind) => $"{kind}:{ownerKey}";

    /// <summary>
    /// The class to merge into: one compatible class each target project declares itself, from one file that exactly the
    /// target's projects compile; or null when the name is declared any other way.
    /// </summary>
    private INamedTypeSymbol? MergeTarget(List<ImmutableArray<INamedTypeSymbol>> declared, RelocationHolderKind kind, bool abstractHolder,
        bool partialMembers)
    {
        var types = new List<INamedTypeSymbol>();
        for (int index = 0; index < targetProjects.Count; index++)
        {
            if (declared[index] is not [var type] ||
                !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, targetProjects[index].Compilation.Assembly) ||
                type.DeclaringSyntaxReferences.Length != 1 || !Compatible(type, kind, abstractHolder, partialMembers))
                return null;
            types.Add(type);
        }
        if (types.Select(type => Path.GetFullPath(type.DeclaringSyntaxReferences[0].SyntaxTree.FilePath))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList() is not [var path])
            return null;
        var compiling = projects.Where(project => project.Compilation.SyntaxTrees.Any(tree =>
                tree.FilePath.Length > 0 && string.Equals(Path.GetFullPath(tree.FilePath), path, StringComparison.OrdinalIgnoreCase)))
            .Select(project => project.Project.Id).ToHashSet();
        return compiling.SetEquals(targetProjects.Select(project => project.Project.Id)) ? types[0] : null;
    }

    /// <summary>
    /// Whether an existing class can take the members as the tool would render them: a top-level non-generic class, static,
    /// or abstract and not static for a members holder implementing moved interfaces, and declared partial when partial
    /// members move into it (CS0751).
    /// </summary>
    private static bool Compatible(INamedTypeSymbol type, RelocationHolderKind kind, bool abstractHolder, bool partialMembers) =>
        type is { TypeKind: TypeKind.Class, IsGenericType: false, ContainingType: null } &&
        (kind == RelocationHolderKind.Members && abstractHolder ? type is { IsAbstract: true, IsStatic: false } : type.IsStatic) &&
        (!partialMembers || type.DeclaringSyntaxReferences[0].GetSyntax() is ClassDeclarationSyntax declaration &&
            declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    /// <summary>Whether a moved symbol is a partial method or property, whose parts only a partial class declares.</summary>
    private static bool IsPartialMember(ISymbol symbol) => symbol is IMethodSymbol { IsPartialDefinition: true } or
        IMethodSymbol { PartialDefinitionPart: not null } or IPropertySymbol { IsPartialDefinition: true } or
        IPropertySymbol { PartialDefinitionPart: not null };

    /// <summary>
    /// Fails when the class merged into already declares a member that moves into it: two such members never compile. An
    /// extension block is compared by its members, each under its receiver, since a class may declare any number of blocks,
    /// several for one receiver among them.
    /// </summary>
    /// <exception cref="InvalidOperationException">A moved member's name, receiver and parameters are already declared.</exception>
    private static void RejectDeclaredMembers(INamedTypeSymbol holder, string path, RelocationHolderKind kind, IReadOnlyCollection<ISymbol> moved)
    {
        var present = holder.GetMembers().Where(member => member is not INamedTypeSymbol { IsExtension: true })
            .Select(member => (Receiver: "", Member: member))
            .Concat(holder.GetTypeMembers().Where(block => block.IsExtension)
                .SelectMany(block => block.GetMembers().Select(member => (Receiver: TypeText(block.ExtensionParameter?.Type), Member: member))))
            .ToList();
        foreach (ISymbol member in moved.SelectMany(symbol => symbol is INamedTypeSymbol { IsExtension: true } block ? block.GetMembers() : [symbol]))
        {
            string receiver = member.ContainingType is { IsExtension: true } block ? TypeText(block.ExtensionParameter?.Type)
                : kind == RelocationHolderKind.Extensions ? TypeText(member.ContainingType) : "";
            if (present.FirstOrDefault(entry => entry.Receiver == receiver && entry.Member.Name == member.Name &&
                    (entry.Member is not IMethodSymbol || member is not IMethodSymbol || Parameters(entry.Member) == Parameters(member))).Member
                is { } declared)
                throw new InvalidOperationException($"{holder.ToDisplayString()} in {path} already declares {declared.ToDisplayString()}, " +
                    $"so {member.ToDisplayString()} cannot move into it.");
        }
    }

    /// <summary>A type's fully qualified text, empty for none.</summary>
    private static string TypeText(ITypeSymbol? type) => type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "";

    /// <summary>A method's parameter types and kinds, fully qualified.</summary>
    private static string Parameters(ISymbol method) => string.Join(",", ((IMethodSymbol)method).Parameters
        .Select(parameter => $"{parameter.RefKind} {TypeText(parameter.Type)}"));
}
