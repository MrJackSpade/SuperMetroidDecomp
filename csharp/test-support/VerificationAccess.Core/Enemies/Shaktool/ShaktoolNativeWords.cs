namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Verification's native view of the Shaktool identities (#1412): the cartridge routine words that stand for each member.
/// Core's values are arbitrary; a native snapshot or ROM oracle decodes through this table. Two empty routines stand
/// for <see cref="ShaktoolPreInstruction.Idle"/>, so checks decode a native word and compare identities.
/// </summary>
internal static class ShaktoolNativeWords
{
    /// <summary>The <see cref="ShaktoolPreInstruction"/> each routine word stands for.</summary>
    private static readonly Dictionary<int, ShaktoolPreInstruction> PreInstructionsByWord = new()
    {
        [0xdcaa] = ShaktoolPreInstruction.Idle,
        [0xdcab] = ShaktoolPreInstruction.Idle,
        [0xdcac] = ShaktoolPreInstruction.OrbitPreviousSegment,
        [0xdcd7] = ShaktoolPreInstruction.OrbitAndOrientCenter,
        [0xdd25] = ShaktoolPreInstruction.DriveTailAndReverseAtWalls,
    };

    /// <summary>The pre-instruction the routine <paramref name="word"/> stands for, or throws naming <paramref name="meaning"/>.</summary>
    internal static ShaktoolPreInstruction DecodePreInstruction(int word, string meaning) =>
        PreInstructionsByWord.TryGetValue(word, out ShaktoolPreInstruction preInstruction)
            ? preInstruction
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({nameof(ShaktoolPreInstruction)}).");
}
