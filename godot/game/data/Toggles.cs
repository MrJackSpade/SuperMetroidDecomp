namespace Game.Data;

/// <summary>The names of this game's own settings that are on or off, as the options hold them.</summary>
public static class Toggles
{
    /// <summary>Walking backwards while firing is allowed.</summary>
    public const string Moonwalk = "moonwalk";

    /// <summary>The selected item stays selected until cancelled, instead of cancelling itself.</summary>
    public const string ManualItemCancel = "manual_item_cancel";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [Moonwalk, ManualItemCancel];
}
