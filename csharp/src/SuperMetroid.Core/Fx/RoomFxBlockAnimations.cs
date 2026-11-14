using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the looping room-FX animations (<see cref="RoomFxBlockAnimationMechanicsDefinitions"/>): every other defined
/// animation has no loop here, and an undefined one is rejected.
/// </summary>
public static class RoomFxBlockAnimations
{
    /// <summary>Resolves a looping animation; any other defined animation resolves to false and null.</summary>
    public static bool TryResolve(BlockAnimation animatedObject, [NotNullWhen(true)] out RoomFxBlockAnimationLoop? definition)
    {
        definition = SelectObject(animatedObject);
        return definition is not null;
    }

    /// <summary>The loop of a looping animation, or null for any other defined animation.</summary>
    private static RoomFxBlockAnimationLoop? SelectObject(BlockAnimation animatedObject) =>
        Enum.IsDefined(animatedObject)
            ? RoomFxBlockAnimationMechanicsDefinitions.Objects.GetValueOrDefault(animatedObject)
            : throw new InvalidOperationException($"Undefined {nameof(BlockAnimation)} {animatedObject}.");
}
