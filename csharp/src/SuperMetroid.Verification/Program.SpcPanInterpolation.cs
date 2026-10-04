using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    private static void VerifySpcPanInterpolation()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pan interpolation oracle revision");
        byte Instruction(int address) => rom.ReadByte(0xcf6c08 + address);
        var player = new ManagedSpcPlayer();
        var ram = (byte[])(typeof(ManagedSpcPlayer).GetField("ram", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(player)
            ?? throw new InvalidOperationException("Missing SPC RAM fixture access."));
        var method = typeof(ManagedSpcPlayer).GetMethod("WriteVolume", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Missing real music volume writer.");
        var write = method.CreateDelegate<Action<ManagedSpcMusicChannel, ushort>>(player);
        var channel = new ManagedSpcMusicChannel { FinalVolume = 255 };
        for (int index = 0; index <= 256; index++)
            ram[0x1e1d + index] = rom.ReadByte(0xcf8a25 + index);

        // The first sharp-echo tap127 followed by0 is the original descending pair.
        // Pan21.5 exposes the subtraction width before any other fixture variation.
        Check(0x1580);

        // Confirm all fixed-point pan inputs, including mirrored16-bit overreads,
        // using mutable neighboring RAM with frequent decreasing sample pairs.
        for (int index = 22; index <= 256; index++)
            ram[0x1e1d + index] = unchecked((byte)(255 - index));
        for (int pan = 0; pan <= ushort.MaxValue; pan++) Check((ushort)pan);
        Console.WriteLine("SPC pan interpolation: original descending-pair reproduction and all65536 fixed-point inputs match native register arithmetic.");

        void Check(ushort pan)
        {
            write(channel, pan);
            AssertEqual(NativeVolume(pan), player.ReadDspRegisterForVerification(0), $"native left volume at pan{pan:X4}");
            AssertEqual(NativeVolume(unchecked((ushort)(0x1400 - pan))),
                player.ReadDspRegisterForVerification(1), $"native right volume at pan{pan:X4}");
        }

        byte NativeVolume(ushort pan)
        {
            // Execute the original straight-line interpolation block, not a second
            // copy of the host expression. X=0 selects this fixture's first channel.
            ram[0x10] = (byte)pan;
            ram[0x11] = (byte)(pan >> 8);
            ram[0x321] = channel.FinalVolume;
            byte a = 0, y = 0;
            bool carry = false;
            int pc = 0x1c4f;
            while (pc < 0x1c67)
            {
                switch (Instruction(pc++))
                {
                    case 0xeb: y = ram[Instruction(pc++)]; break; // MOV Y,dp
                    case 0xf6: a = ram[Word() + y]; break; // MOV A,abs+Y
                    case 0x80: carry = true; break; // SETC
                    case 0xb6: // SBC A,abs+Y, retaining only eight bits in A
                        int difference = a - ram[Word() + y] - (carry ? 0 : 1);
                        a = unchecked((byte)difference);
                        carry = difference >= 0;
                        break;
                    case 0xcf: // MUL YA
                        int product = y * a;
                        a = unchecked((byte)product);
                        y = (byte)(product >> 8);
                        break;
                    case 0xdd: a = y; break; // MOV A,Y
                    case 0x60: carry = false; break; // CLRC
                    case 0x96: // ADC A,abs+Y
                        int sum = a + ram[Word() + y] + (carry ? 1 : 0);
                        a = unchecked((byte)sum);
                        carry = sum > 255;
                        break;
                    case 0xfd: y = a; break; // MOV Y,A
                    case 0xf5: a = ram[Word()]; break; // MOV A,abs+X (X=0)
                    default: throw new InvalidDataException($"Unexpected SPC pan opcode at {pc - 1:X4}.");
                }
            }
            AssertEqual(0x1c67, pc, "native interpolation block boundary");
            return y;

            int Word() => Instruction(pc++) | Instruction(pc++) << 8;
        }
    }
}
