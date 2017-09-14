using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    public class CommonUserRatings
    {
        public int UserId_A { get; set; }
        public int UserId_B { get; set; }
        public IList<CommonRatings> ListCommonRatings { get; set; }
        public float SimilarityIndex { get; set; }
        public double Mean_A_rating { get; set; }
        public double Mean_B_rating { get; set; }
        public double FullMean_A_rating { get; set; }
        public double FullMean_B_rating { get; set; }
        public double Rating_A { get; set; }
        public double Rating_B { get; set; }
    }
    public class CommonRatings
    {
        public int Recipe { get; set; }
        public double Rating_A { get; set; }
        public double Rating_B { get; set; }
    }
}
