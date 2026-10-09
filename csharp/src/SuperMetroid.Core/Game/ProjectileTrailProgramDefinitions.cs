using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Fixed trail timing/commands in bank $90; appearance words remain external presentation.</summary>
public static class ProjectileTrailProgramDefinitions
{
    /// <summary>Each ice list contains seventeen poses and ten downward steps before its terminator.</summary>
    public const int IceTerminatorOffset = 17 * 4 + 10 * 2;
    /// <summary>Wave and missile each contain four four-byte timed records.</summary>
    public const int ShortTerminatorOffset = 4 * 4;

    /// <summary>Reads one compiled bank-$90 projectile-trail mechanics word without exposing presentation operands or mutable low-half aliases.</summary>
    /// <param name="address">Full 24-bit bus identity of a terminator, ice movement command, or timed-frame delay word.</param>
    /// <param name="word">Zero for terminators, a compiled movement command for ice fall entries, or the one-/four-tick delay for a visual frame; zero on failure.</param>
    /// <returns>True only for a compiled mechanics address in the native movement bank.</returns>
    public static bool TryRead(int address, out ushort word)
    {
        word = 0;
        if ((address & ~0xffff) != SamusProjectileRomData.Banks.Movement) return false;
        ushort pointer = (ushort)address;
        if (pointer == ProjectileTrailDefinitions.Empty ||
            pointer == ProjectileTrailDefinitions.LeftIce + IceTerminatorOffset ||
            pointer == ProjectileTrailDefinitions.RightIce + IceTerminatorOffset ||
            pointer == ProjectileTrailDefinitions.Wave + ShortTerminatorOffset ||
            pointer == ProjectileTrailDefinitions.Missile + ShortTerminatorOffset) return true;
        if (IsIceFallCommand(pointer - ProjectileTrailDefinitions.LeftIce))
        { word = SamusProjectileRomData.Trails.MoveLeftDown; return true; }
        if (IsIceFallCommand(pointer - ProjectileTrailDefinitions.RightIce))
        { word = SamusProjectileRomData.Trails.MoveRightDown; return true; }
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        {
            if (pointer != frame) continue;
            bool longHold = pointer == ProjectileTrailDefinitions.LeftIce + IceTerminatorOffset - 4 ||
                pointer == ProjectileTrailDefinitions.RightIce + IceTerminatorOffset - 4 ||
                pointer >= ProjectileTrailDefinitions.Wave;
            word = (ushort)(longHold ? 4 : 1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// $90:B4CB/B52D: the ice sprite first falls after six poses, then after two more,
    /// then after every remaining one-tick pose. A command occupies two bytes and a pose four.
    /// </summary>
    private static bool IsIceFallCommand(int offset) => offset == 6 * 4 ||
        (offset >= 8 * 4 + 2 && offset < IceTerminatorOffset - 4 && (offset - (8 * 4 + 2)) % 6 == 0);
    /// <summary>
    /// Reads compiled high-bank mechanics or a genuine mutable bank-$90 low-half alias.
    /// Presentation words and unrelated cartridge bytes are not valid program entries.
    /// </summary>
    public static ushort Read(ISnesAddressSpace bus, int address)
    {
        if (TryRead(address, out ushort word))
            return word;

        SnesAddress source = SnesAddress.FromBusAddress(address);
        if (source.Bank == 0x90 && !source.IsUpperLoRomWindow)
        {
            ISnesMutableMemory memory = bus as ISnesMutableMemory ??
                throw new InvalidOperationException(
                    "Projectile trail low-bank alias requires mutable console memory.");
            return (ushort)(memory.ReadWorkRamByte(address) |
                (memory.ReadWorkRamByte((int)source.AddWithinBank(1)) << 8));
        }

        throw new InvalidDataException(
            $"Projectile trail program word {source} is outside compiled mechanics data " +
            "and is not a mutable bank-$90 low-half alias.");
    }
}
