using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title's turning floor (#1467, #1538): one picture of the whole floor without the Baby Metroid lying on it, and one
/// picture of the Baby in each pose of its pulse animation, drawn over the floor where it lies. The import composes them
/// from the title's floor map and artwork.
/// </summary>
public static class TitleFloors
{
    /// <summary>The name of the whole floor's picture.</summary>
    public const string FloorFrameName = "floor";

    /// <summary>The floor with the Baby in each pose drawn over it, once read, so a pose keeps one floor while its sheet stands.</summary>
    private static readonly ConditionalWeakTable<SpriteSheetFrame, AffineFloor> floors = new();

    /// <summary>The name of the picture of the Baby in <paramref name="pose"/>.</summary>
    public static string BabyFrameName(TitleBabyPose pose) => $"baby-{pose}";

    /// <summary>The floor with the Baby in <paramref name="pose"/> drawn over it.</summary>
    /// <exception cref="InvalidDataException">The installed title floor has no picture of the floor or of the pose.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, TitleBabyPose pose)
    {
        SpriteSheet sheet = Ensure.NotNull(sheets).Get(SpriteSheetId.TitleFloor);
        SpriteSheetFrame floor = sheet.Frame(FloorFrameName);
        return floors.GetValue(sheet.Frame(BabyFrameName(pose)), baby => new AffineFloor(floor, [baby]));
    }
}
