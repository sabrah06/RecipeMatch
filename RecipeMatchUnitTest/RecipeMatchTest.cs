using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecipeMatch.Models;
using System.Collections.Generic;

namespace RecipeMatchUnitTest
{
    [TestClass]
    public class RecipeMatchTest
    {
        [TestMethod]
        public void SpearmanRankingScoreTest()
        {
            int[] RecipeA = { 72, 112, 46, 97, 46, 46, 52 };
            int[] RecipeB = { 20, 2, 7, 7, 4, 12, 7 };
            double rho = RecipeMatch.Program.SpearmansCoeff(RecipeA, RecipeB);
            double expected = -0.14285714285714279;
            Assert.IsTrue(rho.Equals(expected));
        }
        [TestMethod]
        public void SimilarityIndexCoeffTest()
        {
            IList<RecipeRatings> UserARecipeRatings = new List<RecipeRatings>();
            IList<RecipeRatings> UserBRecipeRatings = new List<RecipeRatings>();
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 100,
                RecipeId = 1000,
                UserId = 1,
                Rating = 5
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 100,
                RecipeId = 1000,
                UserId = 1,
                Rating = 5
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 101,
                RecipeId = 1001,
                UserId = 1,
                Rating = 3
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 102,
                RecipeId = 1003,
                UserId = 1,
                Rating = 4
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 103,
                RecipeId = 1004,
                UserId = 1,
                Rating = 4
            });
            UserBRecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 104,
                RecipeId = 1000,
                UserId = 2,
                Rating = 4
            });
            UserBRecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 105,
                RecipeId = 1001,
                UserId = 2,
                Rating = 3
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 106,
                RecipeId = 1002,
                UserId = 2,
                Rating = 4
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 107,
                RecipeId = 1003,
                UserId = 2,
                Rating = 5
            });
            UserARecipeRatings.Add(new RecipeRatings
            {
                UserRatingId = 108,
                RecipeId = 1004,
                UserId = 2,
                Rating = 3
            });
            CommonUserRatings Calrating = RecipeMatch.Program.CalculateSimilarIndex(UserARecipeRatings, UserBRecipeRatings);
            float expectedSimilarityValue = 0.999F;
            Assert.AreEqual(Calrating.SimilarityIndex, expectedSimilarityValue);
        }

        [TestMethod]
        public void TanimotoSimilarityIndexCoeffTest()
        {
            IList<RecipeMatchKey> ListMatches = new List<RecipeMatchKey>();
            ListMatches.Add(new RecipeMatchKey
            {
                RecipeId_A = "101",
                RecipeId_B = "201",
                ExternalRecipeId_A = "600001",
                ExternalRecipeId_B = "700001",
                RecipeIdA_IngCounts = 10,
                RecipeIdB_IngCounts = 12,
                SimilarIngredientsCount = 5
            });
            ListMatches = RecipeMatch.Program.CalculateSimilarIndexValue(ListMatches);
            double test1 = 0.2941;
            Assert.IsTrue(test1.Equals(ListMatches[0].SimilarIndexValue));
        }
    }
}
