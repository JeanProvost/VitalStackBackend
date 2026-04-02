using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Core.Configuration;

public class DataBaseSettings
{
    public const string SectionName = "DataBase";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}

public class AwsCognitoSettings
{
    public const string SectionName = "AwsCognito";
    [Required] public string UserPoolId { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
    [Required] public string CognitoRegion { get; set; } = string.Empty;
}

public class CorsSettings
{
    public const string PolicyName = "AllowSpecificOrigins";
    public const string SectionName = "Cors";
    [Required] public string[] AllowOrigins {  get; set; } = Array.Empty<string>();
}
