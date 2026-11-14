using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>A cinematic sprite pose and the bank-$8C spritemap it shows (#1747).</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Spritemap">The spritemap the pose shows; null for the blank pose, which shows none.</param>
internal readonly record struct CinematicSpritemapSource(CinematicSpritePose Pose, SnesAddress? Spritemap);
