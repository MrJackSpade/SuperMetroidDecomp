using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One of the arm cannon's cover pictures (#2556): the orientation it is drawn for and how far open it shows. The cartridge
/// holds one character of each; which aims show it is the extracted document's.
/// </summary>
/// <param name="Orientation">The orientation the picture is drawn for.</param>
/// <param name="Frame">How far open the picture shows.</param>
internal readonly record struct SamusArmCannonCoverTile(SamusArmCannonCoverOrientation Orientation, SamusArmCannonCoverFrame Frame);
