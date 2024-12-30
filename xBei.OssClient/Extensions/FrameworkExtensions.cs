using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using net.xBei.Clients.Oss.Configurations;

namespace net.xBei.Clients.Oss.Extensions {
    /// <summary>
    /// 
    /// </summary>
    public static class FrameworkExtensions {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IServiceCollection SetupAliOss(this IServiceCollection services,
                                                     IConfigurationRoot configuration,
                                                     string configKey) {
            services.Configure<AliOssSettings>(configuration.GetSection(configKey));
            services.AddSingleton<AliOssClient>();
            return services;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IServiceCollection SetupAliSts(this IServiceCollection services,
                                                     IConfigurationRoot configuration,
                                                     string configKey) {
            //services.Configure<OssAssumeRoleSettings>(configuration.GetSection("OssAssumeRoleSettings"));
            services.Configure<OssAssumeRoleSettings>(configuration.GetSection(configKey));
            services.AddSingleton<AliStsClient>();
            return services;
        }
    }
}
