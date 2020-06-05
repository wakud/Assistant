﻿using Assistant_TEP.Models;
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

        public static DataTable GetReportResults(string scriptsPath, Report report, Dictionary<string, string> parameters, string cok)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var path = scriptsPath + report.FileScript;
            string script = "USE " + cok + "_" + report.DbType.Type + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString("RESConnection");
            DataTable results = new DataTable();
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(script, conn);
                conn.Open();
                foreach(var key in parameters.Keys)
                {
                    command.Parameters.AddWithValue(key, parameters[key]);
                }
                command.CommandTimeout = 600;
                SqlDataReader reader = command.ExecuteReader();
                results.Load(reader);
                reader.Close();
                return results;
            }
        }

    }
}
