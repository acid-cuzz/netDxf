using System;
using System.Collections.Generic;
using System.Linq;
using netDxf;

namespace DowncomerRetrofit
{
    /// <summary>
    /// Design dimensions (mm). Site dimensions are the assumed values from the drawing notes.
    /// </summary>
    public static class Spec
    {
        // existing downcomer and trough (assumed, confirm on site)
        public const double StandpipeOd = 19.05;
        public const double StandpipeWall = 1.65;
        public const double FloorThickness = 6.35;
        public const double Pitch = 40.0;
        public const double HoleDia = 25.0;
        public const double WeirSlotWidth = 3.0;
        public const double WeirSlotBottom = 19.0; // above the seat, i.e. 20 above the floor top as drawn

        // assembly stack, z measured from the seat (flange underside) upwards
        public const double GasketInstalled = 1.0; // 1.5 sheet, shown compressed
        public const double FloorTop = -GasketInstalled;
        public const double FloorBottom = FloorTop - FloorThickness;

        // item 1 insert
        public const double FlangeDia = 40.0;
        public const double FlangeThickness = 9.0;
        public const double FlangeTopChamfer = 1.0;
        public const double FlangeUnderChamfer = 0.5;
        public const double CounterboreDia = 22.0;
        public const double MouthChamfer = 0.5;
        public const double MouthChamferAngle = 30.0;
        public const double BoreDia = 12.0;
        public const double ThreadMajor = 24.0;
        public const double ThreadMinor = 21.546; // M24 x 2 - 6g external minor
        public const double ThreadLength = 34.0;
        public const double ThreadIncomplete = 3.0;
        public const double CollarDia = 21.5;
        public const double CollarLength = 8.0;
        public const double SpigotDia = 16.2;
        public const double SpigotLength = 24.0;
        public const double SpigotChamfer = 0.8;
        public const double BarbDia = 17.0;
        public const double BarbRamp = 4.0;
        public static readonly double[] BarbsFromCollar = { 6.0, 16.0 };
        public const double InsertLength = FlangeThickness + ThreadLength + CollarLength + SpigotLength; // 75

        // item 2 nut
        public const double NutDia = 40.0;
        public const double NutThickness = 10.0;
        public const double NutMinor = 21.84;
        public const double NutChamfer = 1.0;

        // item 4 o-ring
        public const double OringId = 18.5;
        public const double OringCs = 1.78;

        // item 5 retainer
        public const double RetainerFlangeDia = 28.0;
        public const double RetainerFlangeThickness = 2.0;
        public const double RetainerSpigotDia = 21.8;
        public const double RetainerBore = 19.3;
        public const double RetainerSpigotLength = 6.6;
        public const double RetainerLength = RetainerFlangeThickness + RetainerSpigotLength; // 8.6

        // item 6 leg and item 7 band
        public const double TubeOd = 19.05;
        public const double TubeWall = 1.57;
        public const double BandLength = 40.0;
        public const double BandWall = 0.5;
        public const double BandTop = -(ThreadLength + 0.5);

        // derived levels
        public const double CollarTop = -ThreadLength;
        public const double CollarBottom = CollarTop - CollarLength;
        public const double SpigotEnd = CollarBottom - SpigotLength;
        public const double StandpipeRi = StandpipeOd / 2 - StandpipeWall;
        public const double StandpipeRo = StandpipeOd / 2;
    }

    /// <summary>
    /// Half-section profiles of the parts as (r, z) points: r is the radius from the downcomer axis,
    /// z the height above the seat (the flange underside, 1.0 above the floor top).
    /// </summary>
    public static class Parts
    {
        public static List<Vector2> Insert()
        {
            double mouth = Spec.CounterboreDia / 2 + Spec.MouthChamfer * Math.Tan(Spec.MouthChamferAngle * Math.PI / 180);
            double top = Spec.FlangeThickness;
            double rf = Spec.FlangeDia / 2;
            double rt = Spec.ThreadMajor / 2;
            double rc = Spec.CollarDia / 2;
            double rs = Spec.SpigotDia / 2;
            double rb = Spec.BarbDia / 2;

            List<Vector2> p = new List<Vector2>
            {
                new Vector2(mouth, top),
                new Vector2(rf - Spec.FlangeTopChamfer, top),
                new Vector2(rf, top - Spec.FlangeTopChamfer),
                new Vector2(rf, Spec.FlangeUnderChamfer),
                new Vector2(rf - Spec.FlangeUnderChamfer, 0),
                new Vector2(rt, 0),
                new Vector2(rt, Spec.CollarTop + (rt - rc)),
                new Vector2(rc, Spec.CollarTop),
                new Vector2(rc, Spec.CollarBottom),
                new Vector2(rs, Spec.CollarBottom)
            };
            foreach (double b in Spec.BarbsFromCollar)
            {
                double zb = Spec.CollarBottom - b;
                p.Add(new Vector2(rs, zb));
                p.Add(new Vector2(rb, zb));
                p.Add(new Vector2(rs, zb - Spec.BarbRamp));
            }
            p.Add(new Vector2(rs, Spec.SpigotEnd + Spec.SpigotChamfer));
            p.Add(new Vector2(rs - Spec.SpigotChamfer, Spec.SpigotEnd));
            p.Add(new Vector2(Spec.BoreDia / 2, Spec.SpigotEnd));
            p.Add(new Vector2(Spec.BoreDia / 2, 0));
            p.Add(new Vector2(Spec.CounterboreDia / 2, 0));
            p.Add(new Vector2(Spec.CounterboreDia / 2, top - Spec.MouthChamfer));
            return p;
        }

        /// <summary>
        /// Thread minor diameter line of the insert (thin line), from the incomplete thread to the chamfer.
        /// </summary>
        public static Vector2[] InsertThreadLine()
        {
            double r = Spec.ThreadMinor / 2;
            double rt = Spec.ThreadMajor / 2;
            double rc = Spec.CollarDia / 2;
            return new[] { new Vector2(r, -Spec.ThreadIncomplete), new Vector2(r, Spec.CollarTop + (rt - rc)) };
        }

        /// <summary>
        /// Nut section with its underside at z = 0. In the assembly the bolt thread hides the nut thread,
        /// so the nut is drawn to the major diameter; on its own it is drawn to the minor with a chamfered thread.
        /// </summary>
        public static List<Vector2> Nut(bool assembled)
        {
            double rf = Spec.NutDia / 2;
            double c = Spec.NutChamfer;
            double t = Spec.NutThickness;
            double rMaj = Spec.ThreadMajor / 2;
            List<Vector2> p = new List<Vector2>
            {
                new Vector2(rMaj, 0),
                new Vector2(rf - c, 0),
                new Vector2(rf, c),
                new Vector2(rf, t - c),
                new Vector2(rf - c, t),
                new Vector2(rMaj, t)
            };
            if (!assembled)
            {
                double rMin = Spec.NutMinor / 2;
                double tc = rMaj - rMin;
                p.Add(new Vector2(rMin, t - tc));
                p.Add(new Vector2(rMin, tc));
            }
            return p;
        }

        public static Vector2[] NutThreadLine()
        {
            double rMaj = Spec.ThreadMajor / 2;
            double tc = rMaj - Spec.NutMinor / 2;
            return new[] { new Vector2(rMaj, tc), new Vector2(rMaj, Spec.NutThickness - tc) };
        }

        /// <summary>
        /// Seal retainer with the spigot end at z = 0.
        /// </summary>
        public static List<Vector2> Retainer()
        {
            double ri = Spec.RetainerBore / 2;
            double rs = Spec.RetainerSpigotDia / 2;
            double rf = Spec.RetainerFlangeDia / 2;
            double l = Spec.RetainerSpigotLength;
            double h = Spec.RetainerLength;
            const double c = 0.4;
            const double cs = 0.3;
            return new List<Vector2>
            {
                new Vector2(ri, 0),
                new Vector2(rs - cs, 0),
                new Vector2(rs, cs),
                new Vector2(rs, l),
                new Vector2(rf, l),
                new Vector2(rf, h - c),
                new Vector2(rf - c, h),
                new Vector2(ri + c, h),
                new Vector2(ri, h - c)
            };
        }

        public static List<Vector2> Rect(double r0, double z0, double r1, double z1)
        {
            return new List<Vector2> { new Vector2(r0, z0), new Vector2(r1, z0), new Vector2(r1, z1), new Vector2(r0, z1) };
        }

        /// <summary>
        /// O-ring squeezed in the gland between the standpipe and the counterbore, resting on the seat.
        /// Returns centre and semi-axes (radial, axial); the section area is conserved.
        /// </summary>
        public static (Vector2 Centre, double A, double B) Oring()
        {
            double a = (Spec.CounterboreDia - Spec.StandpipeOd) / 4;
            double area = Math.PI * Spec.OringCs * Spec.OringCs / 4;
            double b = area / (Math.PI * a);
            return (new Vector2(Spec.StandpipeRo + a, b), a, b);
        }

        /// <summary>
        /// Bore of the PFA leg pushed over the spigot and barbs up to the collar shoulder.
        /// </summary>
        public static List<Vector2> TubeInner(double zEnd)
        {
            double rs = Spec.SpigotDia / 2;
            double rb = Spec.BarbDia / 2;
            double ri = Spec.TubeOd / 2 - Spec.TubeWall;
            const double bridge = 0.6;
            List<Vector2> p = new List<Vector2> { new Vector2(rs, Spec.CollarBottom) };
            foreach (double b in Spec.BarbsFromCollar)
            {
                double zb = Spec.CollarBottom - b;
                p.Add(new Vector2(rs, zb + bridge));
                p.Add(new Vector2(rb, zb));
                p.Add(new Vector2(rs, zb - Spec.BarbRamp));
            }
            p.Add(new Vector2(rs, Spec.SpigotEnd + Spec.SpigotChamfer));
            p.Add(new Vector2(ri, Spec.SpigotEnd - 3.0));
            p.Add(new Vector2(ri, zEnd));
            return p;
        }

        /// <summary>
        /// The tube wall thins as it is stretched; constant r x t is near enough for a drawing.
        /// </summary>
        public static List<Vector2> TubeOuter(List<Vector2> inner)
        {
            double ri = Spec.TubeOd / 2 - Spec.TubeWall;
            return inner.Select(v => new Vector2(v.X + Spec.TubeWall * ri / v.X, v.Y)).ToList();
        }

        public static List<Vector2> Tube(double zEnd)
        {
            List<Vector2> inner = TubeInner(zEnd);
            List<Vector2> outer = TubeOuter(inner);
            outer.Reverse();
            return inner.Concat(outer).ToList();
        }

        /// <summary>
        /// Heat-shrink band: starts on the collar, bridges the shoulder and follows the leg.
        /// </summary>
        public static List<Vector2> Band()
        {
            double rc = Spec.CollarDia / 2;
            double zBottom = Spec.BandTop - Spec.BandLength;
            const double landing = 2.0;
            List<Vector2> outer = TubeOuter(TubeInner(zBottom - 10));

            List<Vector2> inner = new List<Vector2>
            {
                new Vector2(rc, Spec.BandTop),
                new Vector2(rc, Spec.CollarBottom)
            };
            double zLand = Spec.CollarBottom - landing;
            inner.Add(new Vector2(RadiusAt(outer, zLand), zLand));
            inner.AddRange(outer.Where(v => v.Y < zLand && v.Y > zBottom));
            inner.Add(new Vector2(RadiusAt(outer, zBottom), zBottom));

            List<Vector2> band = new List<Vector2>(inner);
            for (int i = inner.Count - 1; i >= 0; i--)
            {
                band.Add(new Vector2(inner[i].X + Spec.BandWall, inner[i].Y));
            }
            return band;
        }

        /// <summary>
        /// Radius of a profile (ordered by decreasing z) at height z.
        /// </summary>
        public static double RadiusAt(List<Vector2> profile, double z)
        {
            for (int i = 0; i < profile.Count - 1; i++)
            {
                Vector2 a = profile[i];
                Vector2 b = profile[i + 1];
                if (z <= a.Y && z >= b.Y)
                {
                    if (MathHelper.IsEqual(a.Y, b.Y))
                    {
                        return Math.Max(a.X, b.X);
                    }
                    return a.X + (b.X - a.X) * (z - a.Y) / (b.Y - a.Y);
                }
            }
            return profile[profile.Count - 1].X;
        }
    }
}
