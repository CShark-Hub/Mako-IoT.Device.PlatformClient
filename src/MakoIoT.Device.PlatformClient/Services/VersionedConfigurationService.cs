using System;
using System.Collections;
using System.Reflection;
using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.Logging;
using nanoFramework.Json;

namespace MakoIoT.Device.PlatformClient.Services
{
    public class VersionedConfigurationService : IConfigurationService, IConfigVersionService
    {
        private const string ConfigVersionFileName = "mako-configversion";

        private readonly IStorageService _storage;
        private readonly ILogger _logger;
        private string _configString;
        private readonly object _writeLock = new object();

        public event EventHandler ConfigurationUpdated;

        public VersionedConfigurationService(IStorageService storage, ILogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public object GetConfigSection(string sectionName, Type objectType)
        {
            string sectionStr = LoadConfigSection(sectionName);

            if (sectionStr != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject(sectionStr, objectType);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error in config section {sectionName}", e);
                }
            }

            var defaultProp = objectType.GetMethod("get_Default", BindingFlags.Public | BindingFlags.Static);
            if (defaultProp != null)
            {
                _logger.LogInformation($"Using default config for section {sectionName}");
                return defaultProp.Invoke(null, null);
            }

            throw new ConfigurationException("Can't load configuration nor default");
        }

        public bool TryGetConfigSection(string sectionName, Type objectType, out object section)
        {
            throw new NotImplementedException();
        }

        public void UpdateConfigSection(string sectionName, object section)
        {
            string sectionStr = JsonConvert.SerializeObject(section);
            if (SaveConfigSection(sectionStr, sectionName, GetVersion()))
            {
                ConfigurationUpdated?.Invoke(this, new ObjectEventArgs(sectionName));
            }
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString)
        {
            bool result = SaveConfigSection(sectionString, sectionName, GetVersion());
            if (result)
            {
                ConfigurationUpdated?.Invoke(this, new ObjectEventArgs(sectionName));
            }

            return result;
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString, int version)
        {
            bool result = SaveConfigSection(sectionString, sectionName, version);
            if (result)
            {
                ConfigurationUpdated?.Invoke(this, new ObjectEventArgs(sectionName));
            }

            return result;
        }

        public void DeleteAllSections(int version)
        {
            var files = _storage.GetFileNames();
            foreach (var file in files)
            {
                if (file.StartsWith("mako-") && file.EndsWith($"-v{version}.cfg"))
                    _storage.DeleteFile(file);
            }
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString, Type objectType)
        {
            try
            {
                JsonConvert.DeserializeObject(sectionString, objectType);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in config section {sectionName} - config not updated", e);
                return false;
            }
            if (SaveConfigSection(sectionString, sectionName, GetVersion()))
            {
                ConfigurationUpdated?.Invoke(this, new ObjectEventArgs(sectionName));
            }
            return true;
        }

        public void WriteDefault(string sectionName, object section, bool overwrite = false)
        {
            int version = GetVersion();
            if (overwrite || !_storage.FileExists(GetConfigFileName(sectionName, version)))
                UpdateConfigSection(sectionName, section);
        }

        public string[] GetSections()
        {
            var sections = new ArrayList();
            var files = _storage.GetFileNames();
            foreach (var file in files)
            {
                if (IsConfigFile(file))
                    sections.Add(file.Substring(5, file.Length - 9));
            }

            return (string[])sections.ToArray(typeof(string));
        }

        public string LoadConfigSection(string sectionName)
        {
            string fileName = GetConfigFileName(sectionName, GetVersion());
            if (_storage.FileExists(fileName))
            {
                try
                {
                    _configString = _storage.ReadFile(fileName);
                    return _configString;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading config from file", e);
                }
            }
            return null;
        }

        public bool ClearAll()
        {
            var result = true;
            var files = _storage.GetFileNames();
            foreach (var file in files)
            {
                if (IsConfigFile(file))
                {
                    _logger.LogTrace($"Deleting file {file}...");
                    try
                    {
                        _storage.DeleteFile(file);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error deleting config file {file}", e);
                        result = false;
                    }
                }
            }

            return result;
        }

        private bool SaveConfigSection(string config, string sectionName, int version)
        {
            string fileName = GetConfigFileName(sectionName, version);
            try
            {
                lock (_writeLock)
                {
                    _storage.WriteToFile(fileName, config);
                    _logger.LogDebug($"Config section {sectionName} updated.");
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving config to file", e);
            }

            return false;
        }

        private static string GetConfigFileName(string sectionName, int version) => $"mako-{sectionName}-v{version}.cfg".ToLower();

        private static bool IsConfigFile(string name) => name.StartsWith("mako-") && name.EndsWith(".cfg");

        public int GetVersion()
        {
            return !_storage.FileExists(ConfigVersionFileName) ? 0 : int.Parse(_storage.ReadFile(ConfigVersionFileName));
        }

        public void UpdateVersion(int version)
        {
            lock (_writeLock)
            {
                if (_storage.FileExists(ConfigVersionFileName))
                    _storage.DeleteFile(ConfigVersionFileName);

                _storage.WriteToFile(ConfigVersionFileName, version.ToString());
            }
        }
    }

    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message)
        {

        }
    }
}
