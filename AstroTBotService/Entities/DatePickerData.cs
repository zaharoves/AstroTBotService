
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

        public override string ToString()
        {
            if (!DateTime.HasValue)
            {
                return string.Empty;
            }

            var gmtSign = GmtOffset >= TimeSpan.Zero ? "+" : "-";

            return $"{DateTime.Value.ToString("d MMMM yyyyг. HH:mm")} [GMT{gmtSign}{Math.Abs(GmtOffset.Hours)}]";
        }
    }
}
