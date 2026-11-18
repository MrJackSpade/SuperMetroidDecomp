namespace SuperMetroid.Core.Menus;

/// <summary>A button a controller row assigns, three records below the compared menu row.</summary>
public abstract record ControllerButton
{
    /// <summary>Closes the hierarchy to the button declared here.</summary>
    private ControllerButton()
    {
    }

    /// <summary>A button named by its number.</summary>
    /// <param name="Number">The number, which no code names: it is the only value telling two rows apart.</param>
    public sealed record Numbered(int Number) : ControllerButton;
}
