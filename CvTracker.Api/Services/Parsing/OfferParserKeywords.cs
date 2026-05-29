namespace CvTracker.Api.Services.Parsing;

/// <summary>
/// Single source of truth for all keyword arrays used by the offer text parsers.
/// Referenced exclusively by <see cref="TitleCompanyParser"/>, <see cref="ConditionsParser"/>,
/// and <see cref="SectionParser"/> — no keyword literals remain in parser bodies.
/// </summary>
internal static class OfferParserKeywords
{
    // ── Position / title detection ────────────────────────────────────────────

    /// <summary>Role keywords used in strategy-2 position detection.</summary>
    internal static readonly string[] RoleKeywords =
    [
        "senior", "junior", "mid", "lead", "principal", "staff",
        "developer", "engineer", "architect", "analyst", "designer",
        "manager", "specialist", "consultant", "devops", "qa", "tester",
        "programist", "programista", "inżynier", "specjalista", "lider",
    ];

    /// <summary>Seniority markers for strategy-3 condition A.</summary>
    internal static readonly string[] SeniorityKeywords =
    [
        "senior", "junior", "mid", "lead", "principal", "staff",
        "fullstack", "full-stack", "qa", "tester",
    ];

    /// <summary>Role terms for strategy-3 condition A and condition B.</summary>
    internal static readonly string[] RoleTermKeywords =
    [
        "developer", "engineer", "architect", "analyst", "designer",
        "specialist", "consultant", "devops", "programist", "programista",
        "inżynier", "specjalista",
    ];

    // ── Work mode ─────────────────────────────────────────────────────────────

    internal static readonly string[] WorkModeHybrid = ["hybr"];
    internal static readonly string[] WorkModeRemote = ["zdaln", "remote"];
    internal static readonly string[] WorkModeOnSite = ["stacjonar", "on-site", "onsite"];

    // ── Section headings ──────────────────────────────────────────────────────

    internal static readonly string[] SectionHeadingRequirements =
    [
        "wymagania", "requirements", "oczekujemy", "oczekiwania",
        "wymagane", "what we require",
    ];

    internal static readonly string[] SectionHeadingOffer =
    [
        "oferujemy", "co oferujemy", "what we offer",
        "benefity", "benefits", "co zyskasz",
    ];

    // ── Company name validation ───────────────────────────────────────────────

    /// <summary>
    /// First-person plural Polish verbs that indicate a sentence, not a company name.
    /// Any candidate containing one of these is rejected by <c>IsValidCompanyCandidate</c>.
    /// </summary>
    internal static readonly string[] CompanyExclusionVerbs =
    [
        "realizujemy", "oferujemy", "szukamy", "jesteśmy", "zajmujemy",
        "pracujemy", "tworzymy", "budujemy", "działamy", "pomagamy",
    ];

    /// <summary>
    /// Work-mode words that indicate a description, not a company name.
    /// A candidate containing any of these is rejected by <c>IsValidCompanyCandidate</c>.
    /// </summary>
    internal static readonly string[] WorkModeExclusionKeywords =
    [
        "remote", "hybrid", "office", "zdaln", "hybryd", "stacjonar", "on-site", "onsite",
    ];

    /// <summary>
    /// Legal form suffixes/abbreviations used to identify company names.
    /// Listed from longest to shortest so that more-specific patterns take priority.
    /// </summary>
    internal static readonly string[] LegalFormPatterns =
    [
        "GmbH & Co. KG",
        "Spółka z o.o.",
        "Sp. z o.o.",
        "S.K.A.",
        "Sp. k.",
        "Sp. j.",
        "Sp. p.",
        "B.V.",
        "S.A.",
        "Ltd.",
        "GmbH",
        "Inc.",
        "LLC",
        "SA",
    ];

    /// <summary>Connector words between proper nouns indicating a multi-word company name.</summary>
    internal static readonly string[] ConnectorWords = ["and", "&", "i", "oraz"];

    /// <summary>Well-known Polish cities used in location detection and company-name rejection.</summary>
    internal static readonly string[] KnownCities =
    [
        "Warszawa", "Kraków", "Wrocław", "Poznań", "Gdańsk", "Gdynia", "Łódź",
        "Katowice", "Rzeszów", "Lublin", "Szczecin", "Bydgoszcz", "Toruń",
        "Białystok", "Kielce", "Częstochowa", "Radom", "Sosnowiec", "Gliwice",
        "Zabrze", "Bytom", "Olsztyn", "Bielsko-Biała", "Rybnik", "Tychy", "Opole",
    ];
}
