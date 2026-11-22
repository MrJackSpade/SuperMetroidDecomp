namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The installed key of each <see cref="RoomScrollState"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class RoomScrollStateKeys
{
    /// <summary>Each RoomScrollState member's installed key.</summary>
    internal static InstalledKeys<RoomScrollState> Installed { get; } = new(new Dictionary<RoomScrollState, string>
    {
        [RoomScrollState.Closed] = "Closed",
        [RoomScrollState.OpenAlignedToTop] = "OpenAlignedToTop",
        [RoomScrollState.Open] = "Open",
    });
}
