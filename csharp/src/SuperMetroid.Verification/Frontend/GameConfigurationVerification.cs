using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
using SuperMetroid.Diagnostics.Configuration;
namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks shipped game option defaults, accepted INI values, and configuration error messages.</summary>
internal static class GameConfigurationVerification
{
/// <summary>Compares the INI template with programmatic defaults and checks parsing across game, video, audio, and diagnostics sections.</summary>
internal static void VerifyGameConfigurationIni()
{
    // Installed INIs name a renderer; Direct3D 11 is the only one, so the names that select it are still read.
    foreach (string selection in new[] { "direct3d11", "Direct3D11", "auto", "Auto" })
        AssertEqual(new HostOptions(), HostOptionsIni.Parse($"[Video]\nRenderer={selection}"),
            "renderer names that select Direct3D 11 parse case-insensitively and change no option");
    foreach (string invalid in new[] { "Software", "software", "0", "1", "Vulkan", "Software,Auto", "" })
        AssertThrows<InvalidDataException>(() => HostOptionsIni.Parse($"[Video]\nRenderer={invalid}"),
            "the removed software renderer and unknown or numeric renderers are rejected");
    AssertThrows<InvalidDataException>(() => HostOptionsIni.Parse("[Video]\nRenderer=Auto\nrenderer=Direct3D11"),
        "duplicate renderer rejected");
    AssertThrows<InvalidDataException>(() => HostOptionsIni.Parse("[Video]\nReportErrorsToGitHub=true"),
        "another section's option under [Video] rejected");
    HostOptions defaults =
        HostOptionsIni.Parse(HostOptionsIni.DefaultFileContents);
    AssertEqual(new HostOptions(), defaults,
        "shipped INI and programmatic defaults match for every option");
    AssertEqual(defaults, HostOptionsIni.Parse(""),
        "missing-key parser defaults match the shipped INI");
    string[] documentedKeys = HostOptionsIni.DefaultFileContents.Split('\n')
        .Select(line => line.Trim()).Where(line => !line.StartsWith(';') && line.Contains('='))
        .Select(line => line[..line.IndexOf('=')].Trim())
        .Select(key => key == "Enabled" ? nameof(HostOptions.AudioEnabled) : key)
        .OrderBy(key => key, StringComparer.Ordinal).ToArray();
    // The file documents the game's options beside the host's own settings; the host record carries the game's as one member.
    string[] optionProperties = MetadataShape.Properties(typeof(SuperMetroidGameOptions), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
        .Concat(MetadataShape.Properties(typeof(HostOptions), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
        .Select(property => property.Name).Where(name => name != nameof(HostOptions.Game))
        .OrderBy(key => key, StringComparer.Ordinal).ToArray();
    AssertEqual(string.Join(',', optionProperties), string.Join(',', documentedKeys),
        "shipped INI explicitly documents every supported option exactly once");
    AssertEqual(false, defaults.Game.SkipOpeningCinematic,
        "game INI template preserves the opening cinematic");
    AssertEqual(false, defaults.Game.Invincibility,
        "game INI template preserves cartridge damage");
    AssertEqual(false, defaults.Game.InfiniteAmmo,
        "game INI template preserves cartridge ammunition");
    AssertEqual(MapRevealMode.None, defaults.Game.MapReveal,
        "game INI template preserves ordinary map visibility");
    AssertEqual(true, defaults.AudioEnabled, "game INI template enables cartridge audio");
    AssertEqual(100, defaults.MasterVolumePercent, "game INI template uses full host gain");
    AssertEqual(false, defaults.ReportErrorsToGitHub,
        "game INI template disables automatic external reports");
    AssertEqual("MrJackSpade/SuperMetroidDecomp", defaults.GitHubErrorRepository,
        "game INI template names the private development repository");

    HostOptions enabled = HostOptionsIni.Parse(
        "# local developer convenience\n[game]\nSKIPOPENINGCINEMATIC=TrUe\n" +
        "INVINCIBILITY=true\nINFINITEAMMO=true\nMAPREVEAL=sEcReT\n",
        "in-memory enabled fixture");
    AssertEqual(true, enabled.Game.SkipOpeningCinematic,
        "game INI names and boolean values are case-insensitive");
    AssertEqual(true, enabled.Game.Invincibility,
        "game INI enables host invincibility");
    AssertEqual(true, enabled.Game.InfiniteAmmo,
        "game INI enables infinite unlocked ammunition");
    AssertEqual(MapRevealMode.Secret, enabled.Game.MapReveal,
        "game INI parses named map reveal values case-insensitively");

    HostOptions missing = HostOptionsIni.Parse(
        "; An old configuration may not contain newly introduced keys.\n[Game]\n",
        "in-memory missing-key fixture");
    AssertEqual(false, missing.Game.SkipOpeningCinematic,
        "missing game INI key retains retail behavior");
    AssertEqual(false, missing.Game.Invincibility,
        "missing invincibility key retains cartridge damage");
    AssertEqual(false, missing.Game.InfiniteAmmo,
        "missing infinite-ammo key retains cartridge ammunition");
    AssertEqual(MapRevealMode.None, missing.Game.MapReveal,
        "missing map-reveal key retains cartridge visibility");
    AssertEqual(true, missing.AudioEnabled, "missing audio section enables sound by default");
    AssertEqual(100, missing.MasterVolumePercent, "missing volume retains full gain");
    AssertEqual(false, missing.ReportErrorsToGitHub,
        "missing diagnostics section does not publish errors");

    HostOptions audio = HostOptionsIni.Parse(
        "[Audio]\nEnabled=false\nMasterVolumePercent=37\n",
        "in-memory audio fixture");
    AssertEqual(false, audio.AudioEnabled, "audio INI disable");
    AssertEqual(37, audio.MasterVolumePercent, "audio INI gain");

    HostOptions diagnostics = HostOptionsIni.Parse(
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
        "[Game]\nMapReveal=Everything\n",
        "None, Public, or Secret");
    AssertInvalidGameConfiguration(
        "[Game]\nMapReveal=Public\nMapReveal=None\n",
        "duplicate");
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
        "  Game INI: startup/map/audio/diagnostic defaults, strict values, and typo rejection agree.");
}

/// <summary>Requires invalid INI input to fail with a message that identifies the violated option rule.</summary>
internal static void AssertInvalidGameConfiguration(string contents, string expectedMessagePart)
{
    try
    {
        _ = HostOptionsIni.Parse(contents, "invalid in-memory fixture");
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
