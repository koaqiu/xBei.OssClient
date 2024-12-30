using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.xBei.Clients.Oss.Configurations;
/// <summary>
/// 
/// </summary>
public class OssAssumeRoleSettings : AliServiceSettings
{
    /// <summary>
    /// 角色
    /// </summary>
    public string RoleArn { get; set; } = default!;
    /// <summary>
    /// 角色会话名称
    /// </summary>
    public string RoleSessionName { get; set; } = default!;
}
/// <summary>
/// 阿里服务配置（基础）
/// </summary>
public class AliServiceSettings
{
    /// <summary>
    /// 区域ID
    /// </summary>
    public string RegionId { get; set; } = default!;
    /// <summary>
    /// 入口
    /// </summary>
    public string EndPoint { get; set; } = default!;
    /// <summary>
    /// 
    /// </summary>
    public string AccessKeyId { get; set; } = default!;
    /// <summary>
    /// 密钥
    /// </summary>
    public string AccessKeySecret { get; set; } = default!;
    /// <summary>
    /// 入口列表
    /// </summary>
    public IEnumerable<string> EndPoints { get; set; } = default!;
}