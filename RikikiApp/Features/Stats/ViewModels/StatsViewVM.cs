using Microcharts;
using SkiaSharp;

namespace RikikiApp.Features.Stats.ViewModels
{
    internal class StatsViewVM
    {
        public Chart Chart { get; set; }

        public StatsViewVM()
        {
            Chart = new BarChart
            {
                Entries = new[]
                {
                new ChartEntry(10)
                {
                    Label = "Wins",
                    ValueLabel = "10",
                    Color = SKColor.Parse("#4CAF50")
                },
                new ChartEntry(5)
                {
                    Label = "Losses",
                    ValueLabel = "5",
                    Color = SKColor.Parse("#F44336")
                },
                new ChartEntry(15)
                {
                    Label = "Games",
                    ValueLabel = "15",
                    Color = SKColor.Parse("#2196F3")
                }
            }
            };
        }
    }
}
