using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyScrollProgramIdentities(SuperMetroidAddressSpace rom) => VerifyScrollProgramField(rom,0);
    private static void VerifyScrollProgramIndices(SuperMetroidAddressSpace rom) => VerifyScrollProgramField(rom,1);
    private static void VerifyScrollProgramStates(SuperMetroidAddressSpace rom) => VerifyScrollProgramField(rom,2);
    private static void VerifyScrollProgramTermination(SuperMetroidAddressSpace rom) => VerifyScrollProgramField(rom,3);

    private static void VerifyScrollProgramField(SuperMetroidAddressSpace rom, int field)
    {
        // Original generated owner at 8de735494b5027ed7ff3609760fd8ed982e96f08; expected operands come from ROM.
        ushort[] pointers = [0x92B0,0x9389,0x938C,0x938F,0x9396,0x9399,0x939C,0x939F,0x93A2,0x93A5,0x94C2,0x94C7,0x94FA,0x9658,0x968C,0x9744,0x9747,0x974A,0x974D,0x9750,0x9753,0x9756,0x9759,0x97AB,0x97B0,0x9801,0x9966,0x99F3,0x99F6,0x9B46,0x9B4B,0x9B52,0x9B98,0x9BF9,0x9C00,0x9C32,0x9D11,0x9D14,0x9D84,0x9D8B,0x9D8E,0x9D91,0x9D96,0x9D99,0x9E40,0x9E49,0x9F05,0x9F08,0x9F0B,0x9F0E,0x9F5F,0x9FB7,0xA04A,0xA104,0xA28E,0xA36F,0xA374,0xA379,0xA3A9,0xA3DA,0xA439,0xA43E,0xA4A2,0xA4A9,0xA4AE,0xA50F,0xA514,0xA519,0xA51C,0xA59C,0xA6D6,0xA6D9,0xA6DC,0xA6DF,0xA860,0xA8EC,0xA8EF,0xA8F2,0xA8F5,0xA980,0xA987,0xA98A,0xAA70,0xAA75,0xAA7C,0xAA7F,0xACB0,0xADA7,0xADAA,0xAE66,0xAE6B,0xAE6E,0xAE71,0xAEA9,0xAEAC,0xAEB1,0xAF0F,0xAF6F,0xB0A7,0xB0AC,0xB0B1,0xB224,0xB22D,0xB27D,0xB280,0xB2D1,0xB3D9,0xB3DC,0xB445,0xB448,0xB44B,0xB44E,0xB451,0xB454,0xB4E0,0xB547,0xB54E,0xB555,0xB5C3,0xB5C8,0xB5CD,0xB5D2,0xB612,0xB615,0xB61A,0xB61D,0xB622,0xB625,0xB628,0xB68D,0xB690,0xB695,0xB72D,0xB730,0xB737,0xB73C,0xC9EC,0xC9F1,0xCB7A,0xCB7D,0xCB80,0xCB83,0xCB86,0xCC24,0xCCC0,0xCCC5,0xCE3D,0xCF4C,0xCF4F,0xCFB5,0xCFBC,0xCFC1,0xCFC6,0xD012,0xD052,0xD135,0xD138,0xD16A,0xD1A0,0xD1D8,0xD216,0xD219,0xD24D,0xD4BD,0xD67D,0xD68A,0xD695,0xD6C8,0xD6CB,0xD7DF,0xD951,0xD956,0xD95B];
        AssertTrue(pointers.SequenceEqual(RoomPlmScrollProgramDefinitions.Pointers),"Scroll original program identities and order");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                bool owned = pointers.Contains((ushort)raw);
                int writes = 0;
                AssertEqual(owned,RoomPlmScrollProgramDefinitions.TryApply((ushort)raw,(_,_) => writes++),"Scroll complete selector domain");
                if (!owned)
                {
                    AssertEqual(0,writes,"Rejected scroll makes no writes");
                    AssertThrows<InvalidDataException>(() => RoomPlmScrollProgramDefinitions.Get((ushort)raw),"Rejected scroll DTO");
                    AssertThrows<InvalidDataException>(() => RoomPlmScrollProgramDefinitions.Apply((ushort)raw,(_,_) => writes++),"Rejected scroll execution");
                }
            }
            var compiled = RoomPlmPopulationDefinition.FromCompiled(0x8230);
            AssertEqual((ushort?)0x94fa,compiled.Placements.Span[0].CompiledScrollSource,"Retail scroll binds identity");
            AssertTrue(compiled.Placements.Span[0].ScrollProgram.IsEmpty,"Retail scroll carries no regenerated pair table");
            var rebound = new RoomPlmPopulationDefinition(0x8230,
                [compiled.Placements.Span[0] with { RoomArgument = 0xffff }]);
            AssertEqual((ushort?)0x94fa,rebound.Placements.Span[0].CompiledScrollSource,"Copied placement preserves its bound program independently of argument");
            var customized = new RoomPlmPopulationDefinition(0x8230,
                [compiled.Placements.Span[0] with { ScrollProgram = new byte[] {1,2,0x80} }]);
            AssertEqual((ushort?)null,customized.Placements.Span[0].CompiledScrollSource,"Explicit custom pairs replace compiled source");
            AssertTrue(customized.Placements.Span[0].ScrollProgram.Span.SequenceEqual(new byte[] {1,2,0x80}),"Explicit custom pairs survive placement copy");
            AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition(0x9000,
                [new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(0xb703),0,0,0x94fa)]),
                "Constructed scroll still requires decoded program");
        }
        int totalPairs = 0, totalBytes = 0;
        foreach (ushort pointer in pointers)
        {
            var actual = new List<(int Index, RoomScrollState State)>();
            RoomPlmScrollProgramDefinitions.Apply(pointer,(index,state) => actual.Add((index,state)));
            var dto = RoomPlmScrollProgramDefinitions.Get(pointer);
            int cursor = 0, pair = 0;
            while (true)
            {
                byte index = rom.ReadByte(0x8f0000 | (pointer + cursor++));
                if ((index & 0x80) != 0)
                {
                    if (field == 3)
                    {
                        AssertEqual(pair,actual.Count,"Scroll exactly terminates ordered writes");
                        AssertEqual(cursor,dto.Length,"Scroll original encoded length");
                        AssertEqual(index,dto.Span[cursor - 1],"Scroll native terminator");
                    }
                    break;
                }
                byte state = rom.ReadByte(0x8f0000 | (pointer + cursor++));
                if (field == 1)
                {
                    AssertEqual((int)index,actual[pair].Index,"Scroll original ordered storage index");
                    AssertEqual(index,dto.Span[cursor - 2],"Scroll encoded original index");
                }
                if (field == 2)
                {
                    AssertEqual(state,(byte)actual[pair].State,"Scroll original ordered state");
                    AssertEqual(state,dto.Span[cursor - 1],"Scroll encoded original state");
                }
                pair++;
            }
            totalPairs += pair;
            totalBytes += cursor;
        }
        AssertEqual(173,pointers.Length,"Scroll original program count");
        AssertEqual(285,totalPairs,"Scroll original pair count");
        AssertEqual(743,totalBytes,"Scroll original byte count");
    }
}