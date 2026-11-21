using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Generators;

/// <summary>
/// One compilation's state-graph members (#2602). Each covered type is a level of the objects that derive from it: its own
/// fields (its declared instance fields, the values its auto-properties keep and, for a class with a primary constructor,
/// the parameters it captures) are written, visited and restored by its own members, so a derived type calls its base's
/// without reaching anything private to it.
/// </summary>
/// <remarks>
/// <para>
/// A level without a primary constructor is restored by its restore constructor, which chains to its base's restore
/// constructor and assigns every field it declares. A level with a primary constructor must chain to it, so its restore
/// constructor first reads the arguments: for each parameter, the value of the member that keeps it (a record's property of
/// its name, a captured parameter, a member its initializer is made from, or what the base it passes the parameter to keeps
/// of it). The primary constructor then builds the object, its bases through their own constructors, and the restore
/// constructor assigns every field the object holds: its own in place, its bases' through each base's restore method, which
/// assigns what can be assigned after construction and checks that what cannot (a get-only or init-only property) is what
/// the state holds.
/// </para>
/// <para>
/// A state writes an object's levels in the order its restore reads them, each level's fields after their count and each
/// field's value after its name. A value type whose fields are all scalars, enums or such value types is packed when an
/// array holds it: its schema once, then its values back to back.
/// </para>
/// </remarks>
internal sealed partial class StateGraphRun
{
    private const string Core = "global::SuperMetroid.Core.";
    private const string Writer = Core + "IStateGraphWriter";
    private const string Reader = Core + "IStateGraphReader";
    private const string Visitor = Core + "IStateGraphVisitor";
    private const string Registry = Core + "IStateGraphRegistry";
    private const string FieldType = Core + "StateGraphField";
    private const string Restore = Core + "StateGraphRestore";
    private const string NameType = Core + "StateGraphName";

    private static readonly DiagnosticDescriptor NotPartial = new("SME6491", "State-graph type is not partial",
        "{0} must be declared partial: its assembly is marked [assembly: StateGraphAssembly], so the compiler writes the members a debugger state holds it through (#2602)",
        "Build", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnmappedParameter = new("SME6492", "Primary constructor parameter kept by no member",
        "Parameter '{1}' of {0} is kept by no member, so a debugger state cannot rebuild {0} through its primary constructor (#2602): keep the value in a member, or capture the parameter",
        "Build", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedField = new("SME6493", "Field a debugger state cannot hold",
        "{0}.{1} is a {2}, which a debugger state cannot hold (#2602): mark it [NonSerialized] when it is no game state",
        "Build", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private readonly Compilation compilation;
    private readonly SourceProductionContext output;
    private readonly bool partialOnly;
    private readonly bool catalogs;
    private readonly INamedTypeSymbol? nonSerialized;
    private readonly INamedTypeSymbol? inlineArray;

    /// <summary>The source types that get members, every one declared partial within partial containers.</summary>
    private readonly HashSet<INamedTypeSymbol> covered = new(SymbolEqualityComparer.Default);

    /// <summary>Every source type, in declaration order.</summary>
    private readonly List<INamedTypeSymbol> types = [];

    /// <summary>Each node type's level, worked out once.</summary>
    private readonly Dictionary<INamedTypeSymbol, Level?> levels = new(SymbolEqualityComparer.Default);

    /// <summary>The registrations each type's own registration adds: delegate methods, catalogs, declared-type probes.</summary>
    private readonly Dictionary<INamedTypeSymbol, List<string>> extraRegistrations = new(SymbolEqualityComparer.Default);

    /// <summary>The assembly registrar's own lines.</summary>
    private readonly List<string> assemblyRegistrations = [];

    internal StateGraphRun(Compilation compilation, SourceProductionContext output, bool partialOnly, bool catalogs)
    {
        this.compilation = compilation;
        this.output = output;
        this.partialOnly = partialOnly;
        this.catalogs = catalogs;
        nonSerialized = compilation.GetTypeByMetadataName("System.NonSerializedAttribute");
        inlineArray = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.InlineArrayAttribute");
    }

    /// <summary>Writes every covered type's members and the assembly's registrar.</summary>
    internal void Generate()
    {
        Collect(compilation.Assembly.GlobalNamespace);
        foreach (INamedTypeSymbol type in types)
        {
            if (IsCovered(type))
                covered.Add(type);
            else if (!partialOnly && !type.IsFileLocal && type.DeclaringSyntaxReferences.Length > 0)
                output.ReportDiagnostic(Diagnostic.Create(NotPartial, type.Locations.FirstOrDefault(), type.ToDisplayString()));
        }
        CollectDelegateMethods();
        if (catalogs)
        {
            CollectCatalogs();
            new StateGraphExposure(this).Collect();
        }
        foreach (INamedTypeSymbol type in types)
        {
            if (covered.Contains(type))
                output.AddSource(HintName(type), EmitType(type));
        }
        output.AddSource("GeneratedStateGraph.g.cs", EmitRegistrar());
    }

    // ---------------------------------------------------------------- types

    private void Collect(INamespaceSymbol space)
    {
        foreach (INamespaceSymbol child in space.GetNamespaceMembers())
            Collect(child);
        foreach (INamedTypeSymbol type in space.GetTypeMembers())
            CollectType(type);
    }

    private void CollectType(INamedTypeSymbol type)
    {
        if (type.IsImplicitlyDeclared || type.DeclaringSyntaxReferences.Length == 0 ||
            type.TypeKind is not (TypeKind.Class or TypeKind.Struct) ||
            !type.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax))
            return;
        types.Add(type);
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
            CollectType(nested);
    }

    /// <summary>Whether every declaration of the type and of each type containing it is partial.</summary>
    private static bool IsCovered(INamedTypeSymbol type)
    {
        if (type.IsFileLocal)
            return false;
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaringSyntaxReferences.Length == 0 || !current.DeclaringSyntaxReferences.All(reference =>
                    reference.GetSyntax() is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword)))
                return false;
        }
        return true;
    }

    /// <summary>Whether a type of this compilation gets members.</summary>
    internal bool Covers(INamedTypeSymbol type) => covered.Contains(type.OriginalDefinition);

    /// <summary>Whether a type, of this compilation or another, has state-graph members a derived type or a field's registration can call.</summary>
    internal bool HasMembers(INamedTypeSymbol type)
    {
        INamedTypeSymbol definition = type.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(definition.ContainingAssembly, compilation.Assembly))
            return covered.Contains(definition);
        return definition.GetMembers("__StateRegister").Length > 0;
    }

    /// <summary>The level of a node type, or null when no object of it is written by its fields.</summary>
    internal Level? LevelOf(INamedTypeSymbol type)
    {
        type = type.OriginalDefinition;
        if (levels.TryGetValue(type, out Level? known))
            return known;
        levels[type] = null;
        Level? level = BuildLevel(type);
        levels[type] = level;
        return level;
    }

    /// <summary>Whether a type, of this compilation or another, is written by its fields through generated members.</summary>
    internal bool IsNode(INamedTypeSymbol type)
    {
        INamedTypeSymbol definition = type.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(definition.ContainingAssembly, compilation.Assembly))
            return LevelOf(definition) is not null;
        return definition.GetMembers("__StateWriteRestoreOrder_" + Suffix(definition)).Length > 0;
    }

    // ---------------------------------------------------------------- names

    /// <summary>The declared name a state gives a type: its namespace, containing types after '+', and its metadata name.</summary>
    internal static string Declared(INamedTypeSymbol type)
    {
        type = type.OriginalDefinition;
        string name = type.MetadataName;
        for (INamedTypeSymbol? container = type.ContainingType; container is not null; container = container.ContainingType)
            name = container.MetadataName + "+" + name;
        return type.ContainingNamespace is { IsGlobalNamespace: false } space ? space.ToDisplayString() + "." + name : name;
    }

    /// <summary>The suffix of a level's member names: its declared name with every other character an underscore.</summary>
    internal static string Suffix(INamedTypeSymbol type)
    {
        var builder = new StringBuilder();
        foreach (char character in Declared(type))
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        return builder.ToString();
    }

    /// <summary>The type as code names it, fully qualified.</summary>
    internal static string Code(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    /// <summary>A C# string literal.</summary>
    internal static string Literal(string text) => SymbolDisplay.FormatLiteral(text, quote: true);

    /// <summary>The expression naming a type for its registration: its declared name and the types it is constructed with.</summary>
    private static string NameExpression(INamedTypeSymbol type)
    {
        var arguments = new List<string>();
        var chain = new List<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
            chain.Insert(0, current);
        foreach (INamedTypeSymbol level in chain)
            arguments.AddRange(level.TypeArguments.Select(argument => $"typeof({Code(argument)})"));
        return $"new {NameType}({Literal(Declared(type))}{string.Concat(arguments.Select(argument => ", " + argument))})";
    }

    private static string HintName(INamedTypeSymbol type) =>
        "StateGraph/" + Suffix(type) + ".g.cs";

    // ---------------------------------------------------------------- levels

    internal enum MemberKind
    {
        /// <summary>A field the type declares; <see cref="Member.Readonly"/> when it is read-only.</summary>
        Field,

        /// <summary>The value an auto-property keeps, set through a setter that is not init-only.</summary>
        SettableProperty,

        /// <summary>The value a get-only or init-only auto-property keeps, set only while the object is built.</summary>
        FixedProperty,

        /// <summary>A primary constructor parameter the type's members capture.</summary>
        Captured,
    }

    /// <summary>One field a level holds.</summary>
    internal sealed class Member(string name, ITypeSymbol type, MemberKind kind, bool isReadonly)
    {
        internal string Name { get; } = name;
        internal ITypeSymbol Type { get; } = type;
        internal MemberKind Kind { get; } = kind;
        internal bool Readonly { get; } = isReadonly;

        /// <summary>The expression reading the member's value inside its type.</summary>
        internal string Expression => Kind == MemberKind.Captured ? "@" + Name : "this.@" + Name;
    }

    /// <summary>A node type's own fields and how it is built.</summary>
    internal sealed class Level
    {
        internal INamedTypeSymbol Type { get; init; } = null!;
        internal string Suffix { get; init; } = null!;
        internal string Declared { get; init; } = null!;
        internal List<Member> Own { get; init; } = null!;
        internal IMethodSymbol? Primary { get; init; }
        internal INamedTypeSymbol? Base { get; init; }
        internal int InlineLength { get; init; }
        internal ITypeSymbol? InlineElement { get; init; }
        /// <summary>For each primary constructor parameter, the expression reading what keeps its value, or null.</summary>
        internal List<string?> Arguments { get; init; } = null!;
        internal bool IsStruct => Type.TypeKind == TypeKind.Struct;
        internal bool IsSealed => IsStruct || Type.IsSealed;
        internal string Access => IsSealed ? "private" : "protected";
    }

    private Level? BuildLevel(INamedTypeSymbol type)
    {
        if (!covered.Contains(type) || type.IsStatic || type.IsRefLikeType || type.IsAbstract && type.TypeKind == TypeKind.Struct)
            return null;
        INamedTypeSymbol? baseType = null;
        if (type.TypeKind == TypeKind.Class && type.BaseType is { } declaredBase && declaredBase.SpecialType != SpecialType.System_Object)
        {
            if (!IsNode(declaredBase))
                return null;
            baseType = declaredBase;
        }
        var own = new List<Member>();
        int inlineLength = 0;
        ITypeSymbol? inlineElement = null;
        if (inlineArray is not null && type.GetAttributes().FirstOrDefault(attribute =>
                SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, inlineArray)) is { } inline &&
            inline.ConstructorArguments.FirstOrDefault().Value is int length)
        {
            inlineLength = length;
            inlineElement = type.GetMembers().OfType<IFieldSymbol>().First(field => !field.IsStatic).Type;
        }
        else
        {
            foreach (ISymbol symbol in type.GetMembers())
            {
                if (symbol is not IFieldSymbol { IsStatic: false, IsConst: false } field)
                    continue;
                if (IsNonSerialized(field))
                    continue;
                if (field.IsFixedSizeBuffer || field.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer || field.RefKind != RefKind.None)
                {
                    output.ReportDiagnostic(Diagnostic.Create(UnsupportedField, field.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault(),
                        type.ToDisplayString(), field.Name, field.IsFixedSizeBuffer ? "fixed buffer" : field.Type.ToDisplayString()));
                    continue;
                }
                switch (field.AssociatedSymbol)
                {
                    case IPropertySymbol property:
                        own.Add(new Member(property.Name, field.Type,
                            property.SetMethod is { IsInitOnly: false } ? MemberKind.SettableProperty : MemberKind.FixedProperty, isReadonly: false));
                        break;
                    case IEventSymbol @event:
                        own.Add(new Member(@event.Name, field.Type, MemberKind.Field, isReadonly: false));
                        break;
                    case null when !field.IsImplicitlyDeclared:
                        own.Add(new Member(field.Name, field.Type, MemberKind.Field, field.IsReadOnly));
                        break;
                }
            }
        }
        IMethodSymbol? primary = PrimaryConstructor(type);
        if (primary is not null && !type.IsRecord)
        {
            foreach (IParameterSymbol parameter in CapturedParameters(type, primary))
                own.Add(new Member(parameter.Name, parameter.Type, MemberKind.Captured, isReadonly: false));
        }
        var level = new Level
        {
            Type = type,
            Suffix = Suffix(type),
            Declared = Declared(type),
            Own = own,
            Primary = primary,
            Base = baseType,
            InlineLength = inlineLength,
            InlineElement = inlineElement,
            Arguments = [],
        };
        if (primary is not null)
        {
            foreach (IParameterSymbol parameter in primary.Parameters)
            {
                string? argument = MapArgument(type, primary, parameter, own);
                if (argument is null)
                    output.ReportDiagnostic(Diagnostic.Create(UnmappedParameter, parameter.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault(),
                        type.ToDisplayString(), parameter.Name));
                level.Arguments.Add(argument);
            }
        }
        return level;
    }

    private bool IsNonSerialized(IFieldSymbol field) =>
        nonSerialized is not null && field.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, nonSerialized));

    /// <summary>The type's primary constructor, the one its declaration's parameter list declares.</summary>
    internal static IMethodSymbol? PrimaryConstructor(INamedTypeSymbol type) =>
        type.InstanceConstructors.FirstOrDefault(constructor =>
            constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is TypeDeclarationSyntax));

    /// <summary>The primary constructor parameters the type's members (not its initializers) refer to, which it keeps.</summary>
    private List<IParameterSymbol> CapturedParameters(INamedTypeSymbol type, IMethodSymbol primary)
    {
        var captured = new List<IParameterSymbol>();
        if (primary.Parameters.Length == 0)
            return captured;
        var names = new HashSet<string>(primary.Parameters.Select(parameter => parameter.Name), StringComparer.Ordinal);
        foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
        {
            var declaration = (TypeDeclarationSyntax)reference.GetSyntax();
            SemanticModel model = compilation.GetSemanticModel(declaration.SyntaxTree);
            foreach (MemberDeclarationSyntax member in declaration.Members)
            {
                if (member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax || member is FieldDeclarationSyntax)
                    continue;
                foreach (IdentifierNameSyntax identifier in member.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    if (!names.Contains(identifier.Identifier.ValueText) || IsInPropertyInitializer(identifier))
                        continue;
                    if (model.GetSymbolInfo(identifier).Symbol is IParameterSymbol parameter &&
                        SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, primary) &&
                        !captured.Contains(parameter, SymbolEqualityComparer.Default))
                        captured.Add(parameter);
                }
            }
        }
        return [.. primary.Parameters.Where(parameter => captured.Contains(parameter, SymbolEqualityComparer.Default))];
    }

    /// <summary>The types of the primary constructor parameters a source type captures; none for any other type.</summary>
    internal IReadOnlyList<ITypeSymbol> CapturedTypes(INamedTypeSymbol type)
    {
        type = type.OriginalDefinition;
        if (capturedTypes.TryGetValue(type, out List<ITypeSymbol>? known))
            return known;
        var found = new List<ITypeSymbol>();
        if (!type.IsRecord && SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly) &&
            PrimaryConstructor(type) is { } primary && type.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is TypeDeclarationSyntax))
            found.AddRange(CapturedParameters(type, primary).Select(parameter => parameter.Type));
        capturedTypes[type] = found;
        return found;
    }

    private readonly Dictionary<INamedTypeSymbol, List<ITypeSymbol>> capturedTypes = new(SymbolEqualityComparer.Default);

    private static bool IsInPropertyInitializer(SyntaxNode node) =>
        node.Ancestors().Any(ancestor => ancestor is EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax });

    /// <summary>
    /// The expression reading what keeps a primary constructor parameter's value: a record's member of its name, the
    /// captured parameter, a member whose initializer refers to it, or what the base it is passed to keeps of it.
    /// </summary>
    private string? MapArgument(INamedTypeSymbol type, IMethodSymbol primary, IParameterSymbol parameter, List<Member> own)
    {
        if (type.IsRecord)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                ISymbol? named = current.GetMembers(parameter.Name).FirstOrDefault(member => member is IPropertySymbol { IsStatic: false } or IFieldSymbol { IsStatic: false });
                if (named is not null && (SymbolEqualityComparer.Default.Equals(current, type) || named.DeclaredAccessibility != Accessibility.Private) &&
                    Converts(named is IPropertySymbol property ? property.Type : ((IFieldSymbol)named).Type, parameter.Type))
                    return "this.@" + parameter.Name;
            }
        }
        if (own.Any(member => member.Kind == MemberKind.Captured && member.Name == parameter.Name))
            return "@" + parameter.Name;
        // A member made from the parameter: the simplest initializer first (the parameter itself, then a call on it).
        (string Name, int Rank)? best = null;
        foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
        {
            var declaration = (TypeDeclarationSyntax)reference.GetSyntax();
            SemanticModel model = compilation.GetSemanticModel(declaration.SyntaxTree);
            foreach (MemberDeclarationSyntax member in declaration.Members)
            {
                (string Name, EqualsValueClauseSyntax Initializer, ITypeSymbol Type)[] initialized = member switch
                {
                    FieldDeclarationSyntax field when !field.Modifiers.Any(SyntaxKind.StaticKeyword) && !field.Modifiers.Any(SyntaxKind.ConstKeyword) =>
                        [.. field.Declaration.Variables.Where(variable => variable.Initializer is not null && model.GetDeclaredSymbol(variable) is IFieldSymbol)
                            .Select(variable => (variable.Identifier.ValueText, variable.Initializer!, ((IFieldSymbol)model.GetDeclaredSymbol(variable)!).Type))],
                    PropertyDeclarationSyntax { Initializer: { } initializer } property when !property.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                        model.GetDeclaredSymbol(property) is IPropertySymbol symbol => [(property.Identifier.ValueText, initializer, symbol.Type)],
                    _ => [],
                };
                foreach ((string name, EqualsValueClauseSyntax initializer, ITypeSymbol memberType) in initialized)
                {
                    if (!own.Any(candidate => candidate.Name == name && candidate.Kind != MemberKind.Captured) || !Converts(memberType, parameter.Type))
                        continue;
                    int rank = Rank(initializer.Value, parameter, model);
                    if (rank >= 0 && (best is null || rank < best.Value.Rank))
                        best = (name, rank);
                }
            }
        }
        if (best is not null)
            return "this.@" + best.Value.Name;
        // A parameter passed on to the base's primary constructor is kept by what the base keeps of it.
        if (type.BaseType is { } baseType && IsNode(baseType) && PrimaryConstructor(baseType.OriginalDefinition) is { } basePrimary &&
            BaseArguments(type) is { } baseArguments)
        {
            for (int index = 0; index < baseArguments.Count && index < basePrimary.Parameters.Length; index++)
            {
                SemanticModel model = compilation.GetSemanticModel(baseArguments[index].SyntaxTree);
                if (baseArguments[index].Expression is IdentifierNameSyntax identifier &&
                    model.GetSymbolInfo(identifier).Symbol is IParameterSymbol passed &&
                    SymbolEqualityComparer.Default.Equals(passed, parameter) &&
                    Converts(basePrimary.Parameters[index].Type, parameter.Type))
                    return $"this.__StateArgument_{Suffix(baseType.OriginalDefinition)}_{index}()";
            }
        }
        return null;
    }

    /// <summary>The arguments the type's declaration passes to its base's constructor, or null when it passes none.</summary>
    private static IReadOnlyList<ArgumentSyntax>? BaseArguments(INamedTypeSymbol type)
    {
        foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is TypeDeclarationSyntax { BaseList: { } list } &&
                list.Types.FirstOrDefault() is PrimaryConstructorBaseTypeSyntax primaryBase)
                return primaryBase.ArgumentList.Arguments;
        }
        return null;
    }

    /// <summary>How directly an initializer is made from the parameter: 0 is the parameter itself, 1 a call on it, 2 anything referring to it; -1 none.</summary>
    private static int Rank(ExpressionSyntax value, IParameterSymbol parameter, SemanticModel model)
    {
        bool IsParameter(ExpressionSyntax expression) => expression is IdentifierNameSyntax identifier &&
            model.GetSymbolInfo(identifier).Symbol is IParameterSymbol symbol && SymbolEqualityComparer.Default.Equals(symbol, parameter);
        if (IsParameter(value))
            return 0;
        if (value is InvocationExpressionSyntax invocation && invocation.ArgumentList.Arguments.Any(argument => IsParameter(argument.Expression)))
            return 1;
        if (value is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.CoalesceExpression } coalesce && IsParameter(coalesce.Left))
            return 1;
        return value.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(identifier => IsParameter(identifier)) ? 2 : -1;
    }

    private bool Converts(ITypeSymbol from, ITypeSymbol to)
    {
        Conversion conversion = compilation.ClassifyConversion(from, to);
        return conversion.IsIdentity || conversion.IsImplicit && (conversion.IsReference || conversion.IsBoxing || conversion.IsNullable);
    }

    /// <summary>The levels below a primary-constructor level, nearest first, which its primary constructor builds through their own constructors.</summary>
    private List<Level> Inherited(Level level)
    {
        var inherited = new List<Level>();
        for (INamedTypeSymbol? current = level.Base; current is not null;)
        {
            Level? below = current.OriginalDefinition.ContainingAssembly.Equals(compilation.Assembly, SymbolEqualityComparer.Default) ? LevelOf(current) : null;
            inherited.Add(below ?? ForeignLevel(current));
            current = below?.Base ?? ForeignBase(current);
        }
        return inherited;
    }

    /// <summary>A level of another assembly's node type, known only by the names of its members.</summary>
    private static Level ForeignLevel(INamedTypeSymbol type) => new()
    {
        Type = type.OriginalDefinition,
        Suffix = Suffix(type),
        Declared = Declared(type),
        Own = [],
        Primary = null,
        Base = null,
        InlineLength = 0,
        InlineElement = null,
        Arguments = [],
    };

    private INamedTypeSymbol? ForeignBase(INamedTypeSymbol type) =>
        type.BaseType is { SpecialType: not SpecialType.System_Object } next && IsNode(next) ? next : null;
}
