using AstroCalculation.Enums;
using System.Collections.ObjectModel;

namespace AstroCalculation.Entities
{
    public class AspectOrbDictionary : ReadOnlyDictionary<AspectEnum, (double Min, double Max)>
    {
        public AspectOrbDictionary(double orb) 
            : base(new Dictionary<AspectEnum, (double Min, double Max)>
            {
                { AspectEnum.Conjunction, (Constants.Astro.CIRCLE_ANGLES - orb, Constants.Astro.Aspects.CONJUNCTION + orb) },
                { AspectEnum.Sextile, (Constants.Astro.Aspects.SEXTILE - orb, Constants.Astro.Aspects.SEXTILE + orb) },
                { AspectEnum.Square, (Constants.Astro.Aspects.SQUARE - orb, Constants.Astro.Aspects.SQUARE + orb) },
                { AspectEnum.Trine, (Constants.Astro.Aspects.TRINE - orb, Constants.Astro.Aspects.TRINE + orb) },
                { AspectEnum.Opposition, (Constants.Astro.Aspects.OPPOSITION - orb, Constants.Astro.Aspects.OPPOSITION + orb) },
            })
        {
        }
    }
}
