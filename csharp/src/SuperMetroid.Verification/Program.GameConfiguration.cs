using SuperMetroid.Core.Frontend;

internal static partial class Program
{
static void VerifyGameConfigurationIni()
{
    SuperMetroidGameOptions defaults =
        SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents);
    AssertEqual(false, defaults.SkipOpeningCinematic,
        "game INI template preserves the opening cinematic");
    AssertEqual(false, defaults.Invincibility,
        "game INI template preserves cartridge damage");
    AssertEqual(false, defaults.InfiniteAmmo,
        "game INI template preserves cartridge ammunition");
    AssertEqual(true, defaults.AudioEnabled, "game INI template enables cartridge audio");
    AssertEqual(100, defaults.MasterVolumePercent, "game INI template uses full host gain");
    AssertEqual(false, defaults.ReportErrorsToGitHub,
        "game INI template disables automatic external reports");
    AssertEqual("MrJackSpade/SuperMetroidDecomp", defaults.GitHubErrorRepository,
        "game INI template names the private development repository");

    SuperMetroidGameOptions enabled = SuperMetroidGameOptionsIni.Parse(
        "# local developer convenience\n[game]\nSKIPOPENINGCINEMATIC=TrUe\n" +
        "INVINCIBILITY=true\nINFINITEAMMO=true\n",
        "in-memory enabled fixture");
    AssertEqual(true, enabled.SkipOpeningCinematic,
        "game INI names and boolean values are case-insensitive");
    AssertEqual(true, enabled.Invincibility,
        "game INI enables host invincibility");
    AssertEqual(true, enabled.InfiniteAmmo,
        "game INI enables infinite unlocked ammunition");

    SuperMetroidGameOptions missing = SuperMetroidGameOptionsIni.Parse(
        "; An old configuration may not contain newly introduced keys.\n[Game]\n",
        "in-memory missing-key fixture");
    AssertEqual(false, missing.SkipOpeningCinematic,
        "missing game INI key retains retail behavior");
    AssertEqual(false, missing.Invincibility,
        "missing invincibility key retains cartridge damage");
    AssertEqual(false, missing.InfiniteAmmo,
        "missing infinite-ammo key retains cartridge ammunition");
    AssertEqual(true, missing.AudioEnabled, "missing audio section enables sound by default");
    AssertEqual(100, missing.MasterVolumePercent, "missing volume retains full gain");
    AssertEqual(false, missing.ReportErrorsToGitHub,
        "missing diagnostics section does not publish errors");

    SuperMetroidGameOptions audio = SuperMetroidGameOptionsIni.Parse(
        "[Audio]\nEnabled=false\nMasterVolumePercent=37\n",
        "in-memory audio fixture");
    AssertEqual(false, audio.AudioEnabled, "audio INI disable");
    AssertEqual(37, audio.MasterVolumePercent, "audio INI gain");

    SuperMetroidGameOptions diagnostics = SuperMetroidGameOptionsIni.Parse(
        "[Diagnostics]\nReportErrorsToGitHub=true\nGitHubErrorRepository=owner/repo.name\n",
        "in-memory diagnostics fixture");
    AssertEqual(true, diagnostics.ReportErrorsToGitHub,
        "diagnostics INI enables GitHub error reporting");
    AssertEqual("owner/repo.name", diagnostics.GitHubErrorRepository,
        "diagnostics INI selects its repository");

    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpeningCinematic=yes\n",
        "must be either true or false");
    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpeningCinematic=true\nSkipOpeningCinematic=false\n",
        "duplicate");
    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpenngCinematic=true\n",
        "unknown [Game] option");
    AssertInvalidGameConfiguration(
        "[Game]\nInvincibility=enabled\n",
        "must be either true or false");
    AssertInvalidGameConfiguration(
        "[Game]\nInfiniteAmmo=yes\n",
        "must be either true or false");
    AssertInvalidGameConfiguration(
        "[Audio]\nMasterVolumePercent=101\n",
        "0 through 100");
    AssertInvalidGameConfiguration(
        "[Audio]\nEnabeld=true\n",
        "unknown [Audio] option");
    AssertInvalidGameConfiguration(
        "[Diagnostics]\nReportErrorsToGitHub=yes\n",
        "must be either true or false");
    AssertInvalidGameConfiguration(
        "[Diagnostics]\nGitHubErrorRepository=not-a-repository\n",
        "owner/repository form");
    AssertInvalidGameConfiguration(
        "[Diagnostics]\nReportErorsToGitHub=true\n",
        "unknown [Diagnostics] option");

    Console.WriteLine(
        "  Game INI: startup/audio/diagnostic defaults, strict values, and typo rejection agree.");
}

private static void AssertInvalidGameConfiguration(string contents, string expectedMessagePart)
{
    try
    {
        _ = SuperMetroidGameOptionsIni.Parse(contents, "invalid in-memory fixture");
    }
    catch (InvalidDataException exception)
    {
        AssertTrue(
            exception.Message.Contains(expectedMessagePart, StringComparison.OrdinalIgnoreCase),
            $"invalid game INI explains '{expectedMessagePart}'");
        return;
    }

    throw new InvalidOperationException(
        $"Invalid game INI containing '{expectedMessagePart}' was silently accepted.");
}
}
