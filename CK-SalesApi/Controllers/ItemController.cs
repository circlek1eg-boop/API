using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly string _connStr;
    private readonly string _apiKey;

    public ItemsController(IConfiguration configuration)
    {
        _connStr = configuration.GetConnectionString("DefaultConnection");
        _apiKey = configuration["ApiSettings:ApiKey"];
    }

    [HttpGet("SearchItems")]
    public async Task<IActionResult> SearchItems(
        [FromHeader(Name = "x-api-key")] string apiKey,
        string? items = null,
        string? categories = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey != _apiKey)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Invalid API Key."
            });
        }

        try
        {
            using SqlConnection con = new SqlConnection(_connStr);
            await con.OpenAsync();

            StringBuilder query = new StringBuilder(@"
                SELECT DISTINCT
                    ItemLookupCode_Trim,
                    dpname
                FROM ItemsData
                WHERE 1 = 1
            ");

            SqlCommand cmd = new SqlCommand
            {
                Connection = con
            };

            // Filter by Items
            if (!string.IsNullOrWhiteSpace(items))
            {
                var itemList = items
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (itemList.Any())
                {
                    List<string> parameters = new();

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        string parameterName = $"@item{i}";
                        parameters.Add(parameterName);
                        cmd.Parameters.AddWithValue(parameterName, itemList[i]);
                    }

                    query.Append($" AND ItemLookupCode_Trim IN ({string.Join(",", parameters)})");
                }
            }

            // Filter by Categories
            if (!string.IsNullOrWhiteSpace(categories))
            {
                var categoryList = categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (categoryList.Any())
                {
                    List<string> parameters = new();

                    for (int i = 0; i < categoryList.Count; i++)
                    {
                        string parameterName = $"@cat{i}";
                        parameters.Add(parameterName);
                        cmd.Parameters.AddWithValue(parameterName, categoryList[i]);
                    }

                    query.Append($" AND dpname IN ({string.Join(",", parameters)})");
                }
            }

            cmd.CommandText = query.ToString();

            List<object> data = new();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                data.Add(new
                {
                    ItemCode = reader["ItemLookupCode_Trim"]?.ToString(),
                    Category = reader["dpname"]?.ToString()
                });
            }

            return Ok(new
            {
                success = true,
                count = data.Count,
                data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
