using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    class RecipeKey
    {
        public string RecipeId { get; set; }
        public string ExternalRecipeId { get; set; }
        public List<string> Ingredients { get; set; }
    }
}
