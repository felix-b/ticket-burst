using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TicketBurst.Tests.TryOut;

[TestFixture(Category = "tryout")]
public class TickedPdfTests
{
    [Test]
    public void HelloWorld()
    {
        
        Document.Create(container => {
            container.Page(page => {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));
    
                page.Header()
                    .Text("Hello PDF!")
                    .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Column(column => {
                        var text = Placeholders.Paragraph();

                        foreach (var i in Enumerable.Range(2, 5))
                        {
                            RenderTicket(column);
                            column.Item().Padding(0.1f, Unit.Centimetre);
                        }
                    });
                
                page.Footer()
                    .AlignCenter()
                    .Text(x => {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        })
        .GeneratePdf("D:\\hello.pdf");        
    }

    private static void RenderTicket(ColumnDescriptor column)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
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
                    gridIn.Item(columns: 4).AlignCenter().AlignBottom().Text("22 August 2024").FontSize(12).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 2).AlignCenter().AlignBottom().Text("Price C");
                    gridIn.Item();

                    gridIn.Item();
                    gridIn.Item().AlignCenter().AlignTop().Text("119").FontSize(13).Bold();
                    gridIn.Item().AlignCenter().AlignTop().Text("18").FontSize(13).Bold();
                    gridIn.Item().AlignCenter().AlignTop().Text("25").FontSize(13).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 4).AlignCenter().AlignTop().Text("18:30").FontSize(13).Bold();
                    gridIn.Item();
                    gridIn.Item(columns: 2).AlignCenter().AlignTop().Text("$120").FontSize(13).Bold();
                    gridIn.Item();

                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text("Football 1/4 Final Germany - Brazil").FontSize(16).Bold();

                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text("Neo Quimica Arena").FontSize(14).Bold();
                    gridIn.Item(columns: 13).AlignCenter().AlignMiddle().Text("Avenida Miguel Ignacio Curi, 111 Sao Paulo, Brazil").FontSize(12);
                });
            });
    }
}
