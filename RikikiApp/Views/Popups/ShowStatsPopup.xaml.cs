using CommunityToolkit.Maui.Views;
using RikikiApp.Models;
using RikikiApp.Repositories;
using RikikiApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace RikikiApp.Views.Popups;

public partial class ShowStatsPopup : Popup, INotifyPropertyChanged
{
    private readonly IGamePlayerRepository _playersRepo;
    private readonly IRoundRepository _roundsRepo;
    private readonly ICallRepository _callsRepo;
    private readonly RikikiGameEngine _engine;

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    private readonly int _gameId;

    public class PlayerStat
    {
        public string Name { get; set; } = "";
        public List<int?> RoundScores { get; set; } = new();
        public int TotalScore { get; set; }
    }

    public ObservableCollection<PlayerStat> Stats { get; set; } = new();

    public ShowStatsPopup(int gameId)
    {
        InitializeComponent();

        _gameId = gameId;

        var services = Application.Current!.Handler!.MauiContext!.Services;

        _playersRepo = services.GetRequiredService<IGamePlayerRepository>();
        _roundsRepo = services.GetRequiredService<IRoundRepository>();
        _callsRepo = services.GetRequiredService<ICallRepository>();
        _engine = services.GetRequiredService<RikikiGameEngine>();

        BindingContext = this;

        _ = LoadData();
    }

    private async Task LoadData()
    {
        await Task.Delay(200);

        try
        {
            IsLoading = true;

            var players = await _playersRepo.GetByGameIdAsync(_gameId);
            var rounds = await _roundsRepo.GetByGameIdAsync(_gameId);

            var orderedRounds = rounds
                .Where(r => r.isCompleted)
                .OrderBy(r => r.RoundIndex)
                .ToList();

            var allCalls = new List<Call>();

            foreach (var round in orderedRounds)
            {
                var roundCalls = await _callsRepo.GetByRoundIdAsync(round.Id);
                allCalls.AddRange(roundCalls);
            }

            foreach (var player in players.OrderBy(p => p.SeatOrder))
            {
                var scoreValues = new List<int?>();
                var numericScores = new List<int>();

                foreach (var round in orderedRounds)
                {
                    var call = allCalls.FirstOrDefault(c =>
                        c.RoundId == round.Id &&
                        c.GamePlayerId == player.Id);

                    if (call == null || !call.Called.HasValue || !call.Won.HasValue)
                    {
                        scoreValues.Add(null);
                        continue;
                    }

                    var score = _engine.CalculateScore(call);

                    scoreValues.Add(score);
                    numericScores.Add(score);
                }

                Stats.Add(new PlayerStat
                {
                    Name = player.GuestName,
                    RoundScores = scoreValues,
                    TotalScore = numericScores.Sum()
                });
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}