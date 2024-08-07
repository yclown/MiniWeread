using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spire.OCR;
using System.Threading;
using System.Text.RegularExpressions;

namespace MiniWeread
{
    public partial class MainForm : Form
    {
        public string NowBookName;
        public Action<string> TaskCompletedCallback;

        public ReadForm readForm;
        public MainForm()
        {
            InitializeComponent();
            NowBookName = "";
        }

        #region 窗口



        private void SaveWindowPositionAndSize()
        {
            // 使用设置文件保存窗口位置和大小  
            Properties.Settings.Default.MainFormLocation = this.Location;
            Properties.Settings.Default.MainFormSize = this.Size;
            Properties.Settings.Default.Save();
        }

        private void LoadWindowPositionAndSize()
        {
            // 从设置文件中读取窗口位置和大小，并恢复它们  
            if (Properties.Settings.Default.MainFormLocation != Point.Empty)
            {
                this.Location = Properties.Settings.Default.MainFormLocation;
            }

            if (Properties.Settings.Default.MainFormSize != Size.Empty)
            {
                this.Size = Properties.Settings.Default.MainFormSize;
            }
        }

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    base.OnFormClosing(e);

        //    // 保存窗口位置和大小  
        //    SaveWindowPositionAndSize();
        //}
        #endregion

        protected override void WndProc(ref Message m)
        {
           
            if (m.Msg == 0x0112) // WM_SYSCOMMAND  
            {
                // 检查是否是最小化命令  
                if (m.WParam.ToInt32() == 0xF020) // SC_MINIMIZE  
                {
                    this.Hide();
                    //this.ShowInTaskbar = false;
                    // 取消最小化操作  
                    m.Result = (IntPtr)1; // 返回非零值表示消息已处理  
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            string data_str = await GetScreenshotBase64Async();
            byte[] imageBytes = Convert.FromBase64String(data_str);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                OcrScanner scanner = new OcrScanner();
                scanner.Scan(ms, OCRImageFormat.Png);
                string text = FormatText( scanner.Text.ToString());
                MessageBox.Show(text, "当前文本");
                //File.WriteAllText("output.txt", text);
            }
             
        }

        private string FormatText(string text)
        {
            text = text.Replace("Evaluation Warning : The version can be used only for evaluation purpose..."
                        , "")
                        .Replace("\n" , "");

            TaskCompletedCallback?.Invoke(text);
            return text;
        }

        public async Task<string> GetScreenshotTextDebugAsync()
        {
            OcrScanner scanner = new OcrScanner();
            string data_str = await GetScreenshotBase64Async();
            Image image = Base64ToImage(data_str);
            image.Save("screenshot.png"); 
            scanner.Scan("screenshot.png"); 
            string text = FormatText(scanner.Text.ToString()); 
            return text;
        }
        public async Task<string> GetScreenshotTextAsync()
        { 
            string data_str = await GetScreenshotBase64Async();
          
            byte[] imageBytes = Convert.FromBase64String(data_str);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                OcrScanner scanner = new OcrScanner();
                scanner.Scan(ms, OCRImageFormat.Png);
                string text = FormatText(scanner.Text.ToString());
                //保存到txt
                if (this.checkBox1.Checked)
                {
                    if (!string.IsNullOrEmpty(NowBookName))
                    {
                        Directory.CreateDirectory("saves");
                        var s = Path.Combine("saves", NowBookName + ".txt"); 
                        File.AppendAllText(s, text);
                    }

                }

                return text;
                //File.WriteAllText("output.txt", text);
            }
           
        }
        private Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        private JObject GetCaptureScreenshotParameters()
        {

            int width = (int)this.numericWidth.Value;
            int height = (int)this.numericHeight.Value;
            int x = (int)this.numericX.Value;
            int y = (int)this.numericY.Value;
            JObject ob = JObject.FromObject(new
            {
                clip = new
                {
                    x = x,
                    y = y,
                    width = width,
                    height = height,
                    scale = 1
                }
            });

            return ob;

        }


        private async Task<string> GetScreenshotBase64Async()
        {
            string r3 = await webView21.CoreWebView2.
               CallDevToolsProtocolMethodAsync("Page.captureScreenshot",
               GetCaptureScreenshotParameters().ToString());
            JObject o3 = JObject.Parse(r3);
            JToken data = o3["data"];
            string data_str = data.ToString();
            
            return data_str;
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            string data_str = await GetScreenshotBase64Async();
            //byte[] imageBytes = Convert.FromBase64String(data_str);
            Image image = Base64ToImage(data_str);
            image.Save("screenshot.png");
             
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadScreenshotParams();
            LoadWindowPositionAndSize();
            readForm = new ReadForm(this);
            this.checkBox1.Checked = Properties.Settings.Default.AutoSave;
            this.Text = this.Text + "V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                // 隐藏主窗体而不是退出
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void read_Click(object sender, EventArgs e)
        {
            
            this.Hide();
            notifyIcon1.Visible = true;
            readForm.Show();
            readForm.LoadWindowPositionAndSize();
            readForm.Activate();
        }

        
        #region  页面操作 
        public async Task NextPage()
        {
            await this.webView21.
               ExecuteScriptAsync("document.getElementsByClassName('renderTarget_pager_button_right')[0].click()")
                ;
            Thread.Sleep(500);
            await GetScreenshotTextAsync();
        }
        public async Task PrevPage()
        {
            await this.webView21
                .ExecuteScriptAsync("document.getElementsByClassName('renderTarget_pager_button')[0].click()");
            Thread.Sleep(500);
            await GetScreenshotTextAsync();
        }
        public async Task ReRead()
        {
            await GetScreenshotTextAsync();
        }
        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.XYOffset = new Point((int)this.numericX.Value, (int)this.numericY.Value);
            Properties.Settings.Default.ShotSize = new Size((int)this.numericWidth.Value, (int)this.numericHeight.Value);
            Properties.Settings.Default.Save();
            SaveWindowPositionAndSize();
            this.Dispose();
            this.Close(); 
        }

        private void readToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readForm.Show();
            readForm.Activate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }

        private void numeric_ValueChanged(object sender, EventArgs e)
        {
          
        }

     
        //   
        public void LoadScreenshotParams()
        {
            //this.Location = Properties.Settings.Default.MainFormLocation;
            this.numericX.Value = Properties.Settings.Default.XYOffset.X;
            this.numericY.Value = Properties.Settings.Default.XYOffset.Y;

            if (Properties.Settings.Default.ShotSize.Width == 0)
            {
                this.numericWidth.Value = webView21.Width;
            }
            else
            {
                this.numericWidth.Value = Properties.Settings.Default.ShotSize.Width;
            }
            if (Properties.Settings.Default.ShotSize.Height == 0)
            {
                this.numericHeight.Value = webView21.Height;
            }
            else
            {
                this.numericHeight.Value = Properties.Settings.Default.ShotSize.Height;
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.numericX.Value = 0;
            this.numericY.Value = 0;
            this.numericWidth.Value = webView21.Width;
            this.numericHeight.Value = webView21.Height;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.webView21.Source =new Uri( "https://weread.qq.com/");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoSave = this.checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private async  void webView21_NavigationCompletedAsync(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string htmlContent = await webView21.
                    CoreWebView2.ExecuteScriptAsync(@"JSON.parse(document.querySelectorAll('script[type=""application/ld+json""]')[0].text)");

                if (htmlContent != "null") {
                    var text = Regex.Unescape(htmlContent);
                    try
                    {
                        var info = JObject.Parse(text);
                        NowBookName = info["name"] + " " + info["author"]["name"];
                        this.label7.Text = NowBookName;
                    }
                    catch 
                    {
                        NowBookName = "";
                    }
                  

                }
                
                 
            }
        }
 
 

        private void main_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }
    }
}
