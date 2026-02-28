using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace Makale_Analizi
{
    public partial class Form1 : Form
    {
        private Graph? graph;
        private GraphImager? imager;
        private AnalyzerProducts? products;
        private Panel pnlStats;
        private Label lblStats;

        private float zoomScale = 1.0f;
        private PointF viewOffset = new PointF(0, 0);
        private bool isPanning = false;
        private Point mouseDownPos;

        public Form1()
        {
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.Text = "Citation Graph Analyzer - Kocaeli University";

            pnlStats = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };
            lblStats = new Label { Dock = DockStyle.Fill, Padding = new Padding(10), Font = new Font("Segoe UI", 9) };
            pnlStats.Controls.Add(lblStats);
            this.Controls.Add(pnlStats);

            this.Load += Form1_Load;

            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
        }

        private void Form1_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (graph == null) return;
            if (e.Delta > 0) zoomScale *= 1.1f; else zoomScale /= 1.1f;
            if (zoomScale < 0.1f) zoomScale = 0.1f;
            if (zoomScale > 10.0f) zoomScale = 10.0f;
            Invalidate();
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = true;
                mouseDownPos = e.Location;
            }
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
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
                if (imager != null)
                {
                    PointF worldPt = ScreenToWorld(e.Location);
                    imager.OnMouseMove(new Point((int)worldPt.X, (int)worldPt.Y), e.Location);
                    Invalidate();
                }
            }
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs e)
        {
            isPanning = false;

            if (imager == null || products == null || graph == null) return;

            PointF worldPt = ScreenToWorld(e.Location);
            Node? clicked = imager.GetNodeAt(new Point((int)worldPt.X, (int)worldPt.Y));

            if (clicked != null)
            {
                var citationCounts = products.DegreeMap.ToDictionary(k => k.Key, v => v.Value.inDegree);
                HCoreGraphForm hCoreForm = new HCoreGraphForm(clicked, graph, citationCounts);
                hCoreForm.Show();
            }
        }

        private PointF ScreenToWorld(Point screenPoint)
        {
            float wx = (screenPoint.X - viewOffset.X) / zoomScale;
            float wy = (screenPoint.Y - viewOffset.Y) / zoomScale;
            return new PointF(wx, wy);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "JSON|*.json" };
            if (dialog.ShowDialog() != DialogResult.OK) { Close(); return; }

            try
            {
                string jsonRead = File.ReadAllText(dialog.FileName);
                var docs = JsonDocumentParser.Parse(jsonRead);
                if (docs == null) return;

                graph = new Graph();
                var nodeDic = new Dictionary<string, Node>();

                foreach (var doc in docs)
                {
                    if (string.IsNullOrEmpty(doc.id)) continue;
                    string safeTitle = doc.title ?? "Baþlýksýz";
                    Node node = new Node(doc.id, safeTitle, doc.year);
                    graph.AddNode(node);
                    nodeDic[doc.id] = node;
                }

                foreach (var doc in docs)
                {
                    if (string.IsNullOrEmpty(doc.id) || !nodeDic.ContainsKey(doc.id) || doc.referenced_works == null) continue;
                    Node source = nodeDic[doc.id];
                    foreach (var targetId in doc.referenced_works)
                    {
                        if (targetId != null && nodeDic.ContainsKey(targetId))
                            graph.AddEdge(new Edge(source, nodeDic[targetId], EdgeType.Citation));
                    }
                }

                var sortedNodes = graph.Nodes.OrderBy(n => n.Id).ToList();
                for (int i = 0; i < sortedNodes.Count - 1; i++)
                    graph.AddEdge(new Edge(sortedNodes[i], sortedNodes[i + 1], EdgeType.Sequence));
                if (sortedNodes.Count > 1)
                    graph.AddEdge(new Edge(sortedNodes.Last(), sortedNodes.First(), EdgeType.Sequence));

                PerformAnalysis();
                UpdateStats();
                UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void PerformAnalysis()
        {
            if (graph == null) return;
            NodeDegreeAnalyzer da = new NodeDegreeAnalyzer(); da.Analyze(graph);
            BetweennessAnalyzer ba = new BetweennessAnalyzer(); ba.Analyze(graph);
            KCoreAnalyzer ka = new KCoreAnalyzer(); ka.Analyze(graph);
            products = new AnalyzerProducts(da.nodeDegreeMap, ba.betw, ka.kCore);
        }

        private void UpdateStats()
        {
            if (products == null || graph == null) return;
            var maxIn = products.DegreeMap.OrderByDescending(x => x.Value.inDegree).FirstOrDefault();
            var maxOut = products.DegreeMap.OrderByDescending(x => x.Value.outDegree).FirstOrDefault();
            int citationCount = graph.Edges.Count(e => e.Type == EdgeType.Citation);

            lblStats.Text = $"GENEL BÝLGÝLER\n\nMakale: {graph.Nodes.Count}\nAtýf: {citationCount}\n\n" +
                            $"En Çok Alan:\n{maxIn.Key?.Id}\n({maxIn.Value.inDegree})\n\n" +
                            $"En Çok Veren:\n{maxOut.Key?.Id}\n({maxOut.Value.outDegree})\n\n" +
                            "KONTROLLER:\nMouse Tekerlek: Zoom\nSol Týk Sürükle: Gez\nSol Týk: Detay";
        }

        private void UpdateLayout()
        {
            if (graph == null) return;
            var places = new Dictionary<Node, Point>();
            int r = 1000;
            var sortedList = graph.Nodes.OrderBy(n => n.Id).ToList();
            int count = sortedList.Count;

            for (int i = 0; i < count; i++)
            {
                double angle = 2 * Math.PI * i / count;
                int x = (int)(r * Math.Cos(angle));
                int y = (int)(r * Math.Sin(angle));
                places[sortedList[i]] = new Point(x, y);
            }
            imager = new GraphImager(places, products);

            viewOffset = new PointF(this.ClientSize.Width / 2 + pnlStats.Width / 2, this.ClientSize.Height / 2);
            zoomScale = 0.35f;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (imager != null && graph != null)
            {
                e.Graphics.TranslateTransform(viewOffset.X, viewOffset.Y);
                e.Graphics.ScaleTransform(zoomScale, zoomScale);
                imager.Draw(e.Graphics, graph);
                e.Graphics.ResetTransform();
            }
        }
    }
}