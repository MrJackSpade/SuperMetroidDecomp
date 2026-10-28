using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Typed view of the Ceres cinematics' Mode 1 VRAM (#630).</summary>
internal readonly struct CeresVram(SnesVram vram)
{
    /// <summary>The cinematic OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, CeresVramLayout.ObjectCharacters);

    /// <summary>The SPACE COLONY caption tilemap.</summary>
    public VramRegion SpaceColonyTilemap => new(vram, CeresVramLayout.SpaceColonyTilemap);

    /// <summary>The Zebes reveal tilemap.</summary>
    public VramRegion ZebesTilemap => new(vram, CeresVramLayout.ZebesTilemap);
}
