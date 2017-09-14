using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    public class RecipeMatchKey
    {
        public string RecipeId_A { get; set; }
        public string RecipeId_B { get; set; }
        public string ExternalRecipeId_A { get; set; }
        public string ExternalRecipeId_B { get; set; }
        public int RecipeIdA_IngCounts { get; set; }
        public int RecipeIdB_IngCounts { get; set; }
        public int SimilarIngredientsCount { get; set; }
        public double SimilarIndexValue { get; set; }
    }
}
