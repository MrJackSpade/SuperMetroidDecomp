using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Application-owned signed 16.16 window motion, in left/right/top/bottom order.</summary>
internal readonly record struct FileSelectMapWindowMotion(ushort Timer, uint Left, uint Right, uint Top, uint Bottom);

/// <summary>Calculates the native map-window velocities and timer from the stock label origin.</summary>
internal static class FileSelectMapWindowMotions
{
    /// <summary>
    /// $81:AA34..AA93 velocities and $81:AA94..AA9F timers for the six Zebes areas.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all original words and native consumers.
    /// Let D=max(x,256-x,y,256-y) for the stock label position at $81:AA1C+4*area.
    /// An edge at distance d travels floor(256*d/D)/64 pixels per call, negative for
    /// left/top. Quantize the positive magnitude before negating, then promote to signed
    /// 16.16. The farthest edge travels exactly four pixels per call. Timer=floor(D/4),
    /// and the native consumer moves on timer zero before completing at signed underflow.
    /// The generation square is 256 high even though the consumer clamps bottom at224.
    /// Integer numerators are at most65536; division precedes the ten-bit shift.
    /// Stock origins intentionally remain independent of editable label positions: edits
    /// move the initial window but did not change compiled motion in the prior contract.
    /// No generated velocity table or runtime cartridge read is used.
    /// </remarks>
    public static FileSelectMapWindowMotion Get(int area)
    {
        (int x, int y) = StockOrigin(area);
        int farthest = Math.Max(Math.Max(x, 256 - x), Math.Max(y, 256 - y));
        uint Velocity(int distance, bool negative)
        {
            int magnitude = (256 * distance / farthest) << 10;
            return unchecked((uint)(negative ? -magnitude : magnitude));
        }
        return new((ushort)(farthest / 4), Velocity(x, true), Velocity(256 - x, false),
            Velocity(y, true), Velocity(256 - y, false));
    }

    /// <summary>
    /// Original geographic label origins at $81:AA1C..AA33, used solely as stock motion
    /// inputs. Their coordinate disposition is separate from the derived motion review;
    /// the mutable presentation label layout remains independently imported/editable.
    /// </summary>
    internal static (int X, int Y) StockOrigin(int area) => area switch
    {
        (int)AreaId.Crateria => (91, 50),
        (int)AreaId.Brinstar => (42, 127),
        (int)AreaId.Norfair => (94, 181),
        (int)AreaId.WreckedShip => (206, 80),
        (int)AreaId.Maridia => (206, 159),
        (int)AreaId.Tourian => (135, 139),
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have world-map transitions.")
    };
}
