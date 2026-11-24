namespace CK_SalesApi.ViewModel
{
    public class EmployeeLogsVM :ViewsVM
    {
        public string? UserId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? CheckTimeText { get; set; }
        public string? MyDate { get; set; }
        public string? MyTime { get; set; }
        public int? InOut { get; set; }
        public int MachineNumber { get; set; }
    }

}
