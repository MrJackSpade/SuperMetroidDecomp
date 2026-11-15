using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the looping block animations (<see cref="BlockAnimationLoopDefinitions"/>): an animation's loop, the loop a
/// room effect's type shows on the effect overlay and the treadmill a Wrecked Ship entrance door starts. An undefined
/// animation, effect type or direction is rejected.
/// </summary>
public static class BlockAnimationLoops
{
    /// <summary>Resolves the loop of <paramref name="animation"/>; an animation with no loop resolves to false and null.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined.</exception>
    public static bool TryResolve(BlockAnimation animation, [NotNullWhen(true)] out BlockAnimationLoop? loop)
    {
        if (!Enum.IsDefined(animation))
            throw new InvalidOperationException($"Undefined {nameof(BlockAnimation)} {animation}.");
        return BlockAnimationLoopDefinitions.Loops.TryGetValue(animation, out loop);
    }

    /// <summary>The loop of <paramref name="animation"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    public static BlockAnimationLoop Of(BlockAnimation animation) =>
        TryResolve(animation, out BlockAnimationLoop? loop) ? loop
            : throw new InvalidOperationException($"{animation} has no block animation loop.");

    /// <summary>The loop a room effect of <paramref name="type"/> shows on the effect overlay, or null for one that shows none.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is undefined.</exception>
    public static BlockAnimationLoop? OnEffectOverlay(RoomFxType type)
    {
        if (!Enum.IsDefined(type))
            throw new InvalidOperationException($"Undefined {nameof(RoomFxType)} {type}.");
        return BlockAnimationLoopDefinitions.EffectOverlayAnimations.TryGetValue(type, out BlockAnimation animation)
            ? Of(animation) : null;
    }

    /// <summary>The treadmill animation a Wrecked Ship entrance door starts for a belt pushing <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is undefined.</exception>
    public static BlockAnimation Treadmill(WreckedShipTreadmillDirection direction) =>
        BlockAnimationLoopDefinitions.TreadmillAnimations.TryGetValue(direction, out BlockAnimation animation) ? animation
            : throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}.");
}
