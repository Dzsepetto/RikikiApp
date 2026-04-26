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
    private readonly IRoundScoreRepository _roundScoreRepo;

    private int _gameId;

    public Popup? PopupInstance { get; set; }

    public ObservableCollection<PlayerStat> Stats { get; } = new();

    public ShowStatsPopupVM(
        IGamePlayerRepository playersRepo,
        IRoundRepository roundsRepo,
        ICallRepository callsRepo,
        IRoundScoreRepository roundScoreRepo,
        RikikiGameEngine engine)
    {
        _playersRepo = playersRepo;
        _roundsRepo = roundsRepo;
        _roundScoreRepo = roundScoreRepo;
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

            var allScores = new List<RoundScore>();

            foreach (var round in orderedRounds)
            {
                var scores = await _roundScoreRepo.GetByRoundIdAsync(round.Id);
                allScores.AddRange(scores);
            }

            foreach (var player in players.OrderBy(p => p.SeatOrder))
            {
                var scoreValues = new List<int?>();
                var numericScores = new List<int>();

                foreach (var round in orderedRounds)
                {
                    var score = allScores.FirstOrDefault(s =>
                        s.RoundId == round.Id &&
                        s.GamePlayerId == player.Id);

                    if (score == null)
                    {
                        scoreValues.Add(null);
                        continue;
                    }

                    scoreValues.Add(score.Score);
                    numericScores.Add(score.Score);
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