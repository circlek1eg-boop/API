using CK_SalesApi.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CK_SalesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly string connStr =
            "Server=192.168.1.156;User ID=sa;Password=S@leh2024;Database=TopSoft;Connect Timeout=10200;Encrypt=False;TrustServerCertificate=True;";
        public string RmsConnection = string.Format("Server=192.168.1.156;User ID=sa;Password=S@leh2024;Database=DATA_CENTER;Connect Timeout=10200;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;");
        private const string ApiKey = "QprleHZnFL0gFWq6wPHkhrcmJsKQonLmcqb7Mgk/9Lg=";
       
        [HttpGet("GetEmployeeAttendanceLogs")]
        public async Task<IActionResult> GetEmployeeAttendanceLogs(
    [FromHeader(Name = "x-api-key")] string apiKey,
    string startDate,
    string endDate,
    string? employeeCode = null)
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || apiKey != ApiKey)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid API Key."
                });
            }

            try
            {
                EmployeeLogsVM vm = new EmployeeLogsVM();
                List<EmployeeLogsVM> list = new List<EmployeeLogsVM>();

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    await con.OpenAsync();

                    string q = $"Select * from ( { vm.RptEmployeeLogsView} ) AS EmployeeLogs";

                    if (!string.IsNullOrEmpty(employeeCode))
                    {
                        q += " AND EmployeeCode = @EmployeeCode ";
                    }

                    q += " ORDER BY CheckTimeText ";

                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);

                        if (!string.IsNullOrEmpty(employeeCode))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                        }

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                list.Add(new EmployeeLogsVM
                                {
                                    

                                    EmployeeCode = dr["EmployeeCode"] == DBNull.Value
                                        ? string.Empty
                                        : dr["EmployeeCode"].ToString(),

                                    CheckTimeText = dr["CheckTimeText"] == DBNull.Value
    ? string.Empty
    : Convert.ToDateTime(dr["CheckTimeText"]).ToString("yyyy-MM-dd HH:mm:ss"),

                                    MyDate = dr["MyDate"] == DBNull.Value
    ? string.Empty
    : Convert.ToDateTime(dr["MyDate"]).ToString("yyyy-MM-dd"),

                                    MyTime = dr["MyTime"] == DBNull.Value
                                        ? string.Empty
                                        : dr["MyTime"].ToString(),

                                    InOut = dr["InOut"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(dr["InOut"]),

                                    MachineNumber = Convert.ToInt32(dr["MachineNumber"])
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

    }
}
