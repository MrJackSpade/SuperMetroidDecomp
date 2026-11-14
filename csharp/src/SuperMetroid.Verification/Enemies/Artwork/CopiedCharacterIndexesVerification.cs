using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks the enemies the import composes at the character index of the enemy they follow draw in their pre-coloured
/// sheets' standard colours (#1392, #1512). Where a sheet is composed is the import's alone (#1513): a slot carries no
/// character index, so the copy itself is no longer state to check.
/// </summary>
internal static class CopiedCharacterIndexesVerification
{
    /// <summary>
    /// In the first retail room state placing the enemy each such enemy follows, every slot of it draws, from
    /// the room's first enemy frame, in the colours its graphics set composes its sheet in.
    /// </summary>
    internal static void VerifyCopiedCharacterIndexes()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        int slots = 0;
        foreach (EnemyDefinitionId child in SuperMetroid.AssetExtraction.Enemies.EnemyCharacterBaseDefinitions.DrawsAtTilesOf.Keys)
        {
            // A room places the enemy each follows; Phantoon's parts are spawned by her body rather than placed.
            EnemyDefinitionId owner = SuperMetroid.AssetExtraction.Enemies.EnemyCharacterBaseDefinitions.DrawsAtTilesOf[child];
            (RoomDefinition room, RoomStateSettings state) = RetailRooms.All
                .SelectMany(definition => RoomStateLists.GetStates(definition).Select(state => KeyValuePair.Create(definition, state)))
                .FirstOrDefault(candidate => candidate.Value.EnemyPopulation.Placements.ToArray().Any(record => record.DefinitionId == owner));
            if (room is null)
                throw new InvalidOperationException($"No retail room state places {owner}.");
            runtime.LoadCartridgeRoomForDebug(room, state: state);
            // Some copy on their first main-AI frame, which runs before the room's enemies first draw.
            runtime.Enemies.StepStandaloneFrame(runtime.Enemies.StandaloneFrame(runtime.Room.LoadedLevelData) with { Camera = default, TimeIsFrozen = false, Samus = runtime.Player.Samus });
            RoomEnemySlot[] placed = [.. runtime.Enemies.SlotTable.Slots.Where(slot => slot.DefinitionId == child)];
            AssertTrue(placed.Length > 0, $"{room.Name} places {child}");
            foreach (RoomEnemySlot slot in placed)
            {
                // The import colours the sheet of an enemy copying another's colours in its owner's row (#1512), so the copy
                // draws in its sheet's standard colours.
                if (SuperMetroid.AssetExtraction.Enemies.EnemySheetRowDefinitions.DrawsInRowOf.ContainsKey(child))
                    AssertEqual(PictureShading.Standard, slot.Shading,
                        $"{child} in {room.Name} draws in its sheet's standard colours");
                slots++;
            }
        }
        Console.WriteLine($"Copied character indexes: {slots} slots of {SuperMetroid.AssetExtraction.Enemies.EnemyCharacterBaseDefinitions.DrawsAtTilesOf.Count} enemies draw in their sheets' colours.");
    }

    /// <summary>Runs the copied character index check.</summary>
    [VerificationCommand("--copied-character-indexes")]
    internal static int CopiedCharacterIndexesCommand()
    {
        Suite(nameof(VerifyCopiedCharacterIndexes), VerifyCopiedCharacterIndexes);
        return 0;
    }
}
