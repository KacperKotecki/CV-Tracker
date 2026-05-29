using System.Text.RegularExpressions;

namespace CvTracker.Api.Services.Parsing;

/// <summary>
/// Extracts employment conditions — <c>ContractType</c>, <c>WorkMode</c>, and <c>WorkLoad</c> —
/// from raw offer text.
/// Internal — no interface needed; called only from <see cref="OfferTextParserService"/>.
/// </summary>
internal static class ConditionsParser
{
    /// <summary>
    /// Returns the matching <c>ContractType</c> enum name string, or <c>null</c>.
    /// B2B is checked first because it is the most specific pattern.
    /// </summary>
    internal static string? ExtractContractType(string text)
    {
        if (Regex.IsMatch(text, @"\bB2B\b"))
            return "B2B";
        if (Regex.IsMatch(text, @"(?i)\bumow[aę]\s+o\s+prac[ęe]\b|\bUoP\b"))
            return "UoP";
        if (Regex.IsMatch(text, @"(?i)\bumow[aę]\s+zleceni[ae]\b|\bzleceni[ae]\b"))
            return "MandateContract";
        if (Regex.IsMatch(text, @"(?i)\bumow[aę]\s+o\s+dzieło\b|\bo\s+dzieło\b"))
            return "SpecificWorkContract";
        if (Regex.IsMatch(text, @"(?i)\bstaż\b|\bstaz\b|\binternship\b"))
            return "Internship";
        if (Regex.IsMatch(text, @"(?i)\bpraktyki\b|\bapprenticeship\b"))
            return "Apprenticeship";

        return null;
    }

    /// <summary>
    /// Returns the matching <c>WorkMode</c> enum name string, or <c>null</c>.
    /// Hybrid takes precedence when both Hybrid and Remote keywords are present.
    /// </summary>
    internal static string? ExtractWorkMode(string text)
    {
        bool hasHybrid = OfferParserKeywords.WorkModeHybrid.Any(kw =>
            text.Contains(kw, StringComparison.OrdinalIgnoreCase));
        bool hasRemote = OfferParserKeywords.WorkModeRemote.Any(kw =>
            text.Contains(kw, StringComparison.OrdinalIgnoreCase));
        bool hasOnSite = OfferParserKeywords.WorkModeOnSite.Any(kw =>
            text.Contains(kw, StringComparison.OrdinalIgnoreCase));

        // Hybrid beats Remote to avoid false "remote" from "hybrid (2 remote days)".
        if (hasHybrid) return "Hybrid";
        if (hasRemote) return "Remote";
        if (hasOnSite) return "OnSite";

        return null;
    }

    /// <summary>Returns the matching <c>WorkLoad</c> enum name string, or <c>null</c>.</summary>
    internal static string? ExtractWorkLoad(string text)
    {
        if (Regex.IsMatch(text, @"(?i)\bfull.?time\b|\bpełn\w+\s+etat\b"))
            return "FullTime";
        if (Regex.IsMatch(text, @"(?i)\bpart.?time\b|\bczęść\s+etatu\b|\bpół\s+etatu\b"))
            return "PartTime";

        return null;
    }
}
