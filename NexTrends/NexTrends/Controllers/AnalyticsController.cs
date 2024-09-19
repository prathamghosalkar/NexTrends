using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NexTrends.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly NexTrendsContext _context;

        public AnalyticsController(NexTrendsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Analysis(DateTime? startDate, DateTime? endDate, int[] categoryIds = null)
        {
            // Default to the last 7 days if no date range is provided
            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = DateTime.Today.AddDays(-6);  // Last 7 days (including today)
                endDate = DateTime.Today;
            }

            // Ensure the dates are in the correct order (startDate <= endDate)
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // Fetch relevant sales data for the date range
            var query = from order in _context.Orders
                        join cart in _context.Carts on order.CartId equals cart.Id
                        join product in _context.Products on cart.ProductId equals product.Id
                        where order.OrderDate.HasValue
                              && order.OrderDate.Value.Date >= startDate
                              && order.OrderDate.Value.Date <= endDate
                        select new
                        {
                            Date = order.OrderDate.Value.Date,
                            Amount = order.Amount,
                            CategoryId = product.CategoryId,
                            CategoryTitle = product.Category.Name,
                        };

            // Apply category filter if selected
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(q => categoryIds.Contains(q.CategoryId));
            }

            var salesData = await query.ToListAsync();

            // Total Sales Calculation
            decimal totalSales = salesData.Sum(s => s.Amount);
            var cultureInfo = new CultureInfo("en-IN");
            ViewBag.TotalSales = totalSales.ToString("C0", cultureInfo);

            // Doughnut Chart - Sales by Category
            ViewBag.DoughnutChartData = salesData
                .GroupBy(s => s.CategoryId)
                .Select(g => new
                {
                    categoryTitleWithIcon = g.First().CategoryTitle,
                    amount = g.Sum(s => s.Amount),
                    formattedAmount = g.Sum(s => s.Amount).ToString("C0", cultureInfo)
                })
                .OrderByDescending(g => g.amount)
                .ToList();

            // Spline Chart - Sales Trend Over the Date Range
            var salesSummary = salesData
                .GroupBy(s => s.Date)
                .Select(g => new SplineChartData
                {
                    Day = g.Key.ToString("dd-MMM"),
                    Sales = g.Sum(s => s.Amount)
                })
                .ToList();

            // Generate the date range for the spline chart
            var dateRange = Enumerable.Range(0, (endDate.Value - startDate.Value).Days + 1)
                                      .Select(i => startDate.Value.AddDays(i).ToString("dd-MMM"))
                                      .ToArray();

            // Combine sales data with the date range to ensure continuity
            ViewBag.SplineChartData = from day in dateRange
                                      join sales in salesSummary on day equals sales.Day into salesJoined
                                      from sales in salesJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          sales = sales?.Sales ?? 0
                                      };

            // Categories for dropdown (filter option)
            var categories = await _context.Categories.ToListAsync();

            // Calculate total orders and total customers
            int totalOrders = await _context.Orders
                                            .Where(o => o.OrderDate.HasValue
                                                    && o.OrderDate.Value.Date >= startDate
                                                    && o.OrderDate.Value.Date <= endDate)
                                            .CountAsync();

            int totalCustomers = await _context.Orders
                                                .Where(o => o.OrderDate.HasValue
                                                        && o.OrderDate.Value.Date >= startDate
                                                        && o.OrderDate.Value.Date <= endDate)
                                                .Select(o => o.Cart.CustomerId)
                                                .Distinct()
                                                .CountAsync();

            // Calculate recent total orders by category
            var recentOrdersByCategory = salesData
                .GroupBy(s => s.CategoryTitle)
                .Select(g => new
                {
                    CategoryTitle = g.Key,
                    TotalOrders = g.Count()
                })
                .ToList();

            // Prepare the ViewModel
            var viewModel = new ChartData
            {
                Categories = categories,
                SelectedCategoryIds = categoryIds ?? Array.Empty<int>(),  // Ensure it's an array
                StartDate = startDate,
                EndDate = endDate
            };

            // Set ViewBag properties for total orders and total customers
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.RecentOrdersByCategory = recentOrdersByCategory;

            return View(viewModel);
        }

        // SplineChartData class to handle day-wise sales data for the spline chart
        public class SplineChartData
        {
            public string Day { get; set; }  // For the day (formatted)
            public decimal Sales { get; set; }  // Total sales for the day
        }
    }
}
