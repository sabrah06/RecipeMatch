using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    class SpearmanRank
    {
        public string RecipeId_A { get; set; }
        public string RecipeId_B { get; set; }
        public string ExternalRecipeId_A { get; set; }
        public string ExternalRecipeId_B { get; set; }
        public double SpearmanRankScore { get; set; }
    }
}
