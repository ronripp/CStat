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
        private enum GState { Unknown=0, R=0x1, G=0x2, B=0x4, RG=0x8, GB=0x10, BR=0x20, OUT=0x1000, IN=0x2000 }
        private readonly double delta = 1E-6;
        private readonly DPoint rPnt;
        private readonly DPoint gPnt;
        private readonly DPoint bPnt;

        private readonly double minX;
        private readonly double maxX;
        private readonly double minY;
        private readonly double maxY;

        public HueMgr()
        {
            // Using Gamut C
            rPnt = new DPoint(0.6915, 0.3038);
            gPnt = new DPoint(0.17, 0.7);
            bPnt = new DPoint(0.1532, 0.0475);
            minX = bPnt.x;
            maxX = rPnt.x;
            minY = bPnt.y;
            maxY = gPnt.y;
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
            DPoint p1 = new DPoint(0.3, 0.4);
            DPoint p2 = new DPoint(0.35, 0.5);
            DPoint p3 = new DPoint(0.1, 0.2);
            DPoint p4 = new DPoint(0.5, 0.2);
            DPoint p5 = new DPoint(0.5, 0.5);
            GState ps1 = GamutState(new DPoint(0.3, 0.4));
            GState ps2 = GamutState(new DPoint(0.35, 0.5));
            GState ps3 = GamutState(new DPoint(0.1, 0.2));
            GState ps4 = GamutState(new DPoint(0.5, 0.2));
            GState ps5 = GamutState(new DPoint(0.5, 0.5));

            if (AdjustXY(ref pnt))
                return new HueColor(x, y, brightness); // In GamutC
            else
                return null;
        }
        private double GetNormal(DPoint C, DPoint A, DPoint B)
        {
            return ((A.x - C.x) * (B.y - C.y)) - ((A.y - C.y) * (B.x - C.x));
        }

        private GState GamutState(DPoint p)
        {
            double rgN = GetNormal(rPnt, gPnt, p);
            double gbN = GetNormal(gPnt, bPnt, p);
            double brN = GetNormal(bPnt, rPnt, p);

            bool rgOut = rgN < 0;
            bool gbOut = gbN < 0; 
            bool brOut = brN < 0;
            if (!rgOut && !gbOut && !brOut)
            {
                // Extra check but not needed.
                if (InGamut(p.x, p.y))
                    return GState.IN;
                else
                    return GState.OUT; // should NEVER occur
            }

            if (rgOut && gbOut && brOut)
                return GState.OUT; // Should NEVER occur

            if (rgOut && gbOut)
                return GState.G;

            if (gbOut && brOut)
                return GState.B;

            if (brOut && rgOut)
                return GState.R;

            if (rgOut)
                return GState.RG;

            if (gbOut)
                return GState.GB;

            if (brOut)
                return GState.BR;

            return GState.Unknown; // Should NEVER occur
        }

        private bool InGamut(double x, double y)
        {
            return (x >= (minX - delta)) && (x <= (maxX + delta)) && (y >= (minY - delta)) && (y <= (maxY + delta));
        }

        private bool GetEdgeXY(GState edge, ref DPoint pnt)
        {
            DPoint p1, p2;
            switch (edge)
            {
                case GState.RG:
                    p1 = new DPoint(rPnt.x, rPnt.y);
                    p2 = new DPoint(gPnt.x, gPnt.y);
                    break;
                case GState.GB:
                    p1 = new DPoint(gPnt.x, gPnt.y);
                    p2 = new DPoint(bPnt.x, bPnt.y);
                    break;
                case GState.BR:
                    p1 = new DPoint(bPnt.x, bPnt.y);
                    p2 = new DPoint(rPnt.x, rPnt.y);
                    break;
                default:
                    return false;
            }

            double me = (p2.y - p1.y) / (p2.x - p1.x);
            double be = p1.y - (me * p1.x);
            double mp = - 1/me;
            double bp = pnt.y - (mp * pnt.x);
            double xi = (bp - be) / (me - mp);
            double yi = me * xi + be;

            if (InGamut(xi, yi)) TBD: Limit to line min max (not Gamut min max)
            {
                pnt.x = xi;
                pnt.y = yi;
            }

            double sd1 = (xi - p1.x) * (xi - p1.x) + (yi - p1.y) * (yi - p1.y);
            double sd2 = (xi - p2.x) * (xi - p2.x) + (yi - p2.y) * (yi - p2.y);

            switch (edge)
            {
                case GState.RG:
                    pnt = (sd1 < sd2) ? new DPoint(rPnt.x, rPnt.y) : new DPoint(gPnt.x, gPnt.y);
                    return true;
                case GState.GB:
                    pnt = (sd1 < sd2) ? new DPoint(gPnt.x, gPnt.y) : new DPoint(bPnt.x, bPnt.y);
                    return true;
                case GState.BR:
                    pnt = (sd1 < sd2) ? new DPoint(bPnt.x, bPnt.y) : new DPoint(rPnt.x, rPnt.y);
                    return true;
                default:
                    return false;
            }
        }

        private bool AdjustXY(ref DPoint pnt)
        {
            GState gs = GamutState(pnt);
            switch (gs)
            {
                case GState.IN:
                    return true;
                case GState.R:
                    pnt = new DPoint(rPnt.x, rPnt.y);
                    return true;
                case GState.G:
                    pnt = new DPoint(gPnt.x, gPnt.y);
                    return true;
                case GState.B:
                    pnt = new DPoint(bPnt.x, bPnt.y);
                    return true;
                case GState.RG:
                case GState.GB:
                case GState.BR:
                    return GetEdgeXY(gs, ref pnt);
                default:
                case GState.OUT:
                case GState.Unknown:
                    return false;
            }
        }
    }
}
