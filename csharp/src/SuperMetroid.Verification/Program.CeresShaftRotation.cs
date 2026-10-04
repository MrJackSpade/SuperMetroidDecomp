using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresShaftCompiledRotation()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int offset) => (ushort)(rom.ReadByte(CeresShaftRotationDefinitions.ReferenceAddress + offset) |
            rom.ReadByte(CeresShaftRotationDefinitions.ReferenceAddress + offset + 1) << 8);
        // Numeric coefficient/phase proof is owned by LookupTableResearch's
        // VerifyCeresRotation; this check covers actual per-frame publication only.
        var state = new CeresElevatorShaftRoomMainState();
        state.Reset(active: true);
        var empty = new TestAddressSpace();
        ushort phaseWord = 34, timer = 60;
        int updates = 0;
        // Four full sweeps: compare every held frame as well as every matrix
        // publication against independently read retail records and word arithmetic.
        while (updates < 548)
        {
            timer = unchecked((ushort)(timer - 1));
            bool changed = (short)timer < 0;
            int offset = (ushort)(phaseWord * 6);
            if (changed)
                timer = Word(offset);
            var actual = state.Step(empty, null, 0x8000, allowDeparture: false);
            AssertEqual(changed, actual.MatrixChanged, "shaft matrix publication frame");
            AssertEqual(timer, actual.RotationTimer, "shaft native timer every frame");
            if (changed)
            {
                AssertEqual(Word(offset + 4), actual.Transform.MatrixA, "shaft native cosine every publication");
                AssertEqual(Word(offset + 2), actual.Transform.MatrixB, "shaft native sine every publication");
                AssertEqual(unchecked((ushort)-Word(offset + 2)), actual.Transform.MatrixC, "shaft native negative sine");
                phaseWord = (short)phaseWord < 0
                    ? phaseWord == 0x8001 ? (ushort)0 : (ushort)(phaseWord - 1)
                    : phaseWord == 67 ? (ushort)0x8044 : (ushort)(phaseWord + 1);
                updates++;
            }
            AssertEqual(phaseWord, actual.RotationIndex, "shaft forward/reverse native phase");
        }
    }
}
