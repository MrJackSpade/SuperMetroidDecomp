namespace SuperMetroid.Core;

/// <summary>
/// One field of a type as a game snapshot names it (#2602): the type that declares it and its name. A snapshot keeps each
/// field's name before its value, and a restore checks every name against this build's, so a snapshot whose layout this
/// build does not have is refused rather than misread.
/// </summary>
/// <param name="declaringType">The name of the type that declares the field.</param>
/// <param name="name">The field's name: a field's, a property's for the value it keeps, or a constructor parameter's.</param>
public sealed partial class StateGraphField(string declaringType, string name)
{
    /// <summary>The name of the type that declares the field.</summary>
    public string DeclaringType { get; } = Ensure.NotNull(declaringType);

    /// <summary>The field's name.</summary>
    public string Name { get; } = Ensure.NotNull(name);

    /// <inheritdoc/>
    public override string ToString() => $"{DeclaringType}::{Name}";
}
