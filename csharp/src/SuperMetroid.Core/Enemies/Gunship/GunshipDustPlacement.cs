
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The horizontal offset and program of one gunship dust cloud.</summary>
/// <param name="XOffset">The cloud's horizontal offset from Samus, in pixels.</param>
/// <param name="Animation">The typed program the cloud starts.</param>
internal readonly record struct GunshipDustPlacement(int XOffset, GunshipDustSegment Animation);
