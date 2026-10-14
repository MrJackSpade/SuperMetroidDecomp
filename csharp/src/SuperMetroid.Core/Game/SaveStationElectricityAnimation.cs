using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The save station electricity's typed animation.</summary>
internal sealed class SaveStationElectricityAnimation
    : EnemyProjectileTimelineAnimation<SaveStationElectricityFrame, SaveStationElectricityCue>
{
    /// <inheritdoc/>
    private protected override AnimationTimeline<SaveStationElectricityFrame, SaveStationElectricityCue> Timeline =>
        SaveStationElectricityAnimations.Timeline;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(SaveStationElectricityFrame frame) =>
        SaveStationElectricityVisualDefinitions.PresentationKey(frame);
}