namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The installed key of each <see cref="FileCopyArrowShape"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class FileCopyArrowShapeKeys
{
    /// <summary>Each FileCopyArrowShape member's installed key.</summary>
    internal static InstalledKeys<FileCopyArrowShape> Installed { get; } = new(new Dictionary<FileCopyArrowShape, string>
    {
        [FileCopyArrowShape.ShortDown] = "ShortDown",
        [FileCopyArrowShape.ShortUp] = "ShortUp",
        [FileCopyArrowShape.LongDown] = "LongDown",
        [FileCopyArrowShape.LongUp] = "LongUp",
    });
}
