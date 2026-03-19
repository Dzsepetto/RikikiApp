using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikikiApp.Models
{
    internal class ScoreView
    {
        public string PlayerName { get; set; } = "";
        public int Called { get; set; }
        public int Won { get; set; }
        public int Score { get; set; }
    }
}
