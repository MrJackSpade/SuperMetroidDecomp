using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Records an edge from each declaration to every symbol its body, signature, attributes or
/// initializers reference, including the compiler's implicit calls (enumerators, awaiters,
/// deconstruction, collection initializers, user-defined operators and conversions, implicit
/// indexers and base constructors). nameof and documentation crefs are not references.
/// </summary>
internal sealed class ReferenceCollector(SymbolIdentity identity, ReachabilityGraph graph, RootCollector roots,
    JsonSerializationScan json, ReflectionScan reflection)
{
    /// <summary>Every repository symbol referenced anywhere, reachable or not.</summary>
    public HashSet<string> Referenced { get; } = [];

    /// <summary>Scans repository syntax trees in a project and records each bound reference.</summary>
    public void Collect(Project project, Compilation compilation)
    {
        foreach (var tree in compilation.SyntaxTrees.Where(identity.IsRepositorySource))
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
                Visit(project, model, node);
        }
    }

    /// <summary>Handles implicit constructor edges and sends reference-bearing syntax to semantic analysis.</summary>
    private void Visit(Project project, SemanticModel model, SyntaxNode node)
    {
        switch (node)
        {
            case ConstructorDeclarationSyntax { Initializer: null } constructor
                when model.GetDeclaredSymbol(constructor) is { IsStatic: false } declared:
                // Without an initializer the constructor calls the base parameterless constructor.
                graph.Edge(identity.Key(declared), identity.Key(ParameterlessBaseConstructor(declared.ContainingType)));
                return;
            case TypeDeclarationSyntax declaration
                when model.GetDeclaredSymbol(declaration) is INamedTypeSymbol type && identity.Key(type) is { } typeKey:
                string? baseConstructor = identity.Key(ParameterlessBaseConstructor(type));
                if (type.InstanceConstructors.Any(c => c.IsImplicitlyDeclared))
                {
                    string implicitConstructor = typeKey + SymbolIdentity.ImplicitConstructorSuffix;
                    graph.Edge(implicitConstructor, baseConstructor);
                    graph.Edge(implicitConstructor, typeKey);
                }
                // A primary constructor without base arguments also calls the parameterless base constructor.
                if (declaration.ParameterList is not null
                    && declaration.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().Any() != true)
                    foreach (var primary in type.InstanceConstructors.Where(c =>
                                 c.DeclaringSyntaxReferences.Any(r => r.GetSyntax() == declaration)))
                        graph.Edge(identity.Key(primary), baseConstructor);
                return;
            case ExpressionSyntax or ConstructorInitializerSyntax or AttributeSyntax or PrimaryConstructorBaseTypeSyntax
                or CommonForEachStatementSyntax or QueryClauseSyntax or PatternSyntax:
                break;
            default:
                return;
        }
        if (IsInsideNameof(node) || OwnerKey(project, model, node) is not { } owner)
            return;
        // Listing an interface in a base list declares it; only uses through the interface reach it.
        if (node is TypeSyntax { Parent: SimpleBaseTypeSyntax { Parent: BaseListSyntax } } &&
            model.GetTypeInfo(node).Type is { TypeKind: TypeKind.Interface })
            return;

        var targets = new List<ISymbol?>();
        var info = model.GetSymbolInfo(node);
        targets.Add(info.Symbol);
        targets.AddRange(info.CandidateSymbols);
        AddImplicitTargets(model, node, info, targets, owner);
        foreach (var target in targets)
        {
            if (identity.Key(target) is not { } targetKey)
                continue;
            Referenced.Add(targetKey);
            graph.Edge(owner, targetKey);
            // Instantiating through an implicit constructor instantiates the type.
            if (targetKey.EndsWith(SymbolIdentity.ImplicitConstructorSuffix, StringComparison.Ordinal))
                graph.Edge(owner, targetKey[..^SymbolIdentity.ImplicitConstructorSuffix.Length]);
        }
    }

    /// <summary>Adds compiler-inserted calls and value-sensitive reads associated with a syntax node.</summary>
    private void AddImplicitTargets(SemanticModel model, SyntaxNode node, SymbolInfo info, List<ISymbol?> targets, string owner)
    {
        switch (node)
        {
            case CommonForEachStatementSyntax forEach:
                var loop = model.GetForEachStatementInfo(forEach);
                targets.AddRange([loop.GetEnumeratorMethod, loop.MoveNextMethod, loop.CurrentProperty, loop.DisposeMethod]);
                if (loop.ElementConversion.IsUserDefined)
                    targets.Add(loop.ElementConversion.MethodSymbol);
                if (forEach is ForEachVariableStatementSyntax variables)
                    AddDeconstruction(model.GetDeconstructionInfo(variables), targets);
                return;
            case QueryClauseSyntax clause:
                var query = model.GetQueryClauseInfo(clause);
                targets.AddRange([query.CastInfo.Symbol, query.OperationInfo.Symbol]);
                return;
            case ListPatternSyntax when model.GetOperation(node) is IListPatternOperation list:
                targets.AddRange([list.LengthSymbol, list.IndexerSymbol]);
                return;
            case RecursivePatternSyntax when model.GetOperation(node) is IRecursivePatternOperation recursive:
                targets.Add(recursive.DeconstructSymbol);
                return;
            case not ExpressionSyntax:
                return;
        }

        var expression = (ExpressionSyntax)node;
        var conversion = model.GetConversion(expression);
        if (conversion.IsUserDefined)
            targets.Add(conversion.MethodSymbol);
        AddRecordValueReads(model, expression, info, targets);
        AddEnumNameReads(model, expression, info, targets);
        switch (expression)
        {
            case InitializerExpressionSyntax initializer when initializer.IsKind(SyntaxKind.CollectionInitializerExpression):
                foreach (var element in initializer.Expressions)
                {
                    var add = model.GetCollectionInitializerSymbolInfo(element);
                    targets.Add(add.Symbol);
                    targets.AddRange(add.CandidateSymbols);
                }
                break;
            case AwaitExpressionSyntax awaited:
                var awaiter = model.GetAwaitExpressionInfo(awaited);
                targets.AddRange([awaiter.GetAwaiterMethod, awaiter.IsCompletedProperty, awaiter.GetResultMethod]);
                break;
            case AssignmentExpressionSyntax { Left: TupleExpressionSyntax or DeclarationExpressionSyntax } deconstruction:
                AddDeconstruction(model.GetDeconstructionInfo(deconstruction), targets);
                break;
            case ElementAccessExpressionSyntax when model.GetOperation(expression) is IImplicitIndexerReferenceOperation indexer:
                targets.AddRange([indexer.LengthSymbol, indexer.IndexerSymbol]);
                break;
            case InvocationExpressionSyntax invocation when info.Symbol is IMethodSymbol method:
                json.ObserveInvocation(model, invocation, method);
                reflection.ObserveInvocation(model, invocation, method, owner);
                reflection.ObserveCall(model, invocation, method);
                break;
        }
    }

    /// <summary>
    /// A record's synthesized equality, hashing and formatting read every member value, but have
    /// no source declaration to reach. Comparing, hashing or formatting a record, or handing it to
    /// generic code as a type argument (dictionary and set keys, Distinct, EqualityComparer),
    /// therefore reaches all of its properties and fields.
    /// </summary>
    private static void AddRecordValueReads(SemanticModel model, ExpressionSyntax expression, SymbolInfo info, List<ISymbol?> targets)
    {
        if (info.Symbol is IMethodSymbol { ContainingType.IsRecord: true } method &&
            method.Name is WellKnownMemberNames.EqualityOperatorName or WellKnownMemberNames.InequalityOperatorName
                or nameof(Equals) or nameof(GetHashCode) or nameof(ToString))
            AddRecordMembers(method.ContainingType, targets);
        if (info.Symbol is IMethodSymbol { IsGenericMethod: true } generic && EqualitySensitiveMethods.Contains(generic.Name))
            foreach (var argument in generic.TypeArguments)
                AddRecordMembers(argument, targets);
        if (info.Symbol is IMethodSymbol { ContainingType: { IsGenericType: true } owner } member &&
            (EqualitySensitiveTypes.Contains(owner.Name) || EqualitySensitiveMethods.Contains(member.Name)))
            foreach (var argument in owner.TypeArguments)
                AddRecordMembers(argument, targets);
        if (expression is GenericNameSyntax && info.Symbol is INamedTypeSymbol { IsGenericType: true } genericType &&
            EqualitySensitiveTypes.Contains(genericType.Name))
            foreach (var argument in genericType.TypeArguments)
                AddRecordMembers(argument, targets);
        if (expression.Parent is InterpolationSyntax && model.GetTypeInfo(expression).Type is { } formatted)
            AddRecordMembers(formatted, targets);
    }

    /// <summary>Generic types that hash or compare their type arguments' values.</summary>
    private static readonly HashSet<string> EqualitySensitiveTypes =
    [
        "Dictionary", "HashSet", "SortedSet", "SortedDictionary", "ConcurrentDictionary", "ImmutableDictionary",
        "ImmutableHashSet", "FrozenDictionary", "FrozenSet", "EqualityComparer", "Lookup", "ILookup", "IEquatable",
    ];

    /// <summary>Generic methods that compare or hash element values.</summary>
    private static readonly HashSet<string> EqualitySensitiveMethods =
    [
        "Distinct", "DistinctBy", "Contains", "SequenceEqual", "Union", "UnionBy", "Intersect", "IntersectBy",
        "Except", "ExceptBy", "GroupBy", "ToDictionary", "ToHashSet", "ToLookup", "IndexOf", "LastIndexOf",
        "Remove", "CountBy", "AggregateBy", "ToFrozenDictionary", "ToFrozenSet", "Equals",
    ];

    /// <summary>Adds instance values read by synthesized record equality, including nested record values.</summary>
    private static void AddRecordMembers(ITypeSymbol? type, List<ISymbol?> targets, int depth = 0)
    {
        if (depth > 4 || type is not INamedTypeSymbol { IsRecord: true } record)
            return;
        foreach (var member in record.GetMembers())
            if (member is IPropertySymbol { IsStatic: false } or IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false })
            {
                targets.Add(member);
                // Value equality is structural: a nested record member compares its own members.
                AddRecordMembers(member is IPropertySymbol property ? property.Type : ((IFieldSymbol)member).Type, targets, depth + 1);
            }
    }

    /// <summary>
    /// Enum reflection reads members by value without naming them: enumerating values or names,
    /// validating with IsDefined, parsing, and formatting a value by name (ToString, interpolation,
    /// string concatenation, string-enum JSON). Each reaches every member of the enum involved, since
    /// a member's presence changes the result for a value that only arrives through a cast.
    /// </summary>
    private static void AddEnumNameReads(SemanticModel model, ExpressionSyntax expression, SymbolInfo info, List<ISymbol?> targets)
    {
        if (info.Symbol is IMethodSymbol { ContainingType.SpecialType: SpecialType.System_Enum } method &&
            EnumNameMethods.Contains(method.Name))
        {
            foreach (var argument in method.TypeArguments)
                AddEnumMembers(argument, targets);
            if (expression is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax access)
                    AddEnumMembers(model.GetTypeInfo(access.Expression).Type, targets);
                foreach (var argument in invocation.ArgumentList.Arguments)
                    AddEnumMembers(argument.Expression is TypeOfExpressionSyntax typeOf
                        ? model.GetTypeInfo(typeOf.Type).Type
                        : model.GetTypeInfo(argument.Expression).Type, targets);
            }
        }
        if (expression.Parent is InterpolationSyntax)
            AddEnumMembers(model.GetTypeInfo(expression).Type, targets);
        if (expression is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } concatenation &&
            model.GetTypeInfo(concatenation).Type?.SpecialType == SpecialType.System_String)
        {
            AddEnumMembers(model.GetTypeInfo(concatenation.Left).Type, targets);
            AddEnumMembers(model.GetTypeInfo(concatenation.Right).Type, targets);
        }
        if (expression is GenericNameSyntax && info.Symbol is INamedTypeSymbol { Name: "JsonStringEnumConverter", IsGenericType: true } converter)
            foreach (var argument in converter.TypeArguments)
                AddEnumMembers(argument, targets);
    }

    /// <summary>System.Enum methods whose result depends on which members are declared.</summary>
    private static readonly HashSet<string> EnumNameMethods =
    [
        "GetValues", "GetNames", "GetValuesAsUnderlyingType", "IsDefined", "Parse", "TryParse", "GetName",
        "Format", nameof(ToString),
    ];

    /// <summary>Adds enum constants whose names or values are observed by reflection or formatting.</summary>
    private static void AddEnumMembers(ITypeSymbol? type, List<ISymbol?> targets)
    {
        if (type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
            targets.AddRange(enumType.GetMembers().OfType<IFieldSymbol>());
        else if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            AddEnumMembers(nullable.TypeArguments[0], targets);
    }

    /// <summary>Adds the deconstructor and any user-defined conversions used by deconstruction.</summary>
    private static void AddDeconstruction(DeconstructionInfo info, List<ISymbol?> targets)
    {
        targets.Add(info.Method);
        if (info.Conversion is { IsUserDefined: true } conversion)
            targets.Add(conversion.MethodSymbol);
        foreach (var nested in info.Nested)
            AddDeconstruction(nested, targets);
    }

    /// <summary>Finds the base constructor callable without explicit arguments.</summary>
    private static IMethodSymbol? ParameterlessBaseConstructor(INamedTypeSymbol type) =>
        type.BaseType?.InstanceConstructors.FirstOrDefault(c => c.Parameters.All(p => p.IsOptional || p.IsParams));

    /// <summary>Checks whether a node occurs within a <c>nameof</c> operand, which does not execute a reference.</summary>
    private static bool IsInsideNameof(SyntaxNode node)
    {
        foreach (var invocation in node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>())
            if (invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" }
                && invocation.ArgumentList.Span.Contains(node.Span))
                return true;
        return false;
    }

    /// <summary>The declaration whose reachability makes this reference happen.</summary>
    /// <summary>Finds the repository declaration or entry point whose execution owns a source reference.</summary>
    private string? OwnerKey(Project project, SemanticModel model, SyntaxNode node)
    {
        foreach (var ancestor in node.Ancestors())
        {
            switch (ancestor)
            {
                case GlobalStatementSyntax when model.Compilation.GetEntryPoint(CancellationToken.None) is { } entry:
                    return roots.EntryPointKey(entry, project.Name);
                case VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax } field:
                    return identity.Key(model.GetDeclaredSymbol(field));
                case BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax
                    or EventDeclarationSyntax or EnumMemberDeclarationSyntax or BaseTypeDeclarationSyntax
                    or DelegateDeclarationSyntax:
                    return identity.Key(model.GetDeclaredSymbol(ancestor));
            }
        }
        return null;
    }
}
