using Microsoft.EntityFrameworkCore;
namespace MovieVotingCore.Models
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options)
                : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; }
        public DbSet<Vote> Vote { get; set; }
    }
}
