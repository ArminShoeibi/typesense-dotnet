using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Typesense.Tests;

[Trait("Category", "Unit")]
public class CurationResponseTests
{
    [Fact]
    public void SearchResponses_DeserializeCurationMetadata()
    {
        var json = @"{
            ""metadata"": {
                ""curation_id"": ""summer-promotion"",
                ""banner"": { ""title"": ""Summer deals"" },
                ""tags"": [""featured"", ""summer""],
                ""priority"": 2,
                ""enabled"": true,
                ""optional"": null
            }
        }";

        AssertMetadata(JsonSerializer.Deserialize<SearchResult<string>>(json).Metadata);
        AssertMetadata(JsonSerializer.Deserialize<SearchGroupedResult<string>>(json).Metadata);
        AssertMetadata(JsonSerializer.Deserialize<MultiSearchResult<string>>(json).Metadata);
        AssertMetadata(JsonSerializer.Deserialize<UnionSearchResult<string>>(json).Metadata);
    }

    [Fact]
    public void SearchResponses_WithoutMetadata_LeaveMetadataNull()
    {
        JsonSerializer.Deserialize<SearchResult<string>>("{}").Metadata.Should().BeNull();
        JsonSerializer.Deserialize<SearchGroupedResult<string>>("{}").Metadata.Should().BeNull();
        JsonSerializer.Deserialize<MultiSearchResult<string>>("{}").Metadata.Should().BeNull();
        JsonSerializer.Deserialize<UnionSearchResult<string>>("{}").Metadata.Should().BeNull();
    }

    [Theory]
    [InlineData(@"""curated"": true,", true)]
    [InlineData(@"""curated"": false,", false)]
    [InlineData("", null)]
    public void SearchResponses_DeserializeCuratedHits(string curatedProperty, bool? expected)
    {
        var json = "{\"hits\": [{" + curatedProperty + "\"document\": \"product\", \"highlights\": []}]}";

        JsonSerializer.Deserialize<SearchResult<string>>(json).Hits[0].Curated.Should().Be(expected);
        JsonSerializer.Deserialize<MultiSearchResult<string>>(json).Hits[0].Curated.Should().Be(expected);
        JsonSerializer.Deserialize<UnionSearchResult<string>>(json).Hits[0].Curated.Should().Be(expected);

        var groupedJson = "{\"grouped_hits\": [{\"group_key\": [\"category\"], \"hits\": [{"
            + curatedProperty + "\"document\": \"product\", \"highlights\": []}]}]}";

        JsonSerializer.Deserialize<SearchGroupedResult<string>>(groupedJson)
            .GroupedHits[0].Hits[0].Curated.Should().Be(expected);
    }

    private static void AssertMetadata(Dictionary<string, JsonElement> metadata)
    {
        metadata.Should().NotBeNull();
        metadata["curation_id"].GetString().Should().Be("summer-promotion");
        metadata["banner"].GetProperty("title").GetString().Should().Be("Summer deals");
        metadata["tags"][0].GetString().Should().Be("featured");
        metadata["tags"].GetArrayLength().Should().Be(2);
        metadata["priority"].GetInt32().Should().Be(2);
        metadata["enabled"].GetBoolean().Should().BeTrue();
        metadata["optional"].ValueKind.Should().Be(JsonValueKind.Null);
    }
}
