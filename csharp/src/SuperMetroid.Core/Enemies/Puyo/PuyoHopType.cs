namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The seven hop records of <c>PuyoHopTable</c> at $A2:9A07. This is a discriminant,
/// not a flags word: every value selects one eight-byte height/speed/function record.
/// </summary>
public enum PuyoHopType : ushort
{
    /// <summary>Record zero at $A2:9A07: 16-pixel height, packed 8.8 X speed $0100, and the short-hop function; ordinary distant launches may randomize to record one.</summary>
    NormalSmall = 0,
    /// <summary>Record one at $A2:9A0F: 32-pixel height, packed 8.8 X speed $0100, and the big-hop function; proximity to Samus permits random selection among normal records zero through two.</summary>
    NormalBig = 1,
    /// <summary>Record two at $A2:9A17: the third ordinary hop, reached only by chance once Samus is within the proximity radius.</summary>
    NormalHigh = 2,
    /// <summary>Record three at $A2:9A1F: 128-pixel height and packed 8.8 X speed $0140, whose fractional byte is discarded by NTSC movement; selected after a post-drop bounce.</summary>
    Giant = 3,
    /// <summary>Record four at $A2:9A27: no horizontal motion and packed 8.8 downward speed $0100; wall or ceiling collision selects this fall before a recovery bounce.</summary>
    Dropping = 4,
    /// <summary>Record five at $A2:9A2F: 16-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; one of two randomized recovery heights.</summary>
    DroppedSmall = 5,
    /// <summary>Record six at $A2:9A37: 21-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; the taller randomized recovery height.</summary>
    DroppedBig = 6,
}
