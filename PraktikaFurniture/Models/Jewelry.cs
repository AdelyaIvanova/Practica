using System;
using System.Collections.Generic;

namespace PraktikaFurniture.Models;

public partial class Jewelry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Metal { get; set; } = null!;

    public string Gemstone { get; set; } = null!;

    public int Price { get; set; }

    public int StockQuantity { get; set; }
}
