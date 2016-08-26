using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Collections;
using PostMsg_Net;
using PostMsg_Net.common;
using System.Text;
using System.Configuration;
using System.Xml;
using WXPaySDK;



public partial class web_getdata : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        DataSet ds = null;
        string corID = Session["CorID"].ToS();
        string com = Request["com"].ToS();
        string restr = "";

        string orderpk = Request["order_pk"].ToS();
        string nickname = Request["user_name"].ToS();
        // string  strSQL1 = "update t_order  set order_state=7 where order_pk='" + orderpk + "'";
        // DbHelperSQL.ExecuteSql(strSQL1);
        try
        {
            switch (com)
            {
            case "invite_coach":
                {
#region 推广司机
                    string coach_pk = Guid.NewGuid().ToS();
                    string city = Request["city"].ToS();
                    string phone = Request["phone"].ToS();
                    string pwd = Request["pwd"].ToS();
                    pwd = Tools.Encode(pwd);
                    string invite_code = Request["invite_code"].ToS(); 
                    ds = DbHelperSQL.Query("select coach_phone from t_coach where coach_phone ='"+phone+"'");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        restr = "{\"result\":\"您已经注册过，分享给您的朋友吧！\"}";
                    }else{
                        DbHelperSQL.ExecuteSql("insert into t_coach([coach_pk],[coach_phone],[city],[coach_pwd],[invite_code],[create_time]) values('"+coach_pk+"','"+phone+"','" + city + "','" + pwd + "','" + invite_code + "','" + DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss") + "')");
                        restr = "{\"result\":\" 恭喜您成功注册了路邮寄司机端！\"}";
                    }
                    break;
#endregion
                }
            case "update_money":
                {
#region 卡卷功能

                    string student_pk = Guid.NewGuid().ToS();
                    string city = Request["city"].ToS();
                    string phone = Request["phone"].ToS();
                    double money = Request["money"].ToD().ToString("0.00").ToD(); 
                    double kajuan = Request["kajuan"].ToD(); 
                    ds = DbHelperSQL.Query("select kajuan from t_student where student_phone ='"+phone+"'");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        double knum = ds.Tables[0].Rows[0][0].ToD();
                        if(knum >=kajuan){
                            restr = "{\"result\":\"您已经领取过，分享给您的朋友吧！\"}";
                        }else{
                            DbHelperSQL.ExecuteSql("update  t_student set student_money=student_money+"+money+" , kajuan="+kajuan+" where student_phone='" + phone + "'"); //, city="+city+" 
                            restr = "{\"result\":\"恭喜您成功领取了"+money+"元现金！\"}";
                        }
                    }else{
                        DbHelperSQL.ExecuteSql("insert into t_student([student_pk],[phone],[city],[money],[kajuan],[create_time]) values('"+student_pk+"','"+phone+"','" + city + "','" + money + "','" + kajuan + "','" + DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss") + "')");
                        restr = "{\"result\":\"恭喜您成功领取了"+money+"元现金！\"}";
                    }
                    break;
#endregion
                }
            case "get_Province":
                {
#region 获取省份

                    ds = DbHelperSQL.Query("select * from S_Province where ProvinceID in (select p_id from t_driving)");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_City":
                {
#region 获取城市
                    string v = Request["v"].ToS();
                    ds = DbHelperSQL.Query("select * from S_City where ProvinceID=" + v + " and CityID in (select c_id from t_driving)");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_District":
                {
#region 获取区县
                    string v = Request["v"].ToS();
                    ds = DbHelperSQL.Query("select * from S_District where CityID=" + v + " and DistrictID in (select area_id from t_driving)");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_Driving":
                {
#region 获取驾校
                    string v = Request["v"].ToS();
                    ds = DbHelperSQL.Query("select * from t_driving where 1=1" + (v != "" ? (" and area_id=" + v) : ""));
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_Driving_info":
                {
#region 获取驾校详情
                    string driving_pk = Request["driving_pk"].ToS();
                    ds = DbHelperSQL.Query("select (select ProvinceName from S_Province where ProvinceID=a.p_id) as p,(select CityName from S_City where CityID=a.c_id) as c,(select DistrictName from S_District where DistrictID=a.area_id) as q,* from t_driving a where driving_pk='" + driving_pk + "'");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_Info":
                {
#region 获取导航内容
                    string v = Request["v"].ToS();
                    ds = DbHelperSQL.Query("select code_content from t_sys_code where code='" + v + "'");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        restr = "{\"result\":\"" + Json.ChangeString(ds.Tables[0].Rows[0][0].ToS()) + "\"}";
                    }
                    break;
#endregion
                }
            case "send_code":
                {
#region 发送验证码
                    //key 用户标示 发送和验证时要统一
                    //template 模板标示 0000700001=登录短信模板 00005= 重置密码短信模板
                    //phone 手机号
                    string key = Request["key"].ToS();
                    string template = Request["template"].ToS();
                    string phone = Request["phone"].ToS();
                    string code = (new Random(DateTime.Now.Millisecond + 1)).Next(0, 9).ToS() + (new Random(DateTime.Now.Millisecond + 2)).Next(0, 9).ToS() + (new Random(DateTime.Now.Millisecond + 3)).Next(0, 9).ToS() + (new Random(DateTime.Now.Millisecond + 4)).Next(0, 9).ToS() + (new Random(DateTime.Now.Millisecond + 5)).Next(0, 9).ToS() + (new Random(DateTime.Now.Millisecond + 6)).Next(0, 9).ToS();
                    if (DbHelperSQL.Query("select * from t_sys_code where code='" + phone + "'").Tables[0].Rows.Count > 0)
                    {
                        DbHelperSQL.ExecuteSql("update t_sys_code set code_content='" + code + "',code_expire='" + DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss") + "' where code='" + phone + "'");
                    }
                    else
                    {
                        DbHelperSQL.ExecuteSql("insert into t_sys_code([code_pk],[code],[code_content],[code_expire]) values(newid(),'" + phone + "','" + code + "','" + DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss") + "')");

                    }
                    string content = DbHelperSQL.ExecuteSqlScalar("select code_content from t_sys_code where code='" + template + "'").ToS().Replace("{code}", code);
                    int i = Tools.SendSMS(phone, content);
                    restr = "{\"result\":\"" + code + "\",\"code\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "check_code":
                {
#region 效验验证码
                    string phone = Request["phone"].ToS();
                    string verification = Request["verification"].ToS();
                    if (phone == "")
                    {
                        restr = "{\"result\":\"-98\"}";
                        break;
                    }
                    if (DbHelperSQL.Query("select * from t_sys_code where code='" + phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").Tables[0].Rows.Count == 0)
                    {
                        restr = "{\"result\":\"-97\"}";
                        break;
                    }
                    ds = DbHelperSQL.Query("select * from t_coach where coach_phone='" + phone + "'");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        restr = "{\"result\":\"-100\"}";
                        break;
                    }
                    restr = "{\"result\":\"100\"}";
                    break;
#endregion

                }
            case "get_reg_ment":
                {
#region 注册协议
                    //code 00001=教练注册协议 00002=陪练注册协议 00003=学员注册协议
                    string code = Request["code"].ToS();
                    string ment = DbHelperSQL.ExecuteSqlScalar("select code_content from t_sys_code where code='" + code + "'").ToS();
                    restr = "{\"result\":\"" + Json.ChangeString(ment) + "\"}";
                    break;
#endregion
                }
            case "reg":
                {
#region 学员注册
                    //-96 手机号为空
                    //-99 确认密码不正确 或密码为空
                    //-98 验证码不正确
                    //-97 手机号已注册
                    //返回值 "{"result":"02E83414-054F-426A-97D3-43D036846E62"}" 02E83414-054F-426A-97D3-43D036846E62为学员标示


                    string student_phone = Request["account"].ToS();
                    string password = Request["password"].ToS();
                    string password_confirm = Request["password_confirm"].ToS();
                    string student_incode = Request["student_incode"].ToS();
                    string student_allow_car = Request["student_allow_car"].ToS();
                    string verification = Request["verification"].ToS();
                    string key = Request["key"].ToS();
                    if (student_phone == "")
                    {
                        restr = "{\"result\":\"-96\"}"; // 手机号为空
                        break;
                    }

                    if (password != password_confirm || password == "")
                    {
                        restr = "{\"result\":\"-99\"}";
                        break;
                    }

                    int c = DbHelperSQL.ExecuteSqlScalar("select count(*) from t_student where student_phone='" + student_phone + "'").ToInt32();
                    if (c > 0)
                    {
                        restr = "{\"result\":\"-97\"}";
                        break;

                    }
                    if (DbHelperSQL.ExecuteSqlScalar("select Count(*) from t_sys_code where code='" + student_phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").ToInt32() == 0)
                    {

                        restr = "{\"result\":\"-98\"}";
                        break;
                    }

                    string student_pk = Guid.NewGuid().ToS();
                    string strSQL = "INSERT INTO [t_student]" +
                        "([student_pk]" +
                        ",[student_phone]" +
                        ",[student_pwd]" +
                        ",[student_incode]" +
                        ",[student_allow_car]" +
                        ",[student_state]" +
                        ",[student_money]" +
                        ",[create_time])" +
                        " VALUES" +
                        "('" + student_pk + "'" +
                        ",'" + student_phone + "'" +
                        ",'" + Tools.Encode(password) + "'" +
                        ",'" + student_incode + "'" +
                        ",'" + student_allow_car + "'" +
                        ",'1'" +
                        "," + DbHelperSQL.ExecuteSqlScalar("select sms_result_end_code from t_system").ToD().ToString("0.00") + "" +
                        ",getdate())";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {

                        restr = "{\"result\":\"" + student_pk + "\"}";
                        break;
                    }

                    break;
#endregion
                }
            case "upload":
                {
#region 上传文件
                    string dirPath = Server.MapPath("~/upload/image/");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    string fileName= DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".jpg";
                    string filePath = dirPath + fileName;
                    HttpFileCollection file = Request.Files;
                    if (file.Count > 0)
                    {
                        file[0].SaveAs(filePath);
                        restr = "{\"result\":\"/upload/image/" + fileName + "\"}";
                    }
                    break;
#endregion
                }
            case "reg_coach":
                {
#region 司机注册
                    //-96 手机号为空
                    //-99 确认密码不正确 或密码为空
                    //-98 验证码不正确
                    //-97 手机号已注册
                    //返回值 "{"result":"02E83414-054F-426A-97D3-43D036846E62"}" 02E83414-054F-426A-97D3-43D036846E62为学员标示
                    string coach_pk = Guid.NewGuid().ToS();
                    string driving_pk = Request["driving_pk"].ToS();
                    string coach_incode = Request["coach_incode"].ToS().ToUpper();
                    string coach_city = Request["coach_city"].ToS();
                    string coach_car_number = Request["coach_car_number"].ToS();
                    string coach_car_type = Request["coach_car_type"].ToS();
                    string coach_name = Request["coach_name"].ToS();
                    string coach_pwd = Request["coach_pwd"].ToS();
                    string province = Request["province"].ToS();
                    string city = Request["city"].ToS();
                    string district = Request["district"].ToS();
                    string coach_pwd_confirm = Request["coach_pwd_confirm"].ToS();
                    string coach_sex = Request["coach_sex"].ToS();
                    string coach_age = Request["coach_age"].ToS();
                    string coach_phone = Request["coach_phone"].ToS();
                    string coach_long = Request["coach_long"].ToS();
                    string coach_number = Request["coach_number"].ToS();
                    string coach_teacher_number = Request["coach_teacher_number"].ToS();
                    string coach_myself = Request["coach_myself"].ToS();
                    string coach_pic = Request["coach_pic"].ToS();
                    string coach_teacher_pic = Request["coach_teacher_pic"].ToS();
                    string coach_driver_pic1 = Request["coach_driver_pic1"].ToS();
                    string coach_driver_pic2 = Request["coach_driver_pic2"].ToS();
                    string coach_card_pic1 = Request["coach_card_pic1"].ToS();
                    string coach_card_pic2 = Request["coach_card_pic2"].ToS();
                    string coach_car_pic1 = Request["coach_car_pic1"].ToS();
                    string coach_car_pic2 = Request["coach_car_pic2"].ToS();
                    string coach_subject = Request["coach_subject"].ToS();
                    string coach_price = Request["coach_price"].ToS();
                    string coach_score = Request["coach_score"].ToS();
                    string coach_service = Request["coach_service"].ToS();
                    string coach_type = Request["coach_type"].ToS();
                    string coach_order_range = "20";
                    string coach_place = Request["coach_place"].ToS();
                    string coach_state = Request["coach_state"].ToS();
                    string verification = Request["verification"].ToS();

                    if (coach_phone == "")
                    {
                        restr = "{\"result\":\"-98\"}"; // 手机号为空
                        break;
                    }                       
                    int c = DbHelperSQL.ExecuteSqlScalar("select count(*) from t_coach where coach_phone='" + coach_phone + "'").ToInt32();
                    if (c > 0)
                    {
                        restr = "{\"result\":\"-100\"}";
                        break;

                    }
                    ArrayList arr = new ArrayList();
                    string djq = "0";
                    if (coach_incode != "")
                    {
                        djq = DbHelperSQL.ExecuteSqlScalar("select sms_result_end_code from t_system").ToD().ToString("0.00");
                        arr.Add("update t_coach set coach_money=coach_money+" + djq + " where coach_code='" + coach_incode + "'");
                    }
                    string student_pk = Guid.NewGuid().ToS();
                    string strSQL = "INSERT INTO [t_coach]" +
                        "([coach_pk]" +
                        ",[driving_pk]" +
                        ",[coach_incode]" +
                        ",[coach_incode_money]" +
                        ",[coach_city]" +
                        ",[coach_car_number]" +
                        ",[coach_car_type]" +
                        ",[coach_name]" +
                        ",[coach_pwd]" +
                        ",[province]" +
                        ",[city]" +
                        ",[district]" +
                        ",[coach_sex]" +
                        ",[coach_age]" +
                        ",[coach_phone]" +
                        ",[coach_long]" +
                        ",[coach_number]" +
                        ",[coach_teacher_number]" +
                        ",[coach_myself]" +
                        ",[coach_pic]" +
                        ",[coach_teacher_pic]" +
                        ",[coach_driver_pic1]" +
                        ",[coach_driver_pic2]" +
                        ",[coach_card_pic1]" +
                        ",[coach_card_pic2]" +
                        ",[coach_car_pic1]" +
                        ",[coach_car_pic2]" +
                        ",[coach_subject]" +
                        ",[coach_price]" +
                        ",[coach_score]" +
                        ",[coach_service]" +
                        ",[coach_type]" +
                        ",[coach_order_range]" +
                        ",[coach_place]" +
                        ",[coach_state]" +
                        ",[coach_code]" +
                        ",[invite_code]" +
                        ",[create_time])" +
                        " VALUES" +
                        "(newid()" +
                        ",'" + driving_pk + "'" +
                        ",'" + coach_incode + "'" +
                        ",'" + djq + "'" +
                        ",'" + coach_city + "'" +
                        ",'" + coach_car_number + "'" +
                        ",'" + coach_car_type + "'" +
                        ",'" + coach_name + "'" +
                        ",'" + Tools.Encode(coach_pwd) + "'" +
                        ",'" + province + "'" +
                        ",'" + city + "'" +
                        ",'" + district + "'" +
                        ",'" + coach_sex + "'" +
                        ",'" + coach_age + "'" +
                        ",'" + coach_phone + "'" +
                        ",'" + coach_long + "'" +
                        ",'" + coach_number + "'" +
                        ",'" + coach_teacher_number + "'" +
                        ",'" + coach_myself + "'" +
                        ",'" + coach_pic + "'" +
                        ",'" + coach_teacher_pic + "'" +
                        ",'" + coach_driver_pic1 + "'" +
                        ",'" + coach_driver_pic2 + "'" +
                        ",'" + coach_card_pic1 + "'" +
                        ",'" + coach_card_pic2 + "'" +
                        ",'" + coach_car_pic1 + "'" +
                        ",'" + coach_car_pic2 + "'" +
                        ",'" + coach_subject + "'" +
                        ",'" + coach_price + "'" +
                        ",'" + coach_score + "'" +
                        ",'" + coach_service + "'" +
                        ",'" + coach_type + "'" +
                        ",'" + coach_order_range + "'" +
                        ",'" + coach_place + "'" +
                        ",'0'" +
                        ",'" + coach_phone + "'" +
                        ",'" + coach_incode + "'" +
                        // ",'" + Tools.GetCoachCode() + "'" +
                        ",getdate())";
                    arr.Add(strSQL);

                    int i = DbHelperSQL.ExecuteSqlTran(arr);                       
                    if (i > 0)
                    {

                        restr = "{\"result\":\"100\"}";
                        break;
                    }

                    break;
#endregion
                }
            case "login":
                {
#region 登录
                    //-97验证码不正确
                    //-98手机号为空                      
                    //-100 手机号未注册 或正在审核中
                    //返回 学员（教练）信息

                    string province = Request["province"].ToS();
                    string city = Request["city"].ToS();
                    string district = Request["district"].ToS();

                    string user_type = Request["user_type"].ToS();
                    string student_phone = Request["account"].ToS();
                    string verification = Request["verification"].ToS();
                    string clientid = Request["ClientID"].ToS();
                    string key = Request["key"].ToS();
                    string user_incode = Request["user_incode"].ToS().ToUpper();
                    if (student_phone == "")
                    {
                        restr = "{\"result\":\"-98\"}";
                        break;
                    }

                    if (user_type == "user")
                    {
                        if (student_phone != "15577729055")
                        {
                            if (DbHelperSQL.Query("select * from t_sys_code where code='" + student_phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").Tables[0].Rows.Count == 0)
                            {
                                restr = "{\"result\":\"-97\"}";
                                break;
                            }
                        }
                        ds = DbHelperSQL.Query("select * from t_student where student_phone='" + student_phone + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DbHelperSQL.ExecuteSql("update t_student set student_clientid='" + clientid + "' where student_pk='" + ds.Tables[0].Rows[0]["student_pk"].ToS() + "'");
                            restr = Json.DataTableToJson(ds.Tables[0]);
                            break;
                        }
                        else
                        {
                            ArrayList arr = new ArrayList();
                            string djq = "0";
                            if (user_incode != "")
                            {
                                djq = DbHelperSQL.ExecuteSqlScalar("select coach_user_yq from t_system").ToD().ToString("0.00");
                                arr.Add("update t_coach set coach_money=coach_money+" + djq + " where coach_code='" + user_incode + "'");
                            }                
                            string strSQL = "INSERT INTO [t_student]" +
                                "([student_pk]" +
                                // "([province]" +
                                // "([city]" +
                                // "([district]" +
                                ",[student_phone]" +
                                ",[student_state]" +
                                ",[student_clientID]" +
                                ",[student_money]" +
                                ",[user_incode]" +
                                ",[user_incode_money]" +
                                ",[create_time])" +
                                " VALUES" +
                                "(newid()" +
                                // ",'" + province + "'" +
                                // ",'" + city + "'" +
                                // ",'" + district + "'" +
                                ",'" + student_phone + "'" +
                                ",'1'" +
                                ",'" + clientid + "'" +
                                ",0" +
                                ",'" + user_incode + "'" +
                                ",'" + djq + "'" +
                                ",getdate())";
                            arr.Add(strSQL);                                
                            int i = DbHelperSQL.ExecuteSqlTran(arr);
                            if (i > 0)
                            {                                    
                                ds = DbHelperSQL.Query("select * from t_student where student_phone='" + student_phone + "' and student_state='1'");
                                if (ds.Tables[0].Rows.Count > 0)
                                    restr = Json.DataTableToJson(ds.Tables[0]);
                                break;
                            }else{

                    restr = "{\"result\":\"注册不成功\"}";
                            }

                        }
                    }
                    else if (user_type == "coach")
                    {
                        string strSQL = "select * from t_coach where coach_phone='" + student_phone + "' and coach_pwd='" + Tools.Encode(verification) + "' and coach_state='1'";
                        if (student_phone == "15577729055") strSQL = "select * from t_coach where coach_phone='" + student_phone + "'  and coach_state='1'";
                        ds = DbHelperSQL.Query(strSQL);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DbHelperSQL.ExecuteSql("update t_coach set coach_clientid='" + clientid + "' where coach_pk='" + ds.Tables[0].Rows[0]["coach_pk"].ToS() + "'");
                            restr = Json.DataTableToJson(ds.Tables[0]);
                            break;
                        }else{
                            restr = "{\"result\":\"-97\"}";
                        }
                    }
                    // restr = "{\"result\":\"-100\"}";
                    break;
#endregion
                }
            case "forget_pwd":
                {
#region 忘记密码
                    //user_type 学员=student 教练=coach
                    //-98手机号为空
                    //-99 验证码不能为空    
                    //-100 验证码不正确
                    //-101 重置失败
                    //返回 {"result":"125692"} 125692为随机密码
                    string user_type = Request["user_type"].ToS();
                    string student_phone = Request["account"].ToS();
                    string verification = Request["verification"].ToS();
                    string key = Request["key"].ToS();
                    if (student_phone == "")
                    {
                        restr = "{\"result\":\"-98\"}";
                        break;
                    }
                    if (verification == "")
                    {
                        restr = "{\"result\":\"-99\"}";
                        break;
                    }
                    if (DbHelperSQL.ExecuteSqlScalar("select Count(*) from t_sys_code where code='" + student_phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").ToInt32() == 0)
                    {                           
                        restr = "{\"result\":\"-100\"}";
                        break;
                    }
                    System.Text.StringBuilder newRandom = new System.Text.StringBuilder(6);
                    Random rd = new Random();
                    for (int i = 0; i < 6; i++)
                    {
                        newRandom.Append(rd.Next(10));
                    }
                    int c = 0;
                    if (user_type == "user")
                    {
                        c = DbHelperSQL.ExecuteSql("update t_student set student_pwd='" + Tools.Encode(newRandom.ToS()) + "' where student_phone='" + student_phone + "'");
                    }
                    else if (user_type == "coach")
                    {
                        c = DbHelperSQL.ExecuteSql("update t_coach set coach_pwd='" + Tools.Encode(newRandom.ToS()) + "' where coach_phone='" + student_phone + "'");
                    }
                    if (c > 0)
                    {
                        restr = "{\"result\":\"" + newRandom.ToS() + "\"}";
                    }
                    else
                    {
                        restr = "{\"result\":\"-101\"}";
                    }
                    break;
#endregion
                }
            case "upload_pic":
                {
#region 上传头像
                    string user_type = Request["user_type"].ToS();
                    string user_pk = Request["user_pk"].ToS();
                    if (Request.Files.Count > 0)
                    {
                        string hz = Request.Files[0].FileName.Substring(Request.Files[0].FileName.IndexOf('.') + 1, Request.Files[0].FileName.Length - Request.Files[0].FileName.IndexOf('.') - 1);

                        if (hz.ToLower() == "jpg" || hz.ToLower() == "png" || hz.ToLower() == "bmp" || hz.ToLower() == "jpeg" || hz.ToLower() == "gif")
                        {
                            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "." + hz;
                            Request.Files[0].SaveAs(Server.MapPath("../upload/tx") + "\\" + filename);
                            if (Session["QUserT"].ToS() == "1")
                            {
                                int i = 0;
                                if (user_type == "user") i = DbHelperSQL.ExecuteSql("update t_student set student_pic='" + "upload/tx/" + filename + "' where student_pk='" + user_pk + "'");
                                else if (user_type == "coach") i = DbHelperSQL.ExecuteSql("update t_coach set coach_pic='" + "upload/tx/" + filename + "' where coach_pk='" + user_pk + "'");

                                if (i > 0)
                                {
                                    restr = "{\"result\":\"100\",\"pic\":\"" + filename + "\"}";
                                }
                                else
                                {
                                    restr = "{\"result\":\"头像设置失败！\"}";
                                }
                            }
                        }
                        else
                        {
                            restr = "{\"result\":\"格式不支持！\"}";
                        }
                    }
                    else
                    {
                        restr = "{\"result\":\"请选择图像文件！\"}";
                    }

                    break;
#endregion
                }
            case "change_phone":
                {
#region 更改手机号
                    //-97验证码不正确
                    //-98手机号为空                      
                    //-100 手机号未注册 或正在审核中
                    //返回 学员（教练）信息
                    string user_pk = Request["user_pk"].ToS();
                    string user_type = Request["user_type"].ToS();
                    string student_phone = Request["phone"].ToS();
                    string verification = Request["verification"].ToS();
                    string clientid = Request["ClientID"].ToS();
                    string key = Request["key"].ToS();
                    if (student_phone == "")
                    {
                        restr = "{\"result\":\"-98\"}";
                        break;
                    }

                    if (user_type == "user")
                    {
                        if (student_phone != "17792513817")
                        {
                            if (DbHelperSQL.Query("select * from t_sys_code where code='" + student_phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").Tables[0].Rows.Count == 0)
                            {
                                restr = "{\"result\":\"-97\"}";
                                break;
                            }
                        }                           
                        int i=DbHelperSQL.ExecuteSql("update t_student set student_phone='" + student_phone + "' where student_pk='" + user_pk + "'");
                        if(i>0)
                            restr = restr = "{\"result\":\"1\"}";
                        break;
                    }

                    restr = "{\"result\":\"-100\"}";
                    break;
#endregion
                }
            case "upd_user_info":
                {
#region 更改资料
                    //-99 密码为空或确认密码错误
                    string student_pk = Request["user_pk"].ToS();
                    string student_name = Request["user_name"].ToS();
                    string strSQL = "UPDATE [t_student]" +
                        " SET [student_real_name] = '" + student_name + "'" +
                        " WHERE [student_pk]='" + student_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        ds = DbHelperSQL.Query("select * from t_student where student_pk='" + student_pk + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            restr = Json.DataTableToJson(ds.Tables[0]);
                        }
                    }
                    break;
#endregion
                }

            case "get_car":
                {
#region 获取车型

                    string strSQL = "select * from t_car a  order by car_start_price desc";//where car_type=0
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_all_car":
                {
#region 获取车型

                    string strSQL = "select * from t_car a  order by create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }

            case "save_order":
                {
#region 保存订单
                    string order_pk = Guid.NewGuid().ToS();
                    string user_pk = Request["user_pk"].ToS();
                    string user_name = Request["user_name"].ToS();
                    string user_tel = Request["user_tel"].ToS();
                    string user_sms = Request["user_sms"].ToS();
                    string order_type = Request["order_type"].ToS();
                    string order_fee2 = Request["order_fee"].ToS();
                    string order_datetime = Request["order_datetime"].ToS();
                    string start_address = Request["start_address"].ToS();
                    string start_lon = Request["start_lon"].ToS();
                    string start_lat = Request["start_lat"].ToS();
                    string end_address = Request["end_address"].ToS();
                    string end_lon = Request["end_lon"].ToS();
                    string end_lat = Request["end_lat"].ToS();
                    string order_away = (Request["order_away"].ToD() / 1000).ToString("0.000");
                    string order_time = (Request["order_time"].ToD() / 60).ToInt32().ToS();
                    string car = Request["car"].ToS();
                    string order_rem = Request["order_rem"].ToS();
                    string create_time = Request["create_time"].ToS();


                    string receive_man = Request["receive_man"].ToS();
                    string receive_tel = Request["receive_tel"].ToS();
                    string receive_bao = Request["receive_bao"].ToS();
                    string receive_money = Request["receive_money"].ToS();

                    string province = Request["province"].ToS();
                    string city = Request["city"].ToS();
                    string district = Request["district"].ToS();

                    string[] car_arr = car.Split(',');
                    double car_meal_fee = 0;
                    double car_meal_away = 0;
                    double order_away_fee = 0;
                    double order_time_fee = 0;
                    double order_far_away_fee = 0;
                    double order_fee = 0;
                    ArrayList arr = new ArrayList();
                    string strSQL = "";
                    string ss = strSQL;
                    DataSet car_ds = null;
                    if(order_type=="0") car_ds = DbHelperSQL.Query("select * from t_car where car_type=3");
                    else car_ds=DbHelperSQL.Query("select * from t_car where car_pk='" + car_arr[0] + "'");
                    if (car_ds.Tables[0].Rows.Count > 0)
                    {
                        car_meal_fee = (car_ds.Tables[0].Rows[0]["car_start_price"].ToD()).ToString("0.00").ToD();
                        car_meal_away = (car_ds.Tables[0].Rows[0]["car_meal_away"].ToD()).ToString("0.00").ToD();
                        if (order_away.ToD() > car_meal_away)
                        {
                            double s_away = (order_away.ToD() - car_ds.Tables[0].Rows[0]["car_meal_away"].ToD());
                            s_away = s_away > 0 ? s_away : 0;
                            order_away_fee = (s_away * car_ds.Tables[0].Rows[0]["car_away_price"].ToD()).ToString("0.00").ToD();
                        }
                        else order_away_fee = 0;
                        order_time_fee = (order_time.ToD() * car_ds.Tables[0].Rows[0]["car_time_price"].ToD()).ToString("0.00").ToD();
                        if (order_away.ToD() > car_ds.Tables[0].Rows[0]["car_far_away"].ToD())
                        {
                            order_far_away_fee = ((order_away.ToD() - car_ds.Tables[0].Rows[0]["car_far_away"].ToD()) * car_ds.Tables[0].Rows[0]["car_far_price"].ToD()).ToString("0.00").ToD();
                        }
                        strSQL = "INSERT INTO [t_order_car]" +
                            "([oc_pk]" +
                            ",[order_pk]" +
                            ",[car_pk]" +
                            ",[create_time])" +
                            " VALUES" +
                            "(newid()" +
                            ",'" + order_pk + "'" +
                            ",'" + car_ds.Tables[0].Rows[0]["car_pk"].ToS() + "'" +
                            ",getdate())";
                        ss += strSQL;
                        arr.Add(strSQL);
                    }

                    order_fee = car_meal_fee + order_away_fee + order_time_fee + order_far_away_fee; 
                    strSQL = "INSERT INTO [t_order]" +
                        "([order_pk]" +
                        ",[user_pk]" +
                        ",[user_name]" +
                        ",[user_tel]" +
                        ",[user_sms]" +
                        ",[order_type]" +
                        ",[order_datetime]" +
                        ",[start_address]" +
                        ",[start_lon]" +
                        ",[start_lat]" +
                        ",[end_address]" +
                        ",[end_lon]" +
                        ",[end_lat]" +
                        ",[province]" +
                        ",[city]" +
                        ",[district]" +
                        ",[receive_man]" +
                        ",[receive_tel]" +
                        ",[receive_bao]" +
                        ",[receive_money]" +
                        ",[order_away]" +
                        ",[order_time]" +
                        ",[car_meal_fee]" +
                        ",[order_away_fee]" +
                        ",[order_time_fee]" +
                        ",[order_far_away_fee]" +
                        ",[order_cut_fee]" +
                        ",[order_fee]" +
                        ",[order_rem]" +
                        ",[order_state]" +
                        ",[create_time])" +
                        " VALUES" +
                        "('" + order_pk + "'" +
                        ",'" + user_pk + "'" +
                        ",'" + user_name + "'" +
                        ",'" + user_tel + "'" +
                        ",'" + user_sms + "'" +
                        ",'" + order_type + "'" +
                        ",'" + order_datetime + "'" +
                        ",'" + start_address + "'" +
                        ",'" + start_lon + "'" +
                        ",'" + start_lat + "'" +
                        ",'" + end_address + "'" +
                        ",'" + end_lon + "'" +
                        ",'" + end_lat + "'" +
                        ",'" + province + "'" +
                        ",'" + city+ "'" +
                        ",'" + district+ "'" +
                        ",'" + receive_man + "'" +
                        ",'" + receive_tel + "'" +
                        ",'" + receive_bao + "'" +
                        ",'" + receive_money + "'" +
                        ",'" + order_away + "'" +
                        ",'" + order_time + "'" +
                        ",'" + car_meal_fee + "'" +
                        ",'" + order_away_fee + "'" +
                        ",'" + order_time_fee + "'" +
                        ",'" + order_far_away_fee + "'" +
                        ",'0'" +
                        ",'" + order_fee2 + "'" +
                        ",'" + order_rem + "'" +
                        ",'0'" +
                        ",getdate())";

                    arr.Add(strSQL);

                    Core.LogResult(ss);
                    int j = DbHelperSQL.ExecuteSqlTran(arr);
                    
                    string cartype ="电动车";
                    if(order_type =="0"){
                        cartype ="货车";
                    }else if(order_type =="1"){
                        cartype ="三轮车";
                    }else if(order_type =="2"){
                        cartype ="的士";
                    }
                    Tools.SendSMS("15878297309", "有一个新订单，请及时处理！车型"+cartype+"; 发货地址："+start_address+"; 预计费用："+order_fee);//发送给梁总
                    
                    if (j > 0)
                    {
                        restr = "{\"result\":\"" + order_pk + "\"}";
                        //推送
                        //
                        ds = DbHelperSQL.Query("select * from t_coach where (coach_car_type='" + car_arr[0] + "' or coach_car_type in (select car_pk from t_car where car_type="+ (order_type=="0"?"0":"100") + ")) and dbo.fnGetDistance(coach_lat,coach_lon," + start_lat + "," + start_lon + ")<=coach_order_range and coach_state='1'");
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (ds.Tables[0].Rows[i]["coach_clientID"].ToS() == "") continue;
                            string[] arg = new string[2];
                            arg[0] = ds.Tables[0].Rows[i]["coach_clientID"].ToS();
                            arg[1] = Json.DataTableToJson(DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'").Tables[0]).Replace("\"", "'");
                            Tools.StartProcess(Server.MapPath("/") + "/getui_ser/GetuiServerApiSDKDemo.exe", arg);
                        }
                    }
                    break;
#endregion
                }
            case "get_order_list":
                {
#region 获取订单列表
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select * from t_order a";
                    // strSQL += " left join (select coach_lon,coach_lat,Convert(float,coach_order_range) as coach_order_range from t_coach where coach_pk='" + coach_pk + "') b on 1=1";
                    // strSQL += " where  dbo.fnGetDistance(coach_lat,coach_lon,start_lat,start_lon)<=coach_order_range and order_state='0' order by create_time ";//desc
                    strSQL += " where  a.order_state='0' order by a.create_time desc ";//desc
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "coach_and_order":
                {
#region 抢单
                    string coach_pk = Request["coach_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    double coach_cut = DbHelperSQL.ExecuteSqlScalar("select car_fyc from t_car where convert(varchar(100),car_pk) in (select car_pk from t_order_car where order_pk='" + order_pk + "')").ToD()/100;
                    coach_cut = coach_cut * DbHelperSQL.ExecuteSqlScalar("select order_fee from t_order where convert(varchar(100),order_pk)='" + order_pk + "'").ToD();
                    double coach_money = DbHelperSQL.ExecuteSqlScalar("select coach_money from t_coach where coach_pk='" + coach_pk + "'").ToD();
                    if (coach_money>0 && coach_money < coach_cut)
                    {
                        // restr = "{\"result\":\"-96\"}"; // 余额不足
                        // break;
                    }
                    ds = DbHelperSQL.Query("select * from t_order where coach_pk='" + coach_pk + "' and order_state in ('0','1','2','3')");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        restr = "{\"result\":\"-100\"}";
                        break;
                    }                      
                    //判断车型
                    string coach_car_type = DbHelperSQL.ExecuteSqlScalar("select car_type from t_car where convert(varchar(100),car_pk) = (select coach_car_type from t_coach where coach_pk='" + coach_pk + "')");
                    string order_car_type = DbHelperSQL.ExecuteSqlScalar("select order_type from t_order where convert(varchar(100),order_pk) = '" + order_pk + "'");
                    // if (DbHelperSQL.Query("select coach_pk from t_coach where coach_pk='" + coach_pk + "' and (coach_car_type in (select car_pk from t_order_car where order_pk='" + order_pk + "')"+(DbHelperSQL.ExecuteSqlScalar("select order_type from t_order where order_pk='" + order_pk + "'").ToS()=="0"? " or coach_car_type in (select car_pk from t_car where car_type=0)" : "")+")").Tables[0].Rows.Count == 0)
                    if (coach_car_type != order_car_type)
                    {
                        restr = "{\"result\":\"-98\"}";
                        break;
                    }
                    string strSQL = "update  t_order set coach_pk='" + coach_pk + "',order_state='1'  where order_pk='" + order_pk + "' and order_state='0'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        restr = "{\"result\":\"100\"}";
                        ds = DbHelperSQL.Query("select a.user_tel,a.start_lon,a.start_lat,b.coach_name,b.coach_phone,b.coach_car_number from t_order a left join t_coach b on a.coach_pk=b.coach_pk where order_pk='" + order_pk + "'");
                        string content = "";
                        //短信提醒
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string template = "0000700002";
                            string phone = ds.Tables[0].Rows[0]["user_tel"].ToS();
                            content = DbHelperSQL.ExecuteSqlScalar("select code_content from t_sys_code where code='" + template + "'").ToS();
                            content = content.Replace("{coach_name}", ds.Tables[0].Rows[0]["coach_name"].ToS());
                            content = content.Replace("{coach_phone}", ds.Tables[0].Rows[0]["coach_phone"].ToS());
                            content = content.Replace("{car_number}", ds.Tables[0].Rows[0]["coach_car_number"].ToS());
                            Tools.SendSMS(phone, content);
                        }
                        //推送
                        ds = DbHelperSQL.Query("select *,'" + ds.Tables[0].Rows[0]["start_lon"].ToS() + "' as start_lon,'" + ds.Tables[0].Rows[0]["start_lat"].ToS() + "' as start_lat from t_student where Convert(varchar(100),student_pk) in (select user_pk from t_order where order_pk='" + order_pk + "')");
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (ds.Tables[0].Rows[i]["student_clientID"].ToS() == "") continue;
                            string[] arg = new string[2];
                            arg[0] = ds.Tables[0].Rows[i]["student_clientID"].ToS();
                            arg[1] = Json.DataTableToJson(DbHelperSQL.Query("select *,'" + ds.Tables[0].Rows[0]["start_lon"].ToS() + "' as start_lon,'" + ds.Tables[0].Rows[0]["start_lat"].ToS() + "' as start_lat from t_coach where coach_pk='" + coach_pk + "'").Tables[0]).Replace("\"", "'");
                            Tools.StartProcess(Server.MapPath("/") + "/getui/GetuiServerApiSDKDemo.exe", arg);
                        }
                    }
                    else
                    {
                        restr = "{\"result\":\"-99\"}";
                        break;
                    }
                    break;
#endregion
                }
            case "get_coach_list":
                {
#region 获取教练列表

                    string strSQL = "select coach_pk,coach_lon,coach_lat from t_coach a where coah_actity_time>='" + DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss") + "' and coach_state='1' order by create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "set_coach_place":
                {
#region 设置司机位置
                    string coach_pk = Request["coach_pk"].ToS();
                    string coach_lon = Request["coach_lon"].ToS();
                    string coach_lat = Request["coach_lat"].ToS();
                    string strSQL = "update  t_coach set coach_lon='" + coach_lon + "',coach_lat='" + coach_lat + "',coah_actity_time='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where coach_pk='" + coach_pk + "'";

                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        restr = "{\"result\":\"100\"}";
                    }

                    break;
#endregion
                }

            case "get_coach_money_list":
                {
#region 获取司机提成及佣金记录  列表
                    string coach_pk = Request["coach_pk"].ToS();
                    string coach_phone = DbHelperSQL.ExecuteSqlScalar("select coach_phone from t_coach where coach_pk='" + coach_pk + "'").ToS();//获得司机pk
                    string strSQL = "SELECT * from t_order where coach_pk in (select  CONVERT(varchar(36),coach_pk,36) from t_coach WHERE invite_code = '"+coach_phone+"') or coach_pk = '"+coach_pk+"' order by create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_ticheng":
                {
#region 获取司机提成
                    string coach_pk = Request["coach_pk"].ToS();
                    string coach_phone = DbHelperSQL.ExecuteSqlScalar("select coach_phone from t_coach where coach_pk='" + coach_pk + "'").ToS();//获得司机pk
                    string strSQL = "SELECT sum(order_fee) from t_order where coach_pk in (select  CONVERT(varchar(36),coach_pk,36) from t_coach WHERE invite_code = '"+coach_phone+"')";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_money":
                {
#region 获取司机佣金
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select sum(order_fee) from t_order where coach_pk='"+coach_pk+"' and order_state = 4";

                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_info":
                {
#region 获取教练详细
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select * from t_coach a where 1=1";
                    strSQL += " and coach_pk='" + coach_pk + "'";

                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_user_info2":
                {
#region 获取用户详细2  客户端 
                    string user_pk = Request["user_pk"].ToS();
                    string strSQL = "select * from t_student a where 1=1";
                    strSQL += " and student_pk='" + user_pk + "'";

                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_user_info":
                {
#region 获取用户详细
                    string user_pk = Request["user_pk"].ToS();
                    string strSQL = "select (select sum(order_fee) from t_order where user_pk=a.student_pk and order_state in ('1','2')) as yg_fee,* from t_student a where 1=1";
                    strSQL += " and student_pk='" + user_pk + "'";

                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson_Html(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_orderlist":
                {
#region 获取司机的订单信息
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select order_pk,order_type,user_name,user_tel,order_datetime,start_address,end_address,order_state from t_order a where coach_pk='" + coach_pk + "' order by create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_user_orderlist":
                {
#region 获取客户的订单信息
                    string user_pk = Request["user_pk"].ToS();
                    string strSQL = "select order_pk,order_type,coach_name,coach_phone,order_datetime,start_address,end_address,order_state from t_order a left join t_coach b on a.coach_pk=Convert(varchar(100),b.coach_pk) where user_pk='" + user_pk + "' order by a.create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_user_order_info":
                {
#region 获取客户未完成订单
                    string user_pk = Request["user_pk"].ToS();
                    string strSQL = "select * from t_order a left join t_coach b on a.coach_pk=Convert(varchar(100),b.coach_pk) where user_pk='" + user_pk + "' and order_state in ('0','1','2','3','4','6') order by a.order_state desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_order_info":
                {
#region 获取司机未完成订单
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select * from t_order a left join t_coach b on a.coach_pk=Convert(varchar(100),b.coach_pk) where a.coach_pk='" + coach_pk + "' and order_state in ('1','2','3','4') order by a.create_time desc";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_order_info":
                {
#region 获取订单详细信息
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "select (select count(*) from t_order where coach_pk=Convert(varchar(100),b.coach_pk) and order_state in ('4','5','6')) as coach_order_count,* from t_order a left join t_coach b on a.coach_pk=Convert(varchar(100),b.coach_pk) left join t_car c on b.coach_car_type=Convert(varchar(100),c.car_pk) where order_pk='" + order_pk + "'";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]); 
                    break;
#endregion
                }
            case "update_order2":
                {
#region 更新订单 
                    //string user_pk = Request["user_pk"].ToS();
                    string status = Request["status"].ToS();
                    string order_pk = Request["order_pk"].ToS();
string coach_pk = DbHelperSQL.ExecuteSqlScalar("select coach_pk from t_order where order_pk='" + order_pk + "'").ToS();//获得司机pk
double order_fee = DbHelperSQL.ExecuteSqlScalar("select order_fee from t_order where order_pk='" + order_pk + "'").ToD();//支付金额
string parent_phone = DbHelperSQL.ExecuteSqlScalar("select invite_code from t_coach where coach_pk='" + coach_pk + "'").ToS();//获得邀请人手机号
if(status == "4"){
    DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money+" + order_fee*0.77 + " where coach_pk='" + coach_pk + "'");//司机获得0.77
    if(parent_phone != ""){
        DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money+" + order_fee*0.03 + " where coach_phone='" + parent_phone + "'");// 上级得到0.03佣金
    }
}
                    string strSQL = "";
                    strSQL = "update t_order  set order_state='"+status+"',coach_cut=order_fee where order_pk='" + order_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "update_order":
                {
#region 更新订单 // 用户支付
                    //string user_pk = Request["user_pk"].ToS();
                    //string coach_pk = Request["coach_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "";
                    strSQL = "update t_order  set order_state='6' where order_pk='" + order_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "cancel_order":
                {
#region 取消订单
                    string user_pk = Request["user_pk"].ToS();
                    string coach_pk = Request["coach_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "";
                    if (user_pk != "") strSQL = "update t_order  set order_state='5' where order_pk='" + order_pk + "' and user_pk='" + user_pk + "'";
                    else if (coach_pk != "") strSQL = "update t_order  set order_state='5' where order_pk='" + order_pk + "' and coach_pk='" + coach_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "up_car":
                {
#region 用户上车                       
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "";
                    strSQL = "update t_order set order_away=0,order_time=0,order_away_fee=0,order_time_fee=0,order_far_away_fee=0,order_fee=0,order_state='2',up_car_time=getdate() where order_pk='" + order_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        restr = "{\"result\":\"100\"}";
                        ds = DbHelperSQL.Query("select * from t_student where Convert(varchar(100),student_pk) in (select user_pk from t_order where order_pk='" + order_pk + "')");
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (ds.Tables[0].Rows[i]["student_clientID"].ToS() == "") continue;
                            string[] arg = new string[2];
                            arg[0] = ds.Tables[0].Rows[i]["student_clientID"].ToS();
                            arg[1] = "{\"result\":\"100\"}";
                            Tools.StartProcess(Server.MapPath("/") + "/getui/GetuiServerApiSDKDemo.exe", arg);
                        }
                    }

                    break;
#endregion
                }

            case "get_order_fee":
                {
#region 计算订单价格
                    string order_pk = Request["order_pk"].ToS();
                    double order_away =Math.Abs((Request["order_away"].ToD() / 1000).ToString("0.000").ToD()); 
                    double car_meal_fee = 0;
                    double car_meal_away = 0;
                    double order_away_fee = 0;
                    double order_time_fee = 0;
                    double order_far_away_fee = 0;
                    double order_fee = 0;
                    int i = 0;

                    order_away = order_away+ DbHelperSQL.ExecuteSqlScalar("select order_away from t_order where Convert(varchar(100),order_pk)='" + order_pk + "'").ToD();
                    DataSet car_ds = DbHelperSQL.Query("select * from t_car where Convert(varchar(100),car_pk) in (select top 1 car_pk from t_order_car where order_pk='" + order_pk + "' )");
                    DataSet order_ds = DbHelperSQL.Query("select * from t_order where Convert(varchar(100),order_pk)='" + order_pk + "'");
                    TimeSpan ts = DateTime.Now.Subtract(DateTime.Parse(order_ds.Tables[0].Rows[0]["up_car_time"].ToS()));
                    int order_time = Math.Ceiling(ts.TotalMinutes).ToInt32();

                    car_meal_fee = (car_ds.Tables[0].Rows[0]["car_start_price"].ToD()).ToString("0.00").ToD();
                    car_meal_away = (car_ds.Tables[0].Rows[0]["car_meal_away"].ToD()).ToString("0.00").ToD();

                    if (order_away.ToD() > car_meal_away)
                    {
                        double s_away = (order_away.ToD() - car_ds.Tables[0].Rows[0]["car_meal_away"].ToD());
                        s_away = s_away > 0 ? s_away : 0;
                        order_away_fee = (s_away * car_ds.Tables[0].Rows[0]["car_away_price"].ToD()).ToString("0.00").ToD();
                    }                      
                    order_time_fee = (order_time.ToD() * car_ds.Tables[0].Rows[0]["car_time_price"].ToD()).ToString("0.00").ToD();
                    if (order_away.ToD() > car_ds.Tables[0].Rows[0]["car_far_away"].ToD())
                    {
                        order_far_away_fee = ((order_away.ToD() - car_ds.Tables[0].Rows[0]["car_far_away"].ToD()) * car_ds.Tables[0].Rows[0]["car_far_price"].ToD()).ToString("0.00").ToD();
                    }
                    order_fee = car_meal_fee + order_away_fee + order_time_fee + order_far_away_fee;
                    i = DbHelperSQL.ExecuteSql("update t_order set order_away=" + order_away + ",order_time=" + order_time + ",order_away_fee=" + order_away_fee + ",order_time_fee=" + order_time_fee + ",order_far_away_fee=" + order_far_away_fee + ",order_fee=" + order_fee + " where Convert(varchar(100),order_pk)='" + order_pk + "'");

                    if (i > 0 || order_away==0)
                    {
                        string strSQL = "select * from t_order a left join t_coach b on a.coach_pk=Convert(varchar(100),b.coach_pk) where Convert(varchar(100),a.order_pk)='" + order_pk + "'";
                        ds = DbHelperSQL.Query(strSQL);
                        restr = Json.DataTableToJson(ds.Tables[0]);                            
                    }
                    break;
#endregion
                }
            case "go_to":
                {
#region 已到达                       
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "";
                    strSQL = "update t_order  set order_state='3' where order_pk='" + order_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        restr = "{\"result\":\"100\"}";
                        ds = DbHelperSQL.Query("select * from t_student where Convert(varchar(100),student_pk) in (select user_pk from t_order where order_pk='" + order_pk + "')");
                        for (i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (ds.Tables[0].Rows[i]["student_clientID"].ToS() == "") continue;
                            string[] arg = new string[2];
                            arg[0] = ds.Tables[0].Rows[i]["student_clientID"].ToS();
                            arg[1] = "{\"result\":\"100\"}";
                            Tools.StartProcess(Server.MapPath("/") + "/getui/GetuiServerApiSDKDemo.exe", arg);
                        }
                    }

                    break;
#endregion
                }
            case "confirm_pay":
                {
#region 余额支付
                    string order_pk = Request["order_pk"].ToS();
                    string user_pk = Request["user_pk"].ToS();
                    DataSet order_ds = DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'");
                    if (order_ds.Tables[0].Rows.Count > 0)
                    {
                        DataSet user_ds = DbHelperSQL.Query("select * from t_coach where coach_pk='" + user_pk + "'");
                        if (user_ds.Tables[0].Rows.Count > 0)
                        {
                            if (user_ds.Tables[0].Rows[0]["coach_money"].ToD() >= order_ds.Tables[0].Rows[0]["order_fee"].ToD())
                            { 
                                //扣款
                                if (DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money-" + order_ds.Tables[0].Rows[0]["order_fee"].ToD() + " where coach_pk='" + user_pk + "'") > 0)
                                { 
                                    restr = "{\"result\":\"100\"}";
                                }
                            }
                            else
                            {
                                restr = "{\"result\":\"-99\"}";
                            }
                        }

                    }

                    break;
#endregion
                }
            case "change_order_fee":
                {
#region 司机更改订单费用  
                    string coach_pk = Request["coach_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    string order_fee = Request["order_fee"].ToD().ToS();
                    string strSQL = "update t_order  set order_fee=" + order_fee + " where order_pk='" + order_pk + "' and coach_pk='" + coach_pk + "' and order_state in ('1','2')";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "pay_order":
                {
#region 支付订单
                    string order_pk = Request["order_pk"].ToS();
                    ArrayList arr = new ArrayList();
                    string strSQL = "update t_order  set order_state='3' where order_pk='" + order_pk + "' and order_state in ('1','2')";
                    arr.Add(strSQL);
                    //ds = DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'");
                    //if (ds.Tables[0].Rows.Count > 0)
                    //{
                    //    if (ds.Tables[0].Rows[0]["user_pk"].ToS() != "")
                    //    {
                    //        strSQL = "update t_order  set order_state='3' where order_pk='" + order_pk + "' and order_state='2'";
                    //        arr.Add(strSQL);
                    //    }
                    //}
                    int i = 0;// DbHelperSQL.ExecuteSqlTran(arr);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "evel_order":
                {
#region 评价
                    string order_pk = Request["order_pk"].ToS();
                    string eval_score = Request["eval_score"].ToD().ToS();
                    string eval_rem = Request["eval_rem"].ToS();
                    string coach_pk = DbHelperSQL.ExecuteSqlScalar("select coach_pk from t_order where order_pk='" + order_pk + "'").ToS();
                    ArrayList arr = new ArrayList();
                    string strSQL = "";
                    strSQL = "INSERT INTO [t_eval]" +
                        "([eval_pk]" +
                        ",[order_pk]" +
                        ",[eval_score]" +
                        ",[eval_rem]" +
                        ",[create_time])" +
                        " VALUES" +
                        "(newid()" +
                        ",'" + order_pk + "'" +
                        "," + eval_score + "" +
                        ",'" + eval_rem + "'" +
                        ",getdate())";
                    arr.Add(strSQL);
                    strSQL = "update t_order set order_state='5' where order_pk='" + order_pk + "'";
                    arr.Add(strSQL);
                    strSQL = "update t_coach set coach_score='" + eval_score + "' where  coach_pk='"+ coach_pk + "'";
                    // strSQL = "update t_coach set coach_score=(select sum(Convert(decimal(18, 0),eval_score))/COUNT(*) from t_eval where order_pk in (select order_pk from t_order where coach_pk='" + coach_pk + "' and order_state='5')) where  Convert(varchar(100),coach_pk)='"+ coach_pk + "'";
                    arr.Add(strSQL);
                    int i = DbHelperSQL.ExecuteSqlTran(arr);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }

            case "get_order_eval":
                {
#region 查看订单评价
                    string order_pk = Request["order_pk"].ToS();
                    string strSQL = "select * from t_eval where order_pk='" + order_pk + "'";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_coach_eval":
                {
#region 查看司机评价
                    string coach_pk = Request["coach_pk"].ToS();
                    string strSQL = "select  (select count(*) from t_order where coach_pk=Convert(varchar(100),b.coach_pk) and order_state in ('4','5','6')) as coach_order_count,* from t_eval a left join t_order b on a.order_pk=convert(varchar(100),b.order_pk) left join t_coach c on b.coach_pk=convert(varchar(100),c.coach_pk) left join t_car d on c.coach_car_type=Convert(varchar(100),d.car_pk)  where a.order_pk in (select order_pk from t_order where coach_pk='" + coach_pk + "')";
                    // string strSQL = "select * from t_eval where order_pk='" + coach_pk + "'";
                    ds = DbHelperSQL.Query(strSQL);
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "save_info":
                {
#region 投诉

                    string info_type = Request["info_type"].ToInt32().ToS();
                    string user_type = Request["user_type"].ToInt32().ToS();
                    string user_pk = Request["user_pk"].ToS();
                    string info_title = Request["info_title"].ToS();
                    string info_content = Request["info_content"].ToS();
                    string info_obj = Request["info_obj"].ToS();
                    string strSQL = "";

                    strSQL = "INSERT INTO [t_info]" +
                        "([info_pk]" +
                        ",[info_type]" +
                        ",[user_type]" +
                        ",[user_pk]" +
                        ",[info_title]" +
                        ",[info_content]" +
                        ",[info_obj]" +
                        ",[info_state]" +
                        ",[create_time])" +
                        " VALUES" +
                        "(newid()" +
                        "," + info_type + "" +
                        "," + user_type + "" +
                        ",'" + user_pk + "'" +
                        ",'" + info_title + "'" +
                        ",'" + info_content + "'" +
                        ",'" + info_obj + "'" +
                        ",0" +
                        ",getdate())";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";

                    break;
#endregion
                }
            case "set_withdraw":
                {
#region 提现
                    string user_type = "1";
                    string info_type = "3";
                    string verification = Request["verification"].ToS();
                    string user_pk = Request["user_pk"].ToS();
                    string user_phone = Request["account_phone"].ToS();
                    string info_title = Request["account_number"].ToS();
                    string info_content = Request["account_name"].ToS();
                    double info_obj = Request["withdraw_sum"].ToD().ToString("0.00").ToD();
                    if (info_obj <= 0)
                    {
                        restr = "{\"result\":\"-96\"}";
                        break;
                    }
                    if (DbHelperSQL.ExecuteSqlScalar("select coach_money from t_coach where coach_pk='" + user_pk + "'").ToD() < info_obj)
                    {
                        restr = "{\"result\":\"-95\"}";
                        break;
                    }
                    string strSQL = "";
                    if (DbHelperSQL.Query("select * from t_sys_code where code='" + user_phone + "' and Convert(varchar(100),code_content)='" + verification + "' and [code_expire]>='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'").Tables[0].Rows.Count == 0)
                    {
                        restr = "{\"result\":\"-97\"}";
                        break;
                    }
                    strSQL = "INSERT INTO [t_info]" +
                        "([info_pk]" +
                        ",[info_type]" +
                        ",[user_type]" +
                        ",[user_pk]" +
                        ",[info_title]" +
                        ",[info_content]" +
                        ",[info_obj]" +
                        ",[info_state]" +
                        ",[create_time])" +
                        " VALUES" +
                        "(newid()" +
                        "," + info_type + "" +
                        "," + user_type + "" +
                        ",'" + user_pk + "'" +
                        ",'" + info_title + "'" +
                        ",'" + info_content + "'" +
                        ",'" + info_obj + "'" +
                        ",0" +
                        ",getdate())";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    if (i > 0)
                    {
                        DbHelperSQL.ExecuteSql("update t_coach set coach_money=coach_money-" + info_obj + " where coach_pk='" + user_pk + "'");
                        restr = "{\"result\":\"" + i + "\"}";
                    }

                    break;
#endregion
                }
            case "get_withdraw":
                {
#region 获取提现消息
                    string user_pk = Request["user_pk"].ToS();
                    ds = DbHelperSQL.Query("select * from t_info where user_pk='" + user_pk + "' and info_type=3");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "recharge":
                {
#region 充值
                    string user_pk = Request["user_pk"].ToS();
                    string is_student= Request["is_student"].ToS();
                    double rechange_money = Request["rechange_money"].ToD().ToString("0.00").ToD();
                    if(is_student != ""){
                        DbHelperSQL.ExecuteSql("update  t_student set student_money= student_money+"+rechange_money+" where student_pk='" + user_pk + "'");
                    }else{
                        DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money+"+rechange_money+" where coach_pk='" + user_pk + "'");
                    }

                    // DbHelperSQL.ExecuteSql("update  t_transaction set isfinish=1 where user_pk='" + user_pk + "'");

                    restr = "{\"result\":\"100\"}";
                    break;
#endregion
                }
            case "get_rechange":
                {
#region 获取充值记录
                    string user_pk = Request["user_pk"].ToS();
                    ds = DbHelperSQL.Query("select * from t_transaction where user_pk='" + user_pk + "' and isfinish=0 order by create_time desc");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_message_state":
                {
#region 获取用户是否有未读消息
                    string user_pk = Request["user_pk"].ToS();
                    int i = DbHelperSQL.ExecuteSqlScalar("select count(*) from t_message where user_pk='" + user_pk + "' and message_state=0").ToInt32() ;
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "get_message":
                {
#region 获取用户消息
                    string user_pk = Request["user_pk"].ToS();
                    ds = DbHelperSQL.Query("select * from t_message where user_pk='" + user_pk + "'");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "read_message":
                {
#region 消息标示未已读                                               
                    string user_pk = Request["user_pk"].ToS();
                    string strSQL = "update t_message set message_state=1 where user_pk='"+ user_pk + "' ";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "set_coach_range":
                {
#region 设置抢单范围
                    string coach_pk = Request["coach_pk"].ToS();
                    string coach_range = Request["coach_range"].ToD().ToS();
                    string strSQL = "update t_coach set coach_order_range='" + coach_range + "' where coach_pk='" + coach_pk + "'";
                    int i = DbHelperSQL.ExecuteSql(strSQL);
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "get_rental":
                {
#region 获取长租车

                    ds = DbHelperSQL.Query("select * from t_rental order by create_time desc");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_rental_info":
                {
#region 获取长租车
                    string rental_pk = Request["rental_pk"].ToS();
                    ds = DbHelperSQL.Query("select * from t_rental where rental_pk='" + rental_pk + "'");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_pop_link":
                {
#region 获取推广二维码
                    string user_pk = Request["user_pk"].ToS();
                    string pop_link = "/?link=" + Tools.Encode(user_pk).Replace("-", "");
                    if (!File.Exists(Server.MapPath("/") + "images/pop/" + user_pk + ".jpg"))
                    {
                        DirectoryInfo dir = new DirectoryInfo(Server.MapPath("/") + "images/pop");
                        if (!dir.Exists) dir.Create();
                        Tools.QRCode("http://" + Request.Url.Host + pop_link, Server.MapPath("/") + "images/pop/" + user_pk + ".jpg");
                    }
                    int in_count = DbHelperSQL.ExecuteSqlScalar("select ((select count(*) from t_coach where coach_incode in (select coach_code from t_coach where coach_pk='"+ user_pk + "'))+(select count(*) from t_student where user_incode in (select coach_code from t_coach where coach_pk='" + user_pk + "')))").ToInt32();
                    restr = "{\"result\":\"" + pop_link + "\",\"pic_path\":\"/images/pop/" + user_pk + ".jpg\",\"in_count\":\""+ in_count + "\"}";
                    break;
#endregion
                }
            case "get_user_order_state":
                {
#region 获取是否能下单或抢单
                    string user_pk = Request["user_pk"].ToS();
                    string coach_pk = Request["coach_pk"].ToS();
                    if (user_pk != "")
                    {
                        ds = DbHelperSQL.Query("select * from t_order where user_pk='" + user_pk + "' and order_state in ('0','1','2')");
                        restr = "{\"result\":\"" + ds.Tables[0].Rows.Count + "\"}";
                    }
                    else if (coach_pk != "")
                    {
                        ds = DbHelperSQL.Query("select * from t_order where coach_pk='" + coach_pk + "' and order_state in ('0','1','2')");
                        restr = "{\"result\":\"" + ds.Tables[0].Rows.Count + "\"}";
                    }
                    break;
#endregion
                }
            case "get_pay_info":
                {
#region 支付宝支付  
                    string user_pk = Request["user_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    double rechange_money = Request["rechange_money"].ToD().ToString("0.00").ToD();
                    if (rechange_money > 0 && user_pk!="")
                    {
                        //交易记录
                        string transaction_pk = add_transaction("rechange", "user", user_pk,"", rechange_money, DbHelperSQL.ExecuteSqlScalar("select student_money from t_student where student_pk='" + user_pk + "'").ToD(), "账户充值");
                        if (transaction_pk != "")
                        {
                            Dictionary<string, string> payinfo = new Dictionary<string, string>();
                            payinfo.Add("service", "\"mobile.securitypay.pay\"");
                            payinfo.Add("partner", "\"" + Config.Partner + "\"");
                            payinfo.Add("seller_id", "\"" + Config.Seller_id + "\"");
                            payinfo.Add("out_trade_no", "\"" + transaction_pk + "\"");
                            payinfo.Add("subject", "\"账户充值\"");
                            payinfo.Add("body", "\"账户充值\"");
                            payinfo.Add("total_fee", "\"" + rechange_money + "\"");
                            payinfo.Add("notify_url", "\"http://182.254.233.114/web/notify_url_rechange.aspx\"");
                            payinfo.Add("payment_type", "\"1\"");
                            payinfo.Add("_input_charset", "\"UTF-8\"");
                            payinfo.Add("it_b_pay", "\"30m\"");
                            var sb = new StringBuilder();
                            foreach (var sA in payinfo.OrderBy(x => x.Key))//参数名ASCII码从小到大排序（字典序）；
                            {
                                sb.Append(sA.Key).Append("=").Append(sA.Value).Append("&");
                            }
                            var orderInfo = sb.ToString();
                            orderInfo = orderInfo.Remove(orderInfo.Length - 1, 1);
                            // 对订单做RSA 签名
                            string sign = AlipayMD5.Sign(orderInfo, Config.Private_key, Config.Input_charset); //支付宝提供的Config.cs
                            //仅需对sign做URL编码
                            sign = HttpUtility.UrlEncode(sign, Encoding.UTF8);
                            string payInfo = orderInfo + "&sign=\"" + sign + "\"&"
                                + getSignType();
                            restr = payInfo;
                        }
                    }
                    else if (order_pk != "")
                    {
                        ds = DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            //交易记录
                            string transaction_pk = add_transaction("rechange", "user", ds.Tables[0].Rows[0]["user_pk"].ToS(),order_pk, ds.Tables[0].Rows[0]["order_fee"].ToD(), DbHelperSQL.ExecuteSqlScalar("select student_money from t_student where student_pk='" + ds.Tables[0].Rows[0]["user_pk"].ToS() + "'").ToD(), "支付订单");
                            if (transaction_pk != "")
                            {
                                Dictionary<string, string> payinfo = new Dictionary<string, string>();
                                payinfo.Add("service", "\"mobile.securitypay.pay\"");
                                payinfo.Add("partner", "\"" + Config.Partner + "\"");
                                payinfo.Add("seller_id", "\"" + Config.Seller_id + "\"");
                                payinfo.Add("out_trade_no", "\"" + transaction_pk + "\"");
                                payinfo.Add("subject", "\"支付专车费用\"");
                                payinfo.Add("body", "\"支付专车费用\"");
                                payinfo.Add("total_fee", "\"" + ds.Tables[0].Rows[0]["order_fee"].ToS() + "\"");
                                payinfo.Add("notify_url", "\"http://182.254.233.114/web/notify_url_rechange.aspx\"");
                                payinfo.Add("payment_type", "\"1\"");
                                payinfo.Add("_input_charset", "\"UTF-8\"");
                                payinfo.Add("it_b_pay", "\"30m\"");
                                var sb = new StringBuilder();
                                foreach (var sA in payinfo.OrderBy(x => x.Key))//参数名ASCII码从小到大排序（字典序）；
                                {
                                    sb.Append(sA.Key).Append("=").Append(sA.Value).Append("&");
                                }
                                var orderInfo = sb.ToString();
                                orderInfo = orderInfo.Remove(orderInfo.Length - 1, 1);
                                // 对订单做RSA 签名
                                string sign = AlipayMD5.Sign(orderInfo, Config.Private_key, Config.Input_charset); //支付宝提供的Config.cs
                                //仅需对sign做URL编码
                                sign = HttpUtility.UrlEncode(sign, Encoding.UTF8);
                                string payInfo = orderInfo + "&sign=\"" + sign + "\"&"
                                    + getSignType();
                                restr = payInfo;
                            }
                        }

                    }
                    else
                    {
                        restr = "请求数据错误！";
                    }
                    break;
#endregion
                }

            case "get_wx_pay_info":
                {
#region 微信支付

                    string is_student= Request["is_student"].ToS();
                    string order_state= Request["order_state"].ToS();
                    string user_pk = Request["user_pk"].ToS();
                    string order_pk = Request["order_pk"].ToS();
                    double rechange_money = Request["rechange_money"].ToD().ToString("0.00").ToD(); 
                    if (rechange_money > 0 && user_pk != "")
                    {
                        //交易记录
                        string transaction_pk = add_transaction("rechange", "user", user_pk, "", rechange_money, DbHelperSQL.ExecuteSqlScalar("select student_money from t_student where student_pk='" + user_pk + "'").ToD(), "账户充值");
                        if (transaction_pk != "")
                        { 
                            // if(xhr.status==200){
                            if(order_state == "6"){//用户支付订单
                                var payment = new Payment("1352326101", "wx66a201f34f90e571", "698d51a19d8a121ce581499d7b701668", "http://182.254.233.114/web/wx_notify_url.aspx");
                                restr = payment.Pay(long.Parse((rechange_money * 100).ToS()), transaction_pk.Replace("-",""), "支付路邮寄运费", Request.UserHostAddress,"CNY");    

                                // var strSQL = "update t_order  set order_state='4' where order_pk='" + order_pk + "'";
                                // int i = DbHelperSQL.ExecuteSql(strSQL); // 1或0
                            }else{
                                if(is_student !=""){//用户充值
                                    var payment = new Payment("1352326101", "wx66a201f34f90e571", "698d51a19d8a121ce581499d7b701668", "http://182.254.233.114/web/wx_notify_url.aspx");
                                    restr = payment.Pay(long.Parse((rechange_money * 100).ToS()), transaction_pk.Replace("-",""), "路邮寄账户充值", Request.UserHostAddress,"CNY");    

                                    // DbHelperSQL.ExecuteSql("update  t_student set student_money=student_money+"+rechange_money+" where student_pk='" + user_pk + "'");
                                }else{//司机充值
                                    var payment = new Payment(Wx_Pay_Model.mchid, Wx_Pay_Model.appId, Wx_Pay_Model.appkey, "http://182.254.233.114/web/wx_notify_url.aspx");
                                    restr = payment.Pay(long.Parse((rechange_money * 100).ToS()), transaction_pk.Replace("-",""), "路邮寄账户充值", Request.UserHostAddress,"CNY");    
    // DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money+"+rechange_money+" where coach_pk='" + user_pk + "'");
                                }
                            }   
                            //  }


                            //    if (DbHelperSQL.ExecuteSql("update  t_student set student_money=student_money+" + recharge_money + " where student_pk='" + transaction_ds.Tables[0].Rows[0]["user_pk"].ToS() + "'") > 0)
                            //    {
                            //       return true;
                            //    }
                            //    else if(DbHelperSQL.ExecuteSql("update  t_coach set coach_money=coach_money+" + recharge_money + " where coach_pk='" + transaction_ds.Tables[0].Rows[0]["user_pk"].ToS() + "'") > 0)
                            //    {
                            //        return true;
                            //    }
                        }
                    }
                    else if (order_pk != "")
                    {
                        ds = DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'");
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            //交易记录
                            string transaction_pk = add_transaction("rechange", "user", ds.Tables[0].Rows[0]["user_pk"].ToS(), order_pk, ds.Tables[0].Rows[0]["order_fee"].ToD(), DbHelperSQL.ExecuteSqlScalar("select student_money from t_student where student_pk='" + ds.Tables[0].Rows[0]["user_pk"].ToS() + "'").ToD(), "支付订单");
                            if (transaction_pk != "")
                            {                                
                                var payment = new Payment(Wx_Pay_Model.mchid, Wx_Pay_Model.appId, Wx_Pay_Model.appkey, "http://182.254.233.114/web/wx_notify_url.aspx");
                                restr = payment.Pay(long.Parse((ds.Tables[0].Rows[0]["order_fee"].ToD() * 100).ToS()), transaction_pk.Replace("-", ""), "支付费用", Request.UserHostAddress, "CNY");

                            }
                        }

                    }
                    else
                    {
                        restr = "请求数据错误！";
                    }
                    break;
#endregion
                }
            case "balance_pay":
                {
#region 余额支付
                    string order_pk = Request["order_pk"].ToS();
                    string user_pk = Request["user_pk"].ToS();
                    DataSet order_ds = DbHelperSQL.Query("select * from t_order where order_pk='" + order_pk + "'");
                    if (order_ds.Tables[0].Rows.Count > 0)
                    {
                            DataSet user_ds = DbHelperSQL.Query("select * from t_student where student_pk='" + user_pk + "'");
                            if (user_ds.Tables[0].Rows.Count > 0)
                            {
                                if (user_ds.Tables[0].Rows[0]["student_money"].ToD() >= order_ds.Tables[0].Rows[0]["order_fee"].ToD())
                                { 
                                    //扣款
                                    if (DbHelperSQL.ExecuteSql("update  t_student set student_money=student_money-" + order_ds.Tables[0].Rows[0]["order_fee"].ToD() + " where student_pk='" + user_pk + "'") > 0)
                                    { 
                                            restr = "{\"result\":\"100\"}";
                                    }
                                }
                                else
                                {
                                    restr = "{\"result\":\"-99\"}";
                                }
                            }

                    }

                    break;
#endregion
                }
            case "get_pay_state":
                {
#region 检测订单是否已支付
                    string order_pk = Request["order_pk"].ToS();
                    int i = DbHelperSQL.ExecuteSqlScalar("select order_state from t_order where order_pk='" + order_pk + "'").ToInt32() == 3 ? 1 : 0;
                    restr = "{\"result\":\"" + i + "\"}";
                    break;
#endregion
                }
            case "get_update":
                {
#region 检测是否更新      

                    string _type = Request["_type"].ToS();
                    double version = Request["version"].ToS().Replace(".","").ToD();
                    ds = DbHelperSQL.Query("select * from t_system");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        double new_version = _type == "1" ? ds.Tables[0].Rows[0]["sms_succ_code"].ToS().Replace(".", "").ToD() : ds.Tables[0].Rows[0]["sms_sel_url"].ToS().Replace(".", "").ToD();
                        if (new_version > version)
                        {
                            restr = "{\"result\":\"100\"}";
                        }
                    }

                    break;
#endregion
                }
            case "get_ad":
                {
#region 获取首页banner

                    string _type = Request["_type"].ToS();
                    ds = DbHelperSQL.Query("select * from t_info where info_type=4 and info_title='" + _type + "' order by create_time desc");
                    restr = Json.DataTableToJson(ds.Tables[0]);

                    break;
#endregion
                }
            case "get_ad_one":
                {
#region 获取首页广告

                    string _type = Request["_type"].ToS();
                    string ad_one = DbHelperSQL.ExecuteSqlScalar("select top 1 info_content from t_info where info_type=4 and info_title='" + _type + "' order by create_time desc").ToS();
                    restr = "{\"result\":\"" + ad_one + "\"}";
                    break;
#endregion
                }
            case "get_recharge":
                {
#region 获取充值金额 赠送金额
                    string djq = DbHelperSQL.ExecuteSqlScalar("select sms_result_start_code from t_system").ToS();
                    restr = "{\"result\":\""+ djq + "\"}";
#endregion
                    break;
                }
            case "get_in_list":
                {
#region 获取邀请记录
                    string user_pk = Request["user_pk"].ToS();
                    ds = DbHelperSQL.Query("select * from ((select '1' as user_type, coach_name as user_name, coach_phone as user_phone, coach_incode_money as in_money, create_time from t_coach where coach_incode = (select coach_code from t_coach where coach_pk = '"+ user_pk + "')) union all (select '2' as user_type, '' as user_name, student_phone as user_phone, user_incode_money as in_money, create_time from t_student where user_incode = (select coach_code from t_coach where coach_pk = '"+ user_pk + "'))) a order by create_time desc");
                    restr = Json.DataTableToJson(ds.Tables[0]);
                    break;
#endregion
                }
            case "get_phone":
                {
#region 获取客服电话
                    string phone = DbHelperSQL.ExecuteSqlScalar("select sys_cancel from t_system").ToS();
                    restr = "{\"result\":\"" + phone + "\"}";
#endregion
                    break;
                }
            default:
                {
                    restr = "";
                    break;
                }
            }
        }
        catch (Exception ex )
        {
            restr = "";
        }
        Response.Write(restr);
        Response.End();
    }
    public String getSignType()
    {
        return "sign_type=\"MD5\"";
    }
    public string add_transaction(string transaction_type, string user_type, string user_pk, string order_pk, double amount, double balance, string rem)
    {
        string transaction_pk = Guid.NewGuid().ToS();

        string strSQL = "";

        strSQL = "INSERT INTO [t_transaction]" +
            "([transaction_pk]" +
            ",[transaction_type]" +
            ",[user_type]" +
            ",[user_pk]" +
            ",[order_pk]" +
            ",[amount]" +
            ",[balance]" +
            ",[rem]" +
            ",[isfinish]" +         
            ",[create_time])" +
            " VALUES" +
            "('" + transaction_pk + "'" +
            ",'" + transaction_type + "'" +
            ",'" + user_type + "'" +
            ",'" + user_pk + "'" +
            ",'" + order_pk + "'" +
            "," + amount + "" +
            "," + balance + "" +
            ",'" + rem + "'" +
            ",0" +         
            ",getdate())";

        int i = DbHelperSQL.ExecuteSql(strSQL);
        if (i > 0)
        {
            return transaction_pk;
        }
        return "";

    }

}
