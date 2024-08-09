using BStockBot.Api.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BStockBot.Api.Services
{
    public class ReportGenerationService
    {
        private readonly BStockContext _ctx;
        private readonly string[] _emails;
        private readonly string _endpoint;
        private readonly HttpClient _client;

        public ReportGenerationService(BStockContext ctx, IConfiguration configuration, IHttpClientFactory factory)
        { 
            _ctx = ctx;
            _emails = configuration.GetSection("Emails").Get<string[]>();
            _endpoint = configuration["Endpoint"];
            _client = factory.CreateClient();
        }

        public async Task CreateAuctionListReport(List<Auction> lots)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Auctions");

            int r = 0;
            foreach (var lot in lots)
            {
                r++;
                AddAuctionListHeader(ws, r);
                foreach (var model in lot.Manifest)
                {
                    r++;
                    int c = 0;
                    var row = model.GetRow();
                    foreach (var item in row)
                    {
                        c++;
                        if(c == 2)
                        {
                            ws.Cell(r, c).Value = int.Parse(item);
                        }
                        else
                        {
                            ws.Cell(r, c).Value = item;
                        }
                    }

                    ws.Cell(r, 14).FormulaA1 = $"(B{r}*M{r})";
                }

                r++;

                ws.Cell(r, 13).Value = "Total";
                ws.Cell(r, 14).FormulaA1 = $"SUBTOTAL(9, N{r - lot.Manifest.Count}:N{r - 1})";
                ws.Cell(r, 2).FormulaA1 = $"SUBTOTAL(9, B{r - lot.Manifest.Count}:B{r - 1})";
                r++;
            }

            r += 2;

            ws.Cell(r, 1).Value = "Total";
            ws.Cell(r, 13).Value = "Grand Total";
            ws.Cell(r, 2).FormulaA1 = $"SUBTOTAL(9, B2:B{r - 1})";
            ws.Cell(r, 14).FormulaA1 = $"SUBTOTAL(9, N2:N{r - 1})";

            ws.Column(14).Width *= 2;
            ws.Columns(13, 14).Style.NumberFormat.Format = "$ #,##0.00";

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            var blob = Convert.ToBase64String(ms.ToArray());
            var endDate = lots.FirstOrDefault().EndDate;
            var data = new
            {
                emails = _emails,
                name = "Auction List Bot",
                subject = "Auction List Closing on " + endDate.ToString("D"),
                body = "Good evening,\n\nThe auction list for auctions closing on " + endDate.ToString("D") + " is attached to this email.\n\nThank you,\nAuction List Bot",
                blobContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                blobName = "AuctionList_" + endDate.ToString("MM-dd-yyyy") + ".xlsx",
                blob
            };

            await _client.PostAsJsonAsync(_endpoint, data);
        }

        void AddAuctionListHeader(IXLWorksheet ws, int r)
        {
            string[] data = { "Lot #", "Qty", "OEM", "Model", "Item Description", "Part #", "Category",
                "Grade", "Package Type", "Capacity", "Color", "End Date", "Cost", "Total" };

            int c = 0;
            foreach (var header in data)
            {
                c++;
                ws.Cell(r, c).Value = header;
            }
        }

        public async Task CreateClosingPriceReport(List<Guid> auctionIds)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Closing Prices");
            var auctions = await _ctx.Auctions.Where(x => auctionIds.Contains(x.Id)).ToListAsync();

            int r = 0;
            foreach (var auction in auctions)
            {
                var lot = auction.Manifest;

                r++;
                AddHeader(ws, r);
                int rr = 0;
                foreach (var model in lot)
                {
                    r++;
                    rr++;
                    int c = 0;
                    model.EndDate = auction.EndDate.ToShortDateString();
                    var row = model.GetRow();
                    foreach (var item in row)
                    {
                        c++;
                        if (c == 2)
                        {
                            ws.Cell(r, c).Value = int.Parse(item);
                        }
                        else
                        {
                            ws.Cell(r, c).Value = item;
                        }
                    }

                    if (rr == 1)
                    {
                        ws.Cell(r, 13).Value = auction.Price;
                        ws.Cell(r, 15).Value = auction.AuctionId;
                    }
                }

                r++;

                ws.Cell(r, 12).Value = "Cost Per Unit";
                ws.Cell(r, 13).FormulaA1 = $"(M{r - lot.Count}/B{r})";
                ws.Cell(r, 2).FormulaA1 = $"SUBTOTAL(9, B{r - lot.Count}:B{r - 1})";
                r++;
            }

            r += 2;
            ws.Cell(r, 1).Value = "Total";
            ws.Cell(r, 12).Value = "Grand Total";
            ws.Cell(r, 2).FormulaA1 = $"SUBTOTAL(9, B2:B{r - 1})";
            ws.Cell(r, 13).FormulaA1 = $"SUBTOTAL(9, M2:M{r - 1})";

            ws.Column(15).Hide();
            ws.Columns(13, 14).Style.NumberFormat.Format = "$ #,##0.00";

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            var blob = Convert.ToBase64String(ms.ToArray());
            var endDate = auctions.FirstOrDefault().EndDate;
            var data = new
            {
                emails = _emails,
                name = "Auction Closing Prices Bot",
                subject = "Auction Closing Prices for " + endDate.ToString("D"),
                body = "Good evening,\n\nThe closing prices for " + endDate.ToString("D") + " is attached to this email.\n\nThank you,\nAuction Closing Prices Bot",
                blobContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                blobName = "ClosingPrices_" + endDate.ToString("MM-dd-yyyy") + ".xlsx",
                blob
            };

            await _client.PostAsJsonAsync(_endpoint, data);
        }

        static void AddHeader(IXLWorksheet ws, int r)
        {
            string[] data = { "Lot #", "Qty", "OEM", "Model", "Item Description", "Part #", "Category",
                "Grade", "Package Type", "Capacity", "Color", "End Date", "Winning Bid" };

            int c = 0;
            foreach (var header in data)
            {
                c++;
                ws.Cell(r, c).Value = header;
            }
        }
    }
}
