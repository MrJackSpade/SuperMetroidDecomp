namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Chooses the spinning animation an Atomic starts with.</summary>
internal static class AtomicSpins
{
    /// <summary>The spinning animation the enemy population's first parameter selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="parameter"/> selects none of the four appearances.</exception>
    internal static AtomicProgram InitialProgram(int parameter) =>
        (uint)parameter < (uint)AtomicMovementDefinitions.InitialPrograms.Length
            ? AtomicMovementDefinitions.InitialPrograms[parameter]
            : throw new InvalidDataException(
                $"Atomic instruction selector ${parameter:X4} is outside the four authored appearances.");
}
