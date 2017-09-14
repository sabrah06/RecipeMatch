using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeMatch.Models
{
    class CalculatedUserRating
    {
        public int UserId { get; set; }
        public IList<RecipeRatings> RatedRecipeList { get; set; }
        public IList<UnratedUserRecipe> UnratedRecipeList { get; set; }
        public float CalulatedMean()
        {
            return (float) (RatedRecipeList.Sum(s => s.Rating) / RatedRecipeList.Count());
        }
        public float CalulatedSimilarity(IList<int> RatingList1, IList<int> RatingList2)
        {
            float Mean1 = 0;
            float Mean2 = 0;
            float SumSquares1 = 0;
            float SumSquares2 = 0;
            double AdjSumSquares1 = 0;
            double AdjSumSquares2 = 0;
            double AdjSumProd = 0;
            double SimiIndexValue = 0;
            Mean1 = RatingList1.Sum(s => s) / RatingList1.Count();
            Mean2 = RatingList2.Sum(s => s) / RatingList2.Count();
            foreach (int Rat1 in RatingList1)
            {
                if (Rat1 != -1)
                {
                    SumSquares1 += Rat1;
                    AdjSumSquares1 += Math.Pow((Rat1 - Mean1), 2);
                }
            }
            foreach (int Rat2 in RatingList2)
            {
                if (Rat2 != -1)
                {
                    SumSquares2 += Rat2;
                    AdjSumSquares2 += Math.Pow((Rat2 - Mean1), 2);
                }
            }
            int maxlen = RatingList1.Count > RatingList2.Count ? RatingList2.Count : RatingList1.Count;
            for (int ind=0; ind <= maxlen; ind++)
            {
                if (RatingList1[ind] != -1 && RatingList2[ind] != -1)
                {
                    AdjSumProd += (RatingList1[ind] - Mean1) * (RatingList2[ind] - Mean2);
                }
            }
            SimiIndexValue = AdjSumProd / (Math.Sqrt(AdjSumSquares1) * Math.Sqrt(AdjSumSquares2));
            
            return (float)SimiIndexValue;
        }
    }
}
