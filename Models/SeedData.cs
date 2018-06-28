using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace MovieVotingCore.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MovieContext(
                serviceProvider.GetRequiredService<DbContextOptions<MovieContext>>()))
            {
                // Look for any movies.
                if (context.Movie.Any())
                {
                    return;   // DB has been seeded
                }

                context.Movie.AddRange(
                    new Movie
                    {
                        MovieName = "Avengers Infinity War",
                        ImagePath = "avengers",
                        Votes = 0
                    },

                    new Movie
                    {
                        MovieName = "Deadpool 2",
                        ImagePath = "deadpool",
                        Votes = 0
                    },

                    new Movie
                    {
                        MovieName = "Icredibles 2",
                        ImagePath = "incredibles",
                        Votes = 0
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
