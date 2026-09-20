using System;

namespace MOD_Rk7Qp2
{
    // Pure calculation: no game/IL2CPP dependencies. Only call for current
    // player/spouse or player/partner writes after the relationship filter.
    public static class AffinityCapPolicy
    {
        public const float Maximum = 300f;

        public static float Apply(float current, float requested)
        {
            // Do not reinterpret exceptional game input; the caller logs errors.
            if (float.IsNaN(current) || float.IsInfinity(current)
                || float.IsNaN(requested) || float.IsInfinity(requested))
                return requested;

            // An existing over-cap value must be allowed to normalize to 300.
            // Otherwise the exact (possibly fractional) current value is the
            // minimum, protecting against a genuine affinity decrease.
            float protectedMinimum = Math.Min(current, Maximum);
            if (requested < protectedMinimum)
                return protectedMinimum;
            if (requested > Maximum)
                return Maximum;
            return requested;
        }
    }
}
