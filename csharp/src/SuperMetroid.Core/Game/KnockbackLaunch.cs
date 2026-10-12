using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>How a knockback starts: its horizontal direction and timer (#633).</summary>
public sealed record KnockbackLaunch
{
    /// <summary>Bank-$A0's `$0A54`: zero means move left, one means move right.</summary>
    public required ushort KnockbackXDirection { get; init; }

    /// <summary>Nonzero producer-owned timer in gameplay updates; normal contact supplies five, while scripted intro Rinka contact supplies eleven.</summary>
    public ushort KnockbackTimer { get; init; } = 5;
}
