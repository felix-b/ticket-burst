using System.Linq;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TicketBurst.Contracts;

namespace TicketBurst.CheckoutService.Logic;

public class TicketRenderer
{
    public byte[] RenderTicketsToPdf(IEnumerable<TicketContract> tickets)
    {
        var document = ComposePdfDocument(tickets);
        return document.GeneratePdf();
    }
    
    public Document ComposePdfDocument(IEnumerable<TicketContract> tickets)
    {
        return Document.Create(container => {
            container.Page(page => {
                page.ContinuousSize(595.4f, Unit.Point);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));

                page.Header()
                    .Text("Tickets - Print at Home")
                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .Column(column => {
                        var text = Placeholders.Paragraph();

                        foreach (var ticket in tickets)
                        {
                            RenderTicket(column, ticket);
                            column.Item().Padding(0.1f, Unit.Centimetre);
                        }
                    });
            });
        });
    }

    private static void RenderTicket(ColumnDescriptor column, TicketContract ticket)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticket.Id, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

        column
            .Item()
            .MinimalBox()
            .Border(0.1f, Unit.Millimetre)
            .Width(15, Unit.Centimetre) // sizes from 80x40 to 240x120
            .Height(3, Unit.Centimetre)
            .ScaleToFit()
            .DefaultTextStyle(x => x.FontSize(9))
            .Grid(gridOut => {
                gridOut.Columns(5);
                gridOut.Item(columns: 1).Image(qrCodeAsPngByteArr, ImageScaling.FitArea);
                gridOut.Item(columns: 4).Background("F0F0F0").Grid(gridIn => {
                    gridIn.Columns(13);
                    
                    gridIn.Item();
                    gridIn.Item().AlignCenter().AlignBottom().Text("Area");
                    gridIn.Item().AlignCenter().AlignBottom().Text("Row");
                    gridIn.Item().AlignCenter().AlignBottom().Text("Seat");
                    gridIn.Item();
                    gridIn.Item(columns: 4).AlignCenter().AlignBottom().Text(ticket.StartLocalTime.ToString("dd MMM yy")).FontSize(12).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 2).AlignCenter().AlignBottom().Text($"Price {ticket.PriceLevelName}");
                    gridIn.Item();

                    gridIn.Item();
                    gridIn.Item().AlignCenter().AlignTop().Text(ticket.AreaName).FontSize(13).Bold();
                    gridIn.Item().AlignCenter().AlignTop().Text(ticket.RowName).FontSize(13).Bold();
                    gridIn.Item().AlignCenter().AlignTop().Text(ticket.SeatName).FontSize(13).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 4).AlignCenter().AlignTop().Text(ticket.StartLocalTime.ToString("HH:mm")).FontSize(13).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 2).AlignCenter().AlignTop().Text($"${ticket.Price}").FontSize(13).Bold();
                    gridIn.Item();

                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text($"{ticket.ShowTitle} {ticket.EventTitle}").FontSize(16).Bold();

                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text(ticket.VenueName).FontSize(14).Bold();
                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text(ticket.VenueAddress).FontSize(12);
                });
            });
    }
}
