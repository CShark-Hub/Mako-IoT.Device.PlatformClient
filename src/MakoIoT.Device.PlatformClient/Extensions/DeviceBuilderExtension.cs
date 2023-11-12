using MakoIoT.Device.Platform.Interface.Certificates;
using MakoIoT.Device.PlatformClient.ApiClients;
using MakoIoT.Device.PlatformClient.Services;
using MakoIoT.Device.Services.FileStorage.Interface;
using MakoIoT.Device.Services.FileStorage;
using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace MakoIoT.Device.PlatformClient.Extensions
{
    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddPlatformClient(this IDeviceBuilder builder, ConfigureDefaultsDelegate configureDefaultsAction = null)
        {
            builder.Services.AddTransient(typeof(IStreamStorageService), typeof(FileStorageService));
            builder.Services.AddTransient(typeof(ICertService), typeof(PlatformCertService));
            builder.Services.AddTransient(typeof(IConfigUpdateService), typeof(EncryptedConfigUpdateService));
            builder.Services.AddTransient(typeof(IConfigVersionService), typeof(VersionedConfigurationService));
            builder.Services.AddTransient(typeof(IConfigurationService), typeof(VersionedConfigurationService));
            builder.Services.AddTransient(typeof(IEncryptedConfigApiClient), typeof(EncryptedConfigApiClient));

            builder.ConfigureDefaultsAction = configureDefaultsAction;

            return builder;
        }
    }
}
