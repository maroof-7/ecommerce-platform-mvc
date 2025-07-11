using System.Collections.Generic;

namespace DummyProject.Models.ViewModel
{
    public class AnalyticsModel
    {
        public SalesSummary SalesData { get; set; } = new SalesSummary();
    }

    public class SalesSummary
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ProductCategoryStat> ProductStats { get; set; } = new List<ProductCategoryStat>();
        public List<MonthlySalesData> SalesData { get; set; } = new List<MonthlySalesData>();
    }

    public class ProductCategoryStat
    {
        public string Category { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
    }

    public class MonthlySalesData
    {
        public string date { get; set; } = string.Empty; // e.g. "2025-07"
        public int ordersCount { get; set; }
        public decimal revenue { get; set; }
    }
}