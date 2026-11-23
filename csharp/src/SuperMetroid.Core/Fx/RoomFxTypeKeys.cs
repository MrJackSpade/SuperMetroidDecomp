namespace SuperMetroid.Core.Fx;

/// <summary>
/// The installed key of each <see cref="RoomFxType"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class RoomFxTypeKeys
{
    /// <summary>Each RoomFxType member's installed key.</summary>
    internal static InstalledKeys<RoomFxType> Installed { get; } = new(new Dictionary<RoomFxType, string>
    {
        [RoomFxType.None] = "None",
        [RoomFxType.Lava] = "Lava",
        [RoomFxType.Acid] = "Acid",
        [RoomFxType.Water] = "Water",
        [RoomFxType.Spores] = "Spores",
        [RoomFxType.Rain] = "Rain",
        [RoomFxType.Fog] = "Fog",
        [RoomFxType.ScrollingSky] = "ScrollingSky",
        [RoomFxType.Fireflea] = "Fireflea",
        [RoomFxType.TourianEntranceStatue] = "TourianEntranceStatue",
        [RoomFxType.CeresRidley] = "CeresRidley",
        [RoomFxType.CeresElevator] = "CeresElevator",
        [RoomFxType.CeresHaze] = "CeresHaze",
    });
}
