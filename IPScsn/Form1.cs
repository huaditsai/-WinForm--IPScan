using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IPScan
{
    public partial class Form1 : Form
    {
        IpDeal ipDeal;

        ConcurrentQueue<string> cq = new ConcurrentQueue<string>();

        int barMax = 0;//progress bar max value

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//using stopwatch

        public Form1()
        {
            InitializeComponent();

            ipDeal = new IpDeal();

            listView1.ListViewItemSorter = new IPScan.ListViewColumnSorter();
            listView1.ColumnClick += new ColumnClickEventHandler(IPScan.ListViewHelper.ListView_ColumnClick);

            backgroundWorker1.WorkerReportsProgress = true;//backgroundWorker can update

            textHostName.Text = Dns.GetHostEntry(Dns.GetHostName()).HostName;//get my hostname 
        }

        private void btnPutIP_Click(object sender, EventArgs e)//put host ip in textbox
        {
            IPHostEntry iphostentry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)//if IPv4
                    textIpFrom.Text = ipaddress.ToString();
            }
            textIpTo.Text = textIpFrom.Text;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                sw.Reset();//碼表歸零
                sw.Start();//碼表開始計時

                listView1.Items.Clear(); //clear listview items 

                backgroundWorker1.RunWorkerAsync();//DoWorker
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IPAddress IP1 = IPAddress.Parse(textIpFrom.Text);//is IP in textIpFrom?
                IPAddress IP2 = IPAddress.Parse(textIpTo.Text);//is IP in textIpTo?

                backgroundWorker1.WorkerReportsProgress = true;//backgroundWorker can update

                Scan();//start scan ip in range 
            }
            catch (FormatException fe)
            {
                MessageBox.Show(fe.Message);
            }
        }


        private void Scan()
        {
            string ip_from = textIpFrom.Text.Trim();
            string ip_to = textIpTo.Text.Trim();
            string[] ipAll = ipDeal.IpAll(ip_from, ip_to);       

            int i = 1;
            barMax = ipAll.Length;

            //ThreadPool.SetMaxThreads(1000, 1000);
            Parallel.ForEach(ipAll, ipcurr =>
            {
                string roundtripTime="", hostName;
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(ipcurr, 50);//ping

                try
                {
                    if (pingReply.Status == IPStatus.Success)
                    {
                        roundtripTime = pingReply.RoundtripTime.ToString();
                        hostName =  Dns.GetHostEntry(ipcurr).HostName;//ipDeal.GetReverseDNS(ipcurr, 100); //
                    }
                    else
                    {
                        roundtripTime = "N/S";
                        hostName = "N/S";
                    }                    
                }
                catch (SocketException se)
                {
                    //roundtripTime = "N/S";
                    hostName = se.Message;
                }

                backgroundWorker1.ReportProgress(i, i.ToString());
                i++;

                cq.Enqueue(ipcurr + "," + roundtripTime + "," + hostName + "," + pingReply.Status.ToString());

                //Thread.Sleep(1);
                //Application.DoEvents();
            });

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                string ipInfo;
                if (cq.TryDequeue(out ipInfo))
                {
                    string[] inInfos = ipInfo.Split(',');
                    listView1.Items.Add(new ListViewItem(new string[] { inInfos[0], inInfos[1], inInfos[2], inInfos[3] }));
                }

                //listView1.Items.Add(itemsKey, ipCurrent, 0);//add a Item,IP                            
                //listView1.Items[itemsKey].SubItems.Add(roundtripTime);//RoundtripTime
                //listView1.Items[itemsKey].SubItems.Add(hostName);//HostName
                //listView1.Items[itemsKey].SubItems.Add(status.ToString());//Status

                labelDeal.Text = "正在處理 : " + e.UserState.ToString() + " / " + barMax ;

                progressBar1.Maximum = barMax;
                progressBar1.Value = e.ProgressPercentage;

                labelTime.Text = sw.Elapsed.Minutes.ToString("D2") + " : " + sw.Elapsed.Seconds.ToString("D2") + " : " + (sw.Elapsed.Milliseconds / 10).ToString("D2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("1" + ex.Message);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelDeal.Text = "完成";

            sw.Stop();//碼錶停止
        }


    }
}
