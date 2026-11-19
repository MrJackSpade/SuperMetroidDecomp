using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// One owner's generated private-access class (#2570): an extension member for each private member checks name, of the
/// same name and signature, reaching the member through <c>PrivateState</c>. A value type's members that write it take
/// their receiver by reference.
/// </summary>
/// <param name="owner">The type whose private members the class reaches.</param>
/// <param name="path">The file the class is written to.</param>
internal sealed class OwnerShims(INamedTypeSymbol owner, string path)
{
    /// <summary>The instance members that read the owner, rendered.</summary>
    private readonly List<string> _reading = [];

    /// <summary>The instance members that write a value-type owner, rendered.</summary>
    private readonly List<string> _writing = [];

    /// <summary>The static members, rendered.</summary>
    private readonly List<string> _static = [];

    /// <summary>The classic extension methods' receiver forms, rendered, by receiver type.</summary>
    private readonly SortedDictionary<string, List<string>> _receivers = new(StringComparer.Ordinal);

    /// <summary>The file the class is written to.</summary>
    public string Path { get; } = path;

    /// <summary>The owner as generated code writes it.</summary>
    private string Owner => TestOnlyMemberFixer.TypeText(owner);

    /// <summary>Whether the owner is a value type, whose writes need it by reference.</summary>
    private bool IsValue => owner.IsValueType;

    /// <summary>Adds the extension member standing in for <paramref name="member"/>; <paramref name="written"/> when checks write it.</summary>
    public void Add(ISymbol member, bool written)
    {
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

    /// <summary>A field: a property reading it, and writing it when checks do.</summary>
    private void AddField(IFieldSymbol field, bool written)
    {
        string type = TestOnlyMemberFixer.TypeText(field.Type);
        string name = Name(field.Name);
        string doc = Summary($"The private field <c>{field.Name}</c>, reached by name.");
        bool settable = written && !field.IsConst;
        if (field.IsStatic || field.IsConst)
        {
            string get = $"PrivateState.StaticField<{type}>(typeof({Owner}), \"{field.Name}\")";
            _static.Add(settable
                ? $"{doc}internal static {type} {name}\n{{\n    get => {get};\n    set => PrivateState.SetStaticField(typeof({Owner}), \"{field.Name}\", value);\n}}"
                : $"{doc}internal static {type} {name} => {get};");
            return;
        }
        if (IsValue)
        {
            string get = $"PrivateState.ValueField<{Owner}, {type}>(self, \"{field.Name}\")";
            if (settable)
                _writing.Add($"{doc}internal {type} {name}\n{{\n    get => {get};\n    set => PrivateState.SetValueField(ref self, \"{field.Name}\", value);\n}}");
            else
                _reading.Add($"{doc}internal {type} {name} => {get};");
            return;
        }
        string read = $"PrivateState.Field<{Owner}, {type}>(self, \"{field.Name}\")";
        _reading.Add(settable
            ? $"{doc}internal {type} {name}\n{{\n    get => {read};\n    set => PrivateState.SetField(self, \"{field.Name}\", value);\n}}"
            : $"{doc}internal {type} {name} => {read};");
    }

    /// <summary>A property: a property of the same accessors, reaching them by name.</summary>
    private void AddProperty(IPropertySymbol property, bool written)
    {
        string type = TestOnlyMemberFixer.TypeText(property.Type);
        string name = Name(property.Name);
        string doc = Summary($"The private property <c>{property.Name}</c>, reached by name.");
        bool settable = property.SetMethod is not null && (written || property.GetMethod is null);
        bool gettable = property.GetMethod is not null;
        string Accessors(string get, string set) =>
            (gettable ? $"    get => {get};\n" : "") + (settable ? $"    set => {set};\n" : "");
        if (property.IsStatic)
        {
            _static.Add($"{doc}internal static {type} {name}\n{{\n" + Accessors(
                $"PrivateState.StaticGet<{type}>(typeof({Owner}), \"{property.Name}\")",
                $"PrivateState.StaticSet(typeof({Owner}), \"{property.Name}\", value)") + "}");
            return;
        }
        if (IsValue)
        {
            (settable ? _writing : _reading).Add($"{doc}internal {type} {name}\n{{\n" + Accessors(
                $"PrivateState.ValueGet<{Owner}, {type}>(self, \"{property.Name}\")",
                $"PrivateState.ValueSet(ref self, \"{property.Name}\", value)") + "}");
            return;
        }
        _reading.Add($"{doc}internal {type} {name}\n{{\n" + Accessors(
            $"PrivateState.Get<{Owner}, {type}>(self, \"{property.Name}\")",
            $"PrivateState.Set(self, \"{property.Name}\", value)") + "}");
    }

    /// <summary>A method: a method of the same signature, invoking it by name with its parameter types.</summary>
    private void AddMethod(IMethodSymbol method)
    {
        string doc = Summary($"The private method <c>{method.Name}</c>, invoked by name.");
        var parameters = method.Parameters;
        string types = "[" + string.Join(", ", parameters.Select(parameter =>
            $"typeof({TestOnlyMemberFixer.TypeOfText(parameter.Type)})" + (parameter.RefKind is RefKind.None or RefKind.In ? "" : ".MakeByRefType()"))) + "]";
        bool returns = !method.ReturnsVoid;
        string result = returns ? TestOnlyMemberFixer.TypeText(method.ReturnType) : "void";
        string Call(string invoker, string receiver, IEnumerable<IParameterSymbol> passed) =>
            Body(invoker, receiver, types, method.Name, returns ? result : null, [.. passed]);
        if (method.IsStatic)
        {
            string invoker = returns ? $"PrivateState.InvokeStatic<{result}>" : "PrivateState.InvokeStatic";
            _static.Add($"{doc}internal static {result} {Name(method.Name)}({ParameterList(parameters)}){Call(invoker, $"typeof({Owner})", parameters)}");
            if (method.IsExtensionMethod && parameters.Length > 0 && parameters[0].RefKind == RefKind.None)
            {
                string receiverType = TestOnlyMemberFixer.TypeText(parameters[0].Type);
                if (!_receivers.TryGetValue(receiverType, out var members))
                    _receivers[receiverType] = members = [];
                string arguments = string.Join(", ", parameters.Skip(1).Select(parameter => Argument(parameter)).Prepend("self"));
                members.Add($"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters.Skip(1))}) =>\n    " +
                    $"{Owner}.{Name(method.Name)}({arguments});");
            }
            return;
        }
        if (IsValue)
        {
            string invoker = returns ? $"PrivateState.InvokeValue<{Owner}, {result}>" : $"PrivateState.InvokeValue<{Owner}>";
            _writing.Add($"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters)}){Call(invoker, "ref self", parameters)}");
            return;
        }
        string instanceInvoker = returns ? $"PrivateState.Invoke<{Owner}, {result}>" : $"PrivateState.Invoke<{Owner}>";
        _reading.Add($"{doc}internal {result} {Name(method.Name)}({ParameterList(parameters)}){Call(instanceInvoker, "self", parameters)}");
    }


    /// <summary>
    /// A method body invoking the private method: an expression body, or, when a parameter is passed by reference, a block
    /// that passes the arguments in an array and writes each by-reference one back from it.
    /// </summary>
    private static string Body(string invoker, string receiver, string types, string name, string? result, IReadOnlyList<IParameterSymbol> parameters)
    {
        string arguments = "new object?[] { " + string.Join(", ", parameters.Select(parameter => parameter.RefKind == RefKind.Out ? "null" : Name(parameter.Name))) + " }";
        if (parameters.All(parameter => parameter.RefKind is RefKind.None or RefKind.In))
            return $" =>\n    {invoker}({receiver}, \"{name}\", {types}, {arguments});";
        var body = new StringBuilder("\n{\n");
        body.Append(CultureInfo.InvariantCulture, $"    object?[] arguments = {arguments};\n");
        body.Append(result is null ? "    " : $"    {result} result = ").Append(CultureInfo.InvariantCulture, $"{invoker}({receiver}, \"{name}\", {types}, arguments);\n");
        for (int index = 0; index < parameters.Count; index++)
            if (parameters[index].RefKind is RefKind.Ref or RefKind.Out)
                body.Append(CultureInfo.InvariantCulture, $"    {Name(parameters[index].Name)} = ({TestOnlyMemberFixer.TypeText(parameters[index].Type)})arguments[{index}]!;\n");
        if (result is not null)
            body.Append("    return result;\n");
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
        text.Append(TestOnlyMemberFixer.TypeText(parameter.Type)).Append(' ').Append(Name(parameter.Name));
        if (parameter.HasExplicitDefaultValue)
            text.Append(" = ").Append(Default(parameter));
        return text.ToString();
    }));

    /// <summary>A parameter passed on: by reference when it is one.</summary>
    private static string Argument(IParameterSymbol parameter) => RefText(parameter.RefKind) + Name(parameter.Name);

    /// <summary>The keyword a parameter or argument of a reference kind is written with.</summary>
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
            return $"({TestOnlyMemberFixer.TypeText(parameter.Type)})({Convert.ToString(value, CultureInfo.InvariantCulture)})";
        return value switch
        {
            bool flag => flag ? "true" : "false",
            string text => SymbolDisplay.FormatLiteral(text, quote: true),
            char character => SymbolDisplay.FormatLiteral(character, quote: true),
            _ => $"({TestOnlyMemberFixer.TypeText(parameter.Type)})({Convert.ToString(value, CultureInfo.InvariantCulture)})",
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

    /// <summary>The class's source.</summary>
    public string Render()
    {
        if (File.Exists(Path))
            throw new InvalidOperationException($"{Path} exists already; a private-access class is generated whole.");
        var text = new StringBuilder();
        text.Append("using SuperMetroid.Core;\n\n");
        if (!owner.ContainingNamespace.IsGlobalNamespace)
            text.Append(CultureInfo.InvariantCulture, $"namespace {owner.ContainingNamespace.ToDisplayString()};\n\n");
        text.Append(CultureInfo.InvariantCulture, $"/// <summary>\n/// The private members of <see cref=\"{owner.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat).Replace('<', '{').Replace('>', '}')}\"/> checks name (#2570), each an extension member of the\n");
        text.Append("/// same name and signature reaching the private member through <see cref=\"PrivateState\"/>, so checks name them as the\n");
        text.Append("/// owner's own code does and production keeps them private. Generated by the architecture audit's\n");
        text.Append("/// <c>--fix-test-only-members</c>.\n/// </summary>\n");
        text.Append(CultureInfo.InvariantCulture, $"internal static class {TestOnlyMemberFixer.ClassName(owner)}\n{{\n");
        var blocks = new List<string>();
        if (_reading.Count > 0)
            blocks.Add(Block($"/// <summary>The private instance members checks read or call.</summary>\nextension({Owner} self)", _reading));
        if (_writing.Count > 0)
            blocks.Add(Block($"/// <summary>The private instance members checks write, on the value itself.</summary>\nextension(ref {Owner} self)", _writing));
        if (_static.Count > 0)
            blocks.Add(Block($"/// <summary>The private static members checks name.</summary>\nextension({Owner})", _static));
        foreach (var (receiver, members) in _receivers)
            blocks.Add(Block($"/// <summary>The private extension methods checks call on their receiver.</summary>\nextension({receiver} self)", members));
        text.Append(string.Join("\n\n", blocks.Select(block => Indent(block, "    "))));
        text.Append("\n}\n");
        return text.ToString().Replace("\n", "\r\n", StringComparison.Ordinal);
    }

    /// <summary>An extension block holding <paramref name="members"/>.</summary>
    private static string Block(string header, List<string> members) =>
        $"{header}\n{{\n{string.Join("\n\n", members.Select(member => Indent(member, "    ")))}\n}}";

    /// <summary>Every line of <paramref name="text"/> indented.</summary>
    private static string Indent(string text, string by) => string.Join("\n", text.Split('\n').Select(line => line.Length == 0 ? line : by + line));
}
