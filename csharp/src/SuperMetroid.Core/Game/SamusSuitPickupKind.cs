namespace SuperMetroid.Core.Game;

/// <summary>The two native stage-three/stage-six variants of the shared transformation.</summary>
public enum SamusSuitPickupKind : byte
{
    /// <summary>The $91:D4E4 Varia pickup: stage three equips and collects the Varia suit, and the shrinking beam's fixed color returns toward orange.</summary>
    Varia,
    /// <summary>The $91:D5BA Gravity pickup: stage three equips and collects the Gravity suit, and the shrinking beam's fixed color returns toward blue.</summary>
    Gravity,
}
