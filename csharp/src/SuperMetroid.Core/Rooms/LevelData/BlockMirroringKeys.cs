namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="BlockMirroring"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class BlockMirroringKeys
{
    /// <summary>Each BlockMirroring member's installed key.</summary>
    internal static InstalledKeys<BlockMirroring> Installed { get; } = new(new Dictionary<BlockMirroring, string>
    {
        [BlockMirroring.None] = "None",
        [BlockMirroring.Horizontal] = "Horizontal",
        [BlockMirroring.Vertical] = "Vertical",
        [BlockMirroring.Both] = "Both",
    });

    /// <summary>The BlockMirroring members in the order an installed document lists them (#2567), written out beside the keys so reordering the members never changes the bytes written.</summary>
    internal static IReadOnlyList<BlockMirroring> ListedOrder { get; } =
    [
        BlockMirroring.None,
        BlockMirroring.Horizontal,
        BlockMirroring.Vertical,
        BlockMirroring.Both,
    ];
}
