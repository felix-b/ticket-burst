using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace TicketBurst.Tests.TryOut;

[TestFixture]
public class DotnetTests
{
    [Test]
    public void CanDeserializeJsonIntoCSharpRecord()
    {
        var json = "{\"username\":\"MY_USER\",\"host\":\"MY_HOST\",\"port\":3306}";

        var secret = JsonSerializer.Deserialize<MySecretRecord>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        secret.Should().NotBeNull();
        secret!.Host.Should().Be("MY_HOST");
        secret!.UserName.Should().Be("MY_USER");
        secret!.Port.Should().Be(3306);
    }

    public record MySecretRecord(
        string UserName, 
        string Host, 
        int Port
    );
}

