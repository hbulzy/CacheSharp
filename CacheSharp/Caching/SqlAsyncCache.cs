using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CacheSharp.Caching
{
    public sealed class SqlAsyncCache : AsyncCache<string>
    {
        private DbConnection conn;

        public override async Task InitializeAsync(Dictionary<string, string> parameters)
        {
            string connString = parameters["ConnectionString"];
            conn = new SqlConnection(connString);
            await conn.OpenAsync();
        }

        public override void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }

        public override List<string> InitializationProperties
        {
            get { return new List<string>{"ConnectionString", "CharactersPerMessage"}; }
        }

        protected internal override async Task Put(string key, string value, TimeSpan lifeSpan)
        {
            try
            {
                DbCommand command = conn.CreateCommand();
                command.CommandText = "PutAppState";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@KeyID", key));
                command.Parameters.Add(new SqlParameter("@value", value));
                command.Parameters.Add(new SqlParameter("@TimeoutUTC", DateTime.UtcNow.AddMinutes(5)));
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 41301)
                {
                    DbCommand command = conn.CreateCommand();
                    command.CommandText = "PutAppState";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@KeyID", key));
                    command.Parameters.Add(new SqlParameter("@value", value));
                    command.Parameters.Add(new SqlParameter("@TimeoutUTC", DateTime.UtcNow.AddMinutes(5)));
                    command.ExecuteNonQuery();
                }
            }

        }

        protected internal override async Task<string> Get(string key)
        {
            try
            {
                DbCommand getCommand = conn.CreateCommand();
                getCommand.CommandText = "GetAppState";
                getCommand.Parameters.Add(new SqlParameter("@KeyID", key));
                var outValue = new SqlParameter
                {
                    ParameterName = "@Value",
                    Direction = ParameterDirection.Output,
                    Size = 10000
                };
                getCommand.Parameters.Add(outValue);
                getCommand.CommandType = CommandType.StoredProcedure;
                await getCommand.ExecuteNonQueryAsync();
                string value = outValue.SqlValue.ToString();
                return value;   
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 41301)
                {
                    DbCommand getCommand = conn.CreateCommand();
                    getCommand.CommandText = "GetAppState";
                    getCommand.Parameters.Add(new SqlParameter("@KeyID", key));
                    var outValue = new SqlParameter
                    {
                        ParameterName = "@Value",
                        Direction = ParameterDirection.Output,
                        Size = 10000
                    };
                    getCommand.Parameters.Add(outValue);
                    getCommand.CommandType = CommandType.StoredProcedure;
                    getCommand.ExecuteNonQuery();
                    string value = outValue.SqlValue.ToString();
                    return value;   
                }
                throw;
            }
        }

        protected internal override async Task Remove(string key)
        {
            try
            {
                DbCommand command = conn.CreateCommand();
                command.CommandText = "DeleteAppState";
                command.Parameters.Add(new SqlParameter("@KeyID", key));
                command.CommandType = CommandType.StoredProcedure;
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 41301)
                {
                    DbCommand command = conn.CreateCommand();
                    command.CommandText = "DeleteAppState";
                    command.Parameters.Add(new SqlParameter("@KeyID", key));
                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();
                }
            }

        }
    }
}