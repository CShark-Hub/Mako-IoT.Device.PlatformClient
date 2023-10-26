using nanoFramework.TestFramework;
using System;
using System.IO;
using System.Text;
using MakoIoT.Device.PlatformClient.Services;
using MakoIoT.Device.PlatformClient.Test.Mocks;

namespace MakoIoT.Device.PlatformClient.Test
{
    [TestClass]
    public class EncryptedConfigUpdateServiceTest
    {
        [TestMethod]
        public void UpdateConfiguration_should_extract_data_single_section()
        {
            string config = @"<V>2
<S>mysection
{
    ""Prop1"":""value1"",
    ""Prop2"":""value2"",
    ""Prop3"":""value3"",
}";
            using var configStream = new MemoryStream(Encoding.UTF8.GetBytes(config));

            var cfgStorageSvc = new MockConfigVersionService
            {
                Version = 1
            };

            var storageSvc = new MockStreamStorageService();

            var sut = new EncryptedConfigUpdateService(new MockEncryptedConfigApiClient
                {
                    ConfigStream = configStream,
                    ConfigVersion = 2
                },
                cfgStorageSvc, storageSvc);

            sut.UpdateConfiguration();

            Assert.AreEqual(2, cfgStorageSvc.Version);
            Assert.AreEqual("mysection", cfgStorageSvc.SaveSectionName);
            Assert.AreEqual(@"{
    ""Prop1"":""value1"",
    ""Prop2"":""value2"",
    ""Prop3"":""value3"",
}", cfgStorageSvc.SaveData);
        }



        [TestMethod]
        public void UpdateConfiguration_should_extract_data_next_section()
        {
            string config = @"<V>2
<S>mysection
{
    ""Prop1"":""value1"",
    ""Prop2"":""value2"",
    ""Prop3"":""value3"",
}
<S>mysecondsection
{
    ""Prop4"":""value1"",
    ""Prop5"":""value2"",
    ""Prop6"":""value3"",
}";
            using var configStream = new MemoryStream(Encoding.UTF8.GetBytes(config));

            var cfgStorageSvc = new MockConfigVersionService
            {
                Version = 1
            };

            var storageSvc = new MockStreamStorageService();

            var sut = new EncryptedConfigUpdateService(new MockEncryptedConfigApiClient
                {
                    ConfigStream = configStream,
                    ConfigVersion = 2
            },
                cfgStorageSvc, storageSvc);

            sut.UpdateConfiguration();

            Assert.AreEqual(2, cfgStorageSvc.Version);
            Assert.AreEqual("mysecondsection", cfgStorageSvc.SaveSectionName);
            Assert.AreEqual(@"{
    ""Prop4"":""value1"",
    ""Prop5"":""value2"",
    ""Prop6"":""value3"",
}", cfgStorageSvc.SaveData);
        }

        [TestMethod]
        public void UpdateConfiguration_should_exit_if_stream_is_null()
        {
            var cfgStorageSvc = new MockConfigVersionService
            {
                Version = 1
            };

            var storageSvc = new MockStreamStorageService();

            var sut = new EncryptedConfigUpdateService(new MockEncryptedConfigApiClient
                {
                    ConfigStream = null
                },
                cfgStorageSvc, storageSvc);

            sut.UpdateConfiguration();

            Assert.AreEqual(1, cfgStorageSvc.Version);
            Assert.AreEqual(null, cfgStorageSvc.SaveData);
        }
    }
}
