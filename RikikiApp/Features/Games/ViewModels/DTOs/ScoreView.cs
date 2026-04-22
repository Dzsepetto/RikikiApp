namespace RikikiApp.Features.Games.ViewModels.DTOs
{
    public class ScoreView
    {
        public string PlayerName { get; set; } = "";
        public int Called { get; set; }
        public int Won { get; set; }
        public int Score { get; set; }
    }
}
