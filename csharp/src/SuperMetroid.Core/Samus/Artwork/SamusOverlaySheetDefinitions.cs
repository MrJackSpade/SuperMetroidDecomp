using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of the single sprites drawn over Samus's body (#1392): the arm cannon's cover in each
/// direction and opening frame. The Power Suit chest cover is part of the forward-facing body's own frames (#1513).
/// </summary>
public static class SamusOverlaySheetDefinitions
{
    /// <summary>The arm cannon's cover aimed in <paramref name="direction"/> at cover frame <paramref name="frame"/>.</summary>
    public static string ArmCannonFrame(SamusProjectileDirection direction, int frame) => $"ArmCannon.{direction}.{frame}";
}
