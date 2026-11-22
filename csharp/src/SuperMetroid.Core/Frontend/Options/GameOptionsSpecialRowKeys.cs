namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The installed key of each <see cref="GameOptionsSpecialRow"/> (#2520, #2565), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class GameOptionsSpecialRowKeys
{
    /// <summary>Each GameOptionsSpecialRow member's installed key.</summary>
    internal static InstalledKeys<GameOptionsSpecialRow> Installed { get; } = new(new Dictionary<GameOptionsSpecialRow, string>
    {
        [GameOptionsSpecialRow.IconCancel] = "IconCancel",
        [GameOptionsSpecialRow.Moonwalk] = "Moonwalk",
        [GameOptionsSpecialRow.Exit] = "Exit",
    });
}
