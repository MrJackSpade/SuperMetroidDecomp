using System.Reflection;

/// <summary>
/// Reads private production state for verification. Production assemblies carry no test-only
/// accessors; tests that observe internal state name it here, in one place.
/// </summary>
internal static class PrivateState
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    internal static T Field<T>(object owner, string name) => (T)FieldInfo(owner.GetType(), name).GetValue(owner)!;

    internal static T StaticField<T>(Type type, string name) => (T)FieldInfo(type, name).GetValue(null)!;

    internal static void SetField(object owner, string name, object? value)
    {
        FieldInfo field = FieldInfo(owner.GetType(), name);
        field.SetValue(owner, Assignable(field.FieldType, value));
    }

    internal static void SetStaticField(Type type, string name, object? value)
    {
        FieldInfo field = FieldInfo(type, name);
        field.SetValue(null, Assignable(field.FieldType, value));
    }

    internal static T Property<T>(object owner, string name) => (T)Get(PropertyInfo(owner.GetType(), name), owner)!;

    internal static T StaticProperty<T>(Type type, string name) => (T)Get(PropertyInfo(type, name), null)!;

    internal static void SetProperty(object owner, string name, object? value)
    {
        PropertyInfo property = PropertyInfo(owner.GetType(), name);
        Set(property, owner, Assignable(property.PropertyType, value));
    }

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

    internal static object? Invoke(object owner, string name, params object?[] arguments) =>
        Call(MethodInfo(owner.GetType(), name, arguments), owner, arguments);

    internal static object? InvokeStatic(Type type, string name, params object?[] arguments) =>
        Call(MethodInfo(type, name, arguments), null, arguments);

    /// <summary>Invokes a method with out parameters; their results replace the matching <paramref name="arguments"/>.</summary>
    internal static object? InvokeWithOut(object owner, string name, object?[] arguments) =>
        Call(MethodInfo(owner.GetType(), name, arguments), owner, arguments);

    /// <summary>Invokes a static method with out parameters; their results replace the matching <paramref name="arguments"/>.</summary>
    internal static object? InvokeStaticWithOut(Type type, string name, object?[] arguments) =>
        Call(MethodInfo(type, name, arguments), null, arguments);

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

    private static object Construct(ConstructorInfo constructor, object?[] arguments)
    {
        ParameterInfo[] parameters = constructor.GetParameters();
        object?[] complete = [.. arguments, .. parameters.Skip(arguments.Length).Select(parameter => parameter.DefaultValue)];
        return constructor.Invoke(BindingFlags.DoNotWrapExceptions, null, complete, null);
    }

    private static object? Get(PropertyInfo property, object? owner) =>
        Call(property.GetMethod ?? throw new MissingMethodException(property.DeclaringType?.FullName, property.Name + ".get"), owner, []);

    private static void Set(PropertyInfo property, object? owner, object? value) =>
        Call(property.SetMethod ?? throw new MissingMethodException(property.DeclaringType?.FullName, property.Name + ".set"), owner, [value]);

    private static FieldInfo FieldInfo(Type type, string name)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetField(name, Members) is { } field)
                return field;
        throw new MissingFieldException(type.FullName, name);
    }

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

    private static bool Accepts(ParameterInfo[] parameters, object?[] arguments) =>
        arguments.Length <= parameters.Length &&
        parameters.Skip(arguments.Length).All(parameter => parameter.IsOptional) &&
        parameters.Zip(arguments).All(pair => Accepts(pair.First, pair.Second));

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
