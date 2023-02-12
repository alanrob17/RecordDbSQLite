using Dapper;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using DAL.Models;
using System.Dynamic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DAL
{
    public class RecordDataAccess
    {
        public static List<RecordModel> GetRecords()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var records = cn.Query<RecordModel>("SELECT * FROM Record ORDER BY Recorded DESC", new DynamicParameters());

                return records.ToList();
            }
        }

        public static List<RecordModel> GetRecordsByArtistId(int artistId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var records = cn.Query<RecordModel>($"SELECT * FROM Record WHERE ArtistId = {artistId} ORDER BY Recorded DESC");

                return records.ToList();
            }
        }

        public static ArtistModel GetRecordsByArtistIdMultipleTables(int artistId)
        {
            string query = "select * from Artist where artistId = @artistId ; select * from Record where artistId = @artistId order by Recorded;";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var multipleResult = cn.QueryMultiple(query, new { artistId = artistId });

                var artist = multipleResult.Read<ArtistModel>().SingleOrDefault();
                if (artist is ArtistModel)
                {
                    var records = multipleResult.Read<RecordModel>().ToList();
                    artist.Records = records;
                    return artist;
                }
                else
                {
                    return new ArtistModel { ArtistId = 0 };
                }
            }
        }

        public static List<ArtistModel> GetArtistRecordsMultipleTables()
        {
            List<ArtistModel> artists = new();

            string query = "select * from Artist order by LastName, FirstName; select * from Record order by ArtistId, Recorded;";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var multipleResult = cn.QueryMultiple(query);

                artists = multipleResult.Read<ArtistModel>().ToList();
                var records = multipleResult.Read<RecordModel>().ToList();

                foreach (var artist in artists)
                {
                    artist.Records = records.Where(r => r.ArtistId == artist.ArtistId).ToList();
                }
            }

            return artists;
        }

        public static List<RecordModel> GetRecordsByYear(int year)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var records = cn.Query<RecordModel>($"SELECT * FROM Record WHERE Recorded = {year} ORDER BY ArtistId");

                return records.ToList();
            }
        }

        public static RecordModel GetRecordById(int recordId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<RecordModel>($"SELECT * FROM Record WHERE RecordId = {recordId}").FirstOrDefault() ?? new RecordModel { RecordId = 0 };
            }
        }

        public static RecordModel GetRecordByName(RecordModel record)
        {
            RecordModel? foundRecord = null;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                foundRecord = cn.Query<RecordModel>($"SELECT * FROM Record WHERE Name LIKE @Name COLLATE NOCASE", record).FirstOrDefault();
            }

            return foundRecord ?? new RecordModel { RecordId = 0 };
        }

        public static int UpdateRecord(RecordModel record)
        {
            var result = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                result = cn.Execute("UPDATE Record SET Name = @Name, Field = @Field, Recorded = @Recorded, Label = @Label, Pressing = @Pressing, Rating = @Rating, Discs = @Discs, Media = @Media, Bought = @Bought, Cost = @Cost, Review = @Review WHERE RecordId = @RecordId", record);
            }

            return result;
        }

        public static int AddRecord(RecordModel record)
        {
            var result = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var query = @"INSERT INTO Record (ArtistId, Name, Field, Recorded, Label, Pressing, Rating, Discs, Media, Bought, Cost, Review) VALUES (@ArtistId, @Name, @Field, @Recorded, @Label, @Pressing, @Rating, @Discs, @Media, @Bought, @Cost, @Review);";
                result = cn.Execute(query, record);
            }

            return result;
        }

        public static int DeleteRecord(int recordId)
        {
            var result = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                result = cn.Execute($"DELETE FROM Record WHERE RecordId={recordId}");
            }

            return result;
        }

        public static int GetTotalNumberOfCDs()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var count = cn.ExecuteScalar<int>("SELECT SUM(Discs) FROM Record WHERE Media = 'CD' OR Media = 'CD/DVD' OR Media = 'CD/Blu-ray'");

                return count;
            }
        }

        public static int GetTotalNumberOfDiscs()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var count = cn.ExecuteScalar<int>("SELECT SUM(Discs) FROM Record");

                return count;
            }
        }

        public static int GetTotalNumberOfRecords()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var count = cn.ExecuteScalar<int>("SELECT SUM(Discs) FROM Record WHERE Media = 'R'");

                return count;
            }
        }

        public static object GetTotalNumberOfBlurays()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var count = cn.ExecuteScalar<int>("SELECT SUM(Discs) FROM Record WHERE Media = 'CD/Blu-ray' OR Media = 'Blu-ray'");

                return count;
            }
        }

        public static List<dynamic> GetArtistRecordList()
        {
            string query = "SELECT a.ArtistId, a.FirstName, a.LastName, a.Name AS Artist, r.RecordId, r.Name, r.Recorded, r.Media, r.Rating " +
                           "FROM Artist a " +
                           "JOIN Record r ON a.ArtistId = r.ArtistId " +
                           "ORDER BY a.LastName, a.FirstName, r.Recorded";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<dynamic>(query).ToList();
            }
        }

        /// <summary>
        /// Count the number of discs.
        /// </summary>
        public static int CountAllDiscs(string media = "")
        {
            var mediaType = string.Empty;

            switch (media)
            {
                case "":
                    mediaType = "'DVD' OR Media = 'CD/DVD' OR Media = 'Blu-ray' OR Media = 'CD/Blu-ray' OR Media = 'CD' OR Media = 'R'";
                    break;
                case "DVD":
                    mediaType = "'DVD' OR Media = 'CD/DVD' OR Media = 'Blu-ray' OR Media = 'CD/Blu-ray'";
                    break;
                case "CD":
                    mediaType = "'CD'";
                    break;
                case "R":
                    mediaType = "'R'";
                    break;
                default:
                    break;
            }

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var count = cn.ExecuteScalar<int>($"SELECT SUM(Discs) FROM Record WHERE Media = {mediaType}");

                return count;
            }
        }

        public static dynamic? GetArtistRecordEntity(int recordId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var query = @"
                    SELECT 
                        r.*,
                        a.ArtistId as ArtistId,
                        a.FirstName as FirstName,
                        a.LastName as LastName,
                        a.Name as Artist
                    FROM Record r
                    JOIN Artist a ON a.ArtistId = r.ArtistId
                    WHERE r.RecordId = @recordId";

                var result = cn.QueryFirstOrDefault<dynamic>(query, new { recordId });

                if (result == null)
                {
                    result = new ExpandoObject();
                    result.RecordId = 0;
                }

                return result;
            }
        }

        /// <summary>
        /// Get number of records for an artist.
        /// </summary>
        public static int GetArtistNumberOfRecords(int artistId)
        {
            var discs = 0;

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                discs = cn.ExecuteScalar<int>("SELECT SUM(Discs) FROM Record WHERE artistId = @artistId;", new { artistId });
            }

            return discs;
        }

        /// <summary>
        /// Get record details from ToString method.
        /// </summary>
        public static RecordModel GetFormattedRecord(int recordId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<RecordModel>($"SELECT * FROM Record WHERE RecordId = {recordId}").FirstOrDefault() ?? new RecordModel { RecordId = 0 };
            }
        }

        public static string GetArtistNameFromRecord(int recordId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var query = "SELECT a.Name FROM Artist a INNER JOIN Record r ON a.ArtistId = r.ArtistId WHERE r.RecordId = @recordId";
                var parameters = new { recordId };

                return cn.QuerySingleOrDefault<string>(query, parameters);
            }
        }

        /// <summary>
        /// Get the number of discs for a particular year.
        /// </summary>
        public static int GetDiscCountForYear(int year)
        {
            var discCount = 0;

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                discCount = cn.Query<int>($"SELECT COUNT(*) FROM Record WHERE Recorded = {year}").FirstOrDefault();
            }

            return discCount;
        }

        /// <summary>
        /// Get the number of discs that I bought for a particular year.
        /// </summary>
        public static int GetBoughtDiscCountForYear(string year)
        {
            var discCount = 0;

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                // get year from bought date
                discCount = cn.Query<int>($"SELECT SUM(Discs) FROM Record WHERE Bought like '%{year}%'").FirstOrDefault();
            }

            return discCount;
        }

        public static List<dynamic> MissingRecordReviews()
        {
            var query = "SELECT a.ArtistId, A.Name AS Artist, r.RecordId, r.Name, r.Recorded, r.Discs, r.Rating, r.Media " + 
                "FROM Artist a " + 
                "INNER JOIN Record r ON a.ArtistId = r.ArtistId " + 
                "WHERE r.Review = '' " + "" +
                "ORDER BY a.LastName, a.FirstName;";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<dynamic>(query).ToList();
            }
        }

        public static int GetNoReviewCount()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<int>($"SELECT COUNT(Review) FROM Record WHERE Review = '';").FirstOrDefault();
            }
        }
         
        public static List<dynamic> GetCostTotals()
        {
            var artistList = new List<dynamic>();
            var query = "SELECT a.ArtistId, a.FirstName, a.LastName, a.Name, SUM(r.Cost) AS Cost " +
                        "FROM Artist a " +
                        "JOIN Record r ON a.ArtistId = r.ArtistId " +
                        "GROUP BY a.ArtistId, a.FirstName, a.LastName, a.Name " +
                        "ORDER BY a.LastName, a.FirstName;";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return artistList = cn.Query<dynamic>(query).ToList();
            }
        }

        /// <summary>
        /// Get total number of discs for each artist.
        /// </summary>
        public static IEnumerable<dynamic> GetTotalArtistDiscs()
        {
            var artistList = new List<dynamic>();
            var query = "SELECT a.ArtistId, a.FirstName, a.LastName, a.Name, SUM(r.Discs) AS Discs " +
                        "FROM Artist a " +
                        "JOIN Record r ON a.ArtistId = r.ArtistId " +
                        "GROUP BY a.ArtistId, a.FirstName, a.LastName, a.Name " +
                        "ORDER BY a.LastName, a.FirstName;";

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return artistList = cn.Query<dynamic>(query).ToList();
            }
        }

        private static string LoadConnectionString(string id = "RecordDB")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}