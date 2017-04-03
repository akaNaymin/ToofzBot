﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Statsbot
{
    public class Racebot
    {

        private MySqlConnection Connection = new MySqlConnection(Program.database.ToString());

        public string GetResults(string user, int offset)
        {
            StringBuilder sb = new StringBuilder();
            using (Connection)
            {
                try
                {
                    Connection.Open();
                    MySqlCommand comm = new MySqlCommand("SELECT timestamp, character_name, descriptor, seed, time, rank, comment, level"
                        + " FROM racer_data, race_data, user_data WHERE user_data.name='" + user
                        + "' AND racer_data.discord_id=user_data.discord_id AND race_data.race_id=racer_data.race_id ORDER BY race_data.race_id DESC", Connection);
                    using (MySqlDataReader r = comm.ExecuteReader())
                    {
                        for (int i = 0; i < (offset + 10) && r.Read(); i++) //while havn't reached end of reader
                        {
                            if (i >= offset) //starts reading at specified offset
                            {
                                sb.Append(r[0] + "  ");
                                sb.Append(r[1] + "  ");
                                sb.Append(r[2] + "   ");
                                //for (int j = r[3].ToString().Length; j <= 8; j++) { sb.Append(" "); } // displays seed
                                //sb.Append(r[3]);
                                sb.Append(TimeToString(r[4].ToString()) + "  ");
                                sb.Append(CommandHandler.RankToString(Convert.ToInt16(r[5])));
                                if (r[7].ToString() != "-2")
                                    sb.Append(" (f) ");
                                else
                                    sb.Append("     ");
                                sb.Append(r[6] + "\n");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return ("");
                }
            }
            return sb.ToString();
        }


        public string DisplayResults(string user, string q)
        {

            int offset = 0;
            if (q.Contains("offset"))
            {
                q = q.Replace("offset", "&");
                q = q.Replace("=", null);
            }

            if (q.Contains("&"))
            {
                bool pos = int.TryParse(q.Split('&')[1], out offset);
                if (!pos)
                    return ("Please enter a valid offset.");
                q = q.Split('&')[0];
            }

            q = q.Trim();
            if (q != "")
                user = q;

            string results = GetResults(user, offset);
            if (results == "")
                return ("No necrobot results found for \"" + user + "\".");

            StringBuilder sb = new StringBuilder();
            sb.Append("Displaying requested necrobot results for " + user + " \n\n");
            sb.Append(results);
            return (sb.ToString());
        }

        public static string TimeToString(string time)
        {
            TimeSpan t = TimeSpan.FromSeconds(int.Parse(time) / 100);
            return (t.ToString(@"mm\:ss"));
        }

    }
}
