using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.xBei.Clients.Oss.Models {
    /// <summary>
    /// 
    /// </summary>
    public class OssImageInfo {
        /// <summary>
        /// 
        /// </summary>
        public OssImageInfoItem FileSize { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public OssImageInfoItem Format { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public OssImageInfoItem ImageHeight { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public OssImageInfoItem ImageWidth { get; set; } = default!;
    }
    /// <summary>
    /// 
    /// </summary>
    public class OssImageInfoItem {
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; } = default!;
        /// <summary>
        /// 读取整形，如果读取失败会返回null
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int? GetInt => int.TryParse(Value, out var intV) ? intV : null;
    }
}
