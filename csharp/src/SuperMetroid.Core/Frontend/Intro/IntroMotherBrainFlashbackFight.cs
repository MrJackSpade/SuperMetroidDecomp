namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Where the intro's Mother Brain flashback's fight stands (#2554): before her fourth hit, or exploding from it, with the
/// explosions that hit spawned.
/// </summary>
internal abstract record IntroMotherBrainFlashbackFight
{
    /// <summary>Only the stages below exist.</summary>
    private IntroMotherBrainFlashbackFight()
    {
    }

    /// <summary>Mother Brain has not taken her fourth hit: nothing explodes yet.</summary>
    internal sealed record BeforeFourthHit : IntroMotherBrainFlashbackFight;

    /// <summary>Mother Brain's fourth hit has spawned <paramref name="Explosions"/>.</summary>
    /// <param name="Explosions">The explosions her fourth hit spawned.</param>
    internal sealed record Exploding(IntroMotherBrainExplosionSystem Explosions) : IntroMotherBrainFlashbackFight;
}
