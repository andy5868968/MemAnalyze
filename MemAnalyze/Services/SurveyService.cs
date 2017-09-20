using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MemAnalyze.Models;
using MY.Libs;
using System.Text;
using System.Collections;
using System.Data;

namespace MemAnalyze.Services
{
    public class SurveyService
    {
        DBController l_dbc = new DBController();
        DataSet ds = new DataSet();
        StringBuilder l_SQL = new StringBuilder();
        Hashtable l_ht = new Hashtable();

        #region 問題
        public DataTable GetProblem()
        {
            l_SQL.Clear();
            l_SQL.Append("select Problem,CAST(count(Problem)as float)/(select count(*) from Register_Survey)as count from Register_Survey group by Problem");
            ds.Clear();
            l_dbc = new DBController();
            l_dbc.FillDataSet(l_SQL.ToString(), ref ds);
            DataTable dt = ds.Tables[0];
            return dt;
        }
        #endregion
        #region 五穀
        public Dictionary<string, double> GetCereal()
        {
            string[] cereal = { "Eat_Fruit", "Eat_Veg", "Eat_Cereal", "Eat_Meat", "Eat_Milk" };
            Dictionary<string, double> dc = new Dictionary<string, double>();
            for (int i = 0; i < cereal.Length; i++)
            {
                l_SQL.Clear();
                l_SQL.Append("select CAST(count(@EatCereal)as float)/(select count(*) from Register_Survey)as @EatCereal");
                l_SQL.Append(" from Register_Survey where @EatCereal=1 group by @EatCereal;");
                l_SQL.Replace("@EatCereal", cereal[i]);
                ds.Clear();
                l_dbc = new DBController();
                l_dbc.FillDataSet(l_SQL.ToString(), ref ds);
                var value = Convert.ToDouble(ds.Tables[0].Rows[0][cereal[i]]); //Get DataSet table內 欄位資料
                dc.Add(cereal[i], value);
            }
            return dc;
        }
        #endregion
        #region 平均睡眠
        public DataTable GetAvgSleep()
        {
            l_SQL.Clear();
            l_SQL.Append("select Table1.Age,Table1.Avg_Sleep,count(*)as Counter from");
            l_SQL.Append(" (select FLOOR(((CAST(Getdate()as int)-CAST(Member.Birthday as int))/365.25))as Age,");
            l_SQL.Append("case Register_Survey.Avg_Sleep when'6小時以下' then '6' when'6-7小時' then '6.5' when'7-8小時' then '7.5' when'8-9小時' then '8.5' when'9-10小時' then '9.5' else '10' end as Avg_Sleep from Member ");
            l_SQL.Append("Right Join Register_Survey on Member.Account=Register_Survey.Account)as Table1 group by Table1.Age,Table1.Avg_Sleep order by Table1.Age");
            
            ds.Clear();
            l_dbc.FillDataSet(l_SQL.ToString(), ref ds);
            return ds.Tables[0];
        }
        #endregion
    }
}