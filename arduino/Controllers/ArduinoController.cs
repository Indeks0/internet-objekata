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
        public async Task<IActionResult> GetIsEnabledAsync()
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

        [HttpGet]
        [Produces("application/json")]
        [Route("get-system-toggle-info")]
        public async Task<IActionResult> GetSystemToggleInfoAsync()
        {
            List<SystemToggleInfo> listItems = new List<SystemToggleInfo>();

            using (NpgsqlConnection con = GetConnection())
            {
                string sql = "SELECT * FROM systemtoggleinfo ORDER BY id DESC LIMIT 30";
                con.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var sysToggleInfo = new SystemToggleInfo();
                        sysToggleInfo.id = reader[0].ToString();
                        sysToggleInfo.changedTo = bool.Parse(reader[1].ToString());
                        sysToggleInfo.dateCreated = DateTime.Parse(reader[2].ToString());

                        listItems.Add(sysToggleInfo);
                    }
                }

                con.Close();
            }
            return Ok(new
            {
                items = listItems
            });
        }

        [HttpGet]
        [Produces("application/json")]
        [Route("get-temperature-info")]
        public async Task<IActionResult> GetTemperatureInfoAsync()
        {
            List<TemperatureInfo> listItems = new List<TemperatureInfo>();

            using (NpgsqlConnection con = GetConnection())
            {
                string sql = "SELECT * FROM temperaturetime ORDER BY datecreated DESC LIMIT 30";
                con.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var sysToggleInfo = new TemperatureInfo();
                        sysToggleInfo.id = reader[0].ToString();
                        sysToggleInfo.temp = float.Parse(reader[1].ToString());
                        sysToggleInfo.tempSensor = float.Parse(reader[2].ToString());
                        sysToggleInfo.dateCreated = DateTime.Parse(reader[3].ToString());
                        sysToggleInfo.isEnabled = bool.Parse(reader[4].ToString());

                        listItems.Add(sysToggleInfo);
                    }
                }

                con.Close();
            }
            return Ok(new
            {
                items = listItems
            });
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Route("post-info")]
        public async Task<IActionResult> PostInfoAsync([FromForm] PostInfoRest postInfo)
        {
            try
            {
                using (NpgsqlConnection con = GetConnection())
                {
                    var dateCreated = DateTime.UtcNow;
                    string sql = $"INSERT INTO temperaturetime (temp, tempsensor, datecreated, ison) VALUES (:temp, :tempsensor, :datecreated, :ison)";
                    con.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.AddWithValue("temp", float.Parse(postInfo.temp));
                        command.Parameters.AddWithValue("tempsensor", float.Parse(postInfo.tempSensor));
                        command.Parameters.AddWithValue("datecreated", dateCreated);
                        command.Parameters.AddWithValue("ison", bool.Parse(postInfo.isEnabled));
                        await command.ExecuteNonQueryAsync();

                        con.Close();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
                throw;
            }
        }

        [HttpPost]
        [Produces("application/json")]
        [Route("post-system-toggle-info")]
        public async Task<IActionResult> PostSystemToggleInfoAsync(bool nextValue)
        {
            using (NpgsqlConnection con = GetConnection())
            {
                var dateCreated = DateTime.UtcNow;
                string sql = $"INSERT INTO systemtoggleinfo (changedto, datecreated) VALUES (:nextValue, :datecreated)";
                con.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    try
                    {
                        command.Parameters.AddWithValue("nextValue", nextValue);
                        command.Parameters.AddWithValue("datecreated", dateCreated);
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

        [HttpPut]
        [Produces("application/json")]
        [Route("update-temperature")]
        public async Task<IActionResult> UpdateTemperatureAsync(float nextValue)
        {
            using (NpgsqlConnection con = GetConnection())
            {
                string sql = $"UPDATE toggleinfo SET temperature = :nextvalue WHERE id = 0";
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

        #region Classes

        public class PostInfoRest
        {
            #region Properties

            public string temp { get; set; }
            public string tempSensor { get; set; }
            public string isEnabled { get; set; }

            #endregion Properties
        }

        public class SystemToggleInfo
        {
            #region Properties

            public string id { get; set; }
            public bool changedTo { get; set; }
            public DateTime dateCreated { get; set; }

            #endregion Properties
        }

        public class TemperatureInfo
        {
            #region Properties

            public string id { get; set; }
            public float temp { get; set; }
            public float tempSensor { get; set; }
            public DateTime dateCreated { get; set; }
            public bool isEnabled { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}