using System;
using System.IO;
using System.Text;
using MakoIoT.Device.Platform.Interface;
using MakoIoT.Device.PlatformClient.ApiClients;
using MakoIoT.Device.Services.FileStorage.Interface;

namespace MakoIoT.Device.PlatformClient.Services
{
    public class EncryptedConfigUpdateService : IConfigUpdateService
    {
        private readonly IEncryptedConfigApiClient _configApiClient;
        private readonly IConfigVersionService _configVersionService;
        private readonly IStreamStorageService _storageService;

        public EncryptedConfigUpdateService(IEncryptedConfigApiClient configApiClient, IConfigVersionService configVersionService, IStreamStorageService storageService)
        {
            _configApiClient = configApiClient;
            _configVersionService = configVersionService;
            _storageService = storageService;
        }

        public void UpdateConfiguration()
        {
            int currentVersion = _configVersionService.GetVersion();
            int latestVersion = _configApiClient.GetLatestConfigVersion();

            if (latestVersion <= currentVersion)
                return;

            int version = -1;

            if (!_storageService.FileExists(Constants.DeviceEncryptionKeyFile))
                throw new Exception("Device encryption file not found.");
            
            using (var deviceKey = _storageService.ReadFileStream(Constants.DeviceEncryptionKeyFile))
            {
                using (var s = _configApiClient.GetConfig(latestVersion, deviceKey.BaseStream))
                {
                    if (s == null)
                        return;

                    using (var reader = new StreamReader(s))
                    {
                        string section = null;
                        var sectionBuilder = new StringBuilder();

                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                            {
                                _configVersionService.UpdateConfigSectionString(section,
                                    sectionBuilder.ToString().TrimEnd('\n', '\r'), version);
                                break;
                            }

                            if (line.Length > 3)
                            {
                                switch (line.Substring(0, 3))
                                {
                                    case "<V>":
                                        version = int.Parse(line.Substring(3));
                                        continue;
                                    case "<S>":
                                        if (section != null)
                                        {
                                            _configVersionService.UpdateConfigSectionString(section,
                                                sectionBuilder.ToString().TrimEnd('\n', '\r'), version);
                                            sectionBuilder.Clear();
                                        }

                                        section = line.Substring(3);
                                        continue;
                                }
                            }

                            sectionBuilder.AppendLine(line);
                        }
                    }
                }
            }

            //TODO: validate sections

            _configVersionService.UpdateVersion(version);

            _configVersionService.DeleteAllSections(currentVersion);

            _configApiClient.UpdateConfigVersion(version);
        }
    }
}
