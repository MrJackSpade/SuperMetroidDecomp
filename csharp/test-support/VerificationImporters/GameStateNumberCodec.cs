using SuperMetroid.Core;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The codec between a <see cref="SuperMetroidGameState"/> and the number the cartridge's game-state word holds (#1477), for
/// the snapshot import and the checks that compare with a recording's game-state word. The game's states carry
/// arbitrary values; the cartridge's numbering lives only here.
/// </summary>
[EncodingOwner("game-state number")]
internal static class GameStateNumberCodec
{
    /// <summary>
    /// The states in the cartridge's numbering. Numbers three (a no-operation entry), 28 (an unreachable entry) and 29 (a
    /// debug-build menu) name no state the game has.
    /// </summary>
    private static readonly SuperMetroidGameState?[] Numbered =
    [
        SuperMetroidGameState.Reset,
        SuperMetroidGameState.OpeningCinematic,
        SuperMetroidGameState.GameOptionsMenu,
        null,
        SuperMetroidGameState.FileSelectMenus,
        SuperMetroidGameState.FileSelectMap,
        SuperMetroidGameState.LoadingGameData,
        SuperMetroidGameState.MainGameplayFadeIn,
        SuperMetroidGameState.MainGameplay,
        SuperMetroidGameState.HitDoorBlock,
        SuperMetroidGameState.LoadingNextRoomA,
        SuperMetroidGameState.LoadingNextRoomB,
        SuperMetroidGameState.PausingDarkening,
        SuperMetroidGameState.Pausing,
        SuperMetroidGameState.PausedA,
        SuperMetroidGameState.PausedB,
        SuperMetroidGameState.UnpausingA,
        SuperMetroidGameState.UnpausingB,
        SuperMetroidGameState.Unpausing,
        SuperMetroidGameState.DeathSequenceStart,
        SuperMetroidGameState.DeathBlackOutSurroundings,
        SuperMetroidGameState.DeathWaitForMusic,
        SuperMetroidGameState.DeathPreFlashing,
        SuperMetroidGameState.DeathFlashing,
        SuperMetroidGameState.DeathExplosionWhiteOut,
        SuperMetroidGameState.DeathFinalBlackOut,
        SuperMetroidGameState.GameOverMenu,
        SuperMetroidGameState.ReserveTanksAuto,
        null,
        null,
        SuperMetroidGameState.IntroCinematic,
        SuperMetroidGameState.SetUpNewGame,
        SuperMetroidGameState.MadeItToCeresElevator,
        SuperMetroidGameState.BlackoutFromCeres,
        SuperMetroidGameState.CeresGoesBoom,
        SuperMetroidGameState.TimeUp,
        SuperMetroidGameState.WhitingOutFromTimeUp,
        SuperMetroidGameState.CeresGoesBoomWithSamus,
        SuperMetroidGameState.SamusEscapesFromZebes,
        SuperMetroidGameState.EndingAndCredits,
        SuperMetroidGameState.TransitionToDemoA,
        SuperMetroidGameState.TransitionToDemoB,
        SuperMetroidGameState.PlayingDemo,
        SuperMetroidGameState.TransitionFromDemoA,
        SuperMetroidGameState.TransitionFromDemoB,
    ];

    /// <summary>The state the cartridge numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">The number names no state the game has.</exception>
    public static SuperMetroidGameState Decode(int number) =>
        number >= 0 && number < Numbered.Length && Numbered[number] is { } state
            ? state
            : throw new InvalidDataException($"Game-state number {number} names no state the game has.");

    /// <summary>The cartridge's number for <paramref name="state"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="state"/> is undefined.</exception>
    public static int Encode(SuperMetroidGameState state) =>
        Array.IndexOf(Numbered, state) is var number and >= 0
            ? number
            : throw new ArgumentOutOfRangeException(nameof(state), state, "Undefined game state.");
}
