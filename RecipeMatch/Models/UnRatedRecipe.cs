using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    class UnRatedRecipe
    {
        public int UnRatedRecipeId { get; set; }
        public IList<UnratedUserRecipe> ListRatedRecipes { get; set; }
    }

    public class UnratedUserRecipe
    {
        public int RecipeId { get; set; }
        public int UserId { get; set; }
        public double Rating { get; set; }
        public int PredictedRating { get; set; }
        public float PredictedRatingValue { get; set; }
    }
}
