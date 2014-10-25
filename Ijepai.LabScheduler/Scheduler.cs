using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Ijepai.LabScheduler
{
    public class Scheduler
    {
        public void Init()
        {
            Timer checkLabUptime = new Timer();
            checkLabUptime.Elapsed += new ElapsedEventHandler(checkLabUptime_Elapsed);
            checkLabUptime.Interval = 60000;
            checkLabUptime.Enabled = true;
        }

        void checkLabUptime_Elapsed(object sender, ElapsedEventArgs e)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\aspnet-Ijepai.Web-20140505095652.mdf;Initial Catalog=aspnet-Ijepai.Web-20140505095652;Integrated Security=True";
            conn.Open();
            SqlCommand labs = new SqlCommand("Select * from Labs where ((datediff(minute, start_time, getdate()) <= 30) and (status = 'Scheduled'))");
            SqlDataReader labsReader = labs.ExecuteReader();
            while (labsReader.Read())
            {
                string labName = labsReader.GetString(1);
                int labID = labsReader.GetInt32(0);
                SqlCommand participantList = new SqlCommand("Select * from LabParticipants where LabID = " + labID);
                SqlDataReader participantReader = participantList.ExecuteReader();
                while (participantReader.Read())
                {
                    string email = participantReader.GetString(1);
                }
                SqlCommand updateLabsStatus = new SqlCommand("update labs set status='Provisioning' where id = " + labID);
                updateLabsStatus.ExecuteNonQuery();
            }
            conn.Close();
        }
    }
}