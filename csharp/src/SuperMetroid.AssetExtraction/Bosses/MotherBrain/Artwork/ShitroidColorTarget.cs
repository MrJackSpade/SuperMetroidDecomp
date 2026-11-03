namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Mutually exclusive sixteen-color initialization images for the live giant Baby Metroid encounter and its Sidehopper victim.</summary>
public enum ShitroidColorTarget
{
    /// <summary>Live Sidehopper victim target copied to palette colors $90–$9F during encounter initialization.</summary>
    Sidehopper,
    /// <summary>Giant Baby Metroid target copied to palette colors $A0–$AF; the first word is the retained transparent-slot payload.</summary>
    Shitroid,
    /// <summary>Drained Sidehopper corpse target copied to palette colors $F0–$FF for the encounter's victim transition.</summary>
    DeadSidehopper,
}
