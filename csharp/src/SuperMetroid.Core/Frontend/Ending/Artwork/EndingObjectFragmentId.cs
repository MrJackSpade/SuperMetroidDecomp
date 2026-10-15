namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Identities of the four native explosion tilemap fragments uploaded to successive VRAM word pages.</summary>
public enum EndingObjectFragmentId
{
    /// <summary>Wide explosion map from <c>$98:B5C1</c>, <c>Wide_Part_of_Zebes_Explosion_Tilemap</c>, uploaded at VRAM word $7000 (byte $E000).</summary>
    Segment70,
    /// <summary>Concentric wide explosion map from <c>$98:B857</c>, <c>Concentric_Wide_Part_of_Zebes_Explosion_Tilemap</c>, uploaded at VRAM word $7400 (byte $E800).</summary>
    Segment74,
    /// <summary>Eclipse map from <c>$98:BAED</c>, <c>Eclipse_of_Zebes_during_Explosion_Tilemap</c>, uploaded at VRAM word $7800 (byte $F000).</summary>
    Segment78,
    /// <summary>Blank map from <c>$98:BCCD</c>, <c>Blank_BG2_Tilemap</c>, uploaded at VRAM word $7C00 (byte $F800).</summary>
    Segment7C,
}
