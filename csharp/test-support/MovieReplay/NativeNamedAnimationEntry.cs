using SuperMetroid.Core.Animation;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>An animation a family's owners start by its own name (#2411), with the native place its play starts at.</summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <param name="Name">The animation's name, for failure messages.</param>
/// <param name="Animation">The animation.</param>
/// <param name="Entry">The native place it starts at.</param>
internal sealed record NativeNamedAnimationEntry<TSegment>(string Name, Animation<TSegment> Animation, ushort Entry)
    where TSegment : struct;
