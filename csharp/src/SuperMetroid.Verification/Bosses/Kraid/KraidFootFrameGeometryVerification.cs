using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Checks Kraid's five installed foot pictures and the 35 native foot records his animations show (#2210): each record
/// stores a pose of one of the pictures, and each picture is composed from the record where the walk first shows it.
/// </summary>
internal static class KraidFootFrameGeometryVerification
{
    /// <summary>The five foot pictures, in the installed document's order.</summary>
    private static readonly EnemyFrameId[] Pictures =
    [
        EnemyFrameId.KraidFootPlanted, EnemyFrameId.KraidFootLifting, EnemyFrameId.KraidFootRaised,
        EnemyFrameId.KraidFootSwinging, EnemyFrameId.KraidFootReaching,
    ];

    /// <summary>
    /// Traverses the native records the typed foot programs show and compares their poses, the installed pictures'
    /// identities and keys, and every record's two components.
    /// </summary>
    internal static void VerifyKraidFootFrameGeometry(CartridgeImportAddressSpace rom)
    {
        // The typed foot programs match bank $A7 (--kraid-part-animations): every record they show is one of the run's.
        KraidPartSegment[] foot =
        [
            KraidPartSegment.FootInitial, KraidPartSegment.FootNeutral, KraidPartSegment.FootWalkingForward,
            KraidPartSegment.FootLungeForward, KraidPartSegment.FootWalkingBackward,
        ];
        var shown = new HashSet<KraidPartPose>(foot.SelectMany(segment => KraidAnimations.Segments.Beats(segment))
            .Select(beat => beat.ShownFrame).OfType<KraidPartPose>());
        // The run of 33 eighteen-byte roots from $A7:8CE3, its last root $A7:8F47 past the unshown $A7:8F35, and the
        // standing root $A7:A565.
        ushort[] records =
        [
            .. Enumerable.Range(0, 33).Select(root => (ushort)(KraidPartFrameToolingDefinitions.FootStart + 18 * root)),
            KraidPartFrameToolingDefinitions.FootLast, KraidPartFrameToolingDefinitions.FootStanding,
        ];
        AssertEqual(35, records.Length, "Kraid foot native records");
        var stored = new HashSet<KraidPartPose>();
        foreach (ushort record in records)
        {
            KraidPartPose pose = KraidPartFrameToolingDefinitionsConstants.PoseAt(record)
                ?? throw new InvalidOperationException($"Verification failed: $A7:{record:X4} stores no Kraid foot pose.");
            AssertTrue(Pictures.Contains(pose.Picture), $"$A7:{record:X4} shows one of the five foot pictures");
            stored.Add(pose);
            int address = 0xa70000 | record;
            AssertEqual(2, LittleEndianBytes.Word(KraidFootFrameGeometryRomByte(rom, address), KraidFootFrameGeometryRomByte(rom, address + 1)),
                $"$A7:{record:X4}: native two-component frame size");
        }
        AssertTrue(stored.SetEquals(shown), "the foot's animations show exactly the poses its native records store");
        AssertTrue(KraidPartFrameToolingDefinitionsConstants.PoseAt(0x8f35) is null, "the unshown root stores no pose");

        AssertEqual(Pictures.Length, KraidFootVisualDefinitions.FrameCount, "Kraid foot picture count");
        AssertEqual(Pictures.Length, KraidFootVisualDefinitions.Frames.Length, "Kraid foot sequence length");
        for (int index = 0; index < Pictures.Length; index++)
        {
            NativeEnemySpritemapDefinition composed = KraidFootVisualDefinitionsConstants.Frame(index);
            AssertEqual(composed, KraidFootVisualDefinitions.Frames[index], "Kraid foot indexed sequence");
            AssertEqual(new NativeEnemySpritemapDefinition(0xa7, composed.Pointer, $"kraid_foot_oam_{composed.Pointer:X4}"), composed,
                "native Kraid foot identity and key");
            // Each picture is composed where the walk first shows it: unstepped, at its own depth.
            AssertEqual<KraidPartPose?>(new KraidPartPose(Pictures[index]), KraidPartFrameToolingDefinitionsConstants.PoseAt(composed.Pointer),
                $"{Pictures[index]} is composed from $A7:{composed.Pointer:X4}, where the walk first shows it");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => KraidFootVisualDefinitionsConstants.Frame(-1), "negative foot index");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidFootVisualDefinitionsConstants.Frame(Pictures.Length), "foot index past end");
    }

    /// <summary>Reads a native foot presentation operand or spritemap count byte.</summary>
    private static byte KraidFootFrameGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
