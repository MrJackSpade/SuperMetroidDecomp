namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres destruction scene's views show (<see cref="CeresDestructionView"/>).</summary>
internal static partial class CeresDestructionViewDefinitions
{
    /// <summary>The views that show the Zebes reveal's pictures and planet in place of the station's.</summary>
    internal static readonly CeresDestructionView[] ShowingZebes =
        [CeresDestructionView.ZebesReveal, CeresDestructionView.ZebesApproach, CeresDestructionView.ZebesClose];
}
