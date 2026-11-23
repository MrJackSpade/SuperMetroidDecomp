namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What the room effect's overlay shows and where (#1514, #1706): a liquid below its surface (<see cref="LiquidOverlay"/>)
/// or an atmosphere's repeating picture (<see cref="AtmosphereOverlay"/>). Every picture repeats across at its own width,
/// moved line by line by the overlay's horizontal scroll; the backend draws the description.
/// </summary>
public abstract partial record OverlayPicture
{
    /// <summary>Closes the hierarchy to the liquid and the atmosphere.</summary>
    private protected OverlayPicture()
    {
    }
}
