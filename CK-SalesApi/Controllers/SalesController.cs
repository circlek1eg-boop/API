using CK_SalesApi.ViewModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace CK_SalesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly string connStr =
            "Server=192.168.1.156;User ID=sa;Password=S@leh2024;Database=TopSoft;Connect Timeout=10200;Encrypt=False;TrustServerCertificate=True;";
        public string RmsConnection = string.Format("Server=192.168.1.156;User ID=sa;Password=S@leh2024;Database=DATA_CENTER;Connect Timeout=10200;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;");
        private const string ApiKey = "shfalsaierwyrqpafknknvsj3434lakdf34535";
        private const string ApiKeySodic = "sT35hfK345saierwyrqpafknknvsj3434lakdf34535";

        [HttpGet("GetSalesPerTrans")]
        public async Task<IActionResult> GetSalesPerTrans(
       [FromHeader(Name = "x-api-key")] string apiKey, string startDate,
            string endDate,
            string? branchName = null)
        {
            if (string.IsNullOrWhiteSpace(ApiKeySodic) || apiKey != ApiKeySodic)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid API Key."
                });
            }
            try
            {
                SalesSodicVM vm = new SalesSodicVM();
                List<SalesSodicVM> list = new List<SalesSodicVM>();

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();

                    string q = @"
SELECT Sum(SVC)SVC,
    StoreId,
    StoreName,
TransDate,
DateandTime,
    TransactionNumber,
 sum(TotalSalesWithoutTaxSVC)TotalSalesWithoutTaxSVC,
    sum(TotalSales)TotalSales,
    Sum(Tax)Tax,
    Sum(TotalSalesWithoutTax)TotalSalesWithoutTax--,
  --  Vat
FROM
(
    " + vm.RptSalesAll() + @"
) Sales
WHERE dpName !='Cigarettes' and  StoreName in ('SODIC','SODIC_WMC') and TransDate BETWEEN @StartDate AND @EndDate ";

                    if (!string.IsNullOrEmpty(branchName))
                    {
                        q += @" AND StoreName = @BranchName ";
                    }
                    q += @" group by  StoreId,
    StoreName,
TransDate,
DateandTime,
    TransactionNumber ";
                    // Optional: Order by date/transaction if needed
                    // q += @" ORDER BY TransDate DESC";

                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        if (!string.IsNullOrEmpty(branchName))
                        {
                            cmd.Parameters.AddWithValue("@BranchName", branchName);
                        }

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                list.Add(new SalesSodicVM
                                {
                                    TransDate = dr["TransDate"] == DBNull.Value
    ? string.Empty
    : Convert.ToDateTime(dr["TransDate"]).ToString("yyyy-MM-dd"),
                                     StoreId = dr["StoreId"] == DBNull.Value
                                        ? string.Empty
                                        : dr["StoreId"].ToString(),

                                    BranchName = dr["StoreName"] == DBNull.Value
                                        ? string.Empty
                                        : dr["StoreName"].ToString(),

                                    TransactionNumber = dr["TransactionNumber"] == DBNull.Value
                                        ? string.Empty
                                        : dr["TransactionNumber"].ToString(),
                                    DateandTime = dr["DateandTime"] == DBNull.Value
                                        ? string.Empty
                                        : dr["DateandTime"].ToString(),

                                    TotalSalesWithTaxSVC = dr["TotalSales"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(dr["TotalSales"]),

                                    Tax = dr["Tax"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(dr["Tax"]),
                                    SVC = dr["SVC"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(dr["SVC"]),

                                    TotalSalesWithoutTaxSVC = dr["TotalSalesWithoutTaxSVC"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(dr["TotalSalesWithoutTaxSVC"]),

                                    //Vat = dr["Vat"] == DBNull.Value
                                    //    ? 0
                                    //    : Convert.ToDecimal(dr["Vat"])
                                });
                            }
                        }
                    }
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet("GetSalesData")]
        public async Task<IActionResult> GetSalesData(
      [FromHeader(Name = "x-api-key")] string apiKey, string startDate,
            string endDate,
            string? branchName = null)
        {
            try
            {
                SalesVM vm = new SalesVM();

                List<SalesVM> list = new List<SalesVM>();

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();

                    string q = @"
SELECT 
    StoreName,
    SUM(TotalSales) AS TotalSales
FROM
(
    " + vm.RptSalesAll() + @"
) Sales

WHERE TransDate BETWEEN @StartDate AND @EndDate ";
                    if (!string.IsNullOrEmpty(branchName))
                    {
                        q += @"   AND StoreName = @BranchName ";
                    }
                    q += @" GROUP BY StoreName";

                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        if (!string.IsNullOrEmpty(branchName))
                        {
                            cmd.Parameters.AddWithValue("@BranchName", branchName);
                        }

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                list.Add(new SalesVM
                                {
                                    BranchName = dr["StoreName"] == DBNull.Value
            ? string.Empty
            : dr["StoreName"].ToString(),

                                    TotalSales = dr["TotalSales"] == DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(dr["TotalSales"])
                                });
                            }
                        }
                    }
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet("GetItemByBarcode")]
        public async Task<IActionResult> GetItemByBarcode(
            [FromHeader(Name = "x-api-key")] string apiKey,
            string? barcode)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != ApiKey)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid API Key."
                });
            }

            try
            {
                using SqlConnection con = new SqlConnection(RmsConnection);
                await con.OpenAsync();

                string query;

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    // Fetch all items
                    query = @"
                               SELECT distinct  
                    ItemLookupCode AS BarCode,
                    Description AS ItemName,
                    Item.Price,
                    CASE
    WHEN ItemLookupCode LIKE '20%' 
         AND ItemLookupCode LIKE '%000000'
    THEN 'gram'
    ELSE 'Piece'
END AS UnitOfMeasure
                FROM RichCut_DATA_CENTER.dbo.Item
                inner join RichCut_DATA_CENTER.dbo.TransactionEntry Sales on Sales.ItemId=Item.Id
                WHERE Item.StoreID = 1 and inactive=0 and cast(Sales.TransactionTime as date) >=cast(Getdate()-180 as date) ";
                }
                else
                {
                    // Fetch specific item
                    query = @"
                SELECT TOP 1
                    ItemLookupCode AS BarCode,
                    Description AS ItemName,
                    Price,
                    UnitOfMeasure
                FROM RichCut_DATA_CENTER.dbo.Item
                WHERE StoreID = 1
                  AND ItemLookupCode = @Barcode";
                }

                using SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                }

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                var items = new List<object>();

                while (await dr.ReadAsync())
                {
                    items.Add(new
                    {
                        BarCode = dr["BarCode"]?.ToString(),
                        ItemName = dr["ItemName"]?.ToString(),
                        Price = dr["Price"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(dr["Price"]),
                        UnitOfMeasure = dr["UnitOfMeasure"]?.ToString()
                    });
                }

                if (items.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Item not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    count = items.Count,
                    data = items
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet("SearchItems")]
        public async Task<IActionResult> SearchItems(
    [FromHeader(Name = "x-api-key")] string apiKey,
    string? items = null,
    string? categories = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != ApiKey)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid API Key."
                });
            }

            try
            {
                using SqlConnection con = new SqlConnection(connStr);
                await con.OpenAsync();

                var query = new StringBuilder(@"
            SELECT DISTINCT
                ItemLookupCode_Trim,
                dpname
            FROM ItemsData
            WHERE 1 = 1
        ");

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                // Filter Items
                if (!string.IsNullOrWhiteSpace(items))
                {
                    var itemList = items
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    if (itemList.Any())
                    {
                        var parameters = new List<string>();

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            string param = $"@item{i}";
                            parameters.Add(param);
                            cmd.Parameters.AddWithValue(param, itemList[i]);
                        }

                        query.Append($" AND ItemLookupCode_Trim IN ({string.Join(",", parameters)})");
                    }
                }

                // Filter Categories
                if (!string.IsNullOrWhiteSpace(categories))
                {
                    var catList = categories
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    if (catList.Any())
                    {
                        var parameters = new List<string>();

                        for (int i = 0; i < catList.Count; i++)
                        {
                            string param = $"@cat{i}";
                            parameters.Add(param);
                            cmd.Parameters.AddWithValue(param, catList[i]);
                        }

                        query.Append($" AND dpname IN ({string.Join(",", parameters)})");
                    }
                }

                cmd.CommandText = query.ToString();

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                var result = new List<object>();

                while (await dr.ReadAsync())
                {
                    result.Add(new
                    {
                        ItemCode = dr["ItemLookupCode_Trim"]?.ToString(),
                        Category = dr["dpname"]?.ToString()
                    });
                }

                return Ok(new
                {
                    success = true,
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}