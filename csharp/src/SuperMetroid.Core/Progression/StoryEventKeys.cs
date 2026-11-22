namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="StoryEvent"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class StoryEventKeys
{
    /// <summary>Each StoryEvent member's installed key.</summary>
    internal static InstalledKeys<StoryEvent> Installed { get; } = new(new Dictionary<StoryEvent, string>
    {
        [StoryEvent.ZebesAwake] = "ZebesAwake",
        [StoryEvent.BabyMetroidAteSidehopper] = "ShitroidAteSidehopper",
        [StoryEvent.MotherBrainGlassDestroyed] = "MotherBrainGlassDestroyed",
        [StoryEvent.PhantoonStatueGrey] = "PhantoonStatueGrey",
        [StoryEvent.RidleyStatueGrey] = "RidleyStatueGrey",
        [StoryEvent.DraygonStatueGrey] = "DraygonStatueGrey",
        [StoryEvent.KraidStatueGrey] = "KraidStatueGrey",
        [StoryEvent.TourianUnlocked] = "TourianUnlocked",
        [StoryEvent.MaridiaNoobTubeBroken] = "MaridiaNoobTubeBroken",
        [StoryEvent.LowerNorfairChozoLoweredAcid] = "LowerNorfairChozoLoweredAcid",
        [StoryEvent.ShaktoolClearedPath] = "ShaktoolClearedPath",
        [StoryEvent.ZebesTimebombSet] = "ZebesTimebombSet",
        [StoryEvent.CrittersEscaped] = "CrittersEscaped",
        [StoryEvent.FirstMetroidHallCleared] = "FirstMetroidHallCleared",
        [StoryEvent.FirstMetroidShaftCleared] = "FirstMetroidShaftCleared",
        [StoryEvent.SecondMetroidHallCleared] = "SecondMetroidHallCleared",
        [StoryEvent.SecondMetroidShaftCleared] = "SecondMetroidShaftCleared",
        [StoryEvent.BabyMetroidEncounterMusicSkipped] = "ShitroidEncounterMusicSkipped",
        [StoryEvent.OutranSpeedBoosterLavaquake] = "OutranSpeedBoosterLavaquake",
    });

    /// <summary>
    /// The order a save lists the story events that have happened (#2543), written out beside the keys so reordering
    /// StoryEvent's members never changes the bytes of a save the game writes. A save is read in any order.
    /// </summary>
    internal static IReadOnlyList<StoryEvent> SavedOrder { get; } =
    [
        StoryEvent.ZebesAwake,
        StoryEvent.BabyMetroidAteSidehopper,
        StoryEvent.MotherBrainGlassDestroyed,
        StoryEvent.PhantoonStatueGrey,
        StoryEvent.RidleyStatueGrey,
        StoryEvent.DraygonStatueGrey,
        StoryEvent.KraidStatueGrey,
        StoryEvent.TourianUnlocked,
        StoryEvent.MaridiaNoobTubeBroken,
        StoryEvent.LowerNorfairChozoLoweredAcid,
        StoryEvent.ShaktoolClearedPath,
        StoryEvent.ZebesTimebombSet,
        StoryEvent.CrittersEscaped,
        StoryEvent.FirstMetroidHallCleared,
        StoryEvent.FirstMetroidShaftCleared,
        StoryEvent.SecondMetroidHallCleared,
        StoryEvent.SecondMetroidShaftCleared,
        StoryEvent.BabyMetroidEncounterMusicSkipped,
        StoryEvent.OutranSpeedBoosterLavaquake,
    ];
}
