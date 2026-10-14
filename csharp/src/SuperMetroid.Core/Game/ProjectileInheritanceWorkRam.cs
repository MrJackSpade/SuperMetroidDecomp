namespace SuperMetroid.Core.Game;

/// <summary>The projectile-inheritance WRAM block (#629).</summary>
[CpuBusBoundary("catalog: projectile-inheritance WRAM")]
internal static class ProjectileInheritanceWorkRam
{
    /// <summary>
    /// <c>$7E:0DA8-$7E:0DB9</c>, addressed through the bank-$00 low-WRAM mirror: <c>CameraYSubSpeed</c>
    /// ($0DA8) followed by four signed 16.16 distance records, integer word then fraction word:
    /// <c>ProjSpeed_DistanceSamusMovedLeft</c> ($0DAA), <c>...Right</c> ($0DAE), <c>...Up</c> ($0DB2)
    /// and <c>...Down</c> ($0DB6). The launch-velocity load reads the word one byte before each
    /// record, across the preceding field's high byte.
    /// </summary>
    internal static readonly CpuBusCell Records = CpuBusCell.WramBytes(nameof(Records), 0x000da8, 18,
        "u16 camera Y subspeed, then signed 16.16 distance[4] (left, right, up, down)");
}
