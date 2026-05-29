using System.Text.RegularExpressions;

namespace CvTracker.Api.Services;

/// <summary>
/// Static utility that extracts a monthly salary range (PLN brutto) from free-form job offer text.
/// Handles: space/dot/comma thousand separators, "k" suffix, ASCII and Unicode dashes,
/// and applies ×1.23 multiplier when both "netto" and "B2B" appear (but not "brutto").
/// </summary>
public static class SalaryParser
{
    // A salary number: digits, optional groups of exactly-3-digit thousands (space/dot/comma sep),
    // and an optional "k" suffix meaning ×1000.
    private const string NumCapture = @"(\d+(?:[\s.,]\d{3})*k?)";

    // Range: <num> [-–—] <num> <currency>
    private static readonly Regex RangeCurrencyPattern = new(
        $@"{NumCapture}\s*[-\u2013\u2014]\s*{NumCapture}\s*(?:PLN|z[łl])\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Single: <num> <currency>
    private static readonly Regex SingleCurrencyPattern = new(
        $@"{NumCapture}\s*(?:PLN|z[łl])\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Range: <num>k [-–—] <num>k  (no currency keyword needed — k is the indicator)
    private static readonly Regex RangeKPattern = new(
        @"(\d+k)\s*[-\u2013\u2014]\s*(\d+k)(?![a-zA-Z])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Single: <num>k  (no currency, no range)
    private static readonly Regex SingleKPattern = new(
        @"(\d+k)(?!\s*[-\u2013\u2014]|\w)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NettoPattern =
        new(@"\bnetto\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex B2BPattern =
        new(@"\bB2B\b", RegexOptions.Compiled);

    private static readonly Regex BruttoPattern =
        new(@"\bbrutto\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Parses <paramref name="text"/> and returns the minimum and maximum monthly salary in PLN.
    /// Returns <c>(null, null)</c> when no recognisable salary pattern is found.
    /// </summary>
    public static (decimal? Min, decimal? Max) ParseSalary(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null);

        decimal? rawMin = null;
        decimal? rawMax = null;

        // Try patterns in order of specificity (most specific first).
        var rangeMatch = RangeCurrencyPattern.Match(text);
        if (rangeMatch.Success)
        {
            rawMin = ParseNumber(rangeMatch.Groups[1].Value);
            rawMax = ParseNumber(rangeMatch.Groups[2].Value);
        }
        else
        {
            var singleMatch = SingleCurrencyPattern.Match(text);
            if (singleMatch.Success)
            {
                rawMin = rawMax = ParseNumber(singleMatch.Groups[1].Value);
            }
            else
            {
                var rangeKMatch = RangeKPattern.Match(text);
                if (rangeKMatch.Success)
                {
                    rawMin = ParseNumber(rangeKMatch.Groups[1].Value);
                    rawMax = ParseNumber(rangeKMatch.Groups[2].Value);
                }
                else
                {
                    var singleKMatch = SingleKPattern.Match(text);
                    if (singleKMatch.Success)
                        rawMin = rawMax = ParseNumber(singleKMatch.Groups[1].Value);
                }
            }
        }

        if (rawMin is null || rawMax is null) return (null, null);

        // Ensure min ≤ max (swap if the range is reversed).
        if (rawMin > rawMax) (rawMin, rawMax) = (rawMax, rawMin);

        // Apply ×1.23 multiplier only when BOTH "netto" AND "B2B" are present
        // AND "brutto" is absent (brutto overrides netto).
        if (NettoPattern.IsMatch(text) && B2BPattern.IsMatch(text) && !BruttoPattern.IsMatch(text))
        {
            rawMin = Math.Round(rawMin.Value * 1.23m, 0);
            rawMax = Math.Round(rawMax.Value * 1.23m, 0);
        }

        return (rawMin, rawMax);
    }

    /// <summary>
    /// Converts a raw matched number string (e.g. "8 000", "10.000", "12k") to a decimal value.
    /// Thousand separators (space, dot, comma) are stripped; "k" suffix multiplies by 1 000.
    /// </summary>
    private static decimal? ParseNumber(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var s = raw.Trim();
        var hasK = s.EndsWith('k') || s.EndsWith('K');
        if (hasK) s = s[..^1];

        // Strip thousand separators (space, dot, comma).
        s = s.Replace(" ", "").Replace(".", "").Replace(",", "");

        if (!decimal.TryParse(s, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var val))
            return null;

        return hasK ? val * 1000m : val;
    }
}
