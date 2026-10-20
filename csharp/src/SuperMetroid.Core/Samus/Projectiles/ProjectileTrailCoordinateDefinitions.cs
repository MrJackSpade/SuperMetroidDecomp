using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Native trail selection and signed left/right placement records; not editable sprite artwork.</summary>
internal static class ProjectileTrailCoordinateDefinitions
{
    /// <summary>$9BA4B3: BeamTrailOffsets_uncharged, pointer selections.</summary>
    private const int BeamTrailOffsets_uncharged = 0x9ba4b3;
    /// <summary>$9BA4CB: BeamTrailOffsets_charged, pointer selections.</summary>
    private const int BeamTrailOffsets_charged = 0x9ba4cb;
    /// <summary>$9BA4E3: BeamTrailOffsets_spazerSBA, pointer selections.</summary>
    private const int BeamTrailOffsets_spazerSBA = 0x9ba4e3;
    /// <summary>$9BA4F7: UnchargedBeamTrails_Wave_WaveIce, pointer selections.</summary>
    private const int UnchargedBeamTrails_Wave_WaveIce = 0x9ba4f7;
    /// <summary>$9BA50B: UnchargedBeamTrails_Default, pointer selections.</summary>
    private const int UnchargedBeamTrails_Default = 0x9ba50b;
    /// <summary>$9BA51F: UnchargedBeamTrails_IceSpazer, pointer selections.</summary>
    private const int UnchargedBeamTrails_IceSpazer = 0x9ba51f;
    /// <summary>$9BA533: UnchargedBeamTrails_WaveIceSpazer, pointer selections.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer = 0x9ba533;
    /// <summary>$9BA547: UnchargedBeamTrails_IcePlasma, pointer selections.</summary>
    private const int UnchargedBeamTrails_IcePlasma = 0x9ba547;
    /// <summary>$9BA55B: UnchargedBeamTrails_WaveIcePlasma, pointer selections.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma = 0x9ba55b;
    /// <summary>$9BA56F: UnchargedBeamTrails_Default_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_Default_0 = 0x9ba56f;
    /// <summary>$9BA58F: UnchargedBeamTrails_Wave_WaveIce_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_Wave_WaveIce_0 = 0x9ba58f;
    /// <summary>$9BA68F: UnchargedBeamTrails_IceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_0 = 0x9ba68f;
    /// <summary>$9BA6A7: UnchargedBeamTrails_IceSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_2 = 0x9ba6a7;
    /// <summary>$9BA6BF: UnchargedBeamTrails_IceSpazer_4, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_4 = 0x9ba6bf;
    /// <summary>$9BA6CB: UnchargedBeamTrails_IceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_5 = 0x9ba6cb;
    /// <summary>$9BA6E3: UnchargedBeamTrails_IceSpazer_7, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_7 = 0x9ba6e3;
    /// <summary>$9BA6EF: UnchargedBeamTrails_WaveIceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_0 = 0x9ba6ef;
    /// <summary>$9BA717: UnchargedBeamTrails_WaveIceSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_1 = 0x9ba717;
    /// <summary>$9BA767: UnchargedBeamTrails_WaveIceSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_3 = 0x9ba767;
    /// <summary>$9BA7B7: UnchargedBeamTrails_WaveIceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_5 = 0x9ba7b7;
    /// <summary>$9BA807: UnchargedBeamTrails_WaveIceSpazer_7, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_7 = 0x9ba807;
    /// <summary>$9BA82F: UnchargedBeamTrails_IcePlasma_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int UnchargedBeamTrails_IcePlasmaFirst = 0x9ba82f;
    /// <summary>Placement frames in each UnchargedBeamTrails_IcePlasma record.</summary>
    private const int UnchargedBeamTrails_IcePlasmaFrames = 2;
    /// <summary>$9BA86F: UnchargedBeamTrails_WaveIcePlasma_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasmaFirst = 0x9ba86f;
    /// <summary>Placement frames in each UnchargedBeamTrails_WaveIcePlasma record.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasmaFrames = 9;
    /// <summary>$9BA98F: ChargedBeamTrails_Default, pointer selections.</summary>
    private const int ChargedBeamTrails_Default = 0x9ba98f;
    /// <summary>$9BA9A3: ChargedBeamTrails_Wave_WaveIce, pointer selections.</summary>
    private const int ChargedBeamTrails_Wave_WaveIce = 0x9ba9a3;
    /// <summary>$9BA9B7: ChargedBeamTrails_IceSpazer, pointer selections.</summary>
    private const int ChargedBeamTrails_IceSpazer = 0x9ba9b7;
    /// <summary>$9BA9CB: ChargedBeamTrails_WaveIceSpazer, pointer selections.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer = 0x9ba9cb;
    /// <summary>$9BA9DF: ChargedBeamTrails_IcePlasma, pointer selections.</summary>
    private const int ChargedBeamTrails_IcePlasma = 0x9ba9df;
    /// <summary>$9BA9F3: ChargedBeamTrails_WaveIcePlasma, pointer selections.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma = 0x9ba9f3;
    /// <summary>$9BAA07: ChargedBeamTrails_Default_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_Default_0 = 0x9baa07;
    /// <summary>$9BAA27: ChargedBeamTrails_Wave_WaveIce_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_Wave_WaveIce_0 = 0x9baa27;
    /// <summary>$9BAAA7: ChargedBeamTrails_Wave_WaveIce_2, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_Wave_WaveIce_2 = 0x9baaa7;
    /// <summary>$9BAAE7: ChargedBeamTrails_Wave_WaveIce_3, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_Wave_WaveIce_3 = 0x9baae7;
    /// <summary>$9BAB27: ChargedBeamTrails_IceSpazer_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int ChargedBeamTrails_IceSpazerFirst = 0x9bab27;
    /// <summary>Placement frames in each ChargedBeamTrails_IceSpazer record.</summary>
    private const int ChargedBeamTrails_IceSpazerFrames = 10;
    /// <summary>$9BAC67: ChargedBeamTrails_WaveIceSpazer_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int ChargedBeamTrails_WaveIceSpazerFirst = 0x9bac67;
    /// <summary>Placement frames in each ChargedBeamTrails_WaveIceSpazer record.</summary>
    private const int ChargedBeamTrails_WaveIceSpazerFrames = 24;
    /// <summary>$9BAF67: ChargedBeamTrails_IcePlasma_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int ChargedBeamTrails_IcePlasmaFirst = 0x9baf67;
    /// <summary>Placement frames in each ChargedBeamTrails_IcePlasma record.</summary>
    private const int ChargedBeamTrails_IcePlasmaFrames = 8;
    /// <summary>$9BB067: ChargedBeamTrails_WaveIcePlasma_0, the first of 8 adjacent left X/Y and right X/Y placement frames records indexed by octant.</summary>
    private const int ChargedBeamTrails_WaveIcePlasmaFirst = 0x9bb067;
    /// <summary>Placement frames in each ChargedBeamTrails_WaveIcePlasma record.</summary>
    private const int ChargedBeamTrails_WaveIcePlasmaFrames = 22;
    /// <summary>$9BB327: SpazerSBATrail_WaveSpazer, pointer selections.</summary>
    private const int SpazerSBATrail_WaveSpazer = 0x9bb327;
    /// <summary>$9BB33B: SpazerSBATrail_WaveSpazer_0, the first of 4 adjacent left X/Y and right X/Y placement frames records indexed by record.</summary>
    private const int SpazerSBATrail_WaveSpazerFirst = 0x9bb33b;
    /// <summary>Placement frames in each SpazerSBATrail_WaveSpazer record.</summary>
    private const int SpazerSBATrail_WaveSpazerFrames = 4;
    /// <summary>$9BB37B: UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B, pointer selections.</summary>
    private const int UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B = 0x9bb37b;
    /// <summary>$9BB38F: UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F, left X/Y and right X/Y placement frames.</summary>
    private const int UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F = 0x9bb38f;
    /// <summary>$9B:B3A7, first adjacent-code byte reachable by a restored low-six-bit projectile type paired with a cataloged animation frame.</summary>
    private const int ReachableAdjacentCodeStart = 0x9bb3a7;
    /// <summary>$9B:B3C2, final adjacent-code byte reachable by the bounded projectile-type/frame cross-product.</summary>
    private const int ReachableAdjacentCodeEnd = 0x9bb3c2;
    /// <summary>$9B:FFFF, wrapped low byte read by an empty trail family on animation frame zero.</summary>
    private const int EmptyFamilyWrappedCoordinateByte = 0x9bffff;

    /// <summary>
    /// Bank-$9B bytes a trail can read right after a reflection. $90:BE17 installs the new
    /// direction's list start, so a trail spawned before its first record reads the word
    /// preceding that list (a goto operand or another list's frame) as its animation frame.
    /// These runs are every byte that frame can address for each reflectable beam
    /// (charged or not, every combination, and the Spazer/SBA families that have a ROM
    /// direction table), both missile families and all ten directions, copied from the
    /// pinned cartridge. Empty families index bank-$9B WRAM and are read live.
    /// </summary>
    private static readonly FrozenDictionary<int, byte> ReflectedListStartObservations =
        CreateObservations(
        [
            new(0x9ba56e, Lead: 0xa8, new(new(0, 0), new(0, 0))),
            new(0x9ba58e, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9baa26, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9bc0da, Lead: 0x6b, new(new(0, 0), new(124, 8))),
            new(0x9bc10a, Lead: 0x00, new(new(-32, 0), new(0, 0))),
            new(0x9bc13a, Lead: 0x02, new(new(0, 0), new(0, 6))),
            new(0x9bc16a, Lead: 0x06, new(new(0, 1), new(0, -20))),
            new(0x9bc19a, Lead: 0x02, new(new(0, 19), new(0, 20))),
            new(0x9bc1ca, Lead: 0x01, new(new(1, 1), new(1, 2))),
            new(0x9bc1fa, Lead: 0x07, new(new(7, 7), new(7, 8))),
            new(0x9bc24a, Lead: 0x11, new(new(17, 17), new(17, 18))),
            new(0x9bc31a, Lead: 0x15, new(new(37, 16), new(41, 11))),
            new(0x9bc4ea, Lead: 0x8d, new(new(-12, 12), new(-85, 40))),
            new(0x9bc77a, Lead: 0x5b, new(new(-88, -108), new(96, -91))),
            new(0x9bc8aa, Lead: 0xf0, new(new(-60, -115), new(50, 13))),
            new(0x9bcaba, Lead: 0x00, new(new(-115, 74), new(11, -104))),
            new(0x9bcb8a, Lead: 0x60, new(new(-87, 7), new(0, 34))),
            new(0x9bcbda, Lead: 0x09, new(new(34, -16), new(-84, -112))),
            new(0x9bcbfa, Lead: 0x60, new(new(-1, -1), new(-1, -1))),
            new(0x9bcc6a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bccc2, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bccda, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcd3e, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcd4a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcdba, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bce06, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bce2a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bce9a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcea6, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bceca, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcf22, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bcf9e, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd01a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd072, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd16a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd1ea, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd2ba, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd362, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd40a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd4da, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd55a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd652, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd6aa, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd7ca, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd7fa, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd942, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd96a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bd9ba, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bda0a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdaba, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdada, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdbda, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdc32, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdc52, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdc8a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdcaa, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdce2, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdd02, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdd5a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bddce, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bddea, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bde5e, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdf22, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bdfb2, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9be076, Lead: 0x20, new(new(0, 32), new(0, -96))),
            new(0x9be106, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be2d2, Lead: 0x00, new(new(0, -128), new(0, -128))),
            new(0x9be3e2, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be432, Lead: 0x30, new(new(0, 96), new(0, 64))),
            new(0x9be482, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be4d2, Lead: 0x18, new(new(0, 48), new(0, 80))),
            new(0x9be522, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be572, Lead: 0x00, new(new(0, 0), new(0, 3))),
            new(0x9be5c2, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be632, Lead: 0x02, new(new(0, 28), new(0, 48))),
            new(0x9be722, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9be8f2, Lead: 0x00, new(new(0, 0), new(0, 0))),
            new(0x9beb82, Lead: 0x64, new(new(76, 116), new(76, 52))),
            new(0x9becb2, Lead: 0xc1, new(new(65, -59), new(85, -62))),
            new(0x9beee2, Lead: 0x1d, new(new(31, 51), new(63, 47))),
            new(0x9bf012, Lead: 0x7e, new(new(126, 126), new(0, 2))),
            new(0x9bf1e2, Lead: 0xff, new(new(-127, -1), new(-127, -125))),
            new(0x9bf472, Lead: 0x7e, new(new(126, 126), new(0, 2))),
            new(0x9bf5a2, Lead: 0x36, new(new(63, 31), new(31, 0))),
            new(0x9bf6c2, Lead: 0xe0, new(new(96, -16), new(80, -48))),
            new(0x9bf762, Lead: 0x01, new(new(6, 6), new(15, 11))),
            new(0x9bf8b2, Lead: 0x7e, new(new(126, 126), new(0, 2))),
            new(0x9bf9fa, Lead: 0xa0, new(new(-96, 0), new(0, 0))),
            new(0x9bfa02, Lead: 0xff, new(new(-127, -1), new(-127, -125))),
            new(0x9bfa9a, Lead: 0x00, new(new(-58, 0), new(100, 0))),
            new(0x9bfb52, Lead: 0x00, new(new(0, 0), new(5, 0))),
            new(0x9bfb72, Lead: 0x0e, new(new(14, 14), new(14, 14))),
            new(0x9bfc12, Lead: 0xa0, new(new(96, 112), new(-16, 48))),
            new(0x9bfca2, Lead: 0x01, new(new(6, 6), new(15, 11))),
            new(0x9bfcea, Lead: 0xff, new(new(-127, -1), new(-1, 0))),
            new(0x9bfd72, Lead: 0x0e, new(new(14, 14), new(14, 4))),
            new(0x9bfd8a, Lead: 0x0f, new(new(-1, 12), new(-4, 24))),
            new(0x9bff02, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bff2a, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
            new(0x9bff36, Lead: 0xff, new(new(-1, -1), new(-1, -1))),
        ]);

    /// <summary>
    /// The five bytes one frame's four coordinate word reads touch, from the byte before the frame: the first word's
    /// low byte, which only drives the data bus, then the signed left and right offsets the frame reads.
    /// </summary>
    private readonly record struct ObservationRun(int Start, byte Lead, TrailFrame Frame)
    {
        /// <summary>The five bytes in address order: the lead byte, then each offset's signed byte.</summary>
        internal byte[] Bytes => [Lead, unchecked((byte)Frame.Left.X), unchecked((byte)Frame.Left.Y),
            unchecked((byte)Frame.Right.X), unchecked((byte)Frame.Right.Y)];
    }

    /// <summary>Indexes pinned observation runs byte by byte for native-address lookup.</summary>
    private static FrozenDictionary<int, byte> CreateObservations(ObservationRun[] runs)
    {
        var bytes = new Dictionary<int, byte>();
        foreach (ObservationRun run in runs)
            for (int index = 0; index < run.Bytes.Length; index++)
                bytes.Add(run.Start + index, run.Bytes[index]);
        return bytes.ToFrozenDictionary();
    }

    /// <summary>
    /// One record's left and right sparkle offsets from the projectile. The native record stores
    /// each axis as one signed byte; <see cref="TryReadByte"/> emits the low byte of each word.
    /// </summary>
    private readonly record struct TrailFrame(Offset16<RoomPixels> Left, Offset16<RoomPixels> Right);

    /// <summary>How a block's right sparkle reflects its left sparkle across the travel axis.</summary>
    private enum Reflection
    {
        /// <summary>Negates X while retaining Y for vertical travel.</summary>
        AcrossVertical,
        /// <summary>Negates Y while retaining X for horizontal travel.</summary>
        AcrossHorizontal,
        /// <summary>Swaps X and Y around the down-right travel axis.</summary>
        AcrossDiagonalDownRight,
        /// <summary>Negates and swaps coordinates around the up-right travel axis.</summary>
        AcrossDiagonalUpRight,
        /// <summary>Negates both coordinates through the projectile origin.</summary>
        ThroughOrigin,
    }

    /// <summary>A stored left sparkle and the reflection used for its ordinary right partner.</summary>
    private readonly record struct StoredFrame(Offset16<RoomPixels> Left, Reflection Reflection);

    /// <summary>Non-calculated sparkle records keyed by their native bank-$9B addresses.</summary>
    private static readonly FrozenDictionary<int, StoredFrame> Frames = CreateFrames();

    /// <summary>The four signed bytes of one trail frame record, in record order.</summary>
    private enum RecordField
    {
        /// <summary>First byte: signed horizontal offset of the left sparkle.</summary>
        LeftX,
        /// <summary>Second byte: signed vertical offset of the left sparkle.</summary>
        LeftY,
        /// <summary>Third byte: signed horizontal offset of the right sparkle.</summary>
        RightX,
        /// <summary>Fourth byte: signed vertical offset of the right sparkle.</summary>
        RightY,
    }

    /// <summary>
    /// $9B:A4B3-A4F5: the beam-bit dispatch read by $9B:A418/A42A/A43C.
    /// Ice with Spazer/Plasma selects its spread geometry; plain Wave selects its
    /// oscillating geometry; other combinations share the default placement.
    /// </summary>
    private static int BeamFamily(SamusBeamCombination beams, bool charged) => beams switch
    {
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave =>
            charged ? ChargedBeamTrails_Wave_WaveIce : UnchargedBeamTrails_Wave_WaveIce,
        SamusBeamCombination.SpazerIce =>
            charged ? ChargedBeamTrails_IceSpazer : UnchargedBeamTrails_IceSpazer,
        SamusBeamCombination.SpazerIceWave =>
            charged ? ChargedBeamTrails_WaveIceSpazer : UnchargedBeamTrails_WaveIceSpazer,
        SamusBeamCombination.PlasmaIce =>
            charged ? ChargedBeamTrails_IcePlasma : UnchargedBeamTrails_IcePlasma,
        SamusBeamCombination.PlasmaIceWave =>
            charged ? ChargedBeamTrails_WaveIcePlasma : UnchargedBeamTrails_WaveIcePlasma,
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaWave =>
            charged ? ChargedBeamTrails_Default : UnchargedBeamTrails_Default,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(beams), beams, "The trail tables hold twelve retail rows."),
        _ => throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination."),
    };

    /// <summary>The fourteen bank-$9B direction-pointer tables, one per trail family.</summary>
    private enum TrailFamily
    {
        /// <summary><c>UnchargedBeamTrails_Default</c> direction pointers.</summary>
        UnchargedBeamTrailsDefault = UnchargedBeamTrails_Default,
        /// <summary><c>ChargedBeamTrails_Default</c> direction pointers.</summary>
        ChargedBeamTrailsDefault = ChargedBeamTrails_Default,
        /// <summary><c>UnchargedBeamTrails_Wave_WaveIce</c> direction pointers.</summary>
        UnchargedBeamTrailsWaveWaveIce = UnchargedBeamTrails_Wave_WaveIce,
        /// <summary><c>ChargedBeamTrails_Wave_WaveIce</c> direction pointers.</summary>
        ChargedBeamTrailsWaveWaveIce = ChargedBeamTrails_Wave_WaveIce,
        /// <summary><c>UnchargedBeamTrails_IceSpazer</c> direction pointers.</summary>
        UnchargedBeamTrailsIceSpazer = UnchargedBeamTrails_IceSpazer,
        /// <summary><c>UnchargedBeamTrails_WaveIceSpazer</c> direction pointers.</summary>
        UnchargedBeamTrailsWaveIceSpazer = UnchargedBeamTrails_WaveIceSpazer,
        /// <summary><c>UnchargedBeamTrails_IcePlasma</c> direction pointers.</summary>
        UnchargedBeamTrailsIcePlasma = UnchargedBeamTrails_IcePlasma,
        /// <summary><c>UnchargedBeamTrails_WaveIcePlasma</c> direction pointers.</summary>
        UnchargedBeamTrailsWaveIcePlasma = UnchargedBeamTrails_WaveIcePlasma,
        /// <summary><c>ChargedBeamTrails_IceSpazer</c> direction pointers.</summary>
        ChargedBeamTrailsIceSpazer = ChargedBeamTrails_IceSpazer,
        /// <summary><c>ChargedBeamTrails_WaveIceSpazer</c> direction pointers.</summary>
        ChargedBeamTrailsWaveIceSpazer = ChargedBeamTrails_WaveIceSpazer,
        /// <summary><c>ChargedBeamTrails_IcePlasma</c> direction pointers.</summary>
        ChargedBeamTrailsIcePlasma = ChargedBeamTrails_IcePlasma,
        /// <summary><c>ChargedBeamTrails_WaveIcePlasma</c> direction pointers.</summary>
        ChargedBeamTrailsWaveIcePlasma = ChargedBeamTrails_WaveIcePlasma,
        /// <summary><c>SpazerSBATrail_WaveSpazer</c> direction pointers.</summary>
        SpazerSBATrailWaveSpazer = SpazerSBATrail_WaveSpazer,
        /// <summary><c>UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B</c> direction pointers.</summary>
        UnusedSpazerSBATrailSpazerIceSpazer = UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B,
    }

    /// <summary>
    /// $9B:A4F7-A56E/A98F-AA06/B327-B33A/B37B-B38E: each family has ten
    /// direction pointers. The four-byte X/Y pair records pack a fixed number
    /// of animation frames per direction. Vertical facing duplicates share
    /// one octant; Wave shares opposite axes and default beams share all axes.
    /// </summary>
    private static int DirectionSequence(TrailFamily family, SamusProjectileDirection direction)
    {
        int octant = direction == SamusProjectileDirection.UpFacingLeft ? 0 :
            (int)direction >= (int)SamusProjectileDirection.DownFacingLeft ? (int)direction - 1 : (int)direction;
        int waveAxis = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.DownLeft => 2,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 1,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.UpLeft => 3,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => 0,
            _ => throw new InvalidOperationException($"Undefined SamusProjectileDirection {direction}."),
        };
        return family switch
        {
            TrailFamily.UnchargedBeamTrailsDefault => UnchargedBeamTrails_Default_0,
            TrailFamily.ChargedBeamTrailsDefault => ChargedBeamTrails_Default_0,
            TrailFamily.UnchargedBeamTrailsWaveWaveIce => UnchargedBeamTrails_Wave_WaveIce_0 + waveAxis * 16 * 4,
            TrailFamily.ChargedBeamTrailsWaveWaveIce => ChargedBeamTrails_Wave_WaveIce_0 + waveAxis * 16 * 4,
            TrailFamily.UnchargedBeamTrailsIceSpazer => UnchargedBeamTrails_IceSpazer_0 + IceSpazerDirection(direction) * 3 * 4,
            TrailFamily.UnchargedBeamTrailsWaveIceSpazer => UnchargedBeamTrails_WaveIceSpazer_0 + octant * 10 * 4,
            TrailFamily.UnchargedBeamTrailsIcePlasma => TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, octant),
            TrailFamily.UnchargedBeamTrailsWaveIcePlasma => TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, octant),
            TrailFamily.ChargedBeamTrailsIceSpazer => TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, octant),
            TrailFamily.ChargedBeamTrailsWaveIceSpazer => TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, octant),
            TrailFamily.ChargedBeamTrailsIcePlasma => TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, octant),
            TrailFamily.ChargedBeamTrailsWaveIcePlasma => TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, octant),
            TrailFamily.SpazerSBATrailWaveSpazer => TrailRecord(SpazerSBATrail_WaveSpazerFirst, SpazerSBATrail_WaveSpazerFrames, (int)direction % 5 is > 0 and < 4 ? (int)direction % 5 : 0),
            TrailFamily.UnusedSpazerSBATrailSpazerIceSpazer => UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F +
                (direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft ? 3 * 4 : 0),
            _ => throw new InvalidOperationException($"Undefined {nameof(TrailFamily)} {(int)family:X6}."),
        };
    }

    /// <summary>
    /// Maps Ice-Spazer directions to its unusual layout: two vertical sequences first,
    /// then three nonvertical directions per side.
    /// </summary>
    private static int IceSpazerDirection(SamusProjectileDirection direction) => direction switch
    {
        SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => 0,
        SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => 1,
        SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft => (int)direction < 4 ? (int)direction + 1 : (int)direction - 1,
        _ => throw new InvalidOperationException($"Undefined SamusProjectileDirection {direction}."),
    };

    /// <summary>Decodes beam-family and direction pointer tables at odd bank-$9B addresses.</summary>
    private static bool TryPointer(int address, out ushort pointer)
    {
        pointer = 0;
        if ((address & 1) == 0) return false;
        if (address is >= BeamTrailOffsets_uncharged and < BeamTrailOffsets_spazerSBA)
        {
            bool charged = address >= BeamTrailOffsets_charged;
            int start = charged ? BeamTrailOffsets_charged : BeamTrailOffsets_uncharged;
            pointer = unchecked((ushort)BeamFamily(SamusBeamCombinations.FromTableIndex((address - start) / 2), charged));
            return true;
        }
        if (address is >= BeamTrailOffsets_spazerSBA and < UnchargedBeamTrails_Wave_WaveIce)
        {
            // $9B:A4E3 holds ten rows; only the Spazer rows name a trail.
            SamusBeamCombination beams = SamusBeamCombinations.FromTableIndex((address - BeamTrailOffsets_spazerSBA) / 2);
            pointer = unchecked((ushort)(beams switch
            {
                SamusBeamCombination.Spazer or SamusBeamCombination.SpazerIce => UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B,
                SamusBeamCombination.SpazerWave => SpazerSBATrail_WaveSpazer,
                SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
                    SamusBeamCombination.IceWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
                    SamusBeamCombination.PlasmaWave => 0,
                SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave or SamusBeamCombination.SpazerPlasma or
                    SamusBeamCombination.SpazerPlasmaWave or SamusBeamCombination.SpazerPlasmaIce or
                    SamusBeamCombination.SpazerPlasmaIceWave =>
                    throw new ArgumentOutOfRangeException(nameof(address), address, "The SBA trail table holds ten rows."),
                _ => throw new ArgumentOutOfRangeException(nameof(address), address, "Undefined beam combination."),
            }));
            return true;
        }
        int block = address is >= UnchargedBeamTrails_Wave_WaveIce and < UnchargedBeamTrails_Default_0 ? UnchargedBeamTrails_Wave_WaveIce :
            address is >= ChargedBeamTrails_Default and < ChargedBeamTrails_Default_0 ? ChargedBeamTrails_Default :
            address is >= SpazerSBATrail_WaveSpazer and < SpazerSBATrail_WaveSpazerFirst ? SpazerSBATrail_WaveSpazer :
            address is >= UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B and < UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F ? UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B : 0;
        if (block == 0) return false;
        int offset = address - block;
        int family = block + offset / 20 * 20;
        pointer = unchecked((ushort)DirectionSequence((TrailFamily)family, SamusProjectileDirections.At(offset % 20 / 2)));
        return true;
    }
    /// <summary>
    /// $9B:A56F/AA07 default sequences hold the origin. The uncharged Wave
    /// sequences at $A58F-A68E have a sixteen-frame signed oscillation, mirrored
    /// about each peak. Cardinal ramps start at eight then advance four pixels;
    /// diagonal ramps advance four pixels twice, then two pixels twice.
    /// </summary>
    private static bool TryCalculatedFrame(int address, out TrailFrame value)
    {
        value = default;
        if (SequenceFrame(address, UnchargedBeamTrails_Default_0, 8, out _) ||
            SequenceFrame(address, ChargedBeamTrails_Default_0, 8, out _)) return true;
        if (SequenceFrame(address, UnchargedBeamTrails_Wave_WaveIce_0, 4 * 16, out int wave))
        {
            int axis = wave / 16;
            int phase = wave % 8;
            int ramp = Math.Min(phase, 8 - phase);
            int magnitude = ramp == 0 ? 0 : axis < 2 ? 4 * (ramp + 1) : 4 * Math.Min(ramp, 2) + 2 * Math.Max(0, ramp - 2);
            int signed = wave % 16 < 8 ? magnitude : -magnitude;
            value = axis switch
            {
                0 => Frame(new((short)signed, 0), new(0, 0)),
                1 => Frame(new(0, (short)(-signed)), new(0, 0)),
                2 => Frame(new((short)(-signed), (short)(-signed)), new(0, 0)),
                _ => Frame(new((short)signed, (short)(-signed)), new(0, 0)),
            };
            return true;
        }
        if (SequenceFrame(address, ChargedBeamTrails_Wave_WaveIce_0, 2 * 16, out int chargedWave))
        {
            int phase = chargedWave % 16 / 2;
            int ramp = Math.Min(phase, 8 - phase);
            int spread = ramp == 0 ? 0 : Math.Min(4 * (ramp + 1), 16);
            value = chargedWave < 16 ? Frame(new((short)(-spread), 0), new((short)spread, 0)) : Frame(new(0, (short)(-spread)), new(0, (short)spread));
            return true;
        }
        if (SequenceFrame(address, UnchargedBeamTrails_IceSpazer_0, 8 * 3, out int iceSpazer))
        {
            int direction = iceSpazer / 3;
            int phase = iceSpazer % 3;
            int spread = phase * 8;
            if (direction is 0 or 1)
            {
                int behind = phase == 0 ? 0 : direction == 0 ? 8 : -8;
                value = Frame(new((short)(-spread), (short)behind), new((short)spread, (short)behind));
                return true;
            }
            if (direction is 3 or 6)
            {
                int side = direction == 3 ? -1 : 1;
                value = Frame(new((short)(side * 8), (short)(side * spread)), new((short)(side * 8), (short)(-side * spread)));
                return true;
            }
        }
        if (SequenceFrame(address, UnchargedBeamTrails_WaveIceSpazer_0, 8 * 10, out int waveSpazer) &&
            waveSpazer / 10 % 2 == 0)
        {
            int direction = waveSpazer / 10;
            int phase = waveSpazer % 10;
            int spread = 4 * Math.Min(Math.Min(phase, 10 - phase), 4);
            int behind = phase == 0 ? 0 : 8;
            value = direction switch
            {
                0 => Frame(new((short)(-spread), (short)behind), new((short)spread, (short)behind)),
                2 => Frame(new((short)(-behind), (short)(-spread)), new((short)(-behind), (short)spread)),
                4 => Frame(new((short)(-spread), (short)(-behind)), new((short)spread, (short)(-behind))),
                _ => Frame(new((short)behind, (short)(-spread)), new((short)behind, (short)spread)),
            };
            return true;
        }
        if (SequenceFrame(address, UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F, 2 * 3, out int sba))
        {
            int phase = sba % 3;
            int spread = phase * 8;
            int behind = phase == 0 ? 0 : sba < 3 ? 8 : -8;
            value = Frame(new((short)(-spread), (short)behind), new((short)spread, (short)behind));
            return true;
        }
        return false;
    }

    /// <summary>Each native placement frame packs signed left X/Y and right X/Y bytes.</summary>
    private static bool SequenceFrame(int address, int first, int count, out int frame)
    {
        int offset = address - first;
        frame = offset / 4;
        return offset >= 0 && offset < count * 4 && offset % 4 == 0;
    }

    /// <summary>Pairs the two sparkle offsets in one native four-byte placement record.</summary>
    private static TrailFrame Frame(Offset16<RoomPixels> left, Offset16<RoomPixels> right) => new(left, right);
    /// <summary>Four bytes per placement frame: left X/Y and right X/Y.</summary>
    private const int FrameBytes = 4;
    /// <summary>Locates the record at <paramref name="index"/> in a family of adjacent records that each hold <paramref name="frames"/> placement frames.</summary>
    private static int TrailRecord(int first, int frames, int index) => first + index * frames * FrameBytes;
    /// <summary>
    /// Trail sparkle placements by native record address. Each record pairs a left and a right
    /// sparkle; the right one is the left reflected across the beam's travel axis (the reflection is
    /// fixed per direction block). 668 of the 700 right offsets follow it; the 32 drawn
    /// differently, all in diagonal blocks, are kept in <see cref="DrawnRightOffsets"/>.
    /// </summary>
    private static FrozenDictionary<int, StoredFrame> CreateFrames()
    {
        var result = new Dictionary<int, StoredFrame>();
        void Add(int address, Reflection reflection, ReadOnlySpan<(sbyte X, sbyte Y)> left)
        { for (int i = 0; i < left.Length; i++) result.Add(address + i * 4, new(new(left[i].X, left[i].Y), reflection)); }
        Add(UnchargedBeamTrails_IceSpazer_2, Reflection.AcrossDiagonalUpRight, [(-8, 8), (-14, 2), (-20, -4)]);
        Add(UnchargedBeamTrails_IceSpazer_4, Reflection.AcrossDiagonalDownRight, [(-8, -8), (-2, -16), (4, -20)]);
        Add(UnchargedBeamTrails_IceSpazer_5, Reflection.AcrossDiagonalUpRight, [(8, -8), (14, -2), (20, 4)]);
        Add(UnchargedBeamTrails_IceSpazer_7, Reflection.AcrossDiagonalDownRight, [(8, 8), (2, 16), (-4, 20)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 6), (-14, 2), (-16, 0), (-18, -2), (-20, -4), (-18, -2), (-16, 0), (-14, 2), (-12, 6)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -6), (-2, -16), (-16, 0), (-18, 2), (4, -20), (-18, 2), (-16, 0), (-2, -16), (-12, -6)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (2, -14), (0, -16), (-2, -18), (-2, -20), (-2, -20), (-2, -20), (-2, -18), (0, -16), (2, -14)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (6, 10), (2, 16), (0, 16), (-2, 18), (-4, 20), (-2, 18), (0, 16), (2, 16), (6, 10)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 16)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 12)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (-16, 0)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -12)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, -16)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (12, -12)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (16, 0)]);
        Add(TrailRecord(UnchargedBeamTrails_IcePlasmaFirst, UnchargedBeamTrails_IcePlasmaFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (12, 12)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 16), (-8, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-8, 16)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 12), (-20, 8), (-24, 2), (-24, 0), (-24, 0), (-24, 0), (-24, 2), (-20, 8)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (-16, 0), (-16, -8), (-16, -12), (-16, -16), (-16, -16), (-16, -16), (-16, -12), (-16, -8)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -12), (-18, -6), (-20, -2), (-24, 0), (-24, 0), (-24, 0), (-20, -2), (-18, -6)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, -16), (-8, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-8, -16)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (12, -12), (20, -8), (24, -2), (24, 0), (24, 0), (24, 0), (24, -2), (20, -8)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (16, 0), (16, -8), (16, -12), (16, -16), (16, -16), (16, -16), (16, -12), (16, -8)]);
        Add(TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (12, 12), (18, 6), (20, 2), (24, 0), (24, 0), (24, 0), (20, 2), (18, 6)]);
        Add(ChargedBeamTrails_Wave_WaveIce_2, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-4, -4), (-4, -4), (-8, -8), (-8, -8), (-8, -8), (-8, -8), (-10, -10), (-10, -10), (-8, -8), (-8, -8), (-8, -8), (-8, -8), (-4, -4), (-4, -4)]);
        Add(ChargedBeamTrails_Wave_WaveIce_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-4, 4), (-4, 4), (-8, 8), (-8, 8), (-8, 8), (-8, 8), (-10, 10), (-10, 10), (-8, 8), (-8, 8), (-8, 8), (-8, 8), (-4, 4), (-4, 4)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 8), (0, 8), (0, 16), (0, 16), (-8, 16), (-8, 16), (-16, 16), (-16, 16)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-12, 12), (-12, 12), (-16, 8), (-16, 8), (-24, 0), (-24, 0)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 0), (-16, 0), (-16, -8), (-16, -8), (-16, -16), (-16, -16)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-12, -12), (-12, -12), (-16, -8), (-16, -8), (-24, 0), (-24, 0)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -8), (0, -8), (0, -16), (0, -16), (-8, -16), (-8, -16), (-16, -16), (-16, -16)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (12, -12), (12, -12), (16, -8), (16, -8), (24, 0), (24, 0)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (8, 0), (8, 0), (16, 0), (16, 0), (16, -8), (16, -8), (16, -16), (16, -16)]);
        Add(TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (12, 12), (12, 12), (16, 8), (16, 8), (24, 0), (24, 0)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 8), (0, 8), (0, 16), (0, 16), (-4, 16), (-4, 16), (-8, 16), (-8, 16), (-12, 16), (-12, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-12, 16), (-12, 16), (-8, 16), (-8, 16), (-4, 16), (-4, 16)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-12, 12), (-12, 12), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-8, 0), (-8, 0), (-16, 0), (-16, 0), (-16, -4), (-16, -4), (-16, -8), (-16, -8), (-16, -12), (-16, -12), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -12), (-16, -12), (-16, -8), (-16, -8), (-16, -4), (-16, -4)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-12, -12), (-12, -12), (-16, -8), (-16, -8), (-16, -8), (-16, -8), (-20, -4), (-20, -4), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-20, -4), (-20, -4), (-16, -8), (-16, -8), (-16, -8), (-16, -8)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -8), (0, -8), (0, -16), (0, -16), (-4, -16), (-4, -16), (-8, -16), (-8, -16), (-12, -16), (-12, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-12, -16), (-12, -16), (-8, -16), (-8, -16), (-4, -16), (-4, -16)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (12, -12), (12, -12), (8, -16), (8, -16), (8, -16), (8, -16), (4, -20), (4, -20), (0, -24), (0, -24), (0, -24), (0, -24), (0, -24), (0, -24), (4, -20), (4, -20), (8, -16), (8, -16), (8, -16), (8, -16)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (8, 0), (8, 0), (16, 0), (16, 0), (16, -4), (16, -4), (16, -8), (16, -8), (16, -12), (16, -12), (16, -16), (16, -16), (16, -16), (16, -16), (16, -16), (16, -16), (16, -12), (16, -12), (16, -8), (16, -8), (16, -4), (16, -4)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIceSpazerFirst, ChargedBeamTrails_WaveIceSpazerFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (12, 12), (12, 12), (8, 16), (8, 16), (8, 16), (8, 16), (4, 20), (4, 20), (0, 24), (0, 24), (0, 24), (0, 24), (0, 24), (0, 24), (4, 20), (4, 20), (8, 16), (8, 16), (8, 16), (8, 16)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 12), (0, 12), (0, 24), (0, 24), (0, 28), (0, 28)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 16), (-16, 16), (-24, 24), (-24, 24)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-12, 0), (-12, 0), (-24, 0), (-24, 0), (-28, 0), (-28, 0)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-16, -16), (-16, -16), (-24, -24), (-24, -24)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -12), (0, -12), (0, -24), (0, -24), (0, -28), (0, -28)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (16, -16), (16, -16), (24, -24), (24, -24)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (12, 0), (12, 0), (24, 0), (24, 0), (28, 0), (28, 0)]);
        Add(TrailRecord(ChargedBeamTrails_IcePlasmaFirst, ChargedBeamTrails_IcePlasmaFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (16, 16), (16, 16), (24, 24), (24, 24)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 0), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 12), (0, 12), (0, 24), (0, 24), (0, 28), (0, 28), (-8, 28), (-8, 28), (-12, 28), (-12, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-12, 28), (-12, 28), (-8, 28), (-8, 28)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 16), (-16, 16), (-20, 20), (-20, 20), (-28, 12), (-28, 12), (-32, 12), (-32, 12), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 12), (-32, 12), (-28, 12), (-28, 12)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-12, 0), (-12, 0), (-24, 0), (-24, 0), (-28, 0), (-28, 0), (-28, -8), (-28, -8), (-28, -12), (-28, -12), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -12), (-28, -12), (-28, -8), (-28, -8)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-16, -16), (-16, -16), (-20, -20), (-20, -20), (-24, -16), (-24, -16), (-32, -12), (-32, -12), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -12), (-32, -12), (-24, -16), (-24, -16)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 4), Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -12), (0, -12), (0, -24), (0, -24), (0, -28), (0, -28), (-8, -28), (-8, -28), (-12, -28), (-12, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-12, -28), (-12, -28), (-8, -28), (-8, -28)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5), Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (16, -16), (16, -16), (20, -20), (20, -20), (28, -12), (28, -12), (32, -12), (32, -12), (32, -8), (32, -8), (32, -8), (32, -8), (32, -8), (32, -8), (32, -12), (32, -12), (28, -12), (28, -12)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 6), Reflection.AcrossHorizontal, [(0, 0), (0, 0), (12, 0), (12, 0), (24, 0), (24, 0), (28, 0), (28, 0), (28, -8), (28, -8), (28, -12), (28, -12), (28, -16), (28, -16), (28, -16), (28, -16), (28, -16), (28, -16), (28, -12), (28, -12), (28, -8), (28, -8)]);
        Add(TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 7), Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (16, 16), (16, 16), (20, 20), (20, 20), (24, 16), (24, 16), (32, 12), (32, 12), (32, 8), (32, 8), (32, 8), (32, 8), (32, 8), (32, 8), (32, 12), (32, 12), (24, 16), (24, 16)]);
        Add(TrailRecord(SpazerSBATrail_WaveSpazerFirst, SpazerSBATrail_WaveSpazerFrames, 0), Reflection.AcrossVertical, [(0, 0), (16, 0), (0, 0), (-16, 0)]);
        Add(TrailRecord(SpazerSBATrail_WaveSpazerFirst, SpazerSBATrail_WaveSpazerFrames, 1), Reflection.AcrossDiagonalUpRight, [(0, 0), (-10, -10), (0, 0), (10, 10)]);
        Add(TrailRecord(SpazerSBATrail_WaveSpazerFirst, SpazerSBATrail_WaveSpazerFrames, 2), Reflection.AcrossHorizontal, [(0, 0), (0, -16), (0, 0), (0, 16)]);
        Add(TrailRecord(SpazerSBATrail_WaveSpazerFirst, SpazerSBATrail_WaveSpazerFrames, 3), Reflection.AcrossDiagonalDownRight, [(0, 0), (10, -10), (0, 0), (-10, 10)]);
        return result.ToFrozenDictionary();
    }

    /// <summary>The right sparkles drawn off the reflection rule, by native record address.</summary>
    private static readonly FrozenDictionary<int, Offset16<RoomPixels>> DrawnRightOffsets =
        new Dictionary<int, Offset16<RoomPixels>>
        {
            [UnchargedBeamTrails_IceSpazer_2 + 2 * 4] = new(2, 20),
            [UnchargedBeamTrails_IceSpazer_5 + 2 * 4] = new(-2, -20),
            [UnchargedBeamTrails_WaveIceSpazer_1 + 5 * 4] = new(2, 20),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 4 * 4] = new(20, 4),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 5 * 4] = new(20, 4),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 6 * 4] = new(20, 4),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 1) + 2 * 4] = new(-8, 18),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 1) + 3 * 4] = new(-2, 20),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 1) + 7 * 4] = new(-2, 20),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 1) + 8 * 4] = new(-8, 18),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 5) + 2 * 4] = new(8, -18),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 5) + 3 * 4] = new(2, -20),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 5) + 7 * 4] = new(2, -20),
            [TrailRecord(UnchargedBeamTrails_WaveIcePlasmaFirst, UnchargedBeamTrails_WaveIcePlasmaFrames, 5) + 8 * 4] = new(8, -18),
            [TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 2) + 2 * 4] = new(-8, 8),
            [TrailRecord(ChargedBeamTrails_IceSpazerFirst, ChargedBeamTrails_IceSpazerFrames, 2) + 3 * 4] = new(-8, 8),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 8 * 4] = new(-16, 24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 9 * 4] = new(-16, 24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 10 * 4] = new(-12, 28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 11 * 4] = new(-12, 28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 18 * 4] = new(-12, 28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 19 * 4] = new(-12, 28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 20 * 4] = new(-16, 24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 1) + 21 * 4] = new(-16, 24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 8 * 4] = new(16, -24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 9 * 4] = new(16, -24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 10 * 4] = new(12, -28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 11 * 4] = new(12, -28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 18 * 4] = new(12, -28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 19 * 4] = new(12, -28),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 20 * 4] = new(16, -24),
            [TrailRecord(ChargedBeamTrails_WaveIcePlasmaFirst, ChargedBeamTrails_WaveIcePlasmaFrames, 5) + 21 * 4] = new(16, -24),
        }.ToFrozenDictionary();

    /// <summary>Reconstructs a stored right sparkle from its reflection or a drawn exception.</summary>
    private static bool TryStoredFrame(int address, out TrailFrame frame)
    {
        if (!Frames.TryGetValue(address, out StoredFrame stored))
        {
            frame = default;
            return false;
        }
        Offset16<RoomPixels> right = DrawnRightOffsets.TryGetValue(address, out Offset16<RoomPixels> drawn)
            ? drawn : Reflect(stored.Reflection, stored.Left);
        frame = new(stored.Left, right);
        return true;
    }

    /// <summary>Mirrors a left sparkle across the travel axis chosen by its direction block.</summary>
    private static Offset16<RoomPixels> Reflect(Reflection reflection, Offset16<RoomPixels> left) => reflection switch
    {
        Reflection.AcrossVertical => left.MirroredX(),
        Reflection.AcrossHorizontal => new(left.X, unchecked((short)-left.Y)),
        Reflection.AcrossDiagonalDownRight => new(left.Y, left.X),
        Reflection.AcrossDiagonalUpRight => new(unchecked((short)-left.Y), unchecked((short)-left.X)),
        Reflection.ThroughOrigin => -left,
        _ => throw new ArgumentOutOfRangeException(nameof(reflection)),
    };

    /// <summary>
    /// Reads compiled pointer, coordinate, or reachable adjacent-code bytes by native address. The adjacent bytes are
    /// cartridge observations past the authored coordinate data, which <paramref name="overreads"/> holds.
    /// </summary>
    internal static bool TryReadByte(int address, SamusOverreadCatalog? overreads, out byte value)
    {
        if (address == EmptyFamilyWrappedCoordinateByte)
        {
            value = 0xff;
            return true;
        }

        if (address is >= ReachableAdjacentCodeStart and <= ReachableAdjacentCodeEnd)
        {
            value = (overreads ?? throw new InvalidOperationException(
                    "A trail reading past the coordinate data needs the installed Samus overread catalog."))
                .TrailCoordinateAdjacentByte(address - ReachableAdjacentCodeStart);
            return true;
        }

        // Native pointer tables start on odd bytes. Every coordinate record has
        // the same four-byte alignment as the first default frame, even across
        // intervening pointer tables. Family ranges exclude those gaps.
        int pointerAddress = address % 2 == 1 ? address : address - 1;
        if (TryPointer(pointerAddress, out ushort pointer))
        { value = (byte)(pointer >> ((address - pointerAddress) * 8)); return true; }
        int offsetInRecord = (address - UnchargedBeamTrails_Default_0) & 3;
        int recordAddress = address - offsetInRecord;
        if (TryCalculatedFrame(recordAddress, out TrailFrame frame) || TryStoredFrame(recordAddress, out frame))
        {
            value = unchecked((byte)((RecordField)offsetInRecord switch
            {
                RecordField.LeftX => frame.Left.X,
                RecordField.LeftY => frame.Left.Y,
                RecordField.RightX => frame.Right.X,
                RecordField.RightY => frame.Right.Y,
                var field => throw new InvalidOperationException($"Undefined trail record field {field}."),
            }));
            return true;
        }
        if (ReflectedListStartObservations.TryGetValue(address, out value))
            return true;
        value = 0; return false;
    }

    /// <summary>Reads a compiled little-endian word with bank-local high-byte progression.</summary>
    internal static ushort ReadCompiledWord(int address, SamusOverreadCatalog? overreads)
    {
        int next = SnesAddressMath.AddWithinBank(address, 1);
        if (TryReadByte(address, overreads, out byte low) && TryReadByte(next, overreads, out byte high))
            return SnesLittleEndian.Word(low, high);

        throw new InvalidDataException(
            $"Projectile trail pointer word ${address:X6} is outside the compiled coordinate catalog.");
    }

    /// <summary>Reads one beam-family selector from the compiled high-bank pointer catalog.</summary>
    internal static ushort ReadFamilyPointer(int address, SamusOverreadCatalog? overreads) => ReadCompiledWord(address, overreads);

    /// <summary>Reads a direction-list pointer from compiled data; false for an empty family's low-half pointer.</summary>
    internal static bool TryReadDirectionPointer(int address, SamusOverreadCatalog? overreads, out ushort pointer)
    {
        int next = SnesAddressMath.AddWithinBank(address, 1);
        bool compiled = TryReadByte(address, overreads, out byte low) & TryReadByte(next, overreads, out byte high);
        pointer = compiled ? SnesLittleEndian.Word(low, high) : (ushort)0;
        return compiled;
    }

    /// <summary>
    /// Reads both signed coordinate bytes of an absolute-indexed word from compiled data; false when either byte lies
    /// outside it (an empty family's operand straddling into the following bank or an undriven window).
    /// </summary>
    internal static bool TryReadCoordinateWord(ushort operand, ushort index, SamusOverreadCatalog? overreads, out ushort word)
    {
        int address = (SamusProjectileRomData.Banks.PaletteAndTrailData + operand + index) & SnesCpuAddressLayout.AddressMask;
        int next = (address + 1) & SnesCpuAddressLayout.AddressMask;
        bool compiled = TryReadByte(address, overreads, out byte low) & TryReadByte(next, overreads, out byte high);
        word = compiled ? SnesLittleEndian.Word(low, high) : (ushort)0;
        return compiled;
    }
}
