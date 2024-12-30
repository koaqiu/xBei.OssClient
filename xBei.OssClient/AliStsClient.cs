using Microsoft.Extensions.Options;
using net.xBei.Clients.Oss.Configurations;
using net.xBei.Clients.Oss.Models;
using static net.xBei.Clients.Oss.AliOssSettings;

namespace net.xBei.Clients.Oss;
/// <summary>
/// 
/// </summary>
public class AliStsClient {
    private readonly OssAssumeRoleSettings ossAssumeRoleSettings;
    private readonly AliOssSettings ossConfigurations;
    private readonly Config ossConfig;
    private static readonly string[] OssActions = ["oss:PutObject"];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ossConfigurations"></param>
    /// <param name="ossAssumeRoleSettings"></param>
    public AliStsClient(IOptions<AliOssSettings> ossConfigurations,
                        IOptions<OssAssumeRoleSettings> ossAssumeRoleSettings) {
        this.ossAssumeRoleSettings = ossAssumeRoleSettings.Value;
        this.ossConfigurations = ossConfigurations.Value;
        ossConfig = this.ossConfigurations.Services.Values.FirstOrDefault(x => x.IsDefault) ?? this.ossConfigurations.Services.Values.First();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Obsolete("使用另一个版本")]
    public async Task<OssSettingsResponse> GetAssumeRoleAsync(int userId) {
        var roleArn = ossAssumeRoleSettings.RoleArn; //"acs:ram::1680546482100347:role/apists";
        var roleSessionName = ossAssumeRoleSettings.RoleSessionName;// "session-name";
        var path = $"u/{userId}";
        var policyTest = new {
            Version = "1",
            Statement = new[] {
                    new {
                        Action=new []{ "oss:PutObject"},
                        //                 acs:oss:*:1234:my-bucket/dir
                        Resource=new []{ $"acs:oss:*:*:{ossConfig.BucketName}/{path}/*"},
                        Effect="Allow",
                    }
                }
        };
        /*
        var policyFull = new {
            Version = "1",
            Statement = new[] {
                new {
                    Action=new []{ "oss:*"},
                    Resource=new []{ "acs:oss:*:*:*"},
                    Effect="Allow",
                }
            }
        };
        //*/
        // 构建一个阿里云client, 用于发起请求
        //var profile = DefaultProfile.GetProfile(REGIONID, ossAssumeRoleSettings.AccessKeyId, ossAssumeRoleSettings.AccessKeySecret);
        //profile.AddEndpoint(REGIONID, REGIONID, "Sts", ENDPOINT);
        var config = new AlibabaCloud.OpenApiClient.Models.Config {
            // 必填，您的 AccessKey ID
            AccessKeyId = ossAssumeRoleSettings.AccessKeyId,
            // 必填，您的 AccessKey Secret
            AccessKeySecret = ossAssumeRoleSettings.AccessKeySecret,
            // 访问的域名
            Endpoint = ossAssumeRoleSettings.EndPoint
        };
        // 用profile构造client
        var client = new AlibabaCloud.SDK.Sts20150401.Client(config);
        var request = new AlibabaCloud.SDK.Sts20150401.Models.AssumeRoleRequest {
            //Method = MethodType.POST,
            RoleArn = roleArn,
            RoleSessionName = roleSessionName,
            //Policy = policyMini.SerializeObject(), // 若policy为空，则用户将获得该角色下所有权限
            Policy = System.Text.Json.JsonSerializer.Serialize(policyTest), // 若policy为空，则用户将获得该角色下所有权限
            DurationSeconds = 900, // 设置凭证有效时间 最下 15分钟
        };

        //var response = client.GetAcsResponse(request);
        var response = await client.AssumeRoleAsync(request);
        return new OssSettingsResponse {
            Region = ossConfig.RegionId,
            AccessKeyId = response.Body.Credentials.AccessKeyId,
            AccessKeySecret = response.Body.Credentials.AccessKeySecret,
            Expiration = response.Body.Credentials.Expiration,
            SecurityToken = response.Body.Credentials.SecurityToken,
            Bucket = ossConfig.BucketName,
            ImageHost = ossConfig.ImageHost,
            Path = path,
        };
    }
    /// <summary>
    /// 获取临时授权
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="paths"></param>
    /// <param name="duratin">有效时间</param>
    /// <returns></returns>
    public async Task<OssSettingsResponse> GetAssumeRoleAsync(int userId, string[] paths, TimeSpan duratin) {
        var roleArn = ossAssumeRoleSettings.RoleArn;
        var roleSessionName = $"{ossAssumeRoleSettings.RoleSessionName}-{userId}";
        var policyTest = new {
            Version = "1",
            Statement = paths.Select(path =>
                new {
                    Action = OssActions,
                    Resource = new[] { $"acs:oss:*:*:{ossConfig.BucketName}/{path}/*" },
                    Effect = "Allow",
                }).ToArray()
        };

        // 构建一个阿里云client, 用于发起请求
        var config = new AlibabaCloud.OpenApiClient.Models.Config {
            AccessKeyId = ossAssumeRoleSettings.AccessKeyId,
            AccessKeySecret = ossAssumeRoleSettings.AccessKeySecret,
            Endpoint = ossAssumeRoleSettings.EndPoint
        };
        // 用profile构造client
        var client = new AlibabaCloud.SDK.Sts20150401.Client(config);
        var request = new AlibabaCloud.SDK.Sts20150401.Models.AssumeRoleRequest {
            //Method = MethodType.POST,
            RoleArn = roleArn,
            RoleSessionName = roleSessionName,
            Policy = System.Text.Json.JsonSerializer.Serialize(policyTest), // 若policy为空，则用户将获得该角色下所有权限
            DurationSeconds = Math.Max(900, (int)duratin.TotalSeconds), // 设置凭证有效时间 最下 15分钟
        };

        //var response = client.GetAcsResponse(request);
        var response = await client.AssumeRoleAsync(request);
        return new OssSettingsResponse {
            Region = ossConfig.RegionId,
            AccessKeyId = response.Body.Credentials.AccessKeyId,
            AccessKeySecret = response.Body.Credentials.AccessKeySecret,
            Expiration = response.Body.Credentials.Expiration,
            SecurityToken = response.Body.Credentials.SecurityToken,
            Bucket = ossConfig.BucketName,
            ImageHost = ossConfig.ImageHost,
            Paths = paths,
        };
    }
}
