namespace System.Diagnostics.CodeAnalysis;

/// <summary>Nullable-flow annotation netstandard2.0 does not ship: the parameter is non-null when the method returns the given value.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>Marks the parameter; the compiler reads the argument from the attribute's metadata, so nothing stores it.</summary>
    /// <param name="_">The return value for which the parameter is non-null.</param>
    public NotNullWhenAttribute(bool _)
    {
    }
}
