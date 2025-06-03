using System;
using System.Collections.Generic;

namespace DummyProject.Models.ViewModel.Analytics
{
    public class AnalyticsModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SalesDataModel> SalesData { get; set; } = [];
        public List<ProductCategoryModel> ProductStats { get; set; } = new();
    }

    public class SalesDataModel
    {
        public string Date { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductCategoryModel
    {
        public string Category { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
    }
}
