using System;
using System.Data;
using EventTicketing.Dto;
using Microsoft.Data.SqlClient;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace EventTicketing.Services
{
    public interface IUserServices
    {
        Task<bool> Register(UserDto userDto);
        Task<bool> Login(UserDto userDto);
        Task<bool> EventFeedback(int user_id,string event_name,string feedback);
        Task<bool> EventRegistration(int user_id, string event_name, string event_type);
        Task<bool> EventCancellation(int user_id, string event_name, string event_type);
        Task<EventDto> SearchEvent(string name);

    }
    public class UserServices:IUserServices
	{
        public IDBConnection _db { get; set; }
        public IConfiguration _config { get; set; }

        public UserServices(IDBConnection db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        //for registratin of user
        public async Task<bool> Register(UserDto userDto)
        {
            try
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.password);
                string sql = "Insert into users(email, password) values (@email ,@password)";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@email", userDto.email);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            return true;
        }

        public async Task<bool> Login(UserDto userDto)
        {
            try
            {
                if (userDto.email == null || userDto.password == null)
                {
                    return false;
                }

                string sql = "SELECT password FROM users WHERE email = @email";
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

        //for searching event with name
        public async Task<EventDto> SearchEvent(string name)
        {
            EventDto eventDto = new EventDto();
            try
            {
                string sql = "select * from events where name=@name";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@name", name);
                        var res = cmd.ExecuteReader();
                        if (res.HasRows)
                        {
                            while (res.Read())
                            {
                                eventDto.Name = res["name"].ToString();
                                eventDto.VIP_Price = Convert.ToInt32(res["vip_price"]);
                                eventDto.R_Price = Convert.ToInt32(res["r_price"]);
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

        //for giving feedback and updating if already exists
        public async Task<bool> EventFeedback(int user_id, string event_name, string feedback)
        {
            try
            {
                bool exist = false;
                string sql = "select * from feedback where user_id=@user_id and event_name=@event_name";
                using (var con = new SqlConnection(_db.GetConnectionString()))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@user_id", user_id);
                        cmd.Parameters.AddWithValue("@event_name", event_name);
                        var res = cmd.ExecuteReader();
                        if (res.HasRows)
                        {
                            exist = true;                            
                        }
                    }
                }
                if (exist)
                {
                    sql = " update feedback set feedback=@feedback where user_id=@user_id and event_name=@event_name";
                    using (var con = new SqlConnection(_db.GetConnectionString()))
                    {
                        con.Open();
                        using (var cmd = new SqlCommand(sql, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@user_id", user_id);
                            cmd.Parameters.AddWithValue("@event_name", event_name);
                            cmd.Parameters.AddWithValue("@feedback", feedback);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    sql = "Insert into feedback (user_id,event_name,feedback) values(@user_id,@event_name,@feedback) ";
                    using (var con = new SqlConnection(_db.GetConnectionString()))
                    {
                        con.Open();
                        using (var cmd = new SqlCommand(sql, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@user_id", user_id);
                            cmd.Parameters.AddWithValue("@event_name", event_name);
                            cmd.Parameters.AddWithValue("@feedback", feedback);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //for registratin of event
        public async Task<bool> EventRegistration(int user_id, string event_name, string event_type)
        {
            try
            {
                string sql = "Insert into eventRegistration (user_id,event_name,event_type) values (@user_id,@event_name,@event_type)";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@user_id", user_id);
                        cmd.Parameters.AddWithValue("@event_name", event_name);
                        cmd.Parameters.AddWithValue("@event_type", event_type);
                        cmd.ExecuteNonQuery();
                    }
                }
                string sql1 = "";
                if (event_type == "vip")
                    sql1 = "update events set vip = vip + 1 where name = @event_name";
                if (event_type == "regular")
                    sql1 = "update events set regular = regular+ 1 where name = @event_name";
                if (event_type == "early_bird")
                    sql1 = "update events set early_bird= early_bird+ 1 where name = @event_name";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@event_name", event_name);
                        cmd.Parameters.AddWithValue("@event_type", event_type);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        //for registratin Cancelling
        public async Task<bool> EventCancellation(int user_id, string event_name, string event_type)
        {
            try
            {
                string sql = "delete from eventRegistration where user_id=@user_id and event_name=@event_name and event_type=@event_type";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@user_id", user_id);
                        cmd.Parameters.AddWithValue("@event_name", event_name);
                        cmd.Parameters.AddWithValue("@event_type", event_type);
                        cmd.ExecuteNonQuery();
                    }
                }
                string sql1 = "";
                if (event_type == "vip")
                    sql1 = "update events set vip = vip - 1 where name = @event_name";
                if (event_type == "regular")        
                    sql1 = "update events set regular = regular - 1 where name = @event_name";
                if (event_type == "early_bird")
                    sql1 = "update events set early_bird= early_bird - 1 where name = @event_name";
                using (var conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@event_name", event_name);
                        cmd.Parameters.AddWithValue("@event_type", event_type);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

    }
}

