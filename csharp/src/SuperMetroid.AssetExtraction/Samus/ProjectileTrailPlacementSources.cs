using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where each trail placement list is stored in bank $9B.</summary>
internal static class ProjectileTrailPlacementSources
{
    /// <summary>Bank $9B, which holds the trail tables and placements.</summary>
    internal const int Bank = 0x9b0000;

    /// <summary>The first bank-$9B offset the cartridge serves; below it lies direct-page scratch and hardware.</summary>
    internal const int CartridgeHalfStart = 0x8000;

    /// <summary>Bytes in one placement record: the left and right sparkles' signed X and Y offsets.</summary>
    internal const int RecordByteCount = 4;

    /// <summary>The bank-$9B address of each list's first placement.</summary>
    internal static readonly FrozenDictionary<ProjectileTrailPlacementList, ushort> Addresses =
        new Dictionary<ProjectileTrailPlacementList, ushort>
        {
            [ProjectileTrailPlacementList.UnchargedDefault] = 0xa56f,
            [ProjectileTrailPlacementList.UnchargedWaveVertical] = 0xa58f,
            [ProjectileTrailPlacementList.UnchargedWaveHorizontal] = 0xa5cf,
            [ProjectileTrailPlacementList.UnchargedWaveRisingDiagonal] = 0xa60f,
            [ProjectileTrailPlacementList.UnchargedWaveFallingDiagonal] = 0xa64f,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUp] = 0xa68f,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDown] = 0xa69b,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUpRight] = 0xa6a7,
            [ProjectileTrailPlacementList.UnchargedIceSpazerRight] = 0xa6b3,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDownRight] = 0xa6bf,
            [ProjectileTrailPlacementList.UnchargedIceSpazerDownLeft] = 0xa6cb,
            [ProjectileTrailPlacementList.UnchargedIceSpazerLeft] = 0xa6d7,
            [ProjectileTrailPlacementList.UnchargedIceSpazerUpLeft] = 0xa6e3,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUp] = 0xa6ef,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpRight] = 0xa717,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerRight] = 0xa73f,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDownRight] = 0xa767,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDown] = 0xa78f,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerDownLeft] = 0xa7b7,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerLeft] = 0xa7df,
            [ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpLeft] = 0xa807,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUp] = 0xa82f,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUpRight] = 0xa837,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaRight] = 0xa83f,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDownRight] = 0xa847,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDown] = 0xa84f,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaDownLeft] = 0xa857,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaLeft] = 0xa85f,
            [ProjectileTrailPlacementList.UnchargedIcePlasmaUpLeft] = 0xa867,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUp] = 0xa86f,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUpRight] = 0xa893,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaRight] = 0xa8b7,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDownRight] = 0xa8db,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDown] = 0xa8ff,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaDownLeft] = 0xa923,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaLeft] = 0xa947,
            [ProjectileTrailPlacementList.UnchargedWaveIcePlasmaUpLeft] = 0xa96b,
            [ProjectileTrailPlacementList.ChargedDefault] = 0xaa07,
            [ProjectileTrailPlacementList.ChargedWaveVertical] = 0xaa27,
            [ProjectileTrailPlacementList.ChargedWaveHorizontal] = 0xaa67,
            [ProjectileTrailPlacementList.ChargedWaveRisingDiagonal] = 0xaaa7,
            [ProjectileTrailPlacementList.ChargedWaveFallingDiagonal] = 0xaae7,
            [ProjectileTrailPlacementList.ChargedIceSpazerUp] = 0xab27,
            [ProjectileTrailPlacementList.ChargedIceSpazerUpRight] = 0xab4f,
            [ProjectileTrailPlacementList.ChargedIceSpazerRight] = 0xab77,
            [ProjectileTrailPlacementList.ChargedIceSpazerDownRight] = 0xab9f,
            [ProjectileTrailPlacementList.ChargedIceSpazerDown] = 0xabc7,
            [ProjectileTrailPlacementList.ChargedIceSpazerDownLeft] = 0xabef,
            [ProjectileTrailPlacementList.ChargedIceSpazerLeft] = 0xac17,
            [ProjectileTrailPlacementList.ChargedIceSpazerUpLeft] = 0xac3f,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUp] = 0xac67,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUpRight] = 0xacc7,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerRight] = 0xad27,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDownRight] = 0xad87,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDown] = 0xade7,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerDownLeft] = 0xae47,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerLeft] = 0xaea7,
            [ProjectileTrailPlacementList.ChargedWaveIceSpazerUpLeft] = 0xaf07,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUp] = 0xaf67,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUpRight] = 0xaf87,
            [ProjectileTrailPlacementList.ChargedIcePlasmaRight] = 0xafa7,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDownRight] = 0xafc7,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDown] = 0xafe7,
            [ProjectileTrailPlacementList.ChargedIcePlasmaDownLeft] = 0xb007,
            [ProjectileTrailPlacementList.ChargedIcePlasmaLeft] = 0xb027,
            [ProjectileTrailPlacementList.ChargedIcePlasmaUpLeft] = 0xb047,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUp] = 0xb067,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUpRight] = 0xb0bf,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaRight] = 0xb117,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDownRight] = 0xb16f,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDown] = 0xb1c7,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaDownLeft] = 0xb21f,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaLeft] = 0xb277,
            [ProjectileTrailPlacementList.ChargedWaveIcePlasmaUpLeft] = 0xb2cf,
            [ProjectileTrailPlacementList.SbaWaveSpazerVertical] = 0xb33b,
            [ProjectileTrailPlacementList.SbaWaveSpazerRisingDiagonal] = 0xb34b,
            [ProjectileTrailPlacementList.SbaWaveSpazerHorizontal] = 0xb35b,
            [ProjectileTrailPlacementList.SbaWaveSpazerFallingDiagonal] = 0xb36b,
            [ProjectileTrailPlacementList.SbaSpazer] = 0xb38f,
            [ProjectileTrailPlacementList.SbaSpazerDown] = 0xb39b,
        }.ToFrozenDictionary();
}
