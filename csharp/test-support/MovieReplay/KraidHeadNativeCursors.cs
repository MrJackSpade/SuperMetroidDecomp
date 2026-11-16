using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The cartridge's own Kraid head lists, decoded independently of production, and the translation between their native
/// cursors and <see cref="KraidHeadAnimations"/>' typed playbacks (#1600): a record before a step is the head animation
/// standing before that beat, and a list's terminator is the animation holding after its last; the second list, which
/// nothing starts, has no typed playback. The four lists lie back to back from $A7:96D2 to the
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

    /// <summary>
    /// The typed animation each native list is, in cartridge order (#2213): the second list, a slower roar, is one nothing
    /// starts, so the port has no animation for it.
    /// </summary>
    private static readonly KraidHeadSegment?[] ListAnimations = [KraidHeadSegment.Roar, null, KraidHeadSegment.EyeGlow, KraidHeadSegment.Death];

    /// <summary>Every record of the started lists, in cartridge order, beside the typed playback that stands for it.</summary>
    internal IReadOnlyList<KraidHeadNativeRecord> Records { get; }

    /// <summary>
    /// Every native pose of every list, the unstarted roar's included, in cartridge order: what a native mouth read behind a
    /// list's start finds.
    /// </summary>
    internal IReadOnlyList<KraidHeadNativePose> AllPoses { get; }

    /// <summary>Decodes the four native head lists from <paramref name="rom"/>.</summary>
    internal KraidHeadNativeCursors(IImportCartridgeSource rom)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ushort Word(int pointer) => (ushort)(rom.ReadCartridgeByte(Bank | pointer) | (rom.ReadCartridgeByte(Bank | (pointer + 1)) << 8));
        var records = new List<KraidHeadNativeRecord>();
        var poses = new List<KraidHeadNativePose>();
        int program = 0, position = 0;
        for (int pointer = FirstList; pointer < ListsEnd;)
        {
            ushort opcode = Word(pointer);
            bool pose = opcode < 0x8000;
            if (program >= ListAnimations.Length)
                throw new InvalidDataException($"The native Kraid head lists hold more than {ListAnimations.Length} programs.");
            if (pose)
                poses.Add(new((ushort)pointer, Word(pointer + 4), Word(pointer + 6)));
            if (ListAnimations[program] is { } list)
            {
                AnimationPlayback<KraidHeadSegment> playback = opcode == Terminator
                    ? new(KraidHeadAnimations.StartingWith(list), AnimationPlace.HoldingIn(0, position))
                    : KraidHeadAnimations.At(list, position);
                records.Add(new((ushort)pointer, playback, opcode,
                    pose ? Word(pointer + 2) : (ushort)0, pose ? Word(pointer + 4) : (ushort)0, pose ? Word(pointer + 6) : (ushort)0,
                    opcode is RoarCryCall or DyingCryCall ? Word(opcode + 2) : (ushort)0));
            }
            pointer += pose ? 8 : 2;
            position++;
            if (opcode == Terminator)
            {
                program++;
                position = 0;
            }
        }
        if (program != ListAnimations.Length)
            throw new InvalidDataException($"The native Kraid head lists hold {program} programs.");
        Records = records;
        AllPoses = poses;
    }

    /// <summary>The typed playback native pointer <paramref name="pointer"/> stands for, or null when it is no record's start.</summary>
    internal AnimationPlayback<KraidHeadSegment>? TryPlaybackAt(ushort pointer) =>
        Records.FirstOrDefault(record => record.Pointer == pointer) is { Pointer: not 0 } record ? record.Playback : null;

    /// <summary>The typed playback native pointer <paramref name="pointer"/> stands for.</summary>
    internal AnimationPlayback<KraidHeadSegment> PlaybackAt(ushort pointer) => TryPlaybackAt(pointer)
        ?? throw new InvalidDataException($"Native Kraid head pointer $A7:{pointer:X4} is no record's start.");

    /// <summary>
    /// The native pointer <paramref name="playback"/> stands for: its record's, a play done after its last beat standing
    /// at the list's terminator whether or not it has held there yet.
    /// </summary>
    internal ushort PointerAt(AnimationPlayback<KraidHeadSegment> playback) => RecordAt(playback).Pointer;

    /// <summary>The native record <paramref name="playback"/> stands for.</summary>
    internal KraidHeadNativeRecord RecordAt(AnimationPlayback<KraidHeadSegment> playback)
    {
        KraidHeadSegmentPlace place = KraidHeadAnimations.PlaceOf(playback);
        return Records.Single(record => KraidHeadAnimations.PlaceOf(record.Playback) == place);
    }
}
