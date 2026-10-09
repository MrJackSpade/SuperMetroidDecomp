namespace SuperMetroid.Core.Hardware;

/// <summary>Separates a cartridge dispatch failure's stable identity from its occurrence coordinates.</summary>
public sealed class CartridgeDispatchException : InvalidOperationException
{
    /// <summary>Gets the stable native routine, instruction, or pointer identity that could not be dispatched.</summary>
    public string DispatchIdentity { get; }
    /// <summary>Gets whether the failure is independent of the active room and its occurrence coordinates.</summary>
    public bool IsRoomIndependent { get; }

    /// <summary>Creates a dispatch failure with a stable diagnostic identity and scope classification.</summary>
    public CartridgeDispatchException(string dispatchIdentity, string message, bool isRoomIndependent,
        Exception? innerException = null) : base(message, innerException)
    {
        DispatchIdentity = dispatchIdentity;
        IsRoomIndependent = isRoomIndependent;
    }
}
