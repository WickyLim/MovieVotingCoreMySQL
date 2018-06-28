using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MovieVotingCore.Models;

namespace MovieVotingCoreMySQL.Pages
{
    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        private readonly MovieVotingCore.Models.MovieContext _context;

        public AboutModel(MovieVotingCore.Models.MovieContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null){
                Message = "Invalid movie!";
            } else {
                Message = "Thanks for your vote!";

                var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                var vote = await _context.Vote.SingleOrDefaultAsync(v => v.IPAddress == ipAddress);
                if (vote == null) {
                    var movie = await _context.Movie.SingleOrDefaultAsync(m => m.MovieID == id);

                    if (movie != null) {
                        vote = new Vote
                        {
                            IPAddress = ipAddress,
                            VotedMovieID = (int)id
                        };
                        _context.Vote.Add(vote);

                        movie.Votes++;
                    }
                }
                else {
                    var prevMovie = await _context.Movie.SingleOrDefaultAsync(m => m.MovieID == vote.VotedMovieID);
                    var newMovie = await _context.Movie.SingleOrDefaultAsync(m => m.MovieID == id);
                    if (prevMovie != null && newMovie != null) {
                        if(prevMovie.Votes > 0) {
                            prevMovie.Votes--;
                        }
                        newMovie.Votes++;
                        vote.VotedMovieID = (int)id;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Index");
        }
    }
}
