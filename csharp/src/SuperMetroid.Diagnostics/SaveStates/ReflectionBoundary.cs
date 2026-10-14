using System.Reflection;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Reflection members whose framework annotations are nullable only for shapes debugger state never serializes
/// (generic parameters, open types, global fields, non-array types). Each validates once and fails with the shape.
/// </summary>
internal static class ReflectionBoundary
{
    /// <summary>The assembly-qualified name of a closed type, which every serialized type is.</summary>
    internal static string AssemblyQualifiedName(Type type) => type.AssemblyQualifiedName ??
        throw new InvalidOperationException($"{type} is a generic parameter or open type and has no assembly-qualified name.");

    /// <summary>The type declaring an instance field; only global module fields lack one.</summary>
    internal static Type DeclaringType(FieldInfo field) => field.DeclaringType ??
        throw new InvalidOperationException($"Field {field.Name} is a global module field with no declaring type.");

    /// <summary>The element type of an array type.</summary>
    internal static Type ElementType(Type arrayType) => arrayType.GetElementType() ??
        throw new InvalidOperationException($"{arrayType} is not an array type.");
}
