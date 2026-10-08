using netDxf;
using netDxf.Entities;

namespace DowncomerRetrofit
{
    /// <summary>
    /// DWG-DCINS-02 - machining details of items 1, 2 and 5.
    /// </summary>
    public static class Sheet2
    {
        public static Sheet Build()
        {
            Sheet s = new Sheet();
            Insert(s);
            Retainer(s);
            Nut(s);
            Gland(s);
            Features(s);
            Operations(s);

            // column rules
            foreach (double x in new[] { 160.0, 267.0 })
            {
                s.Line(new Vector2(x, 44), new Vector2(x, 284), s.Thin, Sheet.Grey, Lineweight.W13);
            }

            s.TitleBlock(new[]
            {
                "DIMENSIONS IN mm. GENERAL TOLERANCE ±0.3 UNLESS SHOWN.",
                "SIZES SUIT A 19.05 OD STANDPIPE (ASSUMED). IF THE MEASURED OD DIFFERS, ONLY FEATURE A\nCHANGES.",
                "READ WITH DWG-DCINS-01 (ASSEMBLY) AND DWG-DCINS-03 (INSTALLATION)."
            }, "ITEM 1, 2 & 5 — MACHINING DETAILS", "DWG-DCINS-02", "AS SHOWN", "2 OF 3");
            return s;
        }

        private static void Insert(Sheet s)
        {
            View v = new View(88, 252, 2.0);
            foreach (int side in new[] { -1, 1 })
            {
                s.Section(v, Parts.Insert(), Material.Insert, side);
                s.Outline(v, v.Map(Parts.InsertThreadLine(), side), false, s.Thin);
            }
            s.CenterLine(v.P(0, -71), v.P(0, 14));
            s.Text("SEAT", 92, 253.0, 2.22, false, TextAlignment.BaselineLeft, Sheet.Grey);

            double rf = Spec.FlangeDia / 2;
            double rc = Spec.CollarDia / 2;
            double rEnd = Spec.SpigotDia / 2 - Spec.SpigotChamfer;
            double top = Spec.FlangeThickness;
            s.Dim(v, v.P(rf, top, -1), v.P(rf, top), new Vector2(88, 278), 0, "Ø40");
            s.Dim(v, v.P(rf, top), v.P(rf, 0), new Vector2(140, 0), 90);
            s.Dim(v, v.P(rf, 0), v.P(rc, Spec.CollarTop), new Vector2(140, 0), 90);
            s.Dim(v, v.P(rc, Spec.CollarTop), v.P(rc, Spec.CollarBottom), new Vector2(140, 0), 90);
            s.Dim(v, v.P(rc, Spec.CollarBottom), v.P(rEnd, Spec.SpigotEnd), new Vector2(140, 0), 90);
            s.Dim(v, v.P(rf, top), v.P(rEnd, Spec.SpigotEnd), new Vector2(154, 0), 90);

            // feature letters, as the features table
            s.FeatureTag("A", new Vector2(30, 267.4), v.P(Spec.CounterboreDia / 2, 4.0, -1));
            s.FeatureTag("C", new Vector2(30, 253.4), v.P(rf, 4.5, -1));
            s.FeatureTag("D", new Vector2(30, 213.4), v.P(Spec.ThreadMajor / 2, -19.35, -1));
            s.FeatureTag("B", new Vector2(30, 191.4), v.P(Spec.BoreDia / 2, -29.35, -1));
            s.FeatureTag("E", new Vector2(30, 171.4), v.P(rc, -38.35, -1));
            s.FeatureTag("G", new Vector2(30, 151.4), v.P(Spec.BarbDia / 2, Spec.CollarBottom - Spec.BarbsFromCollar[0], -1));
            s.FeatureTag("F", new Vector2(30, 129.4), v.P(Spec.SpigotDia / 2, -63.6, -1));

            s.ViewTitle("ITEM 1 — INSERT", "2:1   PTFE + 25 % GLASS, 1 OFF PER POSITION", 88, 106.0);
        }

        private static void Retainer(Sheet s)
        {
            View v = new View(212, 247, 3.0);
            foreach (int side in new[] { -1, 1 })
            {
                s.Section(v, Parts.Retainer(), Material.Retainer, side);
            }
            s.CenterLine(v.P(0, -3), v.P(0, 11.6));

            double rf = Spec.RetainerFlangeDia / 2;
            double rs = Spec.RetainerSpigotDia / 2;
            double ri = Spec.RetainerBore / 2;
            double h = Spec.RetainerLength;
            double l = Spec.RetainerSpigotLength;
            s.Dim(v, v.P(rf, h, -1), v.P(rf, h), new Vector2(212, 278.8), 0, "Ø28");
            s.Dim(v, v.P(ri, 5.1, -1), v.P(ri, 5.1), new Vector2(212, v.P(0, 5.1).Y), 0, "Ø19.3 ±0.1");
            s.Dim(v, v.P(rs, 0, -1), v.P(rs, 0), new Vector2(212, 241), 0, "Ø21.8 -0.1/0");
            s.Dim(v, v.P(rf, h), v.P(rf, l), new Vector2(260, 0), 90, "2.0");
            s.Dim(v, v.P(rf, l), v.P(rs, 0), new Vector2(260, 0), 90, "6.6 ±0.1");

            s.Text("SPIGOT LENGTH SETS THE O-RING GLAND WIDTH (2.4).", 212, 222.0, 2.29, false, TextAlignment.BaselineCenter, Sheet.Grey);
            s.ViewTitle("ITEM 5 — SEAL RETAINER", "3:1   PTFE + 25 % GLASS, 1 OFF. ONE OPERATION.", 212, 214.0);
        }

        private static void Nut(Sheet s)
        {
            View v = new View(212, 172, 2.0);
            foreach (int side in new[] { -1, 1 })
            {
                s.Section(v, Parts.Nut(false), Material.ClampNut, side);
                s.Outline(v, v.Map(Parts.NutThreadLine(), side), false, s.Thin);
            }
            s.CenterLine(v.P(0, -4), v.P(0, 14));

            double rf = Spec.NutDia / 2;
            double t = Spec.NutThickness;
            s.Dim(v, v.P(rf, t, -1), v.P(rf, t), new Vector2(212, 197), 0, "Ø40");
            s.Dim(v, v.P(rf, t), v.P(rf, 0), new Vector2(258, 0), 90);

            s.Text("M24 × 2 - 6H", 168.5, 163.1, 2.5);
            s.Leader(new Vector2(184, 164.0), v.P(Spec.NutMinor / 2, 4.0, -1));
            s.Text("1 × 45° BOTH ENDS", 216.8, 163.1, 2.5);
            s.Leader(new Vector2(241, 164.0), v.P(rf - Spec.NutChamfer / 2, Spec.NutChamfer / 2));

            s.Text("PLAIN ROUND — TIGHTEN WITH A STRAP WRENCH.", 212, 155.55, 2.29, false, TextAlignment.BaselineCenter, Sheet.Grey);
            s.ViewTitle("ITEM 2 — NUT", "2:1   PTFE + 25 % GLASS, 2 OFF (CLAMP + JAM). ONE OPERATION.", 212, 148.0);
        }

        private static void Gland(Sheet s)
        {
            s.Text("STANDPIPE SEAL GLAND (ITEM 4)", 164, 126.0, 3.33, true);
            double[] xs = { 164, 182, 224, 244, 262 };
            double[] ys = { 122, 117.02, 107.67 };
            string[][] cells =
            {
                new[] { "O-RING", "GLAND", "SQUEEZE", "FILL" },
                new[] { "18.5 × 1.78", "Ø22.0 COUNTERBORE ON THE\nØ19.05 STANDPIPE, 2.4 WIDE\nBETWEEN THE SEAT AND ITEM 5", "17 % (11 % AT\nOD 18.85)", "70 %" }
            };
            s.Table(xs, ys, cells, 2.29, 2.35, firstLine: 3.2);

            s.List(new[]
            {
                ("•", "FEP-ENCAPSULATED, SILICONE CORE. SIZE TO BE CONFIRMED WITH THE\nSUPPLIER."),
                ("•", "ENCAPSULATED RINGS BARELY STRETCH — WARM ITEM 4 IN HOT WATER\nBEFORE FITTING IT."),
                ("•", "STANDARD ITEM 1 SUITS A STANDPIPE OD OF 18.85–19.15 AT THE SEAL BAND.\nOUTSIDE THAT, BORE FEATURE A TO THE MEASURED OD + 2.95 — STILL OP 20\nONLY.")
            }, 164, 3.0, 103.7, 2.43, 2.47, 0.83);
        }

        private static void Features(Sheet s)
        {
            s.Text("ITEM 1 — FEATURES", 272, 281.0, 3.89, true);
            double[] xs = { 272, 279, 308, 396, 406 };
            double[] ys = { 277, 271.99, 262.47, 257.74, 250.61, 243.49, 236.36, 231.63, 224.51 };
            string[][] cells =
            {
                new[] { "", "FEATURE", "SIZE / TOLERANCE", "OP" },
                new[] { "A", "COUNTERBORE", "Ø22.0 +0.1/0 × 9.0 ±0.1 DEEP FROM THE TOP FACE. FLAT BOTTOM, LEVEL\nWITH THE FLANGE UNDERSIDE, IS THE STANDPIPE SEAT. MOUTH\nCHAMFER 0.5 × 30°.", "20" },
                new[] { "B", "BORE", "Ø12.0 THROUGH.", "10" },
                new[] { "C", "FLANGE", "Ø40 × 9. TOP EDGE 1 × 45°, UNDERSIDE EDGE 0.5 × 45°. UNDERSIDE FLAT,\nNO GROOVE.", "10/20" },
                new[] { "D", "THREAD", "M24 × 2 - 6g × 34 FROM THE FLANGE UNDERSIDE. LAST 3 MAY BE\nINCOMPLETE (INSIDE THE FLOOR HOLE). 45° CHAMFER TO E.", "10" },
                new[] { "E", "COLLAR", "Ø21.5 ±0.1 × 8 — PASSES THE NUT MINOR (Ø21.84). THE LEG STOPS ON ITS\nSHOULDER.", "10" },
                new[] { "F", "SPIGOT", "Ø16.2 ±0.1 × 24. END CHAMFER 0.8 × 45°.", "10" },
                new[] { "G", "BARBS", "2 OFF Ø17.0 ±0.1, SHARP FACE TOWARD E, AT 6 AND 16 FROM E. 4 LONG\nRAMP.", "10" }
            };
            s.Table(xs, ys, cells, 2.3, 2.42, new[] { true, false, false, false }, firstLine: 3.2);
        }

        private static void Operations(Sheet s)
        {
            // item 1, under its view
            s.Text("ITEM 1 — TWO LATHE OPERATIONS, NOTHING ELSE", 14, 93.0, 3.33, true);
            double y = s.List(new[]
            {
                ("OP 10", "Ø45 BAR, 90 OUT OF THE CHUCK. FACE. CENTRE-DRILL, DRILL Ø12 × 80 DEEP (B). TURN THE SPIGOT\nAND BOTH BARBS (F, G), THE COLLAR (E), THE THREAD OD, THE FLANGE UNDERSIDE AND THE FLANGE\nØ40 (C). SINGLE-POINT THE M24 × 2 (D). CHAMFERS. PART OFF 1 LONG."),
                ("OP 20", "REVERSE, HOLD ON THE Ø40 FLANGE IN SOFT JAWS. FACE TO 75.0 OVERALL. BORE THE Ø22.0\nCOUNTERBORE FLAT-BOTTOMED TO 9.0 DEEP (A). MOUTH CHAMFER 0.5 × 30°, TOP EDGE 1 × 45°.")
            }, 14, 11, 88.4, 2.57, 2.63, 1.0);
            s.Para("NO MILLING, NO CROSS HOLES, NO INTERNAL OR FACE GROOVES. ONE BORING BAR, ONE THREADING\nTOOL.", 25, y, 2.57, 2.63);

            // items 2 and 5
            s.Text("ITEMS 2 AND 5 — ONE OPERATION EACH", 272, 217.5, 3.33, true);
            s.List(new[]
            {
                ("ITEM 2", "Ø45 BAR. FACE. DRILL Ø22.0, THREAD M24 × 2 (TAP OR SINGLE-POINT), CHAMFER THE THREAD\nBOTH ENDS. TURN Ø40, CHAMFER. PART OFF AT 10, DEBURR. SAME PART FOR THE CLAMP AND\nTHE JAM NUT."),
                ("ITEM 5", "Ø30 BAR. FACE. DRILL AND BORE Ø19.3. TURN THE Ø21.8 SPIGOT 6.6 LONG AND THE Ø28 FLANGE.\nCHAMFERS. PART OFF AT 8.6.")
            }, 272, 12, 212.9, 2.5, 2.55, 1.0);

            s.Text("MANUFACTURING NOTES", 272, 194.1, 3.33, true);
            s.List(new[]
            {
                ("1.", "MATERIAL: PTFE + 25 % GLASS FIBRE, MOULDED ROD FROM VIRGIN RESIN — Ø45 FOR ITEMS 1 AND 2,\nØ30 FOR ITEM 5. CERTIFICATE OF CONFORMANCE PER BATCH."),
                ("2.", "STRESS-RELIEVE (ANNEAL) BLANKS BEFORE FINISH CUTS. CARBIDE TOOLING (THE GLASS IS\nABRASIVE), SHARP, POSITIVE RAKE, LIGHT FINISHING CUTS, NO COOLANT OR WATER-FREE\nCOOLANT."),
                ("3.", "MEASURE AT 20–23 °C — PTFE CHANGES SIZE THROUGH 19 °C."),
                ("4.", "ONLY A, E, F, G AND THE ITEM 5 SPIGOT AND BORE CARRY TIGHT TOLERANCES. EVERYTHING ELSE\n±0.3."),
                ("5.", "SEAL SURFACES (A, SEAT, FLANGE UNDERSIDE, ITEM 5 SPIGOT): Ra 1.6, NO BURRS."),
                ("6.", "RUN A NUT ONTO EVERY ITEM 1 BY HAND BEFORE DISPATCH."),
                ("7.", "MARK PART No., BATCH AND YEAR ON THE FLANGE TOP OUTSIDE Ø28 — ENGRAVE 0.2 DEEP."),
                ("8.", "ITEM 3: PUNCH FROM 1.5 EXPANDED PTFE SHEET, Ø40 × Ø25."),
                ("9.", "KIT PER POSITION: 1, 2 × 2, 3 × 2, 4, 5 — ITEMS 4 AND 5 CAN BE FITTED INTO ITEM 1 AT THE BENCH.\nITEMS 6 AND 7 FROM STOCK ON SITE. CLEAN, DRY, OIL-FREE.")
            }, 272, 6, 189.5, 2.5, 2.56, 0.7);
        }
    }
}
