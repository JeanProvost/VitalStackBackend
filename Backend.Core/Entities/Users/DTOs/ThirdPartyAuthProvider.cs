using System.Text.Json.Serialization;

namespace Backend.Core.Entities.Users.DTOs;

[JsonConverter(typeof(JsonStringEnumConverter<ThirdPartyAuthProvider>))]
public enum ThirdPartyAuthProvider
{
    Google,
    Apple
}
