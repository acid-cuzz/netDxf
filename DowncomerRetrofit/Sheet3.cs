using System.Collections.Generic;
using netDxf;
using netDxf.Entities;

namespace DowncomerRetrofit
{
    /// <summary>
    /// DWG-DCINS-03 - selection, limits and installation.
    /// </summary>
    public static class Sheet3
    {
        private const double Scale = 1.0 / 1.5;
        private const double Seat = 240.91;

        public static Sheet Build()
        {
            Sheet s = new Sheet();
            Sequence(s);
            WhichRepair(s);
            Installation(s);
            Inspection(s);

            s.Line(new Vector2(12, 156), new Vector2(408, 156), s.Thin);
            foreach (double x in new[] { 142.0, 276.0 })
            {
                s.Line(new Vector2(x, 44), new Vector2(x, 153), s.Thin, Sheet.Grey, Lineweight.W13);
            }

            s.TitleBlock(new[]
            {
                "READ WITH DWG-DCINS-01 AND -02. ITEM NUMBERS AS THE SHEET 1 PARTS LIST.",
                "SEQUENCE SKETCHES 1:1.5, SECTIONED ON THE DOWNCOMER CENTRE.",
                "LIMITS OF USE ARE PROVISIONAL UNTIL PROVEN BY TRIAL."
            }, "SELECTION, LIMITS & INSTALLATION", "DWG-DCINS-03", "1:1.5 / NTS", "3 OF 3");
            return s;
        }

        private static View SketchView(int i)
        {
            return new View(52 + 79 * i, Seat, Scale) { HatchFactor = 0.6, Outline = Lineweight.W25 };
        }

        private static AssemblyOptions SketchOptions()
        {
            return new AssemblyOptions
            {
                FloorOuter = 34,
                LegBottom = -89.4,
                CentreTop = 43.0,
                CentreBottom = -92.4
            };
        }

        private static void Sequence(Sheet s)
        {
            s.Text("INSTALLATION SEQUENCE — ONE POSITION (1:1.5)", 14, 280.0, 4.44, true);
            s.Line(new Vector2(14, 278.5), new Vector2(14 + Sheet.TextWidth("INSTALLATION SEQUENCE — ONE POSITION (1:1.5)", 4.44, true), 278.5), s.Notes, null, Lineweight.W30);

            // a - as found
            View a = SketchView(0);
            AssemblyOptions oa = SketchOptions();
            oa.Standpipe = StandpipeState.AsFound;
            oa.Insert = false;
            oa.Leg = false;
            oa.HoleRadius = Spec.StandpipeRo;
            Assembly.Draw(s, a, oa);

            // b - standpipe cut 1.0 above the floor top and lifted out, stub and joint drilled out
            View b = SketchView(1);
            AssemblyOptions ob = SketchOptions();
            ob.Standpipe = StandpipeState.Lifted;
            ob.Insert = false;
            ob.Leg = false;
            Assembly.Draw(s, b, ob);
            s.Line(b.P(16.5, 11), b.P(16.5, 26.5), s.Notes, Sheet.Blue);
            s.Arrow(b.P(16.5, 29), b.P(16.5, 11), 2.4, 0.8, null, Sheet.Blue);
            s.Dim(b, b.P(Spec.HoleDia / 2, Spec.FloorBottom, -1), b.P(Spec.HoleDia / 2, Spec.FloorBottom), b.P(0, -10.4), 0, "Ø25");

            // c - insert, o-ring and retainer in from above; gasket, nut and jam nut from below
            View c = SketchView(2);
            AssemblyOptions oc = SketchOptions();
            oc.Standpipe = StandpipeState.None;
            oc.Leg = false;
            Assembly.Draw(s, c, oc);

            // d - standpipe back onto its seat, leg and band fitted
            View d = SketchView(3);
            Assembly.Draw(s, d, SketchOptions());

            // e - test
            View e = SketchView(4);
            Assembly.Draw(s, e, SketchOptions());
            double level = 22.0;
            foreach (int side in new[] { -1, 1 })
            {
                s.Line(e.P(Spec.StandpipeRo, level, side), e.P(32, level, side), s.Hidden, Sheet.Blue);
            }
            s.Text("ACID LEVEL", e.P(12, level + 3.9).X, e.P(12, level + 3.9).Y, 2.22, false, TextAlignment.BaselineLeft, Sheet.Blue);
            s.Line(e.P(0, Spec.WeirSlotBottom), e.P(0, -79.5), s.Notes, Sheet.Blue);
            s.Arrow(e.P(0, -83.3), e.P(0, Spec.WeirSlotBottom), 2.4, 0.8, null, Sheet.Blue);

            string[] titles = { "AS FOUND", "CUT + DRILL", "INSERT + CLAMP", "STANDPIPE + LEG", "TEST" };
            string[] captions =
            {
                "LEG EATEN THROUGH AT OR JUST BELOW THE FLOOR.\nSTANDPIPE, WEIR AND FLOOR STILL SOUND.",
                "STANDPIPE CUT 1.0 ABOVE THE FLOOR TOP AND LIFTED\nOUT. Ø25 DRILLED ON THE STUB CENTRE — OLD JOINT\nGONE.",
                "INSERT, WITH O-RING AND RETAINER FITTED AT THE\nBENCH, IN FROM ABOVE ON A GASKET. FROM BELOW:\nGASKET, NUT, JAM NUT.",
                "STANDPIPE PUSHED DOWN THROUGH THE RETAINER\nONTO ITS SEAT. PFA LEG PUSHED OVER THE BARBS, BAND\nSHRUNK OVER.",
                "PACKING BACK BY HAND. CIRCULATE — LEVEL IN THE\nWEIR MATCHES ITS NEIGHBOURS."
            };
            for (int i = 0; i < 5; i++)
            {
                double x = 18 + 79 * i;
                s.Circle(new Vector2(x, 172.2), 2.6, s.Notes, null, Lineweight.W25);
                s.Text(((char)('a' + i)).ToString(), x, 172.2, 3.47, true, TextAlignment.MiddleCenter);
                s.Text(titles[i], x + 4.5, 171.95, 3.06, true);
                s.Para(captions[i], x - 2.5, 167.4, 2.5, 2.55);
            }
        }

        private static void WhichRepair(Sheet s)
        {
            s.Text("1. WHICH REPAIR?", 14, 150.95, 3.61, true);
            double[] xs = { 14, 74, 108, 138 };
            double[] ys = { 147.6, 142.59, 133.56, 124.5, 117.93 };
            string[][] cells =
            {
                new[] { "CONDITION FOUND", "REPAIR", "DRAWING" },
                new[] { "LEG DAMAGED BELOW THE FLOOR, MOSTLY\nFROM OUTSIDE. JOINT TO THE FLOOR STILL\nSOUND.", "ENCAPSULATION SLEEVE", "DWG-DCSLV-01, 02" },
                new[] { "LEG GONE AT OR THROUGH THE JOINT, OR THE\nJOINT LEAKS. STANDPIPE, WEIR AND FLOOR\nAROUND IT STILL SOUND.", "RETROFIT INSERT (THIS\nSET)", "DWG-DCINS-01–03" },
                new[] { "FLOOR THINNED OR HOLED AROUND THE\nPOSITION — WILL NOT TAKE A CLAMP.", "BULKHEAD INSERT", "DWG-DCBHD-01, 02" }
            };
            s.Table(xs, ys, cells, 2.36, 2.49,
                rowFill: new Dictionary<int, AciColor> { { 2, new AciColor(253, 242, 233) } },
                boldCells: new HashSet<(int, int)> { (2, 1), (2, 2) },
                firstLine: 2.85);

            s.Text("2. DESIGN BASIS", 14, 112.35, 3.61, true);
            s.List(new[]
            {
                ("•", "WEIR UNCHANGED — THE STANDPIPE IS CUT 1.0 ABOVE THE FLOOR TOP AND SEATS\nLEVEL WITH THE FLANGE UNDERSIDE, WHICH SITS 1.0 UP ON ITS GASKET. SLOT\nHEIGHTS AND FLOW STAY MATCHED TO THE NEIGHBOURS."),
                ("•", "LOAD PATH — FLANGE ON THE FLOOR TOP AND NUT ON THE UNDERSIDE, EACH\nTHROUGH A FLAT ePTFE GASKET. THE FLOOR CARRIES THE CLAMP; THE INSERT\nCARRIES ONLY THE STANDPIPE AND THE LEG."),
                ("•", "SEALING — THE TOP GASKET STOPS ACID BYPASSING THE WEIR; O-RING 4 SEALS THE\nSTANDPIPE IN A GLAND MADE BY THE COUNTERBORE AND ITEM 5. ONLY THE TROUGH\nLEVEL (TENS OF mm) ACTS ON EITHER SEAL."),
                ("•", "MADE ON A LATHE — ITEM 1 IN TWO PLAIN OPERATIONS, ITEMS 2 AND 5 IN ONE EACH.\nNO GROOVES, CROSS HOLES OR MILLING."),
                ("•", "NO METAL ADDED, NO WELDING, NO EXPANDING. EVERYTHING BELOW THE FLOOR IS\nFLUOROPOLYMER."),
                ("•", "SERVICE — H2SO4 98–99.5 %, UPPER-STAGE ACID 70–100 °C TYPICAL. PFA AND FEP\nRATED WELL ABOVE THIS; CREEP OF GLASS-FILLED PTFE UNDER CLAMP LOAD IS THE\nLIMIT TO PROVE.")
            }, 14, 3, 107.8, 2.78, 2.9, 0.82);
        }

        private static void Installation(Sheet s)
        {
            s.Text("3. INSTALLATION", 146, 150.95, 3.61, true);
            s.List(new[]
            {
                ("1.", "SURVEY: MARK AND PHOTOS. STANDPIPE SOUND, WEIR INTACT, OD AT THE SEAL BAND\n(1–12 ABOVE THE FLOOR) 18.85–19.15. UT THE FLOOR OUT TO Ø44. RECORD THE\nDISCHARGE POINT AND SLOT ORIENTATION."),
                ("2.", "CONFIRM THE WEIR SLOT BOTTOM IS 15 OR MORE ABOVE THE FLOOR TOP — ITEM 5\nSTANDS 12 PROUD."),
                ("3.", "BENCH: FIT ITEM 4 (WARMED) AND ITEM 5 INTO ITEM 1. THERMOFORM ITEM 6 TO THE\nMARK, 40 STRAIGHT AT THE TOP."),
                ("4.", "CUT THE STANDPIPE 1.0 ABOVE THE FLOOR TOP (INTERNAL TUBE CUTTER, KERF INTO\nTHE STUB). LIFT OUT, TAG, CHAMFER THE CUT END 0.5 × 30°, DEBURR."),
                ("5.", "DRILL Ø25 ON THE STUB CENTRE FROM INSIDE THE TROUGH (PILOTED ANNULAR\nCUTTER) — STUB AND OLD JOINT COME OUT WITH THE CORE. CLEAR THE OLD LEG\nFROM BELOW. DEBURR BOTH SIDES."),
                ("6.", "DRY-CLEAN BOTH FACES OF THE FLOOR OUT TO Ø44. NO WATER."),
                ("7.", "ITEM 3 ON THE FLOOR TOP, ITEM 1 IN FROM ABOVE. FROM BELOW: ITEM 3, THEN ITEM 2\n— HAND TIGHT UNTIL THE FLANGE SEATS, THEN 1/4 TURN WITH A STRAP WRENCH.\nSECOND ITEM 2 JAMMED AGAINST THE FIRST."),
                ("8.", "PUSH THE STANDPIPE DOWN THROUGH ITEMS 5 AND 4, TWISTING, UNTIL IT SEATS.\nSLOTS FACING AS RECORDED."),
                ("9.", "FROM BELOW: WARM THE END OF ITEM 6 (APPROX. 100 °C) AND PUSH IT OVER THE\nBARBS TO THE COLLAR, AIMED AT THE DISCHARGE POINT. ITEM 7 OVER THE JOINT,\nRECOVERED TOP-DOWN (AS DWG-DCSLV-02)."),
                ("10.", "REPLACE FINAL PACKING BY HAND. CIRCULATE ACID AND CHECK AS SECTION 5.")
            }, 146, 6, 146.4, 2.78, 2.9, 0.7);

            s.Text("4. FIT CHECK (ASSUMED DIMENSIONS)", 146, 73.05, 3.61, true);
            s.List(new[]
            {
                ("•", "M24 NECK IN Ø25 HOLE — 0.5 RADIAL CLEARANCE."),
                ("•", "BELOW THE FLOOR: 1 + 10 + 10 = 21 ON 25.4 OF FULL THREAD — 4 SPARE."),
                ("•", "NUT MINOR Ø21.84 PASSES THE Ø21.5 COLLAR AND THE Ø17 BARBS."),
                ("•", "FLANGE TO NEIGHBOUR TUBE AT 40 PITCH: 10.5 CLEAR."),
                ("•", "ITEM 5 TOP 12 ABOVE THE FLOOR TOP; SLOT BOTTOM 15 MIN. — 3 CLEAR."),
                ("•", "HEAT: THE GRIP SLACKENS APPROX. 0.05 AT 100 °C — PROVE IN THE HOT SOAK.")
            }, 146, 3, 68.5, 2.78, 2.9, 0.8);
        }

        private static void Inspection(Sheet s)
        {
            s.Text("5. INSPECTION & ACCEPTANCE", 280, 150.95, 3.61, true);
            s.List(new[]
            {
                ("•", "FLANGE SEATED, TOP GASKET EVEN ALL ROUND. JAM NUT TIGHT."),
                ("•", "STANDPIPE HOME; TOP AND SLOTS LEVEL WITH NEIGHBOURS ±1. DISCHARGE POINT\n±10."),
                ("•", "LEG HARD AGAINST THE COLLAR; BAND FULLY RECOVERED, NO LIFTED EDGE."),
                ("•", "CIRCULATION TEST: WEIR LEVEL MATCHES THE NEIGHBOURS. NO DRIP AT THE FLANGE,\nNUT OR LEG JOINT.")
            }, 280, 3, 146.4, 2.78, 2.9, 0.8);

            s.Text("6. LIMITS OF USE (PROVISIONAL)", 280, 124.25, 3.61, true);
            s.List(new[]
            {
                ("•", "FLOOR SOUND FROM Ø25 OUT TO Ø44 ON BOTH FACES. THINNED OR PITTED — USE\nDWG-DCBHD."),
                ("•", "PITCH 32 OR MORE; THE Ø40 NUTS CLEAR OF THE NEIGHBOURS' LEGS BELOW THE\nFLOOR."),
                ("•", "STANDPIPE OD AT THE SEAL BAND 18.85–19.15, OR BORE FEATURE A TO SUIT."),
                ("•", "ACCESS BELOW THE FLOOR FOR THE NUTS AND LEG (FINAL PACKING OUT LOCALLY).")
            }, 280, 3, 119.7, 2.78, 2.9, 0.8);

            s.Text("7. TO BE CONFIRMED", 280, 97.55, 3.61, true);
            s.List(new[]
            {
                ("•", "STANDPIPE OD AND WALL, FLOOR THICKNESS, PITCH, SLOT HEIGHTS — SITE OR OEM\nDRAWING."),
                ("•", "DESIGN FLOW PER DOWNCOMER THROUGH THE Ø12 BORE (58 % OF THE ORIGINAL\nAREA; THE LEG IS FULL BORE)."),
                ("•", "GLASS-FILLED PTFE GRADE, GASKET COMPRESSION AND CLAMP RETENTION HOT —\nACID SOAK ON A SCRAP OFFCUT."),
                ("•", "BARB PULL-OFF WITH WARMED 3/4\" PFA — BENCH TEST."),
                ("•", "CUTTER FOR THE TROUGH ALLOY; INTERNAL TUBE CUTTER FOR A 15.75 BORE; O-RING\nSUPPLY.")
            }, 280, 3, 93.0, 2.78, 2.9, 0.8);
        }
    }
}
