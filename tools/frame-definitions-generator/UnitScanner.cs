using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What one unit's code references: the units it may run and the enum members it names.</summary>
internal sealed class UnitScan
{
    /// <summary>The units the code may run: members it calls, reads, constructs or takes as method groups, and initializations.</summary>
    public List<Unit> Targets { get; } = [];

    /// <summary>The enum members the code names.</summary>
    public List<IFieldSymbol> EnumMembers { get; } = [];
}

/// <summary>Reads, once per unit, the semantic references in that unit's code.</summary>
/// <param name="model">The Core facts.</param>
internal sealed class UnitScanner(CoreModel model)
{
    private readonly Dictionary<Unit, UnitScan> _scans = [];

    /// <summary>The references in <paramref name="unit"/>'s code.</summary>
    public UnitScan Scan(Unit unit)
    {
        if (!_scans.TryGetValue(unit, out UnitScan? scan))
        {
            scan = new UnitScan();
            switch (unit.Kind)
            {
                case UnitKind.StaticInitialization:
                    AddInitializers(scan, unit.Type, isStatic: true);
                    break;
                case UnitKind.InstanceInitialization:
                    AddInitializers(scan, unit.Type, isStatic: false);
                    break;
                default:
                    ScanMember(scan, unit.Symbol);
                    break;
            }
            _scans.Add(unit, scan);
        }
        return scan;
    }

    /// <summary>Adds the initialized fields and properties (and for static initialization the static constructor) of a type.</summary>
    private void AddInitializers(UnitScan scan, INamedTypeSymbol type, bool isStatic)
    {
        foreach (ISymbol member in type.GetMembers())
        {
            if (member.IsStatic != isStatic || member.IsImplicitlyDeclared)
                continue;
            switch (member)
            {
                case IFieldSymbol { IsConst: false } field when field.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is VariableDeclaratorSyntax { Initializer: not null }):
                    scan.Targets.Add(Unit.Member(field.OriginalDefinition));
                    break;
                case IPropertySymbol property when property.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is PropertyDeclarationSyntax { Initializer: not null }):
                    scan.Targets.Add(Unit.Member(property.OriginalDefinition));
                    break;
                case IMethodSymbol { MethodKind: MethodKind.StaticConstructor } constructor:
                    scan.Targets.Add(Unit.Member(constructor.OriginalDefinition));
                    break;
            }
        }
    }

    /// <summary>Scans one member's declaration, and for a constructor the initialization it runs.</summary>
    private void ScanMember(UnitScan scan, ISymbol member)
    {
        switch (member)
        {
            case IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructor:
                ScanConstructor(scan, constructor);
                break;
            // Synthesized members (a record's equality, printing and cloning) have no code of their own; their
            // declaring reference is the whole type declaration, which is not what they run.
            case { IsImplicitlyDeclared: true }:
                break;
            case IFieldSymbol field:
                foreach (SyntaxReference reference in field.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                        Visit(scan, initializer);
                }
                break;
            case IPropertySymbol or IEventSymbol or IMethodSymbol:
                foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
                {
                    SyntaxNode declaration = reference.GetSyntax();
                    // A positional record's property is its constructor parameter, which has no code of its own.
                    if (declaration is not ParameterSyntax)
                        Visit(scan, declaration);
                }
                break;
            default:
                throw new InvalidOperationException($"{member.ToDisplayString()} is not a member this generator scans.");
        }
    }

    /// <summary>
    /// Scans a constructor: its body and initializer, or a primary constructor's parameters and base arguments; the
    /// type's static initialization; its instance initializers unless it chains to another constructor of its type;
    /// and the base constructor it implicitly calls.
    /// </summary>
    private void ScanConstructor(UnitScan scan, IMethodSymbol constructor)
    {
        INamedTypeSymbol type = constructor.ContainingType;
        if (constructor.MethodKind == MethodKind.StaticConstructor)
        {
            foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
                Visit(scan, reference.GetSyntax());
            return;
        }

        bool chainsToThis = false;
        bool callsBase = false;
        // An implicitly declared constructor (a default or record copy constructor) runs only initialization.
        IEnumerable<SyntaxReference> declarations = constructor.IsImplicitlyDeclared ? [] : constructor.DeclaringSyntaxReferences;
        foreach (SyntaxReference reference in declarations)
        {
            switch (reference.GetSyntax())
            {
                case ConstructorDeclarationSyntax declaration:
                    Visit(scan, declaration);
                    chainsToThis |= declaration.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword) == true;
                    callsBase |= declaration.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword) == true;
                    break;
                case TypeDeclarationSyntax primary:
                    if (primary.ParameterList is { } parameters)
                        Visit(scan, parameters);
                    foreach (PrimaryConstructorBaseTypeSyntax baseCall in primary.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>()
                                 ?? Enumerable.Empty<PrimaryConstructorBaseTypeSyntax>())
                    {
                        Visit(scan, baseCall);
                        callsBase = true;
                    }
                    break;
            }
        }
        scan.Targets.Add(Unit.StaticInitialization(type));
        if (chainsToThis)
            return;
        scan.Targets.Add(Unit.InstanceInitialization(type));
        if (!callsBase && type.BaseType is { } baseType && model.IsSource(baseType.OriginalDefinition)
            && baseType.OriginalDefinition.InstanceConstructors.FirstOrDefault(c => c.Parameters.All(p => p.IsOptional || p.IsParams)) is { } implicitBase)
        {
            scan.Targets.Add(Unit.Member(implicitBase.OriginalDefinition));
        }
    }

    /// <summary>
    /// Binds every reference in <paramref name="root"/>'s code. Attributes, nameof and typed-start entries are not code
    /// that runs, and a value a case label, constant pattern or equality test compares against is tested, not used:
    /// those are skipped.
    /// </summary>
    private void Visit(UnitScan scan, SyntaxNode root)
    {
        SemanticModel semantic = model.ModelOf(root.SyntaxTree);
        var pending = new Stack<SyntaxNode>([root]);
        while (pending.Count > 0)
        {
            SyntaxNode node = pending.Pop();
            if (node is AttributeListSyntax or CaseSwitchLabelSyntax or ConstantPatternSyntax or RelationalPatternSyntax
                || model.IsStartEntry(node))
            {
                continue;
            }
            if (node is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } } nameOf
                && semantic.GetSymbolInfo(nameOf).Symbol is null)
            {
                continue;
            }
            Bind(scan, semantic, node);
            bool comparison = node.IsKind(SyntaxKind.EqualsExpression) || node.IsKind(SyntaxKind.NotEqualsExpression);
            foreach (SyntaxNode child in node.ChildNodes())
            {
                if (comparison && semantic.GetSymbolInfo(child).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum })
                    continue;
                pending.Push(child);
            }
        }
    }

    /// <summary>Records the symbols <paramref name="node"/> references.</summary>
    private void Bind(UnitScan scan, SemanticModel semantic, SyntaxNode node)
    {
        switch (node)
        {
            case SimpleNameSyntax or BaseObjectCreationExpressionSyntax or ElementAccessExpressionSyntax or ImplicitElementAccessSyntax
                or ConstructorInitializerSyntax or PrimaryConstructorBaseTypeSyntax or BinaryExpressionSyntax
                or PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax or AssignmentExpressionSyntax or CastExpressionSyntax:
                SymbolInfo info = semantic.GetSymbolInfo(node);
                if (info.Symbol is { } symbol)
                    Add(scan, symbol);
                else if (info.CandidateReason is CandidateReason.MemberGroup or CandidateReason.OverloadResolutionFailure)
                {
                    foreach (ISymbol candidate in info.CandidateSymbols)
                        Add(scan, candidate);
                }
                break;
            case CommonForEachStatementSyntax loop:
                ForEachStatementInfo loopInfo = semantic.GetForEachStatementInfo(loop);
                foreach (ISymbol? member in new ISymbol?[] { loopInfo.GetEnumeratorMethod, loopInfo.MoveNextMethod, loopInfo.CurrentProperty })
                {
                    if (member is not null)
                        Add(scan, member);
                }
                break;
        }
        if (node is ExpressionSyntax expression && !SyntaxFacts.IsInTypeOnlyContext(expression)
            && semantic.GetConversion(expression) is { IsUserDefined: true, MethodSymbol: { } conversion })
            Add(scan, conversion);
    }

    /// <summary>Records one referenced symbol as an enum member or as the units it reaches.</summary>
    private void Add(UnitScan scan, ISymbol symbol)
    {
        switch (symbol)
        {
            case IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member:
                scan.EnumMembers.Add(member.OriginalDefinition);
                break;
            case IFieldSymbol field:
                IFieldSymbol definition = field.OriginalDefinition;
                if (!model.IsSource(definition) || definition.IsImplicitlyDeclared)
                    return;
                scan.Targets.Add(Unit.Member(definition));
                // Reading static data runs the type's whole static initialization; a constant is inlined and runs nothing.
                if (definition.IsStatic && !definition.IsConst)
                    scan.Targets.Add(Unit.StaticInitialization(definition.ContainingType));
                break;
            case IPropertySymbol property:
                if (model.NormalizeProperty(property) is not { } normalized)
                    return;
                scan.Targets.Add(Unit.Member(normalized));
                if (normalized.IsStatic && CoreModel.IsDataProperty(normalized))
                    scan.Targets.Add(Unit.StaticInitialization(normalized.ContainingType));
                break;
            case IMethodSymbol method:
                switch (model.NormalizeMethod(method))
                {
                    // An accessor reaches its property, with the static data that property may be.
                    case IPropertySymbol accessed:
                        Add(scan, accessed);
                        break;
                    case { } target:
                        scan.Targets.Add(Unit.Member(target));
                        break;
                }
                break;
            case IEventSymbol @event when model.IsSource(@event):
                scan.Targets.Add(Unit.Member(@event.OriginalDefinition));
                break;
        }
    }
}
