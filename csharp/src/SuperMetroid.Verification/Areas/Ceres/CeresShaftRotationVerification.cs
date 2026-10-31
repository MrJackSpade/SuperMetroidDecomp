using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Areas.Ceres;
/// <summary>Checks the Ceres elevator shaft's timed Mode 7 matrix publications against native rotation records.</summary>
internal static class CeresShaftRotationVerification
{
    /// <summary>Compares held frames and 548 matrix changes across four forward-and-reverse sweeps.</summary>
    internal static void VerifyCeresShaftCompiledRotation()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Numeric coefficients/phases were proven by the #1165 research; this check covers
        // actual per-frame publication only.
        var carryover = new RoomMainCarryover();
        var state = new CeresElevatorShaftRoomMainState(active: true, carryover);
        ushort phaseWord = 34, timer = 60;
        int updates = 0;
        // Four full sweeps: compare every held frame as well as every matrix
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
            AssertEqual(changed, actual.MatrixChanged, "shaft matrix publication frame");
            AssertEqual(timer, state.RotationTimer.RawValue, "shaft native timer every frame");
            if (changed)
            {
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 4))), actual.Transform.MatrixA, "shaft native cosine every publication");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 2))), actual.Transform.MatrixB, "shaft native sine every publication");
                AssertEqual(unchecked((short)-rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 2))), actual.Transform.MatrixC, "shaft native negative sine");
                phaseWord = (short)phaseWord < 0
                    ? phaseWord == 0x8001 ? (ushort)0 : (ushort)(phaseWord - 1)
                    : phaseWord == 67 ? (ushort)0x8044 : (ushort)(phaseWord + 1);
                updates++;
            }
            AssertEqual(phaseWord & 127, state.RotationStep, "shaft turn step every frame");
            AssertEqual(phaseWord >= 32768 ? CeresShaftSweep.Backward : CeresShaftSweep.Forward, state.Sweep, "shaft sweep direction every frame");
            AssertEqual(phaseWord, carryover.Countdown.RawValue, "shaft turn step the following room inherits, as the native phase word");
        }
    }
}
