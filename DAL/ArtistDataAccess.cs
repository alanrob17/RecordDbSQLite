using Dapper;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using DAL.Models;
using System.Text;

namespace DAL
{
    public class ArtistDataAccess
    {
        public static List<ArtistModel> GetArtists()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var artists = cn.Query<ArtistModel>("SELECT * FROM Artist ORDER BY LastName, FirstName", new DynamicParameters());

                return artists.ToList();
            }
        }

        public static ArtistModel? GetArtistById(int artistId)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<ArtistModel>($"SELECT * FROM Artist WHERE ArtistId = {artistId}").FirstOrDefault() ?? new ArtistModel { ArtistId = 0 };
            }
        }

        public static ArtistModel? GetArtistByName(ArtistModel artist)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<ArtistModel>("SELECT * FROM Artist WHERE Name LIKE @Name COLLATE NOCASE", artist).FirstOrDefault() ?? new ArtistModel { ArtistId = 0 };
            }
        }

        public static ArtistModel? GetArtistByFirstLastName(ArtistModel artist)
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                return cn.Query<ArtistModel>("SELECT * FROM Artist WHERE FirstName LIKE @FirstName AND LastName LIKE @LastName COLLATE NOCASE", artist).FirstOrDefault() ?? new ArtistModel { ArtistId = 0 };
            }
        }


        public static int UpdateArtist(ArtistModel artist)
        {
            var i = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                artist.Name = !string.IsNullOrEmpty(artist.FirstName) ? $"{artist.FirstName} {artist.LastName}" : artist.LastName;

                i = cn.Execute("UPDATE Artist SET FirstName = @FirstName, LastName = @LastName, Name = @Name, Biography = @Biography WHERE ArtistId = @ArtistId", artist);
            }

            return i;
        }

        public static int AddArtist(ArtistModel artist)
        {
            var artistId = 0;

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                artist.Name = !string.IsNullOrEmpty(artist.FirstName) ? $"{artist.FirstName} {artist.LastName}" : artist.LastName;

                var foundArtist = cn.QueryFirstOrDefault<ArtistModel>("SELECT * FROM Artist WHERE Name LIKE @Name COLLATE NOCASE", artist);
                if (foundArtist != null)
                {
                    artistId = 9999;
                }
                else
                {
                    var number = cn.Execute("INSERT INTO Artist (FirstName, LastName, Name, Biography) VALUES (@FirstName, @LastName, @Name, @Biography)", artist);
                    if (number == 1)
                    {
                        foundArtist = cn.QueryFirstOrDefault<ArtistModel>("SELECT * FROM Artist WHERE Name LIKE @Name COLLATE NOCASE", artist);
                        artistId = foundArtist?.ArtistId ?? 0;
                    }
                }
            }

            return artistId;
        }

        public static int DeleteArtist(int artistId)
        {
            var result = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                // Delete records before deleting artist
                var records = cn.Query<RecordModel>($"SELECT * FROM Record WHERE artistId = {artistId}", new DynamicParameters());
                foreach (var record in records) 
                {
                    cn.Execute($"DELETE FROM Record WHERE ArtistId={artistId}");
                }
                
                result = cn.Execute($"DELETE FROM Artist WHERE ArtistId={artistId}");
            }

            return result;
        }

        /// <summary>
        /// Get biography from the current record Id.
        /// </summary>
        public static string GetBiography(int artistId)
        {
            var biography = new StringBuilder();

            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var artist = cn.Query<ArtistModel>("SELECT * FROM Artist WHERE artistId = @artistId", new { artistId }).FirstOrDefault();
                if (artist is ArtistModel)
                {
                    biography.Append($"Name: {artist.Name}\n");
                    biography.Append($"Biography:\n{artist.Biography}");
                }
            }

            return biography.ToString();
        }

        public static List<ArtistModel> GetArtistsWithNoBio()
        {
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                var artists = cn.Query<ArtistModel>("SELECT * FROM Artist WHERE Biography IS NULL OR Biography = '';").ToList();

                return artists;
            }
        }

        public static int NoBiographyCount()
        {
            var number = 0;
            using (IDbConnection cn = new SQLiteConnection(LoadConnectionString()))
            {
                number = cn.Query<ArtistModel>("SELECT * FROM Artist WHERE Biography IS NULL OR Biography = '';").Count();
            }

            return number;
        }

        private static string LoadConnectionString(string id = "RecordDB")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}