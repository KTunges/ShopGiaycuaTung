using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_template.Data;
using MVC_template.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace MVC_template.Controllers
{
    public class ShoppingController : Controller
    {
        private readonly QLWebBanHangContext _context;

        public ShoppingController(QLWebBanHangContext context)
        {
            _context = context;
        }

        // =======================
        // HOME
        // =======================
        public IActionResult Home()
        {
            var homeData = new HomeProduct();

            var newProducts = _context.Products
                .Include(a => a.Supplier)
                .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false")
                .OrderByDescending(a => a.Stt)
                .Take(4);

            var topProducts = _context.OrderDetails
                .GroupBy(a => a.ProductId)
                .OrderByDescending(a => a.Sum(p => p.Quantity ?? 0))
                .Select(a => a.Key)
                .ToList();

            var topSaleProducts = _context.Products
                .Include(a => a.Supplier)
                .Where(a => topProducts.Contains(a.ProductId) && (a.Quantity ?? 0) > 1 && a.IsHide == "false")
                .Take(4);

            homeData.topSaleProducts = topSaleProducts;
            homeData.newProducts = newProducts;
            return View(homeData);
        }

        // =======================
        // UTILS
        // =======================
        public bool CheckQuantity(string productID, int quantity)
        {
            var product = _context.Products.FirstOrDefault(a => a.ProductId == productID);
            if (product == null) return false;
            if ((product.Quantity ?? 0) < quantity) return false;
            return true;
        }

        // =======================
        // DANH SÁCH
        // =======================
        public IActionResult Index()
        {
            var supplier = _context.Suppliers.ToList();
            ViewBag.Supplier = supplier;

            var data = _context.Products
                .Include(a => a.Supplier)
                .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false")
                .ToList();

            return View(data);
        }

        [HttpGet("Shopping/Index/NhaPhanPhoi/{idSupplier}")]
        public IActionResult IndexOrderBySupplier(string idSupplier)
        {
            ViewBag.SupplierId = idSupplier;
            var supplier = _context.Suppliers.ToList();
            ViewBag.Supplier = supplier;

            var data = _context.Products
                .Include(a => a.Supplier)
                .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false" && a.SupplierId == idSupplier)
                .ToList();

            return View("Index", data);
        }

        [HttpGet("Shopping/Index/{orValue}")]
        public IActionResult IndexOrderByPrice(string orValue)
        {
            var supplier = _context.Suppliers.ToList();
            ViewBag.Supplier = supplier;

            if (orValue == "tang")
            {
                var data = _context.Products
                    .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false")
                    .Include(a => a.Supplier)
                    .OrderBy(a => a.Prices);
                return View("Index", data);
            }
            else
            {
                var data = _context.Products
                    .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false")
                    .Include(a => a.Supplier)
                    .OrderByDescending(a => a.Prices);
                return View("Index", data);
            }
        }

        [HttpGet("Shopping/Index/NhaPhanPhoi/{idSupplier}/{orValue}")]
        public IActionResult IndexOrderBySupplierPrice(string idSupplier, string orValue)
        {
            var supplier = _context.Suppliers.ToList();
            ViewBag.SupplierId = idSupplier;
            ViewBag.Supplier = supplier;

            if (orValue == "tang")
            {
                var data = _context.Products
                    .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false" && a.SupplierId == idSupplier)
                    .Include(a => a.Supplier)
                    .OrderBy(a => a.Prices)
                    .ToList();
                return View("Index", data);
            }
            else
            {
                var data = _context.Products
                    .Where(a => (a.Quantity ?? 0) > 1 && a.IsHide == "false" && a.SupplierId == idSupplier)
                    .Include(a => a.Supplier)
                    .OrderByDescending(a => a.Prices)
                    .ToList();
                return View("Index", data);
            }
        }

        // =======================
        // ĐĂNG NHẬP / ĐĂNG KÝ
        // =======================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Login model)
        {
            var userlogin = _context.UserLogins
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.UserName == model.UserName && a.PassWord == model.PassWord);

            if (userlogin == null)
            {
                ViewBag.ThongBao = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            if (userlogin.VaiTro == "customer")
            {
                GlobalValues.CustomerID = userlogin.CustomerId;
                var lastName = userlogin.Customer?.LastName ?? "";
                var firstName = userlogin.Customer?.FirstName ?? userlogin.UserName;
                GlobalValues.Name = $"{lastName} {firstName}".Trim();
                GlobalValues.VaiTro = userlogin.VaiTro;
                return RedirectToAction("Home", "Shopping");
            }
            else
            {
                GlobalValues.VaiTro = userlogin.VaiTro;
                return RedirectToAction("Index", "Products");
            }
        }

        public IActionResult Logout()
        {
            GlobalValues.logOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM account)
        {
            string strRegex = @"0[3789][0-9]{8}";
            Regex re = new Regex(strRegex);
            int a;

            if (account.LastName == null || account.UserName == null || account.FirstName == null || account.Address == null || account.PassWord == null)
            {
                ViewBag.Validate = "Không được để trống trường nào";
                return View();
            }
            else if (!int.TryParse(account.PhoneNumber, out a))
            {
                ViewBag.Validate = "Số điện thoại phải là số";
                return View();
            }
            else if (account.PhoneNumber.Length > 11 || account.PhoneNumber.Length < 10)
            {
                ViewBag.Validate = "Số điện thoại tối đa là 11 số và tối thiểu là 10 số";
                return View();
            }

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = account.FirstName,
                LastName = account.LastName,
                PhoneNumber = account.PhoneNumber,
                Address = account.Address,
            };

            _context.Add(customer);
            _context.SaveChanges();

            var userlogin = new UserLogin
            {
                UserName = account.UserName,
                PassWord = account.PassWord,
                VaiTro = "customer",
                CustomerId = customer.CustomerId,
            };
            _context.Add(userlogin);
            _context.SaveChanges();

            ViewBag.Validate = "Tạo tài khoản thành công";
            return View();
        }

        // =====================================================================
        //  SEARCH (KHÔNG CÒN BRAND) – Supplier + Keyword + Giá + Tồn + Sort
        //  /Shopping/Search?keyword=&supplierId=&minPrice=&maxPrice=&inStockOnly=true&sort=bestseller
        // =====================================================================
        [HttpGet]
        public IActionResult Search(
            string? keyword,
            string? supplierId,
            int? minPrice,
            int? maxPrice,
            bool inStockOnly = true,
            string? sort = null)
        {
            // dữ liệu dropdown NCC + giữ lại tham số trên view
            var supplierList = _context.Suppliers.ToList();
            ViewBag.Supplier = supplierList;
            ViewBag.SupplierId = supplierId;

            // query cơ sở
            var query = _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.IsHide == "false");

            // còn hàng
            if (inStockOnly)
                query = query.Where(p => (p.Quantity ?? 0) > 1);

            // từ khóa theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(p => p.ProductName.Contains(kw));
            }

            // lọc theo NCC
            if (!string.IsNullOrWhiteSpace(supplierId))
                query = query.Where(p => p.SupplierId == supplierId);

            // khoảng giá
            if (minPrice.HasValue) query = query.Where(p => (p.Prices ?? 0) >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => (p.Prices ?? 0) <= maxPrice.Value);

            var data = query.ToList();

            // sắp xếp
            if (!string.IsNullOrWhiteSpace(sort))
            {
                switch (sort)
                {
                    case "price_asc":
                        data = data.OrderBy(p => p.Prices ?? 0).ThenBy(p => p.ProductName).ToList();
                        break;
                    case "price_desc":
                        data = data.OrderByDescending(p => p.Prices ?? 0).ThenBy(p => p.ProductName).ToList();
                        break;
                    case "newest":
                        data = data.OrderByDescending(p => p.Stt ?? 0).ToList();
                        break;
                    case "bestseller":
                        var soldMap = _context.OrderDetails
                            .GroupBy(od => od.ProductId)
                            .Select(g => new { ProductId = g.Key, Sold = g.Sum(x => x.Quantity ?? 0) })
                            .ToDictionary(x => x.ProductId, x => x.Sold);
                        data = data
                            .OrderByDescending(p => soldMap.ContainsKey(p.ProductId) ? soldMap[p.ProductId] : 0)
                            .ThenBy(p => p.ProductName)
                            .ToList();
                        break;
                    default:
                        data = data.OrderBy(p => p.ProductName).ToList();
                        break;
                }
            }
            else
            {
                data = data.OrderBy(p => p.ProductName).ToList();
            }

            return View("Search", data);
        }

        // Tương thích URL cũ: AdvancedSearch => redirect sang Search
        [HttpGet]
        public IActionResult AdvancedSearch(
            string? keyword,
            string? supplierId,
            int? minPrice,
            int? maxPrice,
            bool inStockOnly = true,
            string? sort = null)
        {
            return RedirectToAction(nameof(Search), new
            {
                keyword, supplierId, minPrice, maxPrice, inStockOnly, sort
            });
        }

        // =======================
        // GỢI Ý (Sản phẩm + Nhà cung cấp)
        // Trả về [{ type:"product"/"supplier", text:"...", supplierId:"..." }]
        // =======================
        // Đảm bảo có using:
        // using Microsoft.EntityFrameworkCore;

        [HttpGet("Shopping/SearchSuggest")]
        [Produces("application/json")]
        public IActionResult SearchSuggest(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Json(Array.Empty<object>());

                var key = q.Trim();

                var products = _context.Products
                    .Where(p => p.IsHide == "false"
                                && (p.Quantity ?? 0) > 0
                                && p.ProductName != null
                                && EF.Functions.Like(p.ProductName, $"%{key}%"))
                    .OrderBy(p => p.ProductName)
                    .Select(p => new
                    {
                        id    = p.ProductId,
                        text  = p.ProductName!,
                        image = string.IsNullOrEmpty(p.Image) ? "/Images/Template/no-image.png"
                                                            : $"/Images/Products/{p.Image}",
                        price = (int?)(p.Prices ?? 0)
                    })
                    .Take(10)
                    .ToList();

                return Json(products);
            }
            catch
            {
                return Json(Array.Empty<object>());
            }
        }





        // =======================
        // GIỎ HÀNG / ĐẶT HÀNG
        // =======================
        public IActionResult Delete(string id)
        {
            var product = _context.ShoppingCarts.FirstOrDefault(a => a.ProductId == id && a.CustomerId == GlobalValues.CustomerID);
            if (product == null)
            {
                return View("Error");
            }
            _context.ShoppingCarts.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet("/Product/{id}")]
        public IActionResult ProductDetail(string id)
        {
            var data = _context.Products.Where(a => a.ProductId == id);
            if (data.Count() == 0)
            {
                ViewBag.ChiTiet = "Không tìm thấy sản phẩm";
                return View(data);
            }
            return View(data);
        }

        public IActionResult AddToCart(string id, int price, int quantity = 1)
        {
            if (GlobalValues.CustomerID == null)
            {
                return RedirectToAction("Login");
            }

            var cartItem = _context.ShoppingCarts.SingleOrDefault(a => a.ProductId == id && a.CustomerId == GlobalValues.CustomerID);

            if (cartItem == null)
            {
                var newItem = new ShoppingCart
                {
                    ProductId = id,
                    CustomerId = GlobalValues.CustomerID,
                    Quantity = quantity,
                    Unit = price,
                    Total = price * quantity
                };
                _context.Add(newItem);
                _context.SaveChanges();
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.Total = price * (cartItem.Quantity ?? 0);
                _context.Update(cartItem);
                _context.SaveChanges();
            }

            TempData["alert"] = "Đã thêm vào giỏ hàng";
            return RedirectToAction("Index");
        }

        public IActionResult Cart()
        {
            var data = _context.ShoppingCarts
                .Include(a => a.Product)
                .Where(a => a.CustomerId == GlobalValues.CustomerID)
                .ToList();

            if (data.Count() == 0)
            {
                ViewBag.Cart = "Chưa có sẳn phẩm nào trong giỏ hàng";
                ViewBag.ButtonBuy = "Mua ngay";
            }

            return View(data);
        }

        public IActionResult AddToOrder()
        {
            if (GlobalValues.CustomerID == null)
            {
                return RedirectToAction("Login", "Shopping");
            }

            var customerInfor = _context.Customers.FirstOrDefault(a => a.CustomerId == GlobalValues.CustomerID);
            if (customerInfor == null)
            {
                TempData["alert"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Cart));
            }

            var data = _context.ShoppingCarts
                .Include(a => a.Product)
                .Where(a => a.CustomerId == GlobalValues.CustomerID)
                .ToList();

            foreach (var item in data)
            {
                if (!CheckQuantity(item.ProductId, item.Quantity ?? 0))
                {
                    TempData["alert"] = "Sản phẩm: " + (item.Product?.ProductName ?? item.ProductId) + " đã hết số lượng bạn cần";
                    return RedirectToAction(nameof(Cart));
                }
            }
            if (data.Count() == 0)
            {
                TempData["alert"] = "Trong giỏ chưa có sản phẩm nào";
                return RedirectToAction(nameof(Cart));
            }

            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderDate = DateTime.Now.Date,
                CustomerId = GlobalValues.CustomerID,
                Address = customerInfor.Address,
                Phone = customerInfor.PhoneNumber
            };
            _context.Add(order);
            _context.SaveChanges();

            foreach (var item in data)
            {
                var productQuantity = _context.Products.FirstOrDefault(a => a.ProductId == item.ProductId);
                if (productQuantity != null)
                {
                    productQuantity.Quantity = (productQuantity.Quantity ?? 0) - (item.Quantity ?? 0);
                    _context.Update(productQuantity);
                }

                var orderDetail = new OrderDetail
                {
                    Quantity = item.Quantity,
                    Total = item.Total,
                    Unit = item.Unit,
                    ProductId = item.ProductId,
                    OrderId = order.OrderId
                };

                _context.Remove(item);
                _context.Add(orderDetail);
                _context.SaveChanges();
            }

            order.AmountPaid = data.Sum(a => a.Total ?? 0);
            _context.Update(order);
            _context.SaveChanges();

            TempData["alert"] = "Đã đặt hàng";
            return RedirectToAction("Cart");
        }

        public IActionResult Order()
        {
            var data = _context.Orders.Where(a => a.CustomerId == GlobalValues.CustomerID).ToList();
            return View(data);
        }

        public IActionResult OrderDetail(string id, int ordertotal)
        {
            var data = _context.OrderDetails
                .Include(a => a.Product)
                .Include(a => a.Order)
                .Where(a => a.OrderId == id)
                .ToList();

            var orderInfo = data.FirstOrDefault(a => a.OrderId != null);

            if (orderInfo?.Order?.OrderDate != null)
                ViewBag.OrderDate = orderInfo.Order.OrderDate.Value.ToLongDateString();
            else
                ViewBag.OrderDate = "";

            ViewBag.OrderId = id;
            ViewBag.TotalOrder = ordertotal;
            return View(data);
        }
    }
}
