namespace SuperMetroid.Core.Hardware;

/// <summary>Separates a cartridge dispatch failure's stable identity from its occurrence coordinates.</summary>
public sealed class CartridgeDispatchException : InvalidOperationException
{
    public string DispatchIdentity { get; }
    public bool IsRoomIndependent { get; }

    public CartridgeDispatchException(string dispatchIdentity, string message, bool isRoomIndependent,
        Exception? innerException = null) : base(message, innerException)
    {
        DispatchIdentity = dispatchIdentity;
        IsRoomIndependent = isRoomIndependent;
    }
}
