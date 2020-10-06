using Assistant_TEP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Assistant_TEP.MyClasses
{
    public static class BillingUtils
    {
        public static IConfiguration Configuration;

        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        public static DataTable GetReportResults(string scriptsPath, Report report, Dictionary<string, string> parameters, string cok)
        {
            int currentTry = 0;
            int maxTries = 5;
            string exceptionDesc = "";
            while(currentTry < maxTries)
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var path = scriptsPath + report.FileScript;
                    string connString = "RESConnection" + cok + "_" + report.DbType.Type;
                    string script = File.ReadAllText(path, Encoding.GetEncoding(1251));
                    script = script.Replace("$cok$", cok);
                    //Console.WriteLine(connString);
                    string connectionString = Configuration.GetConnectionString(connString);
                    DataTable results = new DataTable();
                    using(SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand command = new SqlCommand(script, conn))
                        {
                            foreach(var key in parameters.Keys)
                            {
                                command.Parameters.AddWithValue(key, parameters[key]);
                            }
                            command.CommandTimeout = 600;
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if(reader != null)
                                {
                                    results.Load(reader);
                                }
                            }
                        }
                    }
                    return results;
                }
                catch (Exception e)
                {
                    //string logPath = scriptsPath + "..\\Logs\\logs.txt";
                    //if (!File.Exists(logPath))
                    //{
                    //    using (FileStream c = File.Create(logPath))
                    //        Console.WriteLine("Created");
                    //}
                    //using (StreamWriter w = File.AppendText(logPath))
                    //    Log(e.ToString(), w);
                    exceptionDesc = e.Message.ToString();
                    currentTry++;
                }
            }
            throw new Exception("Помилка при доступі до бази, спробуйте пізніше" + exceptionDesc);
        }
        
        public static DataTable GetResults(string scriptPath, string cok)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            script = script.Replace("$cok$", cok);
            string connectionString = Configuration.GetConnectionString(connString);
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(script, conn))
                {
                    command.CommandTimeout = 600;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }
        
        public static DataTable ExecuteRawSql(string BaseScript, string cok)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = BaseScript;
            Console.WriteLine(script);
            string connectionString = Configuration.GetConnectionString(connString);

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(script, conn))
                {
                    command.CommandTimeout = 600;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }
    }
}
