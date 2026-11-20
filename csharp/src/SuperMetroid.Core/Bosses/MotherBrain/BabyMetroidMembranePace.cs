namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The pace of the Baby Metroid's membrane pulse (#2404): its values are arbitrary.</summary>
public enum BabyMetroidMembranePace
{
    /// <summary>The ordinary pace, each phase eleven updates.</summary>
    Calm,
    /// <summary>The quick pace while it is latched on and feeding, each phase two updates; its cry is the quick one.</summary>
    Feeding,
}
