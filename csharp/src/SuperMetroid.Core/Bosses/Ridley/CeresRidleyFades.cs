using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's eye and body palette fades.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFades
{
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFades(EnemyPresentation presentation, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _presentation = presentation;
    }

    /// <summary>Advances eye palette rows on each AI call before enabling the body fade.</summary>
    internal void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The original INC/BNE sequence advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
        if (state.FunctionTimer.IsZero)
            return;
        state.FunctionTimer = WrappingCountdown.Start(0);

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFadeDefinitions.Get(state.FadePaletteOffset);
        if (fadeStep.IsComplete)
        {
            state.FadePaletteOffset = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = true;
            return;
        }

        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley eye fade requires installed colors."))
            .ApplyEyeFade(_presentation.Cgram, fadeStep.PaletteRow);
        state.FadePaletteOffset++;
    }

    /// <summary>Reveals body palette rows, clears intangible collision and queues the battle track at the final row.</summary>
    internal void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
        if (state.FunctionTimer.RawValue < 2)
            return;
        state.FunctionTimer = WrappingCountdown.Start(0);

        // The cartridge steps a colour offset one row of colours at a time; the row it reaches is the fade's progress.
        int row = state.FadePaletteOffset;
        if (_bossArtwork.CeresRidleyColors is { } bodyColors &&
            row < CeresRidleyPaletteRomData.BodyFadeRowCount)
            bodyColors.ApplyBodyFade(_presentation.Cgram, row);
        else throw new InvalidOperationException(
            $"Ceres Ridley body-fade row {row} is outside the installed color rows.");
        state.FadePaletteOffset++;
        if (state.FadePaletteOffset < CeresRidleyPaletteRomData.BodyFadeRowCount)
            return;

        // CeresRidley_Func_5 clears tangible bit $0400 after the final palette row, waits
        // five dispatcher calls, then begins the opening-roar list.
        slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.FadePaletteOffset = 0;
        state.Function = RidleyAiFunction.WaitBeforeRoar;
        state.FunctionTimer = WrappingCountdown.Start(4);
        // `$A6:A449` queues track five immediately after the final body-palette row.
        // This is the Ceres battle/escape track; it intentionally continues after Ridley
        // retreats, so omitting this single publication silences both scenes.
        state.MusicRequest = MusicRequest.SelectTrack(5);
    }
}
