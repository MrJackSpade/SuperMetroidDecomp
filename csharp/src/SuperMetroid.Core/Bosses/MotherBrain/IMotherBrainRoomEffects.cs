using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room-level effects Mother Brain's fight drives.</summary>
public interface IMotherBrainRoomEffects
{
    /// <summary>Advances the glass room's PLM argument.</summary>
    void IncrementGlassRoomArgument();

    /// <summary>Sets the default layer-blending configuration.</summary>
    void SetLayerBlendingDefault(LayerBlendingConfiguration configuration);

    /// <summary>Writes the BG2 scroll registers.</summary>
    void SetBg2Scroll(Position16<Bg2Pixels> scroll);
}
