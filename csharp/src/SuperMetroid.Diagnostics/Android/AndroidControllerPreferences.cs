using System.Text.Json;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>
/// Persisted host-key overrides, not cartridge control bindings. Unspecified keys keep
/// the device defaults. Only individual SNES buttons (or unbound) are accepted.
/// This data owner has no Android dependencies so persistence is tested on Windows too.
/// </summary>
internal sealed class AndroidControllerPreferences
{
    /// <summary>Physical host-key names mapped to individual SNES buttons.</summary>
    private readonly Dictionary<string, SnesButton> overrides;

    /// <summary>Starts with device defaults and no host-key overrides.</summary>
    public AndroidControllerPreferences() : this([]) { }
    /// <summary>Retains an already validated override set.</summary>
    private AndroidControllerPreferences(Dictionary<string, SnesButton> values) => overrides = values;

    /// <summary>Uses the device default when no override exists for the physical key.</summary>
    public SnesButton Resolve(string key, SnesButton fallback) => overrides.GetValueOrDefault(key, fallback);

    /// <summary>Creates a new preference set with one valid single-button or unbound override.</summary>
    public AndroidControllerPreferences WithBinding(string key, SnesButton button)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A physical key name is required.", nameof(key));
        Validate(button);
        var updated = new Dictionary<string, SnesButton>(overrides) { [key] = button };
        return new AndroidControllerPreferences(updated);
    }

    /// <summary>Writes physical-key overrides as an indented JSON object of button names.</summary>
    public string Serialize() => JsonSerializer.Serialize(
        overrides.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
        new JsonSerializerOptions { WriteIndented = true });

    /// <summary>Parses exact button names and rejects composite or unknown bindings.</summary>
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

    /// <summary>Accepts only an individual defined SNES button or the unbound value.</summary>
    private static void Validate(SnesButton button)
    {
        int bits = (ushort)button;
        if (!Enum.IsDefined(button) || (bits != 0 && (bits & (bits - 1)) != 0))
            throw new InvalidDataException($"Controller binding must name one SNES button, not {button}.");
    }
}
