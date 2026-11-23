using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>
/// Checks the Multiviola's left-or-up speed (#1620) through its production update against the original main update's
/// arithmetic.
/// </summary>
internal static class MultiviolaLeftOrUpSpeedVerification
{
    /// <summary>The headings a turn holds.</summary>
    private const int HeadingCount = 256;

    /// <summary>A whole place far from the open room's solid edge, on both axes.</summary>
    private const int Centre = 512;

    /// <summary>
    /// Every heading at every speed an authored Multiviola has, run through one production update in an open room: each
    /// axis moves by its heading's speed, complemented in the whole pixels and negated in the fraction when it points left
    /// or up, as the original's main update leaves it. An authored Multiviola heads along an axis, so the speed of nothing
    /// that turns into a pixel an update is reached.
    /// </summary>
    internal static void VerifyMultiviolaLeftOrUpSpeed()
    {
        MultiviolaSpawnSettings[] placements = [.. RoomEnemyPopulationDefinitions.Definitions
            .SelectMany(list => list.Placements.ToArray())
            .Where(placement => placement.DefinitionId == EnemyDefinitionId.Multiviola)
            .Select(placement => (MultiviolaSpawnSettings)placement.SpawnSettings)];
        if (!placements.Any(placement => placement.Heading.RawValue % TrigonometryAngle.QuarterTurn.RawValue == 0))
            throw new InvalidOperationException("No authored Multiviola heads along an axis, so none reaches a speed of nothing turned left or up.");

        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
        foreach (int speed in placements.Select(placement => placement.Speed).Distinct().Order())
        for (int first = 0; first < HeadingCount; first += EnemyPlaceOrder.Capacity)
        {
            // Each room's population load sets up a room full of Multiviolas at the centre, one per heading (#2554); its
            // species builds each one's entity, whose setup builds its state. The moves probe the terrain each one's hitbox
            // spans, an open room's nothing.
            int count = Math.Min(EnemyPlaceOrder.Capacity, HeadingCount - first);
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation([.. Enumerable.Range(first, count).Select(heading =>
                PopulationPlacementAccess.Entry(EnemyDefinitionId.Multiviola, new MultiviolaSpawnSettings(new TrigonometryAngle(heading), speed), new(Centre, Centre)))]);
            for (int heading = first; heading < first + count; heading++)
            {
            var angle = new TrigonometryAngle(heading);
            RoomEnemyPlace place = places[heading - first];
            place.RunMainAi(frame);

            // The original turns an axis left when the heading a quarter turn on lies in the turn's first half, and up
            // when the heading lies in its second half.
            var expected = new FixedPosition<RoomPixels>(
                Moved(HalfWaveSpeed(angle.TurnedBy(TrigonometryAngle.QuarterTurn), speed), turnedLeftOrUp: ((heading + 0x40) & 0x80) == 0),
                Moved(HalfWaveSpeed(angle.TurnedBy(TrigonometryAngle.HalfTurn), speed), turnedLeftOrUp: ((heading + 0x80) & 0x80) == 0));
            if (place.Position != expected)
                throw new InvalidOperationException($"A Multiviola heading {heading} at speed {speed} moved to {place.Position}, not {expected}.");
            }
        }
        Console.WriteLine($"  Multiviola left-or-up speed: all {HeadingCount} headings at each of the {placements.Select(placement => placement.Speed).Distinct().Count()} authored speeds move as the original's main update.");
    }

    /// <summary>The original's unsigned product of <paramref name="speed"/> whole pixels and the half-wave at <paramref name="angle"/>.</summary>
    private static UnsignedSixteenSixteen HalfWaveSpeed(TrigonometryAngle angle, int speed) =>
        UnsignedSixteenSixteen.ShareOfWholePixels(speed, Trigonometry.UnsignedHalfWave(angle.StepsIntoHalfTurn));

    /// <summary>
    /// The centre moved by <paramref name="magnitude"/>, its whole word complemented and its fraction word negated first when
    /// <paramref name="turnedLeftOrUp"/>.
    /// </summary>
    private static FixedCoordinate Moved(UnsignedSixteenSixteen magnitude, bool turnedLeftOrUp)
    {
        int whole = turnedLeftOrUp ? magnitude.Whole ^ ushort.MaxValue : magnitude.Whole;
        int fraction = turnedLeftOrUp ? ((magnitude.Fraction ^ ushort.MaxValue) + 1) & ushort.MaxValue : magnitude.Fraction;
        uint place = unchecked(((uint)Centre << 16) + (((uint)whole << 16) | (uint)fraction));
        return SixteenSixteenWords.ToCoordinate(place);
    }
}
