using MakoIoT.Device.PlatformClient.Services;
using System;

namespace MakoIoT.Device.PlatformClient.Test.Mocks
{
    public class MockConfigVersionService : IConfigVersionService
    {
        public int Version { get; set; }

        public string SaveSectionName { get; set; }
        public string SaveData { get; set; }

        public int GetVersion()
        {
            return Version;
        }

        public void UpdateVersion(int version)
        {
            Version = version;
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString, int version)
        {
            SaveSectionName = sectionName;
            Version = version;
            SaveData = sectionString;
            return true;
        }

        public void DeleteAllSections(int version)
        {
            
        }
    }
}
