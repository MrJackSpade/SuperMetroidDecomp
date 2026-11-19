namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The installed key of each <see cref="ScrollingSky"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class ScrollingSkyKeys
{
    /// <summary>Each ScrollingSky member's installed key.</summary>
    internal static InstalledKeys<ScrollingSky> Installed { get; } = new(new Dictionary<ScrollingSky, string>
    {
        [ScrollingSky.Land] = "Land",
        [ScrollingSky.Ocean] = "Ocean",
    });
}
