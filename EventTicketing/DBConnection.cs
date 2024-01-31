using System;
namespace EventTicketing
{
    public interface IDBConnection
    {
        public string GetConnectionString();
    }
    public class DBConnection : IDBConnection
    {
        public IConfiguration _config { get; set; }
        public DBConnection(IConfiguration config)
        {
            _config = config;
        }

        public string GetConnectionString()
        {
            return _config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
        }
    }
}

