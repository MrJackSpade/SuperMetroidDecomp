using System.Reflection;

namespace SuperMetroid.Tooling;

/// <summary>
/// Public-member reflection lookups for members that must exist (#623). Each returns a non-null member or throws the precise
/// missing-member exception naming the declaring type and the requested member, instead of leaving a null that
/// fails later. They reach public members only: a non-public lookup belongs to its audited caller (#622).
/// </summary>
public static class RequiredReflection
{
    /// <summary>The value of a constant field, which must be a <typeparamref name="T"/>.</summary>
    public static T RequiredConstant<T>(this FieldInfo field) => field.GetRawConstantValue() is T value
        ? value
        : throw new InvalidOperationException($"{field.DeclaringType?.FullName}.{field.Name} is not a {typeof(T).Name} constant.");

    /// <summary>The runtime's trusted platform assembly paths, which every .NET host publishes.</summary>
    public static string[] TrustedPlatformAssemblies() =>
        (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ??
            throw new InvalidOperationException("The .NET host published no trusted platform assemblies."))
        .Split(Path.PathSeparator);
}
