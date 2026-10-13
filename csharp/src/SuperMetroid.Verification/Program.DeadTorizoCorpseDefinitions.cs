using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Dead Torizo corpse metadata and its production initializer.</summary>
internal static class DeadTorizoCorpseDefinitionsVerification
{
    /// <summary>Compares the native configuration and wrap offset with the slot state created without ROM metadata reads.</summary>
    internal static void VerifyDeadTorizoCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;

        // Native configuration record $A9:DD58. Word 5 (graphics initializer $DE18) has
        // no port counterpart; the remaining seven words are the definition's fields.
        const int address = 0xa9dd58;
        ushort[] expected = new ushort[7];
        for (int index = 0, word = 0; word < 8; word++)
        {
            if (word == 5)
                continue;
            expected[index++] = CartridgeImportSource.Require(rom).ReadCartridgeWord((address + word * 2));
        }

        ushort[] actual =
        [
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.EntryCount,
            definition.RotationTablePointer,
            definition.FinishFunction,
        ];
        AssertTrue(actual.AsSpan().SequenceEqual(expected),
            "dead Torizo corpse configuration");

        ushort secondRotationOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2))));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            "dead Torizo corpse wrap offset");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureBus(NewDeadTorizoCorpseDefinitionReadGuard(rom));
        Action<RoomEnemySlot> initialize = enemies.DeadTorizoBehavior.InitializeDeadTorizo;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        initialize(slot);

        DeadTorizoEnemyState state = enemies.DeadTorizo!;
        AssertEqual(definition.RottingTablePointer, state.TablePointer,
            "dead Torizo production rotting table");
        AssertEqual(definition.VramTransferPointer, state.VramTablePointer,
            "dead Torizo production VRAM table");
        AssertEqual(definition.CopyFunction, state.CopyFunction,
            "dead Torizo production copy callback");
        AssertEqual(definition.MoveFunction, state.MoveFunction,
            "dead Torizo production move callback");
        AssertEqual(definition.RotationTablePointer, state.RotationTablePointer,
            "dead Torizo production rotation table");
        AssertEqual(definition.FinishFunction, state.FinishFunction,
            "dead Torizo production finish callback");
        AssertEqual(definition.EntryCount, state.EntryCount,
            "dead Torizo production row count");
        AssertEqual(unchecked((ushort)(definition.EntryCount - 1)), state.YLimit,
            "dead Torizo production Y limit");
        AssertEqual(unchecked((ushort)(definition.EntryCount - 2)), state.LateMoveEntryIndex,
            "dead Torizo production late-move row");
        AssertEqual(definition.WrapOffset, state.WrapOffset,
            "dead Torizo production wrap offset");

        Console.WriteLine(
            "Dead Torizo corpse definitions: all eight native configuration words, the derived wrap offset, and the real initializer pass with migrated metadata reads forbidden.");
    }

    /// <summary>Rejects live rereads of the compiled corpse configuration and row-offset word.</summary>
    private static CartridgeReadGuard NewDeadTorizoCorpseDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa9dd58 and < 0xa9dd68 or >= 0xa9e228 and < 0xa9e22a,
                address => $"Dead Torizo attempted migrated corpse metadata read ${address:X6}.");

    /// <summary>Runs the Dead Torizo corpse metadata and initializer checks.</summary>
    [VerificationCommand("--dead-torizo-corpse-definitions")]
    internal static int DeadTorizoCorpseDefinitionsCommand()
    {
        Suite(nameof(VerifyDeadTorizoCorpseDefinitions), () => VerifyDeadTorizoCorpseDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
