using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Resources;
using System.Timers;

namespace CLARK_INFINITI_UI___TAM_SUNTER_AGV
{
    public partial class Form1 : Form
    {

        System.Drawing.Point location = System.Drawing.Point.Empty;
        private System.Drawing.Point _mouseLoc;
        public List<AGVCallingModel> AGVData = new List<AGVCallingModel>();
        public List<AGVStatusModel> AGVStatus = new List<AGVStatusModel>();
        public List<AGVErrorModel> AGVError = new List<AGVErrorModel>();
        public string url = "http://192.168.1.17:8000/req";
        //public string url = "http://172.16.101.203:8000/req";
        public long jobId, readPower, cntTimer = 0, missionId , jobId1, jobId2,jobNumber;
        public string[] arrayStation = new string[] { "'LINE_1'", "'LINE_2'", "'LINE_3'", "'LINE_4'", "'LINE_5'", "'LINE_6'", "'LINE_7'" };
        //public string[] arrayStation = new string[] { "'SMD_01'", "'SMD_02'", "'SMD_03'", "'SMD_04'", "'SMD_05'", "'SMD_06'", "'SMD_07'" };

        public int[] arrayCall = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public bool writeFlag = false, rt2 = false, endFlag = false;
        public int xAgv, yAgv, interval = 5, cntStop = 0, cntStop4 = 0, counterLog, cntMove = 0, cntCall1, cntCall2;
        public int testButton = 0, agvAddress = 1, waitingTime = 0, collectData = 0, cancelJob;
        public string agvState, agvName = "AGV-1", agvTime, agv1Status, agvRoute, agvRfid, statusDelivery, statusDelivery1, statusDelivery2, line1, line2, obsCode, destLine;
        public bool line1Click = false, line2Click = false, testState=false;
        public bool c1 = false, c2 = false, cancel1 = false, cancel2 = false, jobFinish = true, jobFinish2 = true;
        private static string m_exePath = string.Empty;
        public double dataRfid, dataRfid_ = 0;
        public string jobStatus, status2;
        private void cancelButton_Click_1(object sender, EventArgs e)
        {
            jobFinish = !jobFinish;
            line1Click = false;
            saveJobNumber1 = 0;
        }

        private void cancelButton2_Click_1(object sender, EventArgs e)
        {
            jobFinish2 = !jobFinish2;
            line2Click = false;
            saveJobNumber2 = 0;
        }

        private void cancelCall_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void bunifuButton1_Click_1(object sender, EventArgs e)
        {
            //tbCancelCall.Clear();
            MessageBox.Show("MISSION GIVE UP");
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseLoc = e.Location;
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - _mouseLoc.X;
                int dy = e.Location.Y - _mouseLoc.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }
        public long saveJobNumber1, saveJobNumber2;
        private void btnStation1_Click(object sender, EventArgs e)
        {
            saveJobNumber1 = jobNumber;
            Disini("91", saveJobNumber1.ToString(), "");
            destLine = arrayStation[0];
            line1Click = true;
            cntCall1 +=1;
            if (line1Click)
            {
                btnStation1.ActiveFillColor = Color.Red;
                btnStation1.IdleFillColor = Color.Red;
                btnStation1.ButtonText = "GO TO \n STATION 1";
                c1 = !c1;
            }
            else { }
        }
        private void btnStation2_Click(object sender, EventArgs e)
        {
            saveJobNumber2 = jobNumber;
            Disini("107", saveJobNumber2.ToString(), "");
            destLine = arrayStation[1];
            line2Click = true;
            cntCall2 += 1;
            if (line2Click )
            {
                btnStation2.ActiveFillColor = Color.Red;
                btnStation2.IdleFillColor = Color.Red;
                btnStation2.ButtonText = "GO TO  \n STATION 2";
                c2 = !c2;
            }
            else { }
        }
        class RequestData
        {
            public string command { get; set; }
            public int serialNumber { get; set; }
        }
        class ResponseData

        {
            public string errMark { get; set; }
            public List<List<dynamic>> msg { get; set; }        // add command checker
            public string command { get; set; }
        }
        class ResponseData2
        {
            public string errMark { get; set; }
            public List<dynamic> msg { get; set; }        // add command checker
            public string command { get; set; }
        }
        class CallingData
        {
            public string errMark { get; set; }
            public dynamic msg { get; set; }        // add command checker
            public string command { get; set; }
        }
        public class AGVDeviceModel
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public AGVDeviceModel(string agvId, string agvName, string status)
            {
                this.ID = agvId;
                this.Name = agvName;
                this.Status = status;
            }
        }
        public class AGVStatusModel
        {
            //public string ID { get; set; }
            public string Battery { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public string Status { get; set; }
            public AGVStatusModel(string battery, string agvName, string power, string status)
            {
                this.Battery = battery;
                this.Name = agvName;
                this.State = power;
                this.Status = status;
            }
        }
        public class AGVCallingModel
        {
            public string Time { get; set; }
            public long JobId { get; set; }
            public string Name { get; set; }
            public string Station { get; set; }
            public string Status { get; set; }

            public AGVCallingModel(long jobid, string time, string agvname, string deliv, string status)
            {
                this.Time = time;
                this.JobId = jobid;
                this.Name = agvname;
                this.Station = deliv;
                this.Status = status;
            }
        }
        public class AGVErrorModel
        {
            public string Time { get; set; }
            public string Name { get; set; }
            public string Error { get; set; }
            public string Obstacle { get; set; }
            public AGVErrorModel(string dateTimeNow, string agvName, string errorCode, string obscode)
            {
                this.Time = dateTimeNow;
                this.Name = agvName;
                this.Error = errorCode;
                this.Obstacle = obscode;
            }
        }
        private async void callAPI()
        {
            //Console.WriteLine("\t Start Call API");

            string[] arrayPosition = new string[] {"ENDING","STANDBY", "GO TO LINE", "GO TO LINE","PARKING AREA", "HOME","WIP AREA", "WIP-IN-1", "WIP-IN-2",
                                                   "WIP-IN-3", "WIP-IN-4","WIP-IN-5", "WIP-OUT-1", "LOW SPEED", "GO TO LINE", "LINE AREA","ENDING"};
            string[] arrayRFID = new string[] { "9","0", "1", "2", "4", "5", "19", "20", "21", "22", "23", "24", "10", "11", "31", "32", "33", "34",
                                                "15", "16","37", "38","39", "40","41", "42" ,"8"};
            //==================================================================================================================================// --> missionGetActiveList
            try
            {
                ResponseData data = await API("missionC.missionGetActiveList()");
                if (data.errMark == "OK")
                {
                    //var ds = new BindingList<AGVCallingModel>();
                    var showData = new BindingList<AGVCallingModel>();
                    string lastTIme = "";
                    for (int i = 0; i < data.msg.Count; i++)
                    {
                        counterLog += 1;
                        agvTime = UnixTimeStampToDateTime(data.msg[i][11]).ToString();
                        statusDelivery = data.msg[i][10].ToString();
                        statusDelivery1 = data.msg[0][10].ToString();
                        statusDelivery2 = data.msg[1][10].ToString();
                        jobId = data.msg[i][0];
                        lastTIme = agvTime;
                        // SHOW DATA AGV FROM SERVER
                        if (statusDelivery == "执行")
                        {
                            statusDelivery = "RUNNING";
                            jobId = data.msg[i][0];
                            long[] arrayJobId = new long[] {data.msg[0][0], data.msg[1][0], data.msg[2][0], data.msg[3][0], data.msg[4][0],
                                            data.msg[5][0], data.msg[6][0], data.msg[7][0], data.msg[8][0],data.msg[9][0] };

                            long maxJobid = arrayJobId.Last();
                            long searchJobid = jobId;
                            long indexJob = Array.IndexOf(arrayJobId, searchJobid);

                            AGVCallingModel temp = new AGVCallingModel(jobId, agvTime, agvName, data.msg[i][1].ToString(), statusDelivery);
                            showData.Add(temp);
                        }
                        else if (statusDelivery == "放弃")
                        {
                            statusDelivery = "HOLD";
                            AGVCallingModel temp = new AGVCallingModel(jobId, agvTime, agvName, data.msg[i][1].ToString(), statusDelivery);
                            //showData.Add(temp);
                        }
                        else if (statusDelivery == "正常结束")
                        {
                            statusDelivery = "FINISH";
                            AGVCallingModel temp = new AGVCallingModel(jobId, agvTime, agvName, data.msg[i][1].ToString(), statusDelivery);
                            showData.Add(temp);
                        }
                        else if (statusDelivery == "错误")
                        {
                            statusDelivery = "COM ERR";
                            AGVCallingModel temp = new AGVCallingModel(jobId, agvTime, agvName, data.msg[i][1].ToString(), statusDelivery);
                            showData.Add(temp);
                        }
                    }
                    //Check missionID and Status
                    for (int j = 0; j <data.msg.Count; j++)
                    {
                        missionId = data.msg[j][2];
                        jobStatus = data.msg[j][10];
                        jobNumber = data.msg[j][0];
                        if (jobStatus == "执行") { jobStatus = "RUNNING"; }
                        else if (jobStatus == "放弃") { jobStatus = "GIVE UP"; }
                        else if (jobStatus == "正常结束" && missionId ==56) { jobStatus = "FINISH"; }
                        else if (jobStatus == "正常结束" && missionId == 57) { jobStatus = "FINISH"; }
                        else if (jobStatus == "错误") { jobStatus = "COM ERR"; }
                        else {  }

                        if (line1Click == true && cntCall1 != 0 && jobStatus == "FINISH" && dataRfid ==1)
                        {
                            jobFinish = true;
                            saveJobNumber1 = 0;
                            
                        }
                        else if (line2Click == true && cntCall2 != 0 && jobStatus == "FINISH" && dataRfid == 1)
                        {
                            jobFinish2 = true;
                            saveJobNumber2 = 0;
                            Disini("287", "", "");
                        }
                    }
                    this.gridViewDS.Columns[0].Width = 175;
                    this.gridViewDS.Columns[1].Width = 75;
                    this.gridViewDS.Columns[2].Width = 80;
                    gridViewDS.Invoke((MethodInvoker)delegate { gridViewDS.DataSource = showData; });

                }
                else { Console.WriteLine("GetActiveList ErrorMark : {0}", data.errMark); }
                //==================================================================================================================================// --> getDeviceList
                data = await API("devC.getDeviceList()");
                if (data.errMark == "OK")
                {
                    List<AGVDeviceModel> showDevice = new List<AGVDeviceModel>();
                    for (int i = 0; i < data.msg.Count; i++)
                    {
                        double agvAddress = data.msg[0][3], offTime = data.msg[0][6];
                        if (agvAddress == 1 && offTime >= 3)
                        {
                            agvState = "OFF";
                            discBackground.Visible = true; labelDisconnect.Visible = true;

                        }
                        else if (agvAddress == 1 && offTime < 0.5)
                        {
                            agvState = "ON";
                            discBackground.Visible = false; labelDisconnect.Visible = false;
                        }
                        else { }
                    }
                    //==================================================================================================================================// --> deviceDic BTN_EMC
                    if (agvState == "ON")
                    {
                        ResponseData2 datanonArray = await APInonArray("devC.deviceDic[1].optionsLoader.load(carLib.RAM.DEV.BTN_EMC)");
                        if (datanonArray.errMark == "OK")
                        {
                            string errorCode = "";
                            List<AGVErrorModel> showError = new List<AGVErrorModel>();
                            for (int i = 0; i < datanonArray.msg.Count; i++)
                            {
                                long btnState = datanonArray.msg[1];
                                if (btnState == 0) { errorCode = "EMC STOP"; }
                                else { errorCode = "-"; }
                                AGVErrorModel temp = new AGVErrorModel(DateTime.Now.ToString(), agvName, errorCode, obsCode);
                                showError.Add(temp);
                                //Console.WriteLine("btnstate : {0}", obsCode);
                                gridViewError.Invoke((MethodInvoker)delegate { gridViewError.DataSource = showError; });
                            }
                        }
                        else
                        {
                            List<AGVErrorModel> showError = new List<AGVErrorModel>();
                            AGVErrorModel temp = new AGVErrorModel("", agvName, "", obsCode);
                            showError.Add(temp);
                            //gridViewError.DataSource = showError;
                            gridViewError.Invoke((MethodInvoker)delegate { gridViewError.DataSource = showError; });
                        }
                        //==================================================================================================================================// --> deviceDic OBS
                        datanonArray = await APInonArray("devC.deviceDic[1].optionsLoader.load(carLib.RAM.DEV.OBS)");
                        if (datanonArray.errMark == "OK")
                        {
                            List<AGVErrorModel> showError = new List<AGVErrorModel>();
                            for (int i = 0; i < datanonArray.msg.Count; i++)
                            {
                                long btnState = datanonArray.msg[2];
                                if (btnState != 0)
                                {
                                    obsCode = "OBS STOP";
                                }
                                else { obsCode = "-"; }
                            }
                        }
                    }
                    else { agvState = "OFF"; }
                }
                else { Console.WriteLine("GetDeviceList ErrorMark : {0}", data.errMark); }
                //==================================================================================================================================// --> getCarList
                data = await API("devC.getCarList()");
                if (data.errMark == "OK")
                {
                    List<AGVStatusModel> showData = new List<AGVStatusModel>();
                    for (int i = 0; i < data.msg.Count; i++)
                    {
                        readPower = data.msg[i][7];
                        //"车" Read RFID and detail Car activity
                        double dataStatus = data.msg[i][15], dataRute = data.msg[i][31], readAddress = data.msg[i][0];
                        dataRfid = data.msg[i][33]; 
                        string readType = data.msg[i][2];
                        string searchRFID = dataRfid.ToString();
                        int indexRFID = Array.IndexOf(arrayRFID, searchRFID);

                        agv1Status = dataStatus.ToString(); agvRoute = dataRute.ToString(); agvRfid = dataRfid.ToString();
                        //disini(agvStatus, agvRfid, agvRoute);

                        if (readAddress == agvAddress && readType == "车")
                        {
                            if (dataStatus == 0) { agv1Status = "STOP"; }
                            else if (dataStatus == 1) { agv1Status = "PAUSE"; }
                            else if (dataStatus == 2) { agv1Status = "RUN"; }
                            else { agv1Status = "STANDBY"; }

                            if ((dataRute == 1 || dataRute == 2 || dataRute == 3 || dataRute == 4 || dataRute == 5) && dataRfid ==11)
                            {
                                agvRoute = "GO TO LINE";
                            }
                            if ((dataRute == 20 || dataRute == 2) && (dataRfid == 1))
                            {
                                //Disini("WOI", "", "");
                                agvRoute = "HOME POST";
                                labelPosition.Text = agvRoute;
                                if (jobFinish ==true)
                                {
                                    Disini("b1", "", "");
                                    btnStation1.ActiveFillColor = Color.LimeGreen;
                                    btnStation1.IdleFillColor = Color.LimeGreen;
                                    btnStation1.ButtonText = "LINE 1";
                                    //cntCall1 = 0;
                                }
                                if (jobFinish2 ==true)
                                {
                                    Disini("b2", "", "");
                                    btnStation2.ActiveFillColor = Color.LimeGreen;
                                    btnStation2.IdleFillColor = Color.LimeGreen;
                                    btnStation2.ButtonText = "LINE 2";
                                    //cntCall2 = 0;
                                }
                                
                            }
                            else
                            {
                                //Disini("386", " cok", ""); 
                            }
                            AGVStatusModel temp = new AGVStatusModel(readPower.ToString() + "%", agvName, agvState, agv1Status.ToString());
                            showData.Add(temp);
                        }
                        //Disini("360", "", "");
                        Console.WriteLine("440 Btn1 : {0} cnt1 : {1}, job1 : {2}, mId : {3} ", line1Click, cntCall1, jobFinish,saveJobNumber1);
                        Console.WriteLine("440 Btn2 : {0} cnt2 : {1}, job2 : {2}, mId : {3}\n ", line2Click, cntCall2, jobFinish, saveJobNumber2);
                        //Console.WriteLine("RFID : {0}",dataRfid);
                    }
                    gridViewStatus.Invoke((MethodInvoker)delegate { gridViewStatus.DataSource = showData; });
                }
                else
                {
                    if (data.errMark == "err")
                    {
                        MessageBox.Show("Network Error");
                    }
                    List<AGVStatusModel> showData = new List<AGVStatusModel>();
                    AGVStatusModel temp = new AGVStatusModel(readPower.ToString() + "%", agvName, agvState, agv1Status.ToString());
                    showData.Add(temp);
                    //gridViewStatus.DataSource = showData;
                    gridViewStatus.Invoke((MethodInvoker)delegate { gridViewStatus.DataSource = showData; });
                }

                //==================================================================================================================================// --> 6 -- CALLINGAGV
                dataRfid_ = 1;
                //if (callStation1 == true && cntCall1 ==1 && testState == false && destLine=="'SMD_01'" && statusDelivery !="FINISH" && dataRfid_ ==1)
                //if (line1Click == true && cntCall1 == 1 && testState == false && agv1Status == "STOP" && missionId ==56 && jobStatus == "FINISH")
                if (line1Click == true && cntCall1 ==1 && testState ==false && jobFinish ==true)
                {
                    Disini("Call 1", destLine, saveJobNumber1.ToString());
                    string stationName = "missionC.netMissionAdd(" + destLine + ")";
                    CallingData dataCalling = await APIcalling(stationName);
                    Console.WriteLine(dataCalling.errMark);
                    if (dataCalling.errMark == "OK")
                    {
                        // Reset Button State
                        Console.WriteLine("call : {0}", stationName);
                        jobId = dataCalling.msg;
                        c1 = true; cntCall1 = 2; jobFinish = false;
                }
                else { Console.WriteLine("else : {0}", stationName); }
            }
                //else if (line2Click == true && cntCall2 == 1 && testState ==false && agv1Status=="GO" && jobFinish2==true)
                //else if (line2Click == true && cntCall2 == 1 && testState ==false && agv1Status=="STOP" && missionId ==57 && jobStatus == "FINISH")
                if (line2Click == true && cntCall2 == 1 && testState == false && jobFinish2 == true)
                    {
                    Disini("Call 2", destLine, saveJobNumber2.ToString());
                    string stationName = "missionC.netMissionAdd(" + destLine + ")";
                    CallingData dataCalling2 = await APIcalling(stationName);
                    if (dataCalling2.errMark == "OK")
                    {
                        // Reset Button State
                        Console.WriteLine("call : {0}", stationName);
                        jobId2 = dataCalling2.msg;
                        c2 = true; cntCall2 = 2;
                    }
                    else { Console.WriteLine("else : {0}", stationName); }
                }
                else 
                { 
                    //Console.WriteLine("netMissionAdd ErrorMark : {0}", data.errMark); 
                }

                //=========================================================CANCEL CALL=========================================================================// --> 7
                //if ((callStation1 == false && cntCall1 == 0))
                //{
                //    string cancelJobid = missionId1.ToString();
                //    string cancelApi = "missionC.netMissionCancel(" + cancelJobid + ")";
                //    CallingData dataCancel = await APIcalling(cancelApi);
                //}
                //if ((callStation2 == false && cntCall2 == 0))
                //{
                //    string cancelJobid2 = missionId2.ToString();
                //    string cancelApi = "missionC.netMissionCancel(" + cancelJobid2 + ")";
                //    CallingData dataCancel = await APIcalling(cancelApi);
                //}

                //Console.WriteLine("c1 : {0}, callSattion : {1}, cntCall : {2}, missionId : {3}", c1, callStation1, cntCall1, missionId1);
                //Console.WriteLine("c2 : {0}, callSattion : {1}, cntCall : {2}, missionId : {3}", c2, callStation2, cntCall2, missionId2);
                callAPI();

            }
            catch(NullReferenceException e)
            {
                Console.WriteLine("Catch Exception : {0}", e);
            }
            finally
            {
                //Console.WriteLine("finally block callAPI");
            }
            
        }

        private async Task<ResponseData> API(string command)
        {
            var cmd = new RequestData() { command =command, serialNumber=0};
            var json = JsonConvert.SerializeObject(cmd);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            ResponseData ret;
            while (true)
            {
                ret = JsonConvert.DeserializeObject<ResponseData>(response.Content.ReadAsStringAsync().Result);
                if (ret.command == cmd.command)
                {
                    break;
                }
            }
            return ret;
        }
        private async Task<ResponseData2> APInonArray(string command)
        {
            var cmd = new RequestData() { command = command, serialNumber = 0 };
            var json = JsonConvert.SerializeObject(cmd);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            ResponseData2 ret;
            while (true)
            {
                ret = JsonConvert.DeserializeObject<ResponseData2>(response.Content.ReadAsStringAsync().Result);
                if (ret.command == cmd.command)
                {
                    break;
                }
            }
            return ret;

        }
        private async Task<CallingData> APIcalling(string command)
        {
            var cmd = new RequestData() { command = command, serialNumber = 0 };
            var json = JsonConvert.SerializeObject(cmd);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var response = await client.PostAsync(url, data);
            CallingData ret;
            while (true)
            {
                ret = JsonConvert.DeserializeObject<CallingData>(response.Content.ReadAsStringAsync().Result);
                if (ret.command == cmd.command)
                {
                    break;
                }
            }
            return ret;
        }
        public Form1()
        {
            try
            {
                InitializeComponent();
                Control.CheckForIllegalCrossThreadCalls = false;
                gridViewStatus.Invoke((MethodInvoker)delegate { gridViewDS.DataSource = AGVStatus; });
                gridViewDS.Invoke((MethodInvoker)delegate { gridViewDS.DataSource = AGVData; });
                gridViewError.Invoke((MethodInvoker)delegate { gridViewDS.DataSource = AGVError; });
                AutoClosingMessageBox.Show("Connecting to The server...", "SYSTEM INFO", 10000);
                callAPI();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("NullException Raise {0}", e);
            }
            finally
            {
                Console.WriteLine("Finally Exception");
            }
            
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        
        private static void Disini(string s1,string s2,string s3)
        {
            Console.WriteLine("disini : {0}, {1}, {2} ",s1,s2,s3);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            cntTimer += 1;
            dateLabel.Text = DateTime.Now.ToString();
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            
            DialogResult dialogResult = MessageBox.Show("Do you want to exit application ?", "SYSTEM", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes) 
            {
                Application.Exit();
            }
            else if (dialogResult == DialogResult.No)
            {
                
            }
        }

        //private void btnEnter_Click(object sender, EventArgs e)
        //{
        //    cancelJob = int.Parse(tbCancelCall.Text);
        //    Console.WriteLine(cancelJob);
        //}
        //private void cancelCall_TextChanged(object sender, EventArgs e)
        //{
        //    if (System.Text.RegularExpressions.Regex.IsMatch(tbCancelCall.Text, "[^0-9]"))
        //    {
        //        MessageBox.Show("Please enter only numbers.");
        //        tbCancelCall.Text = tbCancelCall.Text.Remove(tbCancelCall.Text.Length - 1);
        //    }
        //}

        //private void clearText(object sender, EventArgs e)
        //{
        //    tbCancelCall.Clear();

        //}
        //private void bunifuButton1_Click(object sender, EventArgs e)
        //{
        //    //this.tbCancelCall.AppendText("1");
        //}

        // LOG DATA TEST
        //if (counterLog < 11)
        //{
        //    if (statusDelivery == "执行") { statusDelivery = "RUNNING"; }
        //    else if (statusDelivery == "放弃") { statusDelivery = "HOLD"; }
        //    else if (statusDelivery == "正常结束") { statusDelivery = "FINISH"; }
        //    else if (statusDelivery == "错误") { statusDelivery = "COM ERR"; }
        //    else if (statusDelivery == "错误")
        //    {
        //        string disc = agvName + " DISCONNECTED"; labelDisconnect.Text = disc; labelDisconnect.Visible = true;
        //        discBackground.Visible = true;
        //    }
        //    else { Console.WriteLine("statusDelivery : {0} ", statusDelivery); discBackground.Visible = false; labelDisconnect.Visible = false; }
        //}
        //else { }

        // VIsual Effect for Button
        //    btnStation2.ActiveFillColor = Color.LimeGreen;
        //    btnStation2.IdleFillColor = Color.LimeGreen;
        //    btnStation2.ButtonText = "LINE 2";
        //    cntCall2 = 0; 

        //else if (cntCall1 != 0 && jobFinish == true)
        //{
        //    Disini("b1", "", "");
        //    btnStation1.ActiveFillColor = Color.LimeGreen;
        //    btnStation1.IdleFillColor = Color.LimeGreen;
        //    btnStation1.ButtonText = "LINE 1";
        //    //line1 = "GO";
        //    cntCall1 = 0; jobFinish = false;
        //}
        ////else if (cntCall2 != 0 && agvStatus == "STOP")
        //else if (cntCall2 != 0 && jobFinish2 == true)
        //{
        //    Disini("b2", "", "");
        //    btnStation2.ActiveFillColor = Color.LimeGreen;
        //    btnStation2.IdleFillColor = Color.LimeGreen;
        //    btnStation2.ButtonText = "LINE 2";
        //    //line2 = "GO";
        //    cntCall2 = 0; jobFinish2 = false;
        //}
        //Console.Write("289 ");
        //Console.WriteLine("mId : {0} jobNum : {1} stat : {2}", missionId.ToString(), jobNumber, jobStatus);
        //Console.WriteLine("\tl1 : {0} l2 : {1}", line1, line2);
    }
}
