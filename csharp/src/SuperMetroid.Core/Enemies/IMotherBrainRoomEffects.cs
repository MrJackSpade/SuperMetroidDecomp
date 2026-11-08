namespace SuperMetroid.Core.Enemies;

/// <summary>The room-level effects Mother Brain's fight drives.</summary>
public interface IMotherBrainRoomEffects
{
    /// <summary>Advances the glass room's PLM argument.</summary>
    void IncrementGlassRoomArgument();
}
