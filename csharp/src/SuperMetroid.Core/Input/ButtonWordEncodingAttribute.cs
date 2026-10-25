namespace SuperMetroid.Core.Input;

/// <summary>
/// Marks a declaration that encodes held buttons as a stored button word, or decodes one (#1306, #1420): a recording
/// or save codec, an enemy variable that keeps a word, or the validated conversion itself. Elsewhere controller state
/// stays <see cref="ControllerButton"/>, and SME6284 rejects a conversion.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor |
    AttributeTargets.Property, Inherited = false)]
public sealed class ButtonWordEncodingAttribute : Attribute
{
    /// <summary>Records why this declaration owns stored button words.</summary>
    public ButtonWordEncodingAttribute(string reason) => Reason = reason;

    /// <summary>Which button word this declaration stores or reads, and why.</summary>
    public string Reason { get; }
}
