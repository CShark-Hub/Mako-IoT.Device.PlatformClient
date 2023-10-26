using System.IO;

namespace MakoIoT.Device.PlatformClient.ApiClients
{
    public interface IEncryptedConfigApiClient
    {
        int GetLatestConfigVersion();
        Stream GetConfig(int version, Stream privateKey);
        void UpdateConfigVersion(int version);
    }
}
