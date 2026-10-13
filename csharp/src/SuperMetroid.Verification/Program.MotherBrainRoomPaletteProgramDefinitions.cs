using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Mother Brain's compiled room-flash program against native timing and CGRAM output.</summary>
internal static class MotherBrainRoomPaletteProgramDefinitionsVerification
{
    /// <summary>Compares the 48-frame room flash loop, installed palettes, mechanics words, and read boundaries.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyMotherBrainRoomPaletteProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < MotherBrainRoomPaletteProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainRoomPaletteProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | definition.Address)),
                definition.Value,
                $"Mother Brain room-palette mechanics $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewMotherBrainRoomPaletteReadGuard(rom);
        var enemies = new RoomEnemySystem
        {
            Presentation = { MotherBrainRoomColors = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom))) },
        };
        var output = new SnesCgram();
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = output;
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0])
        {
            RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };

        var expected = new SnesCgram();
        ushort nativePointer = 0xd046;
        ushort nativeTimer = 0;
        var nativeOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 48; frame++)
        {
            ushort duration = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | nativePointer));
            if (nativeTimer == duration)
            {
                nativePointer += 4;
                nativeTimer = 0;
                if (CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | nativePointer)) == 0x9b0f)
                    nativePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | (nativePointer + 2)));
            }
            nativeTimer++;
            nativeOperands.Add((ushort)(nativePointer + 2));
            ushort palette = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | (nativePointer + 2)));
            for (int color = 0; color < 12; color++)
            {
                expected.SetColor(0x34 + color, Bgr555.FromWord(checked((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | (palette + color * 2)))))));
                ushort second = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa90000 | (palette + (12 + color) * 2)));
                expected.SetColor(0x53 + color, Bgr555.FromWord(checked((ushort)(second))));
                expected.SetColor(0x73 + color, Bgr555.FromWord(checked((ushort)(second))));
            }
            enemies.MotherBrainBehavior.RunMotherBrainRoomPalette(state);
            AssertEqual(nativePointer, state.RoomPaletteInstructionPointer, "native room flash pointer and loop");
            AssertEqual(nativeTimer, state.RoomPaletteInstructionTimer, "native room flash two-tick cadence");
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(output)), "native room flash full CGRAM and mirrored colors");
        }
        AssertEqual(14, nativeOperands.Count, "native oracle traverses every room flash operand");

        AssertTrue(state.RoomPaletteInstructionPointer is >= 0xd046 and <= 0xd07e,
            "Mother Brain room-palette program loops within its authored control range");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Mother Brain room-palette execution avoids compiled mechanics bytes");
        AssertEqual(
            0,
            guard.ObservedWords.Count,
            "room flash uses installed colors without runtime presentation reads");

        AssertThrows<InvalidDataException>(
            () => MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(
                MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(0)),
            "Mother Brain palette pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(0xd082),
            "Mother Brain palette payload is outside the control program");

        _ = ProbeMotherBrainRoomPaletteAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainRoomPaletteAllocation();
        AssertTrue(checksum != 0,
            "Mother Brain room-palette allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mother Brain room-palette mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain room-palette mechanics: sixteen control words, fourteen " +
            "installed palettes, native CGRAM/pointer/timer loop, zero presentation reads, strict rejection, " +
            "and allocation-free lookup pass.");
    }

    /// <summary>Repeats warmed room-palette mechanics lookups for the caller's allocation measurement.</summary>
    private static int ProbeMotherBrainRoomPaletteAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(
                MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        }
        return checksum;
    }

    /// <summary>Rejects compiled mechanics rereads and records presentation operands requested at runtime.</summary>
    private static CartridgeReadGuard NewMotherBrainRoomPaletteReadGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source)
            .Forbid(MotherBrainRoomPaletteProgramDefinitions.IsCompiledMechanicsByte,
                address => $"Mother Brain room palette read compiled mechanics ${address:X6}.");
        return guard.Observe(
            address => MotherBrainRoomPaletteProgramDefinitions.TryGetPresentationWord(address, out _),
            address =>
            {
                MotherBrainRoomPaletteProgramDefinitions.TryGetPresentationWord(address, out ushort presentation);
                guard.ObservedWords.Add(presentation);
            });
    }

    /// <summary>Dispatches the Mother Brain room-palette mechanics verification command.</summary>
    [VerificationCommand("--mother-brain-room-palette-mechanics")]
    internal static int MotherBrainRoomPaletteMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainRoomPaletteProgramDefinitions), () => VerifyMotherBrainRoomPaletteProgramDefinitions());
        return 0;
    }
}
