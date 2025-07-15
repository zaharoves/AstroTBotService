using System.Globalization;

namespace AstroTBotService
{
    public interface IResourcesLocaleManager
    {
        string GetString(string name, string languageCode);

        string GetString(string name, CultureInfo cultureInfo);

        bool TryGetString(string name, CultureInfo cultureInfo, out string str);
    }
}
