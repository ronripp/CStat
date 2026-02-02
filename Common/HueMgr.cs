using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class HueColor
    {
        public HueColor(double xv, double yv, double bv)
        {
            x = xv;
            y = yv;
            brightness = bv;
        }
        public double x;
        public double y;
        public double brightness;
    }

    public class DPoint
    {
        public DPoint(double xv, double yv)
        {
            x = xv;
            y = yv;
        }
        public double x;
        public double y;
    }

    public class HueMgr // Philips Hue Manager
    {
        private DPoint rPnt;
        private DPoint gPnt;
        private DPoint bPnt;

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;

        public HueMgr()
        {
            DPoint rPnt = new DPoint(0.6915, 0.3038);
            DPoint gPnt = new DPoint(0.17, 0.7);
            DPoint bPnt = new DPoint(0.1532, 0.0475);
            double minX = bPnt.x;
            double maxX = rPnt.x;
            double minY = bPnt.y;
            double maxY = gPnt.y;
        }

        public HueColor RGBtoHueColor(int r8, int g8, int b8)
        {
            // normalize 8 bit 0-255 to 1.0
            double red = ((double)r8) / 255;
            double green = ((double)g8) / 255;
            double blue = ((double)b8) / 255;

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
            var pnt = new DPoint(x, y);

            // Test : Remove
            bool in1 = isInGamutC(new DPoint(0.3, 0.4));
            bool in2 = isInGamutC(new DPoint(0.35, 0.5));
            bool out1 = isInGamutC(new DPoint(0.1, 0.2));
            bool out2 = isInGamutC(new DPoint(0.5, 0.2));
            bool out3 = isInGamutC(new DPoint(0.5, 0.5));

            if (isInGamutC(new DPoint(x, y)))
                return new HueColor(x, y, brightness); // In GamutC

            // Not in Gamut C. Find closest point to triangle edge and use this x,y else choose closes point (r,g or b) for xy

            return new HueColor(0, 0, 0);
        }
        public static double GetNormal(DPoint C, DPoint A, DPoint B)
        {
            return ((A.x - C.x) * (B.y - C.y)) - ((A.y - C.y) * (B.x - C.x));
        }

        public bool isInGamutC(DPoint p)
        {
            TBD //return hint on returning false

            // First see if it is in the Triangular bound before testing cross Product normal sign to filter out some cases.
            if ((p.x < minX) || (p.x > maxX) || (p.y < minY) || (p.y > maxY))
                return false;

            double rgN = GetNormal(rPnt, gPnt, p);
            double gbN = GetNormal(gPnt, bPnt, p);
            double brN = GetNormal(bPnt, rPnt, p);
            return rgN >= 0 && gbN >= 0 && brN >= 0;
        }
    }
}
