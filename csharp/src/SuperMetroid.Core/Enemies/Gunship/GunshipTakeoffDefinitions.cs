using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's takeoff (#1406, #1538): how long it readies its engines, and the look they show once they fire up.</summary>
internal static class GunshipTakeoffDefinitions
{
    /// <summary>
    /// The updates the takeoff spends readying the engines before they fire up and the rumble begins: the escape takeoff's
    /// readying phase runs on five updates, the last of which hands over to the rumble, as the original game's takeoff does.
    /// </summary>
    internal const int EngineReadyingDuration = 5;

    /// <summary>The gunship's look once its engines are readied for the takeoff, which its hull and the takeoff's dust show.</summary>
    internal static EncounterLook TakeoffLook { get; } = new(EncounterArtwork.Gunship, "Takeoff");
}
