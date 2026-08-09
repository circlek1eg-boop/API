using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

namespace CK_SalesApi.ViewModel
{
    public class SalesVM
    {
        
        public string BranchName { get; set; }
        public decimal TotalSales { get; set; }
        public string linkRmsRptSalesView()
        {
            string RptSaleslinkView = @"
                                    select 
                                    dp.code DpID,dp.ID GroupId, dp.Name DpName,
                                    Stores.ID StoreCode ,T.StoreID StoreID,s.username StoreName,s.FRANCHISE StoreFranchise,
                                    T.ItemID ItemID,Item.Description ItemName,Item.ItemLookupCode,
                                    T.TransactionTime Time,T.TransactionTime DateandTime,
                                    Day(T.TransactionTime) ByDay,Month(T.TransactionTime) ByMonth,Year(T.TransactionTime)ByYear, Cast(T.TransactionTime as date) TransDate,T.Quantity Qty,T.Price Price,(T.Quantity * T.Price) TotalSales,
                                    convert(varchar(20),T.TransactionNumber)TransactionNumber,T.Cost,-((T.Cost)*-(T.Quantity)) TotalCostQty,((T.Quantity*T.Price)-(T.Cost*T.Quantity))Profit,T.SalesTax Tax,
                                    (T.Quantity*T.Price)TotalSalesTax,((T.Quantity*T.Price)-T.SalesTax)TotalSalesWithoutTax,((T.Quantity*T.Cost)-T.SalesTax)TotalCostWithoutTax
                                    ,Convert (varchar(20),item.SupplierId )SupplierId,s.company Company";

            RptSaleslinkView += @"
                                    ,s.DManager,s.Username,s.FManager,s.BranchOwner from [192.168.1.156].DATA_CENTER.dbo.[TransactionEntry] T WITH (NOLOCK)
                                    left join  [192.168.1.156].DATA_CENTER.dbo.Item WITH (NOLOCK) on Item.ID=T.ItemID and T.StoreID=Item.storeid
                                    left join  [192.168.1.156].DATA_CENTER.dbo.department dp  WITH (NOLOCK) on dp.ID=Item.DepartmentID and dp.storeid=Item.storeid
                                    left join  [192.168.1.156].DATA_CENTER.dbo.Stores WITH (NOLOCK) on (Stores.STORE_ID=Item.storeid or Item.storeid is null) and Stores.STORE_ID=T.StoreID
                                    left join  [192.168.1.156].CkproUsers.dbo.Storeuser s WITH (NOLOCK) on s.RMSstoNumber=Convert (varchar(20),T.StoreID )";

            return RptSaleslinkView;
        }

        public string RptSalesAll()
        {
            string RptSalesAllView = " ";

                RptSalesAllView = @"select DpID COLLATE SQL_Latin1_General_CP1_CI_AS DpID ,
                         DpName COLLATE SQL_Latin1_General_CP1_CI_AS DpName,
                         convert(varchar(20),StoreId) StoreId,
                         convert(varchar(20),StoreId) StoreIdR,
                         StoreName COLLATE SQL_Latin1_General_CP1_CI_AS StoreName
                         ,StoreFranchise COLLATE SQL_Latin1_General_CP1_CI_AS StoreFranchise,ItemLookupCode,
                         ItemName COLLATE SQL_Latin1_General_CP1_CI_AS ItemName ";

                RptSalesAllView += @",
                         Cast(TransDate as date)TransDate,qty,price,TotalSales,TransactionNumber COLLATE SQL_Latin1_General_CP1_CI_AS TransactionNumber,Cost 
                          ,ByMonth,ByYear,ByDay,0 As StoreIdD,Tax,TotalSalesTax,TotalSalesWithoutTax,(Cost*Qty)TotalCostQty
                          ,DManager COLLATE SQL_Latin1_General_CP1_CI_AS DManager,FManager COLLATE SQL_Latin1_General_CP1_CI_AS FManager,BranchOwner COLLATE SQL_Latin1_General_CP1_CI_AS BranchOwner,Username COLLATE SQL_Latin1_General_CP1_CI_AS Username,Company COLLATE SQL_Latin1_General_CP1_CI_AS Company ";
                
                RptSalesAllView += @"from(" + linkRmsRptSalesView();
               
                RptSalesAllView += ")RptRptSalesNew ";


            RptSalesAllView += @"
                                    union all
                                    select DpId,
                                    r.DpName COLLATE SQL_Latin1_General_CP1_CI_AS DpName,
                                    StoreID as StoreID,
                                    '0' as StoreID,
                                    StoreName StoreNameR,
                                    s.Franchise StoreFranchise ,
                                    r.ItemLookupCode COLLATE SQL_Latin1_General_CP1_CI_AS,r.ItemName COLLATE SQL_Latin1_General_CP1_CI_AS ItemName ";
            RptSalesAllView += @"
            ,Cast(TransDate as date)TransDate,Qty,Price,TotalSales,TransactionNumber,Cost
                                    ,ByMonth,ByYear,ByDay,convert(varchar(20),StoreId) StoreIdD,Tax,TotalSalesTax,TotalSalesWithoutTax,(Cost*Qty)TotalCostQty
                                    ,s.DManager,s.FManager,s.BranchOwner,s.Username,s.Company ";

  
            RptSalesAllView += @" from ";

                RptSalesAllView += " [RptSalesDynamic] r ";
            
            //            if (TMT && TMTNew)
            //                RptSalesAllView += " RptSalesNew r ";
            //else if (TMT)
            //                RptSalesAllView += " RptAX r ";
            //else
            //                RptSalesAllView += " [RptSalesDynamic] r ";
            RptSalesAllView += "        left join Storeuser s on s.storenumber  =Convert (varchar(20),r.StoreID ) collate SQL_Latin1_General_CP1_CI_AS ";
            //if (VItemName)
            //{
            //    RptSalesAllView += @"  left join   (Select distinct  ItemlookupCode,ItemName,DpName from ItemsData ) ca on ca.ItemlookupCode = r.ItemlookupCode";
            //}
           
            RptSalesAllView += @" where r.SalesStatus!='0' 
";
           
            return RptSalesAllView;
        }

    }
}
