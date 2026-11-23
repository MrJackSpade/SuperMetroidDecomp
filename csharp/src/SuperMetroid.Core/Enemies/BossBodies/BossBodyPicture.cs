using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// What a boss body frame shows (#2560): its whole picture and the depth it is drawn at among the room's layers (#1634),
/// one value so a picture cannot lack a depth or a depth stand without a picture.
/// </summary>
/// <param name="Picture">The frame's whole picture.</param>
/// <param name="Depth">The depth the picture is drawn at, as the import decided it from where the game draws the frame.</param>
public sealed partial record BossBodyPicture(SpriteSheetFrame Picture, BossBodyDepth Depth);
