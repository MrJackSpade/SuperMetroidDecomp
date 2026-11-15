using SuperMetroid.Core.Enemies.EffectActors;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// A frozen Metroid's two outer-body effect actors: their frozen animations. They keep drawing from the Metroid's own
/// sheet, in the frozen colours every spawned-object sheet holds (#1881).
/// </summary>
internal static class MetroidOuterBodyDefinitions
{
    /// <summary>The frozen animation of the Metroid's electricity.</summary>
    internal const RoomEffectActorSegment ElectricityFrozenAnimation = RoomEffectActorSegment.MetroidElectricity;

    /// <summary>The frozen animation of the Metroid's shell.</summary>
    internal const RoomEffectActorSegment ShellFrozenAnimation = RoomEffectActorSegment.MetroidShell;
}
