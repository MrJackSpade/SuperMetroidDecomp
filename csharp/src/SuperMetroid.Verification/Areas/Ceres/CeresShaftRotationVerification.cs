using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Areas.Ceres;
/// <summary>Checks the Ceres elevator shaft's timed tilt publications against native rotation records.</summary>
internal static class CeresShaftRotationVerification
{
    /// <summary>Compares held frames and 548 tilt changes across four forward-and-reverse sweeps.</summary>
    internal static void VerifyCeresShaftCompiledRotation()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Numeric coefficients/phases were proven by the #1165 research; this check covers
        // actual per-frame publication only.
        var carryover = new RoomMainCarryover();
        var state = new CeresElevatorShaftRoomMainState(active: true, carryover);
        ushort phaseWord = 34, timer = 60;
        int updates = 0;
        // Four full sweeps: compare every held frame as well as every tilt
        // publication against independently read retail records and word arithmetic.
        while (updates < 548)
        {
            CountdownStep step = WrappingCounter.Decrement(timer);
            timer = step.Value;
            bool changed = step.IsNegative;
            int offset = (ushort)(phaseWord * 6);
            if (changed)
                timer = rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + offset);
            var actual = state.Step(null, elevatorDoorDestroyed: true, allowDeparture: false, carryover);
            AssertEqual(changed, actual.TiltChanged, "shaft tilt publication frame");
            AssertEqual(timer, state.RotationTimer, "shaft native timer every frame");
            if (changed)
            {
                // The native record holds the tilt's sine and cosine in 256ths: its sine is the tilt in steps, and its
                // cosine is the tilt's cosine to within one 256th.
                int nativeSine = unchecked((short)rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 2)));
                int nativeCosine = unchecked((short)rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 4)));
                AssertEqual(nativeSine, actual.Transform.Angle * CeresShaftRotationDefinitions.StepsPerRadian,
                    "shaft tilt is the native record's sine in steps every publication");
                AssertTrue(Math.Abs(nativeCosine - Math.Cos(actual.Transform.Angle) * 256) < 1,
                    "shaft native cosine is the tilt's cosine every publication");
                phaseWord = (short)phaseWord < 0
                    ? phaseWord == 0x8001 ? (ushort)0 : (ushort)(phaseWord - 1)
                    : phaseWord == 67 ? (ushort)0x8044 : (ushort)(phaseWord + 1);
                updates++;
            }
            AssertEqual(phaseWord & 127, state.TurnPhase, "shaft turn phase every frame");
            AssertEqual(phaseWord >= 32768 ? CeresShaftSweep.Backward : CeresShaftSweep.Forward, state.Sweep, "shaft sweep direction every frame");
            AssertEqual(phaseWord, carryover.Countdown.RawValue, "shaft turn step the following room inherits, as the native phase word");
        }
    }
}
