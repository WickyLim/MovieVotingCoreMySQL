using System;
namespace MovieVotingCore.Models
{
    public class Vote
    {
        public int VoteID { get; set; }

        public string IPAddress { get; set; }

        public int VotedMovieID { get; set; }
    }
}
