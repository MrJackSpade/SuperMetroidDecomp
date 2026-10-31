using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks the complete native room set that suppresses statue earthquake sound.</summary>
internal static class LookupQuakeSoundSuppressionVerification
{
    /// <summary>Matches six ROM room comparisons and checks suppression through room FX initialization.</summary>
    internal static void VerifyQuakeSoundSuppression(SuperMetroidAddressSpace rom)
    {
        var nativeRooms = new HashSet<ushort>();
        for (int instruction = 0x8882cd; instruction <= 0x8882e6; instruction += 5)
        {
            AssertEqual((byte)0xc9, QuakeSoundSuppressionRomByte(rom, instruction), "Native CMP immediate");
            AssertEqual((byte)0xf0, QuakeSoundSuppressionRomByte(rom, instruction + 3), "Native BEQ suppression");
            int branchTarget = instruction + 5 + unchecked((sbyte)QuakeSoundSuppressionRomByte(rom, instruction + 4));
            AssertEqual(0x8882ed, branchTarget, "All native room matches share suppression branch");
            nativeRooms.Add(ReadVerificationWord(rom, instruction + 1));
        }
        AssertEqual(6, nativeRooms.Count, "Six distinct native suppressed rooms");
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
            AssertEqual(nativeRooms.Contains(RoomHeaderDefinitions.PointerOf(room)),
                RoomEarthquakeDefinitions.SoundSuppressedRooms.Contains(room),
                "Quake suppression complete room domain");

        var stateSession = new RoomFxSession();
        var state = new RoomLayer3FxState(stateSession);
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new GameplayColours(new SpriteColourSets());
        foreach (RoomHeaderDefinition? room in nativeRooms.Select(pointer => RoomHeaderDefinitions.Get(pointer)).Append(null))
        {
            state = RoomLayer3FxState.ForRoom(stateSession, cgram.BackgroundSets, fx: null, door: null, randomNumber: 0,
                room: room);
            state.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame);
            AssertEqual(room is not null ? 0 : 1, state.SoundRequests.Count,
                "Actual room initialization and quake emission honor native suppression");
            state = RoomLayer3FxState.ForRoom(stateSession, cgram.BackgroundSets, fx: null, door: null, randomNumber: 0,
                room: null);
            state.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame);
            AssertEqual(1, state.SoundRequests.Count, "Unsuppressed room resets previous sentinel");
        }
    }

    /// <summary>Reads a cartridge opcode or operand for the quake-suppression comparison chain.</summary>
    private static byte QuakeSoundSuppressionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
