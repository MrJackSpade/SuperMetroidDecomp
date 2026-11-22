namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The installed key of each <see cref="TitleCard"/>, written out so renaming a member never changes a name the
/// installation or an installed document spells it by.
/// </summary>
internal static partial class TitleCardKeys
{
    /// <summary>Each TitleCard member's installed key.</summary>
    internal static InstalledKeys<TitleCard> Installed { get; } = new(new Dictionary<TitleCard, string>
    {
        [TitleCard.Year] = "Year",
        [TitleCard.Nintendo] = "Nintendo",
        [TitleCard.Presents] = "Presents",
        [TitleCard.MetroidThree] = "MetroidThree",
    });
}
