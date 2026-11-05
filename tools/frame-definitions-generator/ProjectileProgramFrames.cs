using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The projectile frames a program family shows from the programs it is started in (#1513), split by the graphics they
/// draw under.
/// </summary>
/// <param name="Spawned">The frames shown under the graphics the object was spawned with.</param>
/// <param name="Room">
/// The frames shown once one of the programs' cues has switched the object to the room's graphics, which draw from the
/// shared sheet whatever graphics the object was spawned with.
/// </param>
internal sealed record ProgramFrameSplit(IReadOnlySet<string> Spawned, IReadOnlySet<string> Room);

/// <summary>
/// Reads, from SuperMetroid.Core's source, what each projectile program family's programs show (#1513): the family's frame
/// table (the <c>ShownProjectileFrames</c> its <c>Present</c> looks a pose up in), its program set (the steps its
/// <c>Programs</c> names), and the cues whose handlers switch an object to the room's graphics. A program's frames are the
/// table entries of the poses its steps construct, inlining the program set's own step helpers with their constant
/// arguments; a pose argument no constant decides (a loop's phase) matches every entry the pose's other arguments do. The
/// programs a step names are run on into. A family is started in the programs its start names, together with every one
/// of its programs Core names outside the program set and its starts, since code naming a program can switch an object
/// to it. A family whose <c>Present</c> looks its pose up through other state shows every frame its table holds.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class ProjectileProgramFrames(CoreModel model)
{
    /// <summary>A pose argument no constant decides.</summary>
    private const string Any = "*";

    private readonly Dictionary<INamedTypeSymbol, FamilyProgramSet> _families = new(SymbolEqualityComparer.Default);
    private HashSet<IFieldSymbol>? _roomCues;
    private HashSet<SwitchSectionSyntax>? _roomCueSections;
    private readonly Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, HashSet<string>?>> _cueStarts = new(SymbolEqualityComparer.Default);

    /// <summary>The frames <paramref name="family"/>'s table holds, which its programs choose among.</summary>
    public IReadOnlySet<string> TableFrames(INamedTypeSymbol family) => Of(family).TableFrames;

    /// <summary>
    /// Whether <paramref name="section"/> is a cue handler's case that switches an object to the room's graphics, whose
    /// effect the programs publishing its cue carry: the frames they show after it draw from the shared sheet.
    /// </summary>
    public bool IsRoomGraphicsCueSection(SwitchSectionSyntax section)
    {
        FindRoomCues();
        return _roomCueSections!.Contains(section);
    }

    /// <summary>
    /// The frames <paramref name="family"/> shows when started in <paramref name="programs"/>, or in any of its programs
    /// when none are named.
    /// </summary>
    /// <exception cref="InvalidDataException">The family's programs or table cannot be read, or name a program or pose they lack.</exception>
    public ProgramFrameSplit Split(INamedTypeSymbol family, IReadOnlySet<string>? programs) => Shown(family, Of(family), programs);

    /// <summary>The frames <paramref name="set"/>, <paramref name="family"/>'s, shows when started in <paramref name="programs"/>, or in any.</summary>
    private static ProgramFrameSplit Shown(INamedTypeSymbol family, FamilyProgramSet set, IReadOnlySet<string>? programs)
    {
        if (!set.Precise)
            return new ProgramFrameSplit(set.TableFrames, new HashSet<string>(StringComparer.Ordinal));
        var spawned = new HashSet<string>(StringComparer.Ordinal);
        var room = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<(string Program, bool Switched)>();
        var pending = new Queue<(string Program, bool Switched)>();
        IEnumerable<string> entries = programs ?? (IEnumerable<string>)set.Programs.Keys;
        foreach (string entry in entries.Concat(set.NamedOutside))
            pending.Enqueue((entry, false));
        while (pending.TryDequeue(out (string Program, bool Switched) next))
        {
            if (!visited.Add(next))
                continue;
            if (!set.Programs.TryGetValue(next.Program, out List<ProgramItem>? items))
                throw new InvalidDataException($"{family.ToDisplayString()} is started in program {next.Program}, which its program set lacks.");
            bool switched = next.Switched;
            foreach (ProgramItem item in items)
            {
                switch (item.Kind)
                {
                    case ProgramItemKind.RoomGraphics:
                        switched = true;
                        break;
                    case ProgramItemKind.Continue:
                        pending.Enqueue((item.Value, switched));
                        break;
                    case ProgramItemKind.Show:
                        string[] shown = [.. set.Table.Where(entry => Matches(item.Value, entry.Key)).Select(entry => entry.Value)];
                        if (shown.Length == 0)
                            throw new InvalidDataException($"{family.ToDisplayString()}'s program {next.Program} shows pose {item.Value}, which its frame table lacks.");
                        (switched ? room : spawned).UnionWith(shown);
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined program item kind {item.Kind}.");
                }
            }
        }
        return new ProgramFrameSplit(spawned, room);
    }

    /// <summary>Whether pose pattern <paramref name="pattern"/> names table key <paramref name="key"/>: every argument it decides is the key's.</summary>
    private static bool Matches(string pattern, string key)
    {
        string[] wanted = pattern.Split(','), held = key.Split(',');
        if (wanted.Length != held.Length)
            return false;
        for (int index = 0; index < wanted.Length; index++)
        {
            if (wanted[index] != held[index] && !wanted[index].EndsWith("=" + Any, StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    /// <summary>The program set and table of <paramref name="family"/>, read once.</summary>
    private FamilyProgramSet Of(INamedTypeSymbol family)
    {
        if (!_families.TryGetValue(family, out FamilyProgramSet? set))
        {
            set = Read(family);
            _families.Add(family, set);
        }
        return set;
    }

    /// <summary>Reads <paramref name="family"/>'s table, program set and the programs Core names outside them.</summary>
    private FamilyProgramSet Read(INamedTypeSymbol family)
    {
        INamedTypeSymbol animation = ProgramAnimationOf(family);
        var programType = (INamedTypeSymbol)animation.TypeArguments[0];
        var frameType = (INamedTypeSymbol)animation.TypeArguments[1];
        (ISymbol table, bool precise) = TableOf(family);
        Dictionary<string, string> entries = TableEntries(family, table, frameType);
        var tableFrames = new HashSet<string>(entries.Values, StringComparer.Ordinal);
        if (!precise)
            return new FamilyProgramSet(entries, tableFrames, new Dictionary<string, List<ProgramItem>>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal), Precise: false);
        ISymbol programSet = ProgramSetOf(family);
        ExpressionSyntax initializer = InitializerOf(programSet);
        var programs = new Dictionary<string, List<ProgramItem>>(StringComparer.Ordinal);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        INamedTypeSymbol helpers = programSet.ContainingType;
        foreach (AssignmentExpressionSyntax entry in initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry is not { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var name] } }
                || semantic.GetSymbolInfo(name.Expression).Symbol is not IFieldSymbol identity
                || !SymbolEqualityComparer.Default.Equals(identity.ContainingType, programType))
                continue;
            var items = new List<ProgramItem>();
            Visit(entry.Right, semantic, new Dictionary<string, string>(StringComparer.Ordinal), items, programType, frameType, helpers, depth: 0);
            if (!programs.TryAdd(identity.Name, items))
                throw new InvalidDataException($"{family.ToDisplayString()}'s program set lists program {identity.Name} twice.");
        }
        if (programs.Count == 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s program set {programSet.ToDisplayString()} has no [Program] = steps entries this generator can read.");
        // An object's own cues can start it, or more of its family, in other programs (an afterburn spawning the next), so
        // whatever its cue handlers start is among the programs every start of the family runs.
        HashSet<string> entered = NamedOutside(family, programType, initializer);
        entered.UnionWith(CueStarted(family, (INamedTypeSymbol)animation.TypeArguments[2]) ?? programs.Keys.ToHashSet(StringComparer.Ordinal));
        var set = new FamilyProgramSet(entries, tableFrames, programs, entered, Precise: true);
        // The self-check: every frame the table holds is shown by some program, so no step the reader missed leaves a frame
        // the family draws off its sheets.
        ProgramFrameSplit all = Shown(family, set, programs: null);
        string[] unshown = [.. tableFrames.Where(frame => !all.Spawned.Contains(frame) && !all.Room.Contains(frame)).Order(StringComparer.Ordinal)];
        if (unshown.Length > 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s frame table holds frames none of its programs shows as this generator reads them: {string.Join(", ", unshown)}");
        return set;
    }

    /// <summary>
    /// Records, in source order, what <paramref name="node"/> contributes to a program: the poses its key constructions
    /// show, the room-graphics cues it publishes, and the programs it names, inlining calls of the program set's own step
    /// helpers (<paramref name="helpers"/>) with their arguments bound.
    /// </summary>
    private void Visit(SyntaxNode node, SemanticModel semantic, IReadOnlyDictionary<string, string> bindings, List<ProgramItem> items,
        INamedTypeSymbol programType, INamedTypeSymbol frameType, INamedTypeSymbol helpers, int depth)
    {
        switch (node)
        {
            case BaseObjectCreationExpressionSyntax creation
                when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                     && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, frameType):
                items.Add(new ProgramItem(ProgramItemKind.Show, PoseOf(semantic, creation, constructor, bindings)));
                return;
            case InvocationExpressionSyntax invocation
                when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol helper
                     && IsStepHelper(helper, helpers):
                if (depth > 8)
                    throw new InvalidDataException($"{helpers.ToDisplayString()}'s step helpers nest deeper than this generator inlines: {invocation}");
                foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                    Visit(argument, semantic, bindings, items, programType, frameType, helpers, depth);
                var bound = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int index = 0; index < invocation.ArgumentList.Arguments.Count; index++)
                {
                    ArgumentSyntax argument = invocation.ArgumentList.Arguments[index];
                    IParameterSymbol? parameter = argument.NameColon is { } named
                        ? helper.Parameters.FirstOrDefault(candidate => candidate.Name == named.Name.Identifier.Text)
                        : index < helper.Parameters.Length ? helper.Parameters[index] : null;
                    if (parameter is not null)
                        bound[parameter.Name] = ValueOf(semantic, argument.Expression, bindings);
                }
                foreach (SyntaxReference reference in helper.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax() is not MethodDeclarationSyntax declaration)
                        continue;
                    SyntaxNode? body = (SyntaxNode?)declaration.ExpressionBody ?? declaration.Body;
                    if (body is not null)
                        Visit(body, model.ModelOf(declaration.SyntaxTree), bound, items, programType, frameType, helpers, depth + 1);
                }
                return;
            case IdentifierNameSyntax name when semantic.GetSymbolInfo(name).Symbol is IFieldSymbol field:
                FindRoomCues();
                if (_roomCues!.Contains(field.OriginalDefinition))
                    items.Add(new ProgramItem(ProgramItemKind.RoomGraphics, field.Name));
                else if (SymbolEqualityComparer.Default.Equals(field.ContainingType, programType))
                    items.Add(new ProgramItem(ProgramItemKind.Continue, field.Name));
                return;
        }
        foreach (SyntaxNode child in node.ChildNodes())
            Visit(child, semantic, bindings, items, programType, frameType, helpers, depth);
    }

    /// <summary>
    /// Whether <paramref name="method"/> is a step helper whose body a program's call of it is read through: a method of
    /// the program set's type (<paramref name="helpers"/>), or any other Core method yielding steps, but not the step type's
    /// own factories.
    /// </summary>
    private bool IsStepHelper(IMethodSymbol method, INamedTypeSymbol helpers) =>
        model.IsSource(method) && method.MethodKind == MethodKind.Ordinary && method.DeclaringSyntaxReferences.Length > 0
        && method.ContainingType.OriginalDefinition.Name != "AnimationProgramStep"
        && (SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, helpers)
            || method.ReturnType.ToDisplayString().Contains("AnimationProgramStep", StringComparison.Ordinal));

    /// <summary>
    /// The pose <paramref name="creation"/> constructs, as <c>Parameter=Value</c> pairs in parameter order: each argument's
    /// constant, the bound value of a helper parameter it names, an omitted parameter's default, or <see cref="Any"/>.
    /// </summary>
    private static string PoseOf(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, IMethodSymbol constructor,
        IReadOnlyDictionary<string, string> bindings)
    {
        string[] values = [.. constructor.Parameters.Select(parameter => parameter.HasExplicitDefaultValue
            ? Convert.ToString(parameter.ExplicitDefaultValue, System.Globalization.CultureInfo.InvariantCulture) ?? "null" : Any)];
        SeparatedSyntaxList<ArgumentSyntax> arguments = creation.ArgumentList?.Arguments ?? default;
        for (int index = 0; index < arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments[index];
            int position = argument.NameColon is { } named
                ? constructor.Parameters.IndexOf(constructor.Parameters.FirstOrDefault(parameter => parameter.Name == named.Name.Identifier.Text)!)
                : index;
            if (position >= 0 && position < values.Length)
                values[position] = ValueOf(semantic, argument.Expression, bindings);
        }
        return string.Join(',', constructor.Parameters.Select((parameter, index) => $"{parameter.Name}={values[index]}"));
    }

    /// <summary>The value <paramref name="expression"/> passes: a constant, a bound helper parameter's value, or <see cref="Any"/>.</summary>
    private static string ValueOf(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<string, string> bindings)
    {
        if (ConstantOf(semantic, expression) is { } constant)
            return constant;
        return semantic.GetSymbolInfo(expression).Symbol is IParameterSymbol parameter && bindings.TryGetValue(parameter.Name, out string? bound)
            ? bound : Any;
    }

    /// <summary>The constant <paramref name="expression"/> is: an enum member's name, or a literal or constant's value.</summary>
    private static string? ConstantOf(SemanticModel semantic, ExpressionSyntax expression) =>
        semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member
            ? member.Name
            : semantic.GetConstantValue(expression) is { HasValue: true, Value: { } value }
                ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) : null;

    /// <summary>The table's entries, by the constant pose key each is keyed by.</summary>
    private Dictionary<string, string> TableEntries(INamedTypeSymbol family, ISymbol table, INamedTypeSymbol frameType)
    {
        ExpressionSyntax initializer = InitializerOf(table);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        var empty = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (SyntaxNode node in initializer.DescendantNodes())
        {
            (ExpressionSyntax? key, ExpressionSyntax? value) = node switch
            {
                AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var index] }, Right: var right } => (index.Expression, right),
                InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ComplexElementInitializerExpression, Expressions: [var first, var second] } => (first, second),
                _ => ((ExpressionSyntax?)null, (ExpressionSyntax?)null),
            };
            if (key is null || value is null || semantic.GetSymbolInfo(value).Symbol is not IFieldSymbol frame
                || !SymbolEqualityComparer.Default.Equals(frame.ContainingType, model.EnemyProjectileFrameId))
                continue;
            // A pose is a construction of the family's pose type, or for a plain-valued pose its constant.
            string? pose = key is BaseObjectCreationExpressionSyntax creation
                && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, frameType)
                ? PoseOf(semantic, creation, constructor, empty)
                : frameType.SpecialType != SpecialType.None ? ConstantOf(semantic, key) : null;
            if (pose is null || pose.Split(',').Any(part => part.EndsWith("=" + Any, StringComparison.Ordinal)))
                throw new InvalidDataException($"{family.ToDisplayString()}'s frame table keys a frame by {key}, which is no constant pose.");
            if (!entries.TryAdd(pose, frame.Name))
                throw new InvalidDataException($"{family.ToDisplayString()}'s frame table lists pose {pose} twice.");
        }
        if (entries.Count == 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s frame table {table.ToDisplayString()} has no pose entries this generator can read.");
        return entries;
    }

    /// <summary>
    /// The frame table <paramref name="family"/>'s <c>Present</c> indexes, and whether it indexes it with the pose it is
    /// handed, so that each pose a program shows is a table key.
    /// </summary>
    private (ISymbol Table, bool Precise) TableOf(INamedTypeSymbol family)
    {
        IMethodSymbol present = DeclaredIn(family, "Present").OfType<IMethodSymbol>().FirstOrDefault()
            ?? throw new InvalidDataException($"{family.ToDisplayString()} declares no Present this generator can read.");
        foreach (SyntaxReference reference in present.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax declaration)
                continue;
            SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
            foreach (ElementAccessExpressionSyntax access in declaration.DescendantNodes().OfType<ElementAccessExpressionSyntax>())
            {
                ISymbol? table = semantic.GetSymbolInfo(access.Expression).Symbol;
                if (table is not (IPropertySymbol or IFieldSymbol) || table.Name != "ShownProjectileFrames")
                    continue;
                // A plain-valued pose (a number) is shown by value, not constructed, so its programs cannot be read pose by pose.
                bool precise = access.ArgumentList.Arguments is [var argument]
                    && semantic.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol owner, Type.SpecialType: SpecialType.None }
                    && SymbolEqualityComparer.Default.Equals(owner.OriginalDefinition, present.OriginalDefinition);
                return (table.OriginalDefinition, precise);
            }
        }
        throw new InvalidDataException($"{family.ToDisplayString()}'s Present indexes no ShownProjectileFrames table.");
    }

    /// <summary>The program set <paramref name="family"/>'s <c>Programs</c> names.</summary>
    private ISymbol ProgramSetOf(INamedTypeSymbol family)
    {
        IPropertySymbol programs = DeclaredIn(family, "Programs").OfType<IPropertySymbol>().FirstOrDefault()
            ?? throw new InvalidDataException($"{family.ToDisplayString()} declares no Programs this generator can read.");
        foreach (SyntaxReference reference in programs.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;
            SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
            foreach (ExpressionSyntax expression in declaration.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (semantic.GetSymbolInfo(expression).Symbol is ({ } set and (IPropertySymbol or IFieldSymbol))
                    && TypeOf(set) is INamedTypeSymbol { Name: "AnimationProgramSet" })
                    return set.OriginalDefinition;
            }
        }
        throw new InvalidDataException($"{family.ToDisplayString()}'s Programs names no AnimationProgramSet.");
    }

    /// <summary>The type of property or field <paramref name="member"/>.</summary>
    private static ITypeSymbol? TypeOf(ISymbol member) => member switch
    {
        IPropertySymbol property => property.Type,
        IFieldSymbol field => field.Type,
        _ => null,
    };

    /// <summary>The members named <paramref name="name"/> declared in source by <paramref name="family"/> or a base class of it.</summary>
    private IEnumerable<ISymbol> DeclaredIn(INamedTypeSymbol family, string name)
    {
        for (INamedTypeSymbol? type = family; type is not null && model.IsSource(type); type = type.BaseType)
        {
            foreach (ISymbol member in type.GetMembers(name))
            {
                if (!member.IsAbstract && member.DeclaringSyntaxReferences.Length > 0)
                    yield return member;
            }
        }
    }

    /// <summary>The <c>EnemyProjectileProgramAnimation</c> base <paramref name="family"/> derives from, with its type arguments.</summary>
    private INamedTypeSymbol ProgramAnimationOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.ProgramAnimation))
                return type;
        }
        throw new InvalidDataException($"{family.ToDisplayString()} does not derive from {model.ProgramAnimation.Name}.");
    }

    /// <summary>The initializer value of property or field <paramref name="member"/>.</summary>
    private static ExpressionSyntax InitializerOf(ISymbol member)
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
    /// The members of <paramref name="programType"/> Core names outside <paramref name="initializer"/> and the families'
    /// starts: code naming a program can start an object in it. Tests of a program (case labels, patterns, equality) start
    /// nothing.
    /// </summary>
    private HashSet<string> NamedOutside(INamedTypeSymbol family, INamedTypeSymbol programType, SyntaxNode initializer)
    {
        var named = new HashSet<string>(StringComparer.Ordinal);
        var members = new HashSet<string>(programType.GetMembers().OfType<IFieldSymbol>().Select(member => member.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                // Only a name spelled as one of the program type's members can name one; the rest need no binding.
                if (!members.Contains(name.Identifier.Text))
                    continue;
                semantic ??= model.ModelOf(tree);
                if (semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol field || !SymbolEqualityComparer.Default.Equals(field.ContainingType, programType))
                    continue;
                if (name.Ancestors().Any(node => node == initializer || IsTest(node, name))
                    || name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" })
                    || IsStartArgument(semantic, name, family))
                    continue;
                named.Add(field.Name);
            }
        }
        return named;
    }

    /// <summary>
    /// The programs of <paramref name="family"/> the handlers of its cues (<paramref name="cueType"/>, registered with
    /// <c>AddCue</c>) start, through every Core method and construction they reach: its constructions there, and the
    /// typed-start kinds they name that start it. Null when one of them names a program no constant decides.
    /// </summary>
    private HashSet<string>? CueStarted(INamedTypeSymbol family, INamedTypeSymbol cueType)
    {
        if (!_cueStarts.TryGetValue(cueType, out Dictionary<INamedTypeSymbol, HashSet<string>?>? started))
        {
            started = HandlerStarts(cueType);
            _cueStarts.Add(cueType, started);
        }
        return started.TryGetValue(family, out HashSet<string>? programs) ? programs : new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Every projectile program family the handlers of the cues <paramref name="cueType"/> names start, through every Core
    /// method and construction they reach, with the programs they start it in; null for a family started in a program no
    /// constant decides.
    /// </summary>
    private Dictionary<INamedTypeSymbol, HashSet<string>?> HandlerStarts(INamedTypeSymbol cueType)
    {
        var started = new Dictionary<INamedTypeSymbol, HashSet<string>?>(SymbolEqualityComparer.Default);
        var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var pending = new Stack<SyntaxNode>();
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (InvocationExpressionSyntax invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                // Only a call spelled with AddCue can register a cue handler; the rest need no binding.
                if (!invocation.Expression.ToString().Contains("AddCue", StringComparison.Ordinal))
                    continue;
                semantic ??= model.ModelOf(tree);
                if (semantic.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "AddCue", TypeArguments: [var registered] }
                    || !SymbolEqualityComparer.Default.Equals(registered.OriginalDefinition, cueType))
                    continue;
                foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                    pending.Push(argument.Expression);
            }
        }
        while (pending.TryPop(out SyntaxNode? root))
        {
            SemanticModel semantic = model.ModelOf(root.SyntaxTree);
            foreach (SyntaxNode node in root.DescendantNodesAndSelf())
            {
                switch (node)
                {
                    case BaseObjectCreationExpressionSyntax creation when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol constructor:
                        INamedTypeSymbol created = constructor.ContainingType.OriginalDefinition;
                        if (model.FamilyOf(created) is { } family && SymbolEqualityComparer.Default.Equals(family, created))
                            Started(family, model.ProgramsOf(semantic, creation, family));
                        else
                            Enter(constructor);
                        break;
                    case SimpleNameSyntax name when semantic.GetSymbolInfo(name).Symbol is { } symbol:
                        if (symbol is IMethodSymbol method)
                            Enter(method);
                        else if (symbol is IFieldSymbol kind && SymbolEqualityComparer.Default.Equals(kind.ContainingType, model.RoomEnemyProjectileKind)
                                 && model.StartedFamily(kind) is { } typedFamily && !name.Ancestors().Any(ancestor => IsTest(ancestor, name)))
                            Started(typedFamily, model.StartedPrograms(kind));
                        break;
                }
            }
        }
        return started;

        // A family started in programs no constant decides may be started in any of them.
        void Started(INamedTypeSymbol family, IReadOnlySet<string>? programs)
        {
            if (started.TryGetValue(family, out HashSet<string>? known) && known is null)
                return;
            if (programs is null)
                started[family] = null;
            else if (known is null)
                started[family] = new HashSet<string>(programs, StringComparer.Ordinal);
            else
                known.UnionWith(programs);
        }

        // Code the handlers run is read once, Core's own and outside the tables naming every kind.
        void Enter(IMethodSymbol method)
        {
            IMethodSymbol definition = (method.ReducedFrom ?? method).OriginalDefinition;
            if (!model.IsSource(definition) || definition.ContainingType is not { } owner || model.IsTable(owner) || !visited.Add(definition))
                return;
            foreach (SyntaxReference reference in definition.DeclaringSyntaxReferences)
                pending.Push(reference.GetSyntax());
        }
    }

    /// <summary>Whether <paramref name="node"/>, an ancestor of <paramref name="name"/>, only tests or annotates with it: an attribute, a case or pattern, an equality.</summary>
    private static bool IsTest(SyntaxNode node, SyntaxNode name) => node switch
    {
        AttributeListSyntax or CaseSwitchLabelSyntax or CasePatternSwitchLabelSyntax => true,
        SwitchExpressionArmSyntax arm => arm.Pattern.Span.Contains(name.Span),
        IsPatternExpressionSyntax test => test.Pattern.Span.Contains(name.Span),
        BinaryExpressionSyntax binary => binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression),
        _ => false,
    };

    /// <summary>Whether <paramref name="name"/> is the program argument of a construction of <paramref name="family"/>, a start the walks record.</summary>
    private static bool IsStartArgument(SemanticModel semantic, IdentifierNameSyntax name, INamedTypeSymbol family)
    {
        SyntaxNode node = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        // A program a conditional or switch chooses is the argument too.
        while (true)
        {
            switch (node.Parent)
            {
                case ConditionalExpressionSyntax conditional when conditional.WhenTrue == node || conditional.WhenFalse == node:
                    node = conditional;
                    continue;
                case ParenthesizedExpressionSyntax parenthesized:
                    node = parenthesized;
                    continue;
                case SwitchExpressionArmSyntax arm when arm.Expression == node && arm.Parent is SwitchExpressionSyntax choice:
                    node = choice;
                    continue;
            }
            break;
        }
        return node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax creation } }
            && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
            && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, family);
    }

    /// <summary>
    /// Finds, once, the switch sections that switch an object to the room's graphics (<c>Graphics = default</c>) and the
    /// enum members their labels name: the cues whose publication switches the object.
    /// </summary>
    private void FindRoomCues()
    {
        if (_roomCues is not null)
            return;
        var cues = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var sections = new HashSet<SwitchSectionSyntax>();
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (SwitchSectionSyntax section in tree.GetRoot().DescendantNodes().OfType<SwitchSectionSyntax>())
            {
                if (!section.Statements.SelectMany(statement => statement.DescendantNodesAndSelf()).OfType<AssignmentExpressionSyntax>()
                        .Any(assignment => IsRoomGraphicsAssignment(semantic, assignment)))
                    continue;
                sections.Add(section);
                foreach (SwitchLabelSyntax label in section.Labels)
                {
                    foreach (IdentifierNameSyntax named in label.DescendantNodes().OfType<IdentifierNameSyntax>())
                    {
                        if (semantic.GetSymbolInfo(named).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member)
                            cues.Add(member.OriginalDefinition);
                    }
                }
            }
        }
        _roomCueSections = sections;
        _roomCues = cues;
    }

    /// <summary>Whether <paramref name="assignment"/> sets an object's graphics to the room's: <c>X.Graphics = default</c>.</summary>
    private bool IsRoomGraphicsAssignment(SemanticModel semantic, AssignmentExpressionSyntax assignment) =>
        assignment is { RawKind: (int)SyntaxKind.SimpleAssignmentExpression, Left: MemberAccessExpressionSyntax { Name.Identifier.Text: "Graphics" } }
        && assignment.Right is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } or DefaultExpressionSyntax
        && semantic.GetTypeInfo(assignment.Left).Type is { } assigned
        && SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, model.SpriteGraphics);

    /// <summary>What one program step contributes.</summary>
    private enum ProgramItemKind
    {
        /// <summary>It shows a pose.</summary>
        Show,
        /// <summary>It publishes a cue that switches the object to the room's graphics.</summary>
        RoomGraphics,
        /// <summary>It names a program the object runs on into.</summary>
        Continue,
    }

    /// <summary>One thing a program step contributes, in step order.</summary>
    /// <param name="Kind">What it is.</param>
    /// <param name="Value">The pose pattern shown, the cue published, or the program run on into.</param>
    private readonly record struct ProgramItem(ProgramItemKind Kind, string Value);

    /// <summary>One family's table and program set.</summary>
    /// <param name="Table">Each pose key's frame.</param>
    /// <param name="TableFrames">Every frame the table holds.</param>
    /// <param name="Programs">Each program's items, in step order.</param>
    /// <param name="NamedOutside">The programs Core names outside the program set and the family's starts.</param>
    /// <param name="Precise">Whether each pose a program shows is a table key, so the programs choose the frames.</param>
    private sealed record FamilyProgramSet(IReadOnlyDictionary<string, string> Table, IReadOnlySet<string> TableFrames,
        Dictionary<string, List<ProgramItem>> Programs, IReadOnlySet<string> NamedOutside, bool Precise);
}
