
namespace AstroTBotService.Entities
{
    public class DatePickerData
    {
        public DateTime? DateTime { get; set; }

        public TimeSpan GmtOffset { get; set; }

        public int MinYearInterval { get; set; }

        public bool IsSaveCommand { get; set; }

        public bool IsCancelCommand { get; set; }

        public bool IsChangeCommand { get; set; }
    }
}
