using MemAnalyze.Models;
using MemAnalyze.ViewModels;
using MemAnalyze.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Security;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.IO;

namespace MemAnalyze.Controllers
{
    public class MemberController : ApiController
    {
        //Viewmodel
        MemberView memberview = new MemberView();

        //DataBase
        MemAnalyzeEntities db = new MemAnalyzeEntities();

        //Service
        MemberService memberservice = new MemberService();
        MailService mailservice = new MailService();

        string username = "";
        #region 登入
        //[HttpPost]
        //public MemberView Login(Member Logindata)
        //{
        //    string sessionId = "";
        //    CookieHeaderValue cookie = Request.Headers.GetCookies().FirstOrDefault();//抓到所有的cookie
        //    sessionId = cookie != null ? cookie[".ASPXAUTH"].Value : "";

        //    if (sessionId != "")
        //    {
        //        FormsAuthenticationTicket authticket = FormsAuthentication.Decrypt(sessionId);//解析出cookie的username
        //        username = authticket.Name;

        //        bool CheckUserSameorNot = username.Equals(Logindata.Account) ? false : true;//判斷愈登錄帳號與cookie上肢帳號一不一樣 如果不一樣救回傳true

        //        if (CheckUserSameorNot)
        //        {
        //            FormsAuthentication.SignOut();//以防萬一，將form驗證先登出
        //            username = Logindata.Account;
        //            bool thirdlogintype = Logindata.Facebook_id != null ? true : false;//判斷第三方登入 FB 
        //            memberview = memberservice.Loginfun(Logindata, thirdlogintype);
        //            FormsAuthentication.SetAuthCookie(Logindata.Account, false);
        //        }

        //        memberview.Account = username;
        //        memberview.result = (memberservice.Finduser(username) != null) ? ("登入成功") : ("登入失敗");
        //    }
        //    else
        //    {
        //        bool thirdlogintype = Logindata.Facebook_id != null ? true : false;//判斷第三方登入 FB 
        //        memberview = memberservice.Loginfun(Logindata, thirdlogintype);
        //        FormsAuthentication.SetAuthCookie(Logindata.Account, false);
        //        //var ss = StatusCode(HttpStatusCode.NoContent);
        //    }
        //    return memberview;
        //}
        #endregion

        #region 登入
        [HttpPost]
        public MemberView Login(Member Logindata)
        {
            bool thirdlogintype = Logindata.Facebook_id != null ? true : false;//判斷第三方登入 FB 
            memberview = memberservice.Loginfun(Logindata, thirdlogintype);
            return memberview;
        }
        #endregion

        #region 註冊
        [HttpPost]
        public MemberView Register(Member Registerdata)
        {
            memberview = memberservice.Registerfun(Registerdata);
            return memberview;
        }
        #endregion
        #region Cd註冊
        [HttpPost]
        public MemberView CdRegister(CdMember CdRegisterdata)
        {
            memberview = memberservice.CdRegisterfun(CdRegisterdata);
            return memberview;
        }
        #endregion

        #region 主頁資料
        [HttpPost]
        public DataTable PersonalPage(Member data)
        {
            return memberservice.ShowPersonalPage(data.Account);
        }
        #endregion
        #region 小孩主頁資料
        [HttpPost]
        public DataTable CdPersonalPage(CdMember data)
        {
            return memberservice.ShowCdPersonalPage(data.Account, data.CdName);
        }
        #endregion
        #region 修改帳號資料
        [HttpPost]
        public string EdlitPersonalPage(Member data)
        {
            using (SqlConnection conn = new SqlConnection("Server=163.17.135.185;DataBase=MemAnalyze;User Id=sa;Password=ftpms.2015"))
            {

                SqlCommand cmd = new SqlCommand();//設定語法與內容
                cmd.CommandText = "update Member Set Name=@name,Address=@add where Account=@acc";
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@name", data.Name);//一一將參數帶入
                cmd.Parameters.AddWithValue("@add", data.Address);
                cmd.Parameters.AddWithValue("@acc", data.Account);
                conn.Open();//連接開啟
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

                conn.Close();
            }
            return "success";
        }
        #endregion
        #region 修改小孩帳號資料
        [HttpPost]
        public string EdlitCdPersonalPage(CdMember data)
        {
            using (SqlConnection conn = new SqlConnection("Server=163.17.135.185;DataBase=MemAnalyze;User Id=sa;Password=ftpms.2015"))
            {

                SqlCommand cmd = new SqlCommand();//設定語法與內容
                cmd.CommandText = "update CdMember Set Birthday=@birthday,Gender=@gender where Account=@acc and CdName=@cdname";
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@birthday", data.Birthday);//一一將參數帶入
                cmd.Parameters.AddWithValue("@gender", data.Gender);

                cmd.Parameters.AddWithValue("@acc", data.Account);
                cmd.Parameters.AddWithValue("@cdname", data.CdName);
                conn.Open();//連接開啟
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

                conn.Close();
            }
            return "success";
        }
        #endregion

        #region 會員小孩名稱
        [HttpPost]
        public Dictionary<string, string[]> GetAccountCdName(Member data)
        {
            return memberservice.FindCdNameByAccount(data);
        }
        #endregion

        #region 修改密碼
        [HttpPost]
        public MemberView EditPassword(MemberView data)
        {
            Member userdata = memberservice.Finduser(data.Account);
            if (memberservice.HashPassword(data.OdPassword).Equals(userdata.Password))
            {
                userdata.Password = memberservice.HashPassword(data.Password);
                db.Entry(userdata).State = EntityState.Modified;
                db.SaveChanges();
                memberview.result = "修改成功";
            }
            else
            {
                memberview.result = "密碼錯誤";
            }
            memberview.Account = data.Account;
            return memberview;
        }
        #endregion
        #region 忘記密碼
        [HttpPost]
        public string ForgotPassword(Member data)
        {
            Member userdata = memberservice.Finduser(data.Account);
            string result = "";
            if (userdata != null)//根據account find==true ?
            {
                string resultPassword = memberservice.SetForgotPassword();//亂數產生Password
                userdata.Password = memberservice.HashPassword(resultPassword);//將Password 存入userdata
                db.Entry(userdata).State = EntityState.Modified;
                db.SaveChanges();

                string TempMail = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Views/Shared/ForgotPasswordEmailTemplate.html"));

                var uriBuilder = new UriBuilder("http://163.17.135.185/7thWebApi/api/Member/SuccessEmailValidate");
                var parameters = HttpUtility.ParseQueryString(string.Empty);
                parameters["Account"] = userdata.Account;
                parameters["Password"] = userdata.Password;
                uriBuilder.Query = parameters.ToString();

                string Mailbody = mailservice.GetMailBody(TempMail, userdata.Account, uriBuilder.ToString().Replace("%3F", "?"));
                mailservice.SendRegisterMail(Mailbody, userdata.Account);
                result = "已發送驗證信，請去查收";
            }
            return result;
        }
        #endregion

        #region Web上傳圖片
        [HttpPost]
        public async Task<MemberView> PostUserImage(string Account, string Cdname)
        {
            //Dictionary<string, object> dict = new Dictionary<string, object>();
            var extension = string.Empty;//取副檔名
            var message = string.Empty;
            try
            {
                HttpPostedFile postedFile = HttpContext.Current.Request.Files[0];
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB

                    IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        message = string.Format("Please Upload image of type .jpg,.gif,.png.");
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {
                        message = string.Format("Please Upload a file upto 1 mb.");
                    }
                    else
                    {
                        string count = memberservice.CountImageWithAccount(Account);//當前Account小孩的圖片數量 0 start
                        string filename = Account + count + "-Member" + extension;
                        memberservice.UpImgName(Account, Cdname, filename);//儲存圖片名稱in SQL
                        string url = Path.Combine(HttpContext.Current.Server.MapPath("~/Filefolder/"), filename);
                        //Userimage myfolder name where i want to save my image
                        postedFile.SaveAs(url);
                        message = string.Format("Image Updated Successfully.");
                    }
                }
                message = string.Format("Please Upload a image.");
            }
            catch (Exception ex)
            {
                message = string.Format(ex.Message);
            }
            return new MemberView { Account = Account, CdName = Cdname, result = message };
        }
        #endregion
        #region react上傳圖片
        [HttpPost]
        public MemberView PostFile(PutImage Put)
        {
            string imgname = "";
            try
            {
                imgname = memberservice.Upload(Put.Account, Put.CdName, Put.FileStr);
            }
            catch (Exception ex)
            {
                return new MemberView { result = ex.ToString() };
            }
            return new MemberView { result = imgname.ToString() };
        }
        #endregion

        #region 忘記密碼成功頁面
        [HttpGet]
        public string SuccessEmailValidate(string Account, string Password)
        {
            string result = "";
            Member userdata = memberservice.Finduser(Account);
            if (memberservice.Finduser(Account) != null)
            {
                result = userdata.Password == Password ? "驗證成功" : "驗證失敗";
            }
            else
            {
                result = "驗證失敗";
            }
            return result;
        }
        #endregion

        #region 登出
        [HttpPost]
        public string Logout()
        {
            //var currentcookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            //if (currentcookie != null)
            //{
            //    HttpContext.Current.Response.Cookies.Add(new HttpCookie("session-id", ""));
            //}
            //登出表單驗證
            FormsAuthentication.SignOut();
            return "成功登出";
        }
        #endregion
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var resp = new HttpResponseMessage();

            var cookie = new CookieHeaderValue("session-id", "andy5868968@yahoo.com.tw");
            cookie.Expires = DateTimeOffset.Now.AddDays(1);
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";
            resp.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            return resp;
        }
    }
    public class PutImage
    {
        public string Account { get; set; }
        public string CdName { get; set; }
        public string FileStr { get; set; }
    }
}
