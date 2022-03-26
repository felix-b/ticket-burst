using System;
using System.Drawing;
using System.Globalization;

namespace TicketBurst.Contracts;

public record PriceLevelContract(
    string Id,
    string Name,
    string ColorHexRgb
);
