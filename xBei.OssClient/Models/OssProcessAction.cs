using System.Text.RegularExpressions;

namespace net.xBei.Models.AliOss; 
/// <summary>
/// 阿里Oss图片处理动作
/// </summary>
public abstract class OssProcessAction {
    /// <summary>
    /// 
    /// </summary>
    public abstract OssProcessActionType Type { get; }
    /// <summary>
    /// 输出
    /// </summary>
    /// <returns></returns>
    public abstract new string ToString();
    /// <summary>
    /// 
    /// </summary>
    public OssProcessAction() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="paramList"></param>
    public OssProcessAction(IEnumerable<string> paramList) {
        Parse(paramList);
    }
    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="paramList"></param>
    protected abstract void Parse(IEnumerable<string> paramList);
    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static OssProcessAction? Parse(string value) {
        var a = value.Split(",");
        if (a.Length < 2) {
            return default;
        }
        if (Enum.TryParse<OssProcessActionType>(a[0], out var type) == false) {
            return default;
        }
        return type switch {
            OssProcessActionType.format => new OssProcessFormat(a.Skip(1)),
            OssProcessActionType.resize => new OssProcessResize(a.Skip(1)),
            _ => default,
        };
    }
    /// <summary>
    /// 空
    /// </summary>
    public static OssProcessAction Empty { get; } = new EmptyOssProcessAction();
}
/// <summary>
/// 
/// </summary>
public class EmptyOssProcessAction : OssProcessAction {
    /// <inheritdoc/>
    public override OssProcessActionType Type => OssProcessActionType.empty;
    /// <summary>
    /// 
    /// </summary>
    public EmptyOssProcessAction() { }
    /// <inheritdoc/>
    public override string ToString() {
        return string.Empty;
    }
    /// <inheritdoc/>
    protected override void Parse(IEnumerable<string> paramList) { }
}
/// <summary>
/// 转换图片格式
/// </summary>
public class OssProcessFormat : OssProcessAction {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="paramList"></param>
    public OssProcessFormat(IEnumerable<string> paramList) : base(paramList) {
    }
    /// <inheritdoc/>
    public override OssProcessActionType Type => OssProcessActionType.format;
    /// <summary>
    /// 图片格式
    /// </summary>
    public ImageType ImageType { get; set; }
    /// <inheritdoc/>
    public override string ToString() {
        return $"{Type},{ImageType}";
    }
    /// <inheritdoc/>
    protected override void Parse(IEnumerable<string> paramList) {
        if (Enum.TryParse<ImageType>(paramList.FirstOrDefault(), out var result)) {
            ImageType = result;
        }
    }
}
/// <summary>
/// 修改图片大小
/// </summary>
public class OssProcessResize : OssProcessAction {
    /// <summary>
    /// 
    /// </summary>
    public OssProcessResize() : base() {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="paramList"></param>
    public OssProcessResize(IEnumerable<string> paramList) : base(paramList) {
    }
    /// <inheritdoc/>
    public override OssProcessActionType Type => OssProcessActionType.resize;
    /// <summary>
    /// 缩放类型
    /// </summary>
    public ResizeMode Mode { get; set; }
    /// <summary>
    /// 指定当目标缩放图大于原图时是否进行缩放。
    /// </summary>
    public bool Limit { get; set; } = true;
    /// <summary>
    /// 当缩放模式选择为pad（缩放填充）时，可以设置填充的颜色。
    /// RGB颜色值，例如：000000表示黑色，FFFFFF表示白色。
    /// 默认值：FFFFFF（白色）
    /// <see cref="ResizeMode.pad"/>
    /// </summary>
    public string? Color { get; set; }
    /// <summary>
    /// 宽度
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 高度
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 长边
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// 短边
    /// </summary>
    public int Short { get; set; }
    /// <inheritdoc/>
    public override string ToString() {
        return string.Join(',', new[] {
            $"{Type}",
            Width > 0 ? $"w_{Width}" : string.Empty,
            Height > 0 ? $"h_{Height}" : string.Empty,
            Length > 0 ? $"l_{Length}" : string.Empty,
            Short > 0 ? $"s_{Short}" : string.Empty,
            $"m_{Mode}"
        }
        .Where(s => string.IsNullOrWhiteSpace(s) == false));
    }
    /// <inheritdoc/>
    protected override void Parse(IEnumerable<string> paramList) {
        foreach (var p in paramList) {
            var a = p.Split("_");
            if (a.Length != 2)
                return;
            if (int.TryParse(a[1], out var value)) {
                switch (a[0].ToLower()) {
                    case "w": Width = value; break;
                    case "h": Height = value; break;
                    case "l": Length = value; break;
                    case "s": Short = value; break;
                    case "limit": Limit = 1 == value; break;
                }
            } else if (a[0].ToLower() == "m" && Enum.TryParse<ResizeMode>(a[1], out var mode)) {
                Mode = mode;
            } else if (a[0].ToLower() == "color" && regColorHex.IsMatch(a[1])) {
                Color = a[1];
            }
        }
    }
    private readonly Regex regColorHex = new("^[0-9a-f]{6}$", RegexOptions.IgnoreCase);
}
