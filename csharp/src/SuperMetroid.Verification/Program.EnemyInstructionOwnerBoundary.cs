using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEnemyInstructionOwnerBoundary()
    {
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        MethodInfo read = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyInstructionMechanicsWord",
            staticFlags)!;

        var known = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = RoomEnemySystem.BoyonDefinition,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 },
        };
        AssertEqual(
            (ushort)0x817d,
            (ushort)read.Invoke(null, [known, BoyonInstructionProgramDefinitions.Idle])!,
            "known ordinary-enemy owner resolves compiled mechanics");

        var unknown = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = 0x9000,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 },
        };
        TargetInvocationException exception = AssertThrows<TargetInvocationException>(
            () => read.Invoke(null, [unknown, (ushort)0x9000]),
            "unknown ordinary-enemy mechanics owner fails explicitly");
        AssertTrue(exception.InnerException is InvalidDataException,
            "unknown ordinary-enemy mechanics owner surfaces InvalidDataException");
        AssertTrue(
            exception.InnerException!.Message.Contains("no compiled owner", StringComparison.Ordinal),
            "unknown ordinary-enemy mechanics error identifies missing compiled ownership");

        Console.WriteLine(
            "Ordinary-enemy mechanics boundary: known owners resolve compiled words and " +
            "unknown definitions fail explicitly without a cartridge fallback.");
    }
}
