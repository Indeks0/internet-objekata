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
        public async Task<IActionResult> GetIsOnAsync()
        {
            bool isEnabled = false;
            float temperature = 0;
            using (NpgsqlConnection con = GetConnection())
            {
                string sql = "SELECT isenabled,temperature FROM toggleinfo WHERE id = 0";
                con.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        isEnabled = bool.Parse(reader[0].ToString());
                        temperature = float.Parse(reader[1].ToString());
                        //do whatever you like
                    }
                }

                con.Close();
            }
            return Ok(new
            {
                isEnabled = isEnabled,
                temperature = temperature,
            });
        }

        [HttpPost]
        [Produces("application/json")]
        [Route("post-info")]
        public async Task<IActionResult> PostInfoAsync(PostInfoRest postInfo)
        {
            using (NpgsqlConnection con = GetConnection())
            {
                var dateCreated = DateTime.Now;
                string sql = $"INSERT INTO temperaturetime (temp, tempsensor, datecreated, ison) VALUES (:temp, :tempsensor, :datecreated, :ison)";
                con.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    try
                    {
                        command.Parameters.AddWithValue("temp", float.Parse(postInfo.temp));
                        command.Parameters.AddWithValue("tempsensor", float.Parse(postInfo.tempSensor));
                        command.Parameters.AddWithValue("datecreated", dateCreated);
                        command.Parameters.AddWithValue("ison", bool.Parse(postInfo.isEnabled));
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    con.Close();
                }
            }
            return Ok();
        }

        [HttpPut]
        [Produces("application/json")]
        [Route("update-isEnabled")]
        public async Task<IActionResult> UpdateIsEnabledAsync(bool nextValue)
        {
            using (NpgsqlConnection con = GetConnection())
            {
                var dateCreated = DateTime.Now;
                string sql = $"UPDATE toggleinfo SET isenabled = :nextvalue WHERE id = 0";
                con.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    try
                    {
                        command.Parameters.AddWithValue("nextvalue", nextValue);
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    con.Close();
                }
            }
            return Ok();
        }

        private static NpgsqlConnection GetConnection()
        {
            var con = new NpgsqlConnection("Server=tyke.db.elephantsql.com;Database=rrduyjdv;User Id=rrduyjdv;Password=caxhs8GGaKovQ59S5LQFxpPAg1p2Ezwt");
            con.Settings.KeepAlive = 300;
            return con;
        }

        #endregion Methods

        public class PostInfoRest
        {
            public string temp { get; set; }
            public string tempSensor { get; set; }
            public string isEnabled { get; set; }
        }
    }
}