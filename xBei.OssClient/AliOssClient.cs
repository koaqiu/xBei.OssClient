using Aliyun.OSS;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace net.xBei.Clients.Oss {
    /// <summary>
    /// 阿里OSS操作，简单封装
    /// </summary>
    public sealed class AliOssClient {
        //private readonly OssSettingsResponse ossSettings;
        private readonly ILogger<AliOssClient>? logger;
        private readonly AliOssSettings options;
        private readonly static Dictionary<string, AliOssClient> clients = new();
        private readonly AliOssSettings.Config config;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <exception cref="Exception"></exception>
        public AliOssClient(ILogger<AliOssClient> logger, IOptions<AliOssSettings> options) {
            this.options = options.Value;
            if (!(this.options.Services?.Count > 0)) {
                LogError("AliOssClient 配置错误");
                throw new Exception("AliOssClient 配置错误");
            }
            if (this.options.Services.Count > 1 && this.options.Services.Values.All(x => x.IsDefault == false)) {
                LogError("AliOssClient 配置错误：必须指定一个默认配置");
                throw new Exception("AliOssClient 配置错误：必须指定一个默认配置");
            }

            config = this.options.Services.Values.FirstOrDefault(x => x.IsDefault) ?? this.options.Services.Values.First();
            Client = CreateClient(config);
            BucketName = config.BucketName;
            Host = string.IsNullOrWhiteSpace(config.ImageHost) ? $"{config.BucketName}.{config.EndPoint}" : config.ImageHost;
            this.logger = logger;
            clients[config.BucketName] = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public AliOssClient(AliOssSettings.Config config, ILogger<AliOssClient>? logger) {
            this.logger = logger;
            this.config = config;
            Client = string.IsNullOrWhiteSpace(config.SecurityToken) ? CreateClient(config) : CreateClientBySts(config);
            BucketName = config.BucketName;
            Host = string.IsNullOrWhiteSpace(config.ImageHost) ? $"{config.BucketName}.{config.EndPoint}" : config.ImageHost;

            this.options = new AliOssSettings {
                Services = new Dictionary<string, AliOssSettings.Config> {
                    [config.BucketName] = config
                }
            };
            clients[config.BucketName] = this;
        }

        private void LogError(string msg) => logger?.LogError(message: msg);
        /// <summary>
        /// 切换Bucket，一般无需使用
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool TryGetClient(string bucketName, [NotNullWhen(true)] out AliOssClient? client) {
            if (clients.TryGetValue(bucketName, out client)) {
                return true;
            }
            if (options.Services.TryGetValue(bucketName, out var config)) { 
                client = new AliOssClient(config, logger);
                return true;
            }
            client = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public string BucketName { get; private set; }
        /// <summary>
        /// 所有配置好的BucketName
        /// </summary>
        public IEnumerable<string> BucketList => options.Services.Values.Select(x => x.BucketName);
        /// <summary>
        /// OSS的域名（可能时自定义域名）
        /// </summary>
        public string Host { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public OssClient Client { get; private set; }
        private OssClient CreateClient(AliOssSettings.Config config) {
            if (string.IsNullOrWhiteSpace(config.EndPoint)
                || string.IsNullOrWhiteSpace(config.AccessKeyId)
                || string.IsNullOrWhiteSpace(config.AccessKeySecret)) {
                LogError("AliOssClient 配置错误：必须指定一个“EndPoint”、“AccessKeyId”、“AccessKeySecret”");
                throw new Exception("AliOssClient 配置错误：必须指定一个“EndPoint”、“AccessKeyId”、“AccessKeySecret”");
            }
            return new OssClient(config.EndPoint, config.AccessKeyId, config.AccessKeySecret);
        }
        private OssClient CreateClientBySts(AliOssSettings.Config ossSettings) {
            if (string.IsNullOrWhiteSpace(ossSettings.EndPoint)
                || string.IsNullOrWhiteSpace(ossSettings.AccessKeyId)
                || string.IsNullOrWhiteSpace(ossSettings.AccessKeySecret)
                || string.IsNullOrWhiteSpace(ossSettings.SecurityToken)) {
                LogError("AliOssClient 配置错误：必须指定一个“EndPoint”、“AccessKeyId”、“AccessKeySecret”、“SecurityToken”");
                throw new Exception("AliOssClient 配置错误：必须指定“EndPoint”、“AccessKeyId”、“AccessKeySecret”、“SecurityToken”");
            }
            return new OssClient(ossSettings.EndPoint, ossSettings.AccessKeyId, ossSettings.AccessKeySecret, ossSettings.SecurityToken);
        }

        /// <summary>
        /// Object是否存在
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool ObjectExist(string obj) => Client.DoesObjectExist(BucketName, obj);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public ObjectMetadata GetObjectMetadata(string objectKey) => Client.GetObjectMetadata(BucketName, objectKey);
        #region 上传
        /// <summary>
        /// 上传文本到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="content"></param>
        /// <param name="encoding">不传默认是<see cref="Encoding.UTF8"/></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<PutObjectResult> UploadAsync(string objectKey, string content, Encoding? encoding = null, string contentType = "text/plain")
            => await UploadAsync(objectKey, (encoding ?? Encoding.UTF8).GetBytes(content), contentType);
        /// <summary>
        /// 上传JSON数据到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<PutObjectResult> UploadJsonAsync(string objectKey, string json)
            => await UploadAsync(objectKey, Encoding.UTF8.GetBytes(json), contentType: "application/json");
        /// <summary>
        /// 上传数据到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<PutObjectResult> UploadAsync(string objectKey, byte[] content, string contentType = "application/octet-stream")
            => await UploadAsync(objectKey, new MemoryStream(content), contentType);
        /// <summary>
        /// 上传数据到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="stream">必须是可读的</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<PutObjectResult> UploadAsync(string objectKey, Stream stream, string contentType = "application/octet-stream")
            => await UploadAsync(objectKey, stream, new ObjectMetadata() { ContentType = contentType });
        /// <summary>
        /// 上传文件到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="fileToUpload">文件不可访问会报错</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<PutObjectResult> UploadFileAsync(string objectKey, string fileToUpload, string? contentType = default)
            => await UploadFileAsync(objectKey,
                                         fileToUpload,
                                         new ObjectMetadata() { ContentType = GetContentTypeByFilename(fileToUpload, contentType) });
        /// <summary>
        /// 上传文件到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="fileToUpload"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Task<PutObjectResult> UploadFileAsync(string objectKey, string fileToUpload, ObjectMetadata metadata) {
            if (!File.Exists(fileToUpload)) {
                LogError($"文件不存在：{fileToUpload}");
                throw new FileNotFoundException("文件不存在", fileToUpload);
            }
            var tcs = new TaskCompletionSource<PutObjectResult>();
            var result = Client.BeginPutObject(BucketName,
                                               objectKey,
                                               fileToUpload,
                                               metadata,
                                               asyncResult => {
                                                   var result = Client.EndPutObject(asyncResult);
                                                   tcs.SetResult(result);
                                               },
                                               objectKey);
            return tcs.Task;
        }
        /// <summary>
        /// 上传数据到 oss
        /// </summary>
        /// <param name="objectKey"></param>
        /// <param name="stream">必须是可读的</param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<PutObjectResult> UploadAsync(string objectKey, Stream stream, ObjectMetadata metadata)
            => await PutObjectAsync(new PutObjectRequest(BucketName, objectKey, stream, metadata));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<PutObjectResult> PutObjectAsync(PutObjectRequest request) {
            var tcs = new TaskCompletionSource<PutObjectResult>();
            var result = Client.BeginPutObject(request,
                                               asyncResult => {
                                                   var result = Client.EndPutObject(asyncResult);
                                                   tcs.SetResult(result);
                                               },
                                               request.Key);
            return tcs.Task;
        }
        #endregion
        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="objectKey"></param>
        public void DeleteObject(string objectKey) => Client.DeleteObject(BucketName, objectKey);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool DeleteObject(Uri uri) {
            var bucketName = GetBucketName(uri);
            if (string.IsNullOrWhiteSpace(bucketName)) {
                LogError($"无效的地址：{uri}");
                throw new Exception($"无效的地址：{uri}");
            }
            var key = System.Web.HttpUtility.UrlDecode(uri.AbsolutePath[1..]);
            if (Client.DoesObjectExist(bucketName, key) == false) {
                return true;
            }

            Client.DeleteObject(bucketName, key);
            return Client.DoesObjectExist(bucketName, key) == false;
        }
        #endregion
        #region 复制
        /// <summary>
        /// 复制（相同Bucket）
        /// </summary>
        /// <param name="sourceObjectKey"></param>
        /// <param name="targetObjectKey"></param>
        /// <returns></returns>
        public Task<CopyObjectResult> CopyAsync(string sourceObjectKey, string targetObjectKey) {
            var tcs = new TaskCompletionSource<CopyObjectResult>();
            var result = Client.BeginCopyObject(new CopyObjectRequest(BucketName, sourceObjectKey, BucketName, targetObjectKey),
                                               asyncResult => {
                                                   var result = Client.EndCopyResult(asyncResult);
                                                   tcs.SetResult(result);
                                               },
                                               $"copy {sourceObjectKey} to {targetObjectKey}");
            return tcs.Task;
        }
        #endregion
        #region 下载
        /// <summary>
        /// 从oss下载文本数据（默认编码：<see cref="Encoding.UTF8"/>）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> DownloadStringAsync(string key)
            => Encoding.UTF8.GetString((await DownloadAsync(key)).data);
        /// <summary>
        /// 从oss下载数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal async Task<(byte[] data, string contentType)> DownloadAsync(string key) {
            using var stream = new MemoryStream();
            string? contentType;
            try {
                var obj = await GetObjectAsync(BucketName, key);
                contentType = obj.Metadata.ContentType;
                using var requestStream = obj.Content;
                await requestStream.CopyToAsync(stream);
            } catch (Exception ex) {
                contentType = string.Empty;
                Console.WriteLine($"Download oss://{BucketName}/{key} error {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            return (stream.ToArray(), contentType);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(MemoryStream data, string contentType)> DownloadAsync(Uri uri, string process) {
            var bucketName = GetBucketName(uri);
            if (string.IsNullOrWhiteSpace(bucketName)) {
                LogError($"无效的地址：{uri}");
                throw new Exception($"无效的地址：{uri}");
            }
            var key = System.Web.HttpUtility.UrlDecode(uri.AbsolutePath[1..]);
            using var stream = new MemoryStream();
            string? contentType;
            try {
                var obj = await GetObjectAsync(new GetObjectRequest(bucketName, key, process));
                contentType = obj.Metadata.ContentType;
                using var requestStream = obj.Content;
                await requestStream.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            } catch (Exception ex) {
                contentType = string.Empty;
                LogError(ex.Message);
                Console.WriteLine($"Download oss://{bucketName}/{key} error {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            return (stream, contentType);
        }
        /// <summary>
        /// 从Oss中下载对象（返回流）
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public async Task<Stream?> DownloadStreamAsync(string objKey) {
            var ossObj = await GetObjectAsync(BucketName, objKey);
            return ossObj.HttpStatusCode == System.Net.HttpStatusCode.OK ? ossObj.Content : default;
        }
        /// <summary>
        /// 从Oss中获取对象
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public Task<OssObject> GetObjectAsync(string bucketName, string objKey) 
            => GetObjectAsync(new GetObjectRequest(bucketName, objKey));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public Task<OssObject> GetObjectAsync(string objKey) => GetObjectAsync(new GetObjectRequest(BucketName, objKey));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<OssObject> GetObjectAsync(GetObjectRequest request) {
            var tcs = new TaskCompletionSource<OssObject>();
            //#if DEBUG
            //            Console.WriteLine($"Download:{request.BucketName} {request.Key}");
            //#endif
            logger?.LogDebug(message: "Download:{BucketName} {Key}", request.BucketName, request.Key);
            Client.BeginGetObject(request, asyncCallback => {
                var result = Client.EndGetObject(asyncCallback);
                tcs.SetResult(result);
            }, null);
            return tcs.Task;
        }
        #endregion
        /// <summary>
        /// 从<paramref name="url"/>中判断BucketName，判断失败返回 null
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string? GetBucketName(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) ? GetBucketName(uri) : null;
        /// <summary>
        /// 从<paramref name="uri"/>中判断BucketName，判断失败返回 null
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string? GetBucketName(Uri uri) {
            //https://dpj1.oss-cn-shenzhen.aliyuncs.com/Asset/1000/2021-4-10/14302827/62082401-efb9-4a3c-9641-67f50119f4b8.png
            foreach (var item in options.Services) {
                if (string.Compare(uri.Host, item.Value.ImageHost, true) == 0) {
                    return item.Value.BucketName;
                }
            }
            if (uri.Host.EndsWith(".aliyuncs.com", true, null)) {
                return uri.Host.Split(".").FirstOrDefault();
            }
            return null;
        }
        /// <summary>
        /// 获取可以访问的地址
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public string GetUrl(string objKey) {
            var ub = new UriBuilder(Host) {
                Path = objKey
            };
            return ub.Uri.ToString();
        }
        /// <summary>
        /// 判断<paramref name="url"/>是不是系统支持的oss文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="bucket"></param>
        /// <param name="ossObjKey"></param>
        /// <returns></returns>
        public bool IsOssFile(string? url, [NotNullWhen(true)] out string? bucket, [NotNullWhen(true)] out string? ossObjKey) {
            bucket = null;
            ossObjKey = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                return false;
            }
            var hosts = options.Services.Values.Select(x => new {
                host = x.ImageHost ?? string.Empty,
                x.BucketName
            })
                                .Where(x => !string.IsNullOrWhiteSpace(x.BucketName))
                                .ToArray();
            if (!hosts.Any()) {
                return false;
            }
            foreach (var item in hosts) {
                if (uri.Host.Equals(item.host, StringComparison.OrdinalIgnoreCase)) {
                    ossObjKey = uri.AbsolutePath[1..];
                    bucket = item.BucketName;
                    return true;
                }
            }
            if (uri.Host.EndsWith(".aliyuncs.com", true, null)) {
                bucket = uri.Host.Split(".").FirstOrDefault();
                ossObjKey = uri.AbsolutePath[1..];
            }
            ossObjKey = null;
            return false;
        }

        private static bool IsMatch(string input, Regex regex) => regex.IsMatch(input);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetContentTypeByFilename(string filename, string? contentType = default) {
            if (string.IsNullOrWhiteSpace(contentType)) {
                var extension = Path.GetExtension(filename).ToLower();
                contentType = extension switch {
                    ".csv" => "text/csv",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".xlsm" => "application/vnd.ms-excel.sheet.macroEnabled.12",
                    ".zip" => "application/zip",
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg ",
                    ".jpeg" => "image/jpeg ",
                    ".json" => "application/json",
                    ".psd" => "image/vnd.adobe.photoshop",
                    _ => "application/octet-stream",
                };
            }
            return contentType;
        }
    }
}