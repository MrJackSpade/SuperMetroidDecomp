namespace SuperMetroid.Core.Rooms;

/// <summary>Whether a power bomb explosion is active, which silences room and PLM sounds (#1395). Samus's explosion implements it.</summary>
public interface IActivePowerBomb
{
    /// <summary>Whether the explosion is active.</summary>
    bool IsActive { get; }
}
