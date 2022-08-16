using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace arduino.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArduinoController : ControllerBase
    {
        #region Fields

        private readonly ILogger<ArduinoController> _logger;

        #endregion Fields

        #region Constructors

        public ArduinoController(ILogger<ArduinoController> logger)
        {
            _logger = logger;
        }

        #endregion Constructors

        #region Methods

        [HttpGet]
        [Produces("application/json")]
        [Route("is-enabled")]
        public IActionResult Get()
        {
            bool result = false;
            using (NpgsqlConnection con = GetConnection())
            {
                string sql = "SELECT value FROM ison WHERE id = 0";
                con.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result = bool.Parse(reader[0].ToString());
                        //do whatever you like
                    }
                }
            }
            return Ok(new
            {
                value=result,
            });
        }

        private static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection("Server=tyke.db.elephantsql.com;Database=rrduyjdv;User Id=rrduyjdv;Password=caxhs8GGaKovQ59S5LQFxpPAg1p2Ezwt");
        }

        #endregion Methods
    }
}