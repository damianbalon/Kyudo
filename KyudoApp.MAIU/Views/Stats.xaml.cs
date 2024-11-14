using KyudoApp.MAIU.Model;
using SQLite;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Syncfusion.Maui.Calendar;

namespace KyudoApp.MAIU.Views
{
    public partial class Stats : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        private List<PointF> heatmapPoints = new();
        private DateTime day_;
        private bool treningheatmap = false;
        private Trening _trening;

        public List<ChartDataItem> ChartData { get; set; }
        public bool IsCalendarVisible { get; set; } = true;

        public Stats(DateTime date, SQLiteAsyncConnection database)
        {
            day_ = date;
            InitializeComponent();
            _database = database;
            BindingContext = this;

            LoadHeatmapData();
        }

        public Stats(Trening trening, SQLiteAsyncConnection database)
        {
            IsCalendarVisible = false;
            treningheatmap = true;
            _trening = trening;
            day_ = trening.Data;
            InitializeComponent();
            _database = database;
            BindingContext = this;

            this.ForceLayout();

            LoadHeatmapData();
        }

        private void SetTable()
        {
            Strzal.Text = heatmapPoints.Count.ToString();

            int trafione = heatmapPoints.Count(p => new Wynik { X = p.X, Y = p.Y }.CzyTrafiony);
            int nietrafione = heatmapPoints.Count - trafione;

            Trafione.Text = trafione.ToString();
            Podlo.Text = nietrafione.ToString();
        }

        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () =>
            {
                Navigation.RemovePage(Navigation.NavigationStack.Last());
            });

            return true;
        }

        private async Task LoadHeatmapData(DateTime? startDate = null, DateTime? endDate = null)
        {
            var results = await _database.Table<Wynik>().ToListAsync();

            if (!treningheatmap)
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    results = results.Where(r => r.Data >= startDate.Value && r.Data <= endDate.Value).ToList();
                }
            }
            else
            {
                results = results.Where(r => r.Data.Date == day_.Date).ToList();
            }

            var groupedResults = results.GroupBy(r => r.Data.Date)
                                        .Where(group => group.Any())
                                        .OrderBy(group => group.Key);

            heatmapPoints = results.Select(r => new PointF((float)r.X, (float)r.Y)).ToList();
            SetTable();

            ChartData = groupedResults.Select(g => new ChartDataItem
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                HitPercentage = (double)g.Count(p => new Wynik { X = p.X, Y = p.Y }.CzyTrafiony) / g.Count() * 100
            }).ToList();

            OnPropertyChanged(nameof(ChartData));

            HeatmapOverlay.Drawable = new HeatmapDrawable(heatmapPoints, results.Count);
            HeatmapOverlay.Invalidate();
        }

        private async void OnBackToTrainingClicked(object sender, EventArgs e)
        {
            Navigation.RemovePage(Navigation.NavigationStack.Last());
        }

        private async void CalendarStats_SelectionChanged(object sender, Syncfusion.Maui.Calendar.CalendarSelectionChangedEventArgs e)
        {
            var rangeEnd = CalendarStats.SelectedDateRange.EndDate;
            var rangeStart = CalendarStats.SelectedDateRange.StartDate;

            await LoadHeatmapData(rangeStart, rangeEnd);
        }
    }

    public class ChartDataItem
    {
        public string Date { get; set; }
        public double HitPercentage { get; set; }
    }

    public class HeatmapDrawable : IDrawable
    {
        private readonly List<PointF> points;
        private readonly int totalPoints;
        private const float BasePointRadius = 5;
        private const float NeighborRadius = 30;
        private const int LayerCount = 15;

        public HeatmapDrawable(List<PointF> points, int totalPoints)
        {
            this.points = points;
            this.totalPoints = totalPoints;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            DrawMato(canvas, dirtyRect);

            foreach (var point in points)
            {
                int neighborCount = points.Count(p => Distance(p, point) <= NeighborRadius);
                float intensityFactor = Math.Min(1f, neighborCount / (float)Math.Max(totalPoints, 1));

                for (int i = 0; i < LayerCount; i++)
                {
                    float alphaFactor = intensityFactor * (0.5f - (i * 0.03f));
                    float radius = BasePointRadius + (i * 2);
                    var color = GetColorForIntensity(alphaFactor);
                    canvas.FillColor = color;
                    canvas.FillCircle(point.X, point.Y, radius);
                }
            }
        }

        private void DrawMato(ICanvas canvas, RectF dirtyRect)
        {
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;

            float nakashiroRadius = 12.5f / 1.5f;
            float ichiNoKuroRadius = nakashiroRadius + 12.5f / 1.5f;
            float niNoShiroRadius = ichiNoKuroRadius + 30f / 1.5f;
            float niNoKuroRadius = niNoShiroRadius + 15f / 1.5f;
            float sanNoShiroRadius = niNoKuroRadius + 30f / 1.5f;
            float sotoKuroRadius = 125f / 1.5f;

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - sotoKuroRadius, centerY - sotoKuroRadius, sotoKuroRadius * 2, sotoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - sanNoShiroRadius, centerY - sanNoShiroRadius, sanNoShiroRadius * 2, sanNoShiroRadius * 2);

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - niNoKuroRadius, centerY - niNoKuroRadius, niNoKuroRadius * 2, niNoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - niNoShiroRadius, centerY - niNoShiroRadius, niNoShiroRadius * 2, niNoShiroRadius * 2);

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(centerX - ichiNoKuroRadius, centerY - ichiNoKuroRadius, ichiNoKuroRadius * 2, ichiNoKuroRadius * 2);

            canvas.FillColor = Colors.White;
            canvas.FillEllipse(centerX - nakashiroRadius, centerY - nakashiroRadius, nakashiroRadius * 2, nakashiroRadius * 2);
        }

        private Color GetColorForIntensity(float intensity)
        {
            float alpha = Math.Min(1f, intensity);
            return new Color(1, 0, 0, alpha);
        }

        private float Distance(PointF p1, PointF p2)
        {
            return MathF.Sqrt(MathF.Pow(p1.X - p2.X, 2) + MathF.Pow(p1.Y - p2.Y, 2));
        }
    }
}
