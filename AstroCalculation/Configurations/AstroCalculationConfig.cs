
namespace AstroCalculation.Configurations
{
    public class AstroCalculationConfig
    {
        public const string ConfigKey = "Astro";

        public string SwissEphPath { get; set; }
        public Orbs NatalOrbs { get; set; } = new Orbs();
        public Orbs TransitOrbs { get; set; } = new Orbs();
        public Orbs DirectionOrbs { get; set; } = new Orbs();
    }
}
