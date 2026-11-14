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

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        RoomEnemyDefinition definition = CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(EnemyDefinitionId.Multiviola);
        slot.Bind(EnemyDefinitionId.Multiviola, definition);
        // The moves probe the terrain the Multiviola's hitbox spans, an open room's nothing.
        slot.Radius = definition.Radius;
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach (int speed in placements.Select(placement => placement.Speed).Distinct().Order())
        for (int heading = 0; heading < HeadingCount; heading++)
        {
            var angle = new TrigonometryAngle(heading);
            slot.SetSpawnSettings(new MultiviolaSpawnSettings(angle, speed));
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(Centre, Centre));
            MultiviolaBehavior.InitializeMultiviola(slot);
            enemies.MultiviolaBehavior.RunMultiviolaMainAi(slot, frame);

            // The original turns an axis left when the heading a quarter turn on lies in the turn's first half, and up
            // when the heading lies in its second half.
            var expected = new FixedPosition<RoomPixels>(
                Moved(EnemyMath.AbsoluteCosineSpeed(angle, speed), turnedLeftOrUp: ((heading + 0x40) & 0x80) == 0),
                Moved(EnemyMath.AbsoluteSineSpeed(angle, speed), turnedLeftOrUp: ((heading + 0x80) & 0x80) == 0));
            if (slot.Position != expected)
                throw new InvalidOperationException($"A Multiviola heading {heading} at speed {speed} moved to {slot.Position}, not {expected}.");
        }
        Console.WriteLine($"  Multiviola left-or-up speed: all {HeadingCount} headings at each of the {placements.Select(placement => placement.Speed).Distinct().Count()} authored speeds move as the original's main update.");
    }

    /// <summary>
    /// The centre moved by <paramref name="magnitude"/>, its whole word complemented and its fraction word negated first when
    /// <paramref name="turnedLeftOrUp"/>.
    /// </summary>
    private static FixedCoordinate Moved(UnsignedSixteenSixteen magnitude, bool turnedLeftOrUp)
    {
        int whole = turnedLeftOrUp ? magnitude.Whole ^ ushort.MaxValue : magnitude.Whole;
        int fraction = turnedLeftOrUp ? ((magnitude.Fraction ^ ushort.MaxValue) + 1) & ushort.MaxValue : magnitude.Fraction;
        uint place = unchecked(((uint)Centre << 16) + (((uint)whole << 16) | (uint)fraction));
        return FixedCoordinate.FromRaw(place);
    }
}
