using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresSpawnerSchedule(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        // C33B loads the repeat countdown seed; C4A8 reloads the period after a spawn.
        AssertEqual((byte)0xa9, rom.ReadByte(0x8bc33b), "native countdown seed opcode");
        AssertEqual((byte)0xa9, rom.ReadByte(0x8bc4a8), "native countdown reload opcode");
        foreach (int reseedFrame in new[] { 1, 211, 273 })
        foreach (int departure in new[] { 1, 209, 210, 222, 273, int.MaxValue })
        {
            var native = new IntroDiscoverySprite(0, 0, 0, 0xce35);
            ushort countdown = 0;
            ushort actualCountdown = 0;
            bool disabled = false;
            for (int frame = 1; frame <= 300; frame++)
            {
                if (frame == reseedFrame)
                {
                    countdown = Word(0x8bc33c);
                    actualCountdown = CeresExplosionDefinitions.RepeatingCountdownSeed;
                }
                CeresExplosionSpawnEvents expected = default;
                if (native.IsActive && native.PreInstructionPointer == 0xc489 && !disabled)
                {
                    if (frame >= departure) disabled = true;
                    else
                    {
                        countdown = unchecked((ushort)(countdown - 1));
                        if ((short)countdown <= 0)
                        {
                            expected = expected with { Repeating = true };
                            countdown = Word(0x8bc4a9);
                        }
                    }
                }
                native.Step(rom, (opcode, cursor) =>
                {
                    expected = opcode switch
                    {
                        0xc404 => expected with { Initial = true },
                        0xc50c => expected with { Final = true },
                        _ => throw new InvalidOperationException($"Unexpected spawner opcode {opcode:X4}."),
                    };
                    return cursor;
                }, pointer => Word(0x8b0000 | pointer));
                AssertEqual(expected, CeresExplosionDefinitions.EventsAtFrame(frame, frame < departure, ref actualCountdown),
                    $"native spawner events at frame{frame}, reset{reseedFrame}, departure{departure}");
                AssertEqual(countdown, actualCountdown, "native repeat countdown including late fade reset");
            }
            AssertEqual(false, native.IsActive, "native spawner terminates");
        }
        foreach (int outside in new[] { int.MinValue, -1, 0, 301, int.MaxValue })
        {
            ushort untouched = 123;
            AssertEqual(default(CeresExplosionSpawnEvents), CeresExplosionDefinitions.EventsAtFrame(outside, true, ref untouched), "outside finite spawner lifetime");
            AssertEqual((ushort)123, untouched, "inactive countdown is untouched");
        }
    }
}
