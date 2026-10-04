using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream4(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int pattern = 0; pattern < 4; pattern++)
        for (int stage = 0; stage < 6; stage++)
        {
            var actual = RidleyPogoDefinitions.Read(pattern, stage);
            int x = 0xa60000 | Word(0xa6b965 + 2 * pattern);
            int y = 0xa60000 | Word(0xa6b96d + 2 * pattern);
            AssertEqual(Word(x + 2 * stage), actual.X, "stream4 native pogo horizontal magnitude");
            AssertEqual(Word(y + 2 * stage), actual.Y, "stream4 native pogo signed vertical speed");
            AssertEqual(Word(0xa6b94d + 2 * stage), actual.UpwardAcceleration, "stream4 native pogo upward acceleration");
            AssertEqual(Word(0xa6b959 + 2 * stage), actual.DownwardAcceleration, "stream4 native pogo downward acceleration");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(invalid, 0), "stream4 invalid pogo pattern");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyPogoDefinitions.Read(0, invalid), "stream4 invalid pogo stage");
        for (ushort parameter = 0; parameter <= 22; parameter += 2)
        {
            var actual = RidleyExplosionDefinitions.GetPart(parameter);
            AssertEqual(parameter, actual.Parameter, "stream4 breakup parameter identity");
            AssertEqual(Word(0xa6c6ce + parameter), actual.Lifetime, "stream4 native breakup lifetime");
            AssertEqual(Word(0xa6c6e6 + parameter), actual.InitializationRoutine, "stream4 native breakup initialization routine");
        }
        for (int orientation = 0; orientation < 16; orientation++)
            AssertEqual(Word(0xa6c7ba + 2 * orientation), RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, orientation), "stream4 native tail-tip orientation program");
        foreach (ushort invalid in new ushort[] { 1, 23, 24, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.GetPart(invalid), "stream4 invalid breakup parameter");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyExplosionDefinitions.SelectTailInstructionList(RidleyExplosionParts.TailTip, invalid), "stream4 invalid tail-tip orientation");
    }
}
