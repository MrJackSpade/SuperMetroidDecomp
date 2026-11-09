namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The half pictures one installed animation frame of a pose's body selects (#1517, #1585), which the import composes its
/// whole picture from: always a top picture, and a bottom picture unless the frame names none. A frame that names no
/// bottom picture is composed with no bottom-half characters; nothing an earlier frame selected carries into it.
/// </summary>
/// <param name="Top">The top half's picture.</param>
/// <param name="Bottom">The bottom half's picture, or null for a frame that names none.</param>
public readonly record struct SamusBodyFramePictures(SamusBodyPicture Top, SamusBodyPicture? Bottom);
