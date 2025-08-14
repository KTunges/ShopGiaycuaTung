using System;
using System.Collections.Generic;

namespace MVC_template.Models.ViewModels
{
    public class ProductSearchRequest
    {
        public string? Q { get; set; }                  // Từ khóa
        public string? SupplierId { get; set; }         // Mã nhà cung cấp
        public decimal? MinPrice { get; set; }          // Giá tối thiểu
        public decimal? MaxPrice { get; set; }          // Giá tối đa
        public bool InStockOnly { get; set; } = true;   // Chỉ còn hàng
        public string? Sort { get; set; }               // price_asc, price_desc, newest, bestseller
        public int Page { get; set; } = 1;              // Trang
        public int PageSize { get; set; } = 12;         // Số sp/trang
    }

    public class ProductListItem
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string SupplierName { get; set; } = "";
        public int Sold { get; set; }                   // Dùng khi sort theo bán chạy
    }

    public class ProductSearchResult
    {
        public ProductSearchRequest Request { get; set; } = new();
        public List<ProductListItem> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Math.Max(TotalItems, 1) / Math.Max(Request.PageSize, 1));
        public List<(string SupplierId, string SupplierName)> Suppliers { get; set; } = new();
    }
}
