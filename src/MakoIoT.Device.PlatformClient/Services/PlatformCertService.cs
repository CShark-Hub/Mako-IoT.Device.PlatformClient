
using MakoIoT.Device.Platform.Interface;
using MakoIoT.Device.Platform.Interface.Certificates;
using MakoIoT.Device.Services.FileStorage.Interface;

namespace MakoIoT.Device.PlatformClient.Services
{
    public class PlatformCertService : ICertService
    {
        private readonly IStreamStorageService _storageService;

        public PlatformCertService(IStreamStorageService storageService)
        {
            _storageService = storageService;
        }

        public CertificateWrapper GetCertificates()
        {
            return new CertificateWrapper(
                _storageService.ReadFile(Constants.PlatformCertificateFile));
        }
    }
}
