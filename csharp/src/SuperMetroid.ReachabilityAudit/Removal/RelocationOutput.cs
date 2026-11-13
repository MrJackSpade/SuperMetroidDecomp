using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.Declarations;
using static SuperMetroid.ReachabilityAudit.Removal.RelocationSymbols;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The relocation's last step: renders each moved declaration, with the reference step's edits inside it, into the file
/// its holder or its path names (merging into a file an earlier run wrote), applies the edits outside moved spans in place,
/// removes the moved declarations and deletes the files left without a type.
/// </summary>
internal static class RelocationOutput
{
    /// <summary>Renders the moved declarations into their target files and writes every changed source file.</summary>
    /// <param name="moves">The declarations that move.</param>
    /// <param name="collected">The reference step's edits, to which the removal of each moved declaration is added.</param>
    /// <param name="holders">The class each owner's moved members go into.</param>
    /// <param name="trees">Every repository tree, once.</param>
    /// <param name="style">How moved members are packaged.</param>
    /// <param name="targetDirectory">The directory moved code goes into.</param>
    /// <param name="identity">Gives each source file its repository-relative path.</param>
    public static PlacedFiles Place(RelocationMoves moves, ReferenceEdits collected, RelocationHolders holders, IReadOnlyList<ProjectTree> trees,
        RelocationStyle style, string targetDirectory, SymbolIdentity identity)
    {
        var edits = collected.ByTree;
        var compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);
        var movedSpans = moves.MovedSpans;
        var movedInterfaces = moves.MovedInterfaces;
        // A source project's global usings do not follow its code; each moved declaration takes those it names.
        var globalNamespaces = trees.GroupBy(entry => entry.Compilation).ToDictionary(group => group.Key, group =>
            (IReadOnlySet<string>)group.SelectMany(entry => entry.Tree.GetCompilationUnitRoot().Usings)
                .Where(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) && u.Alias is null && u.StaticKeyword.IsKind(SyntaxKind.None) && u.Name is not null)
                .Select(u => u.Name!.ToString()).ToHashSet(StringComparer.Ordinal));
        var files = new SortedDictionary<string, TargetFile>(StringComparer.Ordinal);
        TargetFile FileFor(string path, RelocationMove move)
        {
            if (!files.TryGetValue(path, out var file))
                files[path] = file = new TargetFile([.. Usings(move.Tree)], Namespace(move.Tree), style);
            foreach (string name in GlobalNamespacesNamed(move, globalNamespaces[compilationOf[move.Tree]]))
                file.RequireUsing($"using {name};");
            return file;
        }
        foreach (var move in moves.All)
        {
            string text = Rewritten(move.Tree, move.Node.FullSpan, edits);
            string relative = TargetRelativePath(identity.Relative(move.Tree.FilePath), style.KeepsProjectDirectory);
            if (move.Placement == RelocationPlacement.WholeType)
            {
                FileFor(Path.Combine(targetDirectory, relative), move).Types.Add(text);
                continue;
            }
            // Static members and instance extensions live in separately named classes, each in its own file.
            var file = FileFor(holders.Of(moves.OwnerKey(move.AdapterOwner), move.HolderKind).Path, move);
            var adapter = file.Adapter(move.AdapterOwner, kind => holders.Of(moves.OwnerKey(move.AdapterOwner), kind).Name, movedInterfaces,
                implementation => moves.MoveOf(implementation) is not null);
            bool becomesExtension = move is { Placement: RelocationPlacement.ExtensionMember, ExtensionReceiver: null };
            if (IsPartialMember(move.Node))
            {
                if (becomesExtension)
                {
                    // An extension block cannot hold a partial member (CS0751): the two parts become one member, rendered once.
                    if (JoinedPartial(move, moves.All, edits) is not { } joined)
                        continue;
                    text = joined;
                }
                else
                    adapter.Partial = true;
            }
            text = RelocationModifiers.AsHolderMember(text, (CSharpParseOptions)move.Tree.Options, becomesExtension,
                keepsAccessibility: Unimplemented(move.Symbol));
            if (move.Placement == RelocationPlacement.ExtensionMember)
                adapter.Extensions.Add(new RelocatedExtension(move.ExtensionReceiver ?? adapter.SelfReceiver,
                    move.ExtensionReceiver is null ? text : Outdent(text)));
            else
                adapter.Members.Add(text);
        }

        // 5. Apply in-place edits, removing moved declarations.
        foreach (var group in moves.All.GroupBy(m => m.Tree))
            foreach (var move in group)
                collected.Add(move.Tree, move.Node.FullSpan, "");
        int deleted = 0;
        foreach (var (tree, list) in edits)
        {
            string path = Path.GetFullPath(tree.FilePath);
            string output = InPlace(tree, list, movedSpans.TryGetValue(tree, out var moved) ? moved : []);
            if (CSharpSyntaxTree.ParseText(output).GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Any()
                || CSharpSyntaxTree.ParseText(output).GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>().Any())
                File.WriteAllText(path, output, new UTF8Encoding(HasByteOrderMark(path)));
            else
            {
                File.Delete(path);
                deleted++;
            }
        }
        foreach (var (path, file) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                throw new InvalidOperationException($"{path} has no containing directory."));
            // A later pass adds to the adapters and files an earlier pass created.
            // A merge keeps the existing file's byte-order mark; new files get one.
            bool existing = File.Exists(path);
            File.WriteAllText(path, existing ? file.MergeInto(File.ReadAllText(path)) : file.Render(),
                new UTF8Encoding(!existing || HasByteOrderMark(path)));
        }
        return new PlacedFiles(files.Count, deleted);
    }

    /// <summary>
    /// A source tree's text with its edits applied in place: every edit outside the moved declarations, and the removal of
    /// each moved declaration (an empty edit of exactly its span).
    /// </summary>
    /// <param name="tree">The tree.</param>
    /// <param name="edits">The tree's edits, removals included.</param>
    /// <param name="movedSpans">The full spans of the declarations that move out of the tree.</param>
    /// <exception cref="InvalidOperationException">Two applied edits overlap.</exception>
    internal static string InPlace(SyntaxTree tree, IEnumerable<TextEdit> edits, IReadOnlyList<TextSpan> movedSpans)
    {
        var applied = edits.Where(e => e.Text == "" && movedSpans.Contains(e.Span) || !movedSpans.Any(s => s.Contains(e.Span)))
            .Distinct().OrderByDescending(e => e.Span.Start).ThenByDescending(e => e.Span.End).ToList();
        var text = new StringBuilder(tree.GetText().ToString());
        int floor = int.MaxValue;
        foreach (var (span, replacement) in applied)
        {
            if (span.End > floor)
                throw new InvalidOperationException($"Overlapping relocation edits in {Path.GetFullPath(tree.FilePath)} at {span}.");
            text.Remove(span.Start, span.Length).Insert(span.Start, replacement);
            floor = span.Start;
        }
        return text.ToString();
    }

    /// <summary>Whether a source file starts with a UTF-8 byte-order mark, which rewriting it keeps.</summary>
    private static bool HasByteOrderMark(string path) => File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];

    /// <summary>Applies reference edits inside a moved declaration from the end to preserve original spans.</summary>
    private static string Rewritten(SyntaxTree tree, TextSpan span, Dictionary<SyntaxTree, List<TextEdit>> edits)
    {
        var text = new StringBuilder(tree.GetText().ToString(span));
        if (edits.TryGetValue(tree, out var list))
            foreach (var (editSpan, replacement) in list.Where(e => span.Contains(e.Span) && e.Span != span).Distinct()
                .OrderByDescending(e => e.Span.Start))
                text.Remove(editSpan.Start - span.Start, editSpan.Length).Insert(editSpan.Start - span.Start, replacement);
        return text.ToString();
    }

    /// <summary>Removes the one indentation level an extension block added, so the member re-indents like a type member.</summary>
    private static string Outdent(string text) =>
        string.Join("\n", text.Replace("\r\n", "\n").Split('\n').Select(l => l.StartsWith("    ", StringComparison.Ordinal) ? l[4..] : l));

    /// <summary>Whether a declaration is one part of a partial method, property, indexer or event.</summary>
    private static bool IsPartialMember(SyntaxNode node) =>
        node is MethodDeclarationSyntax or BasePropertyDeclarationSyntax && ((MemberDeclarationSyntax)node).Modifiers.Any(SyntaxKind.PartialKeyword);

    /// <summary>
    /// The text of an instance partial method or property that becomes an extension member, which cannot be partial (CS0751):
    /// one member, the implementation's declaration under the implementation's documentation, or the definition's when the
    /// implementation has none. Returns the text for the implementation's move and null for the definition's, which it covers.
    /// </summary>
    /// <exception cref="InvalidOperationException">The implementation is not repository source that moves with the definition.</exception>
    private static string? JoinedPartial(RelocationMove move, IReadOnlyList<RelocationMove> moves, Dictionary<SyntaxTree, List<TextEdit>> edits)
    {
        ISymbol? implementation = move.Symbol switch
        {
            IMethodSymbol method => method.PartialImplementationPart,
            IPropertySymbol property => property.PartialImplementationPart,
            _ => null,
        };
        RelocationMove? MoveDeclaring(ISymbol? part) => part?.DeclaringSyntaxReferences is [var reference]
            ? moves.FirstOrDefault(m => string.Equals(m.Tree.FilePath, reference.SyntaxTree.FilePath, StringComparison.OrdinalIgnoreCase) &&
                m.Node.Span == reference.Span)
            : null;
        if (MoveDeclaring(implementation) is not { } implementationMove || MoveDeclaring(move.Symbol) is not { } definitionMove)
            throw new InvalidOperationException($"{move.Symbol.ToDisplayString()} becomes an extension member, which cannot be partial " +
                $"(CS0751), and its two parts are not both repository source that moves ({Path.GetFileName(move.Tree.FilePath)}, " +
                $"line {move.Node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}).");
        if (move != implementationMove)
            return null;
        var documented = move.Node.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)) ? move : definitionMove;
        return Rewritten(documented.Tree, TextSpan.FromBounds(documented.Node.FullSpan.Start, documented.Node.SpanStart), edits) +
            Rewritten(move.Tree, TextSpan.FromBounds(move.Node.SpanStart, move.Node.FullSpan.End), edits);
    }

    /// <summary>Copies source using directives into a relocated target file.</summary>
    private static IReadOnlyList<string> Usings(SyntaxTree tree) =>
        [.. tree.GetCompilationUnitRoot().Usings.Select(u => u.ToString())];

    /// <summary>
    /// The source project's global-using namespaces whose types, attributes or extension methods the moved declaration
    /// names without qualification, other than those its own namespace already encloses.
    /// </summary>
    private static IEnumerable<string> GlobalNamespacesNamed(RelocationMove move, IReadOnlySet<string> globalNamespaces)
    {
        if (globalNamespaces.Count == 0)
            yield break;
        string own = Namespace(move.Tree);
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var name in move.Node.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.Parent is QualifiedNameSyntax qualified && qualified.Right == name ||
                name.Parent is MemberAccessExpressionSyntax access && access.Name == name && access.Expression is not (ThisExpressionSyntax or BaseExpressionSyntax) &&
                move.Model.GetSymbolInfo(name).Symbol is not IMethodSymbol { IsExtensionMethod: true } and not IMethodSymbol { ReducedFrom: not null })
                continue;
            INamedTypeSymbol? type = move.Model.GetSymbolInfo(name).Symbol switch
            {
                INamedTypeSymbol typeSymbol => typeSymbol,
                IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType,
                IMethodSymbol { ReducedFrom: { } extension } => extension.ContainingType,
                IMethodSymbol { IsExtensionMethod: true } extension => extension.ContainingType,
                _ => null,
            };
            while (type?.ContainingType is { } outer)
                type = outer;
            if (type?.ContainingNamespace is not { IsGlobalNamespace: false } containing)
                continue;
            string ns = containing.ToDisplayString();
            if (globalNamespaces.Contains(ns) && own != ns && !own.StartsWith(ns + ".", StringComparison.Ordinal))
                named.Add(ns);
        }
        foreach (string ns in named)
            yield return ns;
    }

    /// <summary>Retains a moved declaration's source namespace.</summary>
    private static string Namespace(SyntaxTree tree) =>
        tree.GetCompilationUnitRoot().Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";

    /// <summary>Accumulates whole types and per-owner adapters for one destination source file.</summary>
    private sealed class TargetFile(List<string> usings, string ns, RelocationStyle style)
    {
        /// <summary>Adds a using directive the moved code needs, once.</summary>
        public void RequireUsing(string directive)
        {
            if (!usings.Contains(directive))
                usings.Add(directive);
        }

        /// <summary>Whole declarations to append in this destination namespace.</summary>
        public List<string> Types { get; } = [];
        /// <summary>Adapters indexed by their original shipped owner.</summary>
        private readonly Dictionary<INamedTypeSymbol, AdapterClass> adapters = new(Comparer);

        /// <summary>Gets or creates the owner adapter, adding forwarders for retained interface contracts.</summary>
        /// <param name="owner">The type whose members move.</param>
        /// <param name="holderName">The name of the owner's class of each kind, as <see cref="RelocationHolders"/> chose it.</param>
        /// <param name="movedInterfaces">The interfaces that move whole.</param>
        /// <param name="moves">Whether a symbol moves.</param>
        public AdapterClass Adapter(INamedTypeSymbol owner, Func<RelocationHolderKind, string> holderName, HashSet<INamedTypeSymbol> movedInterfaces,
            Func<ISymbol, bool> moves)
        {
            if (adapters.TryGetValue(owner, out var adapter))
                return adapter;
            var implemented = owner.Interfaces.Select(i => i.OriginalDefinition).Where(movedInterfaces.Contains).ToList();
            adapters[owner] = adapter = new AdapterClass(owner,
                [.. implemented.Select(i => i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))], style, holderName);
            // Contract members gameplay still uses stay on the owner; the adapter forwards to them.
            foreach (var contract in implemented.SelectMany(i => i.GetMembers())
                         .Where(m => m is IPropertySymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary }))
                if (owner.FindImplementationForInterfaceMember(contract) is { } implementation &&
                    SymbolIdentity.Normalize(implementation) is { } normalized && !moves(normalized))
                    adapter.Members.Add(Forwarder(owner, implementation));
            return adapter;
        }

        /// <summary>Emits a static adapter method or property that forwards a retained interface contract.</summary>
        private static string Forwarder(INamedTypeSymbol owner, ISymbol implementation)
        {
            var format = SymbolDisplayFormat.MinimallyQualifiedFormat;
            string target = $"{TypeName(owner)}.{implementation.Name}";
            return implementation switch
            {
                IPropertySymbol property => $"    public static {property.Type.ToDisplayString(format)} {property.Name} => {target};",
                IMethodSymbol method => $"    public static {method.ReturnType.ToDisplayString(format)} {method.Name}(" +
                    string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString(format)} {p.Name}")) +
                    $") => {target}(" + string.Join(", ", method.Parameters.Select(p => p.Name)) + ");",
                _ => throw new InvalidOperationException($"Cannot forward {implementation.ToDisplayString()}."),
            };
        }

        /// <summary>Adds this file's usings, members and types to an existing target file.</summary>
        public string MergeInto(string existing)
        {
            var root = CSharpSyntaxTree.ParseText(existing).GetCompilationUnitRoot();
            var inserts = new List<TextInsertion>();
            var appended = new StringBuilder();
            var presentUsings = root.Usings.Select(u => u.ToString()).ToHashSet(StringComparer.Ordinal);
            string newUsings = string.Concat(usings.Where(u => !presentUsings.Contains(u)).Select(u => u + "\r\n"));
            if (newUsings.Length > 0)
                inserts.Add(new TextInsertion(root.Usings.Count > 0 ? root.Usings[^1].FullSpan.End : 0, newUsings));
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToDictionary(c => c.Identifier.Text, c => c);
            // Text goes in before a closing brace, at the start of its line, each line indented by the given prefix.
            void InsertBefore(SyntaxToken closeBrace, IEnumerable<string> members, string indent) =>
                inserts.Add(new TextInsertion(existing.LastIndexOf('\n', closeBrace.SpanStart - 1) + 1, string.Concat(members.Select(member =>
                    string.Join("\r\n", member.Trim('\r', '\n').Replace("\r\n", "\n").Split('\n')
                        .Select(line => line.Length == 0 ? line : indent + line)) + "\r\n"))));
            foreach (var adapter in adapters.Values)
            {
                if (adapter.Members.Count > 0)
                {
                    if (classes.TryGetValue(adapter.MembersName, out var membersClass))
                    {
                        // RelocationHolders merges partial members only into a partial class.
                        if (adapter.Partial && !membersClass.Modifiers.Any(SyntaxKind.PartialKeyword))
                            throw new InvalidOperationException($"{adapter.MembersName} is not partial, so the partial members moving into it cannot.");
                        InsertBefore(membersClass.CloseBraceToken, adapter.Members, "");
                    }
                    else
                        appended.Append("\r\n").Append(adapter.RenderMembers());
                }
                if (adapter.Extensions.Count == 0)
                    continue;
                if (!classes.TryGetValue(adapter.ExtensionsName, out var extensionsClass))
                {
                    appended.Append("\r\n").Append(adapter.RenderExtensions());
                    continue;
                }
                // Instance members join the class's block for their receiver, or open one.
                foreach (var receiver in adapter.Extensions.Select(e => e.Receiver).Distinct(StringComparer.Ordinal))
                {
                    var block = extensionsClass.Members.OfType<ExtensionBlockDeclarationSyntax>().FirstOrDefault(b =>
                        b.ParameterList?.Parameters is [var parameter] && parameter.ToString() == receiver);
                    if (block is not null)
                        InsertBefore(block.CloseBraceToken, adapter.ExtensionsFor(receiver), "    ");
                    else
                        InsertBefore(extensionsClass.CloseBraceToken, [adapter.ExtensionBlock(receiver)], "");
                }
            }
            foreach (string type in Types)
                appended.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            var output = new StringBuilder(existing);
            foreach (var (position, text) in inserts.OrderByDescending(i => i.Position))
                output.Insert(position, text);
            output.Append(appended);
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        /// <summary>Writes using directives, moved types, and generated adapters into a new target file.</summary>
        public string Render()
        {
            var output = new StringBuilder();
            var allUsings = usings.ToList();
            if (adapters.Values.Any(a => a.Interfaces.Count > 0) && !allUsings.Contains("using SuperMetroid.Tooling;"))
                allUsings.Add("using SuperMetroid.Tooling;");
            foreach (string u in allUsings) output.Append(u).Append("\r\n");
            if (allUsings.Count > 0) output.Append("\r\n");
            if (ns.Length > 0) output.Append("namespace ").Append(ns).Append(";\r\n");
            foreach (string type in Types)
                output.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            foreach (var adapter in adapters.Values.OrderBy(a => style.AdapterName(a.Owner), StringComparer.Ordinal))
            {
                if (adapter.Members.Count > 0)
                    output.Append("\r\n").Append(adapter.RenderMembers());
                if (adapter.Extensions.Count > 0)
                    output.Append("\r\n").Append(adapter.RenderExtensions());
            }
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }
    }

    /// <summary>Renders static and instance-extension members for one original owner.</summary>
    private sealed class AdapterClass(INamedTypeSymbol owner, List<string> interfaces, RelocationStyle style,
        Func<RelocationHolderKind, string> holderName)
    {
        /// <summary>The shipped type whose moved members this adapter holds.</summary>
        public INamedTypeSymbol Owner { get; } = owner;
        /// <summary>The class that holds the owner's moved static members.</summary>
        public string MembersName => holderName(RelocationHolderKind.Members);
        /// <summary>The class that holds the owner's moved instance members.</summary>
        public string ExtensionsName => holderName(RelocationHolderKind.Extensions);
        /// <summary>Moved interfaces implemented by this adapter through relocated members and forwarders.</summary>
        public List<string> Interfaces { get; } = interfaces;
        /// <summary>Static members, nested types, and retained-contract forwarders in the adapter.</summary>
        public List<string> Members { get; } = [];
        /// <summary>Whether a moved partial member is among <see cref="Members"/>, which only a partial class declares (CS0751).</summary>
        public bool Partial { get; set; }
        /// <summary>Instance methods and properties rendered into extension blocks, each with its receiver parameter.</summary>
        public List<RelocatedExtension> Extensions { get; } = [];

        /// <summary>The receiver of a moved instance member of the owner itself.</summary>
        public string SelfReceiver => TypeName(Owner) + " self";

        /// <summary>The moved members whose block takes <paramref name="receiver"/>.</summary>
        public List<string> ExtensionsFor(string receiver) =>
            [.. Extensions.Where(e => e.Receiver == receiver).Select(e => e.Text)];

        /// <summary>Emits the class holding the owner's moved static members.</summary>
        public string RenderMembers()
        {
            string ownerName = TypeName(Owner);
            var output = new StringBuilder("/// <summary>").Append(string.Format(style.MembersSummary, ownerName)).Append("</summary>\r\n");
            string partial = Partial ? "partial " : "";
            if (Interfaces.Count > 0)
                output.Append($"[ToolingFor(typeof({ownerName}))]\r\ninternal abstract {partial}class {MembersName} : {string.Join(", ", Interfaces)}\r\n");
            else
                output.Append($"internal static {partial}class {MembersName}\r\n");
            return output.Append("{\r\n").Append(string.Join("\r\n", Members.Select(m => m.Trim('\r', '\n')))).Append("\r\n}\r\n").ToString();
        }

        /// <summary>Emits the class holding the owner's moved instance members.</summary>
        public string RenderExtensions() =>
            new StringBuilder("/// <summary>").Append(string.Format(style.ExtensionsSummary, TypeName(Owner))).Append("</summary>\r\n")
                .Append($"internal static class {ExtensionsName}\r\n{{\r\n")
                .Append(string.Join("\r\n", Extensions.Select(e => e.Receiver).Distinct(StringComparer.Ordinal).Select(ExtensionBlock)))
                .Append("}\r\n").ToString();

        /// <summary>The documented block for one receiver holding its moved members, indented one level.</summary>
        public string ExtensionBlock(string receiver)
        {
            string ownerName = TypeName(Owner);
            var output = new StringBuilder("    /// <summary>").Append(string.Format(style.ExtensionBlockSummary, ownerName)).Append("</summary>\r\n")
                .Append($"    extension({receiver})\r\n    {{\r\n");
            foreach (string member in ExtensionsFor(receiver))
                output.Append(Indent(member.Trim('\r', '\n'))).Append("\r\n");
            return output.Append("    }\r\n").ToString();
        }

        /// <summary>Adds one indentation level to moved instance members inside an extension block.</summary>
        private static string Indent(string text) =>
            string.Join("\r\n", text.Replace("\r\n", "\n").Split('\n').Select(l => l.Length == 0 ? l : "    " + l));
    }
}
