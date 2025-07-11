namespace DummyProject.Models.ViewModel
{
    public class AdminDashboardStatsViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int LowStockProducts { get; set; }
    }
}