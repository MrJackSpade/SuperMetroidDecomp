namespace SuperMetroid.Core.Input;

/// <summary>
/// The routine a demo-input object runs before its record timer on every step. The object's owner interprets it and
/// may redirect the object to another routine and program. Values are identities only; nothing reads them as numbers.
/// </summary>
public enum DemoInputPreInstruction
{
    /// <summary>No routine: the object was cleared and has not been loaded since. A cleared object is disabled and never steps.</summary>
    None,

    /// <summary>The return-only routine: the object does nothing before its timer.</summary>
    ReturnOnly,

    /// <summary>A title demonstration's routine that ends the demonstration's input when the title leaves the demo.</summary>
    AttractCheckLeave,

    /// <summary>
    /// The diagonal-shinespark demonstration's routine: unless Samus is held by Draygon (the cartridge's actual
    /// comparison, not the shinespark its name suggests), it continues the shinespark input and installs
    /// <see cref="AttractCheckLeave"/>.
    /// </summary>
    AttractShinespark,

    /// <summary>The intro Baby-discovery routine that stops Samus running left at her scripted position.</summary>
    IntroBabyRunningLeft,

    /// <summary>The intro Baby-discovery routine that ends the demo once the crossfade finishes.</summary>
    IntroBabyStopAndLook,
}
