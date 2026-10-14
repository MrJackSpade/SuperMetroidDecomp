namespace SuperMetroid.Core.Game;

/// <summary>The walk counter after one shot reaction and whether it started recoil.</summary>
internal readonly record struct MotherBrainShotReactionResult(ushort WalkCounter, bool HyperBeamRecoil);
