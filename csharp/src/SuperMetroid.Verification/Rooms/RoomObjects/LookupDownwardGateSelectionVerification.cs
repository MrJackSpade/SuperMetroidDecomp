using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks the eight admitted downward-gate arguments against three native selection tables.</summary>
internal static class LookupDownwardGateSelectionVerification
{
    /// <summary>Compares selected instruction lists and rejects every unsupported word argument.</summary>
    internal static void VerifyDownwardGateListSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c70a, 0));

    /// <summary>Compares the left draw-block word for each admitted gate argument.</summary>
    internal static void VerifyDownwardGateLeftSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c71a, 1));

    /// <summary>Compares the right draw-block word for each admitted gate argument.</summary>
    internal static void VerifyDownwardGateRightSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c72a, 2));

    /// <summary>Scans all 16-bit arguments, accepting only the eight even selectors and comparing their native field.</summary>
    private static void VerifyDownwardGateSelectionField(CartridgeImportAddressSpace rom, int source, int field)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort argument = (ushort)raw;
            if (raw is not (0 or 2 or 4 or 6 or 8 or 10 or 12 or 14))
            {
                AssertThrows<InvalidDataException>(() => GateObjects.DownwardGateShotBlock(
                    RoomObjectRoomArgumentWords.Decode(RoomObjectKind.DownwardGateShotBlock, argument) is GateTriggerArgument { Trigger: var trigger }
                        ? trigger : throw new InvalidDataException($"Argument ${argument:X4} names no trigger.")),
                    "Downward gate rejects all unsupported arguments");
                continue;
            }
            DownwardGateShotBlockDefinition action = GateObjects.DownwardGateShotBlock(argument / 2);
            ushort native = ReadSamusEaterObjectWord(rom, source + raw);
            if (field == 0)
            {
                // The trigger's native list draws its shot block once and ends, as its typed change does (#1554).
                NativeTerrainChanges.Compare(CartridgeImportSource.Require(rom), action.Change, native, NativeTerrainChanges.NoSteps,
                    $"Downward gate argument {raw}", out bool began);
                AssertTrue(!began, $"Downward gate argument {raw} trigger opens nothing");
                continue;
            }
            // The cartridge stores zero for a side that is left alone. Its other words give the side's block a collision
            // kind (the high nibble) and a behaviour (the low byte) and keep what the block shows (#1580).
            RoomCollisionType? nativeCollision = native == 0 ? null : RoomLevelWordCodec.Decode(native).Collision;
            AssertEqual(nativeCollision, field == 1 ? action.LeftCollision : action.RightCollision,
                $"Downward gate original field {field}, argument {raw} collision");
            if (native != 0)
                AssertEqual(native & 0xff,
                    RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(BlockReactionDefinitions.DownwardGateTriggers.ReactionFor(action.Trigger))),
                    $"Downward gate original field {field}, argument {raw} behaviour");
        }
    }
}
