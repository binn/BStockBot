using CsvHelper.Configuration.Attributes;
using System.Text.Json.Serialization;

namespace BStockBot.Worker.Models
{
    public class BStockAuction
    {
        [Name("Lot #")]
        public string LotNo { get; set; }

        [Name("Qty")]
        public string Quantity { get; set; }

        [Name("OEM")]
        public string OEM { get; set; }

        [Name("Model")]
        public string Model { get; set; }

        [Name("Item Description")]
        public string ItemDescription { get; set; }

        [Name("Part #")]
        public string PartNo { get; set; }

        [Name("Category")]
        public string Category { get; set; }

        [Name("Grade")]
        public string Grade { get; set; }

        [Name("Package Type")]
        public string PackageType { get; set; }

        [Name("Capacity")]
        public string Capacity { get; set; }

        [Name("Color")]
        public string Color { get; set; }

        [Name("End Date")]
        public string EndDate { get; set; }

        [Name("URL")]
        [JsonIgnore]
        public string Url { get; set; }

        [Ignore]
        public string AuctionId { get; set; }

        public bool ClosesToday()
        {
            return DateTime.Today.ToString("M/d/yy") == EndDate; 
        }
    }
}
