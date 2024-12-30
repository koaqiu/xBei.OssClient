namespace net.xBei.Models.AliOss; 
/// <summary>
/// 
/// </summary>
public enum OssProcessActionType {
    /// <summary>
    /// 空
    /// </summary>
    empty = 0,
    /// <summary>
    /// 缩放
    /// </summary>
    resize,
    /// <summary>
    /// 图片高级压缩，转换格式
    /// </summary>
    format,
}
/// <summary>
/// 
/// </summary>
public enum ImageType {
    /// <summary>
    /// 
    /// </summary>
    jpg,
    /// <summary>
    /// 
    /// </summary>
    png,
    /// <summary>
    /// 
    /// </summary>
    webp,
    /// <summary>
    /// 
    /// </summary>
    gif,
    /// <summary>
    /// 
    /// </summary>
    tiff,
}
/// <summary>
/// 缩放的模式
/// </summary>
public enum ResizeMode {
    /// <summary>
    /// （默认值）：等比缩放，缩放图限制为指定w与h的矩形内的最大图片。
    /// </summary>
    lfit = 0,
    /// <summary>
    /// 等比缩放，缩放图为延伸出指定w与h的矩形框外的最小图片。
    /// </summary>
    mfit,
    /// <summary>
    /// 将原图等比缩放为延伸出指定w与h的矩形框外的最小图片，之后将超出的部分进行居中裁剪。
    /// </summary>
    fill,
    /// <summary>
    /// 将原图缩放为指定w与h的矩形内的最大图片，之后使用指定颜色居中填充空白部分。
    /// </summary>
    pad,
    /// <summary>
    /// 固定宽高，强制缩放。
    /// </summary>
    @fixed
}
