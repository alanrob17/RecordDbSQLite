using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _ad = DAL.ArtistDataAccess;

namespace RecordDbSQLite.Tests
{
    public class ArtistTest
    {
        internal static void CreateArtist()
        {
            var artist = new ArtistModel
            {
                FirstName = "James",
                LastName = "Robson",
                Biography = "This is the Bio for James Robson."
            };

            var artistId = _ad.AddArtist(artist);

            if (artistId > 0 && artistId < 9999)
            {
                Console.WriteLine($"Artist added with Id: {artistId}.");
            }
            else if (artistId == 9999)
            {
                Console.WriteLine("ERROR: Artist already exists in the database!");
            }
            else
            {
                Console.WriteLine("ERROR: Artist couldn't be added to the database!");
            }
        }

        internal static void GetArtistByName()
        {
            var artistToFind = new ArtistModel { Name = "Bob Dylan" };

            var artist = _ad.GetArtistByName(artistToFind);
            var message = artist?.ArtistId > 0 ? $"Id: {artist.ArtistId} - {artist.FirstName} {artist.LastName}." : "ERROR: Artist not found!";

            Console.WriteLine(message);
        }

        internal static void GetAllArtists()
        {
            var artists = _ad.GetArtists();

            foreach (var artist in artists)
            {
                PrintArtist(artist);
            }
        }

        internal static void PrintArtist(ArtistModel artist)
        {
            try
            {
                var bio = string.IsNullOrEmpty(artist.Biography) ? "No Biography" : (artist.Biography.Length > 30 ? artist.Biography.Substring(0, 30) + "..." : artist.Biography);
                Console.WriteLine($"Id: {artist.ArtistId} - {artist.FirstName} {artist.LastName}\n{bio}\n");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"No artist was found.\n\n{ex.Message}");
            }
        }

        internal static void UpdateArtist()
        {
            ArtistModel artist = new()
            {
                ArtistId = 823,
                FirstName = "Alan",
                LastName = "Robson",
                Biography = "Alan is an Australian C&W superstar."
            };

            var i = _ad.UpdateArtist(artist);

            var message = i > 0 ? "Artist updated." : "ERROR: Artist not updated!";
            Console.WriteLine(message);

        }

        internal static void GetArtistById(int artistId)
        {
            var artist = _ad.GetArtistById(artistId);
            var message = artist?.ArtistId > 0 ? $"Id: {artist.ArtistId} - {artist.FirstName} {artist.LastName}." : "ERROR: Artist not found!";

            Console.WriteLine(message);
        }

        internal static void DeleteArtist(int artistId)
        {
            int i = _ad.DeleteArtist(artistId);
            var message = i > 0 ? "Artist deleted." : "ERROR: Artist not deleted!";
            Console.WriteLine(message);
        }

        internal static void GetBiography(int artistid)
        {
            var biography = _ad.GetBiography(artistid);

            if (biography.Length > 5)
            {
                Console.WriteLine(biography);
            }
        }

        internal static void ArtistHtml(int artistId)
        {
            var artist = _ad.GetArtistById(artistId);
            var message = artist?.ArtistId > 0 ? $"<p><strong>Id:</strong> {artist.ArtistId}</p>\n<p><strong>Name:</strong> {artist.FirstName} {artist.LastName}</p>\n<p><strong>Biography:</strong>\n<div>{artist.Biography}</p></div>" : "ERROR: Artist not found!";

            Console.WriteLine(message);
        }

        internal static void GetArtistId(string firstName, string lastName)
        {
            var artistToFind = new ArtistModel { FirstName = firstName, LastName = lastName };

            var artist = _ad.GetArtistByFirstLastName(artistToFind);
            var message = artist?.ArtistId > 0 ? $"Id: {artist.ArtistId} - {artist.FirstName} {artist.LastName}." : "ERROR: Artist not found!";

            Console.WriteLine(message);

        }

        internal static void GetArtistsWithNoBio()
        {
            List<ArtistModel> artists = _ad.GetArtistsWithNoBio();

            foreach (var artist in artists) 
            {
                Console.WriteLine($"Id: {artist.ArtistId} - {artist.Name}");
            }
        }

        internal static void GetNoBiographyCount()
        {
            var number = _ad.NoBiographyCount();

            Console.WriteLine($"The total number of artists with missing biographies: {number}.");
        }
    }
}
