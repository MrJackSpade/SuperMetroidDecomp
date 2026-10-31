using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>One pixel row move a rot call hands its corpse.</summary>
/// <param name="PixelRow">The corpse pixel row moved.</param>
/// <param name="Operation">Whether the row is moved down or copied down.</param>
internal readonly record struct CorpseRowMove(int PixelRow, CorpseRowOperation Operation);
