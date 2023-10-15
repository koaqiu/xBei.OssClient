# xBei.OssClient

## 介绍
阿里oss客户端，基于阿里官方sdk封装，支持net6.0;net7.0。
主要封装了Object相关操作，支持依赖注入。

**开发中**

## 使用
安装：`PM > Install-Package xBei.OssClient`

### 使用依赖注入
配置`appsettings.json`
```json
{
  "OssSettings": {
    "Services":{
        "xbei-img":{
            "AccessKeyId": "AccessKeyId",
            "AccessKeySecret": "Secret",
            "EndPoint": "oss-cn-shanghai.aliyuncs.com",
            "BucketName": "xbei-img",
            "ImageHost": "https://i.xbei.net"
        }
     }
  }
}
```
注册服务：
```csharp
services.Configure<AliOssSettings>(configuration.GetSection("OssSettings"));
services.AddSingleton<AliOssClient>();
```

### 直接初始化
```csharp
//STS临时授权
var ossClient = new AliOssClient(new AliOssSettings.Config {
    AccessKeyId = "AccessKeyId",
    AccessKeySecret = "AccessKeySecret",
    SecurityToken = "SecurityToken",
    EndPoint = "oss-cn-hangzhou.aliyuncs.com",
    BucketName = "Bucket",
    ImageHost = "https://i.xbei.net",
    Path = "Path",
    Expiration = "Expiration",
    IsDefault = true,
}, logger);

//AK授权
var ossClient = new AliOssClient(new AliOssSettings.Config {
    AccessKeyId = "AccessKeyId",
    AccessKeySecret = "AccessKeySecret",
    EndPoint = "oss-cn-shanghai.aliyuncs.com",
    BucketName = "Bucket",
    ImageHost = "https://i.xbei.net",
    IsDefault = true,
}, logger);
```
