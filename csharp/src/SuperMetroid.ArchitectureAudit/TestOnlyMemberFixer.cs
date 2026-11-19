using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.SourceLayout;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// The test-only-member census's fix for its <c>narrow</c> findings (#2570): each member, or accessor, production's own
/// type reaches but only checks reach from outside it becomes private, and its owner's generated
/// <c>&lt;Owner&gt;PrivateAccess</c> class in test-support names it as the owner's own code does, reaching the private
/// member through <c>PrivateState</c>, so checks keep their call syntax. C# binds a check's name to that extension member
/// because the private member is no longer accessible to it.
/// <para>
/// A finding the fix cannot make mechanically is listed for a person: a member production does not need (<c>unused</c>
/// or <c>unneeded</c>, which leaves production instead), a generated declaration (its generator is fixed), and a member
/// whose shape no extension member can stand in for (an indexer, an event, a generic member or owner, a constructor, a
/// positional record property, a required member, a <c>ref struct</c> in its signature, a field sharing its declaration
/// with one that stays). Every edit is computed and validated before any file is written.
/// </para>
/// </summary>
internal static class TestOnlyMemberFixer
{
    /// <summary>Fully qualified names with nullable reference annotations, which the generated code writes every type with.</summary>
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Fully qualified names without nullable reference annotations, which <c>typeof</c> takes.</summary>
    private static readonly SymbolDisplayFormat TypeOfFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    /// <summary>Makes every mechanical narrow finding private with its shim, and lists the rest; returns the exit code.</summary>
    public static int Fix(LoadedSolution solution)
    {
        var findings = TestOnlyMembers.Collect(solution);
        var manual = new List<string>();
        var edits = new Dictionary<SyntaxTree, List<TextChange>>();
        var shims = new Dictionary<string, OwnerShims>(StringComparer.Ordinal);
        int fixedMembers = 0;
        foreach (var member in findings.GroupBy(finding => finding.Uses.Member, SymbolEqualityComparer.Default)
                     .OrderBy(group => group.Key!.ToDisplayString(), StringComparer.Ordinal))
        {
            ISymbol symbol = member.Key!;
            var ways = member.ToList();
            string? refusal = Refusal(symbol, ways);
            if (refusal is not null)
            {
                manual.Add($"MANUAL {Describe(solution.RepositoryRoot, symbol)} ({string.Join(", ", ways.Select(way => $"{way.Uses.Way} {way.Shape}"))}): {refusal}");
                continue;
            }
            foreach (var (tree, change) in Narrow(symbol))
                Add(edits, tree, change);
            Shims(solution.RepositoryRoot, shims, symbol.ContainingType).Add(symbol, Written(ways));
            fixedMembers++;
        }

        var files = new List<(string Path, string Text)>();
        foreach (var (tree, changes) in edits)
        {
            SourceText before = tree.GetText();
            SourceText after = before.WithChanges(changes.OrderBy(change => change.Span.Start));
            int errorsBefore = tree.GetDiagnostics().Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            int errorsAfter = CSharpSyntaxTree.ParseText(after, (CSharpParseOptions)tree.Options).GetDiagnostics()
                .Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            if (errorsAfter != errorsBefore)
                throw new InvalidOperationException($"{tree.FilePath}: narrowing leaves {errorsAfter - errorsBefore} new syntax errors.");
            files.Add((tree.FilePath, after.ToString()));
        }
        foreach (OwnerShims owner in shims.Values)
            files.Add((owner.Path, owner.Render()));
        foreach (var (path, text) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: File.Exists(path) && HasByteOrderMark(path)));
        }
        foreach (string line in manual)
            Console.WriteLine(line);
        Console.WriteLine($"Narrowed {fixedMembers} members in {edits.Count} files with {shims.Count} private-access classes; " +
            $"{manual.Count} left for a person.");
        return 0;
    }

    /// <summary>Why a member's findings cannot be fixed mechanically, or null.</summary>
    private static string? Refusal(ISymbol symbol, List<TestOnlyMemberFinding> ways)
    {
        if (ways.FirstOrDefault(way => way.Shape != "narrow") is { } needless)
            return needless.Shape == "unused" ? "production never uses it: it leaves production" :
                "production does not need this way of reaching it: it leaves production";
        if (symbol.DeclaringSyntaxReferences.Any(reference => GeneratedSource.IsGenerated(reference.SyntaxTree, CancellationToken.None) ||
                reference.SyntaxTree.FilePath.Contains(".Generated.", StringComparison.OrdinalIgnoreCase)))
            return "generated declaration: fix its generator";
        INamedTypeSymbol owner = symbol.ContainingType;
        for (INamedTypeSymbol? type = owner; type is not null; type = type.ContainingType)
        {
            if (type.IsGenericType)
                return "generic owner";
            if (type.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                return "owner not visible to test-support";
        }
        if (symbol.DeclaringSyntaxReferences.Length != 1 && symbol is not IMethodSymbol { IsPartialDefinition: true })
            return "declared in several places";
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Constructor }:
                return "constructor: checks construct it through public behaviour or PrivateState.New";
            case IMethodSymbol { IsGenericMethod: true }:
                return "generic method";
            case IMethodSymbol { IsPartialDefinition: true } or IMethodSymbol { PartialImplementationPart: not null }:
                return "partial method";
            case IMethodSymbol method when method.Parameters.Any(parameter => parameter.Type.IsRefLikeType || parameter.Type.TypeKind == TypeKind.Pointer ||
                                                                          parameter.RefKind is RefKind.RefReadOnlyParameter) ||
                                           method.ReturnType.IsRefLikeType || method.ReturnsByRef || method.ReturnsByRefReadonly:
                return "ref struct, pointer or by-reference return in its signature";
            case IPropertySymbol { IsIndexer: true }:
                return "indexer";
            case IPropertySymbol property when property.Type.IsRefLikeType || property.ReturnsByRef || property.ReturnsByRefReadonly:
                return "ref struct or by-reference property";
            case IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true }:
                return "required member";
            case IPropertySymbol property when property.DeclaringSyntaxReferences[0].GetSyntax() is ParameterSyntax:
                return "positional record property";
            case IPropertySymbol property when !ways.Any(way => way.Uses.Way == "read") && property.GetMethod is not null &&
                                               property.SetMethod is null:
                return "property shape";
            case IPropertySymbol property when Partial(property, ways):
                return "one accessor stays wider: checks reach the other through PrivateState";
            case IFieldSymbol field when field.Type.IsRefLikeType || field.Type.TypeKind == TypeKind.Pointer || field.IsFixedSizeBuffer:
                return "ref struct, pointer or fixed buffer field";
            case IFieldSymbol field when field.DeclaringSyntaxReferences[0].GetSyntax() is VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax { Variables.Count: > 1 },
            }:
                return "field sharing its declaration";
            case IEventSymbol:
                return "event";
            case IMethodSymbol or IPropertySymbol or IFieldSymbol:
                return null;
            default:
                return $"{symbol.Kind}";
        }
    }

    /// <summary>
    /// Whether only one of a property's accessors becomes private while the other stays wider: a check reaching the
    /// private one would still bind to the property, which its own code keeps accessible, and not to the extension.
    /// </summary>
    private static bool Partial(IPropertySymbol property, List<TestOnlyMemberFinding> ways)
    {
        bool reads = ways.Any(way => way.Uses.Way == "read");
        bool writes = ways.Any(way => way.Uses.Way == "write");
        bool getterStays = property.GetMethod is { DeclaredAccessibility: not Accessibility.Private } && !reads;
        bool setterStays = property.SetMethod is { DeclaredAccessibility: not Accessibility.Private } && !writes;
        return getterStays || setterStays;
    }

    /// <summary>Whether any check writes the member: assigns it, increments it or passes it by reference.</summary>
    private static bool Written(List<TestOnlyMemberFinding> ways) =>
        ways.Any(way => way.Uses.Way == "write") || ways.SelectMany(way => way.Uses.CheckSites).Any(site =>
            site is SimpleNameSyntax name && TestOnlyMembers.IsWrite(name));

    /// <summary>The edits that make a member private: its own accessibility, and its accessors' modifiers dropped.</summary>
    private static IEnumerable<(SyntaxTree Tree, TextChange Change)> Narrow(ISymbol symbol)
    {
        SyntaxNode declaration = symbol.DeclaringSyntaxReferences[0].GetSyntax();
        MemberDeclarationSyntax member = declaration switch
        {
            VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } => field,
            MemberDeclarationSyntax declared => declared,
            _ => throw new InvalidOperationException($"{symbol.ToDisplayString()} is declared by a {declaration.Kind()}."),
        };
        SyntaxTree tree = member.SyntaxTree;
        foreach (TextChange change in Private(member.Modifiers, member))
            yield return (tree, change);
        if (member is PropertyDeclarationSyntax { AccessorList: { } accessors })
        {
            foreach (AccessorDeclarationSyntax accessor in accessors.Accessors)
                foreach (SyntaxToken modifier in accessor.Modifiers.Where(IsAccessibility))
                    yield return (tree, new TextChange(TextSpan.FromBounds(modifier.SpanStart, modifier.GetNextToken().SpanStart), ""));
        }
    }

    /// <summary>The edits that replace a declaration's accessibility modifiers with <c>private</c>.</summary>
    private static IEnumerable<TextChange> Private(SyntaxTokenList modifiers, MemberDeclarationSyntax member)
    {
        var accessibility = modifiers.Where(IsAccessibility).ToList();
        if (accessibility.Count == 0)
        {
            int at = modifiers.Count > 0 ? modifiers[0].SpanStart : member.ChildNodesAndTokens()
                .First(child => !child.IsKind(SyntaxKind.AttributeList)).SpanStart;
            yield return new TextChange(new TextSpan(at, 0), "private ");
            yield break;
        }
        yield return new TextChange(accessibility[0].Span, "private");
        foreach (SyntaxToken other in accessibility.Skip(1))
            yield return new TextChange(TextSpan.FromBounds(other.SpanStart, other.GetNextToken().SpanStart), "");
    }

    /// <summary>Whether a modifier states accessibility.</summary>
    private static bool IsAccessibility(SyntaxToken modifier) => modifier.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword or
        SyntaxKind.ProtectedKeyword or SyntaxKind.PrivateKeyword;

    /// <summary>Adds a change to a tree's edits.</summary>
    private static void Add(Dictionary<SyntaxTree, List<TextChange>> edits, SyntaxTree tree, TextChange change)
    {
        if (!edits.TryGetValue(tree, out var list))
            edits[tree] = list = [];
        if (list.Any(other => other.Span.OverlapsWith(change.Span) || other.Span == change.Span && change.Span.IsEmpty))
            throw new InvalidOperationException($"{tree.FilePath}: two narrowing edits overlap at {change.Span}.");
        list.Add(change);
    }

    /// <summary>The generated private-access class of <paramref name="owner"/>, created on first use.</summary>
    private static OwnerShims Shims(string root, Dictionary<string, OwnerShims> shims, INamedTypeSymbol owner)
    {
        string key = owner.ToDisplayString(TypeOfFormat);
        if (!shims.TryGetValue(key, out OwnerShims? shim))
            shims[key] = shim = new OwnerShims(owner, ShimPath(root, owner));
        return shim;
    }

    /// <summary>Where an owner's private-access class lives: its assembly's verification-access folder, mirroring its path.</summary>
    private static string ShimPath(string root, INamedTypeSymbol owner)
    {
        string file = Path.GetFullPath(owner.DeclaringSyntaxReferences[0].SyntaxTree.FilePath);
        string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
        string assembly = owner.ContainingAssembly.Name;
        string prefix = $"csharp/src/{assembly}/";
        if (!relative.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"{owner.ToDisplayString()} is declared outside {prefix}: {relative}.");
        string directory = Path.GetDirectoryName(relative[prefix.Length..])?.Replace('\\', '/') ?? "";
        string access = assembly switch
        {
            "SuperMetroid.Core" => "csharp/test-support/VerificationAccess.Core",
            "SuperMetroid.AssetExtraction" => "csharp/test-support/VerificationAccess.Core/AssetExtraction",
            "SuperMetroid.Diagnostics" => "csharp/test-support/VerificationAccess.Core/Diagnostics",
            "SuperMetroid.Desktop" => "csharp/test-support/VerificationAccess.Desktop",
            "SuperMetroid.Rendering.Direct3D11" => "csharp/test-support/VerificationAccess.Rendering.Direct3D11",
            _ => throw new InvalidOperationException($"{assembly} has no verification-access folder."),
        };
        return Path.Combine(root, access, directory, $"{ClassName(owner)}.cs");
    }

    /// <summary>The private-access class's name: the owner's name, with its containing types', and <c>PrivateAccess</c>.</summary>
    internal static string ClassName(INamedTypeSymbol owner)
    {
        var names = new List<string>();
        for (INamedTypeSymbol? type = owner; type is not null; type = type.ContainingType)
            names.Insert(0, type.Name);
        return string.Concat(names) + "PrivateAccess";
    }

    /// <summary>Whether a file starts with a UTF-8 byte-order mark.</summary>
    private static bool HasByteOrderMark(string path)
    {
        byte[] start = new byte[3];
        using FileStream stream = File.OpenRead(path);
        return stream.Read(start, 0, 3) == 3 && start is [0xEF, 0xBB, 0xBF];
    }

    /// <summary>A member's display name with the file and line of its declaration.</summary>
    private static string Describe(string root, ISymbol member)
    {
        Location location = member.Locations.First(location => location.IsInSource);
        return $"{Path.GetRelativePath(root, location.SourceTree!.FilePath).Replace('\\', '/')}:{location.GetLineSpan().StartLinePosition.Line + 1}: " +
            member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }

    /// <summary>A type as generated code writes it.</summary>
    internal static string TypeText(ITypeSymbol type) => type.ToDisplayString(TypeFormat);

    /// <summary>A type as <c>typeof</c> takes it.</summary>
    internal static string TypeOfText(ITypeSymbol type) => type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(TypeOfFormat);
}
