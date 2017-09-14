using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeMatch.Models;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;


namespace RecipeMatch.Repositories
{
    class RecipeRepo
    {
        public IList<RecipeKey> GetAllrecipes()
        {
            IList<RecipeKey> recipeList = new List<RecipeKey>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetAllRecipes",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    recipeList.Add(new RecipeKey
                    {
                        RecipeId = idlReader["RecipeId"].ToString(),
                        ExternalRecipeId = idlReader["External_recipe_id"].ToString(),
                    });
                }
            }
            return recipeList;
        }

        public bool SaveRecipeSimilarityIndex(RecipeMatchKey rmk)
        {
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.SaveRecipeSimilarities",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeA_Id", rmk.RecipeId_A);
                idlCommand.Parameters.AddWithValue("@RecipeB_Id", rmk.RecipeId_B);
                idlCommand.Parameters.AddWithValue("@ExternalRecipeA_id", rmk.ExternalRecipeId_A);
                idlCommand.Parameters.AddWithValue("@ExternalRecipeB_id", rmk.ExternalRecipeId_B);
                idlCommand.Parameters.AddWithValue("@SimiIndiValue", rmk.SimilarIndexValue);
                idlCommand.ExecuteNonQuery();

                return true;
            }
        }

        public bool SaveSpearmanRankScore(SpearmanRank sr)
        {
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.SaveSpearmanRankScore",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeA", sr.RecipeId_A);
                idlCommand.Parameters.AddWithValue("@RecipeB", sr.RecipeId_B);
                idlCommand.Parameters.AddWithValue("@SpearmanRankScore", sr.SpearmanRankScore);
                idlCommand.ExecuteNonQuery();

                return true;
            }
        }

        public bool SaveRecipeMetaInfo(RecipeInfo rinfo)
        {
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.SaveRecipeMeta",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeId", rinfo.RecipeId);
                idlCommand.Parameters.AddWithValue("@GlutenFree", rinfo.GlutenFree);
                idlCommand.Parameters.AddWithValue("@DiaryFree", rinfo.DairyFree);
                idlCommand.Parameters.AddWithValue("@Vegetarian", rinfo.Vegetarian);
                idlCommand.Parameters.AddWithValue("@Vegan", rinfo.Vegan);
                idlCommand.Parameters.AddWithValue("@VeryPopular", rinfo.VeryPopular);
                idlCommand.Parameters.AddWithValue("@Veryhealthy", rinfo.VeryHealthy);
                idlCommand.ExecuteNonQuery();

                if (rinfo.CookingInstructions.Count > 1)
                {
                    SqlCommand idlCommand1 = new SqlCommand
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "dbo.SaveCookingInstruction",
                        Connection = sqlConnection,
                        CommandTimeout = 0
                    };

                    idlCommand1.Parameters.AddWithValue("@RecipeId", rinfo.RecipeId);
                    idlCommand1.Parameters.AddWithValue("@CookingInst1", rinfo.CookingInstructions[0]);
                    if (rinfo.CookingInstructions.Count > 1)
                        idlCommand1.Parameters.AddWithValue("@CookingInst2", rinfo.CookingInstructions[1] ?? string.Empty);
                    if (rinfo.CookingInstructions.Count > 2)
                        idlCommand1.Parameters.AddWithValue("@CookingInst3", rinfo.CookingInstructions[2] ?? string.Empty);
                    if (rinfo.CookingInstructions.Count > 3)
                        idlCommand1.Parameters.AddWithValue("@CookingInst4", rinfo.CookingInstructions[3] ?? string.Empty);
                    if (rinfo.CookingInstructions.Count > 4)
                        idlCommand1.Parameters.AddWithValue("@CookingInst5", rinfo.CookingInstructions[4] ?? string.Empty);
                    idlCommand1.ExecuteNonQuery();
                }
                foreach (var ingre in rinfo.Ingredients)
                {
                    SqlCommand idlCommand2 = new SqlCommand
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "dbo.SaveRecipeIngredients",
                        Connection = sqlConnection,
                        CommandTimeout = 0
                    };

                    idlCommand2.Parameters.AddWithValue("@RecipeId", rinfo.RecipeId);
                    idlCommand2.Parameters.AddWithValue("@Ingredient", ingre.ToString());
                    idlCommand2.ExecuteNonQuery();
                }
            }
            return true;
        }


        public bool SavePredictedRating(IList<UnratedUserRecipe> SavePredictedRatings)
        {
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.SavePedictedRating",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeId", 0);
                idlCommand.Parameters.AddWithValue("@UserId", 0);
                idlCommand.Parameters.AddWithValue("@PredictedRating", 0);
                idlCommand.Parameters.AddWithValue("@PredictedRatingValue", 0);
                idlCommand.Parameters.Add("@Saved", SqlDbType.Int);
                idlCommand.Parameters["@Saved"].Direction = ParameterDirection.Output;
                foreach (var unrated in SavePredictedRatings)
                {                  
                    idlCommand.Parameters["@RecipeId"].Value= unrated.RecipeId;
                    idlCommand.Parameters["@UserId"].Value = unrated.UserId;
                    idlCommand.Parameters["@PredictedRating"].Value = unrated.PredictedRating;
                    idlCommand.Parameters["@PredictedRatingValue"].Value = unrated.PredictedRatingValue;
                    idlCommand.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool SaveSimiIndexUsers(IList<CommonUserRatings> SimiIndexUsers)
        {
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.SaveUserSimilarityIndex",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@UserA", 0);
                idlCommand.Parameters.AddWithValue("@UserB", 0);
                idlCommand.Parameters.AddWithValue("@simiIndexValue", 0);
                idlCommand.Parameters.Add("@Saved", SqlDbType.Int);
                idlCommand.Parameters["@Saved"].Direction = ParameterDirection.Output;
                foreach (var simiIndex in SimiIndexUsers)
                {
                    if (simiIndex.SimilarityIndex > 0)
                    {
                        idlCommand.Parameters["@UserA"].Value = simiIndex.UserId_A;
                        idlCommand.Parameters["@UserB"].Value = simiIndex.UserId_B;
                        idlCommand.Parameters["@simiIndexValue"].Value = simiIndex.SimilarityIndex;
                        idlCommand.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }
        public IList<RecipeKey> GetRecipesforMatching()
        {
            IList<RecipeKey> recipeList = new List<RecipeKey>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRecipesToMatch",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    recipeList.Add(new RecipeKey
                    {
                        RecipeId = idlReader["RecipeId"].ToString(),
                        ExternalRecipeId = idlReader["External_recipe_id"].ToString(),
                    });
                }
            }
            return recipeList;
        }

        public IList<RecipeKey> GetRecipesforSR()
        {
            IList<RecipeKey> recipeList = new List<RecipeKey>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRatingsForSR",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    recipeList.Add(new RecipeKey
                    {
                        RecipeId = idlReader["RecipeId"].ToString(),
                        ExternalRecipeId = idlReader["External_recipe_id"].ToString(),
                    });
                }
            }
            return recipeList;
        }

        public IList<RecipeRatings> GetRatingsForRecipe(int RecipeId, int NumofRatings)
        {
            IList<RecipeRatings> recipeList = new List<RecipeRatings>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRatingsForRecipe",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeId", RecipeId);
                if (NumofRatings > -1)
                    idlCommand.Parameters.AddWithValue("@Topratings", NumofRatings);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    recipeList.Add(new RecipeRatings
                    {
                        RecipeId = Convert.ToInt32(idlReader["RecipeId"].ToString()),
                        Rating = Convert.ToInt32(idlReader["Rating"].ToString()),
                        UserId = Convert.ToInt32(idlReader["UserId"].ToString()),
                        UserRatingId = Convert.ToInt32(idlReader["UserRatingId"].ToString()),
                    });
                }
            }
            return recipeList;
        }
        public IList<string> GetIngredientsForMatching(string recipeId)
        {
            IList<string> ingredientList = new List<string>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetIngredientsForRecipe",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeId", recipeId);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    ingredientList.Add(idlReader["ingredient"].ToString());
                }
            }
            return ingredientList;
        }

        public IList<RecipeRatings> GetRecipeRatingforuserId(int userId)
        {
            IList<RecipeRatings> RecipeRatingList = new List<RecipeRatings>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRecipeRatingbyUserId",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    RecipeRatingList.Add(new RecipeRatings
                    {
                        RecipeId = Convert.ToInt32(idlReader["RecipeId"].ToString()),
                        UserRatingId = Convert.ToInt32(idlReader["UserRatingId"].ToString()),
                        Rating = Convert.ToDouble(idlReader["UserRating"].ToString()),
                        UserId = Convert.ToInt32(idlReader["UserId"].ToString())
                    });
                }
            }
            return RecipeRatingList;
        }

        public IList<int> GetRecipesforPredRating(int userId)
        {
            IList<int> RecipeRatingList = new List<int>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRecipesforPredRating",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    RecipeRatingList.Add(Convert.ToInt32(idlReader["RecipeId"].ToString()));
                }
            }
            return RecipeRatingList;
        }


        public IList<RecipeRatings> GetRatedRecipeuserId(int userId)
        {
            IList<RecipeRatings> RecipeRatingList = new List<RecipeRatings>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRatedRecipeuserId",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    RecipeRatingList.Add(new RecipeRatings
                    {
                        RecipeId = Convert.ToInt32(idlReader["RecipeId"].ToString()),
                        UserRatingId = Convert.ToInt32(idlReader["UserRatingId"].ToString()),
                        Rating = Convert.ToDouble(idlReader["UserRating"].ToString()),
                        UserId = Convert.ToInt32(idlReader["UserId"].ToString())
                    });
                }
            }
            return RecipeRatingList;
        }

        public IList<UserRatings> GetUserRatings(int recipeId)
        {
            IList<UserRatings> UserRatingList = new List<UserRatings>();
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ApiConnectionString"].ConnectionString))
            {
                SqlCommand idlCommand = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.GetRecipeRating",
                    Connection = sqlConnection,
                    CommandTimeout = 0
                };
                sqlConnection.Open();
                idlCommand.Parameters.AddWithValue("@RecipeId", recipeId);
                SqlDataReader idlReader = idlCommand.ExecuteReader();
                while (idlReader.Read())
                {
                    UserRatingList.Add(new UserRatings
                    {
                        UserId = Convert.ToInt32(idlReader["UserId"].ToString()),
                        Rating = Convert.ToDouble(idlReader["RecipeRating"].ToString())
                    });
                }
            }
            return UserRatingList;
        }
    }
}
