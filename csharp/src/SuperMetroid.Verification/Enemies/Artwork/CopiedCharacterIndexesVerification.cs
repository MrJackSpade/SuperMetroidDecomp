using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks the enemies that copy the character index, or the palette, of the enemy they follow draw where their
/// pre-coloured sheets are composed (#1392): a sheet composed anywhere else cannot be drawn.
/// </summary>
internal static class CopiedCharacterIndexesVerification
{
    /// <summary>
    /// In the first retail room state placing the enemy each such enemy follows, every slot of it draws, from
    /// the room's first enemy frame, at the character index and in the palette its graphics set composes its sheet with.
    /// </summary>
    internal static void VerifyCopiedCharacterIndexes()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        int slots = 0;
        foreach (EnemyDefinitionId child in EnemyTileBaseDefinitions.DrawsAtTilesOf.Keys)
        {
            // A room places the enemy each follows; Phantoon's parts are spawned by her body rather than placed.
            EnemyDefinitionId owner = EnemyTileBaseDefinitions.DrawsAtTilesOf[child];
            (RoomHeaderDefinition room, RoomStateSettings state) = RoomHeaderDefinitions.All
                .SelectMany(definition => RoomStateSelection.GetStates(definition).Select(state => KeyValuePair.Create(definition, state)))
                .FirstOrDefault(candidate => candidate.Value.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == owner));
            if (room is null)
                throw new InvalidOperationException($"No retail room state places {owner}.");
            runtime.LoadCartridgeRoomForDebug(room, state: state);
            // Some copy on their first main-AI frame, which runs before the room's enemies first draw.
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = default, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
            SpriteGraphics composed = runtime.Enemies.GraphicsSet.DrawIndexes(child);
            RoomEnemySlot[] placed = [.. runtime.Enemies.SlotTable.Slots.Where(slot => slot.DefinitionId == child)];
            AssertTrue(placed.Length > 0, $"{room.Name} places {child}");
            foreach (RoomEnemySlot slot in placed)
            {
                AssertEqual(composed.TileBase, slot.TileIndexBase, $"{child} in {room.Name} draws at the character index its sheet is composed at");
                if (EnemyTileBaseDefinitions.DrawsInPaletteOf.ContainsKey(child))
                    AssertEqual(runtime.Enemies.GraphicsSet.DrawRow(child), slot.PaletteRowIn(runtime.Enemies),
                        $"{child} in {room.Name} draws in the row its sheet is coloured in");
                slots++;
            }
        }
        Console.WriteLine($"Copied character indexes: {slots} slots of {EnemyTileBaseDefinitions.DrawsAtTilesOf.Count} enemies draw where their sheets are composed.");
    }

    /// <summary>Runs the copied character index check.</summary>
    [VerificationCommand("--copied-character-indexes")]
    internal static int CopiedCharacterIndexesCommand()
    {
        Suite(nameof(VerifyCopiedCharacterIndexes), VerifyCopiedCharacterIndexes);
        return 0;
    }
}
