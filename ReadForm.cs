using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniWeread
{
    public partial class ReadForm : Form
    {
        private MainForm mainForm;
        private Point mouseDownLocation; // 用于记录鼠标按下的位置 
        //public ReadForm()
        //{
        //    InitializeComponent();
        //}
        private int charactersPerPage = 20; // 每页显示的字符数  
        private int currentPage = 0; // 当前页码（以0开始） 
        private string fullText = "";
        public ReadForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;

            this.mainForm.TaskCompletedCallback = res =>
            { 
                this.fullText = res.Replace("\n","");
                
            };
            InitHotkey();

            this.label1.MouseDown += new MouseEventHandler(this.Label_MouseDown);
            this.label1.MouseMove += new MouseEventHandler(this.Label_MouseMove);
            this.label1.MouseUp += new MouseEventHandler(this.Label_MouseUp);
           
            SetStyle();
            this.Disposed += ReadDisposed;
        }


        public void SetStyle()
        {
            this.label1.ForeColor = Properties.Settings.Default.FontColor;
            this.BackColor = Properties.Settings.Default.BackColor;
            this.Opacity = Properties.Settings.Default.ReadOpacity;
        }
        private void ReadDisposed(object sender, EventArgs e)
        {
            //Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.ReRead);
            //Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.PrevPage);
            //Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.NextPage);
            ////Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.NextPage);
            //Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.ToggleOnTop);
            //Hotkey.UnregisterHotKey(this.Handle, (int)HotKeyEvent.ToggleBar);
            SaveWindowPositionAndSize();

        }

        private void Read_Load(object sender, EventArgs e)
        {
            //this.mainForm.ReRead();

            this.ShowInTaskbar = false;
        }

        #region 窗口事件

        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownLocation = e.Location; // 记录鼠标按下的位置  
            }
        }

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - mouseDownLocation.X; // 根据鼠标位置的变化移动窗口  
                this.Top += e.Y - mouseDownLocation.Y;
                
            }
        }

        private void Label_MouseUp(object sender, MouseEventArgs e)
        {
            // 可以在这里添加一些鼠标释放后的逻辑  
            //SaveWindowPositionAndSize();
        }

        private void SaveWindowPositionAndSize()
        {
            
            // 使用设置文件保存窗口位置和大小  
            Properties.Settings.Default.ReadFormLocation = this.Location; 
            Properties.Settings.Default.ReadFormSize = this.Size;
            Properties.Settings.Default.Save();
        }
        // 从设置文件中读取窗口位置和大小，并恢复它们  
        public void LoadWindowPositionAndSize()
        {
           
            if (Properties.Settings.Default.ReadFormLocation != Point.Empty)
            {
                
                this.Location = Properties.Settings.Default.ReadFormLocation;
            }

            if (Properties.Settings.Default.ReadFormSize != Size.Empty)
            {
                this.Size = Properties.Settings.Default.ReadFormSize;
            }
            Properties.Settings.Default.ReadOpacity = this.Opacity;
        }



        #endregion


        #region 翻页

        private async void ReRead()
        {
            await this.mainForm.ReRead();

            UpdateLabelText();
        }
        private async void PrevPage()
        {
            // 如果当前页码大于0，则翻回上一页  
            if (currentPage > 0)
            {
                currentPage--;
                UpdateLabelText();
            }
            else
            {
                await mainForm.PrevPage();

                currentPage = this.fullText.Length / charactersPerPage ;
                UpdateLabelText();
            }


        }

        private async void NextPage()
        {
            // 计算下一页的起始索引  
            int startIndex = (currentPage+1) * charactersPerPage;
           
            // 如果下一页的起始索引小于文本长度，则翻页  
            if (startIndex < fullText.Length)
            {
                currentPage++;
                UpdateLabelText();
            }
            else
            {
                currentPage=0;
                await this.mainForm.NextPage();
                UpdateLabelText();
            }
            
        }

        private void UpdateLabelText()
        {
            // 计算当前页的起始和结束索引  
            //int startIndex = currentPage * charactersPerPage;
            //int endIndex = Math.Min(startIndex + charactersPerPage, fullText.Length);
            var str= fullText.Select(c => c.ToString())
                .Skip(currentPage * charactersPerPage).Take(charactersPerPage).ToArray();

            // 提取当前页的文本并更新Label  
            string pageText = string.Join("",str);
            label1.Text = pageText;
        }

        #endregion
        private void ReadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; 
                this.Hide();
               
            }
        }



        #region 快捷键绑定
        enum HotKeyEvent
        {
            ToggleHide,
            ReRead,
            PrevPage,
            NextPage,
            ToggleOnTop,
            ToggleBar
        }
        public void InitHotkey()
        {
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.ReRead, Hotkey.KeyModifiers.Alt, Keys.X);
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.PrevPage, Hotkey.KeyModifiers.Alt, Keys.R);
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.NextPage, Hotkey.KeyModifiers.Alt, Keys.C);
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.ToggleHide, Hotkey.KeyModifiers.Alt, Keys.S);
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.ToggleOnTop, Hotkey.KeyModifiers.Ctrl, Keys.T);
            //Hotkey.RegisterHotKey(this.Handle, (int)HotKeyEvent.ToggleBar, Hotkey.KeyModifiers.Alt, Keys.B);
        
        
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 ) // 0x0312是热键消息  
            {
                int wparam = m.WParam.ToInt32();
                switch (wparam) {
                    case (int)HotKeyEvent.ReRead:

                        ReRead();
                        break;
                    case (int)HotKeyEvent.PrevPage:

                        PrevPage();
                        break;
                    case (int)HotKeyEvent.NextPage:

                        NextPage();
                        break;
                    case (int)HotKeyEvent.ToggleHide:
                        if (Visible)
                        {
                            this.Hide();
                        }
                        else
                        {
                            this.Show();
                            this.Activate();
                        } 
                        break;
                    case (int)HotKeyEvent.ToggleBar:
                        if(this.FormBorderStyle == FormBorderStyle.None)
                        {
                            this.FormBorderStyle = FormBorderStyle.Sizable;
                        }
                        else
                        {
                            this.FormBorderStyle = FormBorderStyle.None;
                        }
                        
                        break;
                    case (int)HotKeyEvent.ToggleOnTop:

                        this.TopMost = !this.TopMost;
                        break;
                }
            }
            base.WndProc(ref m);
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            ReRead();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrevPage();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NextPage();
        }

        private void ReadForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (char.ToUpper(e.KeyChar) == 'D') {
            //    NextPage();
            //}
        }

        private void ReadForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.L:
                    ReRead();
                    break;
                case Keys.D:
                    NextPage();
                    break;
                case Keys.A:
                    PrevPage();
                    break;
                case Keys.Left:
                    PrevPage();
                    break;
                case Keys.Right:
                    NextPage();
                    break;

                case Keys.Up:
                    
                    break;
                case Keys.Down:
                    
                    break;
              
                case Keys.S://显示隐藏
                    if (Visible)
                    {
                        this.Hide();
                    }
                    else
                    {
                        this.Show();
                        this.Activate();
                    }
                    break;
                case Keys.G: //工具栏
                    if (this.FormBorderStyle == FormBorderStyle.None)
                    {
                        this.FormBorderStyle = FormBorderStyle.Sizable;
                    }
                    else
                    {
                        this.FormBorderStyle = FormBorderStyle.None;
                    } 
                    break;
                case Keys.T: //置顶
                    this.TopMost = !this.TopMost;
                    break; 
                case Keys.F: //字体颜色
                    ColorDialog colorDia = new ColorDialog();
                    colorDia.Color = this.label1.ForeColor;
                    if (colorDia.ShowDialog() == DialogResult.OK)
                    {
                        //获取所选择的颜色
                        Color colorChoosed = colorDia.Color;
                        //改变panel的背景色
                        this.label1.ForeColor = colorChoosed;
                        Properties.Settings.Default.FontColor = colorChoosed;
                    } 
                    break;
                case Keys.B: //背景色
                    ColorDialog colorDia2 = new ColorDialog();
                    colorDia2.Color = this.BackColor;
                    if (colorDia2.ShowDialog() == DialogResult.OK)
                    { 
                        Color colorChoosed = colorDia2.Color; 
                        this.BackColor = colorChoosed;
                        Properties.Settings.Default.BackColor = colorChoosed;
                    }
                    break;
                case Keys.Q: //更改字体大小
                    this.label1.Font= new Font(
                        this.label1.Font.FontFamily,
                        this.label1.Font.Size-0.2f);

                    break;
                case Keys.W://更改字体大小
                    this.label1.Font = new Font(
                        this.label1.Font.FontFamily,
                        this.label1.Font.Size + 0.2f);
                    break;
                case Keys.E: //减少字符数
                    if (charactersPerPage > 1)
                    {
                        charactersPerPage -= 1;
                        Properties.Settings.Default.PageCount = charactersPerPage;
                        currentPage = 0;
                        UpdateLabelText();
                    } 
                    break;
                case Keys.R: //增加字符数 
                    charactersPerPage += 1;
                    currentPage = 0;
                    Properties.Settings.Default.PageCount = charactersPerPage;
                    UpdateLabelText();
                    break;
                case Keys.Space: //自动翻页


                    break;
                case Keys.O: //透明度-
                    if (this.Opacity > 0.05)
                    {
                        this.Opacity -= 0.05;
                        Properties.Settings.Default.ReadOpacity = this.Opacity;
                    }
                   
                    break;
                case Keys.P: //透明度+
                    if (this.Opacity < 1)
                    {
                        this.Opacity += 0.05;
                        Properties.Settings.Default.ReadOpacity = this.Opacity;
                    }
                     

                    break;
            }
        }

        private void ReadForm_ResizeEnd(object sender, EventArgs e)
        {
            //SaveWindowPositionAndSize();
        }


         
    }
}
