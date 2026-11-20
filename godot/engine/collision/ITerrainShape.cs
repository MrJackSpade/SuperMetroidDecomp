using Engine.Shared;

namespace Engine.Collision;

/// <summary>A room's terrain as collision sees it: which pixels are solid ground.</summary>
public interface ITerrainShape
{
    /// <summary>Whether any pixel of <paramref name="rect"/> is solid terrain, with slopes treated as <paramref name="slopes"/> says.</summary>
    bool IsSolidIn(PixelRect rect, SlopeTreatment slopes);

    /// <summary>Whether the solid terrain in <paramref name="rect"/>, if any, includes sloped ground.</summary>
    bool HasSlopeIn(PixelRect rect);
}
