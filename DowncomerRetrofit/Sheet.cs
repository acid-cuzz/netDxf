using System;
using System.Collections.Generic;
using System.Linq;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using netDxf.Units;

namespace DowncomerRetrofit
{
    /// <summary>
    /// Section fill and hatch of one material. Spacing is in sheet mm at a hatch factor of 1.
    /// </summary>
    public sealed class Material
    {
        public AciColor Fill;
        public double Angle;
        public double Spacing;
        public bool Cross;
        public AciColor HatchColor;

        public static readonly AciColor HatchBlue = new AciColor(143, 169, 191);

        public static readonly Material Insert = new Material { Fill = new AciColor(243, 239, 228), Angle = 135, Spacing = 1.2, HatchColor = HatchBlue };
        public static readonly Material ClampNut = new Material { Fill = new AciColor(243, 239, 228), Angle = 45, Spacing = 1.2, HatchColor = HatchBlue };
        public static readonly Material JamNut = new Material { Fill = new AciColor(243, 239, 228), Angle = 135, Spacing = 2.0, HatchColor = HatchBlue };
        public static readonly Material Retainer = new Material { Fill = new AciColor(233, 223, 199), Angle = 45, Spacing = 1.0, HatchColor = HatchBlue };
        public static readonly Material Gasket = new Material { Fill = new AciColor(255, 241, 184) };
        public static readonly Material Floor = new Material { Fill = new AciColor(217, 217, 217), Angle = 45, Spacing = 1.5, HatchColor = new AciColor(96, 96, 96) };
        public static readonly Material Standpipe = new Material { Fill = new AciColor(207, 207, 207), Angle = 135, Spacing = 0.8, HatchColor = new AciColor(85, 85, 85) };
        public static readonly Material Tube = new Material { Fill = new AciColor(214, 234, 248), Angle = 45, Spacing = 1.4, Cross = true, HatchColor = new AciColor(127, 179, 213) };
        public static readonly Material Band = new Material { Fill = new AciColor(23, 165, 137) };
        public static readonly Material OringJacket = new Material { Fill = new AciColor(46, 134, 193) };
        public static readonly Material OringCore = new Material { Fill = new AciColor(232, 180, 184) };
        public static readonly Material PlanFloor = new Material { Fill = new AciColor(242, 242, 242) };
    }

    /// <summary>
    /// Maps part coordinates (r, z) in mm onto the sheet. Side -1 mirrors to the left of the axis.
    /// An optional circle clips everything drawn through the view (detail views).
    /// </summary>
    public sealed class View
    {
        public readonly Vector2 Origin;
        public readonly double Scale;
        public double HatchFactor = 1.0;
        public Lineweight? Outline;
        public Vector2? ClipCentre;
        public double ClipRadius;

        public View(double x, double y, double scale)
        {
            this.Origin = new Vector2(x, y);
            this.Scale = scale;
        }

        public Vector2 P(double r, double z, int side = 1)
        {
            return new Vector2(this.Origin.X + side * r * this.Scale, this.Origin.Y + z * this.Scale);
        }

        public Vector2 P(Vector2 rz, int side = 1)
        {
            return this.P(rz.X, rz.Y, side);
        }

        public List<Vector2> Map(IEnumerable<Vector2> rz, int side = 1)
        {
            return rz.Select(v => this.P(v, side)).ToList();
        }

        public double L(double length)
        {
            return length * this.Scale;
        }
    }

    /// <summary>
    /// An A3 landscape sheet drawn 1:1 in model space (1 unit = 1 mm of paper).
    /// Views are drawn at their own scale and the dimension styles carry the matching linear factor,
    /// so every dimension reads in true part size.
    /// </summary>
    public sealed class Sheet
    {
        public const double Width = 420.0;
        public const double Height = 297.0;

        // Helvetica/Arial cap height as a fraction of the font size
        private const double CapRatio = 0.70;

        public static readonly AciColor Red = new AciColor(176, 0, 0);
        public static readonly AciColor Grey = new AciColor(107, 107, 107);
        public static readonly AciColor Blue = new AciColor(46, 134, 193);

        public readonly DxfDocument Doc;
        public readonly TextStyle Regular;
        public readonly TextStyle Bold;
        public readonly Layer Border;
        public readonly Layer Visible;
        public readonly Layer Thin;
        public readonly Layer Hidden;
        public readonly Layer Center;
        public readonly Layer Hatching;
        public readonly Layer Fill;
        public readonly Layer Dims;
        public readonly Layer Notes;
        public readonly Layer Tables;

        private readonly Dictionary<double, DimensionStyle> dimStyles = new Dictionary<double, DimensionStyle>();

        public Sheet()
        {
            this.Doc = new DxfDocument(DxfVersion.AutoCad2013);
            this.Doc.BuildDimensionBlocks = true;
            this.Doc.DrawingVariables.InsUnits = DrawingUnits.Millimeters;
            this.Doc.DrawingVariables.LwDisplay = true;
            this.Doc.DrawingVariables.LtScale = 1.0;

            this.Regular = new TextStyle("DCINS", "arial.ttf");
            this.Bold = new TextStyle("DCINS-BOLD", "arialbd.ttf");

            Linetype center = new Linetype("DCINS_CENTER", new LinetypeSegment[]
            {
                new LinetypeSimpleSegment(8.0), new LinetypeSimpleSegment(-1.5),
                new LinetypeSimpleSegment(1.5), new LinetypeSimpleSegment(-1.5)
            }, "Centre line ____ _ ____ _ ____");
            Linetype hidden = new Linetype("DCINS_HIDDEN", new LinetypeSegment[]
            {
                new LinetypeSimpleSegment(2.0), new LinetypeSimpleSegment(-1.0)
            }, "Hidden line __ __ __ __");

            this.Border = this.AddLayer("BORDER", Lineweight.W70);
            this.Visible = this.AddLayer("VISIBLE", Lineweight.W35);
            this.Thin = this.AddLayer("THIN", Lineweight.W18);
            this.Hidden = this.AddLayer("HIDDEN", Lineweight.W18, hidden);
            this.Center = this.AddLayer("CENTER", Lineweight.W13, center);
            this.Hatching = this.AddLayer("HATCH", Lineweight.W13);
            this.Fill = this.AddLayer("FILL", Lineweight.W0);
            this.Dims = this.AddLayer("DIMENSIONS", Lineweight.W18);
            this.Notes = this.AddLayer("NOTES", Lineweight.W18);
            this.Tables = this.AddLayer("TABLES", Lineweight.W18);
        }

        private Layer AddLayer(string name, Lineweight lineweight, Linetype linetype = null)
        {
            Layer layer = new Layer(name) { Color = AciColor.Default, Lineweight = lineweight };
            if (linetype != null)
            {
                layer.Linetype = linetype;
            }
            return this.Doc.Layers.Add(layer);
        }

        public void Save(string file)
        {
            this.Doc.Save(file);
        }

        #region geometry

        public T Add<T>(T entity, Layer layer, AciColor color = null, Lineweight? lineweight = null) where T : EntityObject
        {
            entity.Layer = layer;
            if (color != null)
            {
                entity.Color = color;
            }
            if (lineweight.HasValue)
            {
                entity.Lineweight = lineweight.Value;
            }
            this.Doc.Entities.Add(entity);
            return entity;
        }

        public Line Line(Vector2 a, Vector2 b, Layer layer, AciColor color = null, Lineweight? lineweight = null)
        {
            return this.Add(new Line(a, b), layer, color, lineweight);
        }

        public Polyline2D Poly(IEnumerable<Vector2> points, bool closed, Layer layer, AciColor color = null, Lineweight? lineweight = null)
        {
            return this.Add(new Polyline2D(points, closed), layer, color, lineweight);
        }

        public Circle Circle(Vector2 centre, double radius, Layer layer, AciColor color = null, Lineweight? lineweight = null)
        {
            return this.Add(new Circle(centre, radius), layer, color, lineweight);
        }

        private void HatchLoops(IEnumerable<EntityObject> loops, HatchPattern pattern, AciColor color, Layer layer)
        {
            List<HatchBoundaryPath> paths = loops.Select(e => new HatchBoundaryPath(new[] { e })).ToList();
            this.Add(new Hatch(pattern, paths, false), layer, color);
        }

        /// <summary>
        /// Section lines stored with the hatch: a user-defined hatch (angle and spacing in mm) for single lines,
        /// a named custom pattern for cross-hatching. Neither depends on acad.pat or acadiso.pat.
        /// </summary>
        private HatchPattern Pattern(Material m, double factor)
        {
            List<HatchPatternLineDefinition> lines = new List<HatchPatternLineDefinition>
            {
                new HatchPatternLineDefinition { Angle = 0, Origin = Vector2.Zero, Delta = new Vector2(0, 1) }
            };
            if (m.Cross)
            {
                lines.Add(new HatchPatternLineDefinition { Angle = 90, Origin = Vector2.Zero, Delta = new Vector2(0, 1) });
            }
            return new HatchPattern(m.Cross ? "DCINS_CROSS" : "_USER", lines)
            {
                Type = m.Cross ? HatchType.Custom : HatchType.UserDefined,
                Angle = m.Angle,
                Scale = m.Spacing * factor
            };
        }

        /// <summary>
        /// Fills and hatches the given closed loops (holes are inner loops) with a material.
        /// </summary>
        public void FillLoops(IList<EntityObject> loops, Material m, double hatchFactor = 1.0)
        {
            if (m.Fill != null)
            {
                this.HatchLoops(loops, HatchPattern.Solid, m.Fill, this.Fill);
            }
            if (m.Spacing > 0)
            {
                this.HatchLoops(loops, this.Pattern(m, hatchFactor), m.HatchColor, this.Hatching);
            }
        }

        /// <summary>
        /// A sectioned part: fill, hatch and outline of a polygon given in part coordinates.
        /// </summary>
        public void Section(View v, IEnumerable<Vector2> rz, Material m, int side = 1, bool outline = true, Lineweight? lineweight = null)
        {
            List<Vector2> pts = v.Map(rz, side);
            List<Vector2> clipped = v.ClipCentre.HasValue ? ClipPolygon(pts, v.ClipCentre.Value, v.ClipRadius) : pts;
            if (clipped.Count >= 3)
            {
                this.FillLoops(new EntityObject[] { new Polyline2D(clipped, true) }, m, v.HatchFactor);
            }
            if (outline)
            {
                this.Outline(v, pts, true, this.Visible, lineweight ?? v.Outline);
            }
        }

        /// <summary>
        /// Draws a polyline already mapped to the sheet, clipped to the view's detail circle if it has one.
        /// </summary>
        public void Outline(View v, List<Vector2> pts, bool closed, Layer layer, Lineweight? lineweight = null, AciColor color = null)
        {
            if (!v.ClipCentre.HasValue)
            {
                this.Poly(pts, closed, layer, color, lineweight);
                return;
            }

            List<Vector2> run = null;
            int n = closed ? pts.Count : pts.Count - 1;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % pts.Count];
                if (!ClipSegment(a, b, v.ClipCentre.Value, v.ClipRadius, out Vector2 ca, out Vector2 cb))
                {
                    this.FlushRun(ref run, layer, lineweight, color);
                    continue;
                }
                if (run != null && Vector2.Distance(run[run.Count - 1], ca) < 1e-9)
                {
                    run.Add(cb);
                }
                else
                {
                    this.FlushRun(ref run, layer, lineweight, color);
                    run = new List<Vector2> { ca, cb };
                }
                if (Vector2.Distance(cb, b) > 1e-9)
                {
                    this.FlushRun(ref run, layer, lineweight, color);
                }
            }
            this.FlushRun(ref run, layer, lineweight, color);
        }

        private void FlushRun(ref List<Vector2> run, Layer layer, Lineweight? lineweight, AciColor color)
        {
            if (run != null && run.Count >= 2)
            {
                this.Poly(run, false, layer, color, lineweight);
            }
            run = null;
        }

        /// <summary>
        /// Sutherland-Hodgman clip of a polygon against a circle (approximated by a fine regular polygon).
        /// </summary>
        public static List<Vector2> ClipPolygon(List<Vector2> subject, Vector2 centre, double radius)
        {
            const int n = 360;
            List<Vector2> output = subject;
            for (int i = 0; i < n && output.Count > 0; i++)
            {
                double a0 = 2 * Math.PI * i / n;
                double a1 = 2 * Math.PI * (i + 1) / n;
                Vector2 e0 = centre + radius * new Vector2(Math.Cos(a0), Math.Sin(a0));
                Vector2 e1 = centre + radius * new Vector2(Math.Cos(a1), Math.Sin(a1));
                List<Vector2> input = output;
                output = new List<Vector2>();
                for (int j = 0; j < input.Count; j++)
                {
                    Vector2 cur = input[j];
                    Vector2 prev = input[(j + input.Count - 1) % input.Count];
                    bool curIn = Inside(cur, e0, e1);
                    bool prevIn = Inside(prev, e0, e1);
                    if (curIn)
                    {
                        if (!prevIn)
                        {
                            output.Add(Intersect(prev, cur, e0, e1));
                        }
                        output.Add(cur);
                    }
                    else if (prevIn)
                    {
                        output.Add(Intersect(prev, cur, e0, e1));
                    }
                }
            }
            return output;
        }

        private static bool Inside(Vector2 p, Vector2 e0, Vector2 e1)
        {
            return (e1.X - e0.X) * (p.Y - e0.Y) - (e1.Y - e0.Y) * (p.X - e0.X) >= 0;
        }

        private static Vector2 Intersect(Vector2 p0, Vector2 p1, Vector2 e0, Vector2 e1)
        {
            Vector2 d = p1 - p0;
            Vector2 e = e1 - e0;
            double den = d.X * e.Y - d.Y * e.X;
            double t = ((e0.X - p0.X) * e.Y - (e0.Y - p0.Y) * e.X) / den;
            return p0 + t * d;
        }

        /// <summary>
        /// Clips a segment to the inside of a circle.
        /// </summary>
        public static bool ClipSegment(Vector2 a, Vector2 b, Vector2 centre, double radius, out Vector2 ca, out Vector2 cb)
        {
            ca = a;
            cb = b;
            Vector2 d = b - a;
            Vector2 f = a - centre;
            double qa = Vector2.DotProduct(d, d);
            double qb = 2 * Vector2.DotProduct(f, d);
            double qc = Vector2.DotProduct(f, f) - radius * radius;
            if (qa < 1e-18)
            {
                return qc <= 0;
            }
            double disc = qb * qb - 4 * qa * qc;
            if (disc <= 0)
            {
                return false;
            }
            double s = Math.Sqrt(disc);
            double t0 = Math.Max(0.0, (-qb - s) / (2 * qa));
            double t1 = Math.Min(1.0, (-qb + s) / (2 * qa));
            if (t0 >= t1)
            {
                return false;
            }
            ca = a + t0 * d;
            cb = a + t1 * d;
            return true;
        }

        /// <summary>
        /// Break line between two points with a zig-zag in the middle (thin).
        /// </summary>
        public void BreakLine(Vector2 a, Vector2 b, double amplitude = 0.85)
        {
            Vector2 u = Vector2.Normalize(b - a);
            Vector2 n = new Vector2(-u.Y, u.X);
            Vector2 m = Vector2.MidPoint(a, b);
            double w = amplitude;
            this.Poly(new[]
            {
                a,
                m - w * u,
                m - 0.5 * w * u + w * n,
                m + 0.5 * w * u - w * n,
                m + w * u,
                b
            }, false, this.Thin);
        }

        public void CenterLine(Vector2 a, Vector2 b)
        {
            this.Line(a, b, this.Center);
        }

        /// <summary>
        /// Filled dot used as a leader terminator.
        /// </summary>
        public void Dot(Vector2 p, double radius = 0.45, AciColor color = null)
        {
            this.HatchLoops(new EntityObject[] { new Circle(p, radius) }, HatchPattern.Solid, color ?? AciColor.ByLayer, this.Notes);
        }

        public void Arrow(Vector2 tip, Vector2 from, double length = 2.6, double width = 0.9, Layer layer = null, AciColor color = null)
        {
            Vector2 u = Vector2.Normalize(tip - from);
            Vector2 n = new Vector2(-u.Y, u.X);
            Vector2 back = tip - length * u;
            Polyline2D head = new Polyline2D(new[] { tip, back + 0.5 * width * n, back - 0.5 * width * n }, true);
            this.HatchLoops(new EntityObject[] { head }, HatchPattern.Solid, color ?? AciColor.ByLayer, layer ?? this.Notes);
        }

        /// <summary>
        /// Leader from a note to a feature, ending in a dot on the feature.
        /// </summary>
        public void Leader(Vector2 from, Vector2 to)
        {
            this.Line(from, to, this.Notes);
            this.Dot(to);
        }

        /// <summary>
        /// Item balloon with leaders to one or more features.
        /// </summary>
        public void Balloon(string label, Vector2 centre, params Vector2[] targets)
        {
            const double r = 2.89;
            this.Circle(centre, r, this.Notes, null, Lineweight.W25);
            this.Text(label, centre.X, centre.Y, 3.61, true, TextAlignment.MiddleCenter);
            foreach (Vector2 t in targets)
            {
                Vector2 start = centre + r * Vector2.Normalize(t - centre);
                this.Leader(start, t);
            }
        }

        /// <summary>
        /// Feature letter in a tinted circle with a leader to the feature.
        /// </summary>
        public void FeatureTag(string label, Vector2 centre, Vector2 target)
        {
            const double r = 2.45;
            Circle c = new Circle(centre, r);
            this.HatchLoops(new EntityObject[] { c }, HatchPattern.Solid, Material.Gasket.Fill, this.Fill);
            this.Circle(centre, r, this.Notes, null, Lineweight.W25);
            this.Text(label, centre.X, centre.Y, 3.19, true, TextAlignment.MiddleCenter);
            Vector2 start = centre + r * Vector2.Normalize(target - centre);
            this.Leader(start, target);
        }

        #endregion

        #region text

        public static double TextHeight(double fontSize)
        {
            return fontSize * CapRatio;
        }

        public Text Text(string value, double x, double y, double fontSize, bool bold = false,
            TextAlignment alignment = TextAlignment.BaselineLeft, AciColor color = null, double rotation = 0, Layer layer = null)
        {
            Text text = new Text(value, new Vector2(x, y), TextHeight(fontSize), bold ? this.Bold : this.Regular)
            {
                Alignment = alignment,
                Rotation = rotation
            };
            return this.Add(text, layer ?? this.Notes, color);
        }

        /// <summary>
        /// Multi-line text with the line breaks of the original; y is the first baseline, pitch the baseline spacing.
        /// </summary>
        public MText Para(string text, double x, double y, double fontSize, double pitch, bool bold = false,
            AciColor color = null, Layer layer = null, MTextAttachmentPoint attachment = MTextAttachmentPoint.TopLeft)
        {
            double h = TextHeight(fontSize);
            string[] lines = text.Split('\n');
            MText m = new MText(string.Join("\\P", lines.Select(EscapeMText)), new Vector2(x, y + h), h, 0.0, bold ? this.Bold : this.Regular)
            {
                AttachmentPoint = attachment,
                LineSpacingStyle = MTextLineSpacingStyle.Exact,
                LineSpacingFactor = pitch / (h * 5.0 / 3.0)
            };
            return this.Add(m, layer ?? this.Notes, color);
        }

        private static string EscapeMText(string s)
        {
            return s.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}");
        }

        /// <summary>
        /// Numbered or bulleted list: marker at x, text at x + indent; items separated by gap, lines by pitch.
        /// Returns the baseline below the last line.
        /// </summary>
        public double List(IEnumerable<(string Marker, string Text)> items, double x, double indent, double y, double fontSize,
            double pitch, double gap, bool boldMarker = true)
        {
            foreach ((string marker, string body) in items)
            {
                this.Text(marker, x, y, fontSize, boldMarker && marker != "•");
                this.Para(body, x + indent, y, fontSize, pitch);
                y -= pitch * body.Split('\n').Length + gap;
            }
            return y;
        }

        /// <summary>
        /// Underlined view title with a scale / material line below, centred on x.
        /// </summary>
        public void ViewTitle(string title, string sub, double x, double y)
        {
            this.Text(title, x, y, 4.44, true, TextAlignment.BaselineCenter);
            double w = TextWidth(title, 4.44, true);
            this.Line(new Vector2(x - w / 2, y - 1.15), new Vector2(x + w / 2, y - 1.15), this.Notes, null, Lineweight.W30);
            if (sub != null)
            {
                this.Text(sub, x, y - 5.2, 3.19, false, TextAlignment.BaselineCenter);
            }
        }

        /// <summary>
        /// Advance width of upper-case annotation text in Arial / Helvetica (bold or regular), used for underlines.
        /// </summary>
        public static double TextWidth(string s, double fontSize, bool bold)
        {
            const string wide = "MW";
            double w = 0;
            foreach (char c in s)
            {
                if (c == ' ' || c == 'I' || c == '.' || c == ',') w += 0.278;
                else if (c == '(' || c == ')' || c == '-' || c == ':') w += 0.333;
                else if (c == '\u2014') w += 1.0;
                else if (char.IsDigit(c)) w += 0.556;
                else if (wide.IndexOf(c) >= 0) w += c == 'W' ? 0.944 : 0.833;
                else if ("CDHNRUK&".IndexOf(c) >= 0) w += 0.722;
                else if ("OQG".IndexOf(c) >= 0) w += 0.778;
                else if ("FTZ".IndexOf(c) >= 0) w += 0.611;
                else if (c == 'L') w += bold ? 0.611 : 0.556;
                else if (c == 'J') w += bold ? 0.556 : 0.5;
                else if (c == 'A' || c == 'B') w += bold ? 0.722 : 0.667;
                else w += 0.667;
            }
            return w * TextHeight(fontSize) / 0.716;
        }

        #endregion

        #region dimensions

        public DimensionStyle DimStyle(double viewScale)
        {
            if (this.dimStyles.TryGetValue(viewScale, out DimensionStyle style))
            {
                return style;
            }
            style = new DimensionStyle("DCINS-" + (viewScale >= 1 ? viewScale.ToString("0.##") + "TO1" : "1TO" + (1 / viewScale).ToString("0.##")))
            {
                TextStyle = this.Regular,
                TextHeight = TextHeight(2.64),
                ArrowSize = 2.4,
                ExtLineOffset = 0.8,
                ExtLineExtend = 1.2,
                TextOffset = 0.7,
                TextInsideAlign = true,
                TextOutsideAlign = true,
                TextVerticalPlacement = DimensionStyleTextVerticalPlacement.Above,
                LengthPrecision = 2,
                SuppressLinearTrailingZeros = true,
                DecimalSeparator = '.',
                DimScaleLinear = 1.0 / viewScale,
                DimLineColor = AciColor.ByLayer,
                ExtLineColor = AciColor.ByLayer,
                TextColor = AciColor.ByLayer
            };
            this.dimStyles.Add(viewScale, style);
            return this.Doc.DimensionStyles.Add(style);
        }

        /// <summary>
        /// Linear dimension between two sheet points with the dimension line through 'at'.
        /// Rotation 0 measures horizontally, 90 vertically.
        /// </summary>
        public LinearDimension Dim(View v, Vector2 p1, Vector2 p2, Vector2 at, double rotation, string userText = null)
        {
            LinearDimension d = new LinearDimension(p1, p2, 1.0, rotation, this.DimStyle(v.Scale));
            d.SetDimensionLinePosition(at);
            if (userText != null)
            {
                d.UserText = userText;
            }
            return this.Add(d, this.Dims);
        }

        #endregion

        #region tables and title block

        /// <summary>
        /// Ruled table. cells[row][col] use '\n' for line breaks; row 0 is the bold header.
        /// </summary>
        public void Table(double[] xs, double[] ys, string[][] cells, double fontSize, double pitch,
            bool[] boldColumns = null, IDictionary<int, AciColor> rowFill = null, ISet<(int, int)> boldCells = null, double pad = 0.8, double firstLine = 3.2)
        {
            if (rowFill != null)
            {
                foreach (KeyValuePair<int, AciColor> f in rowFill)
                {
                    Polyline2D band = new Polyline2D(new[]
                    {
                        new Vector2(xs[0], ys[f.Key]), new Vector2(xs[xs.Length - 1], ys[f.Key]),
                        new Vector2(xs[xs.Length - 1], ys[f.Key + 1]), new Vector2(xs[0], ys[f.Key + 1])
                    }, true);
                    this.HatchLoops(new EntityObject[] { band }, HatchPattern.Solid, f.Value, this.Fill);
                }
            }

            for (int i = 0; i < ys.Length; i++)
            {
                bool edge = i == 0 || i == ys.Length - 1;
                this.Line(new Vector2(xs[0], ys[i]), new Vector2(xs[xs.Length - 1], ys[i]), this.Tables, null, edge ? Lineweight.W35 : Lineweight.W18);
            }
            for (int j = 0; j < xs.Length; j++)
            {
                bool edge = j == 0 || j == xs.Length - 1;
                this.Line(new Vector2(xs[j], ys[0]), new Vector2(xs[j], ys[ys.Length - 1]), this.Tables, null, edge ? Lineweight.W35 : Lineweight.W18);
            }

            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    string s = cells[i][j];
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    bool bold = i == 0 || (boldColumns != null && boldColumns[j]) || (boldCells != null && boldCells.Contains((i, j)));
                    double y = i == 0 ? (ys[0] + ys[1]) / 2 - TextHeight(fontSize) / 2 : ys[i] - firstLine;
                    this.Para(s, xs[j] + pad, y, fontSize, pitch, bold, null, this.Tables);
                }
            }
        }

        public void Frame()
        {
            this.Poly(new[] { new Vector2(10, 10), new Vector2(410, 10), new Vector2(410, 287), new Vector2(10, 287) }, true, this.Border);
        }

        /// <summary>
        /// Common border and title block of the DWG-DCINS set.
        /// </summary>
        public void TitleBlock(string[] notes, string subtitle, string drawingNo, string scale, string sheetNo)
        {
            this.Frame();
            this.Line(new Vector2(10, 38), new Vector2(410, 38), this.Border, null, Lineweight.W50);
            foreach (double x in new[] { 150.0, 212.0, 322.0 })
            {
                this.Line(new Vector2(x, 10), new Vector2(x, 38), this.Border, null, Lineweight.W35);
            }
            this.Line(new Vector2(322, 28.67), new Vector2(410, 28.67), this.Border, null, Lineweight.W25);
            this.Line(new Vector2(322, 19.33), new Vector2(410, 19.33), this.Border, null, Lineweight.W25);
            this.Line(new Vector2(366, 10), new Vector2(366, 38), this.Border, null, Lineweight.W25);

            // general notes
            this.Text("GENERAL NOTES", 12, 34.2, 3.19, true);
            double y = 30.2;
            for (int i = 0; i < notes.Length; i++)
            {
                this.Text((i + 1) + ".", 12, y, 2.64, true);
                this.Para(notes[i], 18, y, 2.64, 2.75);
                y -= 2.75 * notes[i].Split('\n').Length + 0.3;
            }

            // confidentiality
            this.Text("CONFIDENTIAL", 152, 34.15, 3.61, true, TextAlignment.BaselineLeft, Red);
            this.Para("© 2026 J. YOUNG. THIS DRAWING AND THE\nDESIGN SHOWN ARE CONFIDENTIAL. NOT\nTO BE COPIED, DISCLOSED OR USED\nWITHOUT WRITTEN PERMISSION.", 152, 29.4, 2.64, 2.75);

            // title
            this.Text("TITLE", 214, 34.2, 2.5, false, TextAlignment.BaselineLeft, Grey);
            this.Text("DOWNCOMER RETROFIT INSERT", 267, 26.45, 4.72, true, TextAlignment.BaselineCenter, Red);
            this.Text(subtitle, 267, 20.2, 3.47, false, TextAlignment.BaselineCenter, Red);
            this.Text("CONCEPT — NOT FOR CONSTRUCTION", 267, 14.4, 3.06, false, TextAlignment.BaselineCenter, Red);

            // boxes
            this.Box("DESIGNED", "J. YOUNG", 323.5, 28.67);
            this.Box("DATE", "26/09/2026", 367.5, 28.67);
            this.Box("DRAWING No.", drawingNo, 323.5, 19.33);
            this.Box("SCALE", scale, 367.5, 19.33);
            this.Box("SHEET", sheetNo, 323.5, 10.0);
            this.Box("REV", "B", 367.5, 10.0);
        }

        private void Box(string label, string value, double x, double bottom)
        {
            this.Text(label, x, bottom + 6.1, 2.22, false, TextAlignment.BaselineLeft, Grey);
            this.Text(value, x, bottom + 1.75, 3.75, true, TextAlignment.BaselineLeft, Red);
        }

        #endregion
    }
}
