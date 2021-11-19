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
using Microsoft.AspNetCore.Hosting;

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

        public static DataTable GetReportResults(
            string scriptsPath, Report report,
            Dictionary<string, string> parameters, string cok,
            Dictionary<string, string>? parametersReplacements = null
        )
        {
            int currentTry = 0;
            int maxTries = 2;   //було 5 спроб
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
                    string connectionString = Configuration.GetConnectionString(connString);
                    DataTable results = new DataTable();
                    using(SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        if (parametersReplacements != null)
                        {
                            foreach(var paramName in parametersReplacements.Keys)
                            {
                                script = script.Replace(paramName, parametersReplacements[paramName]);
                            }
                        }
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
                    string logPath = scriptsPath + "..\\Logs\\logs.txt";
                    if (!File.Exists(logPath))
                    {
                        using (FileStream c = File.Create(logPath))
                            Console.WriteLine("Created");
                    }
                    using (StreamWriter w = File.AppendText(logPath))
                        Log(e.ToString(), w);

                    if (e.Message.ToLower().Contains("deadlock"))
                    {
                        //System.Threading.Thread.Sleep(50000);
                        throw new Exception("База заблокована вибірка не можлива!" + exceptionDesc);
                    }

                    exceptionDesc = e.Message.ToString();
                    currentTry++;
                }
            }
            throw new Exception("Помилка при доступі до бази, спробуйте пізніше" + exceptionDesc);
        }

        public static DataTable GetSubsData(string scriptPath, string cok, List<ObminSubs> subs)
        {
            List<string> Inserts = new List<String>();
            foreach(ObminSubs s in subs)
            {
                Inserts.Add(
                    string.Format(
                        "INSERT @table VALUES ('{0}', '{1}') ", s.OWN_NUM, s.NUMB
                    )
               );
            }
            string InsertScript = String.Join("\n", Inserts);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            script = script.Replace("$cok$", cok);
            script = script.Replace("$params$", InsertScript);
            //Console.WriteLine(script);
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
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
        
        public static void ExecuteRawSql(string BaseScript, string cok, DataTable? dt=null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";   //то для РЕС
            string script = BaseScript;
            string connectionString = Configuration.GetConnectionString(connString);
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(script, conn))
                {
                    command.CommandTimeout = 600;
                    if(dt != null)
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader != null)
                            {
                                dt.Load(reader);
                            }
                        }
                    }
                    else
                    {
                        command.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        public static DataTable AddAbon(IWebHostEnvironment env, string OsRah, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path = env.WebRootPath + "\\Files\\Scripts\\Forma103.sql";
            string script;
            script = "USE " + cokCode + "_Utility" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            _ = new List<string>();
            script += " WHERE a.AccountNumber = '" + OsRah + "' AND addr.FullAddress IS NOT NULL AND a.DateTo = '2079-06-06'";
            string connectionString = Configuration.GetConnectionString("RESConnection" + cokCode + "_Utility");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        public static DataTable AddJuridic(IWebHostEnvironment env, string OsRah, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path = env.WebRootPath + "\\Files\\Scripts\\Forma103J.sql";
            string script;
            script = "USE " + cokCode + "_Juridical" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            _ = new List<string>();
            script += " AND c.ContractNumber = '" + OsRah + "'";
            string connectionString = Configuration.GetConnectionString("RESConnection" + cokCode + "_Juridical");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        public static DataTable Ukrspecinform(string scriptPath, string cok)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            //script = script.Replace("$cok$", cok);
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable GetAccNumb(string scriptPath, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cokCode + "_Utility";
            string script;
            script = "USE " + cokCode + "_Utility" + "\n";
            script += File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable GetPilgaCity (string scriptPath, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cokCode + "_Utility";
            string script;
            script = "USE " + cokCode + "_Utility" + "\n";
            script += File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable GetPilga2 (string scriptPath, string cokCode, int period, List<int> vs)
        {
            List<string> nsp = new List<string>();
            foreach (var s in vs)
            {
                nsp.Add(string.Format("'{0}'", s.ToString()));
            }
            string InsertScript = "WHERE KodNasPunktu IN (" + string.Join(", ", nsp) + ")";
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cokCode + "_Utility";
            string script;
            script = "USE " + cokCode + "_Utility" + "\n";
            script += File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString(connString);
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                script = script.Replace("$params$", InsertScript);
                //Console.WriteLine(script);
                using (SqlCommand command = new SqlCommand(script, conn))
                {
                    command.Parameters.AddWithValue("period", period);
                    command.CommandTimeout = 600;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable Napovnenia(string scriptPath, string cok, List<Zvirka> subs)
        {
            List<string> Inserts = new List<String>();
            foreach (Zvirka s in subs)
            {
                Inserts.Add(
                    string.Format(
                        "INSERT @table VALUES ('{0}') ", s.OWN_NUM
                    )
               );
            }
            string InsertScript = String.Join("\n", Inserts);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            script = script.Replace("$cok$", cok);
            script = script.Replace("$params$", InsertScript);
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable PilgNewAcc(string scriptPath, string cok, List<ZvirkaOsPilg> pilg)
        {
            List<string> Inserts = new List<String>();
            foreach (ZvirkaOsPilg s in pilg)
            {
                Inserts.Add(
                    string.Format(
                        "INSERT @table VALUES ('{0}') ", s.RAH
                    )
               );
            }
            string InsertScript = String.Join("\n", Inserts);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            script = script.Replace("$cok$", cok);
            script = script.Replace("$params$", InsertScript);
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable GetMoneySubsData(string scriptPath, string cok)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cok + "_Utility";
            string script = File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
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
                        if (reader != null)
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable GetDovidkaSubs(string scriptPath, string cokCode, List<string> OsRahList)
        {
            string InsertScript = "AccountNumber IN (" + OsRahList + ")";
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string connString = "RESConnection" + cokCode + "_Utility";
            string script;
            script = "USE " + cokCode + "_Utility" + "\n";
            script += File.ReadAllText(scriptPath, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString(connString);
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                script = script.Replace("$params$", InsertScript);
                //Console.WriteLine(script);
                using (SqlCommand command = new SqlCommand(script, conn))
                {
                    command.CommandTimeout = 600;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader != null)
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
