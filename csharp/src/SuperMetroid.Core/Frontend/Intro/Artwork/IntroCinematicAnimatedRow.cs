namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening-palette CGRAM rows whose colors are derived from shared paint, valued by row index.</summary>
internal enum IntroCinematicAnimatedRow
{
    /// <summary>$8C:E4E9-E508: first object palette repeats a neutral ramp after its background slot.</summary>
    NeutralCycle = 8,
    /// <summary>$8C:E5A9-E5C8, Palettes_Intro_CrossFade; its first eight visible inks repeat palette2.</summary>
    CrossFade = 14,
}
