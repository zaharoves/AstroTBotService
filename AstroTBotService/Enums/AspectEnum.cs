namespace AstroTBotService.Enums
{
    public enum AspectEnum
    {
        /// <summary>
        /// No aspects
        /// </summary>
        None,
        /// <summary>
        /// Corner between planets = 0 degrees
        /// </summary>
        Conjunction,
        /// <summary>
        /// Corner between planets = 60 degrees
        /// </summary>
        Sextile,
        /// <summary>
        /// Corner between planets = 90 degrees
        /// </summary>
        Square,
        /// <summary>
        /// Corner between planets = 120 degrees
        /// </summary>
        Trine,
        /// <summary>
        /// Corner between planets = 180 degrees
        /// </summary>
        Opposition
    }
}
