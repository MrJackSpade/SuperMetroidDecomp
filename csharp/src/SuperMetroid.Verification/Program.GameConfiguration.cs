using SuperMetroid.Core.Frontend;

internal static partial class Program
{
static void VerifyGameConfigurationIni()
{
    SuperMetroidGameOptions defaults =
        SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents);
    AssertEqual(false, defaults.SkipOpeningCinematic,
        "game INI template preserves the opening cinematic");
    AssertEqual(true, defaults.AudioEnabled, "game INI template enables cartridge audio");
    AssertEqual(100, defaults.MasterVolumePercent, "game INI template uses full host gain");

    SuperMetroidGameOptions enabled = SuperMetroidGameOptionsIni.Parse(
        "# local developer convenience\n[game]\nSKIPOPENINGCINEMATIC=TrUe\n",
        "in-memory enabled fixture");
    AssertEqual(true, enabled.SkipOpeningCinematic,
        "game INI names and boolean values are case-insensitive");

    SuperMetroidGameOptions missing = SuperMetroidGameOptionsIni.Parse(
        "; An old configuration may not contain newly introduced keys.\n[Game]\n",
        "in-memory missing-key fixture");
    AssertEqual(false, missing.SkipOpeningCinematic,
        "missing game INI key retains retail behavior");
    AssertEqual(true, missing.AudioEnabled, "missing audio section enables sound by default");
    AssertEqual(100, missing.MasterVolumePercent, "missing volume retains full gain");

    SuperMetroidGameOptions audio = SuperMetroidGameOptionsIni.Parse(
        "[Audio]\nEnabled=false\nMasterVolumePercent=37\n",
        "in-memory audio fixture");
    AssertEqual(false, audio.AudioEnabled, "audio INI disable");
    AssertEqual(37, audio.MasterVolumePercent, "audio INI gain");

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
        "[Audio]\nMasterVolumePercent=101\n",
        "0 through 100");
    AssertInvalidGameConfiguration(
        "[Audio]\nEnabeld=true\n",
        "unknown [Audio] option");

    Console.WriteLine(
        "  Game INI: startup/audio defaults, strict gain, and typo rejection agree.");
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
