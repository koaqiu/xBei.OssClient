namespace net.xBei.Clients.Oss;
/// <summary>
/// 阿里OSS设置
/// </summary>
public class AliOssSettings {
    /// <summary>
    /// 可以配置多个服务，key建议使用<c>BucketName</c>
    /// </summary>
    public required Dictionary<string, Config> Services { get; set; }
    /// <summary>
    /// 是否开发模式
    /// </summary>
    public bool IsDevelopment { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public class Config {
        /// <summary>
        /// 
        /// </summary>
        public string AccessKeyId { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public string AccessKeySecret { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public string EndPoint { get; set; } = default!;
        /// <summary>
        /// 区域ID
        /// </summary>
        public string RegionId { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public string BucketName { get; set; } = default!;
        /// <summary>
        /// 自定义域名（强烈建议设置）
        /// </summary>
        public string? ImageHost { get; set; }
        /// <summary>
        /// 是否默认（只能设置一个）
        /// </summary>
        public bool IsDefault { get; set; } = default;
        //STS
        /// <summary>
        /// 过期
        /// </summary>
        public string Expiration { get; set; } = default!;
        /// <summary>
        /// Token
        /// </summary>
        public string SecurityToken { get; set; } = default!;
        /// <summary>
        /// 授权路径
        /// </summary>
        public string Path { get; set; } = default!;
        /// <summary>
        /// 授权路径
        /// </summary>
        public string Paths { get; set; } = default!;
    }
}
