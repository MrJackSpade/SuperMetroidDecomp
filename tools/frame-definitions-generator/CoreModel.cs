using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>An enemy behaviour: a type that declares the definitions whose enemies it creates.</summary>
/// <param name="Type">The behaviour type.</param>
/// <param name="Family">Its family name: the type name without a Behavior or Factory suffix.</param>
/// <param name="Path">The Core-relative path of the file declaring its definitions.</param>
/// <param name="Definitions">The EnemyDefinitionId member names it serves, in declaration order.</param>
/// <param name="PartsTable">The parts table its definitions come from, when they come from one.</param>
/// <param name="Parts">The part the parts table maps each definition to, when its entry names an enum member.</param>
internal sealed record Behaviour(
    INamedTypeSymbol Type, string Family, string Path, IReadOnlyList<string> Definitions, ISymbol? PartsTable,
    IReadOnlyDictionary<string, IFieldSymbol> Parts);

/// <summary>
/// The semantic facts the generator reads from SuperMetroid.Core: its types, the enemy behaviours and the definitions
/// they serve, the projectile program families and the typed-start tables naming the family each kind starts, the
/// types each behaviour owns, and which members implement or override which.
/// </summary>
internal sealed class CoreModel
{
    /// <summary>Tables that name every kind or frame; reading one would make every kind or frame every reader's.</summary>
    private static readonly string[] TableNames =
    [
        "EnemyProjectileDefinitionCatalog",
        "RoomSpriteObjectVisualDefinitions",
        "RoomSpriteObjectDefinitions",
        "EnemyFrameNameDefinitions",
    ];

    /// <summary>
    /// The types outside every behaviour whose members are each a common spawner of their own: the enemy projectile
    /// engine's spawn services serve unrelated rooms (the save station's electricity, the dud shot, the dust explosion),
    /// so each runs where its own callers do rather than wherever any of them does.
    /// </summary>
    private static readonly string[] MemberSpawnerTypeNames = ["EnemyProjectileEngine"];

    /// <summary>The owners marker for a type whose owners are still being resolved, so a cycle contributes nothing.</summary>
    private static readonly HashSet<INamedTypeSymbol> Resolving = new(SymbolEqualityComparer.Default);

    private readonly Dictionary<SyntaxTree, SemanticModel> _models = [];
    private readonly HashSet<INamedTypeSymbol> _tables = new(SymbolEqualityComparer.Default);
    private readonly HashSet<INamedTypeSymbol> _memberSpawnerTypes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<string, INamedTypeSymbol> _spawnerTypes = new(StringComparer.Ordinal);
    private readonly HashSet<INamedTypeSymbol> _families = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, Behaviour> _behaviours = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> _creationContexts = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, IReadOnlySet<INamedTypeSymbol>?> _owners = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ISymbol, List<ISymbol>> _dispatch = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IFieldSymbol, INamedTypeSymbol> _starts = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IFieldSymbol, IReadOnlySet<string>> _startPrograms = new(SymbolEqualityComparer.Default);
    private readonly HashSet<SyntaxNode> _startEntries = [];
    private readonly HashSet<INamedTypeSymbol> _constructedFamilies = new(SymbolEqualityComparer.Default);
    private readonly INamedTypeSymbol _entity;
    private readonly Dictionary<INamedTypeSymbol, bool> _entityTypes = new(SymbolEqualityComparer.Default);
    private readonly INamedTypeSymbol _projectileBehaviour;
    private readonly Dictionary<IMethodSymbol, List<CallSite>> _callSites = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, HashSet<IParameterSymbol>> _programParameters = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, bool> _projectileBehaviourTypes = new(SymbolEqualityComparer.Default);

    /// <summary>Reads every fact the walks need from <paramref name="core"/>, failing on anything it cannot resolve.</summary>
    public CoreModel(CoreSource core)
    {
        Compilation = core.Compilation;
        Directory = core.Directory;
        Types = CollectTypes();
        EnemyFrameId = SingleType("EnemyFrameId");
        EnemyProjectileFrameId = SingleType("EnemyProjectileFrameId");
        RoomSpriteObjectKind = SingleType("RoomSpriteObjectKind");
        RoomEnemyProjectileKind = SingleType("RoomEnemyProjectileKind");
        EnemyDefinitionId = SingleType("EnemyDefinitionId");
        ArchitectureReview = SingleType("ArchitectureReviewAttribute");
        EnemySheetDraw = SingleType("EnemySheetDraw");
        SpriteGraphics = SingleType("SpriteGraphics");
        SpawnedObjectOwner = SingleType("SpawnedObjectOwner");
        RoomEnemySlot = SingleType("RoomEnemySlot");
        _entity = SingleType("IEnemyEntity");
        INamedTypeSymbol factory = SingleType("IEnemyEntityFactory");
        INamedTypeSymbol typedAnimation = SingleType("EnemyProjectileTypedAnimation", arity: 3);
        TypedAnimation = typedAnimation;
        _projectileBehaviour = SingleType("EnemyProjectileBehavior");
        foreach (string name in TableNames)
            _tables.Add(SingleType(name));
        foreach (string name in MemberSpawnerTypeNames)
            _memberSpawnerTypes.Add(SingleType(name));

        foreach (INamedTypeSymbol type in Types)
        {
            if (type.TypeKind == TypeKind.Class && !type.IsAbstract && DerivesFrom(type, typedAnimation))
                _families.Add(type);
        }
        foreach (INamedTypeSymbol type in Types)
        {
            if (type.TypeKind == TypeKind.Class && !type.IsAbstract
                && type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, factory)))
            {
                _behaviours.Add(type, ReadBehaviour(type));
            }
        }
        Behaviours = [.. _behaviours.Values.OrderBy(b => b.Path, StringComparer.Ordinal)];
        CheckDistinct();
        foreach (SyntaxTree tree in Compilation.SyntaxTrees)
            ReadTree(tree);
        BuildDispatch();
    }

    /// <summary>The Core compilation.</summary>
    public Compilation Compilation { get; }

    /// <summary>The Core project directory.</summary>
    public string Directory { get; }

    /// <summary>Every source type in Core, nested types included, in source order.</summary>
    public IReadOnlyList<INamedTypeSymbol> Types { get; }

    /// <summary>The enemy frame identity.</summary>
    public INamedTypeSymbol EnemyFrameId { get; }

    /// <summary>The enemy projectile frame identity.</summary>
    public INamedTypeSymbol EnemyProjectileFrameId { get; }

    /// <summary>The room sprite-object kind identity.</summary>
    public INamedTypeSymbol RoomSpriteObjectKind { get; }

    /// <summary>The enemy projectile kind identity.</summary>
    public INamedTypeSymbol RoomEnemyProjectileKind { get; }

    /// <summary>The enemy definition identity.</summary>
    public INamedTypeSymbol EnemyDefinitionId { get; }

    /// <summary>The explicit draw of a named definition's sheet frame.</summary>
    public INamedTypeSymbol EnemySheetDraw { get; }

    /// <summary>The graphics a spawned object draws with: whose sheet, in which shading.</summary>
    public INamedTypeSymbol SpriteGraphics { get; }

    /// <summary>Whose spawned-object sheet a spawned object draws from.</summary>
    public INamedTypeSymbol SpawnedObjectOwner { get; }

    /// <summary>An enemy slot.</summary>
    public INamedTypeSymbol RoomEnemySlot { get; }

    /// <summary>
    /// The base every projectile program family derives from (#1600), whose type arguments are its segment, frame and event
    /// types; the segment type names the programs (the segments an animation starts at).
    /// </summary>
    public INamedTypeSymbol TypedAnimation { get; }

    /// <summary>What each projectile program family's animations show, read from their segments and animations once.</summary>
    public ProjectileProgramFrames ProgramFrames => _programFrames ??= new ProjectileProgramFrames(this);

    private ProjectileProgramFrames? _programFrames;

    /// <summary>Whether <paramref name="type"/> is an entity type: one implementing IEnemyEntity.</summary>
    public bool IsEntityType(INamedTypeSymbol type)
    {
        if (!_entityTypes.TryGetValue(type, out bool entity))
        {
            entity = type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, _entity));
            _entityTypes.Add(type, entity);
        }
        return entity;
    }

    /// <summary>The EnemyDefinitionId member named <paramref name="name"/>.</summary>
    public IFieldSymbol DefinitionField(string name) =>
        EnemyDefinitionId.GetMembers(name) is [IFieldSymbol field]
            ? field
            : throw new InvalidDataException($"EnemyDefinitionId has no single member {name}.");

    /// <summary>The attribute the generated classes carry.</summary>
    public INamedTypeSymbol ArchitectureReview { get; }

    /// <summary>Every enemy behaviour, ordered by the path of the file declaring its definitions.</summary>
    public IReadOnlyList<Behaviour> Behaviours { get; }

    /// <summary>The families code constructs outside the typed-start entries (by constructor or a factory of their own).</summary>
    public IReadOnlySet<INamedTypeSymbol> ConstructedFamilies => _constructedFamilies;

    /// <summary>Every projectile program family, in source order.</summary>
    public IEnumerable<INamedTypeSymbol> Families => Types.Where(_families.Contains);

    /// <summary>The semantic model of <paramref name="tree"/>.</summary>
    public SemanticModel ModelOf(SyntaxTree tree)
    {
        if (!_models.TryGetValue(tree, out SemanticModel? model))
        {
            model = Compilation.GetSemanticModel(tree);
            _models.Add(tree, model);
        }
        return model;
    }

    /// <summary>The initializer value of Core type <paramref name="type"/>'s property or field <paramref name="member"/>.</summary>
    /// <exception cref="InvalidDataException">No one type has that name, it declares no one such member, or the member has no initializer.</exception>
    public ExpressionSyntax MemberInitializer(string type, string member) => InitializerOf(Member(type, member));

    /// <summary>Core type <paramref name="type"/>'s property or field <paramref name="member"/>.</summary>
    /// <exception cref="InvalidDataException">No one type has that name, or it declares no one such member.</exception>
    public ISymbol Member(string type, string member)
    {
        INamedTypeSymbol[] types = [.. Types.Where(candidate => candidate.Name == type)];
        if (types is not [var declaring])
            throw new InvalidDataException($"Expected one Core type {type}; found {types.Length}.");
        return declaring.GetMembers(member) is [var single and (IPropertySymbol or IFieldSymbol)]
            ? single
            : throw new InvalidDataException($"{type} does not declare one property or field {member}.");
    }

    /// <summary>The initializer value of property or field <paramref name="member"/>.</summary>
    /// <exception cref="InvalidDataException">The member has no initializer this generator can read.</exception>
    public static ExpressionSyntax InitializerOf(ISymbol member)
    {
        foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
        {
            switch (reference.GetSyntax())
            {
                case PropertyDeclarationSyntax { Initializer.Value: var value }:
                    return value;
                case PropertyDeclarationSyntax { ExpressionBody.Expression: var body }:
                    return body;
                case VariableDeclaratorSyntax { Initializer.Value: var value }:
                    return value;
            }
        }
        throw new InvalidDataException($"{member.ToDisplayString()} has no initializer this generator can read.");
    }

    /// <summary>
    /// The initializer of <paramref name="member"/>, followed by the initializer of every dictionary member it spreads in, in
    /// the order spread, as <c>TorizoAnimations.Segments</c> joins four tables of segments.
    /// </summary>
    public List<ExpressionSyntax> InitializersWithSpreads(ISymbol member)
    {
        var initializers = new List<ExpressionSyntax>();
        var pending = new Queue<ExpressionSyntax>([InitializerOf(member)]);
        while (pending.TryDequeue(out ExpressionSyntax? initializer))
        {
            initializers.Add(initializer);
            SemanticModel semantic = ModelOf(initializer.SyntaxTree);
            foreach (SpreadElementSyntax spread in initializer.DescendantNodes().OfType<SpreadElementSyntax>())
            {
                if (semantic.GetTypeInfo(spread.Expression).Type is INamedTypeSymbol { Name: "Dictionary" or "IReadOnlyDictionary" }
                    && semantic.GetSymbolInfo(spread.Expression).Symbol is (IPropertySymbol or IFieldSymbol) and var table)
                    pending.Enqueue(InitializerOf(table.OriginalDefinition));
            }
        }
        return initializers;
    }

    /// <summary>The EnemyFrameId members <paramref name="initializer"/> names.</summary>
    public HashSet<string> EnemyFramesNamed(ExpressionSyntax initializer)
    {
        SemanticModel semantic = ModelOf(initializer.SyntaxTree);
        return new HashSet<string>(initializer.DescendantNodes().OfType<ExpressionSyntax>()
            .Select(node => semantic.GetSymbolInfo(node).Symbol).OfType<IFieldSymbol>()
            .Where(field => SymbolEqualityComparer.Default.Equals(field.ContainingType, EnemyFrameId))
            .Select(field => field.Name), StringComparer.Ordinal);
    }

    /// <summary>Whether <paramref name="symbol"/> is declared in Core (implicitly declared members of Core types included).</summary>
    public bool IsSource(ISymbol symbol) => SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, Compilation.Assembly);

    /// <summary>The program family a typed-start entry starts for <paramref name="kind"/>, if the kind has one.</summary>
    public INamedTypeSymbol? StartedFamily(IFieldSymbol kind) => _starts.GetValueOrDefault(kind);

    /// <summary>
    /// The programs a typed-start entry starts <paramref name="kind"/>'s family in, when its construction names them as
    /// constants; null when it names none or the kind has no entry.
    /// </summary>
    public IReadOnlySet<string>? StartedPrograms(IFieldSymbol kind) => _startPrograms.GetValueOrDefault(kind);

    /// <summary>
    /// The programs <paramref name="creation"/>, a construction of a projectile program family, can start it in: the
    /// members of the family's program type its first argument names, each branch of a conditional or switch among them;
    /// null when any branch is no such member.
    /// </summary>
    public IReadOnlySet<string>? ProgramsOf(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, INamedTypeSymbol family)
    {
        INamedTypeSymbol? programType = ProgramTypeOf(family);
        if (creation.ArgumentList is not { Arguments: [var first, ..] } || programType is null)
            return null;
        var programs = new HashSet<string>(StringComparer.Ordinal);
        return AddPrograms(semantic, first.Expression, programType, programs) ? programs : null;
    }

    /// <summary>
    /// The type naming <paramref name="family"/>'s programs (the segments an animation starts at): the first type argument
    /// of the typed animation it derives from; null when it derives from none.
    /// </summary>
    public INamedTypeSymbol? ProgramTypeOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, TypedAnimation))
                return type.TypeArguments[0] as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Every place Core calls <paramref name="callee"/> (#1784): invocations, constructions (target-typed ones included),
    /// and constructor initializers and primary-constructor base calls, each with the unit whose code makes it.
    /// </summary>
    public IReadOnlyList<CallSite> CallSites(IMethodSymbol callee)
    {
        IMethodSymbol definition = callee.OriginalDefinition;
        if (_callSites.TryGetValue(definition, out List<CallSite>? known))
            return known;
        bool constructor = definition.MethodKind == MethodKind.Constructor;
        var sites = new List<CallSite>();
        foreach (SyntaxTree tree in Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                // Only a call spelled with the callee's name (or a construction that may make its type) can call it; the
                // rest need no binding.
                ArgumentListSyntax? arguments = node switch
                {
                    InvocationExpressionSyntax invocation when !constructor && CalledName(invocation.Expression) == definition.Name => invocation.ArgumentList,
                    ObjectCreationExpressionSyntax creation when constructor && CalledName(creation.Type) == definition.ContainingType.Name =>
                        creation.ArgumentList ?? SyntaxFactory.ArgumentList(),
                    ImplicitObjectCreationExpressionSyntax creation when constructor => creation.ArgumentList,
                    ConstructorInitializerSyntax initializer when constructor => initializer.ArgumentList,
                    PrimaryConstructorBaseTypeSyntax baseCall when constructor => baseCall.ArgumentList,
                    _ => null,
                };
                if (arguments is null)
                    continue;
                semantic ??= ModelOf(tree);
                if (semantic.GetSymbolInfo(node).Symbol is not IMethodSymbol called
                    || !SymbolEqualityComparer.Default.Equals((called.ReducedFrom ?? called).OriginalDefinition, definition))
                    continue;
                sites.Add(new CallSite(arguments, semantic, CallerOf(semantic, node)));
            }
        }
        _callSites.Add(definition, sites);
        return sites;
    }

    /// <summary>The name a call or construction spells for what it calls or makes: the rightmost simple name.</summary>
    private static string? CalledName(ExpressionSyntax expression) => expression switch
    {
        SimpleNameSyntax name => name.Identifier.Text,
        MemberAccessExpressionSyntax access => access.Name.Identifier.Text,
        MemberBindingExpressionSyntax binding => binding.Name.Identifier.Text,
        QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
        AliasQualifiedNameSyntax alias => alias.Name.Identifier.Text,
        _ => null,
    };

    /// <summary>
    /// The unit whose code <paramref name="node"/> is: the member declaring it (a lambda's or local function's code being
    /// that member's, a property initializer's or accessor's its property's, a primary constructor's base call its
    /// constructor's); null outside every member.
    /// </summary>
    private Unit? CallerOf(SemanticModel semantic, SyntaxNode node)
    {
        foreach (SyntaxNode ancestor in node.Ancestors())
        {
            ISymbol? declared = ancestor switch
            {
                BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax => semantic.GetDeclaredSymbol(ancestor),
                VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax } declarator => semantic.GetDeclaredSymbol(declarator),
                TypeDeclarationSyntax type => (semantic.GetDeclaredSymbol(type) as INamedTypeSymbol)?.InstanceConstructors
                    .FirstOrDefault(constructor => constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() == type)),
                _ => null,
            };
            ISymbol? member = declared switch
            {
                IMethodSymbol method => NormalizeMethod(method),
                IPropertySymbol property => NormalizeProperty(property),
                IFieldSymbol field when IsSource(field) => field.OriginalDefinition,
                IEventSymbol @event when IsSource(@event) => @event.OriginalDefinition,
                _ => null,
            };
            if (member is not null)
                return Unit.Member(member);
            if (ancestor is BaseTypeDeclarationSyntax)
                return null;
        }
        return null;
    }

    /// <summary>
    /// The parameters that hand <paramref name="family"/> the program it is constructed in (#1784): the parameter a
    /// construction's program argument is, or the positional record parameter whose property it is, and, in turn, each such
    /// parameter a call or construction passing one of them passes on. A program member passed to one of them is a start
    /// the walks record, as one passed to the construction itself is.
    /// </summary>
    public IReadOnlySet<IParameterSymbol> ProgramParameters(INamedTypeSymbol family)
    {
        if (_programParameters.TryGetValue(family, out HashSet<IParameterSymbol>? known))
            return known;
        var parameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var pending = new Queue<IParameterSymbol>();
        foreach (IMethodSymbol constructor in family.OriginalDefinition.InstanceConstructors)
        {
            if (constructor.Parameters is [var program, ..])
                Pass(constructor, program);
        }
        while (pending.TryDequeue(out IParameterSymbol? handed))
            Pass((IMethodSymbol)handed.ContainingSymbol, handed);
        _programParameters.Add(family, parameters);
        return parameters;

        // Each parameter (or positional record property) a call of the callee passes for its parameter hands the program on.
        void Pass(IMethodSymbol callee, IParameterSymbol parameter)
        {
            foreach (CallSite site in CallSites(callee))
            {
                if (site.ArgumentFor(parameter) is not { } passed)
                    continue;
                foreach (ExpressionSyntax choice in Choices(passed))
                {
                    if (HandedParameter(site.Semantic.GetSymbolInfo(choice).Symbol) is { } source && parameters.Add(source))
                        pending.Enqueue(source);
                }
            }
        }
    }

    /// <summary>
    /// The parameter <paramref name="symbol"/> reads a value handed in through: a parameter of a method (not of a lambda or
    /// local function), or the primary-constructor parameter a positional record's property is; null for anything else.
    /// </summary>
    public IParameterSymbol? HandedParameter(ISymbol? symbol) => symbol switch
    {
        IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not (MethodKind.AnonymousFunction or MethodKind.LocalFunction) } } parameter =>
            parameter.OriginalDefinition,
        IPropertySymbol property when IsSource(property) => PositionalParameter(property),
        _ => null,
    };

    /// <summary>The parameter <paramref name="argument"/> is passed for, in the call or construction holding it; null when none binds.</summary>
    public static IParameterSymbol? ParameterOf(SemanticModel semantic, ArgumentSyntax argument)
    {
        if (argument.Parent is not ArgumentListSyntax { Parent: { } call } list
            || semantic.GetSymbolInfo(call).Symbol is not IMethodSymbol { ReducedFrom: null } method)
            return null;
        IParameterSymbol? parameter = argument.NameColon is { } name
            ? method.Parameters.FirstOrDefault(candidate => candidate.Name == name.Name.Identifier.Text)
            : list.Arguments.IndexOf(argument) is var index && index < method.Parameters.Length ? method.Parameters[index] : null;
        return parameter?.OriginalDefinition;
    }

    /// <summary>The values <paramref name="expression"/> may be, through parentheses, conditionals and switch expressions.</summary>
    public static IEnumerable<ExpressionSyntax> Choices(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Choices(parenthesized.Expression);
            case ConditionalExpressionSyntax conditional:
                return Choices(conditional.WhenTrue).Concat(Choices(conditional.WhenFalse));
            case SwitchExpressionSyntax choice:
                return choice.Arms.SelectMany(arm => Choices(arm.Expression));
            default:
                return [expression];
        }
    }

    /// <summary>
    /// The primary-constructor parameter a positional record's <paramref name="property"/> is (its containing symbol the
    /// constructor); null for any other property.
    /// </summary>
    public IParameterSymbol? PositionalParameter(IPropertySymbol property)
    {
        foreach (SyntaxReference reference in property.OriginalDefinition.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ParameterSyntax parameter
                && ModelOf(parameter.SyntaxTree).GetDeclaredSymbol(parameter) is { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } } declared)
                return declared;
        }
        return null;
    }

    /// <summary>
    /// Adds the members of <paramref name="programType"/> <paramref name="expression"/> can be to <paramref name="programs"/>,
    /// through conditionals, switches and parentheses; false when some branch is no such member.
    /// </summary>
    private static bool AddPrograms(SemanticModel semantic, ExpressionSyntax expression, INamedTypeSymbol programType, HashSet<string> programs)
    {
        switch (expression)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return AddPrograms(semantic, parenthesized.Expression, programType, programs);
            case ConditionalExpressionSyntax conditional:
                return AddPrograms(semantic, conditional.WhenTrue, programType, programs) & AddPrograms(semantic, conditional.WhenFalse, programType, programs);
            case SwitchExpressionSyntax choice:
                bool all = true;
                foreach (SwitchExpressionArmSyntax arm in choice.Arms)
                    all &= AddPrograms(semantic, arm.Expression, programType, programs);
                return all;
        }
        if (semantic.GetSymbolInfo(expression).Symbol is not IFieldSymbol program || !SymbolEqualityComparer.Default.Equals(program.ContainingType, programType))
            return false;
        programs.Add(program.Name);
        return true;
    }

    /// <summary>Whether <paramref name="node"/> is a typed-start entry, which the walks read as its kind's family, not as code.</summary>
    public bool IsStartEntry(SyntaxNode node) => _startEntries.Contains(node);

    /// <summary>The members implementing or overriding <paramref name="member"/>.</summary>
    public IReadOnlyList<ISymbol> Implementations(ISymbol member) =>
        _dispatch.TryGetValue(member, out List<ISymbol>? implementations) ? implementations : [];

    /// <summary>Whether <paramref name="type"/> is, or is nested in, a table that names every kind or frame.</summary>
    public bool IsTable(INamedTypeSymbol type) => Chain(type).Any(_tables.Contains);

    /// <summary>
    /// Whether <paramref name="type"/> is, or is nested in, an enemy projectile behaviour (#1760): EnemyProjectileBehavior or
    /// a type deriving from it, whose members the projectile engine runs for whichever projectile its spawner bound it to.
    /// </summary>
    public bool IsProjectileBehaviour(INamedTypeSymbol type)
    {
        INamedTypeSymbol definition = type.OriginalDefinition;
        if (!_projectileBehaviourTypes.TryGetValue(definition, out bool behaviour))
        {
            behaviour = Chain(definition).Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, _projectileBehaviour)
                || DerivesFrom(candidate, _projectileBehaviour));
            _projectileBehaviourTypes.Add(definition, behaviour);
        }
        return behaviour;
    }

    /// <summary>The program family <paramref name="type"/> is or is nested in, if any.</summary>
    public INamedTypeSymbol? FamilyOf(INamedTypeSymbol type) => Chain(type).FirstOrDefault(_families.Contains);

    /// <summary>The behaviour <paramref name="type"/> is or is nested in, if any.</summary>
    public INamedTypeSymbol? BehaviourOf(INamedTypeSymbol type) => Chain(type).FirstOrDefault(_behaviours.ContainsKey);

    /// <summary>
    /// The behaviours whose code <paramref name="type"/> is: the behaviour it is or is nested in, or the behaviours of
    /// every place that constructs it when all of them are behaviour code; null for shared code, which a type
    /// constructed outside every behaviour, or never constructed, is.
    /// </summary>
    public IReadOnlySet<INamedTypeSymbol>? OwnersOf(INamedTypeSymbol type)
    {
        IReadOnlySet<INamedTypeSymbol>? owners = ResolveOwners(type.OriginalDefinition);
        return ReferenceEquals(owners, Resolving) ? null : owners;
    }

    /// <summary>Whether <paramref name="type"/> is behaviour code that is not <paramref name="behaviour"/>'s.</summary>
    /// <param name="type">The type of the code.</param>
    /// <param name="behaviour">The behaviour whose walk asks, or null for a walk outside every behaviour.</param>
    public bool IsForeign(INamedTypeSymbol type, INamedTypeSymbol? behaviour)
    {
        IReadOnlySet<INamedTypeSymbol>? owners = OwnersOf(type);
        return owners is not null && (behaviour is null || !owners.Contains(behaviour));
    }

    /// <summary>The units holding every piece of <paramref name="type"/>'s own code (its nested types excluded).</summary>
    public IEnumerable<Unit> UnitsOf(INamedTypeSymbol type)
    {
        if (type.TypeKind is TypeKind.Enum or TypeKind.Delegate)
            yield break;
        yield return Unit.StaticInitialization(type);
        yield return Unit.InstanceInitialization(type);
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (ISymbol member in type.GetMembers())
        {
            ISymbol? normalized = member switch
            {
                INamedTypeSymbol => null,
                IMethodSymbol { IsImplicitlyDeclared: true, MethodKind: not MethodKind.Constructor } => null,
                IMethodSymbol method => NormalizeMethod(method),
                IFieldSymbol { IsImplicitlyDeclared: true } => null,
                IFieldSymbol field => field.OriginalDefinition,
                IPropertySymbol { IsImplicitlyDeclared: true } => null,
                IPropertySymbol property => NormalizeProperty(property),
                IEventSymbol @event => @event.OriginalDefinition,
                _ => null,
            };
            if (normalized is not null && seen.Add(normalized))
                yield return Unit.Member(normalized);
        }
    }

    /// <summary>
    /// The member a reference to <paramref name="method"/> reaches: its source definition, or the property or event an
    /// accessor belongs to; null for code that is not a separate member (local functions, lambdas) or not Core's.
    /// </summary>
    public ISymbol? NormalizeMethod(IMethodSymbol method)
    {
        IMethodSymbol definition = (method.ReducedFrom ?? method).OriginalDefinition;
        if (definition.PartialImplementationPart is { } implementation)
            definition = implementation;
        switch (definition.MethodKind)
        {
            case MethodKind.PropertyGet or MethodKind.PropertySet:
                return definition.AssociatedSymbol is IPropertySymbol property ? NormalizeProperty(property) : null;
            case MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise:
                return definition.AssociatedSymbol is IEventSymbol @event && IsSource(@event) ? @event.OriginalDefinition : null;
            case MethodKind.LocalFunction or MethodKind.AnonymousFunction or MethodKind.DelegateInvoke
                or MethodKind.BuiltinOperator or MethodKind.FunctionPointerSignature:
                return null;
        }
        return IsSource(definition) ? definition : null;
    }

    /// <summary>The source definition of <paramref name="property"/>, or null when it is not Core's.</summary>
    public IPropertySymbol? NormalizeProperty(IPropertySymbol property)
    {
        IPropertySymbol definition = property.OriginalDefinition;
        if (definition.PartialImplementationPart is { } implementation)
            definition = implementation;
        return IsSource(definition) ? definition : null;
    }

    /// <summary>Whether <paramref name="property"/> is data: an auto-property or one with an initializer.</summary>
    public static bool IsDataProperty(IPropertySymbol property) =>
        property.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<PropertyDeclarationSyntax>().Any(declaration =>
            declaration.Initializer is not null
            || (declaration.ExpressionBody is null && declaration.AccessorList is { } accessors
                && accessors.Accessors.All(a => a.Body is null && a.ExpressionBody is null)));

    /// <summary><paramref name="type"/> and every type containing it, as source definitions.</summary>
    public static IEnumerable<INamedTypeSymbol> Chain(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.OriginalDefinition; current is not null; current = current.ContainingType?.OriginalDefinition)
            yield return current;
    }

    /// <summary>The Core-relative, slash-separated path of <paramref name="tree"/>.</summary>
    public string RelativePath(SyntaxTree tree) =>
        Path.GetRelativePath(Directory, tree.FilePath).Replace(Path.DirectorySeparatorChar, '/');

    /// <summary>The names of <paramref name="enumType"/>'s members sorted by declaration order.</summary>
    public static List<string> InDeclarationOrder(INamedTypeSymbol enumType, IEnumerable<string> names)
    {
        var order = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (IFieldSymbol member in enumType.GetMembers().OfType<IFieldSymbol>())
            order.Add(member.Name, order.Count);
        return [.. names.OrderBy(name => order.TryGetValue(name, out int index)
            ? index
            : throw new InvalidDataException($"{name} is not a member of {enumType.Name}."))];
    }

    /// <summary>The usings a generated file needs for <paramref name="types"/>, in the given order without repeats.</summary>
    public static IEnumerable<string> Usings(params INamedTypeSymbol[] types) =>
        types.Select(t => $"using {t.ContainingNamespace.ToDisplayString()};").Distinct(StringComparer.Ordinal);

    /// <summary>
    /// The common spawner whose code <paramref name="unit"/> is, outside every behaviour: its type's name, or for a type
    /// whose members spawn for unrelated rooms, <c>Type.Member</c>. The import names each spawner so.
    /// </summary>
    /// <exception cref="InvalidDataException">Two different types would share one spawner name.</exception>
    public string SpawnerKey(Unit unit)
    {
        INamedTypeSymbol type = Chain(unit.Type).Last();
        if (_spawnerTypes.TryGetValue(type.Name, out INamedTypeSymbol? known) && !SymbolEqualityComparer.Default.Equals(known, type))
            throw new InvalidDataException($"Common spawners {known.ToDisplayString()} and {type.ToDisplayString()} share the name {type.Name}.");
        _spawnerTypes[type.Name] = type;
        if (!_memberSpawnerTypes.Contains(type))
            return type.Name;
        string member = unit.Kind switch
        {
            UnitKind.StaticInitialization => "StaticInitialization",
            UnitKind.InstanceInitialization => "InstanceInitialization",
            _ when unit.IsConstructor => "Constructor",
            _ => unit.Symbol.Name,
        };
        INamedTypeSymbol declaring = Chain(unit.Type).First();
        return SymbolEqualityComparer.Default.Equals(declaring, type) ? $"{type.Name}.{member}" : $"{type.Name}.{declaring.Name}.{member}";
    }

    /// <summary>Where <paramref name="unit"/> is declared, as a Core-relative path and line, with the unit's name.</summary>
    public string Describe(Unit unit)
    {
        SyntaxReference? reference = unit.Symbol.DeclaringSyntaxReferences.FirstOrDefault()
            ?? unit.Type.DeclaringSyntaxReferences.FirstOrDefault();
        if (reference is null)
            return unit.ToString();
        int line = reference.SyntaxTree.GetLineSpan(reference.Span).StartLinePosition.Line + 1;
        return $"{RelativePath(reference.SyntaxTree)}:{line} {unit}";
    }

    /// <summary>Every source type of the compilation, nested included, ordered by file and position.</summary>
    private List<INamedTypeSymbol> CollectTypes()
    {
        var types = new List<INamedTypeSymbol>();
        var namespaces = new Stack<INamespaceSymbol>([Compilation.Assembly.GlobalNamespace]);
        while (namespaces.Count > 0)
        {
            INamespaceSymbol current = namespaces.Pop();
            foreach (INamespaceSymbol child in current.GetNamespaceMembers())
                namespaces.Push(child);
            var nested = new Stack<INamedTypeSymbol>(current.GetTypeMembers());
            while (nested.Count > 0)
            {
                INamedTypeSymbol type = nested.Pop();
                if (type.DeclaringSyntaxReferences.Length == 0)
                    continue;
                types.Add(type);
                foreach (INamedTypeSymbol inner in type.GetTypeMembers())
                    nested.Push(inner);
            }
        }
        return [.. types
            .OrderBy(t => t.DeclaringSyntaxReferences[0].SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(t => t.DeclaringSyntaxReferences[0].Span.Start)];
    }

    /// <summary>The one Core type named <paramref name="name"/>.</summary>
    private INamedTypeSymbol SingleType(string name, int arity = 0)
    {
        var matches = Types.Where(t => t.Name == name && t.Arity == arity).ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidDataException($"Expected exactly one Core type {name} of arity {arity}; found {matches.Count}.");
    }

    /// <summary>Whether <paramref name="type"/>'s base chain includes <paramref name="baseType"/>.</summary>
    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType))
                return true;
        }
        return false;
    }

    /// <summary>Reads the definitions <paramref name="type"/> declares, directly or through a parts table's keys.</summary>
    private Behaviour ReadBehaviour(INamedTypeSymbol type)
    {
        var declared = type.GetMembers("Definitions").OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToList();
        if (declared is not [{ DeclaringSyntaxReferences: [var reference] }])
            throw new InvalidDataException($"{type.ToDisplayString()} creates enemies but does not declare one Definitions property.");
        if (reference.GetSyntax() is not PropertyDeclarationSyntax { Initializer.Value: CollectionExpressionSyntax collection })
            throw new InvalidDataException($"{type.ToDisplayString()}.Definitions is not initialized with a collection expression.");
        SemanticModel model = ModelOf(collection.SyntaxTree);
        var names = new List<string>();
        var parts = new Dictionary<string, IFieldSymbol>(StringComparer.Ordinal);
        ISymbol? partsTable = null;
        foreach (CollectionElementSyntax element in collection.Elements)
        {
            switch (element)
            {
                case ExpressionElementSyntax { Expression: var expression }:
                    names.Add(DefinitionName(model, expression, type));
                    break;
                case SpreadElementSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Keys", Expression: var table } }:
                    ISymbol? tableSymbol = model.GetSymbolInfo(table).Symbol;
                    if (partsTable is not null)
                        throw new InvalidDataException($"{type.ToDisplayString()}.Definitions spreads more than one parts table.");
                    partsTable = tableSymbol?.OriginalDefinition;
                    foreach ((string key, IFieldSymbol? part) in PartsTableKeys(tableSymbol, type))
                    {
                        names.Add(key);
                        if (part is not null)
                            parts.Add(key, part);
                    }
                    break;
                default:
                    throw new InvalidDataException($"{type.ToDisplayString()}.Definitions has an element this generator cannot read: {element}");
            }
        }
        if (names.Count == 0)
            throw new InvalidDataException($"{type.ToDisplayString()} declares no definitions.");
        string family = Regex.Replace(type.Name, "(Behavior|Factory)$", "");
        return new Behaviour(type, family, RelativePath(collection.SyntaxTree), names, partsTable, parts);
    }

    /// <summary>The EnemyDefinitionId member <paramref name="expression"/> names.</summary>
    private string DefinitionName(SemanticModel model, ExpressionSyntax expression, INamedTypeSymbol owner) =>
        model.GetSymbolInfo(expression).Symbol is IFieldSymbol field
        && SymbolEqualityComparer.Default.Equals(field.ContainingType, EnemyDefinitionId)
            ? field.Name
            : throw new InvalidDataException($"{owner.ToDisplayString()}: {expression} is not an EnemyDefinitionId member.");

    /// <summary>The definitions keying a parts table, in its initializer's order, each with the enum member its entry maps it to.</summary>
    private List<(string Key, IFieldSymbol? Part)> PartsTableKeys(ISymbol? table, INamedTypeSymbol owner)
    {
        ExpressionSyntax? value = table switch
        {
            IPropertySymbol { DeclaringSyntaxReferences: [var reference] } =>
                (reference.GetSyntax() as PropertyDeclarationSyntax)?.Initializer?.Value,
            IFieldSymbol { DeclaringSyntaxReferences: [var reference] } =>
                (reference.GetSyntax() as VariableDeclaratorSyntax)?.Initializer?.Value,
            _ => null,
        };
        if (value is not BaseObjectCreationExpressionSyntax { Initializer: { } initializer })
            throw new InvalidDataException($"{owner.ToDisplayString()}: the parts table {table?.ToDisplayString() ?? "(unresolved)"} is not a dictionary initializer.");
        SemanticModel model = ModelOf(initializer.SyntaxTree);
        var keys = new List<(string, IFieldSymbol?)>();
        foreach (ExpressionSyntax entry in initializer.Expressions)
        {
            if (entry is not AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] }, Right: var part })
                throw new InvalidDataException($"{owner.ToDisplayString()}: the parts table entry {entry} is not an [EnemyDefinitionId.X] = ... entry.");
            IFieldSymbol? partMember = model.GetSymbolInfo(part).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member
                ? member.OriginalDefinition
                : null;
            keys.Add((DefinitionName(model, key.Expression, owner), partMember));
        }
        return keys.Count > 0 ? keys : throw new InvalidDataException($"{owner.ToDisplayString()}: the parts table {table!.ToDisplayString()} has no entries.");
    }

    /// <summary>Requires every definition and family name to belong to one behaviour.</summary>
    private void CheckDistinct()
    {
        var servedBy = new Dictionary<string, string>(StringComparer.Ordinal);
        var families = new HashSet<string>(StringComparer.Ordinal);
        foreach (Behaviour behaviour in Behaviours)
        {
            if (!families.Add(behaviour.Family))
                throw new InvalidDataException($"Two behaviours share the family name {behaviour.Family}.");
            foreach (string definition in behaviour.Definitions)
            {
                if (!servedBy.TryAdd(definition, behaviour.Family))
                    throw new InvalidDataException($"{definition} is served by both {servedBy[definition]} and {behaviour.Family}.");
            }
        }
    }

    /// <summary>Reads the typed-start entries and the places that construct each type in <paramref name="tree"/>.</summary>
    private void ReadTree(SyntaxTree tree)
    {
        SemanticModel model = ModelOf(tree);
        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            switch (node)
            {
                case AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] }, Right: LambdaExpressionSyntax start } entry
                    when model.GetSymbolInfo(key.Expression).Symbol is IFieldSymbol kind
                         && SymbolEqualityComparer.Default.Equals(kind.ContainingType, RoomEnemyProjectileKind):
                    ReadStart(model, entry, kind, start);
                    break;
                case BaseObjectCreationExpressionSyntax creation
                    when model.GetSymbolInfo(creation).Symbol is IMethodSymbol { ContainingType: { } created } && IsSource(created)
                         && node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault() is { } context:
                    INamedTypeSymbol contextType = model.GetDeclaredSymbol(context) as INamedTypeSymbol
                        ?? throw new InvalidDataException($"{RelativePath(tree)}: {context.Identifier} declares no type symbol.");
                    INamedTypeSymbol key2 = created.OriginalDefinition;
                    if (!_creationContexts.TryGetValue(key2, out List<INamedTypeSymbol>? contexts))
                        _creationContexts.Add(key2, contexts = []);
                    contexts.Add(contextType.OriginalDefinition);
                    // A family constructed in code, not by a typed-start entry its kind names, must be spawned by some code.
                    if (_families.Contains(key2) && !node.Ancestors().Any(_startEntries.Contains))
                        _constructedFamilies.Add(key2);
                    break;
            }
        }
    }

    /// <summary>Records a typed-start entry: <paramref name="kind"/> starts the family its lambda constructs.</summary>
    private void ReadStart(SemanticModel model, AssignmentExpressionSyntax entry, IFieldSymbol kind, LambdaExpressionSyntax start)
    {
        if (start is not ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 0, ExpressionBody: BaseObjectCreationExpressionSyntax creation }
            || model.GetSymbolInfo(creation).Symbol is not IMethodSymbol { ContainingType: { } family }
            || !_families.Contains(family.OriginalDefinition))
        {
            throw new InvalidDataException(
                $"{RelativePath(entry.SyntaxTree)}: the typed start for {kind.Name} does not construct a projectile program family: {entry}");
        }
        INamedTypeSymbol started = family.OriginalDefinition;
        if (_starts.TryGetValue(kind, out INamedTypeSymbol? previous) && !SymbolEqualityComparer.Default.Equals(previous, started))
            throw new InvalidDataException($"{kind.Name} starts both {previous.Name} and {started.Name}.");
        _starts[kind] = started;
        if (ProgramsOf(model, creation, started) is { } programs)
            _startPrograms[kind] = programs;
        _startEntries.Add(entry);
    }

    /// <summary>The owners of <paramref name="type"/>, or <see cref="Resolving"/> while a cycle is being resolved.</summary>
    private IReadOnlySet<INamedTypeSymbol>? ResolveOwners(INamedTypeSymbol type)
    {
        if (_owners.TryGetValue(type, out IReadOnlySet<INamedTypeSymbol>? cached))
            return cached;
        if (BehaviourOf(type) is { } behaviour)
        {
            var own = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default) { behaviour };
            _owners.Add(type, own);
            return own;
        }
        var contexts = new List<INamedTypeSymbol>();
        if (_creationContexts.TryGetValue(type, out List<INamedTypeSymbol>? sites))
            contexts.AddRange(sites);
        if (type.ContainingType is { } container)
            contexts.Add(container.OriginalDefinition);
        _owners.Add(type, Resolving);
        HashSet<INamedTypeSymbol>? owners = new(SymbolEqualityComparer.Default);
        bool contributed = false;
        foreach (INamedTypeSymbol context in contexts)
        {
            if (SymbolEqualityComparer.Default.Equals(context, type))
                continue;
            IReadOnlySet<INamedTypeSymbol>? contextOwners = ResolveOwners(context);
            if (ReferenceEquals(contextOwners, Resolving))
                continue;
            if (contextOwners is null)
            {
                owners = null;
                break;
            }
            owners.UnionWith(contextOwners);
            contributed = true;
        }
        IReadOnlySet<INamedTypeSymbol>? result = owners is not null && contributed ? owners : null;
        _owners[type] = result;
        return result;
    }

    /// <summary>Indexes, for every Core interface and overridable member, the Core members implementing or overriding it.</summary>
    private void BuildDispatch()
    {
        foreach (INamedTypeSymbol type in Types)
        {
            if (type.TypeKind == TypeKind.Interface)
                continue;
            foreach (INamedTypeSymbol implemented in type.AllInterfaces)
            {
                foreach (ISymbol member in implemented.GetMembers())
                {
                    if (member.IsStatic || member is IMethodSymbol { MethodKind: not MethodKind.Ordinary } || member is INamedTypeSymbol)
                        continue;
                    ISymbol definition = member.OriginalDefinition;
                    if (!IsSource(definition) || type.FindImplementationForInterfaceMember(member) is not { } implementation)
                        continue;
                    if (Normalize(implementation) is { } target)
                        AddDispatch(definition, target);
                }
            }
            foreach (ISymbol member in type.GetMembers())
            {
                if (!member.IsOverride || member is IMethodSymbol { MethodKind: not MethodKind.Ordinary } || Normalize(member) is not { } target)
                    continue;
                for (ISymbol? overridden = Overridden(member); overridden is not null; overridden = Overridden(overridden))
                {
                    if (IsSource(overridden.OriginalDefinition))
                        AddDispatch(overridden.OriginalDefinition, target);
                }
            }
        }
    }

    /// <summary>The member <paramref name="member"/> directly overrides.</summary>
    private static ISymbol? Overridden(ISymbol member) => member switch
    {
        IMethodSymbol method => method.OverriddenMethod,
        IPropertySymbol property => property.OverriddenProperty,
        IEventSymbol @event => @event.OverriddenEvent,
        _ => null,
    };

    /// <summary>The unit symbol of a member, or null when it is not a separate piece of Core code.</summary>
    private ISymbol? Normalize(ISymbol member) => member switch
    {
        IMethodSymbol method => NormalizeMethod(method),
        IPropertySymbol property => NormalizeProperty(property),
        IEventSymbol @event when IsSource(@event) => @event.OriginalDefinition,
        _ => null,
    };

    /// <summary>Records that a reference to <paramref name="member"/> may run <paramref name="target"/>.</summary>
    private void AddDispatch(ISymbol member, ISymbol target)
    {
        if (!_dispatch.TryGetValue(member, out List<ISymbol>? targets))
            _dispatch.Add(member, targets = []);
        if (!targets.Contains(target, SymbolEqualityComparer.Default))
            targets.Add(target);
    }
}
