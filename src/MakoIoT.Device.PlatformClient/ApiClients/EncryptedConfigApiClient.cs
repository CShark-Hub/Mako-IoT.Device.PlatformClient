using System;
using System.IO;
using System.Net;
using System.Net.Http;
using MakoIoT.Device.Platform.Interface.Certificates;
using MakoIoT.Device.Platform.Interface.Configuration;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.PlatformClient.ApiClients
{
    public class EncryptedConfigApiClient : IEncryptedConfigApiClient, IDisposable
    {
        public const string ApiKeyHeaderName = "X-API-Key";

        private readonly PlatformConfig _config;
        private readonly HttpClient _httpClient;

        public EncryptedConfigApiClient(IConfigurationService configService, ICertService certService)
        {
            _config = (PlatformConfig)configService.GetConfigSection(PlatformConfig.SectionName,
                typeof(PlatformConfig));

            _httpClient = new HttpClient();

            _httpClient.HttpsAuthentCert = certService.GetCertificates()?.X509Certificate;
            _httpClient.DefaultRequestHeaders.Add(ApiKeyHeaderName, _config.PlatformServiceApiKey);
        }

        public int GetLatestConfigVersion()
        {
            using var response = _httpClient.Get($"{_config.PlatformServiceUrl}/configversion");

            response.EnsureSuccessStatusCode();

            return int.Parse(response.Content.ReadAsString());
        }

        public Stream GetConfig(int version, Stream privateKey)
        {
            using var response = _httpClient.Post($"{_config.PlatformServiceUrl}/encryptedconfig?version={version}", 
                new StreamContent(privateKey));

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStream();
        }

        public void UpdateConfigVersion(int version)
        {
            using var putResponse = _httpClient.Put($"{_config.PlatformServiceUrl}/configversion?currentversion={version}", null);

            putResponse.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
