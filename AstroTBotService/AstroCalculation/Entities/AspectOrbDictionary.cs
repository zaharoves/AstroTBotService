using AstroTBotService.Enums;
using System.Collections.ObjectModel;

namespace AstroTBotService.AstroCalculation.Entities
{
    public class AspectOrbDictionary : ReadOnlyDictionary<AspectEnum, (double Min, double Max)>
    {
        public AspectOrbDictionary(double orb) 
            : base(new Dictionary<AspectEnum, (double Min, double Max)>
            {
                { AspectEnum.Conjunction, (Constants.CIRCLE_ANGLES - orb, Constants.CONJUNCTION + orb) },
                { AspectEnum.Sextile, (Constants.SEXTILE - orb, Constants.SEXTILE + orb) },
                { AspectEnum.Square, (Constants.SQUARE - orb, Constants.SQUARE + orb) },
                { AspectEnum.Trine, (Constants.TRINE - orb, Constants.TRINE + orb) },
                { AspectEnum.Opposition, (Constants.OPPOSITION - orb, Constants.OPPOSITION + orb) },
            })
        {
        }
    }
}
