using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models.Profile
{
    public class ProfileDbo : IProfile
    {
        public string GetPassword(string userAccount)
        {
            string passwordfordb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordfordb = connection.Query<string>("spGetUserPassword", new { Id = userAccount },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordfordb;
        }
    }
}