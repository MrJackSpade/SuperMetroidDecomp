namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The shape and paint rules of the Shitroid's colours: its innard pulse and its three complete target images.</summary>
public static class ShitroidColorDefinitions
{
    /// <summary>The first colour of the Shitroid's initial image that the innard pulse repeats.</summary>
    public const int NormalFirstInitialColor = 4;

    /// <summary>The RGB5 units the innards dim by at each of the pulse's four levels before retracing. Organ-only painted light; independent of phase timing and cry scheduling.</summary>
    public const int NormalOrganDimmingStep = 5;

    /// <summary>The units above its target a full-red highlight starts at, so the first two levels clip to full red. Distinct from the dimming step.</summary>
    public const int NormalOrganRedHeadroom = 5;

    /// <summary>The darkest innard red, which stops here while green and blue reach zero. A paint intensity, not a gameplay threshold.</summary>
    public const int NormalOrganRedFloor = 5;

    /// <summary>The innard-pulse images, dimming and then retracing; the count does not encode the palette-update cadence or cry timing.</summary>
    public const int NormalFrameCount = 8;

    /// <summary>The colours in each innard-pulse image: the four innard inks of the Shitroid's colour set, not a complete set.</summary>
    public const int NormalColorsPerFrame = 4;

    /// <summary>The colours in each complete target image (sidehopper, Shitroid, dead sidehopper), including the transparent first colour.</summary>
    public const int TargetColorCount = 16;
}
