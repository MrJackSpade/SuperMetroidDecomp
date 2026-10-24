namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shared trigonometric and boost-state constants used by shinespark.</summary>
public static class ShinesparkDefinitions
{
    /// <summary><c>$90:D2BD</c>, EndSuperJump's minimum energy to sustain a native shinespark.</summary>
    public const ushort MinimumSustainingEnergy = 30;
    /// <summary>High-byte state identifying a fully active Speed Booster.</summary>
    public const ushort ActiveSpeedBoostCounter = 0x0400;
}
