using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    public class RecipeInfo
    {
        public int RecipeId { get; set; }
        public string ExternalRecipeId { get; set; }
        public string ReadyinMinutes { get; set; }
        public List<string> Ingredients { get; set; }
        public string ListOfIngredients { get; set; }
        public List<string> CookingInstructions { get; set; }
        public bool DairyFree { get; set; }
        public bool VeryPopular { get; set; }
        public bool GlutenFree { get; set; }
        public bool Vegetarian { get; set; }
        public bool Vegan { get; set; }
        public bool VeryHealthy { get; set; }
    }
}
