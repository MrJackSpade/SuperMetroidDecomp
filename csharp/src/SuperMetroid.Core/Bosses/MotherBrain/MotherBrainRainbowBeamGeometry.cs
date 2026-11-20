namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The cone of Mother Brain's rainbow beam: its apex at her mouth and its two edges, each an angle in 256ths of a turn
/// measured as the beam aims, from straight down turning toward her right. The display draws whatever cone they describe.
/// </summary>
/// <param name="Mouth">The cone's apex: the mouth's room position, right of the head and below.</param>
/// <param name="LowerEdge">The edge half the beam's width short of its aim, wrapped around the turn.</param>
/// <param name="UpperEdge">The edge half the beam's width past its aim, wrapped around the turn.</param>
public readonly record struct MotherBrainRainbowBeamGeometry(
    Position16<RoomPixels> Mouth,
    TrigonometryAngle LowerEdge,
    TrigonometryAngle UpperEdge);
