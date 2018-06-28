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
    public class IndexModel : PageModel
    {
        private readonly MovieVotingCore.Models.MovieContext _context;

        public IndexModel(MovieVotingCore.Models.MovieContext context)
        {
            _context = context;
        }

        public IList<Movie> Movie { get; set; }

        public async Task OnGetAsync()
        {
            Movie = await _context.Movie.ToListAsync();
        }
    }
}
