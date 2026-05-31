using CvTracker.Api.Services.Parsing;

namespace CvTracker.Api.Services;

/// <summary>
/// Implements <see cref="IOfferTextParserService"/> by delegating to four focused parsers:
/// <see cref="TitleCompanyParser"/>, <see cref="ConditionsParser"/>, and <see cref="SectionParser"/>.
/// All parsing is local — no external API calls are made.
/// </summary>
public sealed class OfferTextParserService : IOfferTextParserService
{
    private readonly ISkillNormalizationService _normalizationService;
    private readonly TitleCompanyParser _titleCompanyParser;
    private const int MaxTextLength = 20_000;

    public OfferTextParserService(ISkillNormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
        _titleCompanyParser = new TitleCompanyParser(normalizationService);
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
            Position        = _titleCompanyParser.ExtractPosition(text, lines),
            CompanyName     = _titleCompanyParser.ExtractCompanyName(text, lines),
            Location        = SectionParser.ExtractLocation(text),
            ContractType    = ConditionsParser.ExtractContractType(text),
            WorkMode        = ConditionsParser.ExtractWorkMode(text),
            WorkLoad        = ConditionsParser.ExtractWorkLoad(text),
            SalaryMin       = salaryMin,
            SalaryMax       = salaryMax,
            OurRequirements = SectionParser.ExtractSection(text, OfferParserKeywords.SectionHeadingRequirements),
            WhatWeOffer     = SectionParser.ExtractSection(text, OfferParserKeywords.SectionHeadingOffer),
            RequiredSkillIds = _normalizationService.FindAllInText(text).ToList(),
        };
    }
}
