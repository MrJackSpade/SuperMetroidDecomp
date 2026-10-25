using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Verification's native view of Phantoon's identities and packed record words (#1413): the cartridge routine words
/// naming each member, and how the cartridge packs the swoop target and the death mosaic. Core's values are arbitrary;
/// a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PhantoonNativeWords
{
    /// <summary>The bank-$A7 routine word naming each <see cref="PhantoonAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PhantoonAiFunction> AiFunctions = new(new Dictionary<PhantoonAiFunction, ushort>
    {
        [PhantoonAiFunction.SpawnStartingFlames] = 0xd4a9,
        [PhantoonAiFunction.WaitBeforeActivatingStartingFlames] = 0xd4ee,
        [PhantoonAiFunction.WaitForStartingFlamesToDisappear] = 0xd508,
        [PhantoonAiFunction.WavyFadeIn] = 0xd54a,
        [PhantoonAiFunction.PickFirstRoundPattern] = 0xd596,
        [PhantoonAiFunction.MoveInFigureEightThenOpenEye] = 0xd5e7,
        [PhantoonAiFunction.EyeTracksSamus] = 0xd60d,
        [PhantoonAiFunction.BecomeSolidAndSwoop] = 0xd65c,
        [PhantoonAiFunction.Swooping] = 0xd678,
        [PhantoonAiFunction.FadeOutWhileSwooping] = 0xd6b9,
        [PhantoonAiFunction.WaitAfterFadeOut] = 0xd6d4,
        [PhantoonAiFunction.PickNextAppearance] = 0xd6e2,
        [PhantoonAiFunction.FadeInBeforeFigureEight] = 0xd72d,
        [PhantoonAiFunction.BecomeSolidAfterFlameRain] = 0xd73f,
        [PhantoonAiFunction.FadeInDuringFlameRain] = 0xd767,
        [PhantoonAiFunction.TrackSamusDuringFlameRain] = 0xd788,
        [PhantoonAiFunction.FadeOutDuringFlameRain] = 0xd7d5,
        [PhantoonAiFunction.SpawnFlameRain] = 0xd7f7,
        [PhantoonAiFunction.FadeOutBeforeFirstFlameRain] = 0xd82a,
        [PhantoonAiFunction.FadeOutBeforeRage] = 0xd85c,
        [PhantoonAiFunction.MoveToTopCenterForRage] = 0xd874,
        [PhantoonAiFunction.FadeInForRage] = 0xd891,
        [PhantoonAiFunction.Enraged] = 0xd8ac,
        [PhantoonAiFunction.FadeOutAfterRage] = 0xd916,
        [PhantoonAiFunction.FinishFatalSwoop] = 0xd92e,
        [PhantoonAiFunction.DyingFadeInOut] = 0xd948,
        [PhantoonAiFunction.DyingExplosions] = 0xd98b,
        [PhantoonAiFunction.BeginFinalWavyDeath] = 0xda51,
        [PhantoonAiFunction.DyingFadeOut] = 0xda86,
        [PhantoonAiFunction.AlmostDead] = 0xdad7,
        [PhantoonAiFunction.Dead] = 0xdb3d,
        [PhantoonAiFunction.NoOperation] = 0xd4a8,
    });

    /// <summary>The tentacles' word E bit set once the swoop target turns back.</summary>
    internal const int SwoopReturningBit = 0x8000;

    /// <summary>The eye's word C once the death mosaic has finished growing.</summary>
    internal const ushort DeathMosaicFinished = 0xffff;

    /// <summary>The eye's word C for a death-mosaic block size: the size less one in the high nibble, BG2 enabled.</summary>
    internal static ushort DeathMosaicWord(int size) => (ushort)((size - 1) * 16 | 0x02);

    /// <summary>
    /// Imports the packed views of a recorded Phantoon part's words: the body's function (word F), the tentacles' swoop
    /// target (word E) and the eye's death mosaic (word C, when it holds a mosaic word).
    /// </summary>
    internal static void Import(PhantoonPartState part, int partIndex, ushort wordC, ushort wordE, ushort wordF)
    {
        if (partIndex == 0 && wordF != 0)
            part.Function = AiFunctions.Decode(wordF, "recorded PhantoonAiFunction");
        if (partIndex == 2)
        {
            part.SwoopTargetX = wordE & ~SwoopReturningBit;
            part.SwoopReturning = (wordE & SwoopReturningBit) != 0;
        }
        if (partIndex == 1)
        {
            part.DeathMosaicFinished = wordC == DeathMosaicFinished;
            for (int size = PhantoonDeathDefinitions.InitialMosaicSize; size <= PhantoonDeathDefinitions.FinalMosaicSize; size++)
                if (wordC == DeathMosaicWord(size))
                    part.DeathMosaicSize = size;
        }
    }
}
