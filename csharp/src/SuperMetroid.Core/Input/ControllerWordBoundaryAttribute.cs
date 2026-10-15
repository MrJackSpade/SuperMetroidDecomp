namespace SuperMetroid.Core.Input;

/// <summary>
/// Marks a declaration that stores or reads controller words (#1306): a journal or SRAM codec, an enemy variable
/// that keeps a word, or the validated conversion itself. Elsewhere controller state stays <see cref="SnesButton"/>,
/// and SME6284 rejects a conversion.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
internal sealed class ControllerWordBoundaryAttribute : Attribute
{
    /// <summary>Records why this declaration owns raw controller words.</summary>
    public ControllerWordBoundaryAttribute(string reason) => Reason = reason;

    /// <summary>Which word this boundary stores or reads, and why.</summary>
    public string Reason { get; }
}
