using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MemAnalyze.ViewModels;
using MemAnalyze.Models;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity;
using System.Net.Mail;
using MY.Libs;
using System.Data;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Web.Hosting;

namespace MemAnalyze.Services
{
    public class MemberService
    {
        MemAnalyzeEntities db = new MemAnalyzeEntities();
        MemberView memberview = new MemberView();
        #region 登入
        public MemberView Loginfun(Member Logindata, bool datatype)//bool 如果是第三方登入 就會是true
        {
            string validatestr = "";
            Member userdata = Finduser(Logindata.Account);
            if (userdata != null)//判斷帳號
            {
                if (userdata.Password == HashPassword(Logindata.Password) || datatype == true)//檢查帳號密碼 OR FB登入 
                {
                    validatestr = "登入成功";//判斷問卷是否填寫
                }
                else
                {
                    validatestr = "密碼錯誤";
                }
            }
            else
            {
                validatestr = "帳號錯誤";
            }
            memberview.Account = Logindata.Account;
            memberview.result = validatestr;
            return memberview;
        }
        #endregion
        #region 使用者資料
        public Member Finduser(string Account)
        {
            return db.Member.Find(Account);
        }
        #endregion
        #region 顯示主頁資料
        public DataTable ShowPersonalPage(string Account)//以ViewModel 解決 回傳datetime格式時錯誤，最後以string.format解決  
        {
            DBController l_dbc = new DBController();
            DataSet ds = new DataSet();
            StringBuilder l_SQL = new StringBuilder();
            Hashtable l_ht = new Hashtable();

            l_SQL.Clear();
            l_SQL.Append("select Account,Name,Address from Member where Account=@account");
            l_ht.Clear();
            l_ht.Add("@account", new StructureSQLParameter(Account, SqlDbType.NVarChar));
            ds.Clear();
            l_dbc.FillDataSet(l_SQL.ToString(), "", ref ds, l_ht);
            return ds.Tables[0];
            /*memberview.Birthday = $"{memberview.Memberdata.Birthday:yyyy-MM-dd}";*/
            //6 以後 新方法 /*$"{datetime.toString():yyyyMMdd}"*/
        }
        #endregion
        #region 顯示小孩主頁資料
        public DataTable ShowCdPersonalPage(string Account, string CdName)//以ViewModel 解決 回傳datetime格式時錯誤，最後以string.format解決  
        {
            DBController l_dbc = new DBController();
            DataSet ds = new DataSet();
            StringBuilder l_SQL = new StringBuilder();
            Hashtable l_ht = new Hashtable();

            l_SQL.Clear();
            l_SQL.Append("select Account,CdName,Gender,Imageurl,CAST(DATEPART(YYYY,Birthday)as nvarchar)+'-'");
            l_SQL.Append("+RIGHT(100+CAST(DATEPART(MM,Birthday) as nvarchar),2)+'-'");
            l_SQL.Append("+RIGHT(100+CAST(DATEPART(DD,Birthday) as nvarchar),2)as Birthday from CdMember where Account=@account ");
            l_SQL.Append("and CdName=@cdname");
            l_ht.Clear();
            l_ht.Add("@account", new StructureSQLParameter(Account, SqlDbType.NVarChar));
            l_ht.Add("@cdname", new StructureSQLParameter(CdName, SqlDbType.NVarChar));
            ds.Clear();
            l_dbc.FillDataSet(l_SQL.ToString(), "", ref ds, l_ht);
            return ds.Tables[0];
            /*memberview.Birthday = $"{memberview.Memberdata.Birthday:yyyy-MM-dd}";*/
            //6 以後 新方法 /*$"{datetime.toString():yyyyMMdd}"*/
        }
        #endregion
        #region 會員小孩名稱
        public Dictionary<string,string[]> FindCdNameByAccount(Member data)
        {
            DBController l_dbc = new DBController();
            DataSet ds = new DataSet();
            StringBuilder l_SQL = new StringBuilder();
            Hashtable l_ht = new Hashtable();
            Dictionary<string, string[]> dc = new Dictionary<string, string[]>();
            string[] objarr;

            l_SQL.Clear();
            l_SQL.Append("select CdName from CdMember where Account=@acc");
            l_ht.Clear();
            l_ht.Add("acc", new StructureSQLParameter(data.Account, SqlDbType.NVarChar));
            ds.Clear();
            l_dbc.FillDataSet(l_SQL.ToString(), "", ref ds, l_ht);
            objarr = new string[ds.Tables[0].Rows.Count];

            for(var i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                var cdname=ds.Tables[0].Rows[i]["CdName"].ToString();
                objarr[i] = cdname;
            }
            dc.Add("CdName",objarr);
            return dc;
        }
        #endregion
        #region 註冊
        public MemberView Registerfun(Member Registerdata)
        {
            string validate = "註冊失敗";
            if (Registerdata.Account != null)
            {
                if (Finduser(Registerdata.Account) == null)//無重複帳號 註冊成功
                {
                    Registerdata.Password = Registerdata.Password != null ? HashPassword(Registerdata.Password) : "";
                    db.Member.Add(Registerdata);
                    db.SaveChanges();
                    validate = "註冊成功，請去登入";
                }
                else
                {
                    if (Finduser(Registerdata.Account).Password == "")//第三方登入 註冊
                    {
                        Member olddata = Finduser(Registerdata.Account);
                        olddata.Password = HashPassword(Registerdata.Password);//新增密碼進去
                        db.SaveChanges();
                        validate = "註冊成功，請去登入";
                    }
                    else
                    {
                        validate = "帳號重複";
                    }
                }
            }
            memberview.Account = Registerdata.Account;
            memberview.result = validate;
            return memberview;
        }
        #endregion
        #region Cd註冊
        public MemberView CdRegisterfun(CdMember CdRegisterdata)
        {
            string validate = "小孩姓名重複";
            var CountCdData = db.CdMember.Where(x => x.Account == CdRegisterdata.Account);
            if (CountCdData.Count() < 5)
            {
                if (CountCdData.Where(x => x.CdName == CdRegisterdata.CdName).Count() == 0)//找尋同帳號內是否有重複的CdName
                {
                    CdRegisterdata.Imageurl = "";//預設給IMG URL 空字串
                    db.CdMember.Add(CdRegisterdata);
                    db.SaveChanges();
                    validate = "新增成功";
                }
            }
            else
            {
                validate = "已達新增上限";
            }

            memberview.Account = CdRegisterdata.Account;
            memberview.CdName = CdRegisterdata.CdName;
            memberview.result = validate;
            return memberview;
        }
        #endregion
        #region react上傳圖片
        public string Upload(string account, string cdname, string filestr)
        {
            var count = CountImageWithAccount(account);
            string imgname = account + count + "-Member.png";
            //依你的需求，再把 圖片傳進來的字串轉為byte[]
            var imgdata = Convert.FromBase64String(filestr);
            UpImgName(account, cdname, imgname);
            //將byte[]圖片資料放入記憶流內
            using (MemoryStream ms = new MemoryStream(imgdata, 0, imgdata.Length))
            {
                Image newImage = Image.FromStream(ms);
                string ImgUrl = Path.Combine(HostingEnvironment.MapPath("~/Filefolder/"), imgname);
                newImage.Save(ImgUrl, System.Drawing.Imaging.ImageFormat.Png);
                return imgname;
            }
        }
        #endregion
        #region 圖片數量
        public string CountImageWithAccount(string account)//當前Account小孩的圖片數量 0 start
        {
            return db.CdMember.Where(x => x.Account == account && x.Imageurl != " ").Count().ToString();
        }
        #endregion
        #region 忘記密碼
        public string SetForgotPassword()//setPassword
        {
            string[] CODE = { "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"
                    ,"U","W","X","Y","Z","1","2","3","4","5","6","7","8","9","0","a","b","c","d","e","f","g"
                    ,"h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
            string ValidateStr = string.Empty;
            Random rd = new Random();
            for (int i = 0; i < 10; i++)
            {
                ValidateStr = CODE[rd.Next(CODE.Count())];
            }
            return ValidateStr;
        }
        #endregion
        #region 密碼hash
        public string HashPassword(string Password)
        {
            //宣告Hash時所添加的無意義亂數值
            string salykey = "1234567890";
            //將剛剛宣告的字串與密碼結合
            string saltAndPassword = String.Concat(Password, salykey);
            //定義一個SHA1的Hash物件
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] PasswordData = Encoding.Default.GetBytes(saltAndPassword);
            byte[] Hashdata = sha1Hasher.ComputeHash(PasswordData);
            string Hashresult = Convert.ToBase64String(Hashdata);
            return Hashresult;
        }
        #endregion
        #region 圖片名稱儲存進SQL
        public void UpImgName(string Account, string CdName, string Imgurl)
        {
            DBController l_dbc = new DBController();
            DataSet ds = new DataSet();
            StringBuilder l_SQL = new StringBuilder();
            Hashtable l_ht = new Hashtable();
            l_SQL.Clear();
            l_SQL.Append("Update CdMember set Imageurl=@imgurl where Account=@account and CdName=@cdname");
            l_ht.Add("@imgurl", new StructureSQLParameter(Imgurl, SqlDbType.NVarChar));
            l_ht.Add("@account", new StructureSQLParameter(Account, SqlDbType.NVarChar));
            l_ht.Add("@cdname", new StructureSQLParameter(CdName, SqlDbType.NVarChar));
            int c = l_dbc.DbExecuteNonQuery(l_SQL.ToString(), l_ht);
        }
        #endregion
    }
}