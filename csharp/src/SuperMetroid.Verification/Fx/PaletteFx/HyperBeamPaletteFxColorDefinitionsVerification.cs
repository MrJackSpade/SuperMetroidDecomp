using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx.PaletteFx;
/// <summary>Checks the compiled Hyper Beam palette cycle against its cartridge control words and restored-pointer domain.</summary>
internal static class HyperBeamPaletteFxColorDefinitionsVerification
{

    /// <summary>Rejects runtime reads of palette-FX control words that have compiled definitions.</summary>
    internal static CartridgeReadGuard NewHyperBeamPaletteFxControlGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(IsHyperBeamPaletteFxControlByte,
            address => $"Hyper Beam palette-FX attempted control read ${address:X6}.");

    /// <summary>Recognizes entry and loop commands plus each frame's timer and terminator, excluding color payloads.</summary>
    private static bool IsHyperBeamPaletteFxControlByte(int address)
    {
        if (address >= HyperBeamPaletteFxColorDefinitions.NativeEntryControlAddress &&
            address < HyperBeamPaletteFxColorDefinitions.NativeEntryControlAddress + 4)
        {
            return true;
        }

        if (address >= HyperBeamPaletteFxColorDefinitions.NativeLoopControlAddress &&
            address < HyperBeamPaletteFxColorDefinitions.NativeLoopControlAddress + 4)
        {
            return true;
        }

        int relative = address -
            HyperBeamPaletteFxColorDefinitions.NativeFirstFrameTimerAddress;
        if (relative is < 0 or
            >= (HyperBeamPaletteFxColorDefinitionsConstants.FrameCount *
                HyperBeamPaletteFxColorDefinitionsConstants.FrameByteCount))
        {
            return false;
        }

        int inFrame = relative % HyperBeamPaletteFxColorDefinitionsConstants.FrameByteCount;
        return inFrame is < sizeof(ushort) or
            >= (HyperBeamPaletteFxColorDefinitionsConstants.FrameByteCount - sizeof(ushort));
    }
}
