namespace Engine.Actors;

/// <summary>What an update settled for a player with regard to their energy running out.</summary>
public enum EmptyEnergy
{
    /// <summary>They have energy, or are dead or dying already: there was nothing to settle.</summary>
    NotGone,

    /// <summary>Their energy is gone and what they are has saved them.</summary>
    Saved,

    /// <summary>Their energy is gone and they have begun to die.</summary>
    Dying,

    /// <summary>Their energy is gone and nothing was allowed to follow from it on this update: they are asked again on a later one.</summary>
    Withheld,
}
