using System.Reflection;

namespace SuperMetroid.Tooling;

/// <summary>Cartridge values and helpers of <see cref="RequiredReflection"/> that only verification reads.</summary>
internal static class RequiredReflectionConstants
{
    /// <summary>The value of a constant field, which must be a <typeparamref name="T"/>.</summary>
    public static T RequiredConstant<T>(this FieldInfo field) => field.GetRawConstantValue() is T value
        ? value
        : throw new InvalidOperationException($"{field.DeclaringType?.FullName}.{field.Name} is not a {typeof(T).Name} constant.");
}
