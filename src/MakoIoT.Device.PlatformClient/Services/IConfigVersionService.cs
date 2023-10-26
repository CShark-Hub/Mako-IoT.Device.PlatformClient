
namespace MakoIoT.Device.PlatformClient.Services
{
    public interface IConfigVersionService
    {
        int GetVersion();
        void UpdateVersion(int version);
        bool UpdateConfigSectionString(string sectionName, string sectionString, int version);
        void DeleteAllSections(int version);
    }
}
