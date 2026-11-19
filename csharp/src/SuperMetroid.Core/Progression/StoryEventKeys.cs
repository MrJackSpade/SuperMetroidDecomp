namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="StoryEvent"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class StoryEventKeys
{
    /// <summary>Each StoryEvent member's installed key.</summary>
    internal static InstalledKeys<StoryEvent> Installed { get; } = new(new Dictionary<StoryEvent, string>
    {
        [StoryEvent.ZebesAwake] = "ZebesAwake",
        [StoryEvent.ShitroidAteSidehopper] = "ShitroidAteSidehopper",
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
        [StoryEvent.ShitroidEncounterMusicSkipped] = "ShitroidEncounterMusicSkipped",
        [StoryEvent.OutranSpeedBoosterLavaquake] = "OutranSpeedBoosterLavaquake",
    });
}
