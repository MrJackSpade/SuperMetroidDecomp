using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The cartridge's own Kraid head lists, decoded independently of production, and the translation between their native
/// cursors and <see cref="KraidHeadAnimations"/>' typed cursors. The four lists lie back to back from $A7:96D2 to the
/// mouth shapes at $A7:9788; each ends in a $FFFF terminator. A timed pose is eight bytes (duration, tilemap, outer and
/// inner mouth shapes); a cry is a two-byte call. Only verification and the native snapshot import know these addresses.
/// </summary>
internal sealed class KraidHeadNativeCursors
{
    /// <summary>The native bank holding Kraid's head lists.</summary>
    internal const int Bank = 0xa70000;

    /// <summary>The first head list, the attack roar.</summary>
    internal const ushort FirstList = 0x96d2;

    /// <summary>The mouth shapes that follow the last head list.</summary>
    internal const ushort ListsEnd = 0x9788;

    /// <summary>The roar cry's native call target.</summary>
    internal const ushort RoarCryCall = 0xaf94;

    /// <summary>The death cry's native call target.</summary>
    internal const ushort DyingCryCall = 0xaf9f;

    /// <summary>The native terminator word that ends a list.</summary>
    internal const ushort Terminator = 0xffff;

    /// <summary>Every native record, in cartridge order, beside the typed cursor that stands for it.</summary>
    internal IReadOnlyList<KraidHeadNativeRecord> Records { get; }

    /// <summary>Decodes the four native head lists from <paramref name="rom"/>.</summary>
    internal KraidHeadNativeCursors(IImportCartridgeSource rom)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ushort Word(int pointer) => (ushort)(rom.ReadCartridgeByte(Bank | pointer) | (rom.ReadCartridgeByte(Bank | (pointer + 1)) << 8));
        var records = new List<KraidHeadNativeRecord>();
        int program = 0, position = 0;
        for (int pointer = FirstList; pointer < ListsEnd;)
        {
            ushort opcode = Word(pointer);
            bool pose = opcode < 0x8000;
            records.Add(new((ushort)pointer, new(Enum.GetValues<KraidHeadProgram>()[program], position), opcode,
                pose ? Word(pointer + 2) : (ushort)0, pose ? Word(pointer + 4) : (ushort)0, pose ? Word(pointer + 6) : (ushort)0,
                opcode is RoarCryCall or DyingCryCall ? Word(opcode + 2) : (ushort)0));
            pointer += pose ? 8 : 2;
            position++;
            if (opcode == Terminator)
            {
                program++;
                position = 0;
            }
        }
        if (program != Enum.GetValues<KraidHeadProgram>().Length)
            throw new InvalidDataException($"The native Kraid head lists hold {program} programs.");
        Records = records;
    }

    /// <summary>The typed cursor native pointer <paramref name="pointer"/> stands for, or null when it is no record's start.</summary>
    internal AnimationProgramCursor<KraidHeadProgram>? TryCursorAt(ushort pointer) =>
        Records.FirstOrDefault(record => record.Pointer == pointer) is { Pointer: not 0 } record ? record.Cursor : null;

    /// <summary>The typed cursor native pointer <paramref name="pointer"/> stands for.</summary>
    internal AnimationProgramCursor<KraidHeadProgram> CursorAt(ushort pointer) => TryCursorAt(pointer)
        ?? throw new InvalidDataException($"Native Kraid head pointer $A7:{pointer:X4} is no record's start.");

    /// <summary>The native pointer <paramref name="cursor"/> stands for.</summary>
    internal ushort PointerAt(AnimationProgramCursor<KraidHeadProgram> cursor) =>
        Records.Single(record => record.Cursor == cursor).Pointer;

    /// <summary>The native record <paramref name="cursor"/> stands for.</summary>
    internal KraidHeadNativeRecord RecordAt(AnimationProgramCursor<KraidHeadProgram> cursor) =>
        Records.Single(record => record.Cursor == cursor);
}
