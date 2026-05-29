using System.Text;
using System.Text.RegularExpressions;

namespace CvTracker.Api.Services;

/// <summary>
/// Implements <see cref="IOfferTextParserService"/> using regex/heuristic matching.
/// All parsing is local — no external API calls are made.
/// </summary>
public sealed class OfferTextParserService : IOfferTextParserService
{
    private readonly ISkillNormalizationService _normalizationService;
    private const int MaxTextLength = 20_000;

    // Role keywords used to identify a position line.
    private static readonly string[] RoleKeywords =
    [
        "senior", "junior", "mid", "lead", "principal", "staff",
        "developer", "engineer", "architect", "analyst", "designer",
        "manager", "specialist", "consultant", "devops", "qa", "tester",
        "programist", "programista", "inżynier", "specjalista", "lider"
    ];

    // Well-known Polish cities for location detection.
    private static readonly Regex KnownCitiesPattern = new(
        @"\b(Warszawa|Kraków|Wrocław|Poznań|Gdańsk|Gdynia|Łódź|Katowice|Rzeszów" +
        @"|Lublin|Szczecin|Bydgoszcz|Toruń|Białystok|Kielce|Częstochowa|Radom" +
        @"|Sosnowiec|Gliwice|Zabrze|Bytom|Olsztyn|Bielsko-Biała|Rybnik|Tychy|Opole)\b",
        RegexOptions.Compiled);

    // Detects a "generic heading" line that signals the end of a parsed section.
    // Matches lines that are entirely a heading (ends with ":" or is ALL-CAPS-ish short label).
    private static readonly Regex GenericHeadingPattern = new(
        @"^\s*[^\n]{2,60}:\s*$|^\s*[A-ZŁÓŹĄĆĘŚŃ][A-ZŁÓŹĄĆĘŚŃ\s]{2,40}$",
        RegexOptions.Compiled);

    public OfferTextParserService(ISkillNormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
    }

    /// <inheritdoc/>
    public ScrapedOfferDto Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ScrapedOfferDto { RequiredSkillIds = [] };

        // Defence-in-depth: hard truncate before any regex work.
        if (text.Length > MaxTextLength)
            text = text[..MaxTextLength];

        var lines = text.Split('\n', StringSplitOptions.None)
                        .Select(l => l.TrimEnd())
                        .ToArray();

        var (salaryMin, salaryMax) = SalaryParser.ParseSalary(text);

        return new ScrapedOfferDto
        {
            Position        = ExtractPosition(text, lines),
            CompanyName     = ExtractCompanyName(text),
            Location        = ExtractLocation(text),
            ContractType    = ExtractContractType(text),
            WorkMode        = ExtractWorkMode(text),
            WorkLoad        = ExtractWorkLoad(text),
            SalaryMin       = salaryMin,
            SalaryMax       = salaryMax,
            OurRequirements = ExtractSection(text,
                ["wymagania", "requirements", "oczekujemy", "oczekiwania",
                 "wymagane", "what we require"]),
            WhatWeOffer     = ExtractSection(text,
                ["oferujemy", "co oferujemy", "what we offer",
                 "benefity", "benefits", "co zyskasz"]),
            RequiredSkillIds = _normalizationService.FindAllInText(text).ToList(),
        };
    }

    // ── Extraction helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the job position/title using three strategies in order:
    /// 1. Label match ("Stanowisko:" / "Position:")
    /// 2. Role keyword found in early lines
    /// 3. First short non-sentence line (2–8 words, no period)
    /// </summary>
    private static string? ExtractPosition(string text, string[] lines)
    {
        // 1. Label-based.
        var labelMatch = Regex.Match(text,
            @"(?i)(?:stanowisko|position)\s*[:\u2014]\s*([^\n]{1,100})");
        if (labelMatch.Success)
        {
            var v = labelMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // 2. Role keyword in first 10 non-empty lines.
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)).Take(10))
        {
            var trimmed = line.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is >= 2 and <= 10 &&
                RoleKeywords.Any(kw => trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            {
                return trimmed;
            }
        }

        // 3. First 2–8-word line without a period (title-like).
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)).Take(5))
        {
            var trimmed = line.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is >= 2 and <= 8 && !trimmed.Contains('.'))
                return trimmed;
        }

        return null;
    }

    /// <summary>Extracts company name from label, pattern-based clues, or capitalised phrase.</summary>
    private static string? ExtractCompanyName(string text)
    {
        // 1. Label-based: case-insensitive; handles colon, en-dash (\u2013), and em-dash (\u2014).
        //    Covers: Firma, Company, Pracodawca, Employer, O nas, About us, Kim jesteśmy.
        var labelMatch = Regex.Match(text,
            @"(?i)(?:firma|company|pracodawca|employer|o\s+nas|about\s+us|kim\s+jeste[sś]my)\s*[:\u2013\u2014]\s*([^\n]{1,100})");
        if (labelMatch.Success)
        {
            var v = labelMatch.Groups[1].Value.Split('\n')[0].Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // Inline patterns search only the first 500 characters (per spec).
        var head = text.Length > 500 ? text[..500] : text;

        // 2. English "at <CompanyName>" — matches 1–3 capitalised words (including Polish capitals).
        //    No (?i) flag so [A-ZŁÓŹĄĆĘŚŃ] remains case-sensitive, capturing only proper nouns.
        var atMatch = Regex.Match(head,
            @"\bat\s+([A-ZŁÓŹĄĆĘŚŃ][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-\.]*(?:\s+[A-ZŁÓŹĄĆĘŚŃ][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-\.]*){0,2})(?=[\s,\.\n]|$)");
        if (atMatch.Success)
        {
            var v = atMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        // 3. Polish "w firmie <Name>" — 1–3 words; no dot in char class to avoid over-matching.
        var wFirmieMatch = Regex.Match(head,
            @"(?i)\bw\s+firmie\s+([A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*(?:\s+[A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*){0,2})(?=[\s,\.\n]|$)");
        if (wFirmieMatch.Success)
        {
            var v = wFirmieMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v) && char.IsUpper(v[0])) return v;
        }

        // 4. Polish "dla <Name>" — e.g. "pracuj dla Nexus" or "rekrutujemy dla TechCorp".
        //    Excludes role-keyword matches so "dla Senior Developera" is not treated as a company name.
        var dlaMatch = Regex.Match(head,
            @"(?i)\bdla\s+([A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*(?:\s+[A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń][A-Za-zŁÓŹĄĆĘŚŃłóźąćęśń0-9&\-]*){0,2})(?=[\s,\.\n]|$)");
        if (dlaMatch.Success)
        {
            var v = dlaMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v) && char.IsUpper(v[0]) &&
                !RoleKeywords.Any(kw => v.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                return v;
        }

        // 5. Capitalised phrase heuristic: a standalone line of 2–5 title-case words in the
        //    first 300 chars that does NOT look like a job title.
        var head300 = text.Length > 300 ? text[..300] : text;
        foreach (var line in head300.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2 || words.Length > 5) continue;
            // Every word must start with an uppercase letter (Unicode-aware).
            if (!words.All(w => w.Length > 0 && char.IsUpper(w[0]))) continue;
            // Skip lines that look like a job title.
            if (RoleKeywords.Any(kw => trimmed.Contains(kw, StringComparison.OrdinalIgnoreCase))) continue;
            return trimmed;
        }

        return null;
    }

    /// <summary>Extracts location from label or known-city list.</summary>
    private static string? ExtractLocation(string text)
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

    /// <summary>Returns the matching ContractType enum name string, or null.</summary>
    private static string? ExtractContractType(string text)
    {
        // B2B checked first as it's the most specific.
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
    /// Returns the matching WorkMode enum name string, or null.
    /// Hybrid takes precedence when both Hybrid and Remote keywords are present.
    /// </summary>
    private static string? ExtractWorkMode(string text)
    {
        bool hasHybrid = Regex.IsMatch(text, @"(?i)\bhybr");
        bool hasRemote = Regex.IsMatch(text, @"(?i)\bzdaln|\bremote\b");
        bool hasOnSite = Regex.IsMatch(text, @"(?i)\bstacjonar|\bon.?site\b");

        // Hybrid beats Remote to avoid false "remote" from "hybrid (2 remote days)".
        if (hasHybrid) return "Hybrid";
        if (hasRemote) return "Remote";
        if (hasOnSite) return "OnSite";

        return null;
    }

    /// <summary>Returns the matching WorkLoad enum name string, or null.</summary>
    private static string? ExtractWorkLoad(string text)
    {
        if (Regex.IsMatch(text, @"(?i)\bfull.?time\b|\bpełn\w+\s+etat\b"))
            return "FullTime";
        if (Regex.IsMatch(text, @"(?i)\bpart.?time\b|\bczęść\s+etatu\b|\bpół\s+etatu\b"))
            return "PartTime";

        return null;
    }

    /// <summary>
    /// Extracts the text block following any of the supplied <paramref name="headings"/>
    /// (case-insensitive) up to the next generic heading or end of text.
    /// Returns <c>null</c> when the heading is not found.
    /// </summary>
    private static string? ExtractSection(string text, string[] headings)
    {
        var lines = text.Split('\n');
        bool inSection = false;
        var sb = new StringBuilder();

        // Build a pattern that matches a line whose trimmed content IS one of the headings.
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
                // Stop when we hit the next generic heading and have already collected content.
                if (sb.Length > 0 && GenericHeadingPattern.IsMatch(line))
                    break;

                sb.AppendLine(line);
            }
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
