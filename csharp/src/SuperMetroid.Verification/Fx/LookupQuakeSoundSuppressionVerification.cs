using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks the complete native room set that suppresses statue earthquake sound.</summary>
internal static class LookupQuakeSoundSuppressionVerification
{
    /// <summary>Matches six ROM room comparisons and checks suppression through room FX initialization.</summary>
    internal static void VerifyQuakeSoundSuppression(CartridgeImportAddressSpace rom)
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
        foreach (RoomDefinition room in RetailRooms.All)
            AssertEqual(nativeRooms.Contains(RoomDefinitions.PointerOf(room)),
                room.QuakesAreSilent,
                "Quake suppression complete room domain");

        var state = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.RoomFxOverlayState>([typeof(global::SuperMetroid.Core.Audio.GameplaySoundRequests)], new object?[] { SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone() });
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        foreach (RoomDefinition? room in nativeRooms.Select(pointer => RoomDefinitions.Get(pointer)).Append(null))
        {
            var suppressedSounds = SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone();
            state = RoomFxOverlayState.ForRoom(fx: null, door: null, randomNumber: 0,
                room: room, sounds: suppressedSounds);
            state.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame);
            AssertEqual(room is not null ? 0 : 1, suppressedSounds.SoundRequests.Count,
                "Actual room initialization and quake emission honor native suppression");
            var resetSounds = SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone();
            state = RoomFxOverlayState.ForRoom(fx: null, door: null, randomNumber: 0,
                room: null, sounds: resetSounds);
            state.PublishStatueEarthquakeSound(0, SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame);
            AssertEqual(1, resetSounds.SoundRequests.Count, "Unsuppressed room resets previous sentinel");
        }
    }

    /// <summary>Reads a cartridge opcode or operand for the quake-suppression comparison chain.</summary>
    private static byte QuakeSoundSuppressionRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
