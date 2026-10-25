
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The horizontal offset and program of one gunship dust cloud.</summary>
/// <param name="XOffset">The xoffset.</param>
/// <param name="Program">The program.</param>
internal readonly record struct GunshipDustPlacement(int XOffset, GunshipDustProgram Program);
