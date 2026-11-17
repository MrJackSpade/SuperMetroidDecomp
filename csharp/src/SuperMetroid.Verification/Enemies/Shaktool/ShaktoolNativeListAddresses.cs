using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>
/// The native bank-$AA lists the Shaktool ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ShaktoolNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ShaktoolSegment>[] Entries =
    [
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandHeadBobPrimaryPiece, 0xd9fc),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandHeadBobFinalPiece, 0xda04),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandPrimaryPiece, 0xda0e),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.SawHandFinalPiece, 0xda1e),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceHeadBobBack, 0xda56),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceHeadBobFront, 0xda62),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.ArmPieceNormal, 0xda72),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadHeadBob, 0xda90),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingLeft, 0xdaa4),
        new NativeProgramEntry<ShaktoolSegment>(ShaktoolSegment.HeadAimingDown, 0xdad4),
    ];

    /// <summary>The cartridge's table of the head's list for each aim, by the bearing of the direction it aims.</summary>
    private const int AimListTable = 0xaadd15;

    /// <summary>
    /// The picture the native list the cartridge selects for the head aiming at <paramref name="bucket"/>, the bearing of
    /// the direction it aims, shows (#2284), as its spritemap word: each list shows one picture and holds it, looping on
    /// itself or sleeping.
    /// </summary>
    internal static ushort AimedSpritemap(IImportCartridgeSource cartridge, int bucket)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        ushort list = cartridge.ReadCartridgeWord(AimListTable + (bucket >> 5) * 2);
        string at = $"Shaktool head list ${list:X4} at bearing {bucket}";
        int bank = Bank << 16;
        AssertEqual(NativeEnemyListDefinitions.WordKind.Frame,
            NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(bank | list)), $"{at}: frame");
        ushort next = cartridge.ReadCartridgeWord(bank | (list + 4));
        NativeEnemyListDefinitions.WordKind after = NativeEnemyListDefinitions.Classify(next);
        if (after == NativeEnemyListDefinitions.WordKind.GoTo)
            AssertEqual(list, cartridge.ReadCartridgeWord(bank | (list + 6)), $"{at}: loops on its picture");
        else if (after != NativeEnemyListDefinitions.WordKind.Sleep)
            throw new InvalidDataException($"{at} goes on to ${next:X4} after its picture.");
        return cartridge.ReadCartridgeWord(bank | (list + 2));
    }

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, ShaktoolNativeListAddressesInstruction> Instructions = new()
    {
        [0xd94a] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Lower, ""),
        [0xd953] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.Raise, ""),
        [0xd9ba] = new ShaktoolNativeListAddressesInstruction(ShaktoolCueKind.ResumeMotion, ""),
    };

    /// <summary>
    /// The native routine the head's attack list calls between its two long pauses (#1803): it returned at once, so the
    /// animations have no event for it.
    /// </summary>
    internal static readonly HashSet<ushort> OmittedInstructions = [0xd99f];
}
