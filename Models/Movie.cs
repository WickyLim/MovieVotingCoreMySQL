using System;
namespace MovieVotingCore.Models
{
    public class Movie
    {
        public int MovieID { get; set; }

        public string MovieName { get; set; }

        public string ImagePath { get; set; }

        public int Votes { get; set; }
    }
}
