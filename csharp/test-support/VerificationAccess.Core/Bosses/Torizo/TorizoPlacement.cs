using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Places a Torizo in a check's enemies as his room's population places him (#2554): the population load sets him up,
/// building his state, and his own step and turning cues leave him in the posture the check names.
/// </summary>
internal static class TorizoPlacement
{
    /// <summary>A check's enemies, which a Torizo is placed in.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Places the Torizo of <paramref name="variant"/> (Bomb Torizo when none) in the room's place
        /// <paramref name="ordinal"/>, behind stand-ins in the places before it, as the room's population places him and sets
        /// him up in a room whose Torizo is not beaten; stands him at <paramref name="at"/>, since his setup moves him where his
        /// arena wakes him; and plays the step, and the turning cue when it turns, that leave him in the posture of the authored
        /// <paramref name="postureWord"/> (<see cref="TorizoParameterFlagWords.PostureOf"/>).
        /// </summary>
        /// <returns>The Torizo's place.</returns>
        internal RoomEnemyPlace PlaceTorizo(int postureWord, Position16<RoomPixels> at, TorizoVariant? variant = null, int ordinal = 0)
        {
            TorizoVariant placed = variant ?? TorizoVariant.Bomb;
            EnemyDefinitionId species = placed == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo;
            RoomEnemyPlacement[] entries =
            [
                .. Enumerable.Repeat(PopulationPlacementAccess.Entry(EnemyPlaceFixtures.StandIn), ordinal),
                PopulationPlacementAccess.Entry(species, at),
            ];
            RoomEnemyPlace torizo = Ensure.NotNull(enemies).LoadPopulation(entries)[ordinal];
            torizo.Position = torizo.Position.WithWhole(at);
            TorizoPosture posture = TorizoParameterFlagWords.PostureOf(postureWord);
            enemies.PlayTorizoCue(torizo, new TorizoCue.Stepped(posture.Facing, posture.LeadingFoot));
            if (posture.IsTurning)
                enemies.PlayTorizoCue(torizo, new TorizoCue.Signal(TorizoCueKind.SetTurningAround));
            return torizo;
        }

        /// <summary>
        /// Plays <paramref name="cue"/> on the Torizo in <paramref name="torizo"/> as his animation plays it, beside a lone
        /// Samus in an open room, and returns where it continues his animation, or none when it plays on.
        /// </summary>
        internal AnimationContinuation<TorizoBodySegment>? PlayTorizoCue(RoomEnemyPlace torizo, TorizoCue cue)
        {
            TorizoEntity entity = Ensure.NotNull(torizo).EntityOf<TorizoEntity>();
            var inputs = new TorizoCueInputs
            {
                Torizo = entity, Samus = RoomEnemyFrameFixtures.StandaloneSamus(), Level = RoomEnemyFrameFixtures.OpenRoom(new(64, 64)),
                ControllerInput = 0, FrameCounter = 0,
            };
            return Ensure.NotNull(enemies).TorizoBehavior.ApplyTorizoCue(torizo.HeldOccupant(), torizo, entity.State, inputs, cue, entity.State.BodyPlayback);
        }
    }
}
