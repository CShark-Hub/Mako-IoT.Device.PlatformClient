using System.IO;
using MakoIoT.Device.Services.FileStorage.Interface;

namespace MakoIoT.Device.PlatformClient.Test.Mocks
{
    public class MockStreamStorageService : IStreamStorageService
    {
        public void WriteToFile(string fileName, string text)
        {
            throw new System.NotImplementedException();
        }

        public bool FileExists(string fileName)
        {
            return true;
        }

        public string ReadFile(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFile(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles()
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFileNames()
        {
            throw new System.NotImplementedException();
        }

        public StreamWriter WriteToFileStream(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public StreamReader ReadFileStream(string fileName)
        {
            return new StreamReader(new MemoryStream(new byte[0]));
        }
    }
}
