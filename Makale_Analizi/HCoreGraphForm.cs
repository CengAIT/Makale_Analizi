using Makale_Analizi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Makale_Analizi
{
    internal partial class HCoreGraphForm : Form
    {
        private Graph displayGraph;
        private Graph masterGraph;
        private Dictionary<Node, int> masterCitationCounts;
        private GraphImager? imager;
        private Label lblStats;
        private Node rootNode;

        private float zoomScale = 1.0f;
        private PointF viewOffset = new PointF(0, 0);
        private bool isPanning = false;
        private Point mouseDownPos;

        public HCoreGraphForm(Node root, Graph fullGraph, Dictionary<Node, int> citations)
        {
            this.Text = "H-Core Analiz Penceresi (Zoom: Tekerlek | Gez: Sürükle | Genişlet: Tıkla)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 900);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            masterGraph = fullGraph;
            masterCitationCounts = citations;
            displayGraph = new Graph();
            rootNode = root;
            displayGraph.AddNode(root);

            lblStats = new Label
            {
                AutoSize = true,
                Location = new Point(10, 10),
                BackColor = Color.LemonChiffon,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10)
            };
            this.Controls.Add(lblStats);

            ExpandNode(root);

            this.Paint += HCoreGraphForm_Paint;
            this.MouseWheel += HCoreGraphForm_MouseWheel;
            this.MouseDown += HCoreGraphForm_MouseDown;
            this.MouseMove += HCoreGraphForm_MouseMove;
            this.MouseUp += HCoreGraphForm_MouseUp;
        }

        private void HCoreGraphForm_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta > 0) zoomScale *= 1.1f; else zoomScale /= 1.1f;

            if (zoomScale < 0.1f) zoomScale = 0.1f;
            if (zoomScale > 10.0f) zoomScale = 10.0f;

            Invalidate();
        }

        private void HCoreGraphForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = true;
                mouseDownPos = e.Location;
            }
        }

        private void HCoreGraphForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                float dx = (e.X - mouseDownPos.X) / zoomScale;
                float dy = (e.Y - mouseDownPos.Y) / zoomScale;
                viewOffset = new PointF(viewOffset.X + dx, viewOffset.Y + dy);
                mouseDownPos = e.Location;
                Invalidate();
            }
            else
            {
                if (imager == null) return;

                PointF worldPt = ScreenToWorld(e.Location);

                imager.OnMouseMove(new Point((int)worldPt.X, (int)worldPt.Y), e.Location);

                this.Invalidate();
            }
        }

        private void HCoreGraphForm_MouseUp(object? sender, MouseEventArgs e)
        {
            isPanning = false;

            if (imager == null) return;

            PointF worldPt = ScreenToWorld(e.Location);
            var n = imager.GetNodeAt(new Point((int)worldPt.X, (int)worldPt.Y));

            if (n != null) ExpandNode(n);
        }

        private void HCoreGraphForm_Paint(object? sender, PaintEventArgs e)
        {
            if (imager != null)
            {
                e.Graphics.TranslateTransform(viewOffset.X, viewOffset.Y);
                e.Graphics.ScaleTransform(zoomScale, zoomScale);

                imager.Draw(e.Graphics, displayGraph);

                e.Graphics.ResetTransform();
            }
        }

        private PointF ScreenToWorld(Point screenPoint)
        {
            float wx = (screenPoint.X - viewOffset.X) / zoomScale;
            float wy = (screenPoint.Y - viewOffset.Y) / zoomScale;
            return new PointF(wx, wy);
        }

        private void ExpandNode(Node center)
        {
            var res = HIndexCalculator.Calculate(center, masterGraph, masterCitationCounts);
            lblStats.Text = $"Seçili: {center.Id}\nH-Index: {res.HIndex}\nH-Median: {res.HMedian}\nCore: {res.HCoreNodes.Count}\n(Genişletmek için tıklayın)";

            foreach (var node in res.HCoreNodes)
                if (!displayGraph.Nodes.Contains(node)) displayGraph.AddNode(node);

            foreach (var n1 in displayGraph.Nodes)
                foreach (var n2 in displayGraph.Nodes)
                {
                    if (n1 == n2) continue;
                    if (masterGraph.Edges.Any(e => e.Source == n1 && e.Target == n2 && e.Type == EdgeType.Citation))
                        if (!displayGraph.Edges.Any(e => e.Source == n1 && e.Target == n2))
                            displayGraph.AddEdge(new Edge(n1, n2, EdgeType.Citation));
                }

            RecalculateLayout(res.HCoreNodes, center);
            this.Invalidate();
        }

        private void RecalculateLayout(List<Node> currentCore, Node currentCenter)
        {
            Dictionary<Node, Point> places = new Dictionary<Node, Point>();
            Dictionary<Node, Color> nodeColors = new Dictionary<Node, Color>();
            Dictionary<Edge, Color> edgeColors = new Dictionary<Edge, Color>();

            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            if (displayGraph.Nodes.Count > 0)
            {
                places[rootNode] = new Point(cx, cy);
                nodeColors[rootNode] = Color.LightPink;

                var otherNodes = displayGraph.Nodes.Where(n => n != rootNode).ToList();
                for (int i = 0; i < otherNodes.Count; i++)
                {
                    Node n = otherNodes[i];
                    double angle = i * 0.9;
                    int r = 150 + (i * 50);
                    places[n] = new Point(cx + (int)(r * Math.Cos(angle)), cy + (int)(r * Math.Sin(angle)));

                    if (currentCore.Contains(n)) nodeColors[n] = Color.LemonChiffon;
                    else if (n == currentCenter) nodeColors[n] = Color.Salmon;
                    else nodeColors[n] = Color.LightGreen;
                }

                foreach (var edge in displayGraph.Edges)
                {
                    if (edge.Source == currentCenter || edge.Target == currentCenter)
                        edgeColors[edge] = Color.Black;
                    else
                        edgeColors[edge] = Color.Red;
                }
            }

            imager = new GraphImager(places, null);
            imager.CustomNodeColors = nodeColors;
            imager.CustomEdgeColors = edgeColors;
        }
    }
}