namespace SuperMetroid.Core.Menus;

/// <summary>A row of the controller page, which names the button it assigns.</summary>
public abstract record ControllerRow
{
    /// <summary>Closes the hierarchy to the row declared here.</summary>
    private ControllerRow()
    {
    }

    /// <summary>Assigns a button.</summary>
    /// <param name="Button">The button, which no code names.</param>
    public sealed record AssignsButton(ControllerButton Button) : ControllerRow;
}
