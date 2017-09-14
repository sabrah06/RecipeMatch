using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    public class RecipeRatings
    {
        public int UserRatingId { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public double Rating { get; set; }

    }
}
