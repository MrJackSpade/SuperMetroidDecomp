namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The installed key of each <see cref="GameOptionsPrimaryRow"/> (#2520, #2565), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class GameOptionsPrimaryRowKeys
{
    /// <summary>Each GameOptionsPrimaryRow member's installed key.</summary>
    internal static InstalledKeys<GameOptionsPrimaryRow> Installed { get; } = new(new Dictionary<GameOptionsPrimaryRow, string>
    {
        [GameOptionsPrimaryRow.StartGame] = "StartGame",
        [GameOptionsPrimaryRow.EnglishText] = "EnglishText",
        [GameOptionsPrimaryRow.JapaneseText] = "JapaneseText",
        [GameOptionsPrimaryRow.ControllerSettings] = "ControllerSettings",
        [GameOptionsPrimaryRow.SpecialSettings] = "SpecialSettings",
    });
}
