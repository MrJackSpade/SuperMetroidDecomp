using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Which room states' library backgrounds the import pictures (#1649): only those the player sees as their own pictures.
/// Every other background's view is confirmed against the state that loads it, so the import never leaves a background
/// unpictured where nothing else draws what the player sees there:
/// <list type="bullet">
/// <item>The boss rooms' lists copy the boss's own layer, which the boss's initialisation has just cleared to its empty
/// tile; the boss then draws its body there, which its body sheet pictures.</item>
/// <item>The sky rooms' lists copy one sky page, which the sky's rows then show over; the sky draws those rows from its
/// own image.</item>
/// <item>The clearing lists fill the layer with the empty tile, which shows nothing over the common room elements, and in
/// the Ceres elevator shaft, whose door shows the room as the turning floor alone, the layer is not shown at all.</item>
/// </list>
/// </summary>
internal static class PicturedLibraryBackgrounds
{
    /// <summary>
    /// Whether <paramref name="state"/> shows its library background as that background's own picture, and the program
    /// that builds the picture.
    /// </summary>
    /// <param name="state">The room state.</param>
    /// <param name="program">The program building the state's background when it is pictured.</param>
    /// <returns>False for a state that loads no library background, or whose background the player sees otherwise.</returns>
    /// <exception cref="InvalidDataException">
    /// The state shows a boss's body on its background but places no boss whose body sheet draws it, shows the sky's rows
    /// without running the sky's bands, or its background's view is undefined.
    /// </exception>
    internal static bool TryProgramOf(RoomStateSettings state, [NotNullWhen(true)] out LibraryBackgroundProgram? program)
    {
        Ensure.NotNull(state);
        program = null;
        if (state.Background is not { } background)
            return false;
        switch (background.View)
        {
            case LibraryBackgroundView.Picture:
                program = LibraryBackgroundPrograms.Of(background);
                return true;
            case LibraryBackgroundView.BossBody:
                if (!state.EnemyPopulation.Placements.Span.Any(record => BossBodySheetComposer.DrawsBodyOf(record.DefinitionId)))
                {
                    throw new InvalidDataException(
                        $"Room state {state.Name} shows its boss's body on its background {background}, but places no boss whose body sheet draws it.");
                }
                return false;
            case LibraryBackgroundView.SkyRows:
                if (!ScrollingSkyState.IsScrollingSkyRoomMain(state.MainBehavior) || !ScrollingSkyState.SetupSpawnsBands(state.SetupBehavior))
                {
                    throw new InvalidDataException(
                        $"Room state {state.Name} shows the sky's rows on its background {background}, but does not run the sky's bands.");
                }
                return false;
            case LibraryBackgroundView.Nothing:
                return false;
            default:
                throw new InvalidDataException($"Library background {background} has undefined view {background.View}.");
        }
    }
}
