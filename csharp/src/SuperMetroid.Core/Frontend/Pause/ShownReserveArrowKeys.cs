namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The installed key of each <see cref="ShownReserveArrow"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ShownReserveArrowKeys
{
    /// <summary>Each ShownReserveArrow member's installed key.</summary>
    internal static InstalledKeys<ShownReserveArrow> Installed { get; } = new(new Dictionary<ShownReserveArrow, string>
    {
        [ShownReserveArrow.NoReserves] = "NoReserves",
        [ShownReserveArrow.Lit] = "Lit",
        [ShownReserveArrow.Dim] = "Dim",
    });
}
