namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Whether starting a crystal flash checks the held input chord.</summary>
public enum CrystalFlashInputCheck
{
    /// <summary>The chord must be held.</summary>
    Required,

    /// <summary>The chord is not checked.</summary>
    Skipped,
}
