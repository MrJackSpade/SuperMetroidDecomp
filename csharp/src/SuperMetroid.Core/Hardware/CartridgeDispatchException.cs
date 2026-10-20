namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Separates a cartridge dispatch failure's stable identity from its occurrence coordinates. The identity names the
/// untranslated routine, instruction or pointer, so the failure is the same bug in every room it occurs in.
/// </summary>
public sealed class CartridgeDispatchException : InvalidOperationException
{
    /// <summary>Gets the stable native routine, instruction, or pointer identity that could not be dispatched.</summary>
    public string DispatchIdentity { get; }

    /// <summary>Creates a dispatch failure with a stable diagnostic identity.</summary>
    public CartridgeDispatchException(string dispatchIdentity, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DispatchIdentity = dispatchIdentity;
    }
}
