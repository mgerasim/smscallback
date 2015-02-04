using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Limilabs.Mail.MIME;
using System.IO;
using System.Net.Sockets;
using Limilabs.Proxy;
using System.Net;

namespace SMSCallback
{
    public partial class SMSCallback : ServiceBase
    {
        private System.Timers.Timer timer;

        private const string _server = "imap.mail.ru";
        private const string _user = "calldo@bk.ru";
        private const string _password = "WHoOyiirkWNIv0hdHIli";

        public SMSCallback()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("OnStart");
            this.timer = new System.Timers.Timer(3000D);  // 3000 milliseconds = 3 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timerRun_Tick);
            this.timer.Start();
        }

        protected override void OnStop()
        {
            Log("OnStop");
            this.timer.Stop();
            this.timer = null;
        }

        private void Log(string message)
        {  
            /*
            using (StreamWriter w = File.AppendText("smscallback_log.txt"))
            {
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
            message);
            }
             * */
        }

        private void timerRun_Tick(object sender, EventArgs e)
        {
            Log("timerRun_Tick");
            try
            {
                this.timer.Stop();

                ProxyFactory factory = new ProxyFactory();
                IProxyClient proxy = factory.CreateProxy(ProxyType.Http, "10.198.7.38", 3128);
                Socket socket = proxy.Connect(_server, Imap.DefaultSSLPort);

                Imap imap = new Imap();
                imap.AttachSSL(socket, _server);
                                
                imap.Login(_user, _password);                       // You can also use: LoginPLAIN, LoginCRAM, LoginDIGEST, LoginOAUTH methods,
                // or use UseBestLogin method if you want Mail.dll to choose for you.

                imap.SelectInbox();                                 // You can select other folders, e.g. Sent folder: imap.Select("Sent");

                List<long> uids = imap.Search(Flag.Unseen);     // Find all unseen messages.

                Log("Number of unseen messages is: " + uids.Count);

                foreach (long uid in uids)
                {
                   
                    IMail email = new MailBuilder().CreateFromEml(  // Download and parse each message.
                        imap.GetMessageByUID(uid));
                   
                    Log(email.Text);

                    string url = "";
                    try
                    {
                        url = "http://10.198.1.90/ais/sms.pl?bgn=" + email.Text.Split(' ')[0] + "&end=" + email.Text.Split(' ')[1];
                    }
                    catch
                    {
                    }

                    Log("'"+url+"'");

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    response.Close();
                }
            }
            catch(Exception ex)
            {
                Log(ex.Message);
                Log(ex.Source);
            }
            this.timer.Start();
        }
    }
}
