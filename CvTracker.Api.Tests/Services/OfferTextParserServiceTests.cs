using CvTracker.Api.Services;
using CvTracker.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CvTracker.Api.Tests.Services;

/// <summary>
/// Unit tests for <see cref="OfferTextParserService.Parse"/>.
/// Uses a real <see cref="SkillNormalizationService"/> backed by an EF Core InMemory database.
/// </summary>
public class OfferTextParserServiceTests
{
    // ── Infrastructure helpers ────────────────────────────────────────────────

    /// <summary>Builds a service provider with an in-memory AppDbContext.</summary>
    private static IServiceProvider BuildServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a <see cref="SkillNormalizationService"/> pre-seeded with the supplied
    /// technologies and aliases.
    /// </summary>
    private static async Task<SkillNormalizationService> BuildNormalizationServiceAsync(
        string dbName,
        IEnumerable<(int Id, string Name, string Category, string[] Aliases)> techs)
    {
        var provider = BuildServiceProvider(dbName);
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int aliasId = 1;
        foreach (var (id, name, category, aliases) in techs)
        {
            db.Technologies.Add(TestBuilders.BuildTechnology(id, name, category));
            foreach (var alias in aliases)
                db.TechnologyAliases.Add(new TechnologyAlias { Id = aliasId++, Alias = alias, TechnologyId = id });
        }
        await db.SaveChangesAsync();

        var sut = new SkillNormalizationService(provider.GetRequiredService<IServiceScopeFactory>());
        await sut.InitializeAsync();
        return sut;
    }

    /// <summary>Creates an <see cref="OfferTextParserService"/> with a minimal normalization service.</summary>
    private static async Task<OfferTextParserService> BuildParserAsync(
        string dbName,
        IEnumerable<(int Id, string Name, string Category, string[] Aliases)>? techs = null)
    {
        var normService = await BuildNormalizationServiceAsync(dbName, techs ?? []);
        return new OfferTextParserService(normService);
    }

    // ── Position extraction ───────────────────────────────────────────────────

    [Fact]
    public async Task Parse_PositionViaStanowiskoLabel_ReturnsPosition()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Stanowisko: Senior .NET Developer\nFirma: Acme");

        result.Position.Should().Be("Senior .NET Developer");
    }

    [Fact]
    public async Task Parse_PositionViaPositionLabel_ReturnsPosition()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Position: Backend Engineer\nLocation: Warsaw");

        result.Position.Should().Be("Backend Engineer");
    }

    [Fact]
    public async Task Parse_PositionViaRoleKeyword_ReturnsFirstMatchingLine()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = "Acme Sp. z o.o.\nSenior Developer\nWarszawa, zdalnie";
        var result = parser.Parse(text);

        result.Position.Should().Be("Senior Developer");
    }

    [Fact]
    public async Task Parse_PositionInferredFromFirstShortLine_ReturnsLine()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // No label, no keyword — first 2–8 word line without period.
        const string text = "Java Backend Engineer\nDołącz do naszego zespołu.";
        var result = parser.Parse(text);

        result.Position.Should().Be("Java Backend Engineer");
    }

    [Fact]
    public async Task Parse_NoPositionCues_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // Single-word lines + long sentence — no extractable position.
        const string text = "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod.";
        var result = parser.Parse(text);

        result.Position.Should().BeNull();
    }

    // ── Company name extraction ───────────────────────────────────────────────

    [Fact]
    public async Task Parse_CompanyNameViaFirmaLabel_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Firma: TechCorp S.A.\nStanowisko: Developer");

        result.CompanyName.Should().Be("TechCorp S.A.");
    }

    [Fact]
    public async Task Parse_CompanyNameViaEmployerLabel_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Employer: Nexus Solutions\nLocation: Warsaw");

        result.CompanyName.Should().Be("Nexus Solutions");
    }

    [Fact]
    public async Task Parse_CompanyNameViaONasLabel_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "O nas:" is used on some Polish job sites to list the company name on the same line.
        var result = parser.Parse("O nas: Acme Sp. z o.o.\nJesteśmy dynamiczną firmą.");

        result.CompanyName.Should().Be("Acme Sp. z o.o.");
    }

    [Fact]
    public async Task Parse_CompanyNameViaAboutUsLabel_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("About us: Bright Future Ltd\nWe are growing fast.");

        result.CompanyName.Should().Be("Bright Future Ltd");
    }

    [Fact]
    public async Task Parse_CompanyNameViaAtPattern_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Senior Developer at Nexus Solutions in Warsaw, full-time.");

        result.CompanyName.Should().Be("Nexus Solutions");
    }

    [Fact]
    public async Task Parse_CompanyNameViaWFirmiePattern_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Dołącz do naszego zespołu w firmie Acme. Praca zdalna.");

        result.CompanyName.Should().Be("Acme");
    }

    [Fact]
    public async Task Parse_CompanyNameViaDlaPattern_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Rekrutujemy dla TechCorp. Wyślij CV na hr@techcorp.pl.");

        result.CompanyName.Should().Be("TechCorp");
    }

    [Fact]
    public async Task Parse_CompanyNameViaCapitalisedPhrase_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // A standalone 2–5 word title-case line in the first 300 chars that is not a job title.
        const string text = "Bright Future Solutions\nStanowisko: .NET Developer\nWarszawa";

        var result = parser.Parse(text);

        result.CompanyName.Should().Be("Bright Future Solutions");
    }

    [Fact]
    public async Task Parse_CompanyNameCapitalisedPhraseIsJobTitle_DoesNotReturnJobTitleAsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Senior Developer" is title-case but is a job title — should NOT be returned as company name.
        const string text = "Senior Developer\nFirma: TechCorp\nWarszawa";

        var result = parser.Parse(text);

        result.CompanyName.Should().Be("TechCorp");
    }

    // ── Position — strategy 3 tightening ─────────────────────────────────────

    [Fact]
    public async Task Parse_PositionCityName_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Warszawa" is a single word — fails all strategy-3 conditions (word count < 2).
        const string text = "Warszawa\nOferujemy pracę zdalną, B2B.";
        var result = parser.Parse(text);

        result.Position.Should().BeNull();
    }

    [Fact]
    public async Task Parse_PositionSectionHeading_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Benefity" is a single-word section heading — word count < 2, strategy 3 does not fire.
        const string text = "Benefity\nPrywatna opieka medyczna";
        var result = parser.Parse(text);

        result.Position.Should().BeNull();
    }

    // ── Company name — legal form heuristic ───────────────────────────────────

    [Fact]
    public async Task Parse_CompanyNameViaLegalFormSpZOO_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = "Acme Sp. z o.o.\nOpis stanowiska.";
        var result = parser.Parse(text);

        result.CompanyName.Should().Be("Acme Sp. z o.o.");
    }

    [Fact]
    public async Task Parse_CompanyNameViaLegalFormGmbH_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = "TechCorp GmbH\nSzukamy programisty.";
        var result = parser.Parse(text);

        result.CompanyName.Should().Be("TechCorp GmbH");
    }

    [Fact]
    public async Task Parse_CompanyNameViaLegalFormLtd_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = "Future Ltd.\nJoin our team.";
        var result = parser.Parse(text);

        result.CompanyName.Should().Be("Future Ltd.");
    }

    // ── Company name — candidate validation ───────────────────────────────────

    [Fact]
    public async Task Parse_CompanyNameTitleCaseFallback_ExclusionVerbRejectsCandidate_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Tworzymy" is an exclusion verb — IsValidCompanyCandidate returns false.
        const string text = "Tworzymy Nowoczesne Rozwiązania\nStanowisko: Developer";
        var result = parser.Parse(text);

        result.CompanyName.Should().BeNull();
    }

    [Fact]
    public async Task Parse_CompanyNameTitleCaseFallback_DashRejectsCandidate_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Tech-Corp" contains a hyphen — IsValidCompanyCandidate returns false.
        const string text = "Tech-Corp Solutions\nStanowisko: Developer";
        var result = parser.Parse(text);

        result.CompanyName.Should().BeNull();
    }

    [Fact]
    public async Task Parse_CompanyNameTitleCaseFallback_CityCommaRejectsCandidate_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Centrum, Warszawa" matches the comma + known-city pattern — candidate rejected.
        const string text = "Centrum, Warszawa\nStanowisko: Developer";
        var result = parser.Parse(text);

        result.CompanyName.Should().BeNull();
    }

    [Fact]
    public async Task Parse_CompanyNameTitleCaseFallback_ValidStandaloneLine_ReturnsCompany()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Bright Minds Agency" is a standalone title-case line with no disqualifying signals.
        const string text = "Bright Minds Agency\nStanowisko: Developer";
        var result = parser.Parse(text);

        result.CompanyName.Should().Be("Bright Minds Agency");
    }


    [Fact]
    public async Task Parse_LocationViaLokalizacjaLabel_ReturnsLocation()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Lokalizacja: Wrocław (hybryda)\nStanowisko: Engineer");

        result.Location.Should().Be("Wrocław (hybryda)");
    }

    [Fact]
    public async Task Parse_LocationViaKnownCity_ReturnsCity()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // "Warszawa" appears in nominative form, which the known-city pattern matches.
        var result = parser.Parse("Stanowisko: Developer. Warszawa lub praca zdalna. Aplikuj teraz.");

        result.Location.Should().Be("Warszawa");
    }

    // ── ContractType extraction ───────────────────────────────────────────────

    [Fact]
    public async Task Parse_ContractTypeB2BKeyword_ReturnsB2B()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Kontrakt: B2B lub UoP, wynagrodzenie 15 000 PLN netto.");

        result.ContractType.Should().Be("B2B");
    }

    [Fact]
    public async Task Parse_ContractTypeUmowaOPrace_ReturnsUoP()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Oferujemy umowę o pracę na czas nieokreślony.");

        result.ContractType.Should().Be("UoP");
    }

    [Fact]
    public async Task Parse_ContractTypeZlecenie_ReturnsMandateContract()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Forma zatrudnienia: umowa zlecenie.");

        result.ContractType.Should().Be("MandateContract");
    }

    [Fact]
    public async Task Parse_ContractTypeNotFound_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Dołącz do naszego zespołu. Praca zdalna. Elastyczne godziny.");

        result.ContractType.Should().BeNull();
    }

    // ── WorkMode extraction ───────────────────────────────────────────────────

    [Fact]
    public async Task Parse_WorkModeHybridWhenBothRemoteAndHybridPresent_ReturnsHybrid()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Praca hybrydowa (2 dni zdalnie, 3 dni w biurze).");

        result.WorkMode.Should().Be("Hybrid");
    }

    [Fact]
    public async Task Parse_WorkModeZdalni_ReturnsRemote()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Stanowisko w pełni zdalne, możliwość pracy z dowolnego miejsca.");

        result.WorkMode.Should().Be("Remote");
    }

    [Fact]
    public async Task Parse_WorkModeNotFound_ReturnsNull()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Aplikuj teraz. Wyślij CV na adres hr@firma.pl.");

        result.WorkMode.Should().BeNull();
    }

    // ── WorkLoad extraction ───────────────────────────────────────────────────

    [Fact]
    public async Task Parse_WorkLoadFullTime_ReturnsFullTime()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("We are looking for a full-time developer.");

        result.WorkLoad.Should().Be("FullTime");
    }

    [Fact]
    public async Task Parse_WorkLoadCzescEtatu_ReturnsPartTime()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        var result = parser.Parse("Zatrudnimy na część etatu, 20 godzin tygodniowo.");

        result.WorkLoad.Should().Be("PartTime");
    }

    // ── Section extraction ────────────────────────────────────────────────────

    [Fact]
    public async Task Parse_OurRequirementsUnderWymaganiaHeading_ExtractsSection()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = """
            Stanowisko: Developer
            Wymagania:
            - min. 3 lata doświadczenia
            - znajomość C#
            Oferujemy:
            - karta Multisport
            """;

        var result = parser.Parse(text);

        result.OurRequirements.Should().Contain("3 lata doświadczenia");
        result.OurRequirements.Should().Contain("C#");
    }

    [Fact]
    public async Task Parse_WhatWeOfferUnderOferujemyHeading_ExtractsSection()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        const string text = """
            Stanowisko: Developer
            Wymagania:
            - C# 10+
            Oferujemy:
            - karta Multisport
            - prywatna opieka medyczna
            """;

        var result = parser.Parse(text);

        result.WhatWeOffer.Should().Contain("Multisport");
        result.WhatWeOffer.Should().Contain("opieka medyczna");
    }

    // ── Skill extraction ──────────────────────────────────────────────────────

    [Fact]
    public async Task Parse_RequiredSkillIds_DeduplicatedAndFromAliasTable()
    {
        var parser = await BuildParserAsync(
            Guid.NewGuid().ToString(),
            [(1, "C#", "Languages", ["c#", "csharp"]),
             (2, "SQL", "Databases", ["sql"])]);

        // "c#" and "csharp" both map to tech id 1; "sql" maps to id 2.
        var result = parser.Parse(
            "Wymagamy znajomości C# (csharp) oraz SQL. Wynagrodzenie 12 000 PLN brutto.");

        result.RequiredSkillIds.Should().BeEquivalentTo([1, 2]);
    }

    // ── Truncation defence ────────────────────────────────────────────────────

    [Fact]
    public async Task Parse_TextExceeds20000Chars_TruncatedBeforeParsing_DoesNotThrow()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // Build a 25 000-character string; parser must not throw and must complete.
        var longText = "Stanowisko: Developer\n" + new string('x', 25_000);

        var act = () => parser.Parse(longText);

        act.Should().NotThrow();
        var result = parser.Parse(longText);
        result.Position.Should().Be("Developer");
    }

    // ── Edge case ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Parse_CompletelyUnrecognisedText_ReturnsEmptyDtoWithoutException()
    {
        var parser = await BuildParserAsync(Guid.NewGuid().ToString());

        // Gibberish — no recognisable fields, all should be null / empty list.
        var result = parser.Parse(
            "aaaaa bbbbb ccccc ddddd eeeee fffff ggggg hhhhh iiiii jjjjj kkkkk");

        result.Position.Should().BeNull();
        result.CompanyName.Should().BeNull();
        result.Location.Should().BeNull();
        result.ContractType.Should().BeNull();
        result.WorkMode.Should().BeNull();
        result.WorkLoad.Should().BeNull();
        result.SalaryMin.Should().BeNull();
        result.SalaryMax.Should().BeNull();
        result.RequiredSkillIds.Should().BeEmpty();
    }
}
