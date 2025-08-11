using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AstroTBotService
{
    public class ResourcesLocaleManager : IResourcesLocaleManager
    {
        private readonly ResourceManager _resourceManager;
        private readonly CultureInfo _defaultCultureInfo;

        public ResourcesLocaleManager()
        {
            _resourceManager = new ResourceManager("AstroTBotService.Resources.Resources", Assembly.GetExecutingAssembly());
            _defaultCultureInfo = new CultureInfo("en");
        }

        public string GetString(string name, CultureInfo cultureInfo)
        {
            return _resourceManager.GetString(name, cultureInfo)
                ?? _resourceManager.GetString(name, _defaultCultureInfo)
                ?? name;
        }

        public bool TryGetString(string name, CultureInfo cultureInfo, out string str)
        {
            str = _resourceManager.GetString(name, cultureInfo);

            return !string.IsNullOrWhiteSpace(str);
        }
    }
}
