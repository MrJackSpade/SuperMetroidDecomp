using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Verification's native view of Phantoon's identities and packed record words (#1413): the cartridge routine words
/// naming each member, and how the cartridge packs the swoop target and the death mosaic. Core's values are arbitrary;
/// a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PhantoonNativeWords
{
    /// <summary>
    /// The high byte of a destroyable flame's spawn parameter naming each <see cref="PhantoonFlameSpawnType"/>: the byte
    /// offset of the flame's initializer in its jump table. The low byte is the type's own index.
    /// </summary>
    internal static readonly NativeWordIdentities<PhantoonFlameSpawnType> FlameSpawnTypes = new(new Dictionary<PhantoonFlameSpawnType, ushort>
    {
        [PhantoonFlameSpawnType.Casual] = 0,
        [PhantoonFlameSpawnType.Enraged] = 2,
        [PhantoonFlameSpawnType.Rain] = 4,
        [PhantoonFlameSpawnType.Spiral] = 6,
    });

    /// <summary>The bank-$A7 routine word naming each <see cref="PhantoonPhase"/>.</summary>
    internal static readonly NativeWordIdentities<PhantoonPhase> Phases = new(new Dictionary<PhantoonPhase, ushort>
    {
        [PhantoonPhase.SpawnStartingFlames] = 0xd4a9,
        [PhantoonPhase.WaitBeforeActivatingStartingFlames] = 0xd4ee,
        [PhantoonPhase.WaitForStartingFlamesToDisappear] = 0xd508,
        [PhantoonPhase.WavyFadeIn] = 0xd54a,
        [PhantoonPhase.PickFirstRoundPattern] = 0xd596,
        [PhantoonPhase.MoveInFigureEightThenOpenEye] = 0xd5e7,
        [PhantoonPhase.EyeTracksSamus] = 0xd60d,
        [PhantoonPhase.BecomeSolidAndSwoop] = 0xd65c,
        [PhantoonPhase.Swooping] = 0xd678,
        [PhantoonPhase.FadeOutWhileSwooping] = 0xd6b9,
        [PhantoonPhase.WaitAfterFadeOut] = 0xd6d4,
        [PhantoonPhase.PickNextAppearance] = 0xd6e2,
        [PhantoonPhase.FadeInBeforeFigureEight] = 0xd72d,
        [PhantoonPhase.BecomeSolidAfterFlameRain] = 0xd73f,
        [PhantoonPhase.FadeInDuringFlameRain] = 0xd767,
        [PhantoonPhase.TrackSamusDuringFlameRain] = 0xd788,
        [PhantoonPhase.FadeOutDuringFlameRain] = 0xd7d5,
        [PhantoonPhase.SpawnFlameRain] = 0xd7f7,
        [PhantoonPhase.FadeOutBeforeFirstFlameRain] = 0xd82a,
        [PhantoonPhase.FadeOutBeforeRage] = 0xd85c,
        [PhantoonPhase.MoveToTopCenterForRage] = 0xd874,
        [PhantoonPhase.FadeInForRage] = 0xd891,
        [PhantoonPhase.Enraged] = 0xd8ac,
        [PhantoonPhase.FadeOutAfterRage] = 0xd916,
        [PhantoonPhase.FinishFatalSwoop] = 0xd92e,
        [PhantoonPhase.DyingFadeInOut] = 0xd948,
        [PhantoonPhase.DyingExplosions] = 0xd98b,
        [PhantoonPhase.BeginFinalWavyDeath] = 0xda51,
        [PhantoonPhase.DyingFadeOut] = 0xda86,
        [PhantoonPhase.AlmostDead] = 0xdad7,
        [PhantoonPhase.Dead] = 0xdb3d,
    });

    /// <summary>The bank-$A7 return the body's phase word names while it runs no phase.</summary>
    internal const ushort NoPhase = 0xd4a8;

    /// <summary>The tentacles' word E bit set once the swoop target turns back.</summary>
    internal const int SwoopReturningBit = 0x8000;

    /// <summary>The eye's word C once the death mosaic has finished growing.</summary>
    internal const ushort DeathMosaicFinished = 0xffff;

    /// <summary>The eye's word C for a death-mosaic block size: the size less one in the high nibble, BG2 enabled.</summary>
    internal static ushort DeathMosaicWord(int size) => (ushort)((size - 1) * 16 | 0x02);

    /// <summary>Imports the tentacles' swoop target from their recorded word E.</summary>
    internal static void ImportSwoopTarget(PhantoonTentaclesState part, ushort wordE)
    {
        part.SwoopTargetX = wordE & ~SwoopReturningBit;
        part.SwoopReturning = (wordE & SwoopReturningBit) != 0;
    }

    /// <summary>Imports the eye's death mosaic from its recorded word C, when it holds a mosaic word.</summary>
    internal static void ImportDeathMosaic(PhantoonEyeState part, ushort wordC)
    {
        part.DeathMosaicFinished = wordC == DeathMosaicFinished;
        for (int size = PhantoonDeathDefinitions.InitialMosaicSize; size <= PhantoonDeathDefinitions.FinalMosaicSize; size++)
            if (wordC == DeathMosaicWord(size))
                part.DeathMosaicSize = size;
    }
}
