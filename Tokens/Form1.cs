using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Tokens
{
    public partial class Form1 : Form
    {
        string filePath="";
         string text = "";
         int len;
         char CHAR;//当前读到的字符
         string token;//当前处理的单词
         int num;
         float f;
         bool IsEnd = false;
         string errorInf = "";
         int i = -1;
         enum classify
         {
             KEYWORDSY = 0,/*keyword*/
             CALSY = 3,/*分解符*/
             IDSY = 1,/*identifier*/
             INTSY = 2,/*整数*/
             FLOATSY = 4,/*浮点数*/
             COMMENTSY = 5/*注释*/,
             SINGLESY=7,/*单字符运算符*/
             DOUBLE=8,/*双字符运算符*/
         }
         classify symbol;
         string outt = "";
         string[] Kind = { "关键字", "标识符", "整数", "分界符", "浮点数", "注释", "" ,"单运算符","双运算符"};
         string[] res = { "begin", "end", "if", "then", "else", "const", "while", "var", "procedure", "Ood", "call", "write", "read","repeat","until" };


        public Form1()
        {
            InitializeComponent();
            this.ControlBox = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();//一个打开文件的对话框    
            openFileDialog1.Filter = "文本文件(*.txt)|*.txt";//设置允许打开的扩展名    
            if (openFileDialog1.ShowDialog() == DialogResult.OK)//判断是否选择了文件      
            {
                filePath = openFileDialog1.FileName;//打开文件的路径  
                SourceText.Text = "";//清空textBox1  
                StreamReader streamReader = new StreamReader(filePath, Encoding.Default);//记录用户选择的文件路径  
                while (!streamReader.EndOfStream)
                {//如果这个还没有读到文件尾  
                    string line = streamReader.ReadLine();//就一行一行地读  
                    SourceText.Text += line + "\r\n";
                }
                text = SourceText.Text;
                len = text.Length;
                streamReader.Close();//一定要关闭这个流，不然会和下面保存文件的流冲突  
               // SourceText.ScrollBars = ScrollBars.Vertical;
                SourceText.ScrollBars = ScrollBars.Both;
            }
            else
            {
                MessageBox.Show("请打开txt文件");
            }  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (text != "")
            {
                Results.ScrollBars = ScrollBars.Vertical;
                Results.ScrollBars = ScrollBars.Both;
                Results.Text = "";
                i = -1;
                Results.Text = "单词\t\t类别\t\t值\r\n";
                Console.WriteLine(Results.Text);
                IsEnd = false;
                while (IsEnd==false)
                {
                    errorInf = "";
                    int t = getsym();
                    if (IsEnd == false)
                    {
                        Results.Text = Results.Text + token;
                        if (token == "procedure") Results.Text += "\t";
                        else Results.Text += "\t\t";
                       Results.Text +=Kind[(int)symbol];
                        if (Kind[(int)symbol] == "单运算符" || Kind[(int)symbol] == "双运算符") Results.Text += "\t";
                        else Results.Text += "\t\t";
                        Results.Text+=outt + errorInf + "\r\n";
                    }
                    this.Refresh();
                   Console.Write(Results.Text);
                }
                MessageBox.Show("编译结束");
                
            }
        }











        void clearToken()
        {
            token = "";
            outt = "";
        }
        void catToken()
        {
            token = token + CHAR;
        }
        int reserver()
        {
            for (int i = 0; i < 15; i++)
            {
                if (res[i] == token)
                {
                    return i;
                    //break;
                }
            }
            return -1;
        }

        bool isColon()
        {
            if (CHAR == ':')
                return true;
            else return false;
        }
        bool isEqu()
        {
            if (CHAR == '=')
                return true;
            else return false;
        }
        bool isPlus()
        {
            if (CHAR == '+')
                return true;
            else return false;
        }
        bool isMinus()
        {
            if (CHAR == '-')
                return true;
            else return false;
        }
        bool isStar()
        {
            if (CHAR == '*')
                return true;
            else return false;
        }
        bool isLpar()
        {
            if (CHAR == '(')
                return true;
            else return false;
        }
        bool isRpar()
        {
            if (CHAR == ')')
                return true;
            else return false;
        }
        bool isComma()
        {
            if (CHAR == ',')
                return true;
            else return false;
        }
        bool isSemi()
        {
            if (CHAR == ';')
                return true;
            else return false;
        }
        bool isDivi()
        {
            if (CHAR == '/')
                return true;
            else return false;
        }
        bool isSmall()
        {
            if (CHAR == '<')
                return true;
            else return false;
        }
        bool isBig()
        {
            if (CHAR == '>')
                return true;
            else return false;
        }
        bool isDot()
        {
            if (CHAR == '.')
                return true;
            else return false;
        }
        void error()
        {
            Console.WriteLine("This word is error!\n");
            MessageBox.Show(token +" here is wrong!");
            errorInf = "This word is error!";
            symbol = (classify)6;
        }
        string returnError()
        {
            return errorInf;
        }
        void retract()
        {
            if (i > 0)
            {
                i--;
            }
            else
            {
                i = 0;
            }
            CHAR = text[i];
        }
        void getChar()
        {
            if (i < len - 1)
            {
                i++;
                CHAR = text[i];
            }
            else { CHAR = '\0'; IsEnd = true; }

        }

        string transNum()
        {
            int numVal = Int32.Parse(token);
            string str = Convert.ToString(numVal, 2);
            return str;
        }
        string transFloat()
        {
            string ans = "";
            return ans;
        }




        int getsym()
        {
            if (IsEnd == false)
            {
                clearToken();
                getChar();
                //cout << "first get char  " << CHAR << endl;
                while (CHAR=='\r' || Char.IsWhiteSpace(CHAR) || CHAR == '\t'||CHAR=='\n'||CHAR==' ') { getChar(); }
                //字母
                if (Char.IsLetter(CHAR))
                {
                    catToken();
                    getChar();
                    while (Char.IsLetter(CHAR) || Char.IsDigit(CHAR))
                    {
                        catToken(); getChar();
                    }
                    retract();
                    int resultValue = reserver();
                    if (resultValue == -1) { symbol = classify.IDSY; outt = token; }
                    else { symbol = classify.KEYWORDSY; outt = token; }
                }
                //数字
                else if (Char.IsDigit(CHAR))
                {
                    catToken();
                    getChar();
                    while (Char.IsDigit(CHAR))
                    {
                        catToken(); getChar();
                    }
                    //整数
                    if (!isDot())
                    {
                        retract();
                        outt = transNum();
                        symbol = classify.INTSY;

                    }
                    else
                    {
                        catToken();
                        getChar();
                        while (Char.IsDigit(CHAR))
                        {
                            catToken(); getChar();
                        }
                        if (isDot())
                        {
                           
                            catToken();
                            getChar();
                            while (Char.IsDigit(CHAR))
                            {
                                catToken(); getChar();
                            }
                            symbol = (classify)6;
                            error();
                             //MessageBox.Show(token +" here is wrong!");
                        }
                        else { symbol = classify.FLOATSY; }

                        retract();

                        outt = token;
                    }
                }
                else if (isColon())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); symbol = classify.DOUBLE; outt = ":="; }
                    else { retract(); symbol = classify.CALSY; outt = "："; }
                }
                else if (isEqu()) { catToken(); symbol = classify.SINGLESY; outt = "="; }
                else if (isPlus()) { catToken(); symbol = classify.SINGLESY; outt = "+"; }
                else if (isMinus()) { catToken(); symbol = classify.SINGLESY; outt = "-"; }
                else if (isStar()) { catToken(); symbol = classify.SINGLESY; outt = "*"; }
                else if (isLpar()) { catToken(); symbol = classify.CALSY; outt = "（"; }
                else if (isRpar()) { catToken(); symbol = classify.CALSY; outt = "）"; }
                else if (isComma()) { catToken(); symbol = classify.CALSY; outt = "，"; }
                else if (isSemi()) { catToken(); symbol = classify.CALSY; outt = ";"; }
                else if (isSmall())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); symbol = classify.DOUBLE; outt = "<="; }
                    else
                    {
                        retract(); symbol = classify.SINGLESY; outt = "<";
                    }
                }
                else if (isBig())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); symbol = classify.DOUBLE; outt = ">="; }
                    else
                    {
                        retract(); symbol = classify.SINGLESY; outt = ">";
                    }
                }
                else if(isDot())
                {
                    catToken();
                    symbol = classify.CALSY;outt=".";
                }
                else if (isDivi())
                {

                    getChar();
                    if (isStar())
                    {
                        do
                        {
                            do { getChar(); } while (!isStar());
                            do
                            {
                                getChar();
                                if (isDivi())
                                {
                                    symbol = classify.COMMENTSY;
                                    outt = ""; return 0;
                                }
                            } while (isStar());
                        } while (!isStar());
                        symbol = classify.COMMENTSY;
                        outt = "";
                    }
                    else
                    {
                        retract();
                        catToken();
                        symbol = classify.SINGLESY; outt = "/";
                    }
                }
                else if (IsEnd == true) return 0;
                else { catToken(); error(); }
                return 0;
            }
            return -1;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result;
            result = MessageBox.Show("确定退出吗？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                Application.ExitThread();
            }
            else
            {
                e.Cancel = true;
            }
        }

        /*private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        */








    }
}
