
namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>The byte method, progression and width of a word read.</summary>
/// <param name="ByteMethod">The byte method.</param>
/// <param name="Progression">The progression.</param>
/// <param name="Long">The long.</param>
internal readonly record struct WordReadShape(string ByteMethod, WordReadProgression Progression, bool Long);
