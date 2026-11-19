using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// One owner's generated private-access class in test-support (#2570): an extension member for each private member
/// checks, or code relocated into test-support, name, of the same name and signature, reaching the member through
/// <c>PrivateState</c>. C# binds such a name to the extension member because the private member is not accessible there,
/// so checks keep their syntax and production keeps the member private. A mutable value type's members that write it
/// take their receiver by reference. A class already generated keeps its members and takes only the new ones.
/// </summary>
/// <param name="owner">The type whose private members the class reaches.</param>
/// <param name="path">The file the class is written to.</param>
internal sealed class PrivateAccessShims(INamedTypeSymbol owner, string path)
{
    /// <summary>Fully qualified names with nullable reference annotations, which the generated code writes every type with.</summary>
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>The verification-access folder of each shipped assembly, under the repository root.</summary>
    private static readonly Dictionary<string, string> AccessFolders = new(StringComparer.Ordinal)
    {
        ["SuperMetroid.Core"] = "csharp/test-support/VerificationAccess.Core",
        ["SuperMetroid.AssetExtraction"] = "csharp/test-support/VerificationAccess.Core/AssetExtraction",
        ["SuperMetroid.Diagnostics"] = "csharp/test-support/VerificationAccess.Core/Diagnostics",
        ["SuperMetroid.Desktop"] = "csharp/test-support/VerificationAccess.Desktop",
        ["SuperMetroid.Rendering.Direct3D11"] = "csharp/test-support/VerificationAccess.Rendering.Direct3D11",
    };

    /// <summary>The folders whose generated classes one imports file imports, each beside the projects compiling it.</summary>
    private static readonly string[] ImportFolders =
    [
        "csharp/test-support/VerificationAccess.Core", "csharp/test-support/VerificationAccess.Desktop",
        "csharp/test-support/VerificationAccess.Rendering.Direct3D11",
    ];

    /// <summary>The file each import folder imports its generated classes in.</summary>
    internal const string ImportsFileName = "PrivateAccessImports.cs";

    /// <summary>The instance members that read the owner, rendered, by key.</summary>
    private readonly List<RenderedMember> _reading = [];

    /// <summary>The instance members that write a mutable value-type owner, rendered, by key.</summary>
    private readonly List<RenderedMember> _writing = [];

    /// <summary>The static members, rendered, by key.</summary>
    private readonly List<RenderedMember> _static = [];

    /// <summary>The classic extension methods' receiver forms, rendered, by receiver type.</summary>
    private readonly SortedDictionary<string, List<RenderedMember>> _receivers = new(StringComparer.Ordinal);

    /// <summary>The file the class is written to.</summary>
    public string Path { get; } = path;

    /// <summary>The owner as generated code writes it.</summary>
    private string Owner => TypeText(owner);

    /// <summary>Whether the owner is a value type its members can change, whose writes need it by reference.</summary>
    private bool IsMutableValue => owner.IsValueType && !owner.IsReadOnly;

    /// <summary>
    /// The generated class of <paramref name="owner"/> in the repository at <paramref name="root"/>: in its assembly's
    /// verification-access folder, or, for code relocated into <paramref name="relocationTarget"/> when that folder is not
    /// under it and holds no such class yet, beside the relocated code, which the same projects compile.
    /// </summary>
    public static PrivateAccessShims For(string root, INamedTypeSymbol owner, string? relocationTarget = null)
    {
        string path = PathFor(root, owner);
        if (relocationTarget is null || File.Exists(path) ||
            System.IO.Path.GetFullPath(path).StartsWith(System.IO.Path.GetFullPath(relocationTarget) + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return new(owner, path);
        string file = System.IO.Path.GetRelativePath(root, System.IO.Path.GetFullPath(owner.DeclaringSyntaxReferences[0].SyntaxTree.FilePath)).Replace('\\', '/');
        string inAssembly = file[$"csharp/src/{owner.ContainingAssembly.Name}/".Length..];
        string directory = System.IO.Path.GetDirectoryName(inAssembly) ?? "";
        return new(owner, System.IO.Path.Combine(relocationTarget, directory, $"{ClassName(owner)}.cs"));
    }

    /// <summary>The type whose private-access class stands in for <paramref name="member"/>: its own, or, for a member of an extension block, the static class declaring the block.</summary>
    public static INamedTypeSymbol OwnerOf(ISymbol member) =>
        member.ContainingType is { TypeKind: TypeKind.Extension, ContainingType: { } declaring } ? declaring : member.ContainingType;

    /// <summary>
    /// Why no extension member can stand in for <paramref name="member"/>, or null: a constructor, an indexer, an event, a
    /// generic member or owner, an owner test-support cannot see, a <c>ref struct</c> or pointer in its signature.
    /// </summary>
    public static string? Refusal(ISymbol member)
    {
        if (member.ContainingType is { TypeKind: TypeKind.Extension } block && (block.IsGenericType || block.ExtensionParameter is not { RefKind: RefKind.None } ||
                member is not (IMethodSymbol { MethodKind: MethodKind.Ordinary, IsGenericMethod: false } or IPropertySymbol { IsIndexer: false })))
            return "extension-block member other than a method or property of a by-value receiver";
        for (INamedTypeSymbol? type = OwnerOf(member); type is not null; type = type.ContainingType)
        {
            if (type.IsGenericType)
                return "generic owner";
            if (type.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                return "owner not visible to test-support";
        }
        return member switch
        {
            IMethodSymbol { MethodKind: MethodKind.Constructor } => "constructor",
            IMethodSymbol { IsGenericMethod: true } => "generic method",
            IMethodSymbol { MethodKind: not MethodKind.Ordinary } => "not an ordinary method",
            IMethodSymbol method when method.Parameters.Any(parameter => parameter.Type.IsRefLikeType || parameter.Type.TypeKind == TypeKind.Pointer ||
                                                                      parameter.RefKind == RefKind.RefReadOnlyParameter) ||
                                       method.ReturnType.IsRefLikeType || method.ReturnsByRef || method.ReturnsByRefReadonly =>
                "ref struct, pointer or by-reference return in its signature",
            IPropertySymbol { IsIndexer: true } => "indexer",
            IPropertySymbol property when property.Type.IsRefLikeType || property.ReturnsByRef || property.ReturnsByRefReadonly =>
                "ref struct or by-reference property",
            IFieldSymbol field when field.Type.IsRefLikeType || field.Type.TypeKind == TypeKind.Pointer || field.IsFixedSizeBuffer =>
                "ref struct, pointer or fixed buffer field",
            IEventSymbol => "event",
            IMethodSymbol or IPropertySymbol or IFieldSymbol => null,
            _ => member.Kind.ToString(),
        };
    }

    /// <summary>Adds the extension member standing in for <paramref name="member"/>; <paramref name="written"/> when its stand-in is written.</summary>
    public void Add(ISymbol member, bool written)
    {
        if (member.ContainingType is { TypeKind: TypeKind.Extension, ExtensionParameter: { } receiver })
        {
            AddExtensionMember(member, receiver);
            return;
        }
        switch (member)
        {
            case IFieldSymbol field:
                AddField(field, written);
                break;
            case IPropertySymbol property:
                AddProperty(property, written);
                break;
            case IMethodSymbol method:
                AddMethod(method);
                break;
            default:
                throw new InvalidOperationException($"{member.ToDisplayString()} is a {member.Kind}, which no extension member stands in for.");
        }
    }

    /// <summary>
    /// A member of an extension block: the same member of a block on the same receiver, invoking the static method the
    /// block's member compiles to (the receiver first, a property's accessors as <c>get_</c> and <c>set_</c> methods).
    /// </summary>
    private void AddExtensionMember(ISymbol member, IParameterSymbol receiver)
    {
        string receiverType = TypeText(receiver.Type);
        if (!_receivers.TryGetValue(receiverType, out var members))
            _receivers[receiverType] = members = [];
        string receiverTypeOf = $"typeof({TypeOfText(receiver.Type)})";
        if (member is IPropertySymbol property)
        {
            string type = TypeText(property.Type);
            string get = $"PrivateState.InvokeStatic<{type}>(typeof({Owner}), \"get_{property.Name}\", [{receiverTypeOf}], new object?[] {{ self }})";
            string set = $"PrivateState.InvokeStatic(typeof({Owner}), \"set_{property.Name}\", [{receiverTypeOf}, typeof({TypeOfText(property.Type)})], new object?[] {{ self, value }})";
            members.Add(new($"property|{property.Name}", Summary($"The private extension property <c>{property.Name}</c>, reached by name.") +
                $"internal {type} {Name(property.Name)}\n{{\n" + (property.GetMethod is not null ? $"    get => {get};\n" : "") +
                (property.SetMethod is not null ? $"    set => {set};\n" : "") + "}"));
            return;
        }
        var method = (IMethodSymbol)member;
        var parameters = method.Parameters;
        string types = "[" + string.Join(", ", parameters.Select(parameter =>
            $"typeof({TypeOfText(parameter.Type)})" + (parameter.RefKind is RefKind.None or RefKind.In ? "" : ".MakeByRefType()")).Prepend(receiverTypeOf)) + "]";
        bool returns = !method.ReturnsVoid;
        string result = returns ? TypeText(method.ReturnType) : "void";
        string invoker = returns ? $"PrivateState.InvokeStatic<{result}>" : "PrivateState.InvokeStatic";
        string arguments = "new object?[] { " + string.Join(", ", parameters.Select(parameter => parameter.RefKind == RefKind.Out ? "null" : Name(parameter.Name)).Prepend("self")) + " }";
        string signature = string.Join(",", parameters.Select(parameter => $"{parameter.RefKind}:{TypeOfText(parameter.Type)}"));
        if (parameters.Any(parameter => parameter.RefKind is RefKind.Ref or RefKind.Out))
            throw new InvalidOperationException($"{method.ToDisplayString()} passes a parameter by reference, which an extension-block stand-in does not.");
        members.Add(new($"method|{method.Name}|{signature}", Summary($"The private extension method <c>{method.Name}</c>, invoked by name.") +
            $"internal {result} {Name(method.Name)}({ParameterList(parameters)}) =>\n    {invoker}(typeof({Owner}), \"{method.Name}\", {types}, {arguments});"));
    }

    /// <summary>A field: a property reading it, and writing it when its stand-in is written.</summary>
    private void AddField(IFieldSymbol field, bool written)
    {
        string type = TypeText(field.Type);
        string name = Name(field.Name);
        string doc = Summary($"The private field <c>{field.Name}</c>, reached by name.");
        bool settable = written && !field.IsConst;
        string key = $"property|{field.Name}";
        if (field.IsStatic || field.IsConst)
        {
            string get = $"PrivateState.StaticField<{type}>(typeof({Owner}), \"{field.Name}\")";
            _static.Add(new(key, settable
                ? $"{doc}internal static {type} {name}\n{{\n    get => {get};\n    set => PrivateState.SetStaticField(typeof({Owner}), \"{field.Name}\", value);\n}}"
                : $"{doc}internal static {type} {name} => {get};"));
            return;
        }
        if (owner.IsValueType)
        {
            string get = $"PrivateState.ValueField<{Owner}, {type}>(self, \"{field.Name}\")";
            if (settable && IsMutableValue)
                _writing.Add(new(key, $"{doc}internal {type} {name}\n{{\n    get => {get};\n    set => PrivateState.SetValueField(ref self, \"{field.Name}\", value);\n}}"));
            else
                _reading.Add(new(key, $"{doc}internal {type} {name} => {get};"));
            return;
        }
        string read = $"PrivateState.Field<{Owner}, {type}>(self, \"{field.Name}\")";
        _reading.Add(new(key, settable
            ? $"{doc}internal {type} {name}\n{{\n    get => {read};\n    set => PrivateState.SetField(self, \"{field.Name}\", value);\n}}"
            : $"{doc}internal {type} {name} => {read};"));
    }

    /// <summary>A property: a property of the same accessors, reaching them by name.</summary>
    private void AddProperty(IPropertySymbol property, bool written)
    {
        string type = TypeText(property.Type);
        string name = Name(property.Name);
        string doc = Summary($"The private property <c>{property.Name}</c>, reached by name.");
        bool settable = property.SetMethod is not null && (written || property.GetMethod is null);
        bool gettable = property.GetMethod is not null;
        string key = $"property|{property.Name}";
        string Accessors(string get, string set) =>
            (gettable ? $"    get => {get};\n" : "") + (settable ? $"    set => {set};\n" : "");
        if (property.IsStatic)
        {
            _static.Add(new(key, $"{doc}internal static {type} {name}\n{{\n" + Accessors(
                $"PrivateState.StaticGet<{type}>(typeof({Owner}), \"{property.Name}\")",
                $"PrivateState.StaticSet(typeof({Owner}), \"{property.Name}\", value)") + "}"));
            return;
        }
        if (owner.IsValueType)
        {
            settable &= IsMutableValue;
            (settable ? _writing : _reading).Add(new(key, $"{doc}internal {type} {name}\n{{\n" + Accessors(
                $"PrivateState.ValueGet<{Owner}, {type}>(self, \"{property.Name}\")",
                $"PrivateState.ValueSet(ref self, \"{property.Name}\", value)") + "}"));
            return;
        }
        _reading.Add(new(key, $"{doc}internal {type} {name}\n{{\n" + Accessors(
            $"PrivateState.Get<{Owner}, {type}>(self, \"{property.Name}\")",
            $"PrivateState.Set(self, \"{property.Name}\", value)") + "}"));
    }

    /// <summary>A method: a method of the same signature, invoking it by name with its parameter types.</summary>
    private void AddMethod(IMethodSymbol method)
    {
        string doc = Summary($"The private method <c>{method.Name}</c>, invoked by name.");
        var parameters = method.Parameters;
        string types = "[" + string.Join(", ", parameters.Select(parameter =>
            $"typeof({TypeOfText(parameter.Type)})" + (parameter.RefKind is RefKind.None or RefKind.In ? "" : ".MakeByRefType()"))) + "]";
        bool returns = !method.ReturnsVoid;
        string result = returns ? TypeText(method.ReturnType) : "void";
        string signature = string.Join(",", parameters.Select(parameter => $"{parameter.RefKind}:{TypeOfText(parameter.Type)}"));
        if (method.IsStatic)
        {
            string invoker = returns ? $"PrivateState.InvokeStatic<{result}>" : "PrivateState.InvokeStatic";
            // A classic extension method keeps its receiver form only: both forms lower to one static method, which a
            // class cannot declare twice.
            if (method.IsExtensionMethod && parameters.Length > 0 && parameters[0].RefKind == RefKind.None)
            {
                string receiverType = TypeText(parameters[0].Type);
                if (!_receivers.TryGetValue(receiverType, out var members))
                    _receivers[receiverType] = members = [];
                members.Add(new($"method|{method.Name}|{signature}", $"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters.Skip(1))})" +
                    Body(invoker, $"typeof({Owner})", types, method.Name, returns ? result : null, [.. parameters], selfArgument: "self")));
                return;
            }
            _static.Add(new($"method|{method.Name}|{signature}",
                $"{doc}internal static {result} {Name(method.Name)}({ParameterList(parameters)})" +
                Body(invoker, $"typeof({Owner})", types, method.Name, returns ? result : null, [.. parameters])));
            return;
        }
        if (owner.IsValueType)
        {
            string invoker = returns ? $"PrivateState.InvokeValue<{Owner}, {result}>" : $"PrivateState.InvokeValue<{Owner}>";
            // A method of a value type that cannot change it takes the value as it is; one that can, the value itself.
            (IsMutableValue && !method.IsReadOnly ? _writing : _reading).Add(new($"method|{method.Name}|{signature}",
                $"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters)})" +
                Body(invoker, "ref self", types, method.Name, returns ? result : null, [.. parameters])));
            return;
        }
        string instanceInvoker = returns ? $"PrivateState.Invoke<{Owner}, {result}>" : $"PrivateState.Invoke<{Owner}>";
        _reading.Add(new($"method|{method.Name}|{signature}", $"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters)})" +
            Body(instanceInvoker, "self", types, method.Name, returns ? result : null, [.. parameters])));
    }

    /// <summary>
    /// A method body invoking the private method: an expression body, or, when a parameter is passed by reference, a block
    /// that passes the arguments in an array and writes each by-reference one back from it.
    /// </summary>
    private static string Body(string invoker, string receiver, string types, string name, string? result, IReadOnlyList<IParameterSymbol> parameters,
        string? selfArgument = null)
    {
        string arguments = "new object?[] { " + string.Join(", ", parameters.Select((parameter, index) =>
            index == 0 && selfArgument is not null ? selfArgument : parameter.RefKind == RefKind.Out ? "null" : Name(parameter.Name))) + " }";
        if (parameters.All(parameter => parameter.RefKind is RefKind.None or RefKind.In))
            return $" =>\n    {invoker}({receiver}, \"{name}\", {types}, {arguments});";
        var body = new StringBuilder("\n{\n");
        body.Append(CultureInfo.InvariantCulture, $"    object?[] invocationArguments = {arguments};\n");
        body.Append(result is null ? "    " : $"    {result} invocationResult = ")
            .Append(CultureInfo.InvariantCulture, $"{invoker}({receiver}, \"{name}\", {types}, invocationArguments);\n");
        for (int index = 0; index < parameters.Count; index++)
            if (parameters[index].RefKind is RefKind.Ref or RefKind.Out)
                body.Append(CultureInfo.InvariantCulture, $"    {Name(parameters[index].Name)} = ({TypeText(parameters[index].Type)})invocationArguments[{index}]!;\n");
        if (result is not null)
            body.Append("    return invocationResult;\n");
        return body.Append('}').ToString();
    }

    /// <summary>A parameter list as the extension member declares it: attributes, kinds, types, names and defaults.</summary>
    private static string ParameterList(IEnumerable<IParameterSymbol> parameters) => string.Join(", ", parameters.Select(parameter =>
    {
        var text = new StringBuilder();
        foreach (AttributeData attribute in parameter.GetAttributes().Where(attribute =>
                     attribute.AttributeClass?.ContainingNamespace?.ToDisplayString() == "System.Diagnostics.CodeAnalysis"))
            text.Append('[').Append(attribute.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Append('(').Append(string.Join(", ", attribute.ConstructorArguments.Select(Literal))).Append(")] ");
        text.Append(RefText(parameter.RefKind));
        if (parameter.IsParams)
            text.Append("params ");
        text.Append(TypeText(parameter.Type)).Append(' ').Append(Name(parameter.Name));
        if (parameter.HasExplicitDefaultValue)
            text.Append(" = ").Append(Default(parameter));
        return text.ToString();
    }));

    /// <summary>The keyword a parameter of a reference kind is written with.</summary>
    private static string RefText(RefKind kind) => kind switch
    {
        RefKind.Ref => "ref ",
        RefKind.Out => "out ",
        RefKind.In => "in ",
        RefKind.RefReadOnlyParameter => "ref readonly ",
        RefKind.None => "",
        _ => throw new InvalidOperationException($"Unknown reference kind {kind}."),
    };

    /// <summary>A parameter's default value as source writes it.</summary>
    private static string Default(IParameterSymbol parameter)
    {
        object? value = parameter.ExplicitDefaultValue;
        if (value is null)
            return parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T ? "default" : "null";
        if (parameter.Type.TypeKind == TypeKind.Enum)
            return $"({TypeText(parameter.Type)})({Convert.ToString(value, CultureInfo.InvariantCulture)})";
        return value switch
        {
            bool flag => flag ? "true" : "false",
            string text => SymbolDisplay.FormatLiteral(text, quote: true),
            char character => SymbolDisplay.FormatLiteral(character, quote: true),
            _ => $"({TypeText(parameter.Type)})({Convert.ToString(value, CultureInfo.InvariantCulture)})",
        };
    }

    /// <summary>An attribute argument as source writes it.</summary>
    private static string Literal(TypedConstant constant) => constant.Value switch
    {
        null => "null",
        bool flag => flag ? "true" : "false",
        string text => SymbolDisplay.FormatLiteral(text, quote: true),
        var value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null",
    };

    /// <summary>A name as source writes it: a keyword with <c>@</c>.</summary>
    private static string Name(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) is SyntaxKind.FieldKeyword ? "@" + name : name;

    /// <summary>A member's documentation.</summary>
    private static string Summary(string text) => $"/// <summary>{text}</summary>\n";

    /// <summary>The class's source: the whole class, or, when it is generated already, its source with the new members added.</summary>
    public string Render()
    {
        if (File.Exists(Path))
            return Merged(File.ReadAllText(Path));
        var text = new StringBuilder();
        text.Append("using SuperMetroid.Core;\n\n");
        if (!owner.ContainingNamespace.IsGlobalNamespace)
            text.Append(CultureInfo.InvariantCulture, $"namespace {owner.ContainingNamespace.ToDisplayString()};\n\n");
        text.Append(CultureInfo.InvariantCulture, $"/// <summary>\n/// The private members of <see cref=\"{owner.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}\"/> checks name (#2570), each an extension member of the\n");
        text.Append("/// same name and signature reaching the private member through <see cref=\"PrivateState\"/>, so checks name them as the\n");
        text.Append("/// owner's own code does and production keeps them private. Generated by the architecture audit's\n");
        text.Append("/// <c>--fix-test-only-members</c> and the reachability prune.\n/// </summary>\n");
        text.Append(CultureInfo.InvariantCulture, $"internal static class {ClassName(owner)}\n{{\n");
        text.Append(string.Join("\n\n", Blocks(new HashSet<string>(StringComparer.Ordinal)).Select(block => Indent(block, "    "))));
        text.Append("\n}\n");
        return text.ToString().Replace("\n", "\r\n", StringComparison.Ordinal);
    }

    /// <summary>The generated class with every member it does not hold yet added before its closing brace.</summary>
    private string Merged(string existing)
    {
        CompilationUnitSyntax unit = CSharpSyntaxTree.ParseText(existing).GetCompilationUnitRoot();
        ClassDeclarationSyntax holder = unit.DescendantNodes().OfType<ClassDeclarationSyntax>().SingleOrDefault(type => type.Identifier.ValueText == ClassName(owner))
            ?? throw new InvalidOperationException($"{Path} holds no {ClassName(owner)}.");
        var held = new HashSet<string>(StringComparer.Ordinal);
        foreach (MemberDeclarationSyntax member in holder.DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            switch (member)
            {
                case PropertyDeclarationSyntax property:
                    held.Add($"property|{property.Identifier.ValueText.TrimStart('@')}");
                    break;
                case MethodDeclarationSyntax method:
                    held.Add($"method|{method.Identifier.ValueText.TrimStart('@')}|{method.ParameterList.Parameters.Count}");
                    break;
                default:
                    break;
            }
        }
        var blocks = Blocks(held);
        if (blocks.Count == 0)
            return existing;
        string newline = existing.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        int close = holder.CloseBraceToken.SpanStart;
        string added = "\n" + string.Join("\n\n", blocks.Select(block => Indent(block, "    "))) + "\n";
        return existing[..close].TrimEnd() + newline + added.Replace("\n", newline, StringComparison.Ordinal) + existing[close..];
    }

    /// <summary>The extension blocks of every member not in <paramref name="held"/>.</summary>
    private List<string> Blocks(HashSet<string> held)
    {
        bool New(RenderedMember member)
        {
            string[] parts = member.Key.Split('|');
            string key = parts[0] == "method" ? $"method|{parts[1]}|{(parts[2].Length == 0 ? 0 : parts[2].Split(',').Length)}" : member.Key;
            return !held.Contains(key);
        }
        var blocks = new List<string>();
        void Add(string header, IEnumerable<RenderedMember> members)
        {
            var fresh = members.Where(New).ToList();
            if (fresh.Count > 0)
                blocks.Add($"{header}\n{{\n{string.Join("\n\n", fresh.Select(member => Indent(member.Text, "    ")))}\n}}");
        }
        Add($"/// <summary>The private instance members checks read or call.</summary>\nextension({Owner} self)", _reading);
        Add($"/// <summary>The private instance members checks write, on the value itself.</summary>\nextension(ref {Owner} self)", _writing);
        Add($"/// <summary>The private static members checks name.</summary>\nextension({Owner})", _static);
        foreach (var (receiver, members) in _receivers)
            Add($"/// <summary>The private extension methods checks call on their receiver.</summary>\nextension({receiver} self)", members);
        return blocks;
    }

    /// <summary>Every line of <paramref name="text"/> indented.</summary>
    private static string Indent(string text, string by) => string.Join("\n", text.Split('\n').Select(line => line.Length == 0 ? line : by + line));

    /// <summary>
    /// Writes, in each verification-access folder of the repository at <paramref name="root"/>, the global static imports
    /// of its generated private-access classes, so a check names an owner's private member wherever it names the owner.
    /// </summary>
    public static void WriteImports(string root)
    {
        foreach (string folder in ImportFolders)
        {
            string directory = System.IO.Path.Combine(root, folder);
            if (!Directory.Exists(directory))
                continue;
            var imports = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string file in Directory.EnumerateFiles(directory, "*PrivateAccess.cs", SearchOption.AllDirectories))
            {
                CompilationUnitSyntax unit = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetCompilationUnitRoot();
                string space = unit.Members.OfType<BaseNamespaceDeclarationSyntax>().SingleOrDefault()?.Name.ToString()
                    ?? throw new InvalidOperationException($"{file} declares no namespace.");
                foreach (ClassDeclarationSyntax type in unit.DescendantNodes().OfType<ClassDeclarationSyntax>()
                             .Where(type => type.Identifier.ValueText.EndsWith("PrivateAccess", StringComparison.Ordinal)))
                    imports.Add($"global using static {space}.{type.Identifier.ValueText};");
            }
            string path = System.IO.Path.Combine(directory, ImportsFileName);
            if (imports.Count == 0)
            {
                if (File.Exists(path))
                    File.Delete(path);
                continue;
            }
            string text = string.Join("\r\n", new[]
            {
                "// The generated private-access classes (#2570), imported everywhere this folder is compiled: each names its owner's",
                "// private members as extension members, which a check reaches wherever it reaches the owner. The architecture audit's",
                "// --fix-test-only-members and the reachability prune write this file with the classes they generate.",
            }.Concat(imports)) + "\r\n";
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }
    }

    /// <summary>Where an owner's private-access class lives: its assembly's verification-access folder, mirroring its path.</summary>
    /// <exception cref="InvalidOperationException">The owner's assembly has no verification-access folder.</exception>
    public static string PathFor(string root, INamedTypeSymbol owner)
    {
        string file = System.IO.Path.GetFullPath(owner.DeclaringSyntaxReferences[0].SyntaxTree.FilePath);
        string relative = System.IO.Path.GetRelativePath(root, file).Replace('\\', '/');
        string assembly = owner.ContainingAssembly.Name;
        string prefix = $"csharp/src/{assembly}/";
        if (!relative.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"{owner.ToDisplayString()} is declared outside {prefix}: {relative}.");
        string directory = System.IO.Path.GetDirectoryName(relative[prefix.Length..])?.Replace('\\', '/') ?? "";
        if (!AccessFolders.TryGetValue(assembly, out string? access))
            throw new InvalidOperationException($"{assembly} has no verification-access folder.");
        return System.IO.Path.Combine(root, access, directory, $"{ClassName(owner)}.cs");
    }

    /// <summary>The private-access class's name: the owner's name, with its containing types', and <c>PrivateAccess</c>.</summary>
    public static string ClassName(INamedTypeSymbol owner)
    {
        var names = new List<string>();
        for (INamedTypeSymbol? type = owner; type is not null; type = type.ContainingType)
            names.Insert(0, type.Name);
        return string.Concat(names) + "PrivateAccess";
    }

    /// <summary>A type as generated code writes it.</summary>
    public static string TypeText(ITypeSymbol type) => type.ToDisplayString(TypeFormat);

    /// <summary>A type as <c>typeof</c> takes it: without nullable reference annotations.</summary>
    public static string TypeOfText(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>One generated member: the key it is told apart by, and its source.</summary>
    /// <param name="Key">Its kind, name and, for a method, parameters.</param>
    /// <param name="Text">Its source, documentation included.</param>
    private sealed record RenderedMember(string Key, string Text);
}
