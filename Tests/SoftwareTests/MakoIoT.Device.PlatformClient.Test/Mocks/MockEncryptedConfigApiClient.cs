
using System.IO;
using MakoIoT.Device.PlatformClient.ApiClients;

namespace MakoIoT.Device.PlatformClient.Test.Mocks
{
    public class MockEncryptedConfigApiClient : IEncryptedConfigApiClient
    {
        public Stream ConfigStream { get; set; }
        public int ConfigVersion { get; set; }


        public int GetLatestConfigVersion()
        {
            return ConfigVersion;
        }

        public Stream GetConfig(int version, Stream privateKey)
        {
            return ConfigStream;
        }

        public void UpdateConfigVersion(int version)
        {
        }
    }
}
