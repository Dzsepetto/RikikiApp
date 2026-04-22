using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RikikiApp.Core.Popups;
using RikikiApp.Features.Games.Domain.Entities;
using RikikiApp.Features.Games.Domain;
using RikikiApp.Repositories.Interfaces;

namespace RikikiApp.Features.Games.ViewModels.Popups;

public partial class ShowStatsPopupVM : ObservableObject, IPopupAware
{
    private readonly IGamePlayerRepository _playersRepo;
    private readonly IRoundRepository _roundsRepo;
    private readonly ICallRepository _callsRepo;
    private readonly RikikiGameEngine _engine;

    private int _gameId;

    public Popup? PopupInstance { get; set; }

    public ObservableCollection<PlayerStat> Stats { get; } = new();

    public ShowStatsPopupVM(
        IGamePlayerRepository playersRepo,
        IRoundRepository roundsRepo,
        ICallRepository callsRepo,
        RikikiGameEngine engine)
    {
        _playersRepo = playersRepo;
        _roundsRepo = roundsRepo;
        _callsRepo = callsRepo;
        _engine = engine;
    }

    public async Task InitAsync(int gameId)
    {
        _gameId = gameId;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            Stats.Clear();

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
    }

    [RelayCommand]
    private async Task Close()
    {
        if (PopupInstance != null)
            await PopupInstance.CloseAsync();
    }

    public class PlayerStat
    {
        public string Name { get; set; } = "";
        public List<int?> RoundScores { get; set; } = new();
        public int TotalScore { get; set; }
    }
}