using AstroTBotService.Entities;

namespace AstroTBotService.TBot
{
    public interface ILocationPicker
    {
        bool TryParseLocation(string str, out double longitude, out double latitude);

        bool TryParseLongitude(string str, out double longitude);
        
        bool TryParseLatitude(string str, out double latitude);


        Task SendLocation(TBotClientData clientData, string text);

        Task SendConfirmCoordinates(TBotClientData clientData, string text);
    }
}
