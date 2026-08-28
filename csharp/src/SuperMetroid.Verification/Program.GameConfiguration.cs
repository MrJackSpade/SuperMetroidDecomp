using SuperMetroid.Core.Frontend;

internal static partial class Program
{
static void VerifyGameConfigurationIni()
{
    SuperMetroidGameOptions defaults =
        SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents);
    AssertEqual(false, defaults.SkipOpeningCinematic,
        "game INI template preserves the opening cinematic");

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

    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpeningCinematic=yes\n",
        "must be either true or false");
    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpeningCinematic=true\nSkipOpeningCinematic=false\n",
        "duplicate");
    AssertInvalidGameConfiguration(
        "[Game]\nSkipOpenngCinematic=true\n",
        "unknown [Game] option");

    Console.WriteLine(
        "  Game INI: documented defaults, enabled skip, and typo rejection agree.");
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
