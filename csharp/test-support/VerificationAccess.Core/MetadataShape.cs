using SuperMetroid.Core;
using System.Reflection;

/// <summary>
/// Declared shape of a type, for audits that assert what a type stores, such as "stock data keeps no stored
/// color words" or "this cache field no longer exists". It answers questions about declarations only and
/// never reads, writes or invokes a member.
/// </summary>
[ReflectionAudit("metadata shape: member presence and declared member types, never member values")]
internal static class MetadataShape
{
    /// <summary>Whether <paramref name="type"/> declares or inherits a field named <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    internal static bool DeclaresField(Type type, string name, BindingFlags flags) => type.GetField(name, flags) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a field named <paramref name="name"/> visible to the default lookup.</summary>
    internal static bool DeclaresField(Type type, string name) => type.GetField(name) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a property named <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    internal static bool DeclaresProperty(Type type, string name, BindingFlags flags) => type.GetProperty(name, flags) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a property named <paramref name="name"/> visible to the default lookup.</summary>
    internal static bool DeclaresProperty(Type type, string name) => type.GetProperty(name) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a method named <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    internal static bool DeclaresMethod(Type type, string name, BindingFlags flags) => type.GetMethod(name, flags) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a public method <paramref name="name"/> taking exactly <paramref name="parameterTypes"/>.</summary>
    internal static bool DeclaresMethod(Type type, string name, Type[] parameterTypes) => type.GetMethod(name, parameterTypes) is not null;

    /// <summary>Whether <paramref name="type"/> declares or inherits a method named <paramref name="name"/> visible to the default lookup.</summary>
    internal static bool DeclaresMethod(Type type, string name) => type.GetMethod(name) is not null;

    /// <summary>The declared parameter types of <paramref name="type"/>'s single method <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    /// <exception cref="MissingMethodException">No such method is declared.</exception>
    internal static Type[] ParameterTypes(Type type, string name, BindingFlags flags) =>
        type.GetMethod(name, flags) is { } method ? [.. method.GetParameters().Select(parameter => parameter.ParameterType)]
            : throw new MissingMethodException(type.FullName, name);

    /// <summary>The declared parameter types of <paramref name="type"/>'s single public method <paramref name="name"/>.</summary>
    /// <exception cref="MissingMethodException">No such method is declared.</exception>
    internal static Type[] ParameterTypes(Type type, string name) =>
        ParameterTypes(type, name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

    /// <summary>The declared type of <paramref name="type"/>'s field <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    /// <exception cref="MissingFieldException">No such field is declared.</exception>
    internal static Type FieldType(Type type, string name, BindingFlags flags) =>
        type.GetField(name, flags)?.FieldType ?? throw new MissingFieldException(type.FullName, name);

    /// <summary>The compile-time literal of <paramref name="type"/>'s constant <paramref name="name"/> visible to <paramref name="flags"/>.</summary>
    /// <exception cref="MissingFieldException">No such constant is declared.</exception>
    internal static object ConstantValue(Type type, string name, BindingFlags flags) =>
        type.GetField(name, flags) is { IsLiteral: true } constant
            ? constant.GetRawConstantValue() ?? throw new InvalidDataException($"{type.FullName}.{name} has no literal value.")
            : throw new MissingFieldException(type.FullName, name);

    /// <summary>The fields <paramref name="type"/> declares or inherits that are visible to <paramref name="flags"/>.</summary>
    internal static FieldInfo[] Fields(Type type, BindingFlags flags) => type.GetFields(flags);

    /// <summary>The properties <paramref name="type"/> declares or inherits that are visible to <paramref name="flags"/>.</summary>
    internal static PropertyInfo[] Properties(Type type, BindingFlags flags) => type.GetProperties(flags);

    /// <summary>The nested types <paramref name="type"/> declares, filtered by <paramref name="flags"/>.</summary>
    internal static Type[] NestedTypes(Type type, BindingFlags flags) => type.GetNestedTypes(flags);

    /// <summary>Whether <paramref name="assembly"/> declares a type with the given full name.</summary>
    internal static bool AssemblyDeclaresType(Assembly assembly, string fullName) => assembly.GetType(fullName) is not null;

    /// <summary>The methods <paramref name="type"/> declares or inherits that are visible to <paramref name="flags"/>.</summary>
    internal static MethodInfo[] Methods(Type type, BindingFlags flags) => type.GetMethods(flags);
}
