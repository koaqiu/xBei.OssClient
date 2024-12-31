using System.Diagnostics.CodeAnalysis;
using net.xBei.Models.AliOss;

namespace net.xBei.Clients.Oss;
/// <summary>
/// 阿里云OSS图片处理
/// </summary>
public class AliOssProcessRepository {
    private List<OssProcessAction> actions = new();
    /// <summary>
    /// 处理动作
    /// </summary>
    public IEnumerable<OssProcessAction> Actions => actions;
    /// <summary>
    /// 
    /// </summary>
    public AliOssProcessRepository() {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public AliOssProcessRepository AddAction(OssProcessAction action, bool overwrite) {
        if (overwrite) {
            var i = actions.FindIndex(a => a.Type == action.Type);
            if (i >= 0) {
                actions[i] = action;
                return this;
            }
        }
        actions.Add(action);
        return this;
    }
    /// <inheritdoc/>
    public override string ToString() {
        //x-oss-process=image/
        return $"image/{string.Join('/', actions)}";
    }
    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="repository"></param>
    /// <returns></returns>
    public static bool TryParse(Uri uri, [NotNullWhen(true)] out AliOssProcessRepository? repository) {
        //repository = new AliOssProcessRepository();
        if (uri.Query.Length < 1) {
            repository = default;
            return false;
        }
        var data = uri.Query[1..]
            .Split("&")
            .Select(s => s.Split("="))
            .Select(a => new KeyValuePair<string, string>(a[0], a.Length > 1 ? Uri.UnescapeDataString(a[1]) : string.Empty))
            .FirstOrDefault(kv => string.Compare(kv.Key, "x-oss-process", true) == 0);
        if (string.IsNullOrWhiteSpace(data.Value)) {
            repository = default;
            return false;
        }
        if (data.Value.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == false) {
            repository = default;
            return false;
        }
        repository = new AliOssProcessRepository {
            actions = data.Value.Split("/")
                        .Select(action => OssProcessAction.Parse(action) ?? OssProcessAction.Empty)
                        .ToList(),
        };
        return true;
    }
}
