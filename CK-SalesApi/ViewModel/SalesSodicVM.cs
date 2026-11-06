using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

namespace CK_SalesApi.ViewModel
{
    public class SalesSodicVM : ViewsVM
    {


        public string TransDate { get; set; }
        public string DateandTime { get; set; }
        public string StoreId { get; set; }
        public string BranchName { get; set; }
        public string TransactionNumber { get; set; }
        
        public decimal TotalSalesWithTaxSVC { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalSalesWithoutTaxSVC { get; set; }
        public decimal Tax { get; set; }
        public decimal SVC { get; set; }
    }
}
