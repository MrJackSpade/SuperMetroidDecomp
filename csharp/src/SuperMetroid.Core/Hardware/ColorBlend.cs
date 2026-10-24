namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Which layers a colour blend applies to and how it combines (#1407): the layers whose shown pixel blends, whether the
/// blend subtracts rather than adds, and whether it halves the result. The other operand is the layers behind or a
/// fixed colour (<see cref="Rendering.SceneBlend"/>).
/// </summary>
/// <remarks>The members combine freely. The Direct3D 11 backend uploads a combination as the bits below.</remarks>
[Flags]
public enum ColorBlend : byte
{
    /// <summary>No layer blends.</summary>
    None = 0,
    /// <summary>The foreground (background layer 1) blends.</summary>
    Bg1 = 1,
    /// <summary>The background (background layer 2) blends.</summary>
    Bg2 = 2,
    /// <summary>Background layer 3 blends.</summary>
    Bg3 = 4,
    /// <summary>Sprites drawn in the four blending palettes blend; sprites in the other four never do.</summary>
    Sprites = 16,
    /// <summary>The backdrop blends where no layer covers it.</summary>
    Backdrop = 32,
    /// <summary>The blend halves its result, where the other operand is a shown layer or the fixed colour alone.</summary>
    Half = 64,
    /// <summary>The blend subtracts the other operand instead of adding it.</summary>
    Subtract = 128,
}
