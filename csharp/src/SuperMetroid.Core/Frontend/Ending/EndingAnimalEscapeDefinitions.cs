namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the rescued animals' escape pod.</summary>
internal static class EndingAnimalEscapeDefinitions
{
    /// <summary>The pod tests whether the animals were saved, then takes its fixed actor slot four.</summary>
    internal const int ForcedSlot = 2;
    /// <summary>Both pod coordinates start at 128.</summary>
    internal const int Origin = 128;
    /// <summary>The actor's general-purpose timer start (not a spawn delay).</summary>
    internal const int GeneralTimer = 260;
    /// <summary>The pod adds 128 to the 16-bit Y fraction each actor call.</summary>
    internal const int YFractionVelocity = 128;
    /// <summary>The pod advances one horizontal pixel per actor call.</summary>
    internal const int XVelocity = 1;
    /// <summary>The pod deletes itself when its center reaches 272.</summary>
    internal const int DeleteX = 272;
}
