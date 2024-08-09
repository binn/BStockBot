namespace BStockBot.Api.Data
{
    public class BStockAuction
    {
        public string LotNo { get; set; }
        public string Quantity { get; set; }
        public string OEM { get; set; }
        public string Model { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public string Category { get; set; }
        public string Grade { get; set; }
        public string PackageType { get; set; }
        public string Capacity { get; set; }
        public string Color { get; set; }
        public string EndDate { get; set; }
        public string AuctionId { get; set; }

        public bool ClosesToday()
        {
            return DateTime.Today.ToString("M/d/yy") == EndDate;
        }

        public string[] GetRow()
        {
            return new string[]
            {
                LotNo,
                Quantity,
                OEM,
                Model,
                ItemDescription,
                PartNo,
                Category,
                Grade,
                PackageType,
                Capacity,
                Color,
                EndDate
            };
        }
    }
}