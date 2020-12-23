using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LaoHua
{
    /// <summary>
    /// 服务器返回消息
    /// </summary>
    public class ClientResponseMsg
    {
        public bool IsSuccess { get; set; }
        public string Messaage { get; set; }
        public object Data { get; set; }
        public int totalRows { get; set; }
    }
    
    /// <summary>
    /// 上传工单良品数不良品数入口
    /// </summary>
    public class UploadOrderDetailMsg
    {
        public String DBPath { get; set; }
        public String URL { get; set; }
        public String DeviceNo { get; set; }

        public string Start()
        {
            return UploadOrderDetail();
        }

        /// <summary>
        /// 上传工单数据
        /// </summary>
        public string UploadOrderDetail()
        {
            Stopwatch getTime = new Stopwatch();
            getTime.Start();
            string filePath = this.DBPath;//文件路径
            string filename = FileUtil.FindFileNameByTimeDESC(filePath);//最近修改的文件文件名
            string filefullname = filePath+ filename;//原文件全路径
            string filefullname_ = "D:/dbtemp/" + filename;//待读取的文件全路径，temp目录下的同名文件
            string url = this.URL;//MES Contorller地址 LaoHuaDevice UploadLaoHuaOrderDetailMsg
            string DeviceNo = this.DeviceNo;//机台号
            string sql = "select good,defective,ESR,quality_gr_count,quality_dr_count,quality_cldl_count,quality_ldbl_count,quality_ss_count,quality_wlhbl_count,quality_cx_count from (select count(ID) as good from MyDataTable where pd = '良品'), (select count(ID) as defective from MyDataTable where pd <> '良品'), (select count(ID) as ESR from MyDataTable where pd = 'ESR'), (select count(ID) as quality_gr_count from MyDataTable where pd = '高容'), (select count(ID) as quality_dr_count from MyDataTable where pd = '低容'), (select count(ID) as quality_cldl_count from MyDataTable where pd = '出料短路'), (select count(ID) as quality_ldbl_count from MyDataTable where pd = '漏电不良'), (select count(ID) as quality_ss_count from MyDataTable where pd = '损失'),(select count(ID) as quality_wlhbl_count from MyDataTable where pd = '未老化不良'), (select count(ID) as quality_cx_count from MyDataTable where pd = '重选')";//sqlite查询sql
            string returnMsg = string.Empty;
            DataTable dt = new DataTable();
            JObject reqData = new JObject();

            FileUtil.CheckPath(filefullname);
            if (FileUtil.CheckPath(filefullname) ==false)
            {
                getTime.Stop();
                return "Directory'"+ filefullname + "' is Empty";
            }
            
            FileUtil.copyNewFileToTemp(filePath);

            dt = SqlLiteUtil.GetDataTable(sql, filefullname_);
            if(dt == null||dt.Rows.Count == 0)
            {
                getTime.Stop();
                return "'" + filefullname_ + "' select fail or file is empty";
            }

            if(dt.Rows[0]["good"].ToString() != null && dt.Rows[0]["good"].ToString() != "" && dt.Rows[0]["defective"].ToString() != null && dt.Rows[0]["defective"].ToString() != "")
            {
                reqData.Add("OrderDetail", Path.GetFileNameWithoutExtension(filefullname_));
                reqData.Add("OrderDetailGoodNum", dt.Rows[0]["good"].ToString());
                reqData.Add("OrderDetailDefective", dt.Rows[0]["defective"].ToString());
                reqData.Add("ESR", dt.Rows[0]["ESR"].ToString());
                reqData.Add("quality_gr_count", dt.Rows[0]["quality_gr_count"].ToString());//高容
                reqData.Add("quality_dr_count", dt.Rows[0]["quality_dr_count"].ToString());//低容
                reqData.Add("quality_cldl_count", dt.Rows[0]["quality_cldl_count"].ToString());//出料短路
                reqData.Add("quality_ldbl_count", dt.Rows[0]["quality_ldbl_count"].ToString());//漏电不良
                reqData.Add("quality_ss_count", dt.Rows[0]["quality_ss_count"].ToString());//损失
                reqData.Add("quality_wlhbl_count", dt.Rows[0]["quality_wlhbl_count"].ToString());//未老化不良
                reqData.Add("quality_cx_count", dt.Rows[0]["quality_cx_count"].ToString());//重选
                reqData.Add("DeviceNo", DeviceNo);
            }
            else
            {
                getTime.Stop();
                return sql+":Exception in SQL execution result";
            }

            string body = JsonConvert.SerializeObject(reqData);
            returnMsg = RequestUtil.ClientRequest(url, body);
            ClientResponseMsg msgObj = JsonConvert.DeserializeObject<ClientResponseMsg>(returnMsg);

            if (!msgObj.IsSuccess)
            {
                getTime.Stop();
                return filefullname_ + "OrderDetail Upload fail: 良品" + dt.Rows[0]["good"].ToString() + ",不良品" + dt.Rows[0]["defective"].ToString()+ ",ESR" + dt.Rows[0]["ESR"].ToString() + ",高容" + dt.Rows[0]["quality_gr_count"].ToString() + ",低容" + dt.Rows[0]["quality_dr_count"].ToString() + ",出料短路" + dt.Rows[0]["quality_cldl_count"].ToString() + ",漏电不良" + dt.Rows[0]["quality_ldbl_count"].ToString() + ",损失" + dt.Rows[0]["quality_ss_count"].ToString() + ",未老化不良" + dt.Rows[0]["quality_wlhbl_count"].ToString() + ",重选" + dt.Rows[0]["quality_cx_count"].ToString() + ",总耗时:" + getTime.ElapsedMilliseconds.ToString();
            }
            else
            {
                getTime.Stop();
                return filefullname_ + "OrderDetail Upload success: 良品" + dt.Rows[0]["good"].ToString() + ",不良品" + dt.Rows[0]["defective"].ToString() + ",ESR" + dt.Rows[0]["ESR"].ToString()+ ",高容" + dt.Rows[0]["quality_gr_count"].ToString() + ",低容" + dt.Rows[0]["quality_dr_count"].ToString() + ",出料短路" + dt.Rows[0]["quality_cldl_count"].ToString() + ",漏电不良" + dt.Rows[0]["quality_ldbl_count"].ToString() + ",损失" + dt.Rows[0]["quality_ss_count"].ToString() + ",未老化不良" + dt.Rows[0]["quality_wlhbl_count"].ToString() + ",重选" + dt.Rows[0]["quality_cx_count"].ToString() + ",总耗时:" +getTime.ElapsedMilliseconds.ToString();
            }
        }
    }

    /// <summary>
    /// 上传生产明细入口
    /// </summary>
    public class UploadProductionDetailed
    {
        public String URL { get; set; }
        public String DBPath { get; set; }
        public String DeviceNo { get; set; }
        public String TableName { get; set; }
        public String savePath { get; set; }

        public string Start()
        {
            return UploadProductions();
        }

        /// <summary>
        /// 上传生产数据
        /// </summary>
        /// <returns></returns>
        public string UploadProductions()
        {
            string url = this.URL;//生产数据处理controller地址
            string path = this.DBPath;//生产数据存放目录
            //将目录下的所有文件复制到temp目录
            FileUtil.copyFileToTemp(path);

            DirectoryInfo folder = new DirectoryInfo(path);

            string maxkey = FileUtil.FindFileNameByTimeDESC(path);

            //上传除最近修改的文件的所有文件
            foreach (FileInfo file in folder.GetFiles("*.db"))
            {
                if ((Path.GetFileNameWithoutExtension(file.ToString()).Split('-').Length - 1) != 3)
                {
                    continue;
                }
                string filepath = file.FullName;
                string filename = Path.GetFileName(file.ToString());
                if (maxkey == filename)
                {
                    continue;
                }
                if (RequestUtil.HttpUploadFile(url, filepath, filename, new String[] { "source", "sortCol", "tableName", "savePath" }, new String[] { this.DeviceNo, null, this.TableName, this.savePath }) == "Success")
                {
                    File.Delete(filepath);//上传成功后删除文件
                }
                else
                {
                    continue;
                }
            }
            string filepath_ = path + "temp/" + maxkey;
            //上传 temp目录下,最近修改的文件
            if (RequestUtil.HttpUploadFile(url, filepath_, maxkey, new String[] { "source", "sortCol", "tableName", "savePath" }, new String[] { this.DeviceNo, null, this.TableName, this.savePath }) != "Success")
            {
                return "Upload '" + filepath_ + "' fail";
            }
            return "Upload success";
        }

    }

    /// <summary>
    /// 客户端请求
    /// </summary>
    public class RequestUtil
    {
        /// <summary>
        /// 客户端请求数据
        /// </summary>
        public static string ClientRequest(string actionUrl, string jsonBody)
        {
            string returnMsg = string.Empty;
            ClientResponseMsg respMsg = new ClientResponseMsg();
            returnMsg = JsonConvert.SerializeObject(respMsg);
            if (!string.IsNullOrEmpty(jsonBody))
            {
                returnMsg = HttpPost(actionUrl, jsonBody.ToString(), "application/json", "POST", "");
            }
            return returnMsg;
        }

        #region POST
        public static string HttpPost(string url, string body, string contentType = "application/json", string Method = "POST", string bearerToken = "")
        {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = Method;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + bearerToken);
            }
            httpWebRequest.Timeout = 200000;

            if (Method == "POST" || Method == "PUT")
            {
                byte[] btBodys = Encoding.UTF8.GetBytes(body);
                httpWebRequest.ContentLength = btBodys.Length;
                httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
            }

            string responseContent = string.Empty;

            try
            {
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (Stream zipstream = httpWebResponse.GetResponseStream())
                    {
                        if (zipstream != null)
                        {
                            using (StreamReader reader = new StreamReader(zipstream, System.Text.Encoding.GetEncoding("utf-8")))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }
                    }

                }
                httpWebRequest.Abort();
            }
            catch (WebException ex)
            {
                var httpRspn = (HttpWebResponse)ex.Response;
                throw ex;
            }
            return responseContent;
        }
        #endregion

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="fs">文件流</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string HttpUploadFile(string url, String path, string fileName, String[] keys, String[] values)
        {
            using (var client = new HttpClient())
            {
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    int len = keys.Length;
                    for (int i = 0; i < len; i++)
                    {
                        client.DefaultRequestHeaders.Add(keys[i], values[i]); // 存在HEAD里面
                    }
                    multipartFormDataContent.Add(new StreamContent(File.Open(path, FileMode.Open)), "file", fileName);
                    Task<HttpResponseMessage> message = client.PostAsync(url, multipartFormDataContent);
                    message.Wait();
                    MemoryStream ms = new MemoryStream();
                    Task t = message.Result.Content.CopyToAsync(ms);
                    t.Wait();
                    ms.Position = 0;
                    StreamReader sr = new StreamReader(ms, Encoding.UTF8);
                    string content = sr.ReadToEnd();
                    return content;
                }
            }
        }
    }

    /// <summary>
    /// 文件操作
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        /// 复制文件到文件夹
        /// </summary>
        /// <param name="path">最终文件路径</param>
        /// <param name="fileName">指定文件的完整路径</param>
        public static void saveFile(string path, string fileName)
        {
            if (File.Exists(fileName))//必须判断要复制的文件是否存在
            {
                File.Copy(fileName, path, true);//三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <returns></returns>
        public static Boolean CheckPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将文件夹下的所有文件copy到当前文件夹下的temp目录
        /// </summary>
        /// <param name="filepath">文件夹路径</param>
        public static void copyFileToTemp(string filepath)
        {
            //创建文件夹tmp
            DirectoryInfo di = Directory.CreateDirectory(filepath + "//temp");
            DirectoryInfo folder = new DirectoryInfo(filepath);

            foreach (FileInfo file in folder.GetFiles("*.db"))
            {
                string name = file.FullName;//带路径的名称
                string filename = Path.GetFileName(file.ToString());
                saveFile(filepath + "temp/" + filename, name);
            }
        }

        /// <summary>
        /// 将文件夹下的最新修改的文件copy到D盘dbtem目录下
        /// </summary>
        /// <param name="filepath">文件夹路径</param>
        public static void copyNewFileToTemp(string filepath)
        {
            //创建文件夹tmp
            DirectoryInfo di = Directory.CreateDirectory("D://dbtemp");
            DirectoryInfo folder = new DirectoryInfo(filepath);
            saveFile("D:/dbtemp/" + FindFileNameByTimeDESC(filepath), filepath+FindFileNameByTimeDESC(filepath));
        }

        /// <summary>
        /// 获取XML文件UploadConfig.xml，001下指定节点下的值
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        public static string GetXML(string sourceName)
        {
            string xmlFileName = "UploadConfig.xml";
            string result = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);
            //获取到xml文件的根节点
            XmlElement nodeRoot = xmlDoc.DocumentElement;

            //UploadConfig.xml，查看指定节点的值
            XmlNodeList uploadConfig = nodeRoot.SelectNodes("*");
            foreach (XmlNode config in uploadConfig)
            {
                if (config.FirstChild.InnerText.Equals("001"))
                {
                    XmlNodeList configChidNodeList = config.SelectNodes("*");
                    foreach (XmlNode configChileNode in configChidNodeList)
                    {
                        if (configChileNode.Name.Equals(sourceName))
                        {
                            result = configChileNode.InnerText + "";
                        }
                    }
                    Console.WriteLine();
                    break;
                }
                else
                {
                    continue;
                }
            }
            return result;
        }

        /// <summary>
        /// 查找路径下最近修改的.db文件(仅包含三个-的文件)文件名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FindFileNameByTimeDESC(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);

            //找到CSV目录下最近修改的文件文件名
            Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
            foreach (FileInfo file in folder.GetFiles("*.db"))
            {
                if ((Path.GetFileNameWithoutExtension(file.ToString()).Split('-').Length - 1) != 3)
                {
                    continue;
                }
                string filename = Path.GetFileName(file.ToString());//仅文件名
                DateTime updateTime = file.LastWriteTime;
                dict.Add(filename, updateTime);
            }
            if (dict.Keys.Count == 0)
            {
                return null;
            }
            return dict.Keys.Select(x => new { x, y = dict[x] }).OrderByDescending(x => x.y).First().x;
        }
    }

    /// <summary>
    /// Sqlite操作
    /// </summary>
    public class SqlLiteUtil
    {
        /// <summary>
        /// 根据查询sql与db文件路径返回datatable
        /// </summary>
        /// <param name="strSQL">需传入的sql  "SELECT * FROM item_compound;" 数据表名为"item_compound"</param>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string strSQL, string path)
        {
            DataTable dt = null;
            string DBPath = string.Format(@"Data Source={0};", path);
            try
            {
                SQLiteConnection conn = new SQLiteConnection(DBPath);
                SQLiteCommand cmd = new SQLiteCommand(strSQL, conn);
                SQLiteDataAdapter reciever = new SQLiteDataAdapter(cmd);
                dt = new DataTable();
                reciever.Fill(dt);
                conn.Close();
                return dt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
    }

}
