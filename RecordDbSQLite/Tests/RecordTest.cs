using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _at = RecordDbSQLite.Tests.ArtistTest;
using _ad = DAL.ArtistDataAccess;
using _rd = DAL.RecordDataAccess;

namespace RecordDbSQLite.Tests
{
    public class RecordTest
    {
        // see GetArtistRecordsMultipleTables() for a better version.
        internal static void GetRecordList()
        {
            var artists = _ad.GetArtists();
            var records = _rd.GetRecords();

            foreach (var artist in artists)
            {
                Console.WriteLine($"{artist.Name}:\n");

                var ar = from r in records
                         where artist.ArtistId == r.ArtistId
                         orderby r.Recorded descending
                         select r;

                foreach (var rec in ar)
                {
                    Console.WriteLine($"\t{rec.Recorded} - {rec.Name} ({rec.Media})");
                }

                Console.WriteLine();
            }
        }

        internal static void GetTotalNumberOfBlurays()
        {
            var total = _rd.GetTotalNumberOfBlurays();
            Console.WriteLine($"Total number of Blu-rays: {total}");
        }

        internal static void GetTotalNumberOfRecords()
        {
            var total = _rd.GetTotalNumberOfRecords();
            Console.WriteLine($"Total number of Records: {total}");
        }

        internal static void GetTotalNumberOfCDs()
        {
            var total = _rd.GetTotalNumberOfCDs();
            Console.WriteLine($"Total number of CD's: {total}");
        }

        internal static void GetTotalNumberOfDiscs()
        {
            var total = _rd.GetTotalNumberOfDiscs();
            Console.WriteLine($"Total number of Discs: {total}");
        }

        internal static void GetRecordsByYear(int year)
        {
            var records = _rd.GetRecordsByYear(year);
            if (records.Count == 0)
            {
                Console.WriteLine($"No records found for {year}");
            }
            else
            {
                foreach (var record in records)
                {
                    PrintArtistRecord(record);
                }
            }
        }

        internal static void GetRecordsByArtistId(int artistId)
        {
            var artist = _ad.GetArtistById(artistId);

            if (artist is ArtistModel)
            {
                _at.PrintArtist(artist);

                Console.WriteLine("\n----------------------------\n");

                var records = _rd.GetRecordsByArtistId(artistId);

                foreach (var record in records)
                {
                    PrintRecord(record);
                }
            }
        }

        internal static void GetRecordsByArtistIdMultipleTables(int artistId)
        {
            var artist = _rd.GetRecordsByArtistIdMultipleTables(artistId);

            if (artist.ArtistId > 0)
            {
                _at.PrintArtist(artist);

                Console.WriteLine("\n----------------------------\n");

                var records = _rd.GetRecordsByArtistId(artistId);

                foreach (var record in records)
                {
                    PrintRecord(record);
                }
            }
        }

        internal static void GetArtistRecordsMultipleTables()
        {
            var artists = _rd.GetArtistRecordsMultipleTables();

            foreach (var artist in artists)
            {
                Console.WriteLine($"\n{artist.Name}");

                foreach (var record in artist.Records)
                {
                    Console.WriteLine($"\t{record.Name}, {record.Recorded} ({record.Media})");
                }
            }
        }

        internal static void GetRecordByName()
        {
            // Note: you can use %% to get a partial name. 
            RecordModel record = new()
            {
                Name = "%Cutting Edge%"
            };

            var newRecord = _rd.GetRecordByName(record);

            PrintArtistRecord(newRecord);
        }

        internal static void GetRecordById(int recordId)
        {
            var record = _rd.GetRecordById(recordId);

            if (record.RecordId > 0)
            {
                PrintArtistRecord(record);
            }
            else
            {
                Console.WriteLine("ERROR: Record not found!");
            }
        }

        internal static void GetAllRecords()
        {
            var records = _rd.GetRecords();

            foreach (var record in records)
            {
                PrintArtistRecord(record);
            }
        }

        internal static void GetArtistById(int artistId)
        {
            var artist = _ad.GetArtistById(artistId);

            if (artist is ArtistModel)
            {
                _at.PrintArtist(artist);

                var rec = new RecordModel()
                {
                    ArtistId = artistId
                };

                var records = _rd.GetRecordsByArtistId(rec.ArtistId);

                foreach (var record in records)
                {
                    PrintArtistRecord(record);
                }
            }
        }

        internal static void PrintRecord(RecordModel record, bool review = false)
        {
            var rev = string.IsNullOrEmpty(record.Review) ? "No Review" : (record.Review.Length > 30 ? record.Review.Substring(0, 30) + "..." : "No review");
            try
            {
                Console.WriteLine($"Id: {record.RecordId} - {record.ArtistId} {record.Name} - {record.Field}, {record.Recorded}, {record.Label}, {record.Pressing}, {record.Rating}, {record.Discs}, {record.Media}, {record.Bought}, ${record.Cost}.\n {rev}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No record found.\n\n{ex.Message}");
            }
        }

        internal static void PrintArtistRecord(RecordModel record)
        {
            var artist = _ad.GetArtistById(record.ArtistId);

            var message = artist is ArtistModel ? $"{artist.Name} - {record.Recorded}: {record.Name} - {record.Field}." : $"Artist or record not found!\n\n";
            Console.WriteLine(message);
        }

        internal static void CreateRecord()
        {
            RecordModel record = new()
            {
                // RecordId = 283,
                ArtistId = 823,
                Name = "No Fun Allowed",
                Field = "Rock",
                Recorded = 1986,
                Label = "Wobble",
                Pressing = "Aus",
                Rating = "***",
                Discs = 1,
                Media = "CD",
                Bought = "01-06-2022",
                Cost = 19.95m,
                Review = "This is Alans first album."

            };

            var result = _rd.AddRecord(record);
            var message = result > 0 ? $"Record added to database." : "ERROR: Record not added to database.";
            Console.WriteLine(message);
        }

        internal static void DeleteRecord(int recordId)
        {
            int i = _rd.DeleteRecord(recordId);
            var message = i > 0 ? "Record deleted." : "ERROR: Record not deleted!";
            Console.WriteLine(message);

        }

        internal static void UpdateRecord()
        {
            RecordModel record = new()
            {
                RecordId = 5251,
                Name = "Plenty Of Fun Allowed",
                Field = "Jazz",
                Recorded = 1988,
                Label = "Wibble",
                Pressing = "Ger",
                Rating = "****",
                Discs = 2,
                Media = "CD",
                Bought = "01-05-2022",
                Cost = 29.95m,
                Review = "This is Alans second album."
            };

            var i = _rd.UpdateRecord(record);

            var message = i > 0 ? "Record updated." : "ERROR: Record not updated!";
            Console.WriteLine(message);

        }

        // Single record view
        internal static void GetRecordListMultipleTables()
        {
            List<dynamic> records = _rd.GetArtistRecordList();

            foreach (dynamic r in records)
            {
                Console.WriteLine($"Artist: {r.Artist} - {r.Name} - {r.Recorded} ({r.Media}) {r.Rating}.");
            }
        }

        internal static void CountDiscs(string media)
        {
            var discs = _rd.CountAllDiscs(media);

            switch (media)
            {
                case "":
                    Console.WriteLine($"The total number of all discs is: {discs}");
                    break;
                case "DVD":
                    Console.WriteLine($"The total number of all DVD, CD/DVD Blu-ray or CD/Blu-ray discs is: {discs}");
                    break;
                case "CD":
                    Console.WriteLine($"The total number of audio discs is: {discs}");
                    break;
                case "R":
                    Console.WriteLine($"The total number of vinyl discs is: {discs}");
                    break;
                default:
                    break;
            }
        }

        internal static void GetArtistRecordEntity(int recordId)
        {
            var r = _rd.GetArtistRecordEntity(recordId);

            if (r.RecordId > 0)
            {
                Console.WriteLine($"{r.Artist}\n");
                Console.WriteLine($"\t{r.Recorded} - {r.Name} ({r.Media}) - Rating: {r.Rating}");
            }
        }

        internal static void GetArtistNumberOfRecords(int artistId)
        {
            var artist = _ad.GetArtistById(artistId);
            var discs = _rd.GetArtistNumberOfRecords(artistId);

            if (artist is ArtistModel)
            {
                Console.WriteLine($"{artist.Name} has {discs} discs.");
            }
        }

        internal static void GetRecordDetails(int recordId)
        {
            var record = _rd.GetFormattedRecord(recordId);

            if (record is RecordModel)
            {
                Console.WriteLine(record.ToString());
            }
        }

        internal static void GetArtistNameFromRecord(int recordId)
        {
            var name = _rd.GetArtistNameFromRecord(recordId);
            Console.WriteLine(name);
        }

        internal static void GetDiscCountForYear(int year)
        {
            var count = _rd.GetDiscCountForYear(year);

            Console.WriteLine($"The total number of discs for {year} are {count}.");
        }

        internal static void GetBoughtDiscCountForYear(string year)
        {
            var count = _rd.GetBoughtDiscCountForYear(year);

            Console.WriteLine($"The total number of discs bought in {year} is {count}.");
        }

        internal static void GetNoRecordReview()
        {
            List<dynamic> records = _rd.MissingRecordReviews();

            foreach (var record in records)
            {
                Console.WriteLine($"{record.Artist} - Id: {record.RecordId} - {record.Name} - {record.Recorded}");
            }
        }

        internal static void GetNoReviewCount()
        {

            var count = _rd.GetNoReviewCount();

            Console.WriteLine($"The total number of empty Reviews is {count}.");
        }

        internal static void GetTotalArtistCost()
        {
            var list = _rd.GetCostTotals();

            foreach (var item in list)
            {
                Console.WriteLine($"Total cost for {item.Name} is ${item.Cost:F2}.");
            }
        }

        internal static void GetTotalArtistDiscs()
        {
            var list = _rd.GetTotalArtistDiscs();

            foreach (var item in list)
            {
                Console.WriteLine($"Total number of discs for {item.Name} is {item.Discs}.");
            }
        }

        internal static void RecordHtml(int recordId)
        {
            var r = _rd.GetArtistRecordEntity(recordId);

            if (r != null)
            {
                Console.WriteLine($"<p><strong>ArtistId:</strong> {r.ArtistId}</p>\n<p><strong>Artist:</strong> {r.Artist}</p>\n<p><strong>RecordId:</strong> {r.RecordId}</p>\n<p><strong>Recorded:</strong> {r.Recorded}</p>\n<p><strong>Name:</strong> {r.Name}</p>\n<p><strong>Rating:</strong> {r.Rating}</p>\n<p><strong>Media:</strong> {r.Media}</p>\n");
            }
        }
    }
}
