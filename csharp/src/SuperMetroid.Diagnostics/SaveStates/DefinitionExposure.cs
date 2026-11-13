using System.Reflection;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Which definition catalogs may hold an object of a given type where a game can reach it, read from the catalogs'
/// declarations alone, so finding the catalogs that may hold an object runs no initializer (#1716). A catalog may hold
/// an object of a type when a location its members reach is declared with a type the object's type is assignable to.
/// The locations a catalog's members reach are found from declared types, closed over:
/// <list type="bullet">
/// <item>a type of the game's assembly, and any value type, holds what each of its instance fields is declared with;</item>
/// <item>an array holds its element type;</item>
/// <item>
/// another assembly's class or interface (a collection, a lazily loaded part) holds what it exposes: its type
/// arguments, the elements it enumerates, comparers of its type arguments, and arrays of its type arguments where the
/// interface is one an array implements. Its private storage and helpers (bucket arrays, entry arrays, the lock state of
/// a part not loaded yet) are no catalog's members: a game never gets them from a catalog. One the runtime shares
/// between instances, such as the empty array every empty list of an element type starts with, is written by its
/// contents, as an object no catalog holds is;
/// </item>
/// <item>a class that is not sealed, or an interface, holds what each type of the game's assembly that derives from or
/// implements it holds;</item>
/// <item>
/// a location declared as <see cref="object"/>, <see cref="ValueType"/> or a collection that enumerates objects of
/// no declared type can hold anything, so its catalog may hold an object of every type;
/// </item>
/// <item>a delegate, a string and reflection metadata hold nothing a game reaches through them.</item>
/// </list>
/// </summary>
[ReflectionAudit("debugger states find the catalogs that may hold an object from the declared types their static members reach")]
internal sealed class DefinitionExposure
{
    /// <summary>The bits one word of a catalog set holds.</summary>
    private const int WordBits = 64;

    /// <summary>The fields a type's own state is read from: every instance field, of every accessibility.</summary>
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>The game's assembly, whose types are read field by field.</summary>
    private readonly Assembly game;

    /// <summary>The catalogs, in their prefixes' ordinal order.</summary>
    private readonly IReadOnlyList<DefinitionCatalog> catalogs;

    /// <summary>
    /// The game's concrete types under each type they derive from or implement, a constructed type filed under its generic
    /// definition: each with the base or interface it derives from or implements as it declares it, in its own type
    /// parameters for a generic definition.
    /// </summary>
    private readonly Dictionary<Type, List<(Type Derived, Type Declared)>> derived = [];

    /// <summary>Each declared type reached, by its number.</summary>
    private readonly Dictionary<Type, int> numbers = [];

    /// <summary>Each declared type reached, in number order.</summary>
    private readonly List<Type> declared = [];

    /// <summary>The numbers of the types each declared type holds.</summary>
    private readonly List<int[]?> held = [];

    /// <summary>Whether a location declared with each declared type can hold an object of any type.</summary>
    private readonly List<bool> anything = [];

    /// <summary>The catalogs whose members reach each declared type, as one bit per catalog in catalog order.</summary>
    private readonly List<ulong[]?> reaching = [];

    /// <summary>
    /// The declared generic interfaces and generic delegates, by generic definition: an object can be assigned to one
    /// through variance or array covariance without implementing or being that very type.
    /// </summary>
    private readonly Dictionary<Type, List<int>> variantByDefinition = [];

    /// <summary>The declared array types: an array can be assigned to one of another element type through array covariance.</summary>
    private readonly List<int> arrays = [];

    /// <summary>The catalogs that may hold an object of every type.</summary>
    private readonly ulong[] universal;

    /// <summary>The catalogs found for each type asked about, in catalog order.</summary>
    private readonly Dictionary<Type, DefinitionCatalog[]> found = [];

    /// <summary>Reads which declared types each of <paramref name="catalogs"/> reaches, among <paramref name="types"/> of <paramref name="game"/>.</summary>
    /// <param name="game">The game's assembly.</param>
    /// <param name="types">Every type the game's assembly declares.</param>
    /// <param name="catalogs">The catalogs, in their prefixes' ordinal order.</param>
    internal DefinitionExposure(Assembly game, IReadOnlyList<Type> types, IReadOnlyList<DefinitionCatalog> catalogs)
    {
        this.game = Ensure.NotNull(game);
        this.catalogs = Ensure.NotNull(catalogs);
        universal = new ulong[(catalogs.Count + WordBits - 1) / WordBits];
        foreach (Type type in Ensure.NotNull(types))
            FileDerived(type);
        var pending = new Stack<int>();
        int[] visited = [];
        for (int catalog = 0; catalog < catalogs.Count; catalog++)
        {
            // Types are numbered as they are first reached, so the marks of the types this walk has visited grow with them.
            pending.Clear();
            foreach (FieldInfo member in catalogs[catalog].Members)
                pending.Push(Number(member.FieldType));
            while (pending.Count > 0)
            {
                int number = pending.Pop();
                if (visited.Length <= number)
                    Array.Resize(ref visited, Math.Max(number + 1, visited.Length * 2));
                if (visited[number] == catalog + 1)
                    continue;
                visited[number] = catalog + 1;
                (reaching[number] ??= new ulong[universal.Length])[catalog / WordBits] |= 1UL << (catalog % WordBits);
                if (anything[number])
                    universal[catalog / WordBits] |= 1UL << (catalog % WordBits);
                foreach (int next in Held(number))
                    pending.Push(next);
            }
        }
    }

    /// <summary>
    /// The catalogs that may hold an object of <paramref name="type"/> where a game can reach it, in catalog order: those
    /// whose members reach a location declared with a type <paramref name="type"/> is assignable to.
    /// </summary>
    internal IReadOnlyList<DefinitionCatalog> Candidates(Type type)
    {
        Ensure.NotNull(type);
        if (found.TryGetValue(type, out DefinitionCatalog[]? known))
            return known;
        ulong[] set = [.. universal];
        // A class, and every interface the type implements, takes it as it is; a generic interface or delegate of the
        // same definition, or an array of another element type, may take it through variance.
        for (Type? current = type; current is not null; current = current.BaseType)
            IncludeAssignable(set, current, type);
        foreach (Type implemented in type.GetInterfaces())
            IncludeAssignable(set, implemented, type);
        if (type.IsArray)
            foreach (int number in arrays)
                if (declared[number].IsAssignableFrom(type))
                    Include(set, number);
        var candidates = new List<DefinitionCatalog>();
        for (int catalog = 0; catalog < catalogs.Count; catalog++)
            if ((set[catalog / WordBits] & (1UL << (catalog % WordBits))) != 0)
                candidates.Add(catalogs[catalog]);
        DefinitionCatalog[] result = [.. candidates];
        found.Add(type, result);
        return result;
    }

    /// <summary>
    /// Adds to <paramref name="set"/> the catalogs reaching <paramref name="assignable"/>, a class <paramref name="type"/>
    /// derives from or an interface it implements, and those reaching a declared type of its generic definition that
    /// <paramref name="type"/> is assignable to.
    /// </summary>
    private void IncludeAssignable(ulong[] set, Type assignable, Type type)
    {
        if (numbers.TryGetValue(assignable, out int number))
            Include(set, number);
        if (!assignable.IsGenericType || !variantByDefinition.TryGetValue(assignable.GetGenericTypeDefinition(), out List<int>? related))
            return;
        foreach (int candidate in related)
            if (declared[candidate].IsAssignableFrom(type))
                Include(set, candidate);
    }

    /// <summary>Adds the catalogs reaching declared type <paramref name="number"/> to <paramref name="set"/>.</summary>
    private void Include(ulong[] set, int number)
    {
        if (reaching[number] is not { } catalogsReaching)
            return;
        for (int word = 0; word < set.Length; word++)
            set[word] |= catalogsReaching[word];
    }

    /// <summary>Files <paramref name="type"/>, when it is concrete, under every class it derives from and every interface it implements.</summary>
    private void FileDerived(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
            return;
        for (Type? baseType = type.BaseType; baseType is not null && baseType != typeof(object); baseType = baseType.BaseType)
            File(type, baseType);
        foreach (Type implemented in type.GetInterfaces())
            File(type, implemented);
    }

    /// <summary>Files <paramref name="type"/> under <paramref name="declaredBase"/>, a constructed type under its generic definition.</summary>
    private void File(Type type, Type declaredBase)
    {
        Type key = declaredBase.IsGenericType ? declaredBase.GetGenericTypeDefinition() : declaredBase;
        if (!derived.TryGetValue(key, out List<(Type Derived, Type Declared)>? filed))
            derived.Add(key, filed = []);
        filed.Add((type, declaredBase));
    }

    /// <summary>The number of declared type <paramref name="type"/>, numbering it on first sight.</summary>
    private int Number(Type type)
    {
        if (numbers.TryGetValue(type, out int number))
            return number;
        number = declared.Count;
        numbers.Add(type, number);
        declared.Add(type);
        held.Add(null);
        anything.Add(HoldsAnything(type));
        reaching.Add(null);
        if (type.IsArray)
        {
            arrays.Add(number);
        }
        else if (type.IsGenericType && (type.IsInterface || type.IsSubclassOf(typeof(Delegate))))
        {
            Type definition = type.GetGenericTypeDefinition();
            if (!variantByDefinition.TryGetValue(definition, out List<int>? related))
                variantByDefinition.Add(definition, related = []);
            related.Add(number);
        }
        return number;
    }

    /// <summary>The numbers of the declared types a location declared with type <paramref name="number"/> holds, read once.</summary>
    private int[] Held(int number)
    {
        if (held[number] is { } known)
            return known;
        var types = new HashSet<Type>();
        AddHeld(declared[number], types);
        int[] numbered = [.. types.Select(Number)];
        held[number] = numbered;
        return numbered;
    }

    /// <summary>Whether a location declared as <paramref name="type"/> can hold an object of any type.</summary>
    private bool HoldsAnything(Type type) =>
        type == typeof(object) || type == typeof(ValueType) ||
        (type.Assembly != game && !type.IsValueType && type != typeof(string) &&
         typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && Enumerated(type).Count == 0);

    /// <summary>Adds to <paramref name="types"/> the declared types a location declared as <paramref name="type"/> holds.</summary>
    private void AddHeld(Type type, HashSet<Type> types)
    {
        if (type.IsPointer || type.IsByRef || type.IsByRefLike || type.ContainsGenericParameters || type.IsPrimitive || type.IsEnum ||
            type == typeof(string) || HoldsAnything(type) || type.IsSubclassOf(typeof(Delegate)) || typeof(MemberInfo).IsAssignableFrom(type) ||
            type == typeof(Pointer))
            return;
        if (type.IsArray)
        {
            types.Add(Ensure.NotNull(type.GetElementType()));
            return;
        }
        if (type.Assembly == game || type.IsValueType)
        {
            for (Type? current = type; current is not null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(InstanceFields))
                    if (!field.FieldType.IsPointer && !field.FieldType.IsByRef)
                        types.Add(field.FieldType);
        }
        else
        {
            AddExposed(type, types);
        }
        if (!type.IsValueType && !type.IsSealed)
            AddDerived(type, types);
    }

    /// <summary>
    /// Adds what another assembly's class or interface exposes: its type arguments, comparers of them, arrays of them
    /// where it is an interface an array implements, and the elements it enumerates.
    /// </summary>
    private static void AddExposed(Type type, HashSet<Type> types)
    {
        if (type.IsGenericType)
        {
            foreach (Type argument in type.GetGenericArguments())
            {
                types.Add(argument);
                if (argument.IsPointer || argument.IsByRef || argument.IsByRefLike)
                    continue;
                types.Add(typeof(IEqualityComparer<>).MakeGenericType(argument));
                types.Add(typeof(IComparer<>).MakeGenericType(argument));
                if (type.IsInterface && type.IsAssignableFrom(argument.MakeArrayType()))
                    types.Add(argument.MakeArrayType());
            }
        }
        foreach (Type element in Enumerated(type))
            types.Add(element);
    }

    /// <summary>The element types <paramref name="type"/> enumerates as <see cref="IEnumerable{T}"/>.</summary>
    private static List<Type> Enumerated(Type type)
    {
        var elements = new List<Type>();
        foreach (Type implemented in type.IsInterface ? [type, .. type.GetInterfaces()] : type.GetInterfaces())
            if (implemented.IsGenericType && implemented.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                elements.Add(implemented.GetGenericArguments()[0]);
        return elements;
    }

    /// <summary>Adds each of the game's types that derives from or implements <paramref name="type"/>, a generic one constructed to do so.</summary>
    private void AddDerived(Type type, HashSet<Type> types)
    {
        Type key = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        if (!derived.TryGetValue(key, out List<(Type Derived, Type Declared)>? filed))
            return;
        foreach ((Type candidate, Type declaredBase) in filed)
        {
            if (!candidate.ContainsGenericParameters)
            {
                if (type.IsAssignableFrom(candidate))
                    types.Add(candidate);
                continue;
            }
            if (Constructed(candidate, declaredBase, type) is { } constructed)
                types.Add(constructed);
        }
    }

    /// <summary>
    /// <paramref name="definition"/> constructed so that its <paramref name="declaredBase"/>, written in its own type
    /// parameters, is <paramref name="type"/>; null when no type arguments do so or they break its constraints.
    /// </summary>
    private static Type? Constructed(Type definition, Type declaredBase, Type type)
    {
        Type[] parameters = definition.GetGenericArguments();
        var arguments = new Type?[parameters.Length];
        if (!Unify(declaredBase, type, definition, arguments))
            return null;
        if (arguments.Any(argument => argument is null))
            return null;
        Type[] bound = [.. arguments.Select(argument => argument!)];
        for (int index = 0; index < parameters.Length; index++)
            if (!Satisfies(parameters[index], bound[index], definition, bound))
                return null;
        Type constructed = definition.MakeGenericType(bound);
        return type.IsAssignableFrom(constructed) ? constructed : null;
    }

    /// <summary>
    /// Matches <paramref name="pattern"/>, written in <paramref name="definition"/>'s type parameters, against the closed
    /// <paramref name="type"/>, binding each parameter it meets into <paramref name="arguments"/>.
    /// </summary>
    private static bool Unify(Type pattern, Type type, Type definition, Type?[] arguments)
    {
        if (pattern.IsGenericParameter)
        {
            if (pattern.DeclaringType != definition || pattern.DeclaringMethod is not null)
                return false;
            ref Type? argument = ref arguments[pattern.GenericParameterPosition];
            if (argument is null)
                argument = type;
            return argument == type;
        }
        if (!pattern.ContainsGenericParameters)
            return pattern == type;
        if (pattern.IsArray)
            return type.IsArray && pattern.GetArrayRank() == type.GetArrayRank() &&
                Unify(Ensure.NotNull(pattern.GetElementType()), Ensure.NotNull(type.GetElementType()), definition, arguments);
        if (!pattern.IsGenericType || !type.IsGenericType || pattern.GetGenericTypeDefinition() != type.GetGenericTypeDefinition())
            return false;
        Type[] patternArguments = pattern.GetGenericArguments(), typeArguments = type.GetGenericArguments();
        for (int index = 0; index < patternArguments.Length; index++)
            if (!Unify(patternArguments[index], typeArguments[index], definition, arguments))
                return false;
        return true;
    }

    /// <summary>Whether <paramref name="argument"/> meets the constraints of <paramref name="parameter"/> with every parameter of <paramref name="definition"/> bound to <paramref name="bound"/>.</summary>
    private static bool Satisfies(Type parameter, Type argument, Type definition, Type[] bound)
    {
        GenericParameterAttributes special = parameter.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
        if (special.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && argument.IsValueType)
            return false;
        if (special.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
            (!argument.IsValueType || Nullable.GetUnderlyingType(argument) is not null))
            return false;
        if (special.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !argument.IsValueType &&
            (argument.IsAbstract || argument.GetConstructor(Type.EmptyTypes) is null))
            return false;
        if (argument.IsByRefLike && !special.HasFlag(GenericParameterAttributes.AllowByRefLike))
            return false;
        if (parameter.GetCustomAttributesData().Any(attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsUnmanagedAttribute") &&
            ContainsReferences(argument))
            return false;
        foreach (Type constraint in parameter.GetGenericParameterConstraints())
            if (!Substituted(constraint, definition, bound).IsAssignableFrom(argument))
                return false;
        return true;
    }

    /// <summary><paramref name="type"/> with every type parameter of <paramref name="definition"/> replaced by <paramref name="bound"/>.</summary>
    private static Type Substituted(Type type, Type definition, Type[] bound)
    {
        if (type.IsGenericParameter && type.DeclaringType == definition && type.DeclaringMethod is null)
            return bound[type.GenericParameterPosition];
        if (!type.ContainsGenericParameters)
            return type;
        if (type.IsArray)
        {
            Type element = Substituted(Ensure.NotNull(type.GetElementType()), definition, bound);
            return type.IsSZArray ? element.MakeArrayType() : element.MakeArrayType(type.GetArrayRank());
        }
        return type.GetGenericTypeDefinition().MakeGenericType([.. type.GetGenericArguments().Select(argument => Substituted(argument, definition, bound))]);
    }

    /// <summary>Whether values of <paramref name="type"/> can hold a reference, as an unmanaged constraint forbids.</summary>
    private static bool ContainsReferences(Type type) =>
        (typeof(System.Runtime.CompilerServices.RuntimeHelpers)
            .GetMethod(nameof(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences))
            ?? throw new InvalidOperationException("The runtime cannot tell which types hold references."))
            .MakeGenericMethod(type).Invoke(null, null) is true;
}
