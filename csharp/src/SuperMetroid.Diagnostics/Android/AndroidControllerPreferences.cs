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
    /// <summary>Explicit physical-key overrides; keys absent here use device defaults.</summary>
    private readonly Dictionary<string, SnesButton> overrides;

    /// <summary>Creates an empty preference snapshot that uses every device-default binding.</summary>
    public AndroidControllerPreferences() : this([]) { }
    /// <summary>Creates a preference snapshot backed by the supplied validated bindings.</summary>
    /// <param name="values">Physical-key bindings to preserve.</param>
    private AndroidControllerPreferences(Dictionary<string, SnesButton> values) => overrides = values;

    /// <summary>Gets the configured button for a key, falling back to the device mapping.</summary>
    /// <param name="key">Physical key name.</param>
    /// <param name="fallback">Default button when no override exists.</param>
    /// <returns>The explicit binding or <paramref name="fallback"/>.</returns>
    public SnesButton Resolve(string key, SnesButton fallback) => overrides.GetValueOrDefault(key, fallback);

    /// <summary>Returns a new preference snapshot with one physical-key binding replaced.</summary>
    /// <param name="key">Physical key name.</param>
    /// <param name="button">One SNES button or the unbound value.</param>
    /// <returns>A copy containing the requested binding.</returns>
    public AndroidControllerPreferences WithBinding(string key, SnesButton button)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A physical key name is required.", nameof(key));
        Validate(button);
        var updated = new Dictionary<string, SnesButton>(overrides) { [key] = button };
        return new AndroidControllerPreferences(updated);
    }

    /// <summary>Serializes explicit bindings as an indented JSON object.</summary>
    /// <returns>JSON text suitable for Android preference storage.</returns>
    public string Serialize() => JsonSerializer.Serialize(
        overrides.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
        new JsonSerializerOptions { WriteIndented = true });

    /// <summary>Loads and validates the explicit bindings in a stored JSON object.</summary>
    /// <param name="json">Serialized controller preferences.</param>
    /// <returns>Validated preferences; unknown or composite button values are rejected.</returns>
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

    /// <summary>Rejects undefined and combined button values; a binding selects one SNES button.</summary>
    /// <param name="button">Button value to validate.</param>
    private static void Validate(SnesButton button)
    {
        int bits = (ushort)button;
        if (!Enum.IsDefined(button) || (bits != 0 && (bits & (bits - 1)) != 0))
            throw new InvalidDataException($"Controller binding must name one SNES button, not {button}.");
    }
}
