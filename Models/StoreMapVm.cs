namespace MVC_template.Models
{
    public class BranchVm
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? Phone { get; set; }
        public string? Hours { get; set; }
    }
    public class StoreMapVm
    {
        public string Name { get; set; } = "ỐI DỒI ÔI STORE";
        public string Address { get; set; } = "3F Đ. Nguyễn Hữu Thọ, Tân Hưng, Quận 7, Thành phố Hồ Chí Minh";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Zoom { get; set; } = 16;
        public string ApiKey { get; set; } = "";
        public List<BranchVm> Branches { get; set; } = new();
    }
}
