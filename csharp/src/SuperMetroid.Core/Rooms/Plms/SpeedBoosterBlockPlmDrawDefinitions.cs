namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The one-block native bank-$84:A4F3 reveal used by bomb-special Speed Booster
/// terrain. Its type-$B level word remains physical even when appearance changes.
/// </summary>
internal static class SpeedBoosterBlockPlmDrawDefinitions
{
    /// <summary><c>$84:A4F3</c>: bomb reveal of one Speed Booster block.</summary>
    internal const string BombRevealVisualId = "bomb-reveal";

    /// <summary>The Speed Booster block's own draw; its crumble frames are the single shot-block breakup draws.</summary>
    internal enum Draw
    {
        /// <summary>Reveals the Speed Booster block when it is bombed.</summary>
        BombReveal,
    }

    /// <summary>The draw that reveals a bombed Speed Booster block.</summary>
    internal static PlmFrameDraw<Draw> BombRevealDraw { get; } = PlmDraw.Of(Draw.BombReveal);


    /// <summary>Physical type-B parent with the named Speed Booster visual block.</summary>
    internal static ushort BombRevealWord => 0xb000 | RoomPlmVisualBlockDefinitions.SpeedBoosterParent;

    /// <summary>Materializes the one-block native reveal only when artwork import/export requests it.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList BombReveal =>
        new(BombRevealDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(PlmRunDirection.Horizontal, new ushort[] { BombRevealWord }, new(0, 0)),
            });

    /// <summary>Enumerates the sole calculated bomb-reveal record for artwork import/export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => [BombReveal];
}
