using System.Collections.Generic;
using System.Linq;
using netDxf;
using netDxf.Entities;

namespace DowncomerRetrofit
{
    public enum StandpipeState
    {
        None,
        AsFound,
        Lifted,
        Installed
    }

    /// <summary>
    /// What a section through one downcomer position shows.
    /// </summary>
    public sealed class AssemblyOptions
    {
        public StandpipeState Standpipe = StandpipeState.Installed;
        public double StandpipeTop = 25.0;
        public double LiftedBy = 12.0;
        public bool Insert = true;
        public bool Leg = true;
        public double LegBottom = -87.35;
        public double HoleRadius = Spec.HoleDia / 2;
        public double FloorOuter = 30.0;
        public double BreakHalfWidth = 12.8;
        public bool BreakLines = true;
        public bool CentreLine = true;
        public double CentreTop = 28.0;
        public double CentreBottom = -89.5;
    }

    /// <summary>
    /// Sectioned assembly of one position, symmetric about the downcomer axis.
    /// </summary>
    public static class Assembly
    {
        private static List<Vector2> Shift(IEnumerable<Vector2> rz, double dz)
        {
            return rz.Select(v => new Vector2(v.X, v.Y + dz)).ToList();
        }

        public static double BreakAmplitude(View v)
        {
            return System.Math.Min(0.85, 0.4 + 0.25 * v.Scale);
        }

        public static void Draw(Sheet s, View v, AssemblyOptions o)
        {
            double gasketBottom = Spec.FloorBottom - Spec.GasketInstalled;
            double breakExt = 2.75;

            foreach (int side in new[] { -1, 1 })
            {
                // trough floor, broken at its outer end
                s.Section(v, Parts.Rect(o.HoleRadius, Spec.FloorBottom, o.FloorOuter, Spec.FloorTop), Material.Floor, side, false);
                s.Outline(v, v.Map(new[]
                {
                    new Vector2(o.FloorOuter, Spec.FloorTop), new Vector2(o.HoleRadius, Spec.FloorTop),
                    new Vector2(o.HoleRadius, Spec.FloorBottom), new Vector2(o.FloorOuter, Spec.FloorBottom)
                }, side), false, s.Visible, v.Outline);
                if (o.BreakLines)
                {
                    s.BreakLine(v.P(o.FloorOuter, Spec.FloorBottom - breakExt, side), v.P(o.FloorOuter, Spec.FloorTop + breakExt, side), BreakAmplitude(v));
                }

                if (o.Insert)
                {
                    double rg = Spec.FlangeDia / 2;
                    s.Section(v, Parts.Rect(Spec.HoleDia / 2, Spec.FloorTop, rg, 0), Material.Gasket, side, true, v.Outline);
                    s.Section(v, Parts.Rect(Spec.HoleDia / 2, gasketBottom, rg, Spec.FloorBottom), Material.Gasket, side, true, v.Outline);

                    double nut1 = gasketBottom - Spec.NutThickness;
                    double nut2 = nut1 - Spec.NutThickness;
                    s.Section(v, Shift(Parts.Nut(true), nut1), Material.ClampNut, side, true, v.Outline);
                    s.Section(v, Shift(Parts.Nut(true), nut2), Material.JamNut, side, true, v.Outline);

                    s.Section(v, Parts.Insert(), Material.Insert, side, true, v.Outline);
                    s.Outline(v, v.Map(Parts.InsertThreadLine(), side), false, s.Thin);

                    double gland = Spec.FlangeThickness - Spec.RetainerSpigotLength;
                    s.Section(v, Shift(Parts.Retainer(), gland), Material.Retainer, side, true, v.Outline);

                    (Vector2 c, double a, double b) = Parts.Oring();
                    Vector2 centre = v.P(c, side);
                    if (!v.ClipCentre.HasValue || Vector2.Distance(centre, v.ClipCentre.Value) + v.L(b) < v.ClipRadius)
                    {
                        Oring(s, centre, a * v.Scale, b * v.Scale);
                    }
                }

                DrawStandpipe(s, v, o, side);

                if (o.Leg)
                {
                    List<Vector2> tube = Parts.Tube(o.LegBottom);
                    s.Section(v, tube, Material.Tube, side, false);
                    // open at the break: inner wall, top end, outer wall
                    List<Vector2> inner = Parts.TubeInner(o.LegBottom);
                    List<Vector2> outer = Parts.TubeOuter(inner);
                    List<Vector2> path = Enumerable.Reverse(inner).Concat(outer).ToList();
                    s.Outline(v, v.Map(path, side), false, s.Visible, v.Outline);

                    s.Section(v, Parts.Band(), Material.Band, side, true, v.Outline);
                }
            }

            if (o.BreakLines && o.Standpipe != StandpipeState.None)
            {
                double top = o.Standpipe == StandpipeState.Lifted ? o.StandpipeTop + o.LiftedBy : o.StandpipeTop;
                s.BreakLine(v.P(o.BreakHalfWidth, top, -1), v.P(o.BreakHalfWidth, top, 1), BreakAmplitude(v));
            }
            if (o.BreakLines && o.Leg)
            {
                s.BreakLine(v.P(o.BreakHalfWidth + 0.5, o.LegBottom, -1), v.P(o.BreakHalfWidth + 0.5, o.LegBottom, 1), BreakAmplitude(v));
            }
            if (o.CentreLine)
            {
                s.CenterLine(v.P(0, o.CentreBottom), v.P(0, o.CentreTop));
            }
        }

        private static void DrawStandpipe(Sheet s, View v, AssemblyOptions o, int side)
        {
            double ri = Spec.StandpipeRi;
            double ro = Spec.StandpipeRo;
            double top = o.StandpipeTop;
            double bottom = 0.0;
            List<Vector2> wall;
            switch (o.Standpipe)
            {
                case StandpipeState.None:
                    return;
                case StandpipeState.Lifted:
                    bottom = o.LiftedBy;
                    top += o.LiftedBy;
                    wall = new List<Vector2> { new Vector2(ri, top), new Vector2(ri, bottom), new Vector2(ro, bottom), new Vector2(ro, top) };
                    break;
                case StandpipeState.AsFound:
                    // the old downcomer runs on through the floor; its leg is eaten away below the joint
                    wall = side > 0
                        ? new List<Vector2>
                        {
                            new Vector2(ri, top), new Vector2(ri, -18.4), new Vector2(8.67, -22.4), new Vector2(9.31, -17.4),
                            new Vector2(9.05, -13.4), new Vector2(ro, -9.5), new Vector2(ro, top)
                        }
                        : new List<Vector2>
                        {
                            new Vector2(ri, top), new Vector2(ri, -13.4), new Vector2(8.36, -16.4), new Vector2(9.21, -15.4),
                            new Vector2(8.6, -11.4), new Vector2(ro, -9.0), new Vector2(ro, top)
                        };
                    break;
                default:
                    wall = new List<Vector2> { new Vector2(ri, top), new Vector2(ri, bottom), new Vector2(ro, bottom), new Vector2(ro, top) };
                    break;
            }

            s.Section(v, wall, Material.Standpipe, side, false);
            s.Outline(v, v.Map(wall, side), false, s.Visible, v.Outline);

            if (o.Standpipe == StandpipeState.AsFound)
            {
                // corrosion marks beside the eaten leg
                double r = ro + 0.9;
                double z0 = side > 0 ? -10.0 : -9.0;
                double z1 = side > 0 ? -16.5 : -13.5;
                s.Poly(v.Map(new[] { new Vector2(r, z0), new Vector2(r + 0.35, (2 * z0 + z1) / 3), new Vector2(r - 0.25, (z0 + 2 * z1) / 3), new Vector2(r + 0.15, z1) }, side),
                    false, s.Thin, Sheet.Red);
            }

            // weir slot in the far wall, seen through the bore (drawn once)
            if (side > 0)
            {
                double slot = Spec.WeirSlotBottom + bottom;
                double w = Spec.WeirSlotWidth / 2;
                s.Outline(v, v.Map(new[] { new Vector2(-w, top), new Vector2(-w, slot), new Vector2(w, slot), new Vector2(w, top) }), false, s.Visible, netDxf.Lineweight.W25);
            }
        }

        /// <summary>
        /// FEP-encapsulated o-ring: blue jacket, pink silicone core.
        /// </summary>
        public static void Oring(Sheet s, Vector2 centre, double a, double b)
        {
            Ellipse jacket = new Ellipse(centre, 2 * b, 2 * a) { Rotation = 90 };
            Ellipse core = new Ellipse(centre, 2 * b * 0.62, 2 * a * 0.62) { Rotation = 90 };
            s.FillLoops(new EntityObject[] { jacket }, Material.OringJacket);
            s.FillLoops(new EntityObject[] { core }, Material.OringCore);
            s.Add(new Ellipse(centre, 2 * b, 2 * a) { Rotation = 90 }, s.Visible, null, netDxf.Lineweight.W18);
        }
    }
}
