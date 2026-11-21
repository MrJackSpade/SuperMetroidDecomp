using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuperMetroid.EnsureAnalyzer.Generators;

/// <summary>
/// Which declared types the definition catalogs' members can reach, read from the declarations alone (#1716, #2602), so a
/// debugger state asks a catalog about an object only when a location the catalog reaches is declared with a type the
/// object is of. Each declared type a catalog member reaches is registered with what a location declared with it holds
/// and a test of whether an object is of it; the debugger closes the catalogs over what they hold when it first needs them.
/// </summary>
/// <remarks>
/// A location declared with a type of the game's assembly, or with any value type, holds what each instance field of the
/// type and its bases is declared with; an array holds its element type; another assembly's class or interface holds its
/// type arguments, comparers of them, arrays of them where it is an interface an array implements, and the elements it
/// enumerates. A class that is not sealed, or an interface, also holds each of the game's concrete types that derives from
/// or implements it, a generic one taken in each construction its fields' declared types bind to constructions the game's
/// declarations name (#2420). A location declared as <see cref="object"/>, <see cref="ValueType"/> or a collection that
/// enumerates objects of no declared type can hold anything; a delegate, a string and reflection metadata hold nothing.
/// </remarks>
internal sealed class StateGraphExposure(StateGraphRun run)
{
    private static readonly DiagnosticDescriptor NoProbe = new("SME6494", "Declared type no registration can test",
        "No type can test whether an object is a {0}: it names types private to unrelated types, so a debugger state cannot ask the catalogs about it (#2602)",
        "Build", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private readonly Compilation compilation = run.Compilation;
    private readonly Dictionary<INamedTypeSymbol, List<(INamedTypeSymbol Derived, INamedTypeSymbol Declared)>> derived = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> named = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, int> numbers = new(SymbolEqualityComparer.Default);
    private readonly List<ITypeSymbol> declared = [];

    /// <summary>The name a declared type is registered by.</summary>
    internal static string Key(ITypeSymbol type) => StateGraphRun.Code(type);

    /// <summary>Registers every declared type the catalogs' members reach, what each holds, and the test of each.</summary>
    internal void Collect()
    {
        var namedSeen = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol type in run.Types)
        {
            FileDerived(type);
            FileNamed(type, namedSeen);
        }
        var pending = new Stack<ITypeSymbol>();
        foreach (ITypeSymbol member in run.CatalogMemberTypes)
            pending.Push(member);
        var held = new Dictionary<int, HashSet<int>>();
        var visited = new HashSet<int>();
        while (pending.Count > 0)
        {
            int number = Number(pending.Pop());
            if (!visited.Add(number))
                continue;
            var types = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            AddHeld(declared[number], types);
            var heldNumbers = new HashSet<int>();
            foreach (ITypeSymbol type in types)
            {
                heldNumbers.Add(Number(type));
                pending.Push(type);
            }
            held[number] = heldNumbers;
        }
        foreach (int number in visited.OrderBy(number => number))
        {
            ITypeSymbol type = declared[number];
            string heldKeys = string.Join(", ", held[number].OrderBy(next => next).Select(next => StateGraphRun.Literal(Key(declared[next]))));
            run.AddAssemblyRegistration($"__registry.Holds({StateGraphRun.Literal(Key(type))}, {(HoldsAnything(type) ? "true" : "false")}, new string[] {{ {heldKeys} }});");
            Probe(type);
        }
    }

    /// <summary>Registers the test of whether an object is of a declared type, where every type it names is accessible.</summary>
    private void Probe(ITypeSymbol type)
    {
        ITypeSymbol tested = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable ? nullable.TypeArguments[0] : type;
        if (tested.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer || tested.IsRefLikeType || ContainsTypeParameters(tested))
            return;
        INamedTypeSymbol? context = null;
        foreach (INamedTypeSymbol required in Requirements(tested))
        {
            if (context is null || Contains(context, required))
                context = required;
            else if (!Contains(required, context))
            {
                run.Report(Diagnostic.Create(NoProbe, Location.None, tested.ToDisplayString()));
                return;
            }
        }
        string line = $"__registry.Declared({StateGraphRun.Literal(Key(type))}, static __value => __value is {StateGraphRun.Code(tested)});";
        if (context is null)
            run.AddAssemblyRegistration(line);
        else
            run.AddRegistration(context, line);
    }

    /// <summary>Whether <paramref name="outer"/> is <paramref name="inner"/> or contains it.</summary>
    private static bool Contains(INamedTypeSymbol outer, INamedTypeSymbol inner)
    {
        for (INamedTypeSymbol? current = inner; current is not null; current = current.ContainingType)
            if (SymbolEqualityComparer.Default.Equals(current, outer))
                return true;
        return false;
    }

    /// <summary>The types code naming <paramref name="type"/> must lie inside: each private or protected type's container.</summary>
    private static IEnumerable<INamedTypeSymbol> Requirements(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                foreach (INamedTypeSymbol required in Requirements(array.ElementType))
                    yield return required;
                yield break;
            case INamedTypeSymbol named:
                for (INamedTypeSymbol? current = named; current is not null; current = current.ContainingType)
                {
                    if (current.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal &&
                        current.ContainingType is { } container)
                        yield return container.OriginalDefinition;
                    foreach (ITypeSymbol argument in current.TypeArguments)
                        foreach (INamedTypeSymbol required in Requirements(argument))
                            yield return required;
                }
                yield break;
        }
    }

    private bool IsGame(ITypeSymbol type) => SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly);

    private int Number(ITypeSymbol type)
    {
        if (numbers.TryGetValue(type, out int number))
            return number;
        number = declared.Count;
        numbers.Add(type, number);
        declared.Add(type);
        return number;
    }

    private void FileDerived(INamedTypeSymbol type)
    {
        if (type.IsAbstract || type.TypeKind == TypeKind.Interface || type.IsStatic)
            return;
        for (INamedTypeSymbol? baseType = type.BaseType; baseType is not null && baseType.SpecialType != SpecialType.System_Object; baseType = baseType.BaseType)
            File(type, baseType);
        foreach (INamedTypeSymbol implemented in type.AllInterfaces)
            File(type, implemented);
    }

    private void File(INamedTypeSymbol type, INamedTypeSymbol declaredBase)
    {
        INamedTypeSymbol key = declaredBase.OriginalDefinition;
        if (!derived.TryGetValue(key, out List<(INamedTypeSymbol Derived, INamedTypeSymbol Declared)>? filed))
            derived.Add(key, filed = []);
        filed.Add((type, declaredBase));
    }

    private void FileNamed(INamedTypeSymbol type, HashSet<ITypeSymbol> seen)
    {
        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
            Name(field.Type, seen);
        foreach (ITypeSymbol captured in run.CapturedTypes(type))
            Name(captured, seen);
        if (type.BaseType is { } baseType)
            Name(baseType, seen);
        foreach (INamedTypeSymbol implemented in type.Interfaces)
            Name(implemented, seen);
    }

    private void Name(ITypeSymbol type, HashSet<ITypeSymbol> seen)
    {
        if (!seen.Add(type))
            return;
        if (type is IArrayTypeSymbol array)
        {
            Name(array.ElementType, seen);
            return;
        }
        if (type is not INamedTypeSymbol { IsGenericType: true } generic)
            return;
        if (!ContainsTypeParameters(generic))
        {
            if (!named.TryGetValue(generic.OriginalDefinition, out HashSet<INamedTypeSymbol>? constructions))
                named.Add(generic.OriginalDefinition, constructions = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
            constructions.Add(generic);
        }
        foreach (ITypeSymbol argument in generic.TypeArguments)
            Name(argument, seen);
    }

    private static bool ContainsTypeParameters(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol array => ContainsTypeParameters(array.ElementType),
        IPointerTypeSymbol pointer => ContainsTypeParameters(pointer.PointedAtType),
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameters) || named.ContainingType is { } container && ContainsTypeParameters(container),
        _ => false,
    };

    private bool HoldsAnything(ITypeSymbol type) =>
        type.SpecialType is SpecialType.System_Object or SpecialType.System_ValueType ||
        !IsGame(type) && !type.IsValueType && type.SpecialType != SpecialType.System_String && type is INamedTypeSymbol named &&
        (named.SpecialType == SpecialType.System_Collections_IEnumerable || named.AllInterfaces.Any(@interface => @interface.SpecialType == SpecialType.System_Collections_IEnumerable)) &&
        Enumerated(named).Count == 0;

    private void AddHeld(ITypeSymbol type, HashSet<ITypeSymbol> types)
    {
        if (type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.Enum or TypeKind.Delegate || type.IsRefLikeType ||
            ContainsTypeParameters(type) || type.SpecialType is >= SpecialType.System_Boolean and <= SpecialType.System_String ||
            type.SpecialType == SpecialType.System_IntPtr || type.SpecialType == SpecialType.System_UIntPtr || HoldsAnything(type) ||
            type.ContainingNamespace?.ToDisplayString().StartsWith("System.Reflection", StringComparison.Ordinal) == true)
            return;
        if (type is IArrayTypeSymbol array)
        {
            types.Add(array.ElementType);
            return;
        }
        if (type is not INamedTypeSymbol named)
            return;
        if (IsGame(named) || named.IsValueType)
        {
            for (INamedTypeSymbol? current = named; current is not null; current = current.BaseType)
            {
                foreach (IFieldSymbol field in current.GetMembers().OfType<IFieldSymbol>())
                    if (!field.IsStatic && !field.IsConst && field.Type.TypeKind is not (TypeKind.Pointer or TypeKind.FunctionPointer))
                        types.Add(field.Type);
                foreach (ITypeSymbol captured in run.CapturedTypes(current))
                    types.Add(captured);
            }
            if (!IsGame(named))
                AddExposed(named, types);
        }
        else
        {
            AddExposed(named, types);
        }
        if (!named.IsValueType && !named.IsSealed)
            AddDerived(named, types);
    }

    private void AddExposed(INamedTypeSymbol type, HashSet<ITypeSymbol> types)
    {
        INamedTypeSymbol? equality = compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1");
        INamedTypeSymbol? comparer = compilation.GetTypeByMetadataName("System.Collections.Generic.IComparer`1");
        if (type.IsGenericType)
        {
            foreach (ITypeSymbol argument in type.TypeArguments)
            {
                types.Add(argument);
                if (argument.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer || argument.IsRefLikeType)
                    continue;
                if (equality is not null)
                    types.Add(equality.Construct(argument));
                if (comparer is not null)
                    types.Add(comparer.Construct(argument));
                if (type.TypeKind == TypeKind.Interface && IsArrayInterface(type))
                    types.Add(compilation.CreateArrayTypeSymbol(argument));
            }
        }
        foreach (ITypeSymbol element in Enumerated(type))
            types.Add(element);
    }

    private static bool IsArrayInterface(INamedTypeSymbol type) => type.OriginalDefinition.ToDisplayString() is
        "System.Collections.Generic.IEnumerable<T>" or "System.Collections.Generic.ICollection<T>" or "System.Collections.Generic.IList<T>" or
        "System.Collections.Generic.IReadOnlyCollection<T>" or "System.Collections.Generic.IReadOnlyList<T>";

    private static List<ITypeSymbol> Enumerated(INamedTypeSymbol type)
    {
        var elements = new List<ITypeSymbol>();
        IEnumerable<INamedTypeSymbol> interfaces = type.TypeKind == TypeKind.Interface ? [type, .. type.AllInterfaces] : type.AllInterfaces;
        foreach (INamedTypeSymbol implemented in interfaces)
            if (implemented.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                elements.Add(implemented.TypeArguments[0]);
        return elements;
    }

    private void AddDerived(INamedTypeSymbol type, HashSet<ITypeSymbol> types)
    {
        if (!derived.TryGetValue(type.OriginalDefinition, out List<(INamedTypeSymbol Derived, INamedTypeSymbol Declared)>? filed))
            return;
        foreach ((INamedTypeSymbol candidate, INamedTypeSymbol declaredBase) in filed)
        {
            if (candidate.TypeParameters.Length == 0 && !ContainsTypeParameters(candidate))
            {
                if (Assignable(candidate, type))
                    types.Add(candidate);
                continue;
            }
            foreach (INamedTypeSymbol constructed in Constructions(candidate, declaredBase, type))
                types.Add(constructed);
        }
    }

    private bool Assignable(ITypeSymbol from, ITypeSymbol to)
    {
        Conversion conversion = compilation.ClassifyConversion(from, to);
        return conversion.IsIdentity || conversion.IsImplicit && (conversion.IsReference || conversion.IsBoxing);
    }

    private List<INamedTypeSymbol> Constructions(INamedTypeSymbol definition, INamedTypeSymbol declaredBase, INamedTypeSymbol type)
    {
        var constructions = new List<INamedTypeSymbol>();
        if (definition.TypeParameters.Length == 0)
            return constructions;
        var arguments = new ITypeSymbol?[definition.TypeParameters.Length];
        if (!Unify(declaredBase, type, definition, arguments))
            return constructions;
        foreach (ITypeSymbol[] bound in Bindings(definition, arguments, FieldPatterns(definition)))
        {
            if (Constructed(definition, bound, type) is { } constructed && !constructions.Contains(constructed, SymbolEqualityComparer.Default))
                constructions.Add(constructed);
        }
        return constructions;
    }

    private IEnumerable<ITypeSymbol[]> Bindings(INamedTypeSymbol definition, ITypeSymbol?[] arguments, List<INamedTypeSymbol> patterns)
    {
        int boundCount = arguments.Count(argument => argument is not null);
        if (boundCount == arguments.Length)
        {
            yield return [.. arguments.Select(argument => argument!)];
            yield break;
        }
        foreach (INamedTypeSymbol pattern in patterns)
        {
            if (!named.TryGetValue(pattern.OriginalDefinition, out HashSet<INamedTypeSymbol>? constructions))
                continue;
            foreach (INamedTypeSymbol construction in constructions)
            {
                ITypeSymbol?[] extended = [.. arguments];
                if (!Unify(pattern, construction, definition, extended) || extended.Count(argument => argument is not null) == boundCount)
                    continue;
                foreach (ITypeSymbol[] bound in Bindings(definition, extended, patterns))
                    yield return bound;
            }
        }
    }

    private static List<INamedTypeSymbol> FieldPatterns(INamedTypeSymbol definition)
    {
        var patterns = new List<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = definition; current is not null; current = current.BaseType)
            foreach (IFieldSymbol field in current.GetMembers().OfType<IFieldSymbol>())
                if (!field.IsStatic)
                    AddPatterns(field.Type, patterns);
        return patterns;
    }

    private static void AddPatterns(ITypeSymbol type, List<INamedTypeSymbol> patterns)
    {
        if (type is IArrayTypeSymbol array)
        {
            AddPatterns(array.ElementType, patterns);
            return;
        }
        if (type is not INamedTypeSymbol { IsGenericType: true } generic || !ContainsTypeParameters(generic))
            return;
        if (!patterns.Contains(generic, SymbolEqualityComparer.Default))
            patterns.Add(generic);
        foreach (ITypeSymbol argument in generic.TypeArguments)
            AddPatterns(argument, patterns);
    }

    private INamedTypeSymbol? Constructed(INamedTypeSymbol definition, ITypeSymbol[] bound, INamedTypeSymbol type)
    {
        for (int index = 0; index < definition.TypeParameters.Length; index++)
            if (!Satisfies(definition.TypeParameters[index], bound[index], definition, bound))
                return null;
        INamedTypeSymbol constructed = definition.Construct(bound);
        return Assignable(constructed, type) ? constructed : null;
    }

    private static bool Unify(ITypeSymbol pattern, ITypeSymbol type, INamedTypeSymbol definition, ITypeSymbol?[] arguments)
    {
        if (pattern is ITypeParameterSymbol parameter)
        {
            if (parameter.TypeParameterKind != TypeParameterKind.Type || !SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, definition))
                return false;
            arguments[parameter.Ordinal] ??= type;
            return SymbolEqualityComparer.Default.Equals(arguments[parameter.Ordinal], type);
        }
        if (!ContainsTypeParameters(pattern))
            return SymbolEqualityComparer.Default.Equals(pattern, type);
        if (pattern is IArrayTypeSymbol patternArray)
            return type is IArrayTypeSymbol typeArray && patternArray.Rank == typeArray.Rank &&
                Unify(patternArray.ElementType, typeArray.ElementType, definition, arguments);
        if (pattern is not INamedTypeSymbol { IsGenericType: true } patternNamed || type is not INamedTypeSymbol { IsGenericType: true } typeNamed ||
            !SymbolEqualityComparer.Default.Equals(patternNamed.OriginalDefinition, typeNamed.OriginalDefinition))
            return false;
        for (int index = 0; index < patternNamed.TypeArguments.Length; index++)
            if (!Unify(patternNamed.TypeArguments[index], typeNamed.TypeArguments[index], definition, arguments))
                return false;
        return true;
    }

    private bool Satisfies(ITypeParameterSymbol parameter, ITypeSymbol argument, INamedTypeSymbol definition, ITypeSymbol[] bound)
    {
        if (parameter.HasReferenceTypeConstraint && argument.IsValueType)
            return false;
        if (parameter.HasValueTypeConstraint && (!argument.IsValueType || argument.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
            return false;
        if (parameter.HasConstructorConstraint && !argument.IsValueType &&
            (argument.IsAbstract || argument is not INamedTypeSymbol named ||
             !named.InstanceConstructors.Any(constructor => constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility == Accessibility.Public)))
            return false;
        if (argument.IsRefLikeType && !parameter.AllowsRefLikeType)
            return false;
        if (parameter.HasUnmanagedTypeConstraint && run.HoldsReferences(argument, []))
            return false;
        foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
            if (!Assignable(argument, Substituted(constraint, definition, bound)))
                return false;
        return true;
    }

    private ITypeSymbol Substituted(ITypeSymbol type, INamedTypeSymbol definition, ITypeSymbol[] bound)
    {
        if (type is ITypeParameterSymbol parameter && parameter.TypeParameterKind == TypeParameterKind.Type &&
            SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, definition))
            return bound[parameter.Ordinal];
        if (!ContainsTypeParameters(type))
            return type;
        if (type is IArrayTypeSymbol array)
            return compilation.CreateArrayTypeSymbol(Substituted(array.ElementType, definition, bound), array.Rank);
        if (type is INamedTypeSymbol { IsGenericType: true } generic)
            return generic.OriginalDefinition.Construct([.. generic.TypeArguments.Select(argument => Substituted(argument, definition, bound))]);
        return type;
    }
}
