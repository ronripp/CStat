using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class HueColor
    {
        public double x;
        public double y;
        public double brightness;
    }

    public class HueMgr // Philips Hue Manager
    {

        public HueColor RGBtoHueColor(int r8, int g8, int b8)
        {
            // normalize 8 bit 0-255 to 1.0
            double red = r8 / 255;
            double green = g8 / 255;
            double blue = b8 / 255;

            // Perform Gamma Correction
            red = (red > 0.04045) ? Math.Pow((red + 0.055) / (1.0 + 0.055), 2.4) : (red / 12.92);
            green = (green > 0.04045) ? Math.Pow((green + 0.055) / (1.0 + 0.055), 2.4) : (green / 12.92);
            blue = (blue > 0.04045) ? Math.Pow((blue + 0.055) / (1.0 + 0.055), 2.4) : (blue / 12.92);

            // Convert the RGB values to XYZ using the Wide RGB D65 conversion formula The formulas used
            double X = red * 0.4124 + green * 0.3576 + blue * 0.1805;
            double Y = red * 0.2126 + green * 0.7152 + blue * 0.0722;
            double Z = red * 0.0193 + green * 0.1192 + blue * 0.9505;

            // Calculate x, y, and brightness
            double x = X / (X + Y + Z);
            double y = Y / (X + Y + Z);
            double brightness = Y;

            // Check to see if x, y is in the assumed Triangular Gamut C (newer Philip light bulbs)
            // Red Point = 0.6915, 0.3038
            // Green Point = 0.17, 0.7
            // Blue = 0.1532, 0.0475

            // If out, find closest point to triangle edge and use this x,y else choose closes point (r,g or b) for xy

        }

    }
}
