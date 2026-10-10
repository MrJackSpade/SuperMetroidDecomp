using System.Collections.Frozen;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    /// <summary>$9BA82F: UnchargedBeamTrails_IcePlasma_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_0 = 0x9ba82f;
    /// <summary>$9BA837: UnchargedBeamTrails_IcePlasma_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_1 = 0x9ba837;
    /// <summary>$9BA83F: UnchargedBeamTrails_IcePlasma_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_2 = 0x9ba83f;
    /// <summary>$9BA847: UnchargedBeamTrails_IcePlasma_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_3 = 0x9ba847;
    /// <summary>$9BA84F: UnchargedBeamTrails_IcePlasma_4, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_4 = 0x9ba84f;
    /// <summary>$9BA857: UnchargedBeamTrails_IcePlasma_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_5 = 0x9ba857;
    /// <summary>$9BA85F: UnchargedBeamTrails_IcePlasma_6, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_6 = 0x9ba85f;
    /// <summary>$9BA867: UnchargedBeamTrails_IcePlasma_7, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IcePlasma_7 = 0x9ba867;
    /// <summary>$9BA86F: UnchargedBeamTrails_WaveIcePlasma_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_0 = 0x9ba86f;
    /// <summary>$9BA893: UnchargedBeamTrails_WaveIcePlasma_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_1 = 0x9ba893;
    /// <summary>$9BA8B7: UnchargedBeamTrails_WaveIcePlasma_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_2 = 0x9ba8b7;
    /// <summary>$9BA8DB: UnchargedBeamTrails_WaveIcePlasma_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_3 = 0x9ba8db;
    /// <summary>$9BA8FF: UnchargedBeamTrails_WaveIcePlasma_4, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_4 = 0x9ba8ff;
    /// <summary>$9BA923: UnchargedBeamTrails_WaveIcePlasma_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_5 = 0x9ba923;
    /// <summary>$9BA947: UnchargedBeamTrails_WaveIcePlasma_6, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_6 = 0x9ba947;
    /// <summary>$9BA96B: UnchargedBeamTrails_WaveIcePlasma_7, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIcePlasma_7 = 0x9ba96b;
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
    /// <summary>$9BAB27: ChargedBeamTrails_IceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_0 = 0x9bab27;
    /// <summary>$9BAB4F: ChargedBeamTrails_IceSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_1 = 0x9bab4f;
    /// <summary>$9BAB77: ChargedBeamTrails_IceSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_2 = 0x9bab77;
    /// <summary>$9BAB9F: ChargedBeamTrails_IceSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_3 = 0x9bab9f;
    /// <summary>$9BABC7: ChargedBeamTrails_IceSpazer_4, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_4 = 0x9babc7;
    /// <summary>$9BABEF: ChargedBeamTrails_IceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_5 = 0x9babef;
    /// <summary>$9BAC17: ChargedBeamTrails_IceSpazer_6, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_6 = 0x9bac17;
    /// <summary>$9BAC3F: ChargedBeamTrails_IceSpazer_7, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IceSpazer_7 = 0x9bac3f;
    /// <summary>$9BAC67: ChargedBeamTrails_WaveIceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_0 = 0x9bac67;
    /// <summary>$9BACC7: ChargedBeamTrails_WaveIceSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_1 = 0x9bacc7;
    /// <summary>$9BAD27: ChargedBeamTrails_WaveIceSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_2 = 0x9bad27;
    /// <summary>$9BAD87: ChargedBeamTrails_WaveIceSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_3 = 0x9bad87;
    /// <summary>$9BADE7: ChargedBeamTrails_WaveIceSpazer_4, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_4 = 0x9bade7;
    /// <summary>$9BAE47: ChargedBeamTrails_WaveIceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_5 = 0x9bae47;
    /// <summary>$9BAEA7: ChargedBeamTrails_WaveIceSpazer_6, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_6 = 0x9baea7;
    /// <summary>$9BAF07: ChargedBeamTrails_WaveIceSpazer_7, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIceSpazer_7 = 0x9baf07;
    /// <summary>$9BAF67: ChargedBeamTrails_IcePlasma_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_0 = 0x9baf67;
    /// <summary>$9BAF87: ChargedBeamTrails_IcePlasma_1, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_1 = 0x9baf87;
    /// <summary>$9BAFA7: ChargedBeamTrails_IcePlasma_2, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_2 = 0x9bafa7;
    /// <summary>$9BAFC7: ChargedBeamTrails_IcePlasma_3, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_3 = 0x9bafc7;
    /// <summary>$9BAFE7: ChargedBeamTrails_IcePlasma_4, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_4 = 0x9bafe7;
    /// <summary>$9BB007: ChargedBeamTrails_IcePlasma_5, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_5 = 0x9bb007;
    /// <summary>$9BB027: ChargedBeamTrails_IcePlasma_6, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_6 = 0x9bb027;
    /// <summary>$9BB047: ChargedBeamTrails_IcePlasma_7, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_IcePlasma_7 = 0x9bb047;
    /// <summary>$9BB067: ChargedBeamTrails_WaveIcePlasma_0, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_0 = 0x9bb067;
    /// <summary>$9BB0BF: ChargedBeamTrails_WaveIcePlasma_1, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_1 = 0x9bb0bf;
    /// <summary>$9BB117: ChargedBeamTrails_WaveIcePlasma_2, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_2 = 0x9bb117;
    /// <summary>$9BB16F: ChargedBeamTrails_WaveIcePlasma_3, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_3 = 0x9bb16f;
    /// <summary>$9BB1C7: ChargedBeamTrails_WaveIcePlasma_4, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_4 = 0x9bb1c7;
    /// <summary>$9BB21F: ChargedBeamTrails_WaveIcePlasma_5, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_5 = 0x9bb21f;
    /// <summary>$9BB277: ChargedBeamTrails_WaveIcePlasma_6, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_6 = 0x9bb277;
    /// <summary>$9BB2CF: ChargedBeamTrails_WaveIcePlasma_7, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_WaveIcePlasma_7 = 0x9bb2cf;
    /// <summary>$9BB327: SpazerSBATrail_WaveSpazer, pointer selections.</summary>
    private const int SpazerSBATrail_WaveSpazer = 0x9bb327;
    /// <summary>$9BB33B: SpazerSBATrail_WaveSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int SpazerSBATrail_WaveSpazer_0 = 0x9bb33b;
    /// <summary>$9BB34B: SpazerSBATrail_WaveSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int SpazerSBATrail_WaveSpazer_1 = 0x9bb34b;
    /// <summary>$9BB35B: SpazerSBATrail_WaveSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int SpazerSBATrail_WaveSpazer_2 = 0x9bb35b;
    /// <summary>$9BB36B: SpazerSBATrail_WaveSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int SpazerSBATrail_WaveSpazer_3 = 0x9bb36b;
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
    /// Exact cartridge observations after the authored trail-coordinate data. These are
    /// physical results of the bounded saved-state type/frame domain, not executable code.
    /// </summary>
    private static ReadOnlySpan<byte> ReachableAdjacentCode =>
    [
        0x08, 0x8b, 0x4b, 0xab, 0xc2, 0x30, 0xad, 0x1f,
        0x0a, 0x29, 0xff, 0x00, 0x48, 0xc9, 0x03, 0x00,
        0xd0, 0x07, 0xa9, 0x32, 0x00, 0x22, 0x49, 0x90,
        0x80, 0x68, 0xaa, 0xbd,
    ];
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
            new(0x9ba56e, [0xa8, 0x00, 0x00, 0x00, 0x00]),
            new(0x9ba58e, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9baa26, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9bc0da, [0x6b, 0x00, 0x00, 0x7c, 0x08]),
            new(0x9bc10a, [0x00, 0xe0, 0x00, 0x00, 0x00]),
            new(0x9bc13a, [0x02, 0x00, 0x00, 0x00, 0x06]),
            new(0x9bc16a, [0x06, 0x00, 0x01, 0x00, 0xec]),
            new(0x9bc19a, [0x02, 0x00, 0x13, 0x00, 0x14]),
            new(0x9bc1ca, [0x01, 0x01, 0x01, 0x01, 0x02]),
            new(0x9bc1fa, [0x07, 0x07, 0x07, 0x07, 0x08]),
            new(0x9bc24a, [0x11, 0x11, 0x11, 0x11, 0x12]),
            new(0x9bc31a, [0x15, 0x25, 0x10, 0x29, 0x0b]),
            new(0x9bc4ea, [0x8d, 0xf4, 0x0c, 0xab, 0x28]),
            new(0x9bc77a, [0x5b, 0xa8, 0x94, 0x60, 0xa5]),
            new(0x9bc8aa, [0xf0, 0xc4, 0x8d, 0x32, 0x0d]),
            new(0x9bcaba, [0x00, 0x8d, 0x4a, 0x0b, 0x98]),
            new(0x9bcb8a, [0x60, 0xa9, 0x07, 0x00, 0x22]),
            new(0x9bcbda, [0x09, 0x22, 0xf0, 0xac, 0x90]),
            new(0x9bcbfa, [0x60, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcc6a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bccc2, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bccda, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcd3e, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcd4a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcdba, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bce06, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bce2a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bce9a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcea6, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bceca, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcf22, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bcf9e, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd01a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd072, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd16a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd1ea, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd2ba, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd362, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd40a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd4da, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd55a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd652, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd6aa, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd7ca, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd7fa, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd942, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd96a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bd9ba, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bda0a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdaba, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdada, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdbda, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdc32, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdc52, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdc8a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdcaa, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdce2, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdd02, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdd5a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bddce, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bddea, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bde5e, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdf22, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bdfb2, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9be076, [0x20, 0x00, 0x20, 0x00, 0xa0]),
            new(0x9be106, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be2d2, [0x00, 0x00, 0x80, 0x00, 0x80]),
            new(0x9be3e2, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be432, [0x30, 0x00, 0x60, 0x00, 0x40]),
            new(0x9be482, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be4d2, [0x18, 0x00, 0x30, 0x00, 0x50]),
            new(0x9be522, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be572, [0x00, 0x00, 0x00, 0x00, 0x03]),
            new(0x9be5c2, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be632, [0x02, 0x00, 0x1c, 0x00, 0x30]),
            new(0x9be722, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9be8f2, [0x00, 0x00, 0x00, 0x00, 0x00]),
            new(0x9beb82, [0x64, 0x4c, 0x74, 0x4c, 0x34]),
            new(0x9becb2, [0xc1, 0x41, 0xc5, 0x55, 0xc2]),
            new(0x9beee2, [0x1d, 0x1f, 0x33, 0x3f, 0x2f]),
            new(0x9bf012, [0x7e, 0x7e, 0x7e, 0x00, 0x02]),
            new(0x9bf1e2, [0xff, 0x81, 0xff, 0x81, 0x83]),
            new(0x9bf472, [0x7e, 0x7e, 0x7e, 0x00, 0x02]),
            new(0x9bf5a2, [0x36, 0x3f, 0x1f, 0x1f, 0x00]),
            new(0x9bf6c2, [0xe0, 0x60, 0xf0, 0x50, 0xd0]),
            new(0x9bf762, [0x01, 0x06, 0x06, 0x0f, 0x0b]),
            new(0x9bf8b2, [0x7e, 0x7e, 0x7e, 0x00, 0x02]),
            new(0x9bf9fa, [0xa0, 0xa0, 0x00, 0x00, 0x00]),
            new(0x9bfa02, [0xff, 0x81, 0xff, 0x81, 0x83]),
            new(0x9bfa9a, [0x00, 0xc6, 0x00, 0x64, 0x00]),
            new(0x9bfb52, [0x00, 0x00, 0x00, 0x05, 0x00]),
            new(0x9bfb72, [0x0e, 0x0e, 0x0e, 0x0e, 0x0e]),
            new(0x9bfc12, [0xa0, 0x60, 0x70, 0xf0, 0x30]),
            new(0x9bfca2, [0x01, 0x06, 0x06, 0x0f, 0x0b]),
            new(0x9bfcea, [0xff, 0x81, 0xff, 0xff, 0x00]),
            new(0x9bfd72, [0x0e, 0x0e, 0x0e, 0x0e, 0x04]),
            new(0x9bfd8a, [0x0f, 0xff, 0x0c, 0xfc, 0x18]),
            new(0x9bff02, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bff2a, [0xff, 0xff, 0xff, 0xff, 0xff]),
            new(0x9bff36, [0xff, 0xff, 0xff, 0xff, 0xff]),
        ]);

    private readonly record struct ObservationRun(int Start, byte[] Bytes);

    private static FrozenDictionary<int, byte> CreateObservations(ObservationRun[] runs)
    {
        var bytes = new Dictionary<int, byte>();
        foreach (ObservationRun run in runs)
            for (int index = 0; index < run.Bytes.Length; index++)
                bytes.Add(run.Start + index, run.Bytes[index]);
        return bytes.ToFrozenDictionary();
    }

    private readonly record struct Offset(sbyte LeftX, sbyte LeftY, sbyte RightX, sbyte RightY);

    /// <summary>How a block's right sparkle reflects its left sparkle across the travel axis.</summary>
    private enum Reflection { AcrossVertical, AcrossHorizontal, AcrossDiagonalDownRight, AcrossDiagonalUpRight, ThroughOrigin }

    private readonly record struct StoredFrame(sbyte LeftX, sbyte LeftY, Reflection Reflection);

    private static readonly FrozenDictionary<int, StoredFrame> Frames = CreateFrames();

    /// <summary>The four signed bytes of one trail frame record, in record order.</summary>
    private enum RecordField { LeftX, LeftY, RightX, RightY }

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

    /// <summary>
    /// $9B:A4F7-A56E/A98F-AA06/B327-B33A/B37B-B38E: each family has ten
    /// direction pointers. The four-byte X/Y pair records pack a fixed number
    /// of animation frames per direction. Vertical facing duplicates share
    /// one octant; Wave shares opposite axes and default beams share all axes.
    /// </summary>
    private static int DirectionSequence(int family, SamusProjectileDirection direction)
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
            UnchargedBeamTrails_Default => UnchargedBeamTrails_Default_0,
            ChargedBeamTrails_Default => ChargedBeamTrails_Default_0,
            UnchargedBeamTrails_Wave_WaveIce => UnchargedBeamTrails_Wave_WaveIce_0 + waveAxis * 16 * 4,
            ChargedBeamTrails_Wave_WaveIce => ChargedBeamTrails_Wave_WaveIce_0 + waveAxis * 16 * 4,
            UnchargedBeamTrails_IceSpazer => UnchargedBeamTrails_IceSpazer_0 + IceSpazerDirection(direction) * 3 * 4,
            UnchargedBeamTrails_WaveIceSpazer => UnchargedBeamTrails_WaveIceSpazer_0 + octant * 10 * 4,
            UnchargedBeamTrails_IcePlasma => UnchargedBeamTrails_IcePlasma_0 + octant * 2 * 4,
            UnchargedBeamTrails_WaveIcePlasma => UnchargedBeamTrails_WaveIcePlasma_0 + octant * 9 * 4,
            ChargedBeamTrails_IceSpazer => ChargedBeamTrails_IceSpazer_0 + octant * 10 * 4,
            ChargedBeamTrails_WaveIceSpazer => ChargedBeamTrails_WaveIceSpazer_0 + octant * 24 * 4,
            ChargedBeamTrails_IcePlasma => ChargedBeamTrails_IcePlasma_0 + octant * 8 * 4,
            ChargedBeamTrails_WaveIcePlasma => ChargedBeamTrails_WaveIcePlasma_0 + octant * 22 * 4,
            SpazerSBATrail_WaveSpazer => SpazerSBATrail_WaveSpazer_0 +
                ((int)direction % 5 is > 0 and < 4 ? (int)direction % 5 : 0) * 4 * 4,
            UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B => UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F +
                (direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft ? 3 * 4 : 0),
            _ => throw new InvalidOperationException("Unknown projectile trail direction family."),
        };
    }

    // This family packs its two vertical sequences first, followed by each side's
    // three nonvertical directions; the other spread families pack octants in order.
    private static int IceSpazerDirection(SamusProjectileDirection direction) => direction switch
    {
        SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft => 0,
        SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft => 1,
        SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft => (int)direction < 4 ? (int)direction + 1 : (int)direction - 1,
        _ => throw new InvalidOperationException($"Undefined SamusProjectileDirection {direction}."),
    };

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
            address is >= SpazerSBATrail_WaveSpazer and < SpazerSBATrail_WaveSpazer_0 ? SpazerSBATrail_WaveSpazer :
            address is >= UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B and < UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F ? UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B : 0;
        if (block == 0) return false;
        int offset = address - block;
        int family = block + offset / 20 * 20;
        pointer = unchecked((ushort)DirectionSequence(family, (SamusProjectileDirection)(offset % 20 / 2)));
        return true;
    }
    /// <summary>
    /// $9B:A56F/AA07 default sequences hold the origin. The uncharged Wave
    /// sequences at $A58F-A68E have a sixteen-frame signed oscillation, mirrored
    /// about each peak. Cardinal ramps start at eight then advance four pixels;
    /// diagonal ramps advance four pixels twice, then two pixels twice.
    /// </summary>
    private static bool TryCalculatedFrame(int address, out Offset value)
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
                0 => Coordinates(signed, 0, 0, 0),
                1 => Coordinates(0, -signed, 0, 0),
                2 => Coordinates(-signed, -signed, 0, 0),
                _ => Coordinates(signed, -signed, 0, 0),
            };
            return true;
        }
        if (SequenceFrame(address, ChargedBeamTrails_Wave_WaveIce_0, 2 * 16, out int chargedWave))
        {
            int phase = chargedWave % 16 / 2;
            int ramp = Math.Min(phase, 8 - phase);
            int spread = ramp == 0 ? 0 : Math.Min(4 * (ramp + 1), 16);
            value = chargedWave < 16 ? Coordinates(-spread, 0, spread, 0) : Coordinates(0, -spread, 0, spread);
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
                value = Coordinates(-spread, behind, spread, behind);
                return true;
            }
            if (direction is 3 or 6)
            {
                int side = direction == 3 ? -1 : 1;
                value = Coordinates(side * 8, side * spread, side * 8, -side * spread);
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
                0 => Coordinates(-spread, behind, spread, behind),
                2 => Coordinates(-behind, -spread, -behind, spread),
                4 => Coordinates(-spread, -behind, spread, -behind),
                _ => Coordinates(behind, -spread, behind, spread),
            };
            return true;
        }
        if (SequenceFrame(address, UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F, 2 * 3, out int sba))
        {
            int phase = sba % 3;
            int spread = phase * 8;
            int behind = phase == 0 ? 0 : sba < 3 ? 8 : -8;
            value = Coordinates(-spread, behind, spread, behind);
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

    private static Offset Coordinates(int leftX, int leftY, int rightX, int rightY) =>
        new((sbyte)leftX, (sbyte)leftY, (sbyte)rightX, (sbyte)rightY);
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
        { for (int i = 0; i < left.Length; i++) result.Add(address + i * 4, new(left[i].X, left[i].Y, reflection)); }
        Add(UnchargedBeamTrails_IceSpazer_2, Reflection.AcrossDiagonalUpRight, [(-8, 8), (-14, 2), (-20, -4)]);
        Add(UnchargedBeamTrails_IceSpazer_4, Reflection.AcrossDiagonalDownRight, [(-8, -8), (-2, -16), (4, -20)]);
        Add(UnchargedBeamTrails_IceSpazer_5, Reflection.AcrossDiagonalUpRight, [(8, -8), (14, -2), (20, 4)]);
        Add(UnchargedBeamTrails_IceSpazer_7, Reflection.AcrossDiagonalDownRight, [(8, 8), (2, 16), (-4, 20)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 6), (-14, 2), (-16, 0), (-18, -2), (-20, -4), (-18, -2), (-16, 0), (-14, 2), (-12, 6)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -6), (-2, -16), (-16, 0), (-18, 2), (4, -20), (-18, 2), (-16, 0), (-2, -16), (-12, -6)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (2, -14), (0, -16), (-2, -18), (-2, -20), (-2, -20), (-2, -20), (-2, -18), (0, -16), (2, -14)]);
        Add(UnchargedBeamTrails_WaveIceSpazer_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (6, 10), (2, 16), (0, 16), (-2, 18), (-4, 20), (-2, 18), (0, 16), (2, 16), (6, 10)]);
        Add(UnchargedBeamTrails_IcePlasma_0, Reflection.AcrossVertical, [(0, 0), (0, 16)]);
        Add(UnchargedBeamTrails_IcePlasma_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 12)]);
        Add(UnchargedBeamTrails_IcePlasma_2, Reflection.AcrossHorizontal, [(0, 0), (-16, 0)]);
        Add(UnchargedBeamTrails_IcePlasma_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -12)]);
        Add(UnchargedBeamTrails_IcePlasma_4, Reflection.AcrossVertical, [(0, 0), (0, -16)]);
        Add(UnchargedBeamTrails_IcePlasma_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (12, -12)]);
        Add(UnchargedBeamTrails_IcePlasma_6, Reflection.AcrossHorizontal, [(0, 0), (16, 0)]);
        Add(UnchargedBeamTrails_IcePlasma_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (12, 12)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_0, Reflection.AcrossVertical, [(0, 0), (0, 16), (-8, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-8, 16)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (-12, 12), (-20, 8), (-24, 2), (-24, 0), (-24, 0), (-24, 0), (-24, 2), (-20, 8)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_2, Reflection.AcrossHorizontal, [(0, 0), (-16, 0), (-16, -8), (-16, -12), (-16, -16), (-16, -16), (-16, -16), (-16, -12), (-16, -8)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (-12, -12), (-18, -6), (-20, -2), (-24, 0), (-24, 0), (-24, 0), (-20, -2), (-18, -6)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_4, Reflection.AcrossVertical, [(0, 0), (0, -16), (-8, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-8, -16)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (12, -12), (20, -8), (24, -2), (24, 0), (24, 0), (24, 0), (24, -2), (20, -8)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_6, Reflection.AcrossHorizontal, [(0, 0), (16, 0), (16, -8), (16, -12), (16, -16), (16, -16), (16, -16), (16, -12), (16, -8)]);
        Add(UnchargedBeamTrails_WaveIcePlasma_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (12, 12), (18, 6), (20, 2), (24, 0), (24, 0), (24, 0), (20, 2), (18, 6)]);
        Add(ChargedBeamTrails_Wave_WaveIce_2, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-4, -4), (-4, -4), (-8, -8), (-8, -8), (-8, -8), (-8, -8), (-10, -10), (-10, -10), (-8, -8), (-8, -8), (-8, -8), (-8, -8), (-4, -4), (-4, -4)]);
        Add(ChargedBeamTrails_Wave_WaveIce_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-4, 4), (-4, 4), (-8, 8), (-8, 8), (-8, 8), (-8, 8), (-10, 10), (-10, 10), (-8, 8), (-8, 8), (-8, 8), (-8, 8), (-4, 4), (-4, 4)]);
        Add(ChargedBeamTrails_IceSpazer_0, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 8), (0, 8), (0, 16), (0, 16), (-8, 16), (-8, 16), (-16, 16), (-16, 16)]);
        Add(ChargedBeamTrails_IceSpazer_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-12, 12), (-12, 12), (-16, 8), (-16, 8), (-24, 0), (-24, 0)]);
        Add(ChargedBeamTrails_IceSpazer_2, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 0), (-16, 0), (-16, -8), (-16, -8), (-16, -16), (-16, -16)]);
        Add(ChargedBeamTrails_IceSpazer_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-12, -12), (-12, -12), (-16, -8), (-16, -8), (-24, 0), (-24, 0)]);
        Add(ChargedBeamTrails_IceSpazer_4, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -8), (0, -8), (0, -16), (0, -16), (-8, -16), (-8, -16), (-16, -16), (-16, -16)]);
        Add(ChargedBeamTrails_IceSpazer_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (12, -12), (12, -12), (16, -8), (16, -8), (24, 0), (24, 0)]);
        Add(ChargedBeamTrails_IceSpazer_6, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (8, 0), (8, 0), (16, 0), (16, 0), (16, -8), (16, -8), (16, -16), (16, -16)]);
        Add(ChargedBeamTrails_IceSpazer_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (12, 12), (12, 12), (16, 8), (16, 8), (24, 0), (24, 0)]);
        Add(ChargedBeamTrails_WaveIceSpazer_0, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 8), (0, 8), (0, 16), (0, 16), (-4, 16), (-4, 16), (-8, 16), (-8, 16), (-12, 16), (-12, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-16, 16), (-12, 16), (-12, 16), (-8, 16), (-8, 16), (-4, 16), (-4, 16)]);
        Add(ChargedBeamTrails_WaveIceSpazer_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-12, 12), (-12, 12), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8), (-16, 8)]);
        Add(ChargedBeamTrails_WaveIceSpazer_2, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-8, 0), (-8, 0), (-16, 0), (-16, 0), (-16, -4), (-16, -4), (-16, -8), (-16, -8), (-16, -12), (-16, -12), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -12), (-16, -12), (-16, -8), (-16, -8), (-16, -4), (-16, -4)]);
        Add(ChargedBeamTrails_WaveIceSpazer_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-12, -12), (-12, -12), (-16, -8), (-16, -8), (-16, -8), (-16, -8), (-20, -4), (-20, -4), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-24, 0), (-20, -4), (-20, -4), (-16, -8), (-16, -8), (-16, -8), (-16, -8)]);
        Add(ChargedBeamTrails_WaveIceSpazer_4, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -8), (0, -8), (0, -16), (0, -16), (-4, -16), (-4, -16), (-8, -16), (-8, -16), (-12, -16), (-12, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-16, -16), (-12, -16), (-12, -16), (-8, -16), (-8, -16), (-4, -16), (-4, -16)]);
        Add(ChargedBeamTrails_WaveIceSpazer_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (12, -12), (12, -12), (8, -16), (8, -16), (8, -16), (8, -16), (4, -20), (4, -20), (0, -24), (0, -24), (0, -24), (0, -24), (0, -24), (0, -24), (4, -20), (4, -20), (8, -16), (8, -16), (8, -16), (8, -16)]);
        Add(ChargedBeamTrails_WaveIceSpazer_6, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (8, 0), (8, 0), (16, 0), (16, 0), (16, -4), (16, -4), (16, -8), (16, -8), (16, -12), (16, -12), (16, -16), (16, -16), (16, -16), (16, -16), (16, -16), (16, -16), (16, -12), (16, -12), (16, -8), (16, -8), (16, -4), (16, -4)]);
        Add(ChargedBeamTrails_WaveIceSpazer_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (12, 12), (12, 12), (8, 16), (8, 16), (8, 16), (8, 16), (4, 20), (4, 20), (0, 24), (0, 24), (0, 24), (0, 24), (0, 24), (0, 24), (4, 20), (4, 20), (8, 16), (8, 16), (8, 16), (8, 16)]);
        Add(ChargedBeamTrails_IcePlasma_0, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 12), (0, 12), (0, 24), (0, 24), (0, 28), (0, 28)]);
        Add(ChargedBeamTrails_IcePlasma_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 16), (-16, 16), (-24, 24), (-24, 24)]);
        Add(ChargedBeamTrails_IcePlasma_2, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-12, 0), (-12, 0), (-24, 0), (-24, 0), (-28, 0), (-28, 0)]);
        Add(ChargedBeamTrails_IcePlasma_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-16, -16), (-16, -16), (-24, -24), (-24, -24)]);
        Add(ChargedBeamTrails_IcePlasma_4, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -12), (0, -12), (0, -24), (0, -24), (0, -28), (0, -28)]);
        Add(ChargedBeamTrails_IcePlasma_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (16, -16), (16, -16), (24, -24), (24, -24)]);
        Add(ChargedBeamTrails_IcePlasma_6, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (12, 0), (12, 0), (24, 0), (24, 0), (28, 0), (28, 0)]);
        Add(ChargedBeamTrails_IcePlasma_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (16, 16), (16, 16), (24, 24), (24, 24)]);
        Add(ChargedBeamTrails_WaveIcePlasma_0, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, 12), (0, 12), (0, 24), (0, 24), (0, 28), (0, 28), (-8, 28), (-8, 28), (-12, 28), (-12, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-16, 28), (-12, 28), (-12, 28), (-8, 28), (-8, 28)]);
        Add(ChargedBeamTrails_WaveIcePlasma_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (-8, 8), (-8, 8), (-16, 16), (-16, 16), (-20, 20), (-20, 20), (-28, 12), (-28, 12), (-32, 12), (-32, 12), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 8), (-32, 12), (-32, 12), (-28, 12), (-28, 12)]);
        Add(ChargedBeamTrails_WaveIcePlasma_2, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (-12, 0), (-12, 0), (-24, 0), (-24, 0), (-28, 0), (-28, 0), (-28, -8), (-28, -8), (-28, -12), (-28, -12), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -16), (-28, -12), (-28, -12), (-28, -8), (-28, -8)]);
        Add(ChargedBeamTrails_WaveIcePlasma_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (-8, -8), (-8, -8), (-16, -16), (-16, -16), (-20, -20), (-20, -20), (-24, -16), (-24, -16), (-32, -12), (-32, -12), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -8), (-32, -12), (-32, -12), (-24, -16), (-24, -16)]);
        Add(ChargedBeamTrails_WaveIcePlasma_4, Reflection.AcrossVertical, [(0, 0), (0, 0), (0, -12), (0, -12), (0, -24), (0, -24), (0, -28), (0, -28), (-8, -28), (-8, -28), (-12, -28), (-12, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-16, -28), (-12, -28), (-12, -28), (-8, -28), (-8, -28)]);
        Add(ChargedBeamTrails_WaveIcePlasma_5, Reflection.AcrossDiagonalUpRight, [(0, 0), (0, 0), (8, -8), (8, -8), (16, -16), (16, -16), (20, -20), (20, -20), (28, -12), (28, -12), (32, -12), (32, -12), (32, -8), (32, -8), (32, -8), (32, -8), (32, -8), (32, -8), (32, -12), (32, -12), (28, -12), (28, -12)]);
        Add(ChargedBeamTrails_WaveIcePlasma_6, Reflection.AcrossHorizontal, [(0, 0), (0, 0), (12, 0), (12, 0), (24, 0), (24, 0), (28, 0), (28, 0), (28, -8), (28, -8), (28, -12), (28, -12), (28, -16), (28, -16), (28, -16), (28, -16), (28, -16), (28, -16), (28, -12), (28, -12), (28, -8), (28, -8)]);
        Add(ChargedBeamTrails_WaveIcePlasma_7, Reflection.AcrossDiagonalDownRight, [(0, 0), (0, 0), (8, 8), (8, 8), (16, 16), (16, 16), (20, 20), (20, 20), (24, 16), (24, 16), (32, 12), (32, 12), (32, 8), (32, 8), (32, 8), (32, 8), (32, 8), (32, 8), (32, 12), (32, 12), (24, 16), (24, 16)]);
        Add(SpazerSBATrail_WaveSpazer_0, Reflection.AcrossVertical, [(0, 0), (16, 0), (0, 0), (-16, 0)]);
        Add(SpazerSBATrail_WaveSpazer_1, Reflection.AcrossDiagonalUpRight, [(0, 0), (-10, -10), (0, 0), (10, 10)]);
        Add(SpazerSBATrail_WaveSpazer_2, Reflection.AcrossHorizontal, [(0, 0), (0, -16), (0, 0), (0, 16)]);
        Add(SpazerSBATrail_WaveSpazer_3, Reflection.AcrossDiagonalDownRight, [(0, 0), (10, -10), (0, 0), (-10, 10)]);
        return result.ToFrozenDictionary();
    }

    /// <summary>The right sparkles drawn off the reflection rule, by native record address.</summary>
    private static readonly FrozenDictionary<int, (sbyte X, sbyte Y)> DrawnRightOffsets =
        new Dictionary<int, (sbyte X, sbyte Y)>
        {
            [UnchargedBeamTrails_IceSpazer_2 + 2 * 4] = (2, 20),
            [UnchargedBeamTrails_IceSpazer_5 + 2 * 4] = (-2, -20),
            [UnchargedBeamTrails_WaveIceSpazer_1 + 5 * 4] = (2, 20),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 4 * 4] = (20, 4),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 5 * 4] = (20, 4),
            [UnchargedBeamTrails_WaveIceSpazer_5 + 6 * 4] = (20, 4),
            [UnchargedBeamTrails_WaveIcePlasma_1 + 2 * 4] = (-8, 18),
            [UnchargedBeamTrails_WaveIcePlasma_1 + 3 * 4] = (-2, 20),
            [UnchargedBeamTrails_WaveIcePlasma_1 + 7 * 4] = (-2, 20),
            [UnchargedBeamTrails_WaveIcePlasma_1 + 8 * 4] = (-8, 18),
            [UnchargedBeamTrails_WaveIcePlasma_5 + 2 * 4] = (8, -18),
            [UnchargedBeamTrails_WaveIcePlasma_5 + 3 * 4] = (2, -20),
            [UnchargedBeamTrails_WaveIcePlasma_5 + 7 * 4] = (2, -20),
            [UnchargedBeamTrails_WaveIcePlasma_5 + 8 * 4] = (8, -18),
            [ChargedBeamTrails_IceSpazer_2 + 2 * 4] = (-8, 8),
            [ChargedBeamTrails_IceSpazer_2 + 3 * 4] = (-8, 8),
            [ChargedBeamTrails_WaveIcePlasma_1 + 8 * 4] = (-16, 24),
            [ChargedBeamTrails_WaveIcePlasma_1 + 9 * 4] = (-16, 24),
            [ChargedBeamTrails_WaveIcePlasma_1 + 10 * 4] = (-12, 28),
            [ChargedBeamTrails_WaveIcePlasma_1 + 11 * 4] = (-12, 28),
            [ChargedBeamTrails_WaveIcePlasma_1 + 18 * 4] = (-12, 28),
            [ChargedBeamTrails_WaveIcePlasma_1 + 19 * 4] = (-12, 28),
            [ChargedBeamTrails_WaveIcePlasma_1 + 20 * 4] = (-16, 24),
            [ChargedBeamTrails_WaveIcePlasma_1 + 21 * 4] = (-16, 24),
            [ChargedBeamTrails_WaveIcePlasma_5 + 8 * 4] = (16, -24),
            [ChargedBeamTrails_WaveIcePlasma_5 + 9 * 4] = (16, -24),
            [ChargedBeamTrails_WaveIcePlasma_5 + 10 * 4] = (12, -28),
            [ChargedBeamTrails_WaveIcePlasma_5 + 11 * 4] = (12, -28),
            [ChargedBeamTrails_WaveIcePlasma_5 + 18 * 4] = (12, -28),
            [ChargedBeamTrails_WaveIcePlasma_5 + 19 * 4] = (12, -28),
            [ChargedBeamTrails_WaveIcePlasma_5 + 20 * 4] = (16, -24),
            [ChargedBeamTrails_WaveIcePlasma_5 + 21 * 4] = (16, -24),
        }.ToFrozenDictionary();

    private static bool TryStoredFrame(int address, out Offset frame)
    {
        if (!Frames.TryGetValue(address, out StoredFrame stored))
        {
            frame = default;
            return false;
        }
        (sbyte rightX, sbyte rightY) = DrawnRightOffsets.TryGetValue(address, out var drawn)
            ? drawn : Reflect(stored.Reflection, stored.LeftX, stored.LeftY);
        frame = new(stored.LeftX, stored.LeftY, rightX, rightY);
        return true;
    }

    private static (sbyte X, sbyte Y) Reflect(Reflection reflection, sbyte x, sbyte y) => reflection switch
    {
        Reflection.AcrossVertical => ((sbyte)-x, y),
        Reflection.AcrossHorizontal => (x, (sbyte)-y),
        Reflection.AcrossDiagonalDownRight => (y, x),
        Reflection.AcrossDiagonalUpRight => ((sbyte)-y, (sbyte)-x),
        Reflection.ThroughOrigin => ((sbyte)-x, (sbyte)-y),
        _ => throw new ArgumentOutOfRangeException(nameof(reflection)),
    };

    internal static bool TryReadByte(int address, out byte value)
    {
        if (address == EmptyFamilyWrappedCoordinateByte)
        {
            value = 0xff;
            return true;
        }

        if (address is >= ReachableAdjacentCodeStart and <= ReachableAdjacentCodeEnd)
        {
            value = ReachableAdjacentCode[address - ReachableAdjacentCodeStart];
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
        if (TryCalculatedFrame(recordAddress, out Offset frame) || TryStoredFrame(recordAddress, out frame))
        {
            value = unchecked((byte)((RecordField)offsetInRecord switch
            {
                RecordField.LeftX => frame.LeftX,
                RecordField.LeftY => frame.LeftY,
                RecordField.RightX => frame.RightX,
                RecordField.RightY => frame.RightY,
                var field => throw new InvalidOperationException($"Undefined trail record field {field}."),
            }));
            return true;
        }
        if (ReflectedListStartObservations.TryGetValue(address, out value))
            return true;
        value = 0; return false;
    }

    internal static ushort ReadCompiledWord(int address)
    {
        int next = (address & 0xff0000) | ((address + 1) & 0xffff);
        if (TryReadByte(address, out byte low) && TryReadByte(next, out byte high))
            return (ushort)(low | high << 8);

        throw new InvalidDataException(
            $"Projectile trail pointer word ${address:X6} is outside the compiled coordinate catalog.");
    }

    /// <summary>Reads one beam-family selector from the compiled high-bank pointer catalog.</summary>
    internal static ushort ReadFamilyPointer(int address) => ReadCompiledWord(address);

    /// <summary>
    /// Reads a direction-list pointer from compiled data or the genuine bank-$9B low-half
    /// alias selected by empty trail families.
    /// </summary>
    internal static ushort ReadDirectionPointer(ISnesAddressSpace bus, int address)
    {
        int next = (address & 0xff0000) | ((address + 1) & 0xffff);
        if (TryReadByte(address, out byte low) && TryReadByte(next, out byte high))
            return (ushort)(low | high << 8);

        SnesAddress source = SnesAddress.FromBusAddress(address);
        SnesAddress following = SnesAddress.FromBusAddress(next);
        if (source.Bank == 0x9b && !source.IsUpperLoRomWindow &&
            following.Bank == 0x9b && !following.IsUpperLoRomWindow)
            return (ushort)(ReadLiveDirectionByte(bus, address) |
                ReadLiveDirectionByte(bus, next) << 8);

        throw new InvalidDataException(
            $"Projectile trail direction pointer {source} is outside compiled data " +
            "and is not a mutable bank-$9B low-half alias.");
    }

    internal static ushort ReadCoordinateWord(ISnesAddressSpace bus, ushort operand, ushort index)
    {
        int address = (SamusProjectileRomData.Banks.PaletteAndTrailData + operand + index) & SnesCpuAddressLayout.AddressMask;
        int next = (address + 1) & SnesCpuAddressLayout.AddressMask;
        if (TryReadByte(address, out byte compiledLow) &&
            TryReadByte(next, out byte compiledHigh))
            return (ushort)(compiledLow | compiledHigh << 8);

        // A native absolute-indexed word may straddle compiled bank-$9B data and
        // the following bank's mutable mirror. Resolve each byte separately; the
        // low byte drives MDR for an undriven high-byte read.
        byte low = ReadCoordinateOperandByte(bus, address, (byte)(operand >> 8));
        byte high = ReadCoordinateOperandByte(bus, next, low);
        return (ushort)(low | high << 8);
    }

    private static byte ReadCoordinateOperandByte(
        ISnesAddressSpace bus, int address, byte memoryDataRegister)
    {
        if (TryReadByte(address, out byte compiled))
            return compiled;

        int bank = address >> 16;
        int offset = address & 0xffff;
        if ((bank & 0x40) == 0 &&
            (offset >= SnesCpuOpenBusWindows.ReservedBBusStart &&
             offset <= SnesCpuOpenBusWindows.ReservedBBusEnd ||
             offset >= SnesCpuOpenBusWindows.UnpopulatedExpansionStart &&
             offset <= SnesCpuOpenBusWindows.UnpopulatedExpansionEnd))
            return memoryDataRegister;

        return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Trail coordinate requires WRAM."))
                .ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Trail coordinate requires SRAM."))
                .ReadSaveRamByte(address),
            SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                throw new InvalidOperationException($"Trail coordinate ${address:X6} requires an unimplemented peripheral."))
                .ReadPeripheralByte(address),
            _ => throw new InvalidOperationException(
                $"Trail coordinate ${address:X6} requires a compiled cartridge definition."),
        };
    }

    private static byte ReadLiveDirectionByte(ISnesAddressSpace bus, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Trail direction requires WRAM."))
                .ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (bus as ISnesMutableMemory ??
                throw new InvalidOperationException("Trail direction requires SRAM."))
                .ReadSaveRamByte(address),
            SnesDmaSourceKind.Unmapped => (bus as ISnesCpuPeripheralSource ??
                throw new InvalidOperationException($"Trail direction ${address:X6} requires an unimplemented peripheral."))
                .ReadPeripheralByte(address),
            _ => throw new InvalidDataException(
                $"Trail direction ${address:X6} requires a compiled cartridge definition."),
        };
}
