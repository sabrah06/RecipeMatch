using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RecipeMatch.Repositories;
using RecipeMatch.Models;
using Newtonsoft.Json;

namespace RecipeMatch
{
    public class Program
    {
        static HttpClient client = new HttpClient();
        IList<RecipeInfo> ListofRecipes = new List<RecipeInfo>();

        static void Main(string[] args)
        {
            string MashappKey = System.Configuration.ConfigurationSettings.AppSettings["MashappKey"];
            client.BaseAddress = new Uri($"https://spoonacular-recipe-food-nutrition-v1.p.mashape.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Mashape-Key", MashappKey);

            foreach (string arg in args)
            {
                switch(arg)
                {
                    case "I":
                        Console.WriteLine("starting to updating ingredients for recipe");
                        ProcessRecipeIngredients();
                        Console.WriteLine("Finished updating ingredients for recipe");
                        Console.ReadLine();
                        break;
                    case "S":
                        BuildSimilarRecipeLinks();
                        Console.WriteLine("Finished building recipe similarity");
                        Console.ReadLine();
                        break;
                    case "P":
                        BuildSpearmanRankingScore();
                        Console.WriteLine("Finished building spearman ranking scores");
                        Console.ReadLine();
                        break;
                    case "U":
                        BuildSimiliarUsers(1018);
                        Console.WriteLine("Finished building user similarity");
                        Console.ReadLine();
                        break;
                    default:
                        break;
                }
                break;
            }
        }
        public static async void ProcessRecipeIngredients()
        {
            RecipeRepo recipeDB = new RecipeRepo();
            IList<RecipeKey> ListOfRecipes = recipeDB.GetAllrecipes();
            foreach (RecipeKey rkey in ListOfRecipes)
            {
                var tsk = GetRecipeSync(rkey.ExternalRecipeId);
                tsk.Wait();
                var recipeVar = tsk.Result;
            }
        }
        static async Task<string> GetRecipeSync(string recipeId)
        {
            string ingName = string.Empty;

            HttpResponseMessage response = await client.GetAsync($"/recipes/{recipeId}/information");
            if (response.IsSuccessStatusCode)
            {
                dynamic json = await response.Content.ReadAsStringAsync();
                dynamic recipeVar = JsonConvert.DeserializeObject(json);

                RecipeInfo rinfo = PopulateRecipeInfo(recipeVar);
                rinfo.RecipeId = Convert.ToInt32(recipeId);
                bool saved = SaveRecipeInformation(rinfo);
                Console.WriteLine("Completed importing metadata for recipe: " + rinfo.RecipeId);                
            }
            return ingName;
        }

        public static RecipeInfo PopulateRecipeInfo(dynamic recpInfo)
        {
            RecipeInfo rinfo = new RecipeInfo();
            rinfo.GlutenFree = recpInfo.glutenFree;
            rinfo.Vegetarian = recpInfo.vegetarian == true;
            rinfo.ReadyinMinutes = recpInfo.cookingMinutes;
            rinfo.DairyFree = recpInfo.dairyFree == true;
            rinfo.VeryPopular = recpInfo.veryPopular == true;
            rinfo.Ingredients = new List<string>();
            foreach (var ingrd in recpInfo.extendedIngredients)
            {
                rinfo.ListOfIngredients = rinfo.ListOfIngredients + ", " + ingrd.name;
                rinfo.Ingredients.Add(ingrd.name.ToString());
            }
            rinfo.CookingInstructions = new List<string>();
            foreach (var cookInst in recpInfo.analyzedInstructions)
            {
                foreach (var cookSteps in cookInst.steps)
                {
                    rinfo.CookingInstructions.Add(cookSteps.step.ToString());
                }
            }

            return rinfo;
        }

        public static bool SaveRecipeInformation(RecipeInfo rinfo)
        {
            //save the recipe metadata
            RecipeRepo repoDB = new RecipeRepo();
            if (repoDB.SaveRecipeMetaInfo(rinfo))
                return true;
            else
                return false;
        }

        public static void BuildSpearmanRankingScore()
        {
            bool saved = false;
            RecipeRepo repoDB = new RecipeRepo();
            IList<RecipeKey> ListRecipesforSR = repoDB.GetRecipesforSR();
            IList<RecipeRatings> ListRecipeARatings = new List<RecipeRatings>();
            IList<RecipeRatings> ListRecipeBRatings = new List<RecipeRatings>();
            IList<SpearmanRank> ListMatches = new List<SpearmanRank>();
            IList<int> RecipeARatings = new List<int>();
            IList<int> RecipeBRatings = new List<int>();
            int NumofRatings = 0;
            RecipeKey rkeyA;
            RecipeKey rkeyB;
            int[] setX;
            int[] setY;
            double rho = 0;
            int maxRating = 0;
            int iCount = 0;
            for (int outLoop = 0; outLoop < ListRecipesforSR.Count; outLoop++)
            {
                rkeyA = ListRecipesforSR[outLoop];
                ListRecipeARatings = repoDB.GetRatingsForRecipe(Convert.ToInt32(rkeyA.RecipeId), -1);
                NumofRatings = ListRecipeARatings.Count;
                for(int innerLoop = outLoop+1; innerLoop < ListRecipesforSR.Count; innerLoop++)
                {
                    rkeyB = ListRecipesforSR[innerLoop];
                    if (rkeyA.ExternalRecipeId == rkeyB.ExternalRecipeId ||
                        (ListMatches.Any(r => r.ExternalRecipeId_A == rkeyA.ExternalRecipeId && r.ExternalRecipeId_B == rkeyB.ExternalRecipeId))
                        ||
                        (ListMatches.Any(r => r.ExternalRecipeId_B == rkeyA.ExternalRecipeId && r.ExternalRecipeId_A == rkeyB.ExternalRecipeId)))
                        continue;
                    else
                    {
                        RecipeARatings = new List<int>();
                        RecipeBRatings = new List<int>();
                        ListRecipeBRatings = repoDB.GetRatingsForRecipe(Convert.ToInt32(rkeyB.RecipeId), NumofRatings);
                        if (ListRecipeARatings.Count <= ListRecipeBRatings.Count)
                            maxRating = ListRecipeARatings.Count;
                        else
                            maxRating = ListRecipeBRatings.Count;
                        iCount = 0;
                        foreach (RecipeRatings rcpA in ListRecipeARatings)
                        {
                            RecipeARatings.Add(Convert.ToInt32(rcpA.Rating));
                            iCount++;
                            if (iCount >= maxRating)
                                break;
                        }
                        iCount = 0;
                        foreach (RecipeRatings rcpB in ListRecipeBRatings)
                        {
                            RecipeBRatings.Add(Convert.ToInt32(rcpB.Rating));
                            iCount++;
                            if (iCount >= maxRating)
                                break;
                        }
                        if (RecipeBRatings.Count > 1 && RecipeBRatings.Count > 1)
                        {
                            setX = RecipeARatings.ToArray();
                            setY = RecipeBRatings.ToArray();
                            rho = SpearmansCoeff(setX, setY);
                            ListMatches.Add(new SpearmanRank
                            {
                                RecipeId_A = rkeyA.RecipeId,
                                RecipeId_B = rkeyB.RecipeId,
                                ExternalRecipeId_A = rkeyA.ExternalRecipeId,
                                ExternalRecipeId_B = rkeyB.ExternalRecipeId,
                                SpearmanRankScore = rho
                            });
                        }
                    }
                }
            }
            RecipeRepo recipeDB = new RecipeRepo();
            foreach (SpearmanRank sr in ListMatches)
            {
                saved = recipeDB.SaveSpearmanRankScore(sr);
                Console.WriteLine("Saved spearman rank score for : " + sr.ExternalRecipeId_A + " & " + sr.ExternalRecipeId_B);
            }
        }

        public static double SpearmansCoeff(IEnumerable<int> listA, IEnumerable<int> listB)
        {
            if (listA.Count() != listB.Count())
                throw new ArgumentException("Both collections of data must contain an equal number of elements");

            double[] rankofA = GetRanking(listA);
            double[] rankofB = GetRanking(listB);

            var difference = rankofA.Zip(rankofB, (x, y) => new { x, y });
            double sumofDifference = difference.Sum(s => Math.Pow(s.x - s.y, 2));
            int n = listA.Count();

            // Spearman's Coefficient of Correlation
            // ρ = 1 - ((6 * sum of rank differences^2) / (n(n^2 - 1))
            double rho = 1 - ((6 * sumofDifference) / (Math.Pow(n, 3) - n));

            return rho;
        }

        public static double[] GetRanking(IEnumerable<int> values)
        {
            var groupedValues = values.OrderByDescending(n => n)
                                      .Select((val, i) => new { Value = val, IndexedRank = i + 1 })
                                      .GroupBy(i => i.Value);

            double[] rankings = (from n in values
                                 join grp in groupedValues on n equals grp.Key
                                 select grp.Average(g => g.IndexedRank)).ToArray();

            return rankings;
        }
       
        public static void BuildSimilarRecipeLinks()
        {
            bool saved = false;
            RecipeRepo repoDB = new RecipeRepo();
            IList<RecipeKey> ListRecipesforMatching = repoDB.GetRecipesforMatching();
            IList<string> ListRecipeAIngredients = new List<string>();
            IList<string> ListRecipeBIngredients = new List<string>();
            IList<RecipeMatchKey> ListMatches = new List<RecipeMatchKey>();
            int ingCommonCount = 0;
            foreach (RecipeKey rkeyA in ListRecipesforMatching)
            {
                ingCommonCount = 0;
                ListRecipeAIngredients = repoDB.GetIngredientsForMatching(rkeyA.ExternalRecipeId);
                foreach (RecipeKey rkeyB in ListRecipesforMatching)
                {
                    if (rkeyA.ExternalRecipeId == rkeyB.ExternalRecipeId ||
                        (ListMatches.Any(r => r.ExternalRecipeId_A == rkeyA.ExternalRecipeId && r.ExternalRecipeId_B == rkeyB.ExternalRecipeId))
                        ||
                        (ListMatches.Any(r => r.ExternalRecipeId_B == rkeyA.ExternalRecipeId && r.ExternalRecipeId_A == rkeyB.ExternalRecipeId)))
                        continue;
                    else
                    {
                        ListRecipeBIngredients = repoDB.GetIngredientsForMatching(rkeyB.ExternalRecipeId);
                        ingCommonCount = 0;
                        foreach (string ingA in ListRecipeAIngredients)
                        {
                            foreach (string ingB in ListRecipeBIngredients)
                            {
                                if (ingA.Equals(ingB))
                                {
                                    ingCommonCount++;
                                    break;
                                }                              
                            }
                        }
                        ListMatches.Add(new RecipeMatchKey
                        {
                            RecipeId_A = rkeyA.RecipeId,
                            RecipeId_B = rkeyB.RecipeId,
                            ExternalRecipeId_A = rkeyA.ExternalRecipeId,
                            ExternalRecipeId_B = rkeyB.ExternalRecipeId,
                            RecipeIdA_IngCounts = ListRecipeAIngredients.Count,
                            RecipeIdB_IngCounts = ListRecipeBIngredients.Count,
                            SimilarIngredientsCount = ingCommonCount
                        });
                    }
                }
            }
            RecipeRepo recipeDB = new RecipeRepo();
            ListMatches = CalculateSimilarIndexValue(ListMatches);
            foreach(RecipeMatchKey  rmk in ListMatches)
            {
                saved = recipeDB.SaveRecipeSimilarityIndex(rmk);
                Console.WriteLine("Saved similarity index for : " + rmk.ExternalRecipeId_A + " & " + rmk.ExternalRecipeId_B);
            }
        }

        public static IList<RecipeMatchKey> CalculateSimilarIndexValue(IList<RecipeMatchKey>  ListMatches)
        {
            foreach(RecipeMatchKey rmk in ListMatches)
            {
                rmk.SimilarIndexValue = Math.Round((double) rmk.SimilarIngredientsCount / (rmk.RecipeIdA_IngCounts + rmk.RecipeIdB_IngCounts - rmk.SimilarIngredientsCount), 4);
            }
            return ListMatches;
        }

        public static void BuildSimiliarUsers(int PrimaryUser)
        {
            RecipeRepo recipRepo = new RecipeRepo();
            double MeanRating = 0;
            double predRatingRHS = 0;
            double SumSimilarityIndex = 0;
            IList<RecipeRatings> UserARecipeRatings = new List<RecipeRatings>();
            IList<RecipeRatings> UserBRecipeRatings = new List<RecipeRatings>();
            CalculatedUserRating userRating = new CalculatedUserRating();
            userRating.RatedRecipeList = recipRepo.GetRecipeRatingforuserId(PrimaryUser);
            userRating.UnratedRecipeList = new List<UnratedUserRecipe>();
            /****
               To predict a rating for a recipe for a user A
                ==> select the recipes rated by user A
                ==> in this case if the recipes are I, J, K, L and we are predicting the rating of Recipe M
                ==> Find users who have rated I, J, K, L and find the similarity index of these users with user A using pearson's correlation
                ==> if ratings are similar calculate the predicted rating of a recipe M that was rated by User B and not User A
                ==> Using the similatrity index predict the rating of M
                ***/
            foreach (var rcp in userRating.RatedRecipeList.Where(r => r.Rating == -1).ToList())
                userRating.UnratedRecipeList.Add(new UnratedUserRecipe
                {
                    UserId = rcp.UserId,
                    RecipeId = rcp.RecipeId,
                    Rating = rcp.Rating
                });

            //find the recipes that have users have rated
            IList<CommonUserRatings> ListUserRatings = new List<CommonUserRatings>();
            foreach (var unratd in userRating.UnratedRecipeList)
            {
                UserARecipeRatings = recipRepo.GetRatedRecipeuserId(unratd.UserId);
                // fetch users & ratings who have rated the unrated recipe of user A
                IList<UserRatings> UserRatingList = new List<UserRatings>();
                UserRatingList = recipRepo.GetUserRatings(unratd.RecipeId);
                //find the similarity index
                foreach (var userBRating in UserRatingList)
                {
                    //fetch all rating of all recipes for user B
                    UserBRecipeRatings = recipRepo.GetRatedRecipeuserId(userBRating.UserId);
                    // find similarity index with rating of User A and User B
                    CommonUserRatings Calrating = new CommonUserRatings();
                    Calrating = CalculateSimilarIndex(UserARecipeRatings, UserBRecipeRatings);
                    Calrating.UserId_A = unratd.UserId;
                    Calrating.UserId_B = userBRating.UserId;
                    Calrating.Rating_A = -1;
                    Calrating.Rating_B = userBRating.Rating;
                    ListUserRatings.Add(Calrating);
                }
                foreach (var usrRating in ListUserRatings)
                {
                    if (usrRating.UserId_A != usrRating.UserId_B)
                    {
                        if (usrRating.SimilarityIndex > 0)
                        {
                            predRatingRHS += Math.Round((usrRating.Rating_B - usrRating.FullMean_B_rating) * usrRating.SimilarityIndex, 3);
                            SumSimilarityIndex += Math.Round(Math.Abs(usrRating.SimilarityIndex), 3);
                        }
                        else
                        {
                            predRatingRHS += 0;
                            SumSimilarityIndex += 0;
                        }
                    }
                }
                MeanRating = UserARecipeRatings.Sum(r => r.Rating) / UserARecipeRatings.Count();
                unratd.PredictedRatingValue = (float)Math.Round(MeanRating + (predRatingRHS / SumSimilarityIndex), 3);
                unratd.PredictedRating = (int)Math.Ceiling((MeanRating + predRatingRHS / SumSimilarityIndex) * 2) / 2;
            }
            bool savedSimi = false;
            savedSimi = recipRepo.SaveSimiIndexUsers(ListUserRatings);
            Console.WriteLine("Calculated the similarity of for similar users");

            bool saved = recipRepo.SavePredictedRating(userRating.UnratedRecipeList);
            foreach (var pr in userRating.UnratedRecipeList)
                Console.WriteLine("Calculated the predicted rating of user :" + pr.UserId + " recipe : " + pr.RecipeId + " predicted rating: " + pr.PredictedRating);
        }

        public static CommonUserRatings CalculateSimilarIndex(IList<RecipeRatings> UserARating, IList<RecipeRatings> UserBRating)
        {
            float similartityIndex = 0;
            double MeanA = 0;
            double MeanB = 0;
            double AdjMeanA = 0;
            double AdjMeanB = 0;
            double r_AdjMeanA = 0;
            double r_AdjMeanB = 0;
            double ProdAdjuMean = 0;
            double r_ProdAdjuMean = 0;
            double SquareMeanA = 0;
            double SquareMeanB = 0;
            double r_SquareMeanA = 0;
            double r_SquareMeanB = 0;
            //find common recipe ratings of userA and user B
            CommonUserRatings ListofCommonratings = new CommonUserRatings();
            ListofCommonratings.ListCommonRatings = new List<CommonRatings>();
            foreach (var ratingA in UserARating)
            {
                if(UserBRating.Where(r => r.RecipeId == ratingA.RecipeId).Any())
                {
                    var matchingRating = UserBRating.FirstOrDefault(r => r.RecipeId == ratingA.RecipeId);
                    ListofCommonratings.ListCommonRatings.Add(new CommonRatings
                    {
                        Recipe = matchingRating.RecipeId,
                        Rating_A = ratingA.Rating,
                        Rating_B = matchingRating.Rating
                    });
                }
             }
            // After building the common recipe ratings list calculate the simi index
            //find the mean of ratings for User A and user B
            MeanA = ListofCommonratings.ListCommonRatings.Sum(r => r.Rating_A) / ListofCommonratings.ListCommonRatings.Count();
            MeanB = ListofCommonratings.ListCommonRatings.Sum(r => r.Rating_B) / ListofCommonratings.ListCommonRatings.Count();
            ProdAdjuMean = 0;
            AdjMeanA = 0;
            AdjMeanB = 0;
            SquareMeanA = 0;
            SquareMeanB = 0;
            foreach (var commonRating in ListofCommonratings.ListCommonRatings)
            {
                r_AdjMeanA = Math.Round(commonRating.Rating_A - MeanA, 3);
                AdjMeanA += r_AdjMeanA;
                r_AdjMeanB = Math.Round(commonRating.Rating_B - MeanB, 3);
                AdjMeanB += r_AdjMeanB;
                r_ProdAdjuMean = Math.Round(r_AdjMeanA * r_AdjMeanB, 3);
                ProdAdjuMean += r_ProdAdjuMean;
                r_SquareMeanA = Math.Round(Math.Pow(r_AdjMeanA, 2), 3);
                SquareMeanA += r_SquareMeanA;
                r_SquareMeanB = Math.Round(Math.Pow(r_AdjMeanB, 2), 3);
                SquareMeanB += r_SquareMeanB;
            }
            similartityIndex = (float) Math.Round((ProdAdjuMean / (Math.Sqrt(SquareMeanA) * Math.Sqrt(SquareMeanB))), 3);
            ListofCommonratings.SimilarityIndex = similartityIndex;
            ListofCommonratings.Mean_A_rating = MeanA;
            ListofCommonratings.Mean_B_rating = MeanB;
            ListofCommonratings.FullMean_A_rating = UserARating.Sum(r => r.Rating) / UserARating.Count();
            ListofCommonratings.FullMean_B_rating = UserBRating.Sum(r => r.Rating) / UserBRating.Count();
            return ListofCommonratings;
        }
    } 
}

