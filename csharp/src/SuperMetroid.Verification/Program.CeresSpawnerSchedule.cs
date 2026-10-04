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
        foreach (int departure in new[] { 1, 209, 210, 222, 273, int.MaxValue })
        {
            var native = new IntroDiscoverySprite(0, 0, 0, 0xce35);
            ushort countdown = Word(0x8bc33c);
            bool disabled = false;
            for (int frame = 1; frame <= 300; frame++)
            {
                CeresExplosionWave expected = CeresExplosionWave.None;
                if (native.IsActive && native.PreInstructionPointer == 0xc489 && !disabled)
                {
                    if (frame >= departure) disabled = true;
                    else
                    {
                        countdown = unchecked((ushort)(countdown - 1));
                        if ((short)countdown <= 0)
                        {
                            expected = CeresExplosionWave.Repeating;
                            countdown = Word(0x8bc4a9);
                        }
                    }
                }
                native.Step(rom, (opcode, cursor) =>
                {
                    CeresExplosionWave instruction = opcode switch
                    {
                        0xc404 => CeresExplosionWave.Initial,
                        0xc50c => CeresExplosionWave.Final,
                        _ => throw new InvalidOperationException($"Unexpected spawner opcode {opcode:X4}."),
                    };
                    AssertEqual(CeresExplosionWave.None, expected, "no simultaneous pre-instruction and instruction spawn in original schedule");
                    expected = instruction;
                    return cursor;
                }, pointer => Word(0x8b0000 | pointer));
                AssertEqual(expected, CeresExplosionDefinitions.WaveAtFrame(frame, frame < departure), "native spawner event on exact frame");
            }
            AssertEqual(false, native.IsActive, "native spawner terminates");
        }
        foreach (int outside in new[] { int.MinValue, -1, 0, 301, int.MaxValue })
            AssertEqual(CeresExplosionWave.None, CeresExplosionDefinitions.WaveAtFrame(outside, true), "outside finite spawner lifetime");
    }
}
