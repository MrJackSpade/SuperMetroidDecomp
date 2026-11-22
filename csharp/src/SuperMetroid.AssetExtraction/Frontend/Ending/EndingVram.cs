using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Typed view of the ending's VRAM allocations (#630).</summary>
internal readonly partial struct EndingVram(SnesVram vram)
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

    /// <summary>The post-credits transformation's characters.</summary>
    public VramRegion TransformationCharacters => new(vram, EndingVramLayout.TransformationCharacters);

    /// <summary>The post-credits transformation's map.</summary>
    public VramRegion TransformationMap => new(vram, EndingVramLayout.TransformationMap);

    /// <summary>The explosion OBJ fragment uploaded for <paramref name="fragment"/>.</summary>
    public VramRegion ObjectFragment(EndingExplosionPicture fragment) => new(vram, fragment switch
    {
        EndingExplosionPicture.WideExplosion => EndingVramLayout.WideExplosionCharacters,
        EndingExplosionPicture.ConcentricExplosion => EndingVramLayout.ConcentricExplosionCharacters,
        EndingExplosionPicture.Eclipse => EndingVramLayout.EclipseCharacters,
        EndingExplosionPicture.BurstGlow => EndingVramLayout.BurstGlowCharacters,
        _ => throw new ArgumentOutOfRangeException(nameof(fragment), fragment, "Not an explosion OBJ fragment."),
    });
}
