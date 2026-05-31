using System.Text;
using System.Text.RegularExpressions;

namespace CvTracker.Api.Services.Parsing;

/// <summary>
/// Extracts text sections (<c>OurRequirements</c>, <c>WhatWeOffer</c>) and <c>Location</c>
/// from raw offer text.
/// Internal — no interface needed; called only from <see cref="OfferTextParserService"/>.
/// </summary>
internal static class SectionParser
{
    // Compiled regex built once from the known-city list in OfferParserKeywords.
    private static readonly Regex KnownCitiesPattern = new(
        @"\b(" + string.Join("|", OfferParserKeywords.KnownCities.Select(Regex.Escape)) + @")\b",
        RegexOptions.Compiled);

    // Detects a "generic heading" line that signals the end of a parsed section.
    private static readonly Regex GenericHeadingPattern = new(
        @"^\s*[^\n]{2,60}:\s*$|^\s*[A-ZŁÓŹĄĆĘŚŃ][A-ZŁÓŹĄĆĘŚŃ\s]{2,40}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Extracts the text block following any of the supplied <paramref name="headings"/>
    /// (case-insensitive) up to the next generic heading or end of text.
    /// Returns <c>null</c> when the heading is not found or the block is empty.
    /// </summary>
    internal static string? ExtractSection(string text, string[] headings)
    {
        var lines = text.Split('\n');
        bool inSection = false;
        var sb = new StringBuilder();

        var headingPattern = new Regex(
            $@"(?i)^\s*({string.Join("|", headings.Select(Regex.Escape))})\s*:?\s*$",
            RegexOptions.Compiled);

        foreach (var line in lines)
        {
            if (headingPattern.IsMatch(line))
            {
                inSection = true;
                continue;
            }

            if (inSection)
            {
                // Stop when the next generic heading is reached (and content was already collected).
                if (sb.Length > 0 && GenericHeadingPattern.IsMatch(line))
                    break;

                sb.AppendLine(line);
            }
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <summary>
    /// Extracts location from a label ("Lokalizacja:", "Location:", "Miejsce pracy:") or,
    /// failing that, from the first known Polish city name found in the text.
    /// Returns <c>null</c> when neither source matches.
    /// </summary>
    internal static string? ExtractLocation(string text)
    {
        // Label-based.
        var labelMatch = Regex.Match(text,
            @"(?i)(?:lokalizacja|location|miejsce\s+pracy)\s*[:\u2014]\s*([^\n]{1,100})");
        if (labelMatch.Success)
        {
            var v = labelMatch.Groups[1].Value.Split('\n')[0].Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // Known Polish city.
        var cityMatch = KnownCitiesPattern.Match(text);
        return cityMatch.Success ? cityMatch.Value : null;
    }
}
