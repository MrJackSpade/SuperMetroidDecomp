using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledCeresRidleyGetaway(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort offset = 0; offset <= 224; offset += 2)
        {
            var frame = CeresRidleyGetawayDefinitions.FromByteIndex(offset);
            AssertEqual(Word(0xa6ae4d + offset), frame.Zoom, "Ceres getaway exact native zoom");
            if (offset == 224)
            {
                AssertEqual(CeresRidleyGetawayDefinitions.Finished, frame.Zoom, "Ceres getaway terminator");
                continue;
            }
            AssertEqual(Word(0xa6af2f + offset), frame.YVelocity, "Ceres getaway exact native Y increment");
            AssertEqual(Word(0xa6b00f + offset), frame.XVelocity, "Ceres getaway exact native X subtraction");
        }
        foreach (ushort invalid in new ushort[] { 1, 223, 225, 226, 65535 })
            AssertThrows<InvalidDataException>(() => CeresRidleyGetawayDefinitions.FromByteIndex(invalid), "Ceres invalid curve index");
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match, including irregular zoom steps and terminal frame.");
    }
}
