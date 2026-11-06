using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

namespace CK_SalesApi.ViewModel
{
    public class SalesVM : ViewsVM
    {
        
        public string BranchName { get; set; }
        
        public decimal TotalSales { get; set; }        

    }
}
