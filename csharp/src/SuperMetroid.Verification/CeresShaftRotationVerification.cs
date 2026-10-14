using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms;

/// <summary>Checks the Ceres elevator shaft's timed Mode 7 matrix publications against native rotation records.</summary>
internal static class CeresShaftRotationVerification
{
    /// <summary>Compares held frames and 548 matrix changes across four forward-and-reverse sweeps.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres shaft compiled rotation")]
    internal static void VerifyCeresShaftCompiledRotation()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Numeric coefficients/phases were proven by the #1165 research; this check covers
        // actual per-frame publication only.
        var state = new CeresElevatorShaftRoomMainState();
        var scratch = new RoomMainScratchState();
        state.Reset(active: true, scratch);
        var empty = new TestAddressSpace();
        ushort phaseWord = 34, timer = 60;
        int updates = 0;
        // Four full sweeps: compare every held frame as well as every matrix
        // publication against independently read retail records and word arithmetic.
        while (updates < 548)
        {
            NativeWordCounterStep step = NativeWordCounter.Decrement(timer);
            timer = step.Value;
            bool changed = step.IsNegative;
            int offset = (ushort)(phaseWord * 6);
            if (changed)
                timer = rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + offset);
            var actual = state.Step(empty, null, 0x8000, allowDeparture: false, scratch);
            AssertEqual(changed, actual.MatrixChanged, "shaft matrix publication frame");
            AssertEqual(timer, state.RotationTimer.RawValue, "shaft native timer every frame");
            if (changed)
            {
                AssertEqual(rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 4)), actual.Transform.MatrixA, "shaft native cosine every publication");
                AssertEqual(rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 2)), actual.Transform.MatrixB, "shaft native sine every publication");
                AssertEqual(unchecked((ushort)-rom.ReadCartridgeWord(CeresShaftRotationDefinitions.ReferenceAddress + (offset + 2))), actual.Transform.MatrixC, "shaft native negative sine");
                phaseWord = (short)phaseWord < 0
                    ? phaseWord == 0x8001 ? (ushort)0 : (ushort)(phaseWord - 1)
                    : phaseWord == 67 ? (ushort)0x8044 : (ushort)(phaseWord + 1);
                updates++;
            }
            AssertEqual(phaseWord, scratch.Var1, "shaft forward/reverse native phase");
        }
    }
}
