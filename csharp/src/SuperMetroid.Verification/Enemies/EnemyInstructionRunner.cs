using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Typed entry to the enemy instruction interpreter for fixtures that keep their call as an argument row:
/// slot, Samus, level, camera and controller input. A null value-type element is its default, as it was when
/// these rows were passed to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>; an element
/// of the wrong type is rejected.
/// </summary>
internal static class EnemyInstructionRunner
{
    /// <summary>Processes the slot's instruction list for one frame.</summary>
    internal static void Process(RoomEnemySystem enemies, object?[] arguments) =>
        enemies.Instructions.ProcessInstructions(
            arguments[0] as RoomEnemySlot ?? throw new ArgumentException("Element 0 must be the enemy slot.", nameof(arguments)),
            Optional<SamusState>(arguments, 1),
            Optional<RoomLevelData>(arguments, 2),
            Value<Position16<RoomPixels>>(arguments, 3),
            Value<SnesButton>(arguments, 4));

    /// <summary>
    /// Processes <paramref name="slot"/>'s animation for <paramref name="updates"/> updates with no Samus, level, camera or
    /// input, and lists each newly shown frame as <c>update:spritemap</c> (hexadecimal), comma-separated.
    /// </summary>
    internal static string ShownFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int updates)
    {
        object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        var shown = new List<string>();
        for (int update = 0; update < updates; update++)
        {
            Process(enemies, arguments);
            if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                shown.Add($"{update}:{slot.SpritemapPointer:X4}");
        }
        return string.Join(",", shown);
    }

    /// <summary>
    /// The <see cref="ShownFrames"/> listing of <paramref name="spritemaps"/> shown in turn, each held for its duration in
    /// <paramref name="durations"/>, starting on update zero.
    /// </summary>
    internal static string ExpectedFrames(IEnumerable<ushort> spritemaps, IEnumerable<int> durations)
    {
        int update = 0;
        var shown = new List<string>();
        foreach ((ushort spritemap, int duration) in spritemaps.Zip(durations))
        {
            shown.Add($"{update}:{spritemap:X4}");
            update += duration;
        }
        return string.Join(",", shown);
    }

    /// <summary>
    /// The <see cref="ShownFrames"/> listing of one pass through <paramref name="program"/>, a loop of shows that ends by
    /// going back to its start, followed by its first frame again; the generated ROM oracle ties the program itself to
    /// the cartridge, so this states only how the runtime must execute it.
    /// </summary>
    internal static string ExpectedLoop<TProgram, TFrame, TCue>(AnimationProgramSet<TProgram, TFrame, TCue> programs,
        TProgram program, Func<TFrame, ushort> spritemap)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var shows = programs.Steps(program).Where(step => step.Kind == AnimationProgramStepKind.Show).ToArray();
        return ExpectedFrames([.. shows.Select(step => spritemap(step.Frame)), spritemap(shows[0].Frame)],
            [.. shows.Select(step => step.Duration), shows[0].Duration]);
    }

    /// <summary>The update count <see cref="ExpectedLoop"/> covers: one pass plus the first update of the next.</summary>
    internal static int LoopUpdates<TProgram, TFrame, TCue>(AnimationProgramSet<TProgram, TFrame, TCue> programs, TProgram program)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        programs.Steps(program).Where(step => step.Kind == AnimationProgramStepKind.Show).Sum(step => step.Duration) + 1;

    /// <summary>Accepts a nullable fixture reference at an argument index and reports mismatched runtime types.</summary>
    private static T? Optional<T>(object?[] arguments, int index) where T : class => arguments[index] switch
    {
        null => null,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };

    /// <summary>Restores reflection's default-value behavior for a null fixture value type while rejecting wrong types.</summary>
    private static T Value<T>(object?[] arguments, int index) where T : struct => arguments[index] switch
    {
        null => default,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };
}
