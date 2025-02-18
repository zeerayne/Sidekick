using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sidekick.Apis.Poe.Clients;
using Sidekick.Apis.Poe.Modifiers;
using Sidekick.Apis.Poe.Trade;
using Sidekick.Common.Settings;
using Xunit;

namespace Sidekick.Apis.Poe.Tests.Poe2.Query;

[Collection(Collections.Poe2Parser)]
public class TradeSearchServiceTests
{
    private readonly IItemParser parser;
    private readonly ITradeFilterService tradeFilterService;
    private readonly MockHttpClient mockHttpClient = new();
    private readonly TradeSearchService tradeSearchService;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public TradeSearchServiceTests(ParserFixture fixture)
    {
        parser = fixture.Parser;
        tradeFilterService = fixture.TradeFilterService;

        var tradeClient = new Mock<IPoeTradeClient>();
        tradeClient
            .Setup(x => x.HttpClient)
            .Returns(mockHttpClient);

        tradeClient
            .Setup(x => x.Options)
            .Returns(jsonSerializerOptions);

        tradeSearchService = new TradeSearchService(
            NullLogger<TradeSearchService>.Instance,
            fixture.GameLanguageProvider,
            fixture.SettingsService,
            tradeClient.Object,
            fixture.ModifierProvider,
            fixture.FilterProvider,
            fixture.PropertyParser
        );
    }

    [Fact]
    public async Task ExpectInputYieldsIsNotIdentified()
    {
        var input = ResourceHelper.ReadFileContent("TimeLostDiamond/item.txt");

        var parsedItem = parser.ParseItem(input);
        var propertyFilters = await tradeFilterService.GetPropertyFilters(parsedItem);
        var result = await tradeSearchService.Search(parsedItem, propertyFilters);
        Assert.Null(result.Error);

        var actual = Assert.Single(mockHttpClient.Requests);
        var actualQuery = JsonSerializer.Deserialize<Trade.Requests.QueryRequest>(actual, jsonSerializerOptions);

        var identified = actualQuery?.Query.Filters.MiscFilters?.Filters.Identified?.Option;
        Assert.Equal("false", identified);
    }

    [Fact]
    public void ExpectQueryIsNotIdentified()
    {
        var expectedQuery = ResourceHelper.ReadFileContent("TimeLostDiamond/query.json");
        var queryRequest = JsonSerializer.Deserialize<Trade.Requests.QueryRequest>(expectedQuery, jsonSerializerOptions);

        var identified = queryRequest?.Query.Filters.MiscFilters?.Filters.Identified?.Option;
        Assert.Equal("false", identified);
    }

    [Fact]
    public void ExpectSerializationDeserializationNoChanges()
    {
        var expected = ResourceHelper.ReadFileContent("TimeLostDiamond/query.json");
        var queryRequest = JsonSerializer.Deserialize<Trade.Requests.QueryRequest>(expected, jsonSerializerOptions);

        var actual = JsonSerializer.Serialize(queryRequest, jsonSerializerOptions);

        Assert.Equal(actual, expected);
    }

    [Theory]
    [InlineData("TimeLostDiamond/item.txt", "TimeLostDiamond/query.json")]
    public async Task InputShouldCreateExpectedQuery(string pathToInput, string pathToExpectedQuery)
    {
        var input = ResourceHelper.ReadFileContent(pathToInput);
        var expectedQuery = ResourceHelper.ReadFileContent(pathToExpectedQuery);

        var parsedItem = parser.ParseItem(input);
        var propertyFilters = await tradeFilterService.GetPropertyFilters(parsedItem);
        var result = await tradeSearchService.Search(parsedItem, propertyFilters);
        Assert.Null(result.Error);

        var actual = Assert.Single(mockHttpClient.Requests);

        Assert.Equal(actual, expectedQuery);
    }
}
