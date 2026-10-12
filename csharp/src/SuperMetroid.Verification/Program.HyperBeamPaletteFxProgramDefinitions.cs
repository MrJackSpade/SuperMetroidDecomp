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
using static SamusDamageAndGrabVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class HyperBeamPaletteFxProgramDefinitionsVerification
{
    internal static void VerifyHyperBeamPaletteFxProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(SamusPaletteRomData.HyperBeamFx.SetColorIndex,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress),
            "Hyper Beam palette-FX entry command");
        AssertEqual(SamusPaletteRomData.HyperBeamFx.DestinationByteIndex,
            CartridgeImportSource.Require(rom).ReadCartridgeWord((HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress + 2)),
            "Hyper Beam palette-FX destination");

        for (int index = 0;
             index < HyperBeamPaletteFxProgramDefinitions.FrameCount;
             index++)
        {
            int timerAddress =
                HyperBeamPaletteFxProgramDefinitions.NativeFirstFrameTimerAddress +
                index * HyperBeamPaletteFxProgramDefinitions.FrameByteCount;
            AssertEqual(HyperBeamPaletteFxProgramDefinitions.FrameDuration,
                CartridgeImportSource.Require(rom).ReadCartridgeWord(timerAddress),
                $"Hyper Beam palette-FX frame {index} duration");
            AssertEqual(SamusPaletteRomData.HyperBeamFx.Done,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((timerAddress +
                    HyperBeamPaletteFxProgramDefinitions.FrameByteCount - sizeof(ushort))),
                $"Hyper Beam palette-FX frame {index} terminator");

            ushort pointer = unchecked((ushort)(
                HyperBeamPaletteFxProgramDefinitions.FirstFramePointer +
                index * HyperBeamPaletteFxProgramDefinitions.FrameByteCount));
            HyperBeamPaletteFxFrame frame =
                HyperBeamPaletteFxProgramDefinitions.ResolveFrame(pointer, out bool frameLooped);
            AssertTrue(!frameLooped, $"Hyper Beam palette-FX frame {index} is not loop entry");
            AssertEqual(index, frame.Index, $"Hyper Beam palette-FX frame {index} index");
            AssertEqual(HyperBeamPaletteFxProgramDefinitions.FrameDuration,
                frame.Duration, $"Hyper Beam palette-FX frame {index} compiled duration");
        }

        AssertEqual(SamusPaletteRomData.HyperBeamFx.Goto,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress),
            "Hyper Beam palette-FX loop command");
        AssertEqual(HyperBeamPaletteFxProgramDefinitions.FirstFramePointer,
            CartridgeImportSource.Require(rom).ReadCartridgeWord((HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress + 2)),
            "Hyper Beam palette-FX loop target");

        HyperBeamPaletteFxFrame entry =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer,
                out bool entryLooped);
        AssertTrue(!entryLooped && entry.Index == 0,
            "Hyper Beam palette-FX entry resolves to frame zero without a completed loop");
        HyperBeamPaletteFxFrame loop =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                HyperBeamPaletteFxProgramDefinitions.LoopInstructionPointer,
                out bool looped);
        AssertTrue(looped && loop.Index == 0,
            "Hyper Beam palette-FX terminal command resolves to frame zero and publishes a loop");
        AssertThrows<InvalidDataException>(
            () => HyperBeamPaletteFxProgramDefinitions.ResolveFrame(0xd905, out _),
            "Hyper Beam palette-FX rejects an unaligned restored pointer");
        AssertThrows<InvalidDataException>(
            () => HyperBeamPaletteFxProgramDefinitions.ResolveFrame(0xd9d0, out _),
            "Hyper Beam palette-FX rejects a post-program restored pointer");

        Console.WriteLine(
            "Hyper Beam palette-FX control: entry, ten timers/terminators, loop, and " +
            "restored-pointer domain match the cartridge.");
    }

    internal static CartridgeReadGuard NewHyperBeamPaletteFxControlGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(IsHyperBeamPaletteFxControlByte,
            address => $"Hyper Beam palette-FX attempted control read ${address:X6}.");

    private static bool IsHyperBeamPaletteFxControlByte(int address)
    {
        if (address >= HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress &&
            address < HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress + 4)
        {
            return true;
        }

        if (address >= HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress &&
            address < HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress + 4)
        {
            return true;
        }

        int relative = address -
            HyperBeamPaletteFxProgramDefinitions.NativeFirstFrameTimerAddress;
        if (relative is < 0 or
            >= (HyperBeamPaletteFxProgramDefinitions.FrameCount *
                HyperBeamPaletteFxProgramDefinitions.FrameByteCount))
        {
            return false;
        }

        int inFrame = relative % HyperBeamPaletteFxProgramDefinitions.FrameByteCount;
        return inFrame is < sizeof(ushort) or
            >= (HyperBeamPaletteFxProgramDefinitions.FrameByteCount - sizeof(ushort));
    }

    [VerificationCommand("--hyper-beam-palette-fx-program-definitions")]
    internal static int HyperBeamPaletteFxProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyHyperBeamPaletteFxProgramDefinitions), () => VerifyHyperBeamPaletteFxProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
        return 0;
    }
}
