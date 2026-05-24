using System.Text.RegularExpressions;
using CvTracker.Api.Models;

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Static regex engine that converts raw salary strings (Polish and English)
/// into structured <c>(min, max, contractType)</c> tuples.
/// </summary>
/// <remarks>
/// Handled formats:
/// <list type="bullet">
///   <item><c>"od 10 000 do 15 000 PLN brutto (UoP)"</c> → (10000, 15000, UoP)</item>
///   <item><c>"10 000–15 000 PLN gross"</c> → (10000, 15000, null)</item>
///   <item><c>"10 000 zł netto B2B"</c> → (10000, 10000, B2B)</item>
///   <item>null / empty → (null, null, null)</item>
/// </list>
/// Polish thousands separators: regular space (U+0020) and non-breaking space (U+00A0).
/// </remarks>
public static class SalaryParser
{
    // Thousands separator: space or non-breaking space.
    private const string Sep = @"[\s\u00A0]";

    // One number: either thousands-separated (e.g. "10 000", "1 000") or a plain number
    // of at least 3 digits (e.g. "100", "10000"). Single/two-digit bare numbers (e.g. the
    // "2" inside "B2B") are intentionally excluded to avoid false-positive salary matches.
    private static readonly string NumberPattern = $@"(?:\d{{1,3}}(?:{Sep}\d{{3}})+|\d{{3,}})";

    // Full pattern: optional "od <min> do <max>" or "<min>–<max>" or just "<min>".
    private static readonly Regex RangeRegex = new(
        $@"(?:od{Sep}+)?(?<min>{NumberPattern}){Sep}*[–\-]{Sep}*(?<max>{NumberPattern})" +
        $@"|od{Sep}+(?<min2>{NumberPattern}){Sep}+do{Sep}+(?<max2>{NumberPattern})" +
        $@"|(?<single>{NumberPattern})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex B2BRegex   = new(@"\bB2B\b",             RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UoPRegex    = new(@"\bUo[Pp]\b|\bbrutto\b|\bgross\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex NettoRegex  = new(@"\bnetto\b|\bnet\b",  RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ZlecRegex   = new(@"\bzlecen|\bmandate", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Parses a raw salary string into a structured tuple.
    /// </summary>
    /// <param name="input">Raw salary text, e.g. from a job portal.</param>
    /// <returns>
    /// A tuple of (min, max, contractType). Any element may be <see langword="null"/>
    /// if it cannot be determined from the input.
    /// </returns>
    public static (decimal? Min, decimal? Max, ContractType? ContractType) Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (null, null, null);
        }

        var match = RangeRegex.Match(input);
        if (!match.Success)
        {
            return (null, null, DetectContractType(input));
        }

        decimal? min;
        decimal? max;

        // Prefer explicit "od … do …" groups, then dash-range, then single value.
        if (match.Groups["min2"].Success)
        {
            min = ParseNumber(match.Groups["min2"].Value);
            max = ParseNumber(match.Groups["max2"].Value);
        }
        else if (match.Groups["min"].Success)
        {
            min = ParseNumber(match.Groups["min"].Value);
            max = match.Groups["max"].Success ? ParseNumber(match.Groups["max"].Value) : min;
        }
        else
        {
            var single = ParseNumber(match.Groups["single"].Value);
            min = single;
            max = single;
        }

        return (min, max, DetectContractType(input));
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    /// <summary>Strips thousands separators and converts to decimal.</summary>
    private static decimal? ParseNumber(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Remove all spaces and non-breaking spaces used as thousands separators.
        var cleaned = Regex.Replace(raw, $@"[\s\u00A0]", string.Empty);
        return decimal.TryParse(cleaned, out var value) ? value : null;
    }

    /// <summary>
    /// Detects the contract type keyword present in the salary string.
    /// Returns <see langword="null"/> when no keyword is found.
    /// </summary>
    private static ContractType? DetectContractType(string input)
    {
        if (B2BRegex.IsMatch(input))   return ContractType.B2B;
        if (ZlecRegex.IsMatch(input))  return ContractType.MandateContract;
        if (UoPRegex.IsMatch(input))   return ContractType.UoP;
        return null;
    }
}
