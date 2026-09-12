using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledRioLaunches(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        AssertEqual(Word(0xa2bbbb), RioLaunchDefinitions.RioYVelocity, "Rio NTSC Y launch");
        AssertEqual(Word(0xa2bbbf), RioLaunchDefinitions.RioXVelocity, "Rio X launch");
        AssertEqual(Word(0xa2c1c5), RioLaunchDefinitions.NorfairXVelocity, "Norfair Rio X launch");
        AssertEqual(Word(0xa2c6ca), RioLaunchDefinitions.LowerNorfairYVelocity, "Lower Norfair Rio Y launch");
        AssertEqual(Word(0xa2c6ce), RioLaunchDefinitions.LowerNorfairXVelocity, "Lower Norfair Rio X launch");
        for (int random = 0; random <= ushort.MaxValue; random++)
            AssertEqual(Word(0xa2c1c1 + ((random >> 1) & 2)), RioLaunchDefinitions.NorfairYVelocity((ushort)random), "Norfair Rio RNG selector");
        Console.WriteLine("Rio launch definitions: seven native words and all 65536 Norfair RNG selectors match.");
    }
}
