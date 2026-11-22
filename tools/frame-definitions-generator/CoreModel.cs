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
/// <param name="PartEntries">
/// The value the parts table's entry holds for each definition (a part, or the factory building the definition's entity),
/// which a lookup of the definition's entry runs alone.
/// </param>
internal sealed record Behaviour(
    INamedTypeSymbol Type, string Family, string Path, IReadOnlyList<string> Definitions, ISymbol? PartsTable,
    IReadOnlyDictionary<string, IFieldSymbol> Parts, IReadOnlyDictionary<string, ExpressionSyntax> PartEntries);

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
        // The effect actors' animations name their enemy-sheet frames outright (#1817) and start one per kind (#2158); an
        // effect actor's frames reach its spawner's sheet through its kind, as the spawn analysis reads it.
        "RoomEffectActorAnimations",
        // An effect actor's runs are named by the actor itself (#2369): the segment names every kind and the two cycles.
        "RoomEffectActorSegment",
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

    /// <summary>For each type, the static methods of its own that construct it: the factories its callers build it through.</summary>
    private readonly Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> _selfFactories = new(SymbolEqualityComparer.Default);
    /// <summary>
    /// For each type the species composition constructs, the types whose construction there it is handed to, each
    /// construction's own receivers in turn; null once one construction of it is handed anywhere else (a member of the
    /// composition, a call, a conditional), which leaves it the composition's shared code.
    /// </summary>
    private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>?> _compositionHandoffs = new(SymbolEqualityComparer.Default);

    /// <summary>The attribute marking the one composition that builds the room's species and hands each its collaborators.</summary>
    private readonly INamedTypeSymbol _speciesComposition;
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
        RoomEffectActorKind = SingleType("RoomEffectActorKind");
        RoomEnemyProjectileKind = SingleType("RoomEnemyProjectileKind");
        EnemyProjectileMotionBehavior = SingleType("EnemyProjectileMotionBehavior", arity: 1);
        EnemyProjectile = SingleType("EnemyProjectile");
        EnemyDefinitionId = SingleType("EnemyDefinitionId");
        ArchitectureReview = SingleType("ArchitectureReviewAttribute");
        EnemySheetDraw = SingleType("EnemySheetDraw");
        SpriteGraphics = SingleType("SpriteGraphics");
        WeightedRandomChoice = SingleType("WeightedRandomChoice", arity: 1);
        SpawnedObjectOwner = SingleType("SpawnedObjectOwner");
        RoomEnemyPlace = SingleType("RoomEnemyPlace");
        RoomEnemyOccupant = SingleType("RoomEnemyOccupant");
        EnemyOccupant = SingleType("EnemyOccupant");
        AnimationSegmentPlay = SingleType("AnimationSegmentPlay", arity: 5);
        IdentityTable = SingleType("IdentityTable", arity: 2);
        _entity = SingleType("IEnemyEntity");
        INamedTypeSymbol factory = SingleType("IEnemyEntityFactory");
        INamedTypeSymbol timelineAnimation = SingleType("EnemyProjectileTimelineAnimation", arity: 3);
        TimelineAnimation = timelineAnimation;
        _projectileBehaviour = SingleType("EnemyProjectileBehavior");
        _speciesComposition = SingleType("SpeciesCompositionAttribute");
        foreach (string name in TableNames)
            _tables.Add(SingleType(name));
        foreach (string name in MemberSpawnerTypeNames)
            _memberSpawnerTypes.Add(SingleType(name));

        foreach (INamedTypeSymbol type in Types)
        {
            if (type.TypeKind == TypeKind.Class && !type.IsAbstract && DerivesFrom(type, timelineAnimation))
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
        foreach (SyntaxTree tree in AuthoredTrees)
            ReadTree(tree);
        BuildDispatch();
    }

    /// <summary>The Core compilation.</summary>
    public Compilation Compilation { get; }

    /// <summary>
    /// The compilation's trees the game's authors wrote: not the members the compiler writes for debugger states (#2602),
    /// whose restore constructors and registrations make no frame.
    /// </summary>
    public IEnumerable<SyntaxTree> AuthoredTrees => Compilation.SyntaxTrees.Where(tree => !tree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase));

    /// <summary>The Core project directory.</summary>
    public string Directory { get; }

    /// <summary>Every source type in Core, nested types included, in source order.</summary>
    public IReadOnlyList<INamedTypeSymbol> Types { get; }

    /// <summary>The enemy frame identity.</summary>
    public INamedTypeSymbol EnemyFrameId { get; }

    /// <summary>The enemy projectile frame identity.</summary>
    public INamedTypeSymbol EnemyProjectileFrameId { get; }

    /// <summary>The room effect-actor kind identity.</summary>
    public INamedTypeSymbol RoomEffectActorKind { get; }

    /// <summary>The enemy projectile kind identity.</summary>
    public INamedTypeSymbol RoomEnemyProjectileKind { get; }

    /// <summary>The enemy definition identity.</summary>
    public INamedTypeSymbol EnemyDefinitionId { get; }

    /// <summary>
    /// The base of a projectile behaviour that moves its projectiles (#1843), <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c>,
    /// whose type argument names the family's motions.
    /// </summary>
    public INamedTypeSymbol EnemyProjectileMotionBehavior { get; }

    /// <summary>An enemy projectile: the object a projectile behaviour is bound to, which holds its motion.</summary>
    public INamedTypeSymbol EnemyProjectile { get; }

    /// <summary>The explicit draw of a named definition's sheet frame.</summary>
    public INamedTypeSymbol EnemySheetDraw { get; }

    /// <summary>The graphics a spawned object draws with: whose sheet, in which shading.</summary>
    public INamedTypeSymbol SpriteGraphics { get; }

    /// <summary>A random choice among weighted outcomes (#2510), whose <c>Choose</c> gives one of the outcomes its construction lists.</summary>
    public INamedTypeSymbol WeightedRandomChoice { get; }

    /// <summary>Whose spawned-object sheet a spawned object draws from.</summary>
    public INamedTypeSymbol SpawnedObjectOwner { get; }

    /// <summary>An enemy place (#1961).</summary>
    public INamedTypeSymbol RoomEnemyPlace { get; }

    /// <summary>The enemy holding a place, which carries its definition (#2558).</summary>
    public INamedTypeSymbol RoomEnemyOccupant { get; }

    /// <summary>What an enemy's creation builds for its place: its entity and the setup and main AI it runs.</summary>
    public INamedTypeSymbol EnemyOccupant { get; }

    /// <summary>
    /// Whether <paramref name="unit"/> is <paramref name="behaviour"/>'s factory code: its entity factory
    /// (<see cref="UnitScanner.IsCreate"/>), or one of the creations that factory hands the registrar or offers a part, each a
    /// named method building one enemy from its occupant and place (#2554: <c>Spawns([EtecoonEscape], CreateEtecoon)</c>,
    /// <c>Offer(BabyTurtle, new FamilyJoining(...).CreateBaby)</c>), declared by the behaviour or a type nested in it. A
    /// creation is dispatch code: it runs for the definitions the registrar call or the offer handing it names, and the
    /// entity it constructs is theirs.
    /// </summary>
    public bool IsFactoryCode(Unit unit, Behaviour behaviour) => UnitScanner.IsCreate(unit, behaviour) || IsCreation(unit, behaviour);

    /// <summary>Whether <paramref name="unit"/> is a creation of <paramref name="behaviour"/> (see <see cref="IsFactoryCode"/>).</summary>
    private bool IsCreation(Unit unit, Behaviour behaviour)
    {
        if (unit is not { Kind: UnitKind.Member, Symbol: IMethodSymbol { MethodKind: MethodKind.Ordinary } method }
            || !SymbolEqualityComparer.Default.Equals(method.ReturnType.OriginalDefinition, EnemyOccupant)
            || !method.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, RoomEnemyOccupant))
            || !method.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, RoomEnemyPlace)))
            return false;
        for (INamedTypeSymbol? type = method.ContainingType; type is not null; type = type.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, behaviour.Type))
                return true;
        }
        return false;
    }

    /// <summary>
    /// The play of the segments a family's typed animations declare, each segment's beats showing frames and raising cues: the
    /// base of a pausing family's and an unpausing family's segments alike (#2553).
    /// </summary>
    public INamedTypeSymbol AnimationSegmentPlay { get; }

    /// <summary>The table holding a value for every member of an identity, which Core dispatches through (#2553).</summary>
    public INamedTypeSymbol IdentityTable { get; }

    /// <summary>Whether <paramref name="type"/> is a family's segments: a pausing or an unpausing family's (#2553).</summary>
    public bool IsAnimationSegmentsType(INamedTypeSymbol type) => DerivesFrom(type, AnimationSegmentPlay);

    /// <summary>
    /// The base every projectile program family derives from (#1600), whose type arguments are its segment, frame and event
    /// types; the segment type names the programs (the segments an animation starts at). A family whose timeline raises
    /// events and one whose timeline raises none (#2571) both derive from it.
    /// </summary>
    public INamedTypeSymbol TimelineAnimation { get; }

    /// <summary>What each projectile program family's animations show, read from their segments and animations once.</summary>
    public ProjectileProgramFrames ProgramFrames => _programFrames ??= new ProjectileProgramFrames(this);

    private ProjectileProgramFrames? _programFrames;

    /// <summary>
    /// The static segments each family's animation data declares (#2275). The data's own starting animations read them only
    /// to count a segment's beats, which runs none of their cues: the family's programs run those, as its code plays them.
    /// </summary>
    public IReadOnlySet<ISymbol> FamilySegments => _familySegments ??= new HashSet<ISymbol>(
        Families.Select(ProgramFrames.AnimationSegments).OfType<ISymbol>(), SymbolEqualityComparer.Default);

    private HashSet<ISymbol>? _familySegments;

    /// <summary>
    /// Whether <paramref name="unit"/>'s reference to <paramref name="target"/> only counts the beats of its own type's
    /// segments, which runs none of their cues and shows none of their frames: a projectile family's animation data reading
    /// its segments (#2275), or an animation table of any animation data reading its own segments to play them whole or
    /// repeated (#2399: Crocomire's tongue animations, which the body's state names for the tongue's first playback, show
    /// nothing of the body's). The code that plays the segments reaches them.
    /// </summary>
    public bool CountsOwnSegments(Unit unit, Unit target) =>
        target.Kind == UnitKind.Member && SymbolEqualityComparer.Default.Equals(unit.Type, target.Type)
        && (FamilySegments.Contains(target.Symbol)
            || ((unit.Kind == UnitKind.StaticInitialization || unit.Symbol is IPropertySymbol or IFieldSymbol) && IsAnimationSegments(target.Symbol)));

    /// <summary>Whether <paramref name="member"/> is a property or field holding a family's segments: an <c>AnimationSegments</c>.</summary>
    private bool IsAnimationSegments(ISymbol member) =>
        (member switch { IPropertySymbol property => property.Type, IFieldSymbol field => field.Type, _ => null })
            is INamedTypeSymbol type && IsAnimationSegmentsType(type);

    /// <summary>The programs motion handlers restart only the objects of some kinds in, read once (#1854, #1843).</summary>
    public KindRestarts KindRestarts => _kindRestarts ??= new KindRestarts(this);

    /// <summary>Core's dispatch through identity tables, each entry running only for its key (#2553).</summary>
    public IdentityTableDispatch Dispatch => _tableDispatch ??= new IdentityTableDispatch(this);

    private IdentityTableDispatch? _tableDispatch;

    private KindRestarts? _kindRestarts;

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
    /// the order spread, as <c>TorizoAnimations.Segments</c> joins three tables of segments.
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
    /// members of the family's program type its program argument names (<see cref="ProgramArgumentOf"/>), each branch of a
    /// conditional or switch among them; null when any branch is no such member.
    /// </summary>
    public IReadOnlySet<string>? ProgramsOf(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, INamedTypeSymbol family)
    {
        INamedTypeSymbol? programType = ProgramTypeOf(family);
        if (ProgramArgumentOf(semantic, creation, family) is not { } program || programType is null)
            return null;
        var programs = new HashSet<string>(StringComparer.Ordinal);
        return AddPrograms(semantic, program, programType, programs) ? programs : null;
    }

    /// <summary>
    /// The parameter of <paramref name="constructor"/>, one of <paramref name="family"/>'s, that hands it the program it
    /// starts in: the one taking the family's program type or an animation of it. The owner its cues go to may come before
    /// it (#2571). Null when the constructor takes no program.
    /// </summary>
    /// <exception cref="InvalidDataException">The constructor takes more than one program.</exception>
    public IParameterSymbol? ProgramParameterOf(IMethodSymbol constructor, INamedTypeSymbol family)
    {
        if (ProgramTypeOf(family) is not { } programType)
            return null;
        IParameterSymbol[] programs = [.. constructor.Parameters.Where(parameter => IsProgramValue(parameter.Type, programType))];
        return programs switch
        {
            [] => null,
            [var program] => program.OriginalDefinition,
            _ => throw new InvalidDataException($"{constructor.ToDisplayString()} takes more than one program."),
        };
    }

    /// <summary>The argument <paramref name="creation"/>, a construction of <paramref name="family"/>, passes for its program; null when none.</summary>
    public ExpressionSyntax? ProgramArgumentOf(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, INamedTypeSymbol family) =>
        creation.ArgumentList is { } arguments && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol constructor
            && ProgramParameterOf(constructor, family) is { } program
            ? CallSite.ArgumentFor(arguments, program)
            : null;

    /// <summary>Whether <paramref name="type"/> names a program of <paramref name="programType"/>: the program type, or an animation of it.</summary>
    private static bool IsProgramValue(ITypeSymbol type, INamedTypeSymbol programType) =>
        SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, programType.OriginalDefinition)
        || type is INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }
            && SymbolEqualityComparer.Default.Equals(played.OriginalDefinition, programType.OriginalDefinition);

    /// <summary>
    /// The type naming <paramref name="family"/>'s programs (the segments an animation starts at): the first type argument
    /// of the typed animation it derives from; null when it derives from none.
    /// </summary>
    public INamedTypeSymbol? ProgramTypeOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, TimelineAnimation))
                return type.TypeArguments[0] as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// The type of the cues <paramref name="family"/>'s segments publish: the third type argument of the typed animation it
    /// derives from; null when it derives from none.
    /// </summary>
    public INamedTypeSymbol? CueTypeOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, TimelineAnimation))
                return (type.TypeArguments[2] as INamedTypeSymbol)?.OriginalDefinition;
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
        foreach (SyntaxTree tree in AuthoredTrees)
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
        if (node is ExpressionSyntax expression && ConstructedMember(semantic, expression) is { } constructed)
            return Unit.Member(constructed);
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
        foreach (IMethodSymbol constructor in GameConstructors.Of(family.OriginalDefinition))
        {
            if (ProgramParameterOf(constructor, family) is { } program)
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
                    ISymbol? symbol = site.Semantic.GetSymbolInfo(choice).Symbol;
                    // A local a pattern declares hands on what the pattern tests (#2571): shot is { } segment hands on shot.
                    if (symbol is ILocalSymbol local && ProgramTracer.PatternTested(local) is { } tested)
                        symbol = site.Semantic.GetSymbolInfo(tested).Symbol;
                    if (HandedParameter(symbol) is { } source && parameters.Add(source))
                        pending.Enqueue(source);
                }
            }
        }
    }

    /// <summary>
    /// The parameter <paramref name="symbol"/> reads a value handed in through: a parameter of a method (not of a lambda or
    /// local function, and not the instance a member runs on), or the primary-constructor parameter a positional record's
    /// property is; null for anything else.
    /// </summary>
    public IParameterSymbol? HandedParameter(ISymbol? symbol) => symbol switch
    {
        IParameterSymbol { IsThis: false, ContainingSymbol: IMethodSymbol { MethodKind: not (MethodKind.AnonymousFunction or MethodKind.LocalFunction) } } parameter =>
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

    /// <summary>
    /// The values <paramref name="expression"/> may be, through parentheses, conditionals and switch expressions; a branch
    /// that throws gives none.
    /// </summary>
    public static IEnumerable<ExpressionSyntax> Choices(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case ThrowExpressionSyntax:
                return [];
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

    /// <summary>
    /// The member of its own type a constructor assigns <paramref name="creation"/>, a projectile behaviour's construction,
    /// to straight away (<c>Spark = new(…)</c>, an instance field or data property of the constructor's type), or null
    /// (#2554). Such a construction is the member's code, as a getter building the behaviour on first read was: the code
    /// reading the member spawns with it and reaches it, and the constructor, which code outside the member's readers runs
    /// (a combat component the shared enemy composition builds), does not.
    /// </summary>
    public ISymbol? ConstructedMember(SemanticModel semantic, ExpressionSyntax creation)
    {
        if (creation is not BaseObjectCreationExpressionSyntax
            || creation.Parent is not AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment
            || assignment.Right != creation || assignment.Parent is not ExpressionStatementSyntax
            || creation.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() is not { } constructorDeclaration
            || creation.Ancestors().TakeWhile(ancestor => ancestor != constructorDeclaration).Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)
            || semantic.GetDeclaredSymbol(constructorDeclaration) is not { IsStatic: false } constructor
            || semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { ContainingType: var created } || !IsProjectileBehaviour(created))
        {
            return null;
        }
        ISymbol? member = semantic.GetSymbolInfo(assignment.Left).Symbol switch
        {
            IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field when IsSource(field) => field.OriginalDefinition,
            IPropertySymbol { IsStatic: false } property when NormalizeProperty(property) is { } normalized && IsDataProperty(normalized) => normalized,
            _ => null,
        };
        return member is not null && SymbolEqualityComparer.Default.Equals(member.ContainingType.OriginalDefinition, constructor.ContainingType.OriginalDefinition)
            ? member
            : null;
    }

    /// <summary>The constructions <paramref name="member"/>'s type's constructors assign straight to it (<see cref="ConstructedMember"/>).</summary>
    public IEnumerable<ExpressionSyntax> ConstructionsAssignedTo(ISymbol member)
    {
        foreach (IMethodSymbol constructor in GameConstructors.Of(member.ContainingType))
        {
            foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences.Where(reference => !GameConstructors.IsCompilerWritten(reference)))
            {
                if (reference.GetSyntax() is not ConstructorDeclarationSyntax declaration)
                    continue;
                SemanticModel semantic = ModelOf(declaration.SyntaxTree);
                foreach (BaseObjectCreationExpressionSyntax creation in declaration.DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
                {
                    if (SymbolEqualityComparer.Default.Equals(ConstructedMember(semantic, creation), member))
                        yield return creation;
                }
            }
        }
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

    /// <summary>Registrar calls that name the definitions a species creates (<c>EnemySpeciesRegistrar</c>).</summary>
    private static readonly string[] RegistrarCreations = ["Spawns", "Loads", "Joins"];

    /// <summary>
    /// The collection expressions naming the definitions <paramref name="type"/> creates: its Definitions property's
    /// initializer, or the definition lists its OfferCreations hands the registrar, each written in place or held by a
    /// property or field initialized with one.
    /// </summary>
    /// <exception cref="InvalidDataException">The type names its definitions neither way, or in a form this generator cannot read.</exception>
    private List<CollectionExpressionSyntax> DefinitionCollections(INamedTypeSymbol type)
    {
        var declared = type.GetMembers("Definitions").OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToList();
        var collections = new List<CollectionExpressionSyntax>();
        // A species that declares its own definitions may still hand the registrar the parts that join them (Joins).
        if (declared is [{ DeclaringSyntaxReferences: [var reference] }])
        {
            collections.Add(reference.GetSyntax() is PropertyDeclarationSyntax { Initializer.Value: CollectionExpressionSyntax collection }
                ? collection
                : throw new InvalidDataException($"{type.ToDisplayString()}.Definitions is not initialized with a collection expression."));
        }
        foreach (IMethodSymbol offer in type.GetMembers("OfferCreations").OfType<IMethodSymbol>())
        {
            foreach (SyntaxReference syntax in offer.DeclaringSyntaxReferences)
            {
                SemanticModel model = ModelOf(syntax.SyntaxTree);
                foreach (InvocationExpressionSyntax call in syntax.GetSyntax().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string? called = call.Expression switch
                    {
                        MemberAccessExpressionSyntax { Name.Identifier.Text: var name } => name,
                        _ => null,
                    };
                    if (called is null || !RegistrarCreations.Contains(called) || call.ArgumentList.Arguments.Count == 0)
                        continue;
                    // The species' own Definitions, handed on, are already read.
                    if (declared is [var own] && SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(call.ArgumentList.Arguments[0].Expression).Symbol, own))
                        continue;
                    collections.Add(DefinitionList(model, call.ArgumentList.Arguments[0].Expression, type));
                }
            }
        }
        return collections.Count != 0
            ? collections
            : throw new InvalidDataException($"{type.ToDisplayString()} creates enemies but names its definitions neither in a Definitions property nor in its OfferCreations.");
    }

    /// <summary>The collection expression <paramref name="argument"/> is, or the one the property or field it names is initialized with.</summary>
    /// <exception cref="InvalidDataException">The argument is neither.</exception>
    private static CollectionExpressionSyntax DefinitionList(SemanticModel model, ExpressionSyntax argument, INamedTypeSymbol type)
    {
        if (argument is CollectionExpressionSyntax collection)
            return collection;
        SyntaxNode? declaration = model.GetSymbolInfo(argument).Symbol?.DeclaringSyntaxReferences is [var held] ? held.GetSyntax() : null;
        return declaration switch
        {
            PropertyDeclarationSyntax { Initializer.Value: CollectionExpressionSyntax initializer } => initializer,
            VariableDeclaratorSyntax { Initializer.Value: CollectionExpressionSyntax initializer } => initializer,
            _ => throw new InvalidDataException($"{type.ToDisplayString()}.OfferCreations hands the registrar definitions this generator cannot read: {argument}"),
        };
    }

    /// <summary>Reads the definitions <paramref name="type"/> declares, directly or through a parts table's keys.</summary>
    private Behaviour ReadBehaviour(INamedTypeSymbol type)
    {
        List<CollectionExpressionSyntax> collections = DefinitionCollections(type);
        var names = new List<string>();
        var parts = new Dictionary<string, IFieldSymbol>(StringComparer.Ordinal);
        var entries = new Dictionary<string, ExpressionSyntax>(StringComparer.Ordinal);
        ISymbol? partsTable = null;
        foreach (CollectionElementSyntax element in collections.SelectMany(collection => collection.Elements))
        {
            SemanticModel model = ModelOf(element.SyntaxTree);
            switch (element)
            {
                case ExpressionElementSyntax { Expression: var expression }:
                    names.Add(DefinitionName(model, expression, type));
                    break;
                case SpreadElementSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Keys", Expression: var table } }:
                    ISymbol? tableSymbol = model.GetSymbolInfo(table).Symbol;
                    if (partsTable is not null)
                        throw new InvalidDataException($"{type.ToDisplayString()}.Creations spreads more than one parts table.");
                    partsTable = tableSymbol?.OriginalDefinition;
                    foreach ((string key, IFieldSymbol? part, ExpressionSyntax value) in PartsTableKeys(tableSymbol, type))
                    {
                        names.Add(key);
                        entries.Add(key, value);
                        if (part is not null)
                            parts.Add(key, part);
                    }
                    break;
                default:
                    throw new InvalidDataException($"{type.ToDisplayString()}.Creations has an element this generator cannot read: {element}");
            }
        }
        if (names.Count == 0)
            throw new InvalidDataException($"{type.ToDisplayString()} declares no definitions.");
        string family = Regex.Replace(type.Name, "(Behavior|Factory)$", "");
        return new Behaviour(type, family, RelativePath(collections[0].SyntaxTree), names, partsTable, parts, entries);
    }

    /// <summary>The EnemyDefinitionId member <paramref name="expression"/> names.</summary>
    private string DefinitionName(SemanticModel model, ExpressionSyntax expression, INamedTypeSymbol owner) =>
        model.GetSymbolInfo(expression).Symbol is IFieldSymbol field
        && SymbolEqualityComparer.Default.Equals(field.ContainingType, EnemyDefinitionId)
            ? field.Name
            : throw new InvalidDataException($"{owner.ToDisplayString()}: {expression} is not an EnemyDefinitionId member.");

    /// <summary>
    /// The definitions keying a parts table, in its initializer's order, each with its entry's value and the enum member
    /// that value names, when it names one.
    /// </summary>
    private List<(string Key, IFieldSymbol? Part, ExpressionSyntax Value)> PartsTableKeys(ISymbol? table, INamedTypeSymbol owner)
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
        var keys = new List<(string, IFieldSymbol?, ExpressionSyntax)>();
        foreach (ExpressionSyntax entry in initializer.Expressions)
        {
            if (entry is not AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] }, Right: var part })
                throw new InvalidDataException($"{owner.ToDisplayString()}: the parts table entry {entry} is not an [EnemyDefinitionId.X] = ... entry.");
            IFieldSymbol? partMember = model.GetSymbolInfo(part).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member
                ? member.OriginalDefinition
                : null;
            keys.Add((DefinitionName(model, key.Expression, owner), partMember, part));
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
                    if (SymbolEqualityComparer.Default.Equals(contextType.OriginalDefinition, key2)
                        && node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault() is { } factory
                        && model.GetDeclaredSymbol(factory) is { IsStatic: true } factoryMethod)
                    {
                        if (!_selfFactories.TryGetValue(key2, out HashSet<IMethodSymbol>? factories))
                            _selfFactories.Add(key2, factories = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
                        factories.Add(factoryMethod.OriginalDefinition);
                    }
                    if (IsSpeciesComposition(contextType))
                        RecordCompositionHandoff(model, creation, key2);
                    // A family constructed in code, not by a typed-start entry its kind names, must be spawned by some code.
                    if (_families.Contains(key2) && !node.Ancestors().Any(_startEntries.Contains))
                        _constructedFamilies.Add(key2);
                    break;
            }
        }
    }

    /// <summary>Whether <paramref name="type"/> is the species composition (<c>[SpeciesComposition]</c>).</summary>
    private bool IsSpeciesComposition(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _speciesComposition));

    /// <summary>
    /// Records whom the species composition hands its construction of <paramref name="created"/> to (#2554): a
    /// construction held in a local is handed to every construction the local is an argument or initializer value of; any
    /// other construction to the construction it is itself an argument or initializer value of. A construction handed
    /// anywhere else leaves <paramref name="created"/> the composition's shared code.
    /// </summary>
    private void RecordCompositionHandoff(SemanticModel model, BaseObjectCreationExpressionSyntax creation, INamedTypeSymbol created)
    {
        List<INamedTypeSymbol>? receivers = CompositionReceivers(model, creation);
        if (!_compositionHandoffs.TryGetValue(created, out List<INamedTypeSymbol>? known))
            _compositionHandoffs.Add(created, receivers);
        else if (known is not null)
        {
            if (receivers is null)
                _compositionHandoffs[created] = null;
            else
                known.AddRange(receivers);
        }
    }

    /// <summary>The constructions the composition hands <paramref name="creation"/> to, or null when it hands it elsewhere too.</summary>
    private List<INamedTypeSymbol>? CompositionReceivers(SemanticModel model, BaseObjectCreationExpressionSyntax creation)
    {
        if (creation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
            && model.GetDeclaredSymbol(declarator) is ILocalSymbol local)
        {
            BlockSyntax scope = declarator.FirstAncestorOrSelf<BlockSyntax>()
                ?? throw new InvalidDataException($"{RelativePath(creation.SyntaxTree)}: local {local.Name} is declared outside a block.");
            var receivers = new List<INamedTypeSymbol>();
            foreach (IdentifierNameSyntax use in scope.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (use.Identifier.ValueText != local.Name || !SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(use).Symbol, local))
                    continue;
                if (ReceivingConstruction(model, use) is not { } receiver)
                    return null;
                receivers.Add(receiver);
            }
            return receivers.Count == 0 ? null : receivers;
        }
        return ReceivingConstruction(model, creation) is { } direct ? [direct] : null;
    }

    /// <summary>
    /// The type whose construction takes <paramref name="value"/> as an argument or as an initializer's value; null for a
    /// value used any other way.
    /// </summary>
    private INamedTypeSymbol? ReceivingConstruction(SemanticModel model, ExpressionSyntax value)
    {
        BaseObjectCreationExpressionSyntax? receiver = value.Parent switch
        {
            ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax construction } } => construction,
            AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax { Parent: BaseObjectCreationExpressionSyntax construction } } assignment
                when assignment.Right == value => construction,
            _ => null,
        };
        return receiver is not null && model.GetSymbolInfo(receiver).Symbol is IMethodSymbol { ContainingType: { } received } && IsSource(received)
            ? received.OriginalDefinition
            : null;
    }

    /// <summary>
    /// The one behaviour whose code <paramref name="type"/> is because the species composition, its only constructor,
    /// hands every construction of it only to that behaviour's code (#2554: Phantoon's combat and Kraid's encounter access,
    /// projectiles and combat, which the composition builds and passes in as it builds Crocomire's access); null otherwise.
    /// It is that behaviour's code as it was while the behaviour constructed it itself.
    /// </summary>
    private INamedTypeSymbol? CompositionHandedOwner(INamedTypeSymbol type)
    {
        if (!_creationContexts.TryGetValue(type, out List<INamedTypeSymbol>? sites) || sites.Count == 0
            || !sites.All(IsSpeciesComposition)
            || !_compositionHandoffs.TryGetValue(type, out List<INamedTypeSymbol>? receivers) || receivers is not { Count: > 0 })
            return null;
        _owners.Add(type, Resolving);
        var owners = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        bool resolved = true;
        foreach (INamedTypeSymbol receiver in receivers)
        {
            if (ResolveOwners(receiver) is not { Count: > 0 } receiverOwners || ReferenceEquals(receiverOwners, Resolving))
            {
                resolved = false;
                break;
            }
            owners.UnionWith(receiverOwners);
        }
        _owners.Remove(type);
        return resolved && owners.Count == 1 ? owners.First() : null;
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
        if (SpeciesOwners(type) is { } placing)
        {
            _owners.Add(type, placing);
            return placing;
        }
        if (CompositionHandedOwner(type) is { } handedOwner)
        {
            var handed = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default) { handedOwner };
            _owners.Add(type, handed);
            return handed;
        }
        var contexts = new List<INamedTypeSymbol>();
        if (_creationContexts.TryGetValue(type, out List<INamedTypeSymbol>? sites))
            contexts.AddRange(sites);
        if (type.ContainingType is { } container)
            contexts.Add(container.OriginalDefinition);
        // A type its own static factory builds (#2554: an entity's SetUp running its species' setup and constructing it) is
        // built where that factory is called: its callers' code is the code constructing it, as the behaviour constructing
        // the entity directly was. A call outside every member is shared code.
        bool calledOutsideMembers = false;
        foreach (IMethodSymbol factory in _selfFactories.GetValueOrDefault(type) ?? [])
        {
            foreach (CallSite site in CallSites(factory))
            {
                if (site.Caller is { } caller)
                    contexts.Add(caller.Type.OriginalDefinition);
                else
                    calledOutsideMembers = true;
            }
        }
        _owners.Add(type, Resolving);
        // A base never constructed itself, outside every type, whose derived types are all one behaviour's code is that
        // behaviour's code split across types (#2146: CrocomireEntity under the body's and the tongue's entities), as a
        // nested type is; a base deriving into several behaviours' code, or into shared code, stays shared.
        if (contexts.Count == 0 && SingleDerivedOwner(type) is { } derivedOwner)
        {
            var single = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default) { derivedOwner };
            _owners[type] = single;
            return single;
        }
        // An entity base of one species family, whose derived entities, all in its own namespace, are all behaviours' code,
        // is theirs (#2554: RidleyEntity under Norfair Ridley's entity and Ceres Ridley's, each placed by a factory of its
        // own): dispatch into it is the engine running whichever enemy holds the place, never another behaviour's code. A
        // base every family's entities derive from, as the common participation, stays shared.
        if (contexts.Count == 0 && IsEntityType(type) && DerivedTypes.TryGetValue(type, out List<INamedTypeSymbol>? family)
            && family.All(derivedType => SymbolEqualityComparer.Default.Equals(derivedType.ContainingNamespace, type.ContainingNamespace))
            && DerivedOwners(type) is { } derivedOwners)
        {
            _owners[type] = derivedOwners;
            return derivedOwners;
        }
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
        IReadOnlySet<INamedTypeSymbol>? result = owners is not null && contributed && !calledOutsideMembers ? owners : null;
        _owners[type] = result;
        return result;
    }

    /// <summary>
    /// The one behaviour whose code every type deriving directly from <paramref name="type"/> is; null when no type derives
    /// from it, or one is shared code, still being resolved, or another behaviour's.
    /// </summary>
    private INamedTypeSymbol? SingleDerivedOwner(INamedTypeSymbol type)
    {
        if (!DerivedTypes.TryGetValue(type, out List<INamedTypeSymbol>? derivedTypes))
            return null;
        INamedTypeSymbol? owner = null;
        foreach (INamedTypeSymbol derivedType in derivedTypes)
        {
            // The marker of a type still being resolved is empty, so it, like shared code, gives no single owner.
            if (ResolveOwners(derivedType) is not { Count: 1 } owners)
                return null;
            INamedTypeSymbol single = owners.First();
            if (owner is not null && !SymbolEqualityComparer.Default.Equals(owner, single))
                return null;
            owner = single;
        }
        return owner;
    }

    /// <summary>Every Core type deriving directly from each Core class, built on first use.</summary>
    private Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> DerivedTypes
    {
        get
        {
            if (_derived is null)
            {
                _derived = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
                foreach (INamedTypeSymbol candidate in Types)
                {
                    if (candidate.BaseType is { } baseType && IsSource(baseType.OriginalDefinition))
                    {
                        if (!_derived.TryGetValue(baseType.OriginalDefinition, out List<INamedTypeSymbol>? derived))
                            _derived.Add(baseType.OriginalDefinition, derived = []);
                        derived.Add(candidate.OriginalDefinition);
                    }
                }
            }
            return _derived;
        }
    }

    /// <summary>Every Core type deriving directly from each Core class (<see cref="DerivedTypes"/>), once built.</summary>
    private Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>? _derived;

    /// <summary>
    /// The behaviours whose code the types deriving directly from <paramref name="type"/> are, together; null when no type
    /// derives from it, or one is shared code or still being resolved.
    /// </summary>
    private HashSet<INamedTypeSymbol>? DerivedOwners(INamedTypeSymbol type)
    {
        if (!DerivedTypes.TryGetValue(type, out List<INamedTypeSymbol>? derivedTypes))
            return null;
        var owners = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol derivedType in derivedTypes)
        {
            // The marker of a type still being resolved is empty, so it, like shared code, gives no owners.
            if (ResolveOwners(derivedType) is not { Count: > 0 } derivedOwners)
                return null;
            owners.UnionWith(derivedOwners);
        }
        return owners;
    }

    /// <summary>
    /// The behaviours whose code a species is when factories of its own place its enemies (#2554): a type behaviours are
    /// handed at their construction, which the enemy entities are handed too, as their species, as Mother Brain's species is
    /// handed to her body's factory and to her parts' entities. It is those behaviours' code, as it was while the species
    /// placed its enemies itself, never shared code every walk's dispatch reaches; null for any other type.
    /// </summary>
    private IReadOnlySet<INamedTypeSymbol>? SpeciesOwners(INamedTypeSymbol type)
    {
        if (_speciesOwners is null)
        {
            var handedTo = new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
            foreach (INamedTypeSymbol behaviour in _behaviours.Keys)
            {
                foreach (IParameterSymbol parameter in behaviour.InstanceConstructors.SelectMany(constructor => constructor.Parameters))
                {
                    if (parameter.Type is not INamedTypeSymbol handed || !IsSource(handed.OriginalDefinition))
                        continue;
                    if (!handedTo.TryGetValue(handed.OriginalDefinition, out HashSet<INamedTypeSymbol>? behaviours))
                        handedTo.Add(handed.OriginalDefinition, behaviours = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
                    behaviours.Add(behaviour);
                }
            }
            // An entity is handed its species first, before its occupant, place and anything else it is built with.
            var entitySpecies = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (INamedTypeSymbol candidate in Types)
            {
                if (!candidate.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, _entity)))
                    continue;
                foreach (IMethodSymbol constructor in candidate.InstanceConstructors)
                {
                    if (constructor.Parameters is [{ Type: INamedTypeSymbol species }, ..])
                        entitySpecies.Add(species.OriginalDefinition);
                }
            }
            _speciesOwners = new Dictionary<INamedTypeSymbol, IReadOnlySet<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
            foreach ((INamedTypeSymbol species, HashSet<INamedTypeSymbol> behaviours) in handedTo)
            {
                if (entitySpecies.Contains(species) && !_behaviours.ContainsKey(species))
                    _speciesOwners.Add(species, behaviours);
            }
        }
        return _speciesOwners.TryGetValue(type, out IReadOnlySet<INamedTypeSymbol>? owners) ? owners : null;
    }

    /// <summary>Each species factories of its own place, with those factories (<see cref="SpeciesOwners"/>), built on first use.</summary>
    private Dictionary<INamedTypeSymbol, IReadOnlySet<INamedTypeSymbol>>? _speciesOwners;

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
