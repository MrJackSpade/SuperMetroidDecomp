namespace SuperMetroid.Core.Game;

/// <summary>The translated lifetime of the morph-ball eye's bank-$88 HDMA object.</summary>
public enum MorphBallEyeBeamPhase
{
    /// <summary>No reveal HDMA object is installed; beam accumulators and fixed-color components are at their reset values.</summary>
    Inactive,
    /// <summary>The eye AI requested a beam, but its initialization instruction will run in the next HDMA-object pass, before the next enemy AI pass.</summary>
    PendingInitialization,
    /// <summary>The initialized object accelerates its 16.16 angular half-width until it reaches four of the 256 angle units per full turn.</summary>
    Widening,
    /// <summary>The four-unit half-width beam tracks the body angle and cycles sixteen yellow brightness steps while the body's activation flag remains set.</summary>
    Full,
    /// <summary>The object's fixed-color RGB components fade toward zero; reaching the green selector-only value deletes the beam on the following pass.</summary>
    Deactivating,
}
