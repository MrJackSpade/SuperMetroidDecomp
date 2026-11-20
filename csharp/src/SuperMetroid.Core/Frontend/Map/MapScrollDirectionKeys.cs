namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The installed key of each <see cref="MapScrollDirection"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class MapScrollDirectionKeys
{
    /// <summary>Each MapScrollDirection member's installed key.</summary>
    internal static InstalledKeys<MapScrollDirection> Installed { get; } = new(new Dictionary<MapScrollDirection, string>
    {
        [MapScrollDirection.Left] = "Left",
        [MapScrollDirection.Right] = "Right",
        [MapScrollDirection.Up] = "Up",
        [MapScrollDirection.Down] = "Down",
    });
}
