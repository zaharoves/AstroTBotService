using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AstroTBotService
{
    public class ResourcesLocaleManager : IResourcesLocaleManager
    {
        private readonly ResourceManager _resourceManager;

        public ResourcesLocaleManager()
        {
            _resourceManager = new ResourceManager("AstroTBotService.Resources.Resources", Assembly.GetExecutingAssembly());
        }

        public string GetString(string name, string languageCode)
        {
            // Создаем CultureInfo из languageCode
            var cultureInfo = new CultureInfo(languageCode);

            // Получаем строку для конкретной культуры
            return _resourceManager.GetString(name, cultureInfo) ??
                   _resourceManager.GetString(name, new CultureInfo("en-US")) ?? // Возвращаем английский по умолчанию
                   name; // Если ничего не найдено, возвращаем имя ключа
        }

        public string GetString(string name, CultureInfo cultureInfo)
        {
            // Получаем строку для конкретной культуры
            return _resourceManager.GetString(name, cultureInfo) ??
                   _resourceManager.GetString(name, new CultureInfo("en-US")) ?? // Возвращаем английский по умолчанию
                   name; // Если ничего не найдено, возвращаем имя ключа
        }

        public bool TryGetString(string name, CultureInfo cultureInfo, out string str)
        {
            str = _resourceManager.GetString(name, cultureInfo);

            return !string.IsNullOrEmpty(str);
        }
    }
}
