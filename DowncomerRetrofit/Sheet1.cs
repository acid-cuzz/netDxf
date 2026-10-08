using System.Collections.Generic;
using netDxf;
using netDxf.Entities;

namespace DowncomerRetrofit
{
    /// <summary>
    /// DWG-DCINS-01 - general arrangement and parts list.
    /// </summary>
    public static class Sheet1
    {
        public static Sheet Build()
        {
            Sheet s = new Sheet();
            SectionAA(s);
            DetailC(s);
            Plan(s);
            PartsList(s);
            Legend(s);
            DesignIntent(s);

            s.TitleBlock(new[]
            {
                "DIMENSIONS IN mm.",
                "EXISTING STANDPIPE 19.05 OD × 1.65 WT, FLOOR 6.35 AND 40 PITCH ARE ASSUMED — CONFIRM\nON SITE BEFORE MACHINING.",
                "FOR A DOWNCOMER WHOSE LEG IS GONE BUT WHOSE STANDPIPE, WEIR AND SURROUNDING\nFLOOR ARE SOUND.",
                "ITEM 1, 2 AND 5 DETAILS ON DWG-DCINS-02. INSTALLATION ON DWG-DCINS-03."
            }, "GENERAL ARRANGEMENT & PARTS LIST", "DWG-DCINS-01", "AS SHOWN", "1 OF 3");
            return s;
        }

        private static void SectionAA(Sheet s)
        {
            View v = new View(110, 228.7, 2.0);
            Assembly.Draw(s, v, new AssemblyOptions());

            // detail C call-out on the seal
            s.Circle(v.P(10, 4), v.L(8.5), s.Hidden);
            s.Text("C", v.P(19.4, 12.2).X, v.P(19.4, 12.2).Y, 4.44, true, TextAlignment.BaselineCenter);

            // flange height above the floor
            s.Dim(v, v.P(20, Spec.FloorTop), v.P(20, Spec.FlangeThickness), new Vector2(159, 236.7), 90);

            // items
            s.Balloon("5", new Vector2(26, 258.0), v.P(12.5, 10.4, -1));
            s.Balloon("1", new Vector2(26, 245.0), v.P(17.5, 5.5, -1));
            s.Balloon("4", new Vector2(26, 232.0), v.P(10.27, 1.0, -1));
            s.Balloon("3", new Vector2(26, 219.0), v.P(17.5, -0.5, -1), v.P(17.5, -7.85, -1));
            s.Text("2 OFF", 26, 212.25, 2.36, false, TextAlignment.BaselineCenter);
            s.Balloon("2", new Vector2(26, 197.0), v.P(17.0, -13.35, -1), v.P(17.0, -23.35, -1));
            s.Text("2 OFF", 26, 190.25, 2.36, false, TextAlignment.BaselineCenter);
            s.Balloon("7", new Vector2(26, 150.0), v.P(11.05, -38.35, -1));
            s.Balloon("6", new Vector2(26, 80.0), v.P(8.75, -79.4, -1));

            // notes on the right
            Note(s, "STANDPIPE (EXISTING, REUSED) —\nCUT 1.0 ABOVE FLOOR TOP, WEIR INTACT", 275.05, v.P(Spec.StandpipeRo, 22.0));
            Note(s, "WEIR SLOT — BOTTOM 15 MIN.\nABOVE FLOOR TOP (CHECK)", 263.05, v.P(Spec.WeirSlotWidth / 2, 23.5));
            Note(s, "STANDPIPE CUT 1.0 ABOVE THE FLOOR\nTOP, SEATS LEVEL WITH THE FLANGE\nUNDERSIDE — WEIR HEIGHT UNCHANGED", 249.05, v.P(8.7, 0.0));
            Note(s, "TROUGH FLOOR (EXISTING, SOUND)", 235.05, v.P(27.0, -4.2));
            Note(s, "DRILLED OUT Ø25 —\nOLD JOINT REMOVED", 223.05, v.P(Spec.HoleDia / 2, -4.75));
            Note(s, "M24 × 2", 195.05, v.P(11.1, -21.4));
            Note(s, "PLAIN COLLAR Ø21.5 — LEG\nSTOPS HERE, BAND LANDS HERE", 169.05, v.P(Spec.CollarDia / 2, -37.4));
            Note(s, "BARBED SPIGOT Ø16.2 — 2 BARBS.\nNO PINS, NO CROSS HOLES", 149.05, v.P(Spec.BarbDia / 2, -48.0));
            Note(s, "3/4\" PFA TUBE — SAME OD AS\nTHE ORIGINAL DOWNCOMER", 123.05, v.P(8.75, -83.4));

            s.ViewTitle("SECTION A-A — ASSEMBLED", "2:1", 110, 46.0);
        }

        private static void Note(Sheet s, string text, double baseline, Vector2 target)
        {
            s.Para(text, 178, baseline, 2.64, 2.75);
            s.Leader(new Vector2(177, baseline + 0.95), target);
        }

        private static void DetailC(Sheet s)
        {
            View v = new View(164, 70, 4.0) { HatchFactor = 1.3 };
            Vector2 centre = v.P(10, 4);
            v.ClipCentre = centre;
            v.ClipRadius = 33.0;
            Assembly.Draw(s, v, new AssemblyOptions
            {
                Leg = false,
                BreakLines = false,
                CentreLine = false
            });
            s.Circle(centre, 33.0, s.Visible, null, Lineweight.W25);

            DetailNote(s, "ITEM 5 RETAINER — DROPS IN,\nSLIP FIT, TRAPPED BY THE\nSTANDPIPE", 111.2, v.P(12.5, 10.0));
            DetailNote(s, "ITEM 4 O-RING — GLAND MADE\nBY THE COUNTERBORE AND\nITEM 5. NO GROOVE TO MACHINE", 97.2, v.P(10.55, 1.3));
            DetailNote(s, "SEAT = FLANGE-UNDERSIDE PLANE,\n1.0 ABOVE THE FLOOR TOP", 83.2, v.P(8.7, 0.0));
            DetailNote(s, "ITEM 3 — FLAT ePTFE GASKET,\nNO FACE GROOVE", 71.2, v.P(16.0, -0.5));
            DetailNote(s, "PLAIN COUNTERBORE Ø22.0 × 9", 59.2, v.P(Spec.CounterboreDia / 2, 6.0));

            s.ViewTitle("DETAIL C", "4:1   (STANDPIPE SEAL)", 204.2, 46.0);
        }

        private static void DetailNote(Sheet s, string text, double baseline, Vector2 target)
        {
            s.Para(text, 241, baseline, 2.43, 2.54);
            s.Leader(new Vector2(240, baseline + 0.8), target);
        }

        private static void Plan(Sheet s)
        {
            Vector2 c = new Vector2(345.0, 164.43);
            double x0 = 290.0;
            double x1 = 400.0;
            double y0 = c.Y - 20;
            double y1 = c.Y + 20;

            // floor strip between the neighbouring positions, broken at both ends
            Polyline2D strip = new Polyline2D(new[] { new Vector2(x0, y0), new Vector2(x1, y0), new Vector2(x1, y1), new Vector2(x0, y1) }, true);
            s.FillLoops(new EntityObject[] { strip }, Material.PlanFloor);
            s.Line(new Vector2(x0, y0), new Vector2(x1, y0), s.Thin, Sheet.Grey);
            s.Line(new Vector2(x0, y1), new Vector2(x1, y1), s.Thin, Sheet.Grey);
            s.BreakLine(new Vector2(x0, y0 - 3.5), new Vector2(x0, y1 + 3.5));
            s.BreakLine(new Vector2(x1, y0 - 3.5), new Vector2(x1, y1 + 3.5));

            // neighbours at the assumed pitch
            foreach (double dx in new[] { -Spec.Pitch, Spec.Pitch })
            {
                Tube(s, new Vector2(c.X + dx, c.Y));
            }

            // this position from above: flange, retainer and standpipe; the drilled hole is hidden under the flange
            Annulus(s, c, Spec.FlangeDia / 2, Spec.RetainerFlangeDia / 2, Material.Insert.Fill, Lineweight.W35);
            Annulus(s, c, Spec.RetainerFlangeDia / 2, Spec.StandpipeRo, Material.Retainer.Fill, Lineweight.W25);
            s.Circle(c, Spec.HoleDia / 2, s.Hidden);
            Tube(s, c);

            // centre lines
            s.CenterLine(new Vector2(x0 - 3, c.Y), new Vector2(x1 + 3, c.Y));
            foreach (double dx in new[] { -Spec.Pitch, 0.0, Spec.Pitch })
            {
                double r = dx == 0 ? Spec.FlangeDia / 2 + 3 : Spec.StandpipeRo + 2.5;
                s.CenterLine(new Vector2(c.X + dx, c.Y - r), new Vector2(c.X + dx, c.Y + r));
            }

            // section A-A cutting plane: thick ends beyond the strip, arrows in the direction of view
            foreach ((double start, double arrow) in new[] { (x0 - 6, x0 - 4.8), (x1 + 1, x1 + 4.8) })
            {
                s.Line(new Vector2(start, c.Y), new Vector2(start + 5, c.Y), s.Visible, null, Lineweight.W70);
                s.Line(new Vector2(arrow, c.Y), new Vector2(arrow, c.Y + 5.5), s.Notes);
                s.Arrow(new Vector2(arrow, c.Y + 8.0), new Vector2(arrow, c.Y));
                s.Text("A", arrow, c.Y + 9.3, 3.61, true, TextAlignment.BaselineCenter);
            }

            // pitch and clearance
            View v = new View(c.X, c.Y, 1.0);
            s.Dim(v, c, new Vector2(c.X + Spec.Pitch, c.Y), new Vector2(c.X + 20, 189.44), 0, "40 PITCH (ASSUMED)");
            double clear = Spec.Pitch - Spec.FlangeDia / 2 - Spec.StandpipeRo;
            double ya = 150.0;
            double xf = c.X + Spec.FlangeDia / 2;
            double xn = c.X + Spec.Pitch - Spec.StandpipeRo;
            s.Line(new Vector2(xf, c.Y - 0.8), new Vector2(xf, ya - 1.2), s.Dims);
            s.Line(new Vector2(xn, c.Y - 0.8), new Vector2(xn, ya - 1.2), s.Dims);
            s.Line(new Vector2(xf - 5, ya), new Vector2(xf, ya), s.Dims);
            s.Arrow(new Vector2(xf, ya), new Vector2(xf - 5, ya), 2.4, 0.8, s.Dims);
            s.Line(new Vector2(xn, ya), new Vector2(xn + 5, ya), s.Dims);
            s.Arrow(new Vector2(xn, ya), new Vector2(xn + 5, ya), 2.4, 0.8, s.Dims);
            s.Text(clear.ToString("0.0") + " CLEAR", xn + 6, ya - 0.9, 2.5, false, TextAlignment.BaselineLeft, null, 0, s.Dims);

            s.ViewTitle("PLAN ON TROUGH FLOOR", "1:1", c.X, 138.4);
        }

        private static void Annulus(Sheet s, Vector2 c, double r, double ri, AciColor fill, Lineweight lw)
        {
            s.FillLoops(new EntityObject[] { new Circle(c, r), new Circle(c, ri) }, new Material { Fill = fill });
            s.Circle(c, r, s.Visible, null, lw);
        }

        /// <summary>
        /// Standpipe in plan: grey wall, open bore.
        /// </summary>
        private static void Tube(Sheet s, Vector2 c)
        {
            s.FillLoops(new EntityObject[] { new Circle(c, Spec.StandpipeRo), new Circle(c, Spec.StandpipeRi) }, new Material { Fill = Material.Standpipe.Fill });
            s.Circle(c, Spec.StandpipeRo, s.Visible, null, Lineweight.W25);
            s.Circle(c, Spec.StandpipeRi, s.Visible, null, Lineweight.W25);
        }

        private static void PartsList(Sheet s)
        {
            s.Text("PARTS LIST — PER DOWNCOMER", 282, 281.0, 3.89, true);
            double[] xs = { 282, 290, 298, 370, 408 };
            double[] ys = { 277, 271.82, 264.51, 257.25, 249.98, 245.22, 240.42, 230.68, 223.38, 216.11 };
            string[][] cells =
            {
                new[] { "ITEM", "QTY", "DESCRIPTION", "MATERIAL" },
                new[] { "1", "1", "INSERT — Ø40 FLANGE, Ø22 COUNTERBORE, M24 × 2\nNECK, BARBED SPIGOT. DWG-DCINS-02.", "PTFE, 25 % GLASS-FILLED" },
                new[] { "2", "2", "NUT M24 × 2, Ø40 × 10, PLAIN ROUND — CLAMP NUT AND\nJAM NUT, SAME PART. DWG-DCINS-02.", "PTFE, 25 % GF" },
                new[] { "3", "2", "GASKET Ø40 × Ø25 × 1.5 — FLOOR TOP AND UNDERSIDE,\nSAME PART.", "EXPANDED PTFE" },
                new[] { "4", "1", "O-RING 18.5 ID × 1.78 CS — STANDPIPE SEAL.", "FEP-ENCAP., SILICONE CORE" },
                new[] { "5", "1", "SEAL RETAINER — Ø28 TOP HAT, DROP-IN. DWG-DCINS-02.", "PTFE, 25 % GF" },
                new[] { "6", "1", "NEW LEG — TUBE 3/4\" OD × 0.062\" WALL (19.05 × 1.57),\nTHERMOFORMED TO THE DOWNCOMER MARK, CUT TO\nTHE DISCHARGE POINT.", "PFA" },
                new[] { "7", "1", "JOINT BAND — HEAT-SHRINK 1\" 1.6:1 (M23053/11-208 OR\nEQUAL), 40 LONG.", "FEP" },
                new[] { "—", "—", "STANDPIPE — EXISTING, CUT 1.0 ABOVE THE FLOOR TOP\nAND REFITTED.", "EXISTING" }
            };
            s.Table(xs, ys, cells, 2.36, 2.49, new[] { true, false, false, false }, firstLine: 3.27);
        }

        private static void Legend(Sheet s)
        {
            (string Label, Material M)[] left =
            {
                ("GLASS-FILLED PTFE (1, 2)", Material.Insert),
                ("EXPANDED PTFE (3)", Material.Gasket),
                ("PFA TUBE (6)", Material.Tube),
                ("EXISTING STANDPIPE", Material.Standpipe)
            };
            (string Label, Material M)[] right =
            {
                ("GLASS-FILLED PTFE (5)", Material.Retainer),
                ("O-RING, FEP JACKET (4)", Material.OringJacket),
                ("FEP BAND (7)", Material.Band)
            };
            Swatches(s, left, 282);
            Swatches(s, right, 346);
        }

        private static void Swatches(Sheet s, IList<(string Label, Material M)> items, double x)
        {
            double y = 210.22;
            foreach ((string label, Material m) in items)
            {
                List<Vector2> box = new List<Vector2> { new Vector2(x, y), new Vector2(x + 5, y), new Vector2(x + 5, y + 2.6), new Vector2(x, y + 2.6) };
                s.FillLoops(new EntityObject[] { new Polyline2D(box, true) }, m, 0.8);
                s.Poly(box, true, s.Notes, null, Lineweight.W13);
                s.Text(label, x + 7, y + 0.45, 2.43);
                y -= 3.8;
            }
        }

        private static void DesignIntent(Sheet s)
        {
            s.Text("DESIGN INTENT — REV B", 282, 127.45, 3.89, true);
            s.List(new[]
            {
                ("•", "EASY TO MAKE — ITEM 1 IS TWO PLAIN LATHE OPERATIONS: NO INTERNAL GROOVE, NO FACE\nGROOVE, NO CROSS HOLES, NO MILLING. ITEMS 2 AND 5 ARE ONE OPERATION EACH."),
                ("•", "THE O-RING GLAND IS MADE BY A PLAIN COUNTERBORE AND A DROP-IN RETAINER (DETAIL C)."),
                ("•", "FLAT ePTFE GASKETS BOTH SIDES OF THE FLOOR — THEY CONFORM TO A PITTED FLOOR. NUTS\nAND GASKETS ARE ONE PART EACH, USED TWICE."),
                ("•", "STANDPIPE CUT 1.0 ABOVE THE FLOOR TOP AND SEATED LEVEL WITH THE FLANGE UNDERSIDE,\n1.0 UP — WEIR HEIGHT UNCHANGED."),
                ("•", "NEW LEG IS STOCK 3/4\" PFA ON A BARBED SPIGOT UNDER AN FEP BAND. NOTHING METAL BELOW\nTHE FLOOR.")
            }, 282, 3.2, 122.25, 2.5, 2.53, 0.93);
        }
    }
}
