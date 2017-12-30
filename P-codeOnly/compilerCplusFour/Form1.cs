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
using System.Collections;
namespace compilerCplusFour
{
    public partial class Form1 : Form
    {
        //global variables
const int norw = 15;//保留字个数
const int txmax = 100;//符号表最大长度
const int nmax = 14;//数字最多位数
const int al = 10;//标识符最大长度
const int amax = 2048;//最大地址
const int levmax = 3;//最多允许的嵌套层数
const int cxmax = 200;//最多的虚拟机代码数
const int stacksize = 500;//运行时最多允许数据栈为500
const int symnum = 37;//符号数
const int lmax = 500;//source max length

/* 符号 */
enum symbol {
	nul, ident, ifsym, thensym, elsesym, constsym, 
	whilesym, varsym, procsym, 
	oddsym, callsym, writesym, readsym,
	repeatsym,untilsym,
	numbersym, plusym, minusym,
	timesym, slashsym,eqlsym, neqsym,
	lssym, leqsym, gtrsym, geqsym, lparensym,
	rparensym, commasym, semicolonsym, periodsym, becomesym,
	beginsym, endsym, 
	dosym,  commentsym,blanksym
	
};
/* 符号表中的类型 */
enum objecttyp {
	constant,
	variable,
	procedure,
};
enum fct {
	lit, opr, lod,
	sto, cal, ini,
	jmp, jpc,red,wrt
};

/* 虚拟机代码结构 */
struct instruction
{
     public fct f; /* 虚拟机代码指令 */
	public int l;      /* 引用层与声明层的层次差 */
	public int a;      /* 根据f的不同而不同 */
};
/* 虚拟机代码指令 */

const int fucnum = 10;//指令数
string filePath = "";//文件路径
string text = "";//读入的源码
int len;
char CHAR;//当前读到的字符
string token;//当前处理的单词
int num;//当前读到的数字
symbol sym;//当前读到的符号
string id;//刚刚读到的标识符
int ll;//行长度
int lnum;//行号
int cc;//这一行的第几个字符
int err;//标识是否有错
int cx;//虚拟机代码指针
//string[] Lines = new string[lmax];//read by lines
string[] Lines;
int linelen;
instruction[] code=new instruction[cxmax];//已有的虚拟机指令
string []word = { "begin", "end", "if", "then", "else", "const", "while", "var", "procedure", "odd", "call", "write", "read", "repeat", "until","do" };
int i = -1;//词法分析用于读字符
symbol[] wsym=new symbol[norw+5];//保留字单词与枚举值
symbol []ssym=new symbol[300];//单字符的符号+-*、。。。
string []mnemonic=new string[fucnum];//虚拟机代码对应单词 便于输出
//分别表示声明、语句、因子开始的符号集合
bool[] declbegsys = new bool[symnum];
bool[] statbegsys = new bool[symnum ];
bool[] facbegsys = new bool[symnum ];
const int decllen = 3, statlen = 4,faclen = 3;//开始符号集合长度，便于遍历
bool IsEnd = false;//一行是否读到了结束
bool complete = false;//读完全部源代码
ArrayList symset = new ArrayList();//可选符号集合
string[] ErrorMeaasge = new string[45];
/*符号表结构*/
struct tablestruct{
	public string name;
	public objecttyp kind;
	public int val;
	public int level;
	public int addr;
	public int size;//需要给procedure分配的空间
};
tablestruct []table=new tablestruct[txmax];
string outt;
void Error(int n)
{
    Console.WriteLine("**** " + lnum.ToString() + " " + n.ToString()+" "+ErrorMeaasge[n]);
    resultBox.Text = resultBox.Text + "\r\n**** "+lnum;
    resultBox.Text = resultBox.Text + " ^ " + n.ToString()+" "+ErrorMeaasge[n];
        err++;
}
bool inset(int e,ref bool[] s)
        {
            return s[e];
        }
void addset(ref bool []sr,ref bool []s1,ref bool[] s2,int n)
 {
     int i;
     for (i = 0; i < n; i++)
     {
         sr[i] = s1[i] || s2[i];
     }
 }
void memcpy(ref bool [] s1,bool [] s2)
{
    for (int i = 0; i < symnum; i++)
        s1[i] = s2[i];
}
void memset(ref bool [] s1)
{
    for (int j = 0; j < symnum; j++)
        s1[j] = false;
}
void print(bool [] s)
{
    for (int i = 0; i < symnum; i++)
    { Console.Write(s[i] + "  ");
    if (i == 5 || i == 8 || i == 12 || i == 14 || i == 17 || i == 21 || i == 26 || i == 31 || i == 33)
        Console.WriteLine();
    }
    Console.WriteLine();
}
void init()
{

    ll = 0; cc = -1; cx = 0; err = 0; i = -1; CHAR = ' '; lnum = 0; linelen = 0;
    IsEnd = false;  complete = false; //lev = 0;
    Lines = new string[lmax];
    //dx = 3; tx = 0;
    CHAR = ' ';
    //"begin", "end", "if", "then", "else", "const", "while", "var", "procedure", "odd", "call", "write", 
    //"read", "repeat", "until","do"
    wsym[0] = symbol.beginsym; wsym[1] = symbol.endsym; wsym[2] = symbol.ifsym; wsym[3] = symbol.thensym; wsym[4] = symbol.elsesym;
    wsym[5] = symbol.constsym; wsym[6] = symbol.whilesym; wsym[7] = symbol.varsym; wsym[8] = symbol.procsym;
    wsym[9] = symbol.oddsym; wsym[10] = symbol.callsym; wsym[11] = symbol.writesym; wsym[12] = symbol.readsym;
    wsym[13] = symbol.repeatsym; wsym[14] = symbol.untilsym; wsym[15] = symbol.dosym;
    wsym[16] = symbol.blanksym;

    ssym['+'] = symbol.plusym; ssym['-'] = symbol.minusym; ssym['*'] = symbol.timesym; ssym['/'] = symbol.slashsym;
    ssym['('] = symbol.lparensym; ssym[')'] = symbol.rparensym; ssym['='] = symbol.eqlsym; ssym['>'] = symbol.gtrsym; ssym['<'] = symbol.lssym;
    ssym[','] = symbol.commasym; ssym['.'] = symbol.periodsym; ssym[';'] = symbol.semicolonsym;

    mnemonic[(int)fct.lit] = "LIT\r\t"; mnemonic[(int)fct.opr] = "OPR\r\t"; mnemonic[(int)fct.lod] = "LOD\r\t"; mnemonic[(int)fct.sto] = "STO\r\t";
    mnemonic[(int)fct.cal] = "CAL\r\t"; mnemonic[(int)fct.ini] = "INT\r\t"; mnemonic[(int)fct.jmp] = "JMP\r\t"; mnemonic[(int)fct.jpc] = "JPC\r\t";
    mnemonic[(int)fct.red] = "RED\r\t"; mnemonic[(int)fct.wrt] = "WRT\r\t";
/*
    declbegsys.Add(symbol.constsym); declbegsys.Add(symbol.varsym); declbegsys.Add(symbol.procsym);
    statbegsys.Add ( symbol.beginsym); statbegsys.Add( symbol.callsym); statbegsys.Add(symbol.ifsym); statbegsys.Add( symbol.whilesym);
    facbegsys.Add( symbol.ident); facbegsys.Add( symbol.numbersym); facbegsys.Add( symbol.lparensym);
    */
    /* 设置符号集 */
    for (int j = 0; j < symnum; j++)
    {
        declbegsys[j] = false;
        statbegsys[j] = false;
        facbegsys[j] = false;
    }

   /* 设置声明开始符号集 */
    declbegsys[(int)symbol.constsym] = true;
    declbegsys[(int)symbol.varsym] = true;
    declbegsys[(int)symbol.procsym] = true;

    statbegsys[(int)symbol.beginsym] = true; statbegsys[(int)symbol.callsym] = true;
    statbegsys[(int)symbol.ifsym] = true; statbegsys[(int)symbol.whilesym] = true;

    facbegsys[(int)symbol.ident] = true; facbegsys[(int)symbol.numbersym] = true;
    facbegsys[(int)symbol.lparensym] = true;  

    ErrorMeaasge[1] = "应为=而非：=\r\n"; ErrorMeaasge[2] = "=后应为数字\r\n"; ErrorMeaasge[3] = "标识符后应为=\r\n";
    ErrorMeaasge[4] = "const var procedure后应为标识符\r\n"; ErrorMeaasge[5] = "漏掉逗号或分号\r\n"; ErrorMeaasge[6] = "过程说明后的符号不正确\r\n";
    ErrorMeaasge[7] = "应为语句\r\n"; ErrorMeaasge[8] = "程序体内语句后面的符号错误\r\n";
    ErrorMeaasge[9] = "应为句号结束\r\n"; ErrorMeaasge[10] = "语句之间漏分号\r\n"; ErrorMeaasge[11] = "标识符未声明\r\n";
    ErrorMeaasge[12] = "不可向常量或变量赋值\r\n"; ErrorMeaasge[13] = "应为:=\r\n";ErrorMeaasge[14]="call后应为标识符\r\n";
    ErrorMeaasge[15] = "不可调用常量或变量\r\n"; ErrorMeaasge[16] = "应为then\r\n"; ErrorMeaasge[17] = "应为分号或end\r\n";
    ErrorMeaasge[18] = "应为do\r\n"; ErrorMeaasge[19] = "语句后符号不正确\r\n"; ErrorMeaasge[20] = "应为关系运算符\r\n";
    ErrorMeaasge[21] = "表达式内不能有过程\r\n"; ErrorMeaasge[22] = "漏掉右括号\r\n"; ErrorMeaasge[23] = "因子后不可为这个符号\r\n";
    ErrorMeaasge[24] = "表达式不能以此开始\r\n"; ErrorMeaasge[30] = "数字太大\r\n"; ErrorMeaasge[40] = "应为左括号\r\n";

}
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        //read and get tokens
        private void button1_Click(object sender, EventArgs e)
        {
            init();
            resultBox.Text = ""; text = ""; ll = 0; 
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
                    IsEnd = false;
                    linelen = line.Length;
                    Lines[ll] = line; ll++;
                    text =text + line+"\r\n";
                    SourceText.Text += ll+"  "+line + "\r\n";
                }
               // text = SourceText.Text;
                len = text.Length;
                streamReader.Close();//关闭这个流
                SourceText.ScrollBars = ScrollBars.Both;
            }
            else
            {
                MessageBox.Show("请打开txt文件");
            }  

        }
        string transNum()
        {
            int numVal = Int32.Parse(token);
            num = numVal;
            string str = Convert.ToString(numVal, 2);
            return str;
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
            cc = i;
            //CHAR = Lines[curline][i];
            CHAR = text[i];
          // if(resultBox.Text.Length>0) 
           resultBox.Text = resultBox.Text.Substring(0, resultBox.Text.Length - 1);
            wholine = wholine.Substring(0, wholine.Length - 1);
        }
        int eachline;
        string wholine = "";
       void getChar()
        {
            linelen = text.Length;
          /* if(i == -1)
           {
               eachline = 0; wholine = "";
               resultBox.Text = resultBox.Text + cx + "\r\t";
           }*/
             if (i==-1||CHAR == '\n' )
            {
                eachline = 0;
                if (CHAR == '\n') lnum++;
                //if(lnum!=1)
                    resultBox.Text = resultBox.Text + cx + "\r\t";
              // resultBox.Text = resultBox.Text + wholine;
                Console.Write(cx + "\t" + wholine + "\n");
                wholine = "";
                this.Refresh();
            }
            if (i < linelen - 1)
            {
                i++; eachline++;
                CHAR =text[i];
                resultBox.Text = resultBox.Text + CHAR;
                wholine += CHAR;
                //Console.Write(CHAR);
                this.Refresh();
            }
            else { CHAR = '\0'; IsEnd = true; }
            cc = i;
            
        }


        void clearToken()
        {
            token = "";
        }
        void catToken()
        {
            token = token + CHAR;
        }
        symbol reserver()
        {
            
            for (int i = 0; i < 16; i++)
            {
                if (word[i] == token)
                {
                    return wsym[i]; //break;
                }
            }
            return wsym[16];
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

        int getsym()
        {
            if (complete == false)
            {
                clearToken();
                getChar();
                while (CHAR == '\r' || Char.IsWhiteSpace(CHAR) || CHAR == '\t' || CHAR == '\n' || CHAR == ' ') { getChar(); }
                if (CHAR == '\0') { Console.WriteLine("return -1\n"); sym = symbol.nul; return -1; }
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
                    symbol resultValue = reserver();
                    if (resultValue == wsym[16]) { sym = symbol.ident; outt = token; id = token; }
                    else { sym=resultValue; outt = token; }
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
                      retract();
                        outt = transNum();
                        sym = symbol.numbersym;
                        outt = token;
                    
                }
                else if (isColon())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); sym= symbol.becomesym; outt = ":="; }
                    else { retract(); 
                        sym = symbol.nul; outt = "："; }
                }
                else if (isEqu()) { catToken(); sym =symbol.eqlsym; outt = "="; }
                else if (isPlus()) { catToken(); sym = symbol.plusym; outt = "+"; }
                else if (isMinus()) { catToken(); sym = symbol.minusym; outt = "-"; }
                else if (isStar()) { catToken(); sym = symbol.timesym; outt = "*"; }
                else if (isLpar()) { catToken(); sym = symbol.lparensym; outt = "（"; }
                else if (isRpar()) { catToken(); sym= symbol.rparensym; outt = "）"; }
                else if (isComma()) { catToken(); sym = symbol.commasym; outt = "，"; }
                else if (isSemi()) { catToken(); sym= symbol.semicolonsym; outt = ";"; }
                else if (isSmall())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); sym = symbol.leqsym; outt = "<="; }
                    else
                    {
                        retract(); 
                        sym = symbol.lssym; outt = "<";
                    }
                }
                else if (isBig())
                {
                    catToken();
                    getChar();
                    if (isEqu()) { catToken(); sym = symbol.geqsym; outt = ">="; }
                    else
                    {
                        retract(); 
                        sym=symbol.gtrsym; outt = ">";
                    }
                }
                else if (isDot())
                {
                    catToken();
                    sym= symbol.periodsym; outt = ".";
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
                                    sym = symbol.commentsym;
                                    outt = ""; return 0;
                                }
                            } while (isStar());
                        } while (!isStar());
                        sym = symbol.commentsym;
                        outt = "";
                    }
                    else
                    {
                        retract();
                        catToken();
                        sym = symbol.slashsym; outt = "/";
                    }
                }
                else if (IsEnd == true) { //Console.WriteLine("getsym():sym= " + sym); 
                    return 0; }
                else { catToken(); Error(0); }
                //Console.WriteLine("getsym():sym= " + sym);
                return 0;
            }
            sym = symbol.nul;
            Console.WriteLine("return -1\n " );
            return -1;
            
        }
        //compile button
        private void button2_Click(object sender, EventArgs e)
        {
            bool[] nextLev = new bool[symnum];
            int r =getsym();
            instruBox.Text = "";
            addset(ref nextLev, ref declbegsys, ref statbegsys, symnum);
            nextLev[(int)symbol.periodsym] = true;
            Block(0, 0,ref nextLev);
            if (sym != symbol.periodsym) Error(9);
            if(err==0)
            {
                instruBox.Text = instruBox.Text + "Parsing success\r\n";
                Console.WriteLine("Parsing success");
                this.Refresh();
                Inteprete();
            }
            else
            {
                instruBox.Text = instruBox.Text + "there are errors!\r\n";
            }
        }

        /// <summary>
        /// 生成P-code指令 送人目标程序区
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void gen(fct x, int y, int z)
        {
            if (cx >= cxmax) { resultBox.Text = resultBox.Text + "program too long\r\n";
            Console.WriteLine("program too long\n");
            }
            if (z >= amax) { resultBox.Text = resultBox.Text + "address too big\n";
            Console.WriteLine("address too big\n");
            }
            code[cx].f = x;
            code[cx].l = y;
            code[cx].a = z;
            cx++;
        }

        /// <summary>
        /// 测试当前单词符号是否合法
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="n"></param>
        void test(ref bool [] s1,ref bool [] s2,int n)
        {
            if(!inset((int)sym,ref s1))
            {
                Error(n);
                while(!inset((int)sym,ref s1)&&!inset((int)sym,ref s2))
                {
                    int r = getsym();
                    if (r == -1) break;
                }
            }
        }




        /// <summary>
        /// 登记符号表,k为种类 ptx指向符号表尾 lev指向标识符所在层 pdx应给配给变量的相对地址
        /// </summary>
        /// <param name="k"></param>
        void Enterr(objecttyp k,ref int ptx,int lev,ref int pdx)
        {
            ptx++;
            table[ptx].name = id;
            table[ptx].kind = k;
            switch (k)
            {
                case objecttyp.constant:
                    if (num > amax) { Error(30); num = 0; }
                    table[ptx].val = num;
                    break;
                case objecttyp.variable:
                    table[ptx].level = lev;
                    table[ptx].addr = pdx; pdx++;
                    break;
                case objecttyp.procedure:
                    table[ptx].level = lev;
                    break;
            }
        }
        /// <summary>
        /// 查表 返回标识符id的位置，从tx倒叙位置开始查找
        /// 找不到返回0，否则返回在表中的位置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int Position(string id,int tx)
        {
            table[0].name = id;
            int i=tx;
            while(table[i].name !=id)
            {
                i--;
            }
            return i;
        }
        /// <summary>
        /// 常量定义处理,合法进符号表
        /// 常量定义 ::= <标识符>=<无符号整数>
        /// </summary>
        void constdeclaration(ref int ptx,int lev,ref int pdx)
        {
            if(sym==symbol.ident)
            {
                getsym();
                if (sym == symbol.becomesym || sym == symbol.eqlsym)
                {
                    if (sym == symbol.becomesym) Error(1);
                    getsym();
                    if (sym == symbol.numbersym)
                    {
                        Enterr(objecttyp.constant,ref ptx,lev,ref pdx);
                        getsym();
                    }
                    else Error(2);
                }
                else Error(3);
            }
             
            else
            { Error(4); }
        }
        /// <summary>
        /// 处理变量说明
        /// </summary>
        void vardeclaration(ref int ptx,int lev,ref int pdx)
        {
            if (sym == symbol.ident)
            {
                Enterr(objecttyp.variable,ref ptx,lev,ref pdx);
                getsym();
            }
            else Error(4);
        }
        /// <summary>
        /// 列出这一block生成的P-code指令
        /// </summary>
        void listcode(int cx0)
        {
            int i;
            Console.WriteLine("P-code:\n");
            resultBox.Text = resultBox.Text + "\r\nP-code:\r\n";
            for(i = cx0;i<=cx-1;i++)
            {
                Console.WriteLine("{0,2} {1,5} {2,3} {3,5}",i , mnemonic[(int)code[i].f]  ,code[i].l ,code[i].a);
                resultBox.Text = resultBox.Text+ i+"\r\t"+mnemonic[(int)code[i].f]+"\r\t";
                resultBox.Text = resultBox.Text + code[i].l + "\r\t"+code[i].a + "\r\n";
            }
            this.Refresh();
        }
        /// <summary>
        /// 表达式处理 [+-]<term>{<+-><term>}
        /// </summary>
        /// <param name="fsys"></param>
        void expressin(ref bool[] fsys,ref int ptx,int lev)
        {
            bool[] nextLev = new bool[symnum];
            symbol addop;
            memcpy(ref nextLev, fsys);
            nextLev[(int)symbol.plusym] = true; nextLev[(int)symbol.minusym] = true;
                if (sym == symbol.plusym || sym == symbol.minusym)
                {
                    addop = sym;//开头的正负号
                    getsym();

                    term(ref nextLev,ref ptx,lev);
                    if (addop == symbol.minusym) gen(fct.opr, 0, 1);
                }
                else term(ref nextLev, ref ptx, lev);
            while(sym == symbol.plusym||sym == symbol.minusym)
            {
                addop = sym;
                getsym();
                term(ref nextLev, ref ptx, lev);
                if (addop == symbol.plusym) gen(fct.opr, 0, 2);
                else gen(fct.opr, 0, 3);
            }
        }
        /// <summary>
        /// 因子分析
        /// </summary>
        /// <param name="fsys"></param>
        void factor(ref bool[] fsys, ref int ptx, int lev)
        {
            bool[] nextLev = new bool[symnum];
            int i;
            test(ref facbegsys, ref fsys, 24);//检测因子的开始符号

            while (inset((int)sym, ref facbegsys))
            {
                if(sym == symbol.ident)
                {
                    i = Position(id, ptx);
                    if (i == 0) Error(11);
                    else
                    {
                        switch(table[i].kind)
                        {
                            case objecttyp.constant:
                                gen(fct.lit, 0, table[i].val);//常量入栈
                                break;
                            case objecttyp.variable:
                                gen(fct.lod, lev - table[i].level, table[i].addr);//变量地址入栈
                                break;
                            case objecttyp.procedure:
                                Error(21);
                                break;
                        }
                    }
                    getsym();
                }
                else if(sym == symbol.numbersym)
                {
                    if (num > amax) { Error(30); num = 0; }
                    gen(fct.lit, 0, num);
                    getsym();
                }
                else if(sym == symbol.lparensym)//因子表达式
                {
                    getsym();
                    //ArrayList temp = new ArrayList(fsys);
                    //temp.Add(symbol.rparensym);
                    memcpy(ref nextLev,fsys);
                    nextLev[(int)symbol.rparensym] = true;
                    expressin(ref nextLev,ref ptx,lev);
                    if (sym == symbol.rparensym)
                        getsym();
                    else Error(22);
                }
                //Console.WriteLine("fsys again");
               // print(fsys);
                bool[] nexLev = new bool[symnum];
                memset(ref nexLev);
                nexLev[(int)symbol.lparensym] = true;
               // print(nexLev);
               // Console.WriteLine("fsys, sym= "+sym.ToString());
               // print(fsys);
                test(ref fsys, ref nexLev, 23);//报错并找到下一个可行的单词
            }
        }
        /// <summary>
        /// 项 <factor>{<*/><factor>}
        /// </summary>
        /// <param name="fsys"></param>
        void term(ref bool[] fsys, ref int ptx, int lev)
        {
            bool[] nextLev = new bool[symnum];
           // print(fsys);
            symbol mulop;
            memcpy(ref nextLev, fsys);
            nextLev[(int)symbol.timesym] = true; nextLev[(int)symbol.slashsym] = true;
          //  print(nextLev);
            factor(ref nextLev,ref ptx,lev);
            while(sym == symbol.timesym||sym == symbol.slashsym)
            {
                mulop = sym;
                getsym();
                factor(ref nextLev,ref ptx,lev);
                if (mulop == symbol.timesym) gen(fct.opr, 0, 4);
                else gen(fct.opr, 0, 5);
            }
        }
        /// <summary>
        /// 与expression()并列，处理条件分析
        ///  expression-关系运算符-expression|odd-expression
        /// </summary>
        /// <param name="fsys"></param>
        void condition(ref bool[] fsys,ref int ptx,int lev)
        {
            bool[] nextLev = new bool[symnum];
            symbol relop;
            if(sym == symbol.oddsym)
            {
                getsym();
                expressin(ref fsys,ref ptx,lev);
                gen(fct.opr, 0, 6);
            }
            else
            {
                memcpy(ref nextLev, fsys);
                nextLev[(int)symbol.eqlsym] = true; nextLev[(int)symbol.neqsym] = true;
                nextLev[(int)symbol.lssym] = true; nextLev[(int)symbol.gtrsym] = true;
                nextLev[(int)symbol.leqsym] = true; nextLev[(int)symbol.geqsym] = true;
                expressin(ref nextLev,ref ptx,lev);
                if(!(sym==symbol.eqlsym||sym==symbol.neqsym||sym==symbol.lssym||sym==symbol.gtrsym||
                    sym==symbol.leqsym||sym==symbol.geqsym))Error(20);
                else
                {
                    relop = sym;
                    getsym();
                    expressin(ref fsys,ref ptx,lev);
                    switch (relop)
                    {
                        case symbol.eqlsym:gen(fct.opr,0,8);break;
                        case symbol.neqsym: gen(fct.opr, 0, 9); break;
                        case symbol.lssym: gen(fct.opr, 0, 10); break;
                        case symbol.geqsym: gen(fct.opr, 0, 11); break;
                        case symbol.gtrsym: gen(fct.opr, 0, 12); break;
                        case symbol.leqsym: gen(fct.opr, 0, 13); break;
                    }
                }
            }
        }

        /// <summary>
        /// 语句处理
        /// </summary>
        void statement(ref bool[]fsys,ref int ptx,int plev)
        {
            bool[] nextLev = new bool[symnum];
            int i, cx1, cx2;
           // print(fsys);
            if(sym == symbol.ident)//赋值语句
            {
                i = Position(id,ptx);
                if (i == 0) Error(11);//不在表中
                else
                {
                    if (table[i].kind != objecttyp.variable)//不可向非变量赋值
                    {
                        Error(12); i = 0;
                    }
                    else
                    {
                        getsym();
                        if (sym == symbol.becomesym) getsym();
                        else Error(13);
                        memcpy(ref nextLev, fsys);
                        expressin(ref nextLev,ref ptx,plev);
                        //将结果保存在栈顶
                        if (i != 0) gen(fct.sto, plev - table[i].level, table[i].addr);
                    }
                }
               
                
            }
            else if(sym == symbol.readsym)//读操作
            {
                getsym();
                if (sym == symbol.lparensym)
                {
                    do
                    {
                        getsym();
                        if (sym == symbol.ident)
                        {
                            i = Position(id,ptx);
                            if (i == 0) Error(11);
                            else if (table[i].kind != objecttyp.variable) { Error(12); i = 0; }
                            if (i != 0) gen(fct.red, plev - table[i].level, table[i].addr);
                        }
                        else { i = 0; Error(4); 
                        }
                        //if (i == 0) Error(11);
                        getsym();
                    } while (sym == symbol.commasym);
                }
                else Error(40);
                if (sym != symbol.rparensym)
                { Error(22);
                while (!inset((int)sym, ref fsys))
                { int r = getsym();
                if (r == -1) break;
                }
                }
                else getsym();
            }
            //<写语句> ::= write'('<标识符>{,<标识符>}')'
            else if(sym == symbol.writesym)
            {
                getsym();
                if (sym == symbol.lparensym)
                {
                    do
                    {
                        getsym();
                        i = Position(id,ptx);
                        if (i == 0) Error(11);//标识符未说明
                        else
                        {
                            int val = table[i].addr;
                            gen(fct.wrt, 0, 0);
                           
                        }
                        getsym();
                    } while (sym == symbol.commasym);
                    if (sym != symbol.rparensym)
                    {
                        Error(22);
                    }
                    else getsym();
                }
                else Error(40);//应为左括号
            }
                //<过程调用语句> ::= call<标识符>
            else if(sym == symbol.callsym)
            {
                getsym();
                if (sym != symbol.ident) Error(14);//call 后应为标识符
                else
                {
                    i = Position(id,ptx);
                    if (i == 0) Error(11);
                    else
                    {
                        if (table[i].kind == objecttyp.procedure)
                            gen(fct.cal, plev - table[i].level, table[i].addr);
                        else Error(15);
                    }
                    getsym();
                }
               
            }
            //<条件语句> ::= if<条件>then<语句>[else<语句>]
            else if(sym == symbol.ifsym)
            {
                getsym();
                memcpy(ref nextLev, fsys);
                nextLev[(int)symbol.thensym] = true; nextLev[(int)symbol.dosym] = true;
                condition(ref nextLev,ref ptx,plev);
                if (sym == symbol.thensym) getsym();
                else Error(16);
                cx1 = cx;/* 保存当前指令地址 */
                gen(fct.jpc, 0, 0);//条件转移到指令地址,跳转地址暂时写0
                statement(ref fsys,ref ptx,plev);
                code[cx1].a = cx;/* 经statement处理后，cx为then后语句执行完的位置，
                                  * 它正是前面未定的跳转地址，此时进行回填 */  
                if(sym == symbol.elsesym)
                {
                    getsym();
                    cx1 = cx;
                    gen(fct.jpc, 0, 0);
                    statement(ref fsys,ref ptx,plev);
                }
            }
            //<复合语句> ::= begin<语句>{;<语句>}end
            else if(sym == symbol.beginsym)
            {
                getsym();
                memcpy(ref nextLev, fsys);
                nextLev[(int)symbol.semicolonsym] = true; nextLev[(int)symbol.endsym] = true;
                statement(ref nextLev,ref ptx,plev);
                while (sym == symbol.semicolonsym || inset((int)sym, ref statbegsys))
                {
                    if (sym == symbol.semicolonsym) getsym();
                    else Error(10);//语句之间漏分号
                    statement(ref nextLev, ref ptx,plev);
                }
                if (sym == symbol.endsym) getsym();
                else Error(17);
            }
            //<当型循环语句> ::= while<条件>do<语句>
            else if(sym == symbol.whilesym)
            {
                cx1 = cx;//保存条件判断的位置
                getsym();
                memcpy(ref nextLev, fsys);
                nextLev[(int)symbol.dosym] = true;
                condition(ref nextLev,ref ptx,plev);
                cx2 = cx;//循环体的结束的最后位置
                gen(fct.jpc, 0, 0);
                if (sym != symbol.dosym) Error(18);
                else getsym();
                statement(ref fsys,ref ptx, plev);
                gen(fct.jmp, 0, cx1);//跳到循环条件判断位置
                code[cx2].a = cx;//回填跳出循环的地址
            }
            memset(ref nextLev);
            test(ref fsys,ref nextLev,19);//测试语句后的符号是否正确
        }

        /*主体  block部分
         * plev:    当前分程序所在层  
         * tx:     符号表当前尾指针  ,全局变量
         * fsys:   当前模块后继符号集合
         * <分程序> ::= [<常量说明部分>][变量说明部分>][<过程说明部分>]<语句>
         */
        void Block(int plev, int tx, ref bool [] fsys)
        {
            //int i;
            bool[] nextLev = new bool[symnum];
            int dx0;//数据分配索引，即数据分配的相对地址
            int tx0;//初始化时的符号表的索引tx
            int cx0;//保留初始cx
            dx0 = 3;//三个空间用于存放静态区、动态链、返回地址
            tx0 = tx;//本层标识符的初始位置
            table[tx].addr = cx;//code的下标指针
            gen(fct.jmp, 0, 0);//跳转地址暂时填0
            if (plev > levmax) Error(32);
            do
            {//<常量说明部分> ::= const<常量定义>{,<常量定义>};
                if(sym == symbol.constsym)
                {
                    getsym();
                    do
                    {
                        constdeclaration(ref tx,plev,ref dx0);
                        while(sym == symbol.commasym)
                        {
                            getsym();
                            constdeclaration(ref tx,plev,ref dx0);
                        }
                        if (sym == symbol.semicolonsym) getsym();
                        else Error(5);
                    } while (sym == symbol.ident);
                }
                //<变量说明部分>::= var<标识符>{,<标识符>};
                if(sym == symbol.varsym)
                {
                    getsym();
                        vardeclaration(ref tx,plev,ref dx0);
                        while(sym == symbol.commasym)
                        {
                            getsym();
                            vardeclaration(ref tx,plev,ref dx0);
                        }
                        if (sym == symbol.semicolonsym) getsym();
                        else Error(5);
                }
                //<过程说明部分> ::= <过程首部><分程序>;{<过程说明部分>}
                while(sym == symbol.procsym)
                {
                    getsym();
                    if (sym == symbol.ident)
                    {
                        Enterr(objecttyp.procedure,ref tx,plev,ref dx0);
                        getsym();
                    }
                    else Error(4);//procedure后应为标识符
                    if (sym == symbol.semicolonsym) getsym();
                    else Error(5);//漏掉分号
                    memcpy(ref nextLev, fsys);
                    nextLev[(int) symbol.semicolonsym]=true;
                    Block(plev + 1,tx, ref nextLev);
                    if (sym == symbol.semicolonsym)
                    {
                        getsym();
                        memcpy(ref nextLev, statbegsys); 
                        nextLev[(int) symbol.ident]=true;
                        nextLev[(int)symbol.procsym]=true;
                        test(ref nextLev, ref fsys, 6);
                    }
                    else Error(5);
                }
                memcpy(ref nextLev, statbegsys);
                nextLev[(int)symbol.ident] = true;
                test(ref nextLev, ref declbegsys, 7);//应为语句
            } while (inset((int)sym, ref declbegsys));//直到没有声明符号
            code[table[tx0].addr].a = cx;//回填语句开始地址
            table[tx0].addr = cx;//当前过程代码开始地址
            table[tx0].size = dx0;
            cx0 = cx;
            gen(fct.ini, 0, dx0);

            this.Refresh();
            memcpy(ref nextLev, fsys);
            nextLev[(int)symbol.semicolonsym] = true; nextLev[(int)symbol.endsym] = true;
            statement(ref nextLev, ref tx, plev);
            gen(fct.opr, 0, 0); /* 每个过程出口都要使用的释放数据段指令 */
            memset(ref nextLev);
            test(ref fsys, ref nextLev, 8);
            listcode(cx0);

            //输出符号表供调试
            for(int j = 1;j<=tx;j++)
            {
                switch (table[j].kind)
                {
                    case objecttyp.constant:
                        Console.WriteLine("{0,2} const {1,7} val={2,5}", j, table[j].name,table[j].val);
                        resultBox.Text = resultBox.Text + "\r\n" + j + "  const  " + table[j].name + "  " + table[j].val;
                        break;
                    case objecttyp.variable:
                        Console.Write("{0,2} var {1,7}  ", j, table[j].name);
                        Console.WriteLine("lev={0,2},addr={1,3}", table[j].level, table[j].addr);
                        resultBox.Text = resultBox.Text + "\r\n" + j + "  var  " + table[j].name + "  " ;
                        resultBox.Text = resultBox.Text + table[j].level + "  " + table[j].addr;
                        break;
                    case objecttyp.procedure:
                        Console.Write("{0,2} proc {1,7}  ", j, table[j].name);
                        Console.WriteLine("lev={0,2},addr={1,3},size={2,3}", table[j].level, table[j].addr, table[j].size);
                        resultBox.Text = resultBox.Text + "\r\n" + j + "  proc  " + table[j].name + "  ";
                        resultBox.Text = resultBox.Text + table[j].level + "  " + table[j].addr + "  " + table[j].size;
                        break;
                }
            }
            resultBox.Text = resultBox.Text + "\r\n";

        }
        /// <summary>
        /// P-code解释执行程序
        /// </summary>
        void Inteprete()
        {
            int p = 0;//指令指针program-
            int b = 1;//base-指令基址
            int t = 0;//top-register栈顶指针
            instruction i;//instruction register
            int[] s = new int[stacksize];//栈 存放数据
            instruBox.Text = instruBox.Text+"Start!\r\n";
            s[0] = 0; s[1] = 0; s[2] = 0; s[3] = 0;
            do
            {
                i = code[p];
                p++;
                switch(i.f)
                {
                    case fct.lit://取常量a地址到栈顶
                        t++;
                        s[t] = i.a;
                        break;
                    case fct.opr://运算 依值而定
                        switch(i.a)
                        {
                            case 0://函数调用结束后返回
                                t = b - 1;
                                p = s[t + 3];
                                b = s[t + 2];
                                break;
                            case 1://栈顶元素取反
                                s[t] = -s[t];
                                break;
                            case 2://次栈顶加栈顶，退2栈进和
                                t = t - 1;
                                s[t] = s[t] + s[t + 1];
                                break;
                            case 3://相减
                                t--;
                                s[t] = s[t] - s[t + 1];
                                break;
                            case 4://乘法
                                t--;
                                s[t] = s[t] * s[t + 1];
                                break;
                            case 5://除法
                                t--;
                                s[t] = s[t] / s[t + 1];
                                break;
                            case 6://判断栈顶元素奇偶
                                s[t] = s[t] % 2;break;
                            case 8://判断次栈顶与栈顶是否相等
                                t--;
                                s[t] = (s[t] == s[t + 1])?1:0;
                                break;
                            case 9://是否不等
                                t--;
                                s[t] = (s[t] != s[t + 1]) ? 1 : 0;
                                break;
                            case 10://次栈顶是否小于栈顶
                                t--;
                                s[t] = (s[t] < s[t + 1]) ? 1 : 0;
                                break;
                            case 11://次栈顶是否大于等于栈顶
                                t--;
                                s[t] = (s[t] >= s[t + 1]) ? 1 : 0;
                                break;
                            case 12://次栈顶是否大于栈顶
                                t--;
                                s[t] = (s[t] > s[t + 1]) ? 1 : 0;
                                break;
                            case 13://次栈顶是否小于等于
                                t--;
                                s[t] = (s[t] <= s[t + 1]) ? 1 : 0;
                                break;
                            case 14://输出栈顶值
                                instruBox.Text = instruBox.Text + s[t];
                                t--;
                                break;
                            case 15://输出换行符
                                instruBox.Text = instruBox.Text + "\r\n";
                                break;
                            case 16://读入一个输入到栈顶
                                t++;
                                instruBox.Text = instruBox.Text + "input:";
                                break;


                        }
                        break;
                    case fct.lod://取相对当前过程的数据基地址为a的内存的值到栈顶
                        t++;
                        s[t] = s[Base(i.l, ref s, b) + i.a];
                        break;
                    case fct.sto://栈顶值存到相对当前过程数据基地址为a的内存
                        s[Base(i.l, ref s, b) + i.a] = s[t];
                        t--;
                        break;
                    case fct.cal://调用
                        s[t+1]=Base(i.l,ref s,b);//父过程基地址入栈-静态链
                        s[t+2]=b;//本过程基地址入栈-动态链
                        s[t+3]=p;//当前指令指针入栈（返回地址）
                        b=t+1;//改变基地址指针为心过程的基地址
                        p=i.a;//跳转
                        break;
                    case fct.ini://为被调用的过程开辟a个单元的数据区
                        t = t+i.a;
                        break;
                    case fct.jmp://直接跳转
                        p=i.a;
                        break;
                    case fct.jpc://条件跳转
                        if(s[t]==0)p=i.a;
                        t--;
                     break;
                    case fct.red:
                        instruBox.Text = instruBox.Text + "input:\n";
                                break;
                    case fct.wrt:
                                instruBox.Text = instruBox.Text + s[t] + "\r\n";
                                t++;
                                break;
                }
            } while (p != 0);
            instruBox.Text=instruBox.Text+"End!\r\n";
        }
        /// <summary>
        /// 求上面l层过程的基址
        /// </summary>
        /// <param name="l"></param>
        /// <param name="s"></param>
        /// <param name="b"></param>
        int Base(int l,ref int[] s,int b)
        {
            int b1 = b;
            while(l>0)
            {
                b1 = s[b1];
                l--;
            }
            return b1;
        }
    }
}
