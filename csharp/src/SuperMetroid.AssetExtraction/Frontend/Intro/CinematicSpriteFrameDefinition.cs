using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>One visual composition of a cinematic sprite family and its expected retail OAM entry count.</summary>
/// <param name="Name">Stable asset name identifying the visual composition.</param>
/// <param name="Pose">The cinematic sprite pose that shows this composition.</param>
/// <param name="StockPartCount">Retail composition's OAM part count for source comparison, not a required count for independently edited artwork.</param>
public readonly record struct CinematicSpriteFrameDefinition(string Name, CinematicSpritePose Pose, int StockPartCount);
