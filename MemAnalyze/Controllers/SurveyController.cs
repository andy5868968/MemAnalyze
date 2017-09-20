using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using MemAnalyze.Models;
using MY.Libs;
using System.Text;
using System.Collections;
using MemAnalyze.Services;

namespace MemAnalyze.Controllers
{
    public class SurveyController : ApiController
    {
        SurveyService surveyservice = new SurveyService();
        #region 問卷
        [HttpPost]
        public Tuple<string> Questionnaire(Register_Survey rgsur)
        {
            DBController l_dbc = new DBController();
            StringBuilder l_SQL = new StringBuilder();
            Hashtable l_ht = new Hashtable();

            l_SQL.Clear();
            l_SQL.Append("Insert into Register_Survey values(@account,@cdname,@problem,@avg_sleep,@eat_fruit,@eat_veg,@eat_cereal,@eat_meat,@eat_milk)");
            l_ht.Clear();
            l_ht.Add("@account", new StructureSQLParameter(rgsur.Account, SqlDbType.NVarChar));
            l_ht.Add("@cdname", new StructureSQLParameter(rgsur.CdName, SqlDbType.NVarChar));
            l_ht.Add("@problem", new StructureSQLParameter(rgsur.Problem, SqlDbType.NVarChar));
            l_ht.Add("@avg_sleep", new StructureSQLParameter(rgsur.Avg_Sleep, SqlDbType.NVarChar));
            l_ht.Add("@eat_fruit", new StructureSQLParameter(rgsur.Eat_Fruit, SqlDbType.NVarChar));
            l_ht.Add("@eat_veg", new StructureSQLParameter(rgsur.Eat_Veg, SqlDbType.NVarChar));
            l_ht.Add("@eat_cereal", new StructureSQLParameter(rgsur.Eat_Cereal, SqlDbType.NVarChar));
            l_ht.Add("@eat_meat", new StructureSQLParameter(rgsur.Eat_Meat, SqlDbType.NVarChar));
            l_ht.Add("@eat_milk", new StructureSQLParameter(rgsur.Eat_Milk, SqlDbType.NVarChar));

            int c = l_dbc.DbExecuteNonQuery(l_SQL.ToString(), l_ht);

            l_SQL.Clear();
            l_SQL.Append("update Member set isSurvey=@issurvey where Account=@account");
            l_ht.Clear();
            l_ht.Add("@issurvey", new StructureSQLParameter("true", SqlDbType.Bit));
            l_ht.Add("@account", new StructureSQLParameter(rgsur.Account, SqlDbType.NVarChar));
            int count = l_dbc.DbExecuteNonQuery(l_SQL.ToString(), l_ht);
            return Tuple.Create("填寫成功");

        }
        #endregion

        #region 小孩狀態統計
        [HttpPost]
        public DataTable GetSurveyProblem()
        {
            return surveyservice.GetProblem();
        }
        #endregion

        #region 孩童五大類食物飲食分布
        [HttpPost]
        public Dictionary<string, double> GetAvgCereal()
        {
            return surveyservice.GetCereal();
        }
        #endregion

        #region 小孩平均睡眠時間
        [HttpPost]
        public DataTable GetChildAvgAleep()
        {
            return surveyservice.GetAvgSleep();
        }
        #endregion
    }
}
