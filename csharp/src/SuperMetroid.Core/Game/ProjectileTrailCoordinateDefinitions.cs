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
    /// <summary>$9BA5CF: UnchargedBeamTrails_Wave_WaveIce_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_Wave_WaveIce_1 = 0x9ba5cf;
    /// <summary>$9BA60F: UnchargedBeamTrails_Wave_WaveIce_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_Wave_WaveIce_2 = 0x9ba60f;
    /// <summary>$9BA64F: UnchargedBeamTrails_Wave_WaveIce_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_Wave_WaveIce_3 = 0x9ba64f;
    /// <summary>$9BA68F: UnchargedBeamTrails_IceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_0 = 0x9ba68f;
    /// <summary>$9BA69B: UnchargedBeamTrails_IceSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_1 = 0x9ba69b;
    /// <summary>$9BA6A7: UnchargedBeamTrails_IceSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_2 = 0x9ba6a7;
    /// <summary>$9BA6B3: UnchargedBeamTrails_IceSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_3 = 0x9ba6b3;
    /// <summary>$9BA6BF: UnchargedBeamTrails_IceSpazer_4, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_4 = 0x9ba6bf;
    /// <summary>$9BA6CB: UnchargedBeamTrails_IceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_5 = 0x9ba6cb;
    /// <summary>$9BA6D7: UnchargedBeamTrails_IceSpazer_6, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_6 = 0x9ba6d7;
    /// <summary>$9BA6E3: UnchargedBeamTrails_IceSpazer_7, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_IceSpazer_7 = 0x9ba6e3;
    /// <summary>$9BA6EF: UnchargedBeamTrails_WaveIceSpazer_0, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_0 = 0x9ba6ef;
    /// <summary>$9BA717: UnchargedBeamTrails_WaveIceSpazer_1, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_1 = 0x9ba717;
    /// <summary>$9BA73F: UnchargedBeamTrails_WaveIceSpazer_2, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_2 = 0x9ba73f;
    /// <summary>$9BA767: UnchargedBeamTrails_WaveIceSpazer_3, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_3 = 0x9ba767;
    /// <summary>$9BA78F: UnchargedBeamTrails_WaveIceSpazer_4, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_4 = 0x9ba78f;
    /// <summary>$9BA7B7: UnchargedBeamTrails_WaveIceSpazer_5, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_5 = 0x9ba7b7;
    /// <summary>$9BA7DF: UnchargedBeamTrails_WaveIceSpazer_6, left X/Y and right X/Y placement frames.</summary>
    private const int UnchargedBeamTrails_WaveIceSpazer_6 = 0x9ba7df;
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
    /// <summary>$9BAA67: ChargedBeamTrails_Wave_WaveIce_1, left X/Y and right X/Y placement frames.</summary>
    private const int ChargedBeamTrails_Wave_WaveIce_1 = 0x9baa67;
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
    /// <summary>$9BB39B: UNSUED_SpazerSBATrail_Spazer_IceSpazer_1_9BB39B, left X/Y and right X/Y placement frames.</summary>
    private const int UNSUED_SpazerSBATrail_Spazer_IceSpazer_1_9BB39B = 0x9bb39b;
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
    private readonly record struct Offset(sbyte LeftX, sbyte LeftY, sbyte RightX, sbyte RightY);
    private static readonly FrozenDictionary<int, ushort> Pointers = CreatePointers();
    private static readonly FrozenDictionary<int, Offset> Frames = CreateFrames();

    private static FrozenDictionary<int, ushort> CreatePointers()
    {
        var result = new Dictionary<int, ushort>();
        void Add(int address, ReadOnlySpan<ushort> values)
        { for (int i = 0; i < values.Length; i++) result.Add(address + i * 2, values[i]); }
        Add(BeamTrailOffsets_uncharged, [unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce), unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce), unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_IceSpazer), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer), unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_Default), unchecked((ushort)UnchargedBeamTrails_IcePlasma), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma)]);
        Add(BeamTrailOffsets_charged, [unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce), unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce), unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_IceSpazer), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer), unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_Default), unchecked((ushort)ChargedBeamTrails_IcePlasma), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma)]);
        Add(BeamTrailOffsets_spazerSBA, [0, 0, 0, 0, unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B), unchecked((ushort)SpazerSBATrail_WaveSpazer), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B), 0, 0, 0]);
        Add(UnchargedBeamTrails_Wave_WaveIce, [unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_2), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_1), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_3), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_2), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_1), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_3), unchecked((ushort)UnchargedBeamTrails_Wave_WaveIce_0)]);
        Add(UnchargedBeamTrails_Default, [unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0), unchecked((ushort)UnchargedBeamTrails_Default_0)]);
        Add(UnchargedBeamTrails_IceSpazer, [unchecked((ushort)UnchargedBeamTrails_IceSpazer_0), unchecked((ushort)UnchargedBeamTrails_IceSpazer_2), unchecked((ushort)UnchargedBeamTrails_IceSpazer_3), unchecked((ushort)UnchargedBeamTrails_IceSpazer_4), unchecked((ushort)UnchargedBeamTrails_IceSpazer_1), unchecked((ushort)UnchargedBeamTrails_IceSpazer_1), unchecked((ushort)UnchargedBeamTrails_IceSpazer_5), unchecked((ushort)UnchargedBeamTrails_IceSpazer_6), unchecked((ushort)UnchargedBeamTrails_IceSpazer_7), unchecked((ushort)UnchargedBeamTrails_IceSpazer_0)]);
        Add(UnchargedBeamTrails_WaveIceSpazer, [unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_0), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_1), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_2), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_3), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_4), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_4), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_5), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_6), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_7), unchecked((ushort)UnchargedBeamTrails_WaveIceSpazer_0)]);
        Add(UnchargedBeamTrails_IcePlasma, [unchecked((ushort)UnchargedBeamTrails_IcePlasma_0), unchecked((ushort)UnchargedBeamTrails_IcePlasma_1), unchecked((ushort)UnchargedBeamTrails_IcePlasma_2), unchecked((ushort)UnchargedBeamTrails_IcePlasma_3), unchecked((ushort)UnchargedBeamTrails_IcePlasma_4), unchecked((ushort)UnchargedBeamTrails_IcePlasma_4), unchecked((ushort)UnchargedBeamTrails_IcePlasma_5), unchecked((ushort)UnchargedBeamTrails_IcePlasma_6), unchecked((ushort)UnchargedBeamTrails_IcePlasma_7), unchecked((ushort)UnchargedBeamTrails_IcePlasma_0)]);
        Add(UnchargedBeamTrails_WaveIcePlasma, [unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_0), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_1), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_2), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_3), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_4), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_4), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_5), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_6), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_7), unchecked((ushort)UnchargedBeamTrails_WaveIcePlasma_0)]);
        Add(ChargedBeamTrails_Default, [unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0), unchecked((ushort)ChargedBeamTrails_Default_0)]);
        Add(ChargedBeamTrails_Wave_WaveIce, [unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_2), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_1), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_3), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_0), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_2), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_1), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_3), unchecked((ushort)ChargedBeamTrails_Wave_WaveIce_0)]);
        Add(ChargedBeamTrails_IceSpazer, [unchecked((ushort)ChargedBeamTrails_IceSpazer_0), unchecked((ushort)ChargedBeamTrails_IceSpazer_1), unchecked((ushort)ChargedBeamTrails_IceSpazer_2), unchecked((ushort)ChargedBeamTrails_IceSpazer_3), unchecked((ushort)ChargedBeamTrails_IceSpazer_4), unchecked((ushort)ChargedBeamTrails_IceSpazer_4), unchecked((ushort)ChargedBeamTrails_IceSpazer_5), unchecked((ushort)ChargedBeamTrails_IceSpazer_6), unchecked((ushort)ChargedBeamTrails_IceSpazer_7), unchecked((ushort)ChargedBeamTrails_IceSpazer_0)]);
        Add(ChargedBeamTrails_WaveIceSpazer, [unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_0), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_1), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_2), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_3), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_4), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_4), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_5), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_6), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_7), unchecked((ushort)ChargedBeamTrails_WaveIceSpazer_0)]);
        Add(ChargedBeamTrails_IcePlasma, [unchecked((ushort)ChargedBeamTrails_IcePlasma_0), unchecked((ushort)ChargedBeamTrails_IcePlasma_1), unchecked((ushort)ChargedBeamTrails_IcePlasma_2), unchecked((ushort)ChargedBeamTrails_IcePlasma_3), unchecked((ushort)ChargedBeamTrails_IcePlasma_4), unchecked((ushort)ChargedBeamTrails_IcePlasma_4), unchecked((ushort)ChargedBeamTrails_IcePlasma_5), unchecked((ushort)ChargedBeamTrails_IcePlasma_6), unchecked((ushort)ChargedBeamTrails_IcePlasma_7), unchecked((ushort)ChargedBeamTrails_IcePlasma_0)]);
        Add(ChargedBeamTrails_WaveIcePlasma, [unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_0), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_1), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_2), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_3), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_4), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_4), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_5), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_6), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_7), unchecked((ushort)ChargedBeamTrails_WaveIcePlasma_0)]);
        Add(SpazerSBATrail_WaveSpazer, [unchecked((ushort)SpazerSBATrail_WaveSpazer_0), unchecked((ushort)SpazerSBATrail_WaveSpazer_1), unchecked((ushort)SpazerSBATrail_WaveSpazer_2), unchecked((ushort)SpazerSBATrail_WaveSpazer_3), unchecked((ushort)SpazerSBATrail_WaveSpazer_0), unchecked((ushort)SpazerSBATrail_WaveSpazer_0), unchecked((ushort)SpazerSBATrail_WaveSpazer_1), unchecked((ushort)SpazerSBATrail_WaveSpazer_2), unchecked((ushort)SpazerSBATrail_WaveSpazer_3), unchecked((ushort)SpazerSBATrail_WaveSpazer_0)]);
        Add(UNSUED_SpazerSBATrail_Spazer_IceSpazer_9BB37B, [unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_1_9BB39B), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_1_9BB39B), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F), unchecked((ushort)UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F)]);
        return result.ToFrozenDictionary();
    }

    private static FrozenDictionary<int, Offset> CreateFrames()
    {
        var result = new Dictionary<int, Offset>();
        void Add(int address, ReadOnlySpan<Offset> values)
        { for (int i = 0; i < values.Length; i++) result.Add(address + i * 4, values[i]); }
        Add(UnchargedBeamTrails_Default_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0),
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0),
        ]);
        Add(UnchargedBeamTrails_Wave_WaveIce_0,
        [
            new(0, 0, 0, 0), new(8, 0, 0, 0), new(12, 0, 0, 0), new(16, 0, 0, 0),
            new(20, 0, 0, 0), new(16, 0, 0, 0), new(12, 0, 0, 0), new(8, 0, 0, 0),
            new(0, 0, 0, 0), new(-8, 0, 0, 0), new(-12, 0, 0, 0), new(-16, 0, 0, 0),
            new(-20, 0, 0, 0), new(-16, 0, 0, 0), new(-12, 0, 0, 0), new(-8, 0, 0, 0),
        ]);
        Add(UnchargedBeamTrails_Wave_WaveIce_1,
        [
            new(0, 0, 0, 0), new(0, -8, 0, 0), new(0, -12, 0, 0), new(0, -16, 0, 0),
            new(0, -20, 0, 0), new(0, -16, 0, 0), new(0, -12, 0, 0), new(0, -8, 0, 0),
            new(0, 0, 0, 0), new(0, 8, 0, 0), new(0, 12, 0, 0), new(0, 16, 0, 0),
            new(0, 20, 0, 0), new(0, 16, 0, 0), new(0, 12, 0, 0), new(0, 8, 0, 0),
        ]);
        Add(UnchargedBeamTrails_Wave_WaveIce_2,
        [
            new(0, 0, 0, 0), new(-4, -4, 0, 0), new(-8, -8, 0, 0), new(-10, -10, 0, 0),
            new(-12, -12, 0, 0), new(-10, -10, 0, 0), new(-8, -8, 0, 0), new(-4, -4, 0, 0),
            new(0, 0, 0, 0), new(4, 4, 0, 0), new(8, 8, 0, 0), new(10, 10, 0, 0),
            new(12, 12, 0, 0), new(10, 10, 0, 0), new(8, 8, 0, 0), new(4, 4, 0, 0),
        ]);
        Add(UnchargedBeamTrails_Wave_WaveIce_3,
        [
            new(0, 0, 0, 0), new(4, -4, 0, 0), new(8, -8, 0, 0), new(10, -10, 0, 0),
            new(12, -12, 0, 0), new(10, -10, 0, 0), new(8, -8, 0, 0), new(4, -4, 0, 0),
            new(0, 0, 0, 0), new(-4, 4, 0, 0), new(-8, 8, 0, 0), new(-10, 10, 0, 0),
            new(-12, 12, 0, 0), new(-10, 10, 0, 0), new(-8, 8, 0, 0), new(-4, 4, 0, 0),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_0,
        [
            new(0, 0, 0, 0), new(-8, 8, 8, 8), new(-16, 8, 16, 8),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_1,
        [
            new(0, 0, 0, 0), new(-8, -8, 8, -8), new(-16, -8, 16, -8),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_2,
        [
            new(-8, 8, -8, 8), new(-14, 2, -2, 14), new(-20, -4, 2, 20),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_3,
        [
            new(-8, 0, -8, 0), new(-8, -8, -8, 8), new(-8, -16, -8, 16),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_4,
        [
            new(-8, -8, -8, -8), new(-2, -16, -16, -2), new(4, -20, -20, 4),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_5,
        [
            new(8, -8, 8, -8), new(14, -2, 2, -14), new(20, 4, -2, -20),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_6,
        [
            new(8, 0, 8, 0), new(8, 8, 8, -8), new(8, 16, 8, -16),
        ]);
        Add(UnchargedBeamTrails_IceSpazer_7,
        [
            new(8, 8, 8, 8), new(2, 16, 16, 2), new(-4, 20, 20, -4),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_0,
        [
            new(0, 0, 0, 0), new(-4, 8, 4, 8), new(-8, 8, 8, 8), new(-12, 8, 12, 8),
            new(-16, 8, 16, 8), new(-16, 8, 16, 8), new(-16, 8, 16, 8), new(-12, 8, 12, 8),
            new(-8, 8, 8, 8), new(-4, 8, 4, 8),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_1,
        [
            new(0, 0, 0, 0), new(-12, 6, -6, 12), new(-14, 2, -2, 14), new(-16, 0, 0, 16),
            new(-18, -2, 2, 18), new(-20, -4, 2, 20), new(-18, -2, 2, 18), new(-16, 0, 0, 16),
            new(-14, 2, -2, 14), new(-12, 6, -6, 12),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_2,
        [
            new(0, 0, 0, 0), new(-8, -4, -8, 4), new(-8, -8, -8, 8), new(-8, -12, -8, 12),
            new(-8, -16, -8, 16), new(-8, -16, -8, 16), new(-8, -16, -8, 16), new(-8, -12, -8, 12),
            new(-8, -8, -8, 8), new(-8, -4, -8, 4),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_3,
        [
            new(0, 0, 0, 0), new(-12, -6, -6, -12), new(-2, -16, -16, -2), new(-16, 0, 0, -16),
            new(-18, 2, 2, -18), new(4, -20, -20, 4), new(-18, 2, 2, -18), new(-16, 0, 0, -16),
            new(-2, -16, -16, -2), new(-12, -6, -6, -12),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_4,
        [
            new(0, 0, 0, 0), new(-4, -8, 4, -8), new(-8, -8, 8, -8), new(-12, -8, 12, -8),
            new(-16, -8, 16, -8), new(-16, -8, 16, -8), new(-16, -8, 16, -8), new(-12, -8, 12, -8),
            new(-8, -8, 8, -8), new(-4, -8, 4, -8),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_5,
        [
            new(0, 0, 0, 0), new(2, -14, 14, -2), new(0, -16, 16, 0), new(-2, -18, 18, 2),
            new(-2, -20, 20, 4), new(-2, -20, 20, 4), new(-2, -20, 20, 4), new(-2, -18, 18, 2),
            new(0, -16, 16, 0), new(2, -14, 14, -2),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_6,
        [
            new(0, 0, 0, 0), new(8, -4, 8, 4), new(8, -8, 8, 8), new(8, -12, 8, 12),
            new(8, -16, 8, 16), new(8, -16, 8, 16), new(8, -16, 8, 16), new(8, -12, 8, 12),
            new(8, -8, 8, 8), new(8, -4, 8, 4),
        ]);
        Add(UnchargedBeamTrails_WaveIceSpazer_7,
        [
            new(0, 0, 0, 0), new(6, 10, 10, 6), new(2, 16, 16, 2), new(0, 16, 16, 0),
            new(-2, 18, 18, -2), new(-4, 20, 20, -4), new(-2, 18, 18, -2), new(0, 16, 16, 0),
            new(2, 16, 16, 2), new(6, 10, 10, 6),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_0,
        [
            new(0, 0, 0, 0), new(0, 16, 0, 16),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_1,
        [
            new(0, 0, 0, 0), new(-12, 12, -12, 12),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_2,
        [
            new(0, 0, 0, 0), new(-16, 0, -16, 0),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_3,
        [
            new(0, 0, 0, 0), new(-12, -12, -12, -12),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_4,
        [
            new(0, 0, 0, 0), new(0, -16, 0, -16),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_5,
        [
            new(0, 0, 0, 0), new(12, -12, 12, -12),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_6,
        [
            new(0, 0, 0, 0), new(16, 0, 16, 0),
        ]);
        Add(UnchargedBeamTrails_IcePlasma_7,
        [
            new(0, 0, 0, 0), new(12, 12, 12, 12),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_0,
        [
            new(0, 0, 0, 0), new(0, 16, 0, 16), new(-8, 16, 8, 16), new(-16, 16, 16, 16),
            new(-16, 16, 16, 16), new(-16, 16, 16, 16), new(-16, 16, 16, 16), new(-16, 16, 16, 16),
            new(-8, 16, 8, 16),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_1,
        [
            new(0, 0, 0, 0), new(-12, 12, -12, 12), new(-20, 8, -8, 18), new(-24, 2, -2, 20),
            new(-24, 0, 0, 24), new(-24, 0, 0, 24), new(-24, 0, 0, 24), new(-24, 2, -2, 20),
            new(-20, 8, -8, 18),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_2,
        [
            new(0, 0, 0, 0), new(-16, 0, -16, 0), new(-16, -8, -16, 8), new(-16, -12, -16, 12),
            new(-16, -16, -16, 16), new(-16, -16, -16, 16), new(-16, -16, -16, 16), new(-16, -12, -16, 12),
            new(-16, -8, -16, 8),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_3,
        [
            new(0, 0, 0, 0), new(-12, -12, -12, -12), new(-18, -6, -6, -18), new(-20, -2, -2, -20),
            new(-24, 0, 0, -24), new(-24, 0, 0, -24), new(-24, 0, 0, -24), new(-20, -2, -2, -20),
            new(-18, -6, -6, -18),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_4,
        [
            new(0, 0, 0, 0), new(0, -16, 0, -16), new(-8, -16, 8, -16), new(-16, -16, 16, -16),
            new(-16, -16, 16, -16), new(-16, -16, 16, -16), new(-16, -16, 16, -16), new(-16, -16, 16, -16),
            new(-8, -16, 8, -16),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_5,
        [
            new(0, 0, 0, 0), new(12, -12, 12, -12), new(20, -8, 8, -18), new(24, -2, 2, -20),
            new(24, 0, 0, -24), new(24, 0, 0, -24), new(24, 0, 0, -24), new(24, -2, 2, -20),
            new(20, -8, 8, -18),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_6,
        [
            new(0, 0, 0, 0), new(16, 0, 16, 0), new(16, -8, 16, 8), new(16, -12, 16, 12),
            new(16, -16, 16, 16), new(16, -16, 16, 16), new(16, -16, 16, 16), new(16, -12, 16, 12),
            new(16, -8, 16, 8),
        ]);
        Add(UnchargedBeamTrails_WaveIcePlasma_7,
        [
            new(0, 0, 0, 0), new(12, 12, 12, 12), new(18, 6, 6, 18), new(20, 2, 2, 20),
            new(24, 0, 0, 24), new(24, 0, 0, 24), new(24, 0, 0, 24), new(20, 2, 2, 20),
            new(18, 6, 6, 18),
        ]);
        Add(ChargedBeamTrails_Default_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0),
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0),
        ]);
        Add(ChargedBeamTrails_Wave_WaveIce_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 0, 8, 0), new(-8, 0, 8, 0),
            new(-12, 0, 12, 0), new(-12, 0, 12, 0), new(-16, 0, 16, 0), new(-16, 0, 16, 0),
            new(-16, 0, 16, 0), new(-16, 0, 16, 0), new(-16, 0, 16, 0), new(-16, 0, 16, 0),
            new(-12, 0, 12, 0), new(-12, 0, 12, 0), new(-8, 0, 8, 0), new(-8, 0, 8, 0),
        ]);
        Add(ChargedBeamTrails_Wave_WaveIce_1,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, -8, 0, 8), new(0, -8, 0, 8),
            new(0, -12, 0, 12), new(0, -12, 0, 12), new(0, -16, 0, 16), new(0, -16, 0, 16),
            new(0, -16, 0, 16), new(0, -16, 0, 16), new(0, -16, 0, 16), new(0, -16, 0, 16),
            new(0, -12, 0, 12), new(0, -12, 0, 12), new(0, -8, 0, 8), new(0, -8, 0, 8),
        ]);
        Add(ChargedBeamTrails_Wave_WaveIce_2,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-4, -4, 4, 4), new(-4, -4, 4, 4),
            new(-8, -8, 8, 8), new(-8, -8, 8, 8), new(-8, -8, 8, 8), new(-8, -8, 8, 8),
            new(-10, -10, 10, 10), new(-10, -10, 10, 10), new(-8, -8, 8, 8), new(-8, -8, 8, 8),
            new(-8, -8, 8, 8), new(-8, -8, 8, 8), new(-4, -4, 4, 4), new(-4, -4, 4, 4),
        ]);
        Add(ChargedBeamTrails_Wave_WaveIce_3,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-4, 4, 4, -4), new(-4, 4, 4, -4),
            new(-8, 8, 8, -8), new(-8, 8, 8, -8), new(-8, 8, 8, -8), new(-8, 8, 8, -8),
            new(-10, 10, 10, -10), new(-10, 10, 10, -10), new(-8, 8, 8, -8), new(-8, 8, 8, -8),
            new(-8, 8, 8, -8), new(-8, 8, 8, -8), new(-4, 4, 4, -4), new(-4, 4, 4, -4),
        ]);
        Add(ChargedBeamTrails_IceSpazer_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 8, 0, 8), new(0, 8, 0, 8),
            new(0, 16, 0, 16), new(0, 16, 0, 16), new(-8, 16, 8, 16), new(-8, 16, 8, 16),
            new(-16, 16, 16, 16), new(-16, 16, 16, 16),
        ]);
        Add(ChargedBeamTrails_IceSpazer_1,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 8, -8, 8), new(-8, 8, -8, 8),
            new(-12, 12, -12, 12), new(-12, 12, -12, 12), new(-16, 8, -8, 16), new(-16, 8, -8, 16),
            new(-24, 0, 0, 24), new(-24, 0, 0, 24),
        ]);
        Add(ChargedBeamTrails_IceSpazer_2,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 8, -8, 8), new(-8, 8, -8, 8),
            new(-16, 0, -16, 0), new(-16, 0, -16, 0), new(-16, -8, -16, 8), new(-16, -8, -16, 8),
            new(-16, -16, -16, 16), new(-16, -16, -16, 16),
        ]);
        Add(ChargedBeamTrails_IceSpazer_3,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, -8, -8, -8), new(-8, -8, -8, -8),
            new(-12, -12, -12, -12), new(-12, -12, -12, -12), new(-16, -8, -8, -16), new(-16, -8, -8, -16),
            new(-24, 0, 0, -24), new(-24, 0, 0, -24),
        ]);
        Add(ChargedBeamTrails_IceSpazer_4,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, -8, 0, -8), new(0, -8, 0, -8),
            new(0, -16, 0, -16), new(0, -16, 0, -16), new(-8, -16, 8, -16), new(-8, -16, 8, -16),
            new(-16, -16, 16, -16), new(-16, -16, 16, -16),
        ]);
        Add(ChargedBeamTrails_IceSpazer_5,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, -8, 8, -8), new(8, -8, 8, -8),
            new(12, -12, 12, -12), new(12, -12, 12, -12), new(16, -8, 8, -16), new(16, -8, 8, -16),
            new(24, 0, 0, -24), new(24, 0, 0, -24),
        ]);
        Add(ChargedBeamTrails_IceSpazer_6,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 0, 8, 0), new(8, 0, 8, 0),
            new(16, 0, 16, 0), new(16, 0, 16, 0), new(16, -8, 16, 8), new(16, -8, 16, 8),
            new(16, -16, 16, 16), new(16, -16, 16, 16),
        ]);
        Add(ChargedBeamTrails_IceSpazer_7,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 8, 8, 8), new(8, 8, 8, 8),
            new(12, 12, 12, 12), new(12, 12, 12, 12), new(16, 8, 8, 16), new(16, 8, 8, 16),
            new(24, 0, 0, 24), new(24, 0, 0, 24),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 8, 0, 8), new(0, 8, 0, 8),
            new(0, 16, 0, 16), new(0, 16, 0, 16), new(-4, 16, 4, 16), new(-4, 16, 4, 16),
            new(-8, 16, 8, 16), new(-8, 16, 8, 16), new(-12, 16, 12, 16), new(-12, 16, 12, 16),
            new(-16, 16, 16, 16), new(-16, 16, 16, 16), new(-16, 16, 16, 16), new(-16, 16, 16, 16),
            new(-16, 16, 16, 16), new(-16, 16, 16, 16), new(-12, 16, 12, 16), new(-12, 16, 12, 16),
            new(-8, 16, 8, 16), new(-8, 16, 8, 16), new(-4, 16, 4, 16), new(-4, 16, 4, 16),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_1,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 8, -8, 8), new(-8, 8, -8, 8),
            new(-12, 12, -12, 12), new(-12, 12, -12, 12), new(-16, 8, -8, 16), new(-16, 8, -8, 16),
            new(-16, 8, -8, 16), new(-16, 8, -8, 16), new(-16, 8, -8, 16), new(-16, 8, -8, 16),
            new(-24, 0, 0, 24), new(-24, 0, 0, 24), new(-24, 0, 0, 24), new(-24, 0, 0, 24),
            new(-24, 0, 0, 24), new(-24, 0, 0, 24), new(-16, 8, -8, 16), new(-16, 8, -8, 16),
            new(-16, 8, -8, 16), new(-16, 8, -8, 16), new(-16, 8, -8, 16), new(-16, 8, -8, 16),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_2,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 0, -8, 0), new(-8, 0, -8, 0),
            new(-16, 0, -16, 0), new(-16, 0, -16, 0), new(-16, -4, -16, 4), new(-16, -4, -16, 4),
            new(-16, -8, -16, 8), new(-16, -8, -16, 8), new(-16, -12, -16, 12), new(-16, -12, -16, 12),
            new(-16, -16, -16, 16), new(-16, -16, -16, 16), new(-16, -16, -16, 16), new(-16, -16, -16, 16),
            new(-16, -16, -16, 16), new(-16, -16, -16, 16), new(-16, -12, -16, 12), new(-16, -12, -16, 12),
            new(-16, -8, -16, 8), new(-16, -8, -16, 8), new(-16, -4, -16, 4), new(-16, -4, -16, 4),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_3,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, -8, -8, -8), new(-8, -8, -8, -8),
            new(-12, -12, -12, -12), new(-12, -12, -12, -12), new(-16, -8, -8, -16), new(-16, -8, -8, -16),
            new(-16, -8, -8, -16), new(-16, -8, -8, -16), new(-20, -4, -4, -20), new(-20, -4, -4, -20),
            new(-24, 0, 0, -24), new(-24, 0, 0, -24), new(-24, 0, 0, -24), new(-24, 0, 0, -24),
            new(-24, 0, 0, -24), new(-24, 0, 0, -24), new(-20, -4, -4, -20), new(-20, -4, -4, -20),
            new(-16, -8, -8, -16), new(-16, -8, -8, -16), new(-16, -8, -8, -16), new(-16, -8, -8, -16),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_4,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, -8, 0, -8), new(0, -8, 0, -8),
            new(0, -16, 0, -16), new(0, -16, 0, -16), new(-4, -16, 4, -16), new(-4, -16, 4, -16),
            new(-8, -16, 8, -16), new(-8, -16, 8, -16), new(-12, -16, 12, -16), new(-12, -16, 12, -16),
            new(-16, -16, 16, -16), new(-16, -16, 16, -16), new(-16, -16, 16, -16), new(-16, -16, 16, -16),
            new(-16, -16, 16, -16), new(-16, -16, 16, -16), new(-12, -16, 12, -16), new(-12, -16, 12, -16),
            new(-8, -16, 8, -16), new(-8, -16, 8, -16), new(-4, -16, 4, -16), new(-4, -16, 4, -16),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_5,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, -8, 8, -8), new(8, -8, 8, -8),
            new(12, -12, 12, -12), new(12, -12, 12, -12), new(8, -16, 16, -8), new(8, -16, 16, -8),
            new(8, -16, 16, -8), new(8, -16, 16, -8), new(4, -20, 20, -4), new(4, -20, 20, -4),
            new(0, -24, 24, 0), new(0, -24, 24, 0), new(0, -24, 24, 0), new(0, -24, 24, 0),
            new(0, -24, 24, 0), new(0, -24, 24, 0), new(4, -20, 20, -4), new(4, -20, 20, -4),
            new(8, -16, 16, -8), new(8, -16, 16, -8), new(8, -16, 16, -8), new(8, -16, 16, -8),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_6,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 0, 8, 0), new(8, 0, 8, 0),
            new(16, 0, 16, 0), new(16, 0, 16, 0), new(16, -4, 16, 4), new(16, -4, 16, 4),
            new(16, -8, 16, 8), new(16, -8, 16, 8), new(16, -12, 16, 12), new(16, -12, 16, 12),
            new(16, -16, 16, 16), new(16, -16, 16, 16), new(16, -16, 16, 16), new(16, -16, 16, 16),
            new(16, -16, 16, 16), new(16, -16, 16, 16), new(16, -12, 16, 12), new(16, -12, 16, 12),
            new(16, -8, 16, 8), new(16, -8, 16, 8), new(16, -4, 16, 4), new(16, -4, 16, 4),
        ]);
        Add(ChargedBeamTrails_WaveIceSpazer_7,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 8, 8, 8), new(8, 8, 8, 8),
            new(12, 12, 12, 12), new(12, 12, 12, 12), new(8, 16, 16, 8), new(8, 16, 16, 8),
            new(8, 16, 16, 8), new(8, 16, 16, 8), new(4, 20, 20, 4), new(4, 20, 20, 4),
            new(0, 24, 24, 0), new(0, 24, 24, 0), new(0, 24, 24, 0), new(0, 24, 24, 0),
            new(0, 24, 24, 0), new(0, 24, 24, 0), new(4, 20, 20, 4), new(4, 20, 20, 4),
            new(8, 16, 16, 8), new(8, 16, 16, 8), new(8, 16, 16, 8), new(8, 16, 16, 8),
        ]);
        Add(ChargedBeamTrails_IcePlasma_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 12, 0, 12), new(0, 12, 0, 12),
            new(0, 24, 0, 24), new(0, 24, 0, 24), new(0, 28, 0, 28), new(0, 28, 0, 28),
        ]);
        Add(ChargedBeamTrails_IcePlasma_1,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 8, -8, 8), new(-8, 8, -8, 8),
            new(-16, 16, -16, 16), new(-16, 16, -16, 16), new(-24, 24, -24, 24), new(-24, 24, -24, 24),
        ]);
        Add(ChargedBeamTrails_IcePlasma_2,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-12, 0, -12, 0), new(-12, 0, -12, 0),
            new(-24, 0, -24, 0), new(-24, 0, -24, 0), new(-28, 0, -28, 0), new(-28, 0, -28, 0),
        ]);
        Add(ChargedBeamTrails_IcePlasma_3,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, -8, -8, -8), new(-8, -8, -8, -8),
            new(-16, -16, -16, -16), new(-16, -16, -16, -16), new(-24, -24, -24, -24), new(-24, -24, -24, -24),
        ]);
        Add(ChargedBeamTrails_IcePlasma_4,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, -12, 0, -12), new(0, -12, 0, -12),
            new(0, -24, 0, -24), new(0, -24, 0, -24), new(0, -28, 0, -28), new(0, -28, 0, -28),
        ]);
        Add(ChargedBeamTrails_IcePlasma_5,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, -8, 8, -8), new(8, -8, 8, -8),
            new(16, -16, 16, -16), new(16, -16, 16, -16), new(24, -24, 24, -24), new(24, -24, 24, -24),
        ]);
        Add(ChargedBeamTrails_IcePlasma_6,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(12, 0, 12, 0), new(12, 0, 12, 0),
            new(24, 0, 24, 0), new(24, 0, 24, 0), new(28, 0, 28, 0), new(28, 0, 28, 0),
        ]);
        Add(ChargedBeamTrails_IcePlasma_7,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 8, 8, 8), new(8, 8, 8, 8),
            new(16, 16, 16, 16), new(16, 16, 16, 16), new(24, 24, 24, 24), new(24, 24, 24, 24),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_0,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, 12, 0, 12), new(0, 12, 0, 12),
            new(0, 24, 0, 24), new(0, 24, 0, 24), new(0, 28, 0, 28), new(0, 28, 0, 28),
            new(-8, 28, 8, 28), new(-8, 28, 8, 28), new(-12, 28, 12, 28), new(-12, 28, 12, 28),
            new(-16, 28, 16, 28), new(-16, 28, 16, 28), new(-16, 28, 16, 28), new(-16, 28, 16, 28),
            new(-16, 28, 16, 28), new(-16, 28, 16, 28), new(-12, 28, 12, 28), new(-12, 28, 12, 28),
            new(-8, 28, 8, 28), new(-8, 28, 8, 28),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_1,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, 8, -8, 8), new(-8, 8, -8, 8),
            new(-16, 16, -16, 16), new(-16, 16, -16, 16), new(-20, 20, -20, 20), new(-20, 20, -20, 20),
            new(-28, 12, -16, 24), new(-28, 12, -16, 24), new(-32, 12, -12, 28), new(-32, 12, -12, 28),
            new(-32, 8, -8, 32), new(-32, 8, -8, 32), new(-32, 8, -8, 32), new(-32, 8, -8, 32),
            new(-32, 8, -8, 32), new(-32, 8, -8, 32), new(-32, 12, -12, 28), new(-32, 12, -12, 28),
            new(-28, 12, -16, 24), new(-28, 12, -16, 24),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_2,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-12, 0, -12, 0), new(-12, 0, -12, 0),
            new(-24, 0, -24, 0), new(-24, 0, -24, 0), new(-28, 0, -28, 0), new(-28, 0, -28, 0),
            new(-28, -8, -28, 8), new(-28, -8, -28, 8), new(-28, -12, -28, 12), new(-28, -12, -28, 12),
            new(-28, -16, -28, 16), new(-28, -16, -28, 16), new(-28, -16, -28, 16), new(-28, -16, -28, 16),
            new(-28, -16, -28, 16), new(-28, -16, -28, 16), new(-28, -12, -28, 12), new(-28, -12, -28, 12),
            new(-28, -8, -28, 8), new(-28, -8, -28, 8),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_3,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(-8, -8, -8, -8), new(-8, -8, -8, -8),
            new(-16, -16, -16, -16), new(-16, -16, -16, -16), new(-20, -20, -20, -20), new(-20, -20, -20, -20),
            new(-24, -16, -16, -24), new(-24, -16, -16, -24), new(-32, -12, -12, -32), new(-32, -12, -12, -32),
            new(-32, -8, -8, -32), new(-32, -8, -8, -32), new(-32, -8, -8, -32), new(-32, -8, -8, -32),
            new(-32, -8, -8, -32), new(-32, -8, -8, -32), new(-32, -12, -12, -32), new(-32, -12, -12, -32),
            new(-24, -16, -16, -24), new(-24, -16, -16, -24),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_4,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(0, -12, 0, -12), new(0, -12, 0, -12),
            new(0, -24, 0, -24), new(0, -24, 0, -24), new(0, -28, 0, -28), new(0, -28, 0, -28),
            new(-8, -28, 8, -28), new(-8, -28, 8, -28), new(-12, -28, 12, -28), new(-12, -28, 12, -28),
            new(-16, -28, 16, -28), new(-16, -28, 16, -28), new(-16, -28, 16, -28), new(-16, -28, 16, -28),
            new(-16, -28, 16, -28), new(-16, -28, 16, -28), new(-12, -28, 12, -28), new(-12, -28, 12, -28),
            new(-8, -28, 8, -28), new(-8, -28, 8, -28),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_5,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, -8, 8, -8), new(8, -8, 8, -8),
            new(16, -16, 16, -16), new(16, -16, 16, -16), new(20, -20, 20, -20), new(20, -20, 20, -20),
            new(28, -12, 16, -24), new(28, -12, 16, -24), new(32, -12, 12, -28), new(32, -12, 12, -28),
            new(32, -8, 8, -32), new(32, -8, 8, -32), new(32, -8, 8, -32), new(32, -8, 8, -32),
            new(32, -8, 8, -32), new(32, -8, 8, -32), new(32, -12, 12, -28), new(32, -12, 12, -28),
            new(28, -12, 16, -24), new(28, -12, 16, -24),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_6,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(12, 0, 12, 0), new(12, 0, 12, 0),
            new(24, 0, 24, 0), new(24, 0, 24, 0), new(28, 0, 28, 0), new(28, 0, 28, 0),
            new(28, -8, 28, 8), new(28, -8, 28, 8), new(28, -12, 28, 12), new(28, -12, 28, 12),
            new(28, -16, 28, 16), new(28, -16, 28, 16), new(28, -16, 28, 16), new(28, -16, 28, 16),
            new(28, -16, 28, 16), new(28, -16, 28, 16), new(28, -12, 28, 12), new(28, -12, 28, 12),
            new(28, -8, 28, 8), new(28, -8, 28, 8),
        ]);
        Add(ChargedBeamTrails_WaveIcePlasma_7,
        [
            new(0, 0, 0, 0), new(0, 0, 0, 0), new(8, 8, 8, 8), new(8, 8, 8, 8),
            new(16, 16, 16, 16), new(16, 16, 16, 16), new(20, 20, 20, 20), new(20, 20, 20, 20),
            new(24, 16, 16, 24), new(24, 16, 16, 24), new(32, 12, 12, 32), new(32, 12, 12, 32),
            new(32, 8, 8, 32), new(32, 8, 8, 32), new(32, 8, 8, 32), new(32, 8, 8, 32),
            new(32, 8, 8, 32), new(32, 8, 8, 32), new(32, 12, 12, 32), new(32, 12, 12, 32),
            new(24, 16, 16, 24), new(24, 16, 16, 24),
        ]);
        Add(SpazerSBATrail_WaveSpazer_0,
        [
            new(0, 0, 0, 0), new(16, 0, -16, 0), new(0, 0, 0, 0), new(-16, 0, 16, 0),
        ]);
        Add(SpazerSBATrail_WaveSpazer_1,
        [
            new(0, 0, 0, 0), new(-10, -10, 10, 10), new(0, 0, 0, 0), new(10, 10, -10, -10),
        ]);
        Add(SpazerSBATrail_WaveSpazer_2,
        [
            new(0, 0, 0, 0), new(0, -16, 0, 16), new(0, 0, 0, 0), new(0, 16, 0, -16),
        ]);
        Add(SpazerSBATrail_WaveSpazer_3,
        [
            new(0, 0, 0, 0), new(10, -10, -10, 10), new(0, 0, 0, 0), new(-10, 10, 10, -10),
        ]);
        Add(UNSUED_SpazerSBATrail_Spazer_IceSpazer_0_9BB38F,
        [
            new(0, 0, 0, 0), new(-8, 8, 8, 8), new(-16, 8, 16, 8),
        ]);
        Add(UNSUED_SpazerSBATrail_Spazer_IceSpazer_1_9BB39B,
        [
            new(0, 0, 0, 0), new(-8, -8, 8, -8), new(-16, -8, 16, -8),
        ]);
        return result.ToFrozenDictionary();
    }

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
        // intervening pointer tables. Dictionary membership excludes those gaps.
        int pointerAddress = address % 2 == 1 ? address : address - 1;
        if (Pointers.TryGetValue(pointerAddress, out ushort pointer))
        { value = (byte)(pointer >> ((address - pointerAddress) * 8)); return true; }
        int coordinate = (address - UnchargedBeamTrails_Default_0) & 3;
        if (Frames.TryGetValue(address - coordinate, out Offset frame))
        {
            value = unchecked((byte)(coordinate switch { 0 => frame.LeftX, 1 => frame.LeftY, 2 => frame.RightX, _ => frame.RightY }));
            return true;
        }
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
            return (ushort)(bus.ReadByte(address) | bus.ReadByte(next) << 8);

        throw new InvalidDataException(
            $"Projectile trail direction pointer {source} is outside compiled data " +
            "and is not a mutable bank-$9B low-half alias.");
    }

    internal static ushort ReadCoordinateWord(ISnesAddressSpace bus, ushort operand, ushort index)
    {
        int address = (SamusProjectileRomData.Banks.PaletteAndTrailData + operand + index) & SnesCpuAddressLayout.AddressMask;
        bool hasLow = TryReadByte(address, out byte low);
        bool hasHigh = TryReadByte((address + 1) & SnesCpuAddressLayout.AddressMask, out byte high);
        if (hasLow && hasHigh) return (ushort)(low | high << 8);
        // Only a live-memory boundary read needs an adapter. Unknown hardware still uses
        // native operand-driven MDR/open-bus rules; normal compiled reads allocate nothing.
        // The adapter rejects uncompiled upper-ROM addresses rather than treating unrelated
        // cartridge bytes as physical trail coordinates.
        return SnesCpuOperandRead.ReadAbsoluteIndexedWord(new BoundaryBus(bus),
            (byte)(SamusProjectileRomData.Banks.PaletteAndTrailData >> 16), operand, index);
    }

    private sealed class BoundaryBus(ISnesAddressSpace bus) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (TryReadByte(address, out byte value))
                return value;

            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.IsUpperLoRomWindow)
                throw new InvalidDataException(
                    $"Projectile trail coordinate read reached uncompiled cartridge address {source}.");
            return bus.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => bus.WriteByte(address, value);
    }
}
