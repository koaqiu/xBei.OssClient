namespace net.xBei.Clients.Oss.Models;
/// <summary>
/// oss上传设置
/// </summary>
public class OssSettingsResponse {
    /// <summary>
    /// Token
    /// </summary>
    public string SecurityToken { get; set; } = default!;
    /// <summary>
    /// 密钥
    /// </summary>
    public string AccessKeySecret { get; set; } = default!;
    /// <summary>
    /// id
    /// </summary>
    public string AccessKeyId { get; set; } = default!;
    /// <summary>
    /// 过期
    /// </summary>
    public string Expiration { get; set; } = default!;
    /// <summary>
    /// 仓库
    /// </summary>
    public string Bucket { get; set; } = default!;
    /// <summary>
    /// 区域
    /// </summary>
    public string Region { get; set; } = default!;
    /// <summary>
    /// OSS主机
    /// </summary>
    public string? ImageHost { get; set; }
    /// <summary>
    /// 授权路径
    /// </summary>
    [Obsolete("使用“Paths”")]
    public string Path { get; set; } = default!;
    /// <summary>
    /// 授权路径
    /// </summary>
    public string[] Paths { get; set; } = default!;
}
