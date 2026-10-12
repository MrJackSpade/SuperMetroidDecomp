using System.Text.Json;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Android;

/// <summary>
/// Persisted host-key overrides, not cartridge control bindings. Unspecified keys keep
/// the device defaults. Only individual SNES buttons (or unbound) are accepted.
/// This data owner has no Android dependencies so persistence is tested on Windows too.
/// </summary>
internal sealed class AndroidControllerPreferences
{
    private readonly Dictionary<string, SnesButton> overrides;

    public AndroidControllerPreferences() : this([]) { }
    private AndroidControllerPreferences(Dictionary<string, SnesButton> values) => overrides = values;

    public SnesButton Resolve(string key, SnesButton fallback) => overrides.GetValueOrDefault(key, fallback);

    public AndroidControllerPreferences WithBinding(string key, SnesButton button)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A physical key name is required.", nameof(key));
        Validate(button);
        var updated = new Dictionary<string, SnesButton>(overrides) { [key] = button };
        return new AndroidControllerPreferences(updated);
    }

    public string Serialize() => JsonSerializer.Serialize(
        overrides.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
        new JsonSerializerOptions { WriteIndented = true });

    public static AndroidControllerPreferences Parse(string json)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new InvalidDataException("Controller bindings must be a JSON object.");
        var result = new AndroidControllerPreferences();
        foreach (var pair in values)
        {
            if (!Enum.TryParse(pair.Value, out SnesButton button) || pair.Value != button.ToString())
                throw new InvalidDataException($"Unknown controller binding '{pair.Value}' for {pair.Key}.");
            result = result.WithBinding(pair.Key, button);
        }
        return result;
    }

    private static void Validate(SnesButton button)
    {
        int bits = (ushort)button;
        if (!Enum.IsDefined(button) || (bits != 0 && (bits & (bits - 1)) != 0))
            throw new InvalidDataException($"Controller binding must name one SNES button, not {button}.");
    }
}
