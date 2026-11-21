using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Lower Norfair Ridley's death as the game runs it (#2554): his room's population load places him, his body's main AI runs
/// the death phase his encounter stands in, and his breakup's pieces fly and show their pictures by their own AI and
/// animation, so a check observes what his death does rather than driving its private steps.
/// </summary>
internal static class NorfairRidleyDeathFixtures
{
    /// <summary>The most updates any breakup piece flies before it expires, beyond which a check's piece never expires.</summary>
    private const int LongestFlight = 256;

    /// <summary>
    /// Places Lower Norfair Ridley in <paramref name="enemies"/>' first place as his room's population load places him, in a
    /// room that has not seen him beaten, with <paramref name="after"/> listed after him.
    /// </summary>
    /// <returns>His body's place and the encounter his body began.</returns>
    internal static (RoomEnemyPlace Body, NorfairRidleyState State) PlaceRidley(RoomEnemySystem enemies, params RoomEnemyPlacement[] after)
    {
        Ensure.NotNull(enemies).VerificationServices().IsAreaBossDefeated ??= () => false;
        RoomEnemyPlace body = enemies.LoadPopulation([PopulationPlacementAccess.Entry(EnemyDefinitionId.Ridley), .. after])[0];
        return (body, body.EntityOf<NorfairRidleyEntity>().Encounter);
    }

    /// <summary>
    /// Runs one update of Ridley's main AI in <paramref name="phase"/>, its countdown set to run out on its
    /// <paramref name="phaseUpdates"/>th update, his body holding still so that only the phase acts, beside
    /// <paramref name="samus"/>.
    /// </summary>
    internal static void RunPhase(RoomEnemyPlace body, NorfairRidleyState state, NorfairRidleyPhase phase, int phaseUpdates, SamusState samus)
    {
        Ensure.NotNull(state).Phase = phase;
        state.Motion = RidleyMotion.Still;
        state.PhaseTimer.LastFor(phaseUpdates);
        Ensure.NotNull(body).RunMainAi(samus);
    }

    /// <summary>The breakup pieces <paramref name="enemies"/> hold, in their places' order.</summary>
    internal static RoomEnemyPlace[] Pieces(RoomEnemySystem enemies) =>
        [.. Ensure.NotNull(enemies).Places.InUpdateOrder.Where(place => place.Holds(EnemyDefinitionId.RidleyBreakupPiece))];

    /// <summary>
    /// The picture breakup <paramref name="piece"/> shows: its animation's first step after its spawn shows it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The piece shows no picture.</exception>
    internal static EnemyFrameId ShowPicture(RoomEnemyPlace piece)
    {
        EnemyAnimationPlayer.AdvanceAnimation(Ensure.NotNull(piece), RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)),
            Position16<RoomPixels>.Zero, 0);
        return piece.Frame ?? throw new InvalidOperationException($"Breakup piece in place {piece.Ordinal} shows no picture.");
    }

    /// <summary>
    /// Runs breakup <paramref name="piece"/>'s main AI beside <paramref name="samus"/> until it expires, and returns the
    /// updates it flew, its last one included.
    /// </summary>
    /// <exception cref="InvalidOperationException">The piece flies past the longest breakup flight.</exception>
    internal static int FlyUntilExpiry(RoomEnemyPlace piece, SamusState samus)
    {
        for (int updates = 1; updates <= LongestFlight; updates++)
        {
            Ensure.NotNull(piece).RunMainAi(samus);
            if (piece.Occupant is null)
                return updates;
        }
        throw new InvalidOperationException($"Breakup piece in place {piece.Ordinal} flew {LongestFlight} updates without expiring.");
    }

    /// <summary>
    /// Where a breakup piece standing at <paramref name="start"/> with <paramref name="horizontalSpeed"/> and no vertical
    /// speed moves on its first update: its drag slows its horizontal speed and gravity starts its fall before it moves.
    /// </summary>
    internal static FixedPosition<RoomPixels> FirstMove(FixedPosition<RoomPixels> start, SignedEightEight horizontalSpeed)
    {
        SignedEightEight x = horizontalSpeed.SlowedBy(RidleyExplosionDefinitions.BreakupFragmentDrag);
        SignedEightEight y = default(SignedEightEight) + RidleyExplosionDefinitions.BreakupFragmentGravity;
        return start.Advance(new(x.ToSixteenSixteenDelta(), y.ToSixteenSixteenDelta()));
    }
}
