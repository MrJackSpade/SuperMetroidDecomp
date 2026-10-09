using System.Reflection;

/// <summary>
/// Reads private production state for verification. Production assemblies carry no test-only
/// accessors; tests that observe internal state name it here, in one place.
/// </summary>
internal static class PrivateState
{
    /// <summary>Reflection flags that include public and nonpublic instance/static members declared at each type level.</summary>
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>Reads a named instance field, searching base classes when it is not declared on the runtime type.</summary>
    /// <param name="owner">Object whose field value is read.</param>
    /// <param name="name">Field name to resolve.</param>
    internal static T Field<T>(object owner, string name) => (T)FieldInfo(owner.GetType(), name).GetValue(owner)!;

    /// <summary>Reads a named static field from the requested type or one of its base classes.</summary>
    /// <param name="type">Type whose static field is read.</param>
    /// <param name="name">Field name to resolve.</param>
    internal static T StaticField<T>(Type type, string name) => (T)FieldInfo(type, name).GetValue(null)!;

    /// <summary>Sets an instance field after converting compatible primitive constants to its declared type.</summary>
    /// <param name="owner">Object whose field value is changed.</param>
    /// <param name="name">Field name to resolve.</param>
    /// <param name="value">Value assigned after primitive conversion when needed.</param>
    internal static void SetField(object owner, string name, object? value)
    {
        FieldInfo field = FieldInfo(owner.GetType(), name);
        field.SetValue(owner, Assignable(field.FieldType, value));
    }

    /// <summary>Sets a static field after converting compatible primitive constants to its declared type.</summary>
    /// <param name="type">Type whose static field is changed.</param>
    /// <param name="name">Field name to resolve.</param>
    /// <param name="value">Value assigned after primitive conversion when needed.</param>
    internal static void SetStaticField(Type type, string name, object? value)
    {
        FieldInfo field = FieldInfo(type, name);
        field.SetValue(null, Assignable(field.FieldType, value));
    }

    /// <summary>Reads a named instance property by invoking its getter without wrapping accessor exceptions.</summary>
    /// <param name="owner">Object whose property value is read.</param>
    /// <param name="name">Property name to resolve.</param>
    internal static T Property<T>(object owner, string name) => (T)Get(PropertyInfo(owner.GetType(), name), owner)!;

    /// <summary>Reads a named static property by invoking its getter without wrapping accessor exceptions.</summary>
    /// <param name="type">Type whose static property is read.</param>
    /// <param name="name">Property name to resolve.</param>
    internal static T StaticProperty<T>(Type type, string name) => (T)Get(PropertyInfo(type, name), null)!;

    /// <summary>Invokes an instance property's setter with the value converted to its declared primitive type when applicable.</summary>
    /// <param name="owner">Object whose property is changed.</param>
    /// <param name="name">Property name to resolve.</param>
    /// <param name="value">Value passed to the setter.</param>
    internal static void SetProperty(object owner, string name, object? value)
    {
        PropertyInfo property = PropertyInfo(owner.GetType(), name);
        Set(property, owner, Assignable(property.PropertyType, value));
    }

    /// <summary>Invokes a static property's setter with the value converted to its declared primitive type when applicable.</summary>
    /// <param name="type">Type whose static property is changed.</param>
    /// <param name="name">Property name to resolve.</param>
    /// <param name="value">Value passed to the setter.</param>
    internal static void SetStaticProperty(Type type, string name, object? value)
    {
        PropertyInfo property = PropertyInfo(type, name);
        Set(property, null, Assignable(property.PropertyType, value));
    }

    /// <summary>
    /// The value a C# assignment would store: a numeric constant such as <c>1</c> converts to the
    /// member's primitive type, checked, exactly as the compiler converted it in the source.
    /// </summary>
    private static object? Assignable(Type memberType, object? value) =>
        value is null || memberType.IsInstanceOfType(value) || !memberType.IsPrimitive || value is not IConvertible
            ? value
            : Convert.ChangeType(value, memberType, System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Invokes the unique instance overload that accepts the supplied arguments, including omitted optional tails.</summary>
    /// <param name="owner">Object on which the method is invoked.</param>
    /// <param name="name">Method name to resolve.</param>
    /// <param name="arguments">Supplied arguments used for overload matching and invocation.</param>
    internal static object? Invoke(object owner, string name, params object?[] arguments) =>
        Call(MethodInfo(owner.GetType(), name, arguments), owner, arguments);

    /// <summary>Invokes the unique static overload that accepts the supplied arguments.</summary>
    /// <param name="type">Type that declares the static method.</param>
    /// <param name="name">Method name to resolve.</param>
    /// <param name="arguments">Supplied arguments used for overload matching and invocation.</param>
    internal static object? InvokeStatic(Type type, string name, params object?[] arguments) =>
        Call(MethodInfo(type, name, arguments), null, arguments);

    /// <summary>Invokes a method with out parameters; their results replace the matching <paramref name="arguments"/>.</summary>
    internal static object? InvokeWithOut(object owner, string name, object?[] arguments) =>
        Call(MethodInfo(owner.GetType(), name, arguments), owner, arguments);

    /// <summary>Invokes a static method with out parameters; their results replace the matching <paramref name="arguments"/>.</summary>
    internal static object? InvokeStaticWithOut(Type type, string name, object?[] arguments) =>
        Call(MethodInfo(type, name, arguments), null, arguments);

    /// <summary>Typed getter delegate for a reflected span property, avoiding boxing of its ref-struct result.</summary>
    private delegate ReadOnlySpan<T> SpanGetter<T>();

    /// <summary>A private static span property, read through a typed delegate because spans cannot be boxed.</summary>
    internal static ReadOnlySpan<T> StaticSpanProperty<T>(Type type, string name) =>
        PropertyInfo(type, name).GetMethod!.CreateDelegate<SpanGetter<T>>()();

    /// <summary>Constructs <paramref name="type"/> through the one constructor accepting <paramref name="arguments"/>.</summary>
    internal static object Construct(Type type, params object?[] arguments)
    {
        ConstructorInfo[] candidates = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(constructor => Accepts(constructor.GetParameters(), arguments)).ToArray();
        return candidates.Length == 1 ? Construct(candidates[0], arguments)
            : throw new MissingMethodException($"{type.FullName} has {candidates.Length} constructors accepting {arguments.Length} arguments.");
    }

    /// <summary>
    /// An instance with no constructor run, equivalent to a removed empty constructor on a type
    /// without instance field initializers.
    /// </summary>
    internal static T Uninitialized<T>() where T : class =>
        (T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(T));

    /// <summary>A private nested type of <paramref name="outer"/>.</summary>
    internal static Type Nested(Type outer, string name) =>
        outer.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new TypeLoadException($"{outer.FullName} declares no nested type {name}.");

    /// <summary>A non-public top-level type of a repository assembly.</summary>
    internal static Type TypeNamed(string fullName) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).FirstOrDefault(t => t is not null)
            ?? throw new TypeLoadException($"No loaded assembly declares {fullName}.");

    // Members throw exactly what a direct call throws, never a TargetInvocationException wrapper.
    /// <summary>Invokes a selected method or constructor with optional-tail defaults and unwrapped target exceptions.</summary>
    /// <param name="method">Reflection member selected by the matching helper.</param>
    /// <param name="owner">Instance receiver, or null for static members.</param>
    /// <param name="arguments">Arguments to pass; matching out values are copied back into this array.</param>
    private static object? Call(MethodBase method, object? owner, object?[] arguments)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (arguments.Length == parameters.Length)
            return method.Invoke(owner, BindingFlags.DoNotWrapExceptions, null, arguments, null);
        // Omitted trailing optional parameters take their declared defaults, as at a C# call site.
        object?[] complete = [.. arguments, .. parameters.Skip(arguments.Length).Select(parameter => parameter.DefaultValue)];
        object? result = method.Invoke(owner, BindingFlags.DoNotWrapExceptions, null, complete, null);
        Array.Copy(complete, arguments, arguments.Length);
        return result;
    }

    /// <summary>Constructs an instance with the selected constructor, filling omitted optional parameters.</summary>
    /// <param name="constructor">Constructor selected after matching the supplied arguments.</param>
    /// <param name="arguments">Provided constructor arguments.</param>
    private static object Construct(ConstructorInfo constructor, object?[] arguments)
    {
        ParameterInfo[] parameters = constructor.GetParameters();
        object?[] complete = [.. arguments, .. parameters.Skip(arguments.Length).Select(parameter => parameter.DefaultValue)];
        return constructor.Invoke(BindingFlags.DoNotWrapExceptions, null, complete, null);
    }

    /// <summary>Invokes a reflected property getter through the common exception-unwrapping call path.</summary>
    /// <param name="property">Property whose getter is invoked.</param>
    /// <param name="owner">Instance receiver, or null for a static getter.</param>
    private static object? Get(PropertyInfo property, object? owner) =>
        Call(property.GetMethod ?? throw new MissingMethodException(property.DeclaringType?.FullName, property.Name + ".get"), owner, []);

    /// <summary>Invokes a reflected property setter through the common exception-unwrapping call path.</summary>
    /// <param name="property">Property whose setter is invoked.</param>
    /// <param name="owner">Instance receiver, or null for a static setter.</param>
    /// <param name="value">Value assigned by the setter.</param>
    private static void Set(PropertyInfo property, object? owner, object? value) =>
        Call(property.SetMethod ?? throw new MissingMethodException(property.DeclaringType?.FullName, property.Name + ".set"), owner, [value]);

    /// <summary>Finds a field by name while walking from the requested type through its base classes.</summary>
    /// <param name="type">Starting type for the declared-member search.</param>
    /// <param name="name">Field name to find.</param>
    /// <exception cref="MissingFieldException">No matching field is declared in the inheritance chain.</exception>
    private static FieldInfo FieldInfo(Type type, string name)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetField(name, Members) is { } field)
                return field;
        throw new MissingFieldException(type.FullName, name);
    }

    /// <summary>Finds a property by name while walking from the requested type through its base classes.</summary>
    /// <param name="type">Starting type for the declared-member search.</param>
    /// <param name="name">Property name to find.</param>
    /// <exception cref="MissingMemberException">No matching property is declared in the inheritance chain.</exception>
    private static PropertyInfo PropertyInfo(Type type, string name)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetProperty(name, Members) is { } property)
                return property;
        throw new MissingMemberException(type.FullName, name);
    }

    /// <summary>The one method named <paramref name="name"/> whose parameters accept <paramref name="arguments"/>.</summary>
    private static MethodInfo MethodInfo(Type type, string name, object?[] arguments)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            MethodInfo[] candidates = current.GetMethods(Members)
                .Where(method => method.Name == name && Accepts(method.GetParameters(), arguments)).ToArray();
            if (candidates.Length == 1)
                return candidates[0];
            if (candidates.Length > 1)
                throw new AmbiguousMatchException(
                    $"{current.FullName}.{name} has {candidates.Length} overloads accepting {arguments.Length} arguments.");
        }
        throw new MissingMethodException(type.FullName, name);
    }

    /// <summary>Tests whether supplied arguments fit a parameter list, including optional trailing parameters.</summary>
    /// <param name="parameters">Candidate method or constructor parameter list.</param>
    /// <param name="arguments">Arguments supplied by the verification caller.</param>
    private static bool Accepts(ParameterInfo[] parameters, object?[] arguments) =>
        arguments.Length <= parameters.Length &&
        parameters.Skip(arguments.Length).All(parameter => parameter.IsOptional) &&
        parameters.Zip(arguments).All(pair => Accepts(pair.First, pair.Second));

    /// <summary>Checks whether one argument can be passed to a parameter, accounting for null and by-reference types.</summary>
    /// <param name="parameter">Candidate parameter metadata.</param>
    /// <param name="argument">Supplied value, or null for a null/out argument.</param>
    private static bool Accepts(ParameterInfo parameter, object? argument)
    {
        if (parameter.IsOut)
            return argument is null;
        Type type = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType()! : parameter.ParameterType;
        return argument is null
            ? !type.IsValueType || Nullable.GetUnderlyingType(type) is not null
            : type.IsInstanceOfType(argument);
    }
}
