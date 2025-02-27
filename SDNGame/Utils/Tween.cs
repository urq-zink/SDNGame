namespace SDNGame.Utils
{
    public static class Tween
    {
        public static float Linear(float t) => t;

        public static float EaseInQuad(float t) => t * t;
        public static float EaseOutQuad(float t) => t * (2 - t);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseOutCubic(float t) => 1 - EaseInCubic(1 - t);
        public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - EaseInCubic(1 - 2 * t) / 2;

        public static float EaseInQuart(float t) => t * t * t * t;
        public static float EaseOutQuart(float t) => 1 - EaseInQuart(1 - t);
        public static float EaseInOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - EaseInQuart(1 - 2 * t) / 2;

        public static float EaseInSine(float t) => 1 - MathF.Cos(t * MathF.PI / 2);
        public static float EaseOutSine(float t) => MathF.Sin(t * MathF.PI / 2);
        public static float EaseInOutSine(float t) => -(MathF.Cos(MathF.PI * t) - 1) / 2;

        public static float EaseOutBounce(float t)
        {
            if (t < 1 / 2.75f) return 7.5625f * t * t;
            if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }

        public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);
        public static float EaseInOutBounce(float t) => t < 0.5f
            ? EaseInBounce(t * 2) * 0.5f
            : EaseOutBounce(t * 2 - 1) * 0.5f + 0.5f;

        public static float EaseOutElastic(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return MathF.Pow(2, -10 * t) * MathF.Sin((t - 0.075f) * (2 * MathF.PI) / 0.3f) + 1;
        }

        public static float EaseInElastic(float t) => 1 - EaseOutElastic(1 - t);
        public static float EaseInOutElastic(float t) => t < 0.5f
            ? EaseInElastic(t * 2) * 0.5f
            : EaseOutElastic(t * 2 - 1) * 0.5f + 0.5f;

        public static float EaseInExpo(float t) => t == 0 ? 0 : MathF.Pow(2, 10 * (t - 1));
        public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - MathF.Pow(2, -10 * t);
        public static float EaseInOutExpo(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return t < 0.5f
                ? MathF.Pow(2, 20 * t - 10) / 2
                : (2 - MathF.Pow(2, -20 * t + 10)) / 2;
        }

        public static float EaseInBack(float t)
        {
            const float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }

        public static float EaseOutBack(float t)
        {
            const float s = 1.70158f;
            t--;
            return t * t * ((s + 1) * t + s) + 1;
        }

        public static float EaseInOutBack(float t)
        {
            const float s = 1.70158f * 1.525f;
            t *= 2;
            if (t < 1) return 0.5f * (t * t * ((s + 1) * t - s));
            t -= 2;
            return 0.5f * (t * t * ((s + 1) * t + s) + 2);
        }

        public static float EaseInCirc(float t) => 1 - MathF.Sqrt(1 - t * t);
        public static float EaseOutCirc(float t) => MathF.Sqrt(1 - (t - 1) * (t - 1));
        public static float EaseInOutCirc(float t)
        {
            t *= 2;
            if (t < 1) return -0.5f * (MathF.Sqrt(1 - t * t) - 1);
            t -= 2;
            return 0.5f * (MathF.Sqrt(1 - t * t) + 1);
        }

        public static float Lerp(float start, float end, float t) => start + (end - start) * t;
    }
}