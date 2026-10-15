using System.Reflection;

namespace SuperMetroid.Tooling;

/// <summary>Verification access to <see cref="RequiredReflection"/> lookups development tools do not use.</summary>
internal static class RequiredReflectionAccess
{
    /// <summary>The public method named <paramref name="name"/>.</summary>
    public static MethodInfo GetRequiredMethod(this Type type, string name) =>
        type.GetMethod(name) ?? throw new MissingMethodException(type.FullName, name);
    /// <summary>The public field named <paramref name="name"/>.</summary>
    public static FieldInfo GetRequiredField(this Type type, string name) =>
        type.GetField(name) ?? throw new MissingFieldException(type.FullName, name);
    /// <summary>The public property named <paramref name="name"/>.</summary>
    public static PropertyInfo GetRequiredProperty(this Type type, string name) =>
        type.GetProperty(name) ?? throw new MissingMemberException(type.FullName, name);
}
