using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Linq;

namespace Makale_Analizi
{
    internal class GraphImager
    {
        private readonly Size LegendSize = new Size(240, 130);
        private const int MinRadius = 4;
        private const int MaxRadius = 14;
        private const int DefaultDetailRadius = 25;

        private Dictionary<Node, Point> nodePlaces;
        private AnalyzerProducts? products;

        private Node? hoveredNode = null;
        private Point currentMouseScreenPos;
        public Dictionary<Node, Color>? CustomNodeColors { get; set; }
        public Dictionary<Edge, Color>? CustomEdgeColors { get; set; }

        public GraphImager(Dictionary<Node, Point> nodePlaces, AnalyzerProducts? products)
        {
            this.nodePlaces = nodePlaces;
            this.products = products;
        }

        public void OnMouseMove(Point worldPos, Point screenPos)
        {
            hoveredNode = null;
            currentMouseScreenPos = screenPos;

            foreach (var pair in nodePlaces)
            {
                int r = GetNodeRadius(pair.Key);
                if (Math.Pow(worldPos.X - pair.Value.X, 2) + Math.Pow(worldPos.Y - pair.Value.Y, 2) <= (r + 5) * (r + 5))
                {
                    hoveredNode = pair.Key;
                    break;
                }
            }
        }

        public Node? GetNodeAt(Point worldPos)
        {
            foreach (var pair in nodePlaces)
            {
                int r = GetNodeRadius(pair.Key);
                if (Math.Pow(worldPos.X - pair.Value.X, 2) + Math.Pow(worldPos.Y - pair.Value.Y, 2) <= (r + 5) * (r + 5))
                    return pair.Key;
            }
            return null;
        }

        public void Draw(Graphics g, Graph graph)
        {
            if (graph == null || graph.Nodes.Count == 0) return;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (AdjustableArrowCap arrowCap = new AdjustableArrowCap(4, 4, true))
            {
                Pen defaultPen;
                if (products == null) defaultPen = new Pen(Color.Black, 1.5f);
                else defaultPen = new Pen(Color.FromArgb(60, 0, 0, 0), 1);

                using (defaultPen)
                {
                    defaultPen.CustomEndCap = arrowCap;

                    foreach (var edge in graph.Edges.Where(e => e.Type == EdgeType.Citation))
                    {
                        if (!nodePlaces.ContainsKey(edge.Source) || !nodePlaces.ContainsKey(edge.Target)) continue;

                        Point p1 = nodePlaces[edge.Source];
                        Point p2 = nodePlaces[edge.Target];
                        int r = GetNodeRadius(edge.Target);

                        if (CustomEdgeColors != null && CustomEdgeColors.ContainsKey(edge))
                        {
                            using (Pen customPen = new Pen(CustomEdgeColors[edge], 2))
                            {
                                customPen.CustomEndCap = arrowCap;
                                DrawArrow(g, customPen, p1, p2, r);
                            }
                        }
                        else
                        {
                            DrawArrow(g, defaultPen, p1, p2, r);
                        }
                    }
                }
            }

            using (Pen greenPen = new Pen(Color.DarkGreen, 4))
            using (AdjustableArrowCap greenCap = new AdjustableArrowCap(5, 5, true)) // Ok ucu büyük
            {
                greenPen.CustomEndCap = greenCap;

                foreach (var edge in graph.Edges.Where(e => e.Type == EdgeType.Sequence))
                {
                    if (nodePlaces.ContainsKey(edge.Source) && nodePlaces.ContainsKey(edge.Target))
                    {
                        Point p1 = nodePlaces[edge.Source];
                        Point p2 = nodePlaces[edge.Target];

                        DrawArrow(g, greenPen, p1, p2, GetNodeRadius(edge.Target));
                    }
                }
            }

            using (Pen borderPen = new Pen(Color.DimGray, 1))
            using (Pen hoverPen = new Pen(Color.Black, 2))
            {
                foreach (var pair in nodePlaces)
                {
                    Node node = pair.Key;
                    Point p = pair.Value;
                    int r = GetNodeRadius(node);
                    bool isHovered = (node == hoveredNode);

                    Brush bgBrush;
                    if (isHovered)
                        bgBrush = Brushes.Orange;
                    else if (CustomNodeColors != null && CustomNodeColors.ContainsKey(node))
                        bgBrush = new SolidBrush(CustomNodeColors[node]);
                    else
                        bgBrush = CreateNodeBrush(node);

                    g.FillEllipse(bgBrush, p.X - r, p.Y - r, r * 2, r * 2);
                    g.DrawEllipse(isHovered ? hoverPen : borderPen, p.X - r, p.Y - r, r * 2, r * 2);

                    if (isHovered || products == null)
                    {
                        DrawNodeLabel(g, node, p, r);
                    }

                    if (!isHovered && bgBrush is IDisposable d && bgBrush != Brushes.Orange)
                        d.Dispose();
                }
            }

            var originalState = g.Save();
            g.ResetTransform();

            if (products != null) DrawLegend(g);
            if (hoveredNode != null) DrawInfoCard(g, hoveredNode);

            g.Restore(originalState);
        }

        private void DrawArrow(Graphics g, Pen pen, Point from, Point to, int targetRadius)
        {
            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
            Point adjustedTo = new Point(
                (int)(to.X - (targetRadius + 3) * Math.Cos(angle)),
                (int)(to.Y - (targetRadius + 3) * Math.Sin(angle))
            );
            g.DrawLine(pen, from, adjustedTo);
        }

        private void DrawNodeLabel(Graphics g, Node node, Point center, int radius)
        {
            string label = node.Id.Length > 4 ? "..." + node.Id.Substring(node.Id.Length - 4) : node.Id;
            using (Font f = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                SizeF size = g.MeasureString(label, f);
                RectangleF rect = new RectangleF(center.X - size.Width / 2, center.Y - radius - size.Height - 2, size.Width, size.Height);
                g.FillRectangle(Brushes.WhiteSmoke, rect);
                g.DrawString(label, f, Brushes.Black, rect.X, rect.Y);
            }
        }

        private Brush CreateNodeBrush(Node node)
        {
            if (products == null) return new SolidBrush(Color.LightGray);

            if (!products.KCoreMap.TryGetValue(node, out int k))
                return new SolidBrush(Color.LightBlue);

            int maxK = products.KCoreMap.Values.Count > 0 ? products.KCoreMap.Values.Max() : 1;
            if (maxK == 0) maxK = 1;

            double ratio = (double)k / maxK;
            int r = (int)(173 - (173 * ratio));
            int g = (int)(216 - (216 * ratio));
            int b = (int)(230 - (91 * ratio));

            return new SolidBrush(Color.FromArgb(Math.Max(0, r), Math.Max(0, g), Math.Max(0, b)));
        }

        private int GetNodeRadius(Node node)
        {
            if (products == null) return DefaultDetailRadius;

            if (!products.BetweennessMap.TryGetValue(node, out double bet)) return MinRadius;
            int r = MinRadius + (int)((MaxRadius - MinRadius) * bet * 15);
            return Math.Min(MaxRadius, Math.Max(MinRadius, r));
        }

        private void DrawInfoCard(Graphics g, Node node)
        {
            int x = currentMouseScreenPos.X + 15;
            int y = currentMouseScreenPos.Y + 15;

            var degree = (products != null && products.DegreeMap.TryGetValue(node, out var d)) ? d : (0, 0);
            double bet = (products != null && products.BetweennessMap.TryGetValue(node, out var b)) ? b : 0.0;
            int kcore = (products != null && products.KCoreMap.TryGetValue(node, out var k)) ? k : 0;

            string info = $"ID: {node.Id}\n" +
                          $"Başlık: {TruncateString(node.Title, 40)}\n" +
                          $"Yıl: {node.Year}\n" +
                          $"Gelen: {degree.Item1} | Çıkan: {degree.Item2}\n" +
                          $"Betw: {bet:F5} | K-Core: {kcore}";

            using (Font f = new Font("Consolas", 9))
            {
                SizeF size = g.MeasureString(info, f);
                Rectangle rect = new Rectangle(x, y, (int)size.Width + 10, (int)size.Height + 10);

                if (rect.Right > g.VisibleClipBounds.Right) rect.X -= (rect.Width + 30);
                if (rect.Bottom > g.VisibleClipBounds.Bottom) rect.Y -= (rect.Height + 30);

                g.FillRectangle(Brushes.White, rect);
                g.DrawRectangle(Pens.Black, rect);
                g.DrawString(info, f, Brushes.Black, rect.X + 5, rect.Y + 5);
            }
        }

        private void DrawLegend(Graphics g)
        {
            Rectangle area = new Rectangle((int)g.VisibleClipBounds.Width - LegendSize.Width - 20, 20, LegendSize.Width, LegendSize.Height);

            using (Brush bg = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                g.FillRectangle(bg, area);
            g.DrawRectangle(Pens.Gray, area);

            int x = area.X + 10, y = area.Y + 10;
            g.DrawString("LEJANT", SystemFonts.DefaultFont, Brushes.Black, x, y);
            y += 25;

            using (Pen p = new Pen(Color.DarkGreen, 3))
            using (AdjustableArrowCap c = new AdjustableArrowCap(4, 4, true))
            {
                p.CustomEndCap = c;
                g.DrawLine(p, x, y + 5, x + 30, y + 5);
            }
            g.DrawString("ID Sırası (Yeşil Ok)", SystemFonts.DefaultFont, Brushes.Black, x + 35, y);
            y += 25;

            using (Pen p = new Pen(Color.Black, 1))
            {
                using (AdjustableArrowCap c = new AdjustableArrowCap(3, 3)) { p.CustomEndCap = c; g.DrawLine(p, x, y + 5, x + 30, y + 5); }
            }
            g.DrawString("Atıf (Referans)", SystemFonts.DefaultFont, Brushes.Black, x + 35, y);
            y += 25;

            using (Brush b = new SolidBrush(Color.Orange)) g.FillEllipse(b, x + 10, y, 10, 10);
            g.DrawString("Seçili (Hover)", SystemFonts.DefaultFont, Brushes.Black, x + 35, y - 2);
        }

        private string TruncateString(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, max - 3) + "...";
        }
    }
}