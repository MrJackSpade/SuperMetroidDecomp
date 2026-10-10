using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies that the compiled cooldown, initial Y-index, and signed X-velocity selectors match
    /// the native Polyp tables for every possible 16-bit RNG value.
    /// </summary>
    /// <param name="rom">Retail address space containing the native launch and cooldown tables used for comparison.</param>
    private static void VerifyCompiledPolypLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            AssertEqual(Word(0xa2b520 + (random & 0x0e)), PolypLaunchDefinitions.Cooldown((ushort)random), "Polyp native cooldown selection");
            AssertEqual(Word(0xa2b530 + (random & 0x1e)), PolypLaunchDefinitions.InitialYIndex((ushort)random), "Polyp native Y-index selection");
            AssertEqual(Word(0xa2b550 + (random & 0x1e)), PolypLaunchDefinitions.XVelocity((ushort)random), "Polyp native signed X selection");
        }
        Console.WriteLine("Polyp launch definitions: all 65536 RNG words match each of three native table selectors, covering all 40 authored words.");
    }
}
