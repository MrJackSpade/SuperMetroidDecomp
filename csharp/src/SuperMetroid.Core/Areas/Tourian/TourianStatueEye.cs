using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian boss statue's eye, where its release's eye glow and soul appear.</summary>
/// <param name="Position">The room position of the eye, shared by the eye glow and the released soul.</param>
/// <param name="Glow">The eye glow the statue scene shows once the eye lights.</param>
public sealed record TourianStatueEye(Position16<RoomPixels> Position, PictureShade Glow);
