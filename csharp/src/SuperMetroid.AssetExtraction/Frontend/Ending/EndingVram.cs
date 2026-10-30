using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Typed view of the ending's VRAM allocations (#630).</summary>
internal readonly struct EndingVram(SnesVram vram)
{
    /// <summary>The escape/explosion OBJ sheet.</summary>
    public VramRegion ExplosionObjects => new(vram, EndingVramLayout.ExplosionObjects);

    /// <summary>The ending font OBJ characters.</summary>
    public VramRegion EndingFont => new(vram, EndingVramLayout.EndingFont);

    /// <summary>The waiting-Samus characters.</summary>
    public VramRegion WaitingCharacters => new(vram, EndingVramLayout.WaitingCharacters);

    /// <summary>The waiting-Samus tilemap.</summary>
    public VramRegion WaitingTilemap => new(vram, EndingVramLayout.WaitingTilemap);

    /// <summary>The cloud and shooting-scene OBJ characters.</summary>
    public VramRegion PostCreditsObjects => new(vram, EndingVramLayout.PostCreditsObjects);

    /// <summary>The post-credits reward characters.</summary>
    public VramRegion RewardCharacters => new(vram, EndingVramLayout.RewardCharacters);

    /// <summary>Post-credits fragment A.</summary>
    public VramRegion PostCreditsFragmentA => new(vram, EndingVramLayout.PostCreditsFragmentA);

    /// <summary>Post-credits fragment B.</summary>
    public VramRegion PostCreditsFragmentB => new(vram, EndingVramLayout.PostCreditsFragmentB);

    /// <summary>The explosion OBJ fragment uploaded for <paramref name="fragment"/>.</summary>
    public VramRegion ObjectFragment(EndingObjectFragmentId fragment) => new(vram, fragment switch
    {
        EndingObjectFragmentId.Segment70 => EndingVramLayout.Fragment70,
        EndingObjectFragmentId.Segment74 => EndingVramLayout.Fragment74,
        EndingObjectFragmentId.Segment78 => EndingVramLayout.Fragment78,
        EndingObjectFragmentId.Segment7C => EndingVramLayout.Fragment7C,
        _ => throw new ArgumentOutOfRangeException(nameof(fragment), fragment, "Not an explosion OBJ fragment."),
    });
}
