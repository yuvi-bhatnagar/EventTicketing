using System.Data;
using EventTicketing.Dto;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace EventTicketing.Services
{
    public interface IAdminServices
    {
        Task<bool> Login(UserDto userDto);
        Task<bool> AddEvent(EventDto eventDto);
        Task<bool> UpdateEvent(EventDto eventDto,int id);
        Task<bool> DeleteEvent(int id);
        Task<EventDto> GetEvent(int id);
        bool ReadXlsxFile();
    }
    public class AdminServices:IAdminServices
    {
        public IDBConnection _db { get; set; }
        public IConfiguration _config { get; set; }
        public AdminServices(IDBConnection db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<bool> Login(UserDto userDto)
        {
            try
            {
                if (userDto.email == null || userDto.password == null)
                {
                    return false;
                }

                string sql = "SELECT password FROM admin WHERE email = @email";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@email", userDto.email);
                        var hashedPassword = cmd.ExecuteScalar() as string;

                        if (hashedPassword != null)
                        {
                            if (BCrypt.Net.BCrypt.Verify(userDto.password, hashedPassword))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

//for Adding Event
        public async Task<bool> AddEvent(EventDto eventDto)
        {
            bool status = false;
            try
            {
                string sql1 = "select * from events where name=@name";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql1, con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@name", eventDto.Name);
                        var res = cmd.ExecuteScalar();
                        if (res!=null)
                        {
                            return true;
                        }
                    }

                }
                string sql = "Insert into events(name,vip_price,r_price,eb_price) values(@name,@vip_price,@r_price,@eb_price)";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@name", eventDto.Name);
                        cmd.Parameters.AddWithValue("@vip_price", eventDto.VIP_Price);
                        cmd.Parameters.AddWithValue("@r_price", eventDto.R_Price);
                        cmd.Parameters.AddWithValue("@eb_price", eventDto.EB_Price);
                        cmd.ExecuteNonQuery();
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return status;
        }

//for Updating event
        public async Task<bool> UpdateEvent(EventDto eventDto,int id)
        {
            bool status = false;
            try
            {
                string sql = "update events set name=@name,vip_price=@vip_price,r_price=@r_price,eb_price=@eb_price where id=@id";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@name", eventDto.Name);
                        cmd.Parameters.AddWithValue("@vip_price", eventDto.VIP_Price);
                        cmd.Parameters.AddWithValue("@r_price", eventDto.R_Price);
                        cmd.Parameters.AddWithValue("@eb_price", eventDto.EB_Price);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return status;
        }

        //for deleting event
        public async Task<bool> DeleteEvent(int id)
        {
            bool status = false;
            try
            {
                string sql = "delete from events where id=@id";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return status;
        }

        //for Getting event
        public async Task<EventDto> GetEvent(int id)
        {
            EventDto eventDto = new EventDto();
            try
            {
                string sql = "select * from events where id=@id";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@id", id);
                        var res=cmd.ExecuteReader();
                        if (res.HasRows)
                        {
                            while (res.Read()) {
                                eventDto.Name = res["name"].ToString();
                                eventDto.VIP_Price = Convert.ToInt32(res["vip_price"]);
                                eventDto.R_Price= Convert.ToInt32(res["r_price"]);
                                eventDto.EB_Price = Convert.ToInt32(res["eb_price"]);
                            }
                        }
                    }
                }
                return eventDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool ReadXlsxFile()
        {
            try
            {
                var filePath = _config.GetSection("XlsxFilePath").Value;                
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(filePath))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var eventDto = new EventDto()
                        {
                            Name = worksheet.Cells[row, 1].Value.ToString(),
                            VIP_Price = Convert.ToInt32(worksheet.Cells[row, 2].Value),
                            R_Price = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                            EB_Price = Convert.ToInt32(worksheet.Cells[row, 4].Value)
                        };
                        AddEvent(eventDto);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }
    }
}

