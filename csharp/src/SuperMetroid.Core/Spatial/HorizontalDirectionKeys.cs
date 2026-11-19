namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The installed key of each <see cref="HorizontalDirection"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class HorizontalDirectionKeys
{
    /// <summary>Each HorizontalDirection member's installed key.</summary>
    internal static InstalledKeys<HorizontalDirection> Installed { get; } = new(new Dictionary<HorizontalDirection, string>
    {
        [HorizontalDirection.Left] = "Left",
        [HorizontalDirection.Right] = "Right",
    });
}
