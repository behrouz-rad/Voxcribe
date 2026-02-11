// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Desktop.Models;

/// <summary>
/// Represents a language option for Whisper transcription.
/// </summary>
public sealed record LanguageOption(string Code, string DisplayName)
{
    /// <summary>
    /// Gets all supported Whisper languages with "Auto" as the first option, sorted alphabetically.
    /// </summary>
    public static IReadOnlyList<LanguageOption> GetAllLanguages() =>
    [
        new(string.Empty, "Auto (Detect Language)"),
        .. Languages.OrderBy(lang => lang.DisplayName, StringComparer.CurrentCultureIgnoreCase)
    ];

    private static readonly LanguageOption[] Languages =
    [
        new("af", "Afrikaans"),
        new("sq", "Albanian"),
        new("am", "Amharic"),
        new("ar", "Arabic"),
        new("hy", "Armenian"),
        new("as", "Assamese"),
        new("az", "Azerbaijani"),
        new("ba", "Bashkir"),
        new("eu", "Basque"),
        new("be", "Belarusian"),
        new("bn", "Bengali"),
        new("bs", "Bosnian"),
        new("br", "Breton"),
        new("bg", "Bulgarian"),
        new("ca", "Catalan"),
        new("zh", "Chinese"),
        new("hr", "Croatian"),
        new("cs", "Czech"),
        new("da", "Danish"),
        new("nl", "Dutch"),
        new("en", "English"),
        new("et", "Estonian"),
        new("fo", "Faroese"),
        new("fi", "Finnish"),
        new("fr", "French"),
        new("gl", "Galician"),
        new("ka", "Georgian"),
        new("de", "German"),
        new("el", "Greek"),
        new("gu", "Gujarati"),
        new("ht", "Haitian Creole"),
        new("ha", "Hausa"),
        new("haw", "Hawaiian"),
        new("he", "Hebrew"),
        new("hi", "Hindi"),
        new("hu", "Hungarian"),
        new("is", "Icelandic"),
        new("id", "Indonesian"),
        new("it", "Italian"),
        new("ja", "Japanese"),
        new("jw", "Javanese"),
        new("kn", "Kannada"),
        new("kk", "Kazakh"),
        new("km", "Khmer"),
        new("ko", "Korean"),
        new("la", "Latin"),
        new("lv", "Latvian"),
        new("ln", "Lingala"),
        new("lt", "Lithuanian"),
        new("lb", "Luxembourgish"),
        new("mk", "Macedonian"),
        new("mg", "Malagasy"),
        new("ms", "Malay"),
        new("ml", "Malayalam"),
        new("mt", "Maltese"),
        new("mi", "Maori"),
        new("mr", "Marathi"),
        new("mn", "Mongolian"),
        new("my", "Myanmar"),
        new("ne", "Nepali"),
        new("no", "Norwegian"),
        new("nn", "Nynorsk"),
        new("oc", "Occitan"),
        new("ps", "Pashto"),
        new("fa", "Persian"),
        new("pl", "Polish"),
        new("pt", "Portuguese"),
        new("pa", "Punjabi"),
        new("ro", "Romanian"),
        new("ru", "Russian"),
        new("sa", "Sanskrit"),
        new("sr", "Serbian"),
        new("sn", "Shona"),
        new("sd", "Sindhi"),
        new("si", "Sinhala"),
        new("sk", "Slovak"),
        new("sl", "Slovenian"),
        new("so", "Somali"),
        new("es", "Spanish"),
        new("su", "Sundanese"),
        new("sw", "Swahili"),
        new("sv", "Swedish"),
        new("tl", "Tagalog"),
        new("tg", "Tajik"),
        new("ta", "Tamil"),
        new("tt", "Tatar"),
        new("te", "Telugu"),
        new("th", "Thai"),
        new("bo", "Tibetan"),
        new("tr", "Turkish"),
        new("tk", "Turkmen"),
        new("uk", "Ukrainian"),
        new("ur", "Urdu"),
        new("uz", "Uzbek"),
        new("vi", "Vietnamese"),
        new("cy", "Welsh"),
        new("yi", "Yiddish"),
        new("yo", "Yoruba"),
    ];

    /// <summary>
    /// Returns true if this represents automatic language detection.
    /// </summary>
    public bool IsAuto => string.IsNullOrEmpty(Code);

    /// <summary>
    /// Gets the language code to pass to Whisper, or null for auto-detection.
    /// </summary>
    public string? WhisperLanguageCode => IsAuto ? null : Code;

    public override string ToString() => DisplayName;
}
