using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

 //Pretty much took this from the internetz.. well known open java script source and modified to suit C#

namespace Tweens
{
    class Quadratic
    {
        public static float In(float t)
        {
            return t * t;
        }
        public static float Out(float t)
        {
            return t * (2 - t);
        }
        public static float InOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t;
            }

            return -0.5f * (--t * (t - 2) - 1);
        }
    }
    class Cubic
    {
        public static float In(float t)
        {
            return t * t * t;
        }
        public static float Out(float t)
        {
            return --t * t * t + 1;
        }
        public static float InOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t * t;
            }

            return 0.5f * ((t -= 2) * t * t + 2);
        }
    }
    class Elastic
    {
        public static float In(float t)
        {
            if (t <= 0)
            {
                return 0;
            }

            if (t >= 1)
            {
                return 1;
            }
            return -Mathf.Pow(2.0f, 10.0f * (t - 1)) * Mathf.Sin((t - 1.1f) * 5.0f * Mathf.PI);
        }
        public static float Out(float t)
        {
            if (t <= 0)
            {
                return 0;
            }

            if (t >= 1)
            {
                return 1;
            }
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.1f) * 5 * Mathf.PI) + 1;
        }
        public static float InOut(float t)
        {
            if (t <= 0)
            {
                return 0;
            }

            if (t >= 1)
            {
                return 1;
            }
            t *= 2;

            if (t < 1)
            {
                return -0.5f * Mathf.Pow(2, 10 * (t - 1)) * Mathf.Sin((t - 1.1f) * 5 * Mathf.PI);
            }

            return 0.5f * Mathf.Pow(2, -10 * (t - 1)) * Mathf.Sin((t - 1.1f) * 5 * Mathf.PI) + 1;
        }
    }

    class Back
    {
        public static float In(float t)
        {
            const float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float Out(float t)
        {
            const float s = 1.70158f;
            return --t * t * ((s + 1) * t + s) + 1;
        }
        public static float InOut(float t)
        {
            const float s = 1.70158f * 1.525f;

            if ((t *= 2) < 1)
            {
                return 0.5f * (t * t * ((s + 1) * t - s));
            }

            return 0.5f * ((t -= 2) * t * ((s + 1) * t + s) + 2);
        }
    }
    class Bounce
    {
        public static float In(float t)
        {
            return 1.0f - Out(1.0f - t);
        }
        public static float Out(float t)
        {
            if (t < (1 / 2.75f))
            {
                return 7.5625f * t * t;
            }
            else if (t < (2 / 2.75f))
            {
                return 7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f;
            }
            else if (t < (2.5f / 2.75f))
            {
                return 7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f;
            }
            else
            {
                return 7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f;
            }
        }
        public static float InOut(float t)
        {
            if (t < 0.5f)
            {
                return In(t * 2) * 0.5f;
            }

            return Out(t * 2 - 1) * 0.5f + 0.5f;
        }
    }
}
