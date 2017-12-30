/*专门的解释程序，C#编译程序会生成Pcode.txt与bin文件夹下，将此cpp放在统一文件夹下
即可运行解释程序。因为C# winform无法实现多次输入*/
#include<cstdio>
#include<iostream>
#include<cstring>
#include<string>
#include<stdlib.h>
#include<fstream>
using namespace std;
const int cxmax = 200;//最多的虚拟机代码数
const int stacksize = 500;//运行时最多允许数据栈为500
const int symnum = 37;//符号数
const int fucnum = 11;//指令数
int s[stacksize];
string mnemonic[fucnum];
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
enum fct {
	lit, opr, lod,
	sto, cal, ini,
	jmp, jpc,red,wrt
};

/* 虚拟机代码结构 */
struct instruction
{
    fct f; /* 虚拟机代码指令 */
	 int l;      /* 引用层与声明层的层次差 */
	 int a;      /* 根据f的不同而不同 */
};

instruction code[cxmax];
int turn(string str)
{
    if(str=="LIT")return 0;
    else if(str=="OPR")return 1;
    else if(str=="LOD")return 2;
    else if(str=="STO")return 3;
    else if(str=="CAL")return 4;
    else if(str=="INT")return 5;
    else if(str=="JMP")return 6;
    else if(str=="JPC")return 7;
    else if(str=="RED")return 8;
    else if(str=="WRT")return 9;
    else return -1;

}
int cx;
int base(int l,int *s,int b)
{
    int b1=b;
    while(l>0)
    {
        b1 = s[b1];
        l--;
    }
    return b1;
}
void Interprete()
{
    int p = 0;
    int b = 1; /* 指令基址 */
    int t = 0; /* 栈顶指针 */
    instruction i;
    printf("Start inteprete!\n");
     s[0] = 0; /* s[0]不用 */
    s[1] = 0; /* 主程序的三个联系单元均置为0 */
    s[2] = 0;
    s[3] = 0;
    do {
        i = code[p];    /* 读当前指令 */
        p = p + 1;
        switch (i.f)
        {
            case lit:   /* 将常量a的值取到栈顶 */
                t = t + 1;
                s[t] = i.a;
                break;
            case opr:   /* 数学、逻辑运算 */
                switch (i.a)
                {
                    case 0:  /* 函数调用结束后返回 */
                        t = b - 1;
                        p = s[t + 3];
                        b = s[t + 2];
                        break;
                    case 1: /* 栈顶元素取反 */
                        s[t] = - s[t];
                        break;
                    case 2: /* 次栈顶项加上栈顶项，退两个栈元素，相加值进栈 */
                        t = t - 1;
                        s[t] = s[t] + s[t + 1];
                        break;
                    case 3:/* 次栈顶项减去栈顶项 */
                        t = t - 1;
                        s[t] = s[t] - s[t + 1];
                        break;
                    case 4:/* 次栈顶项乘以栈顶项 */
                        t = t - 1;
                        s[t] = s[t] * s[t + 1];
                        break;
                    case 5:/* 次栈顶项除以栈顶项 */
                        t = t - 1;
                        s[t] = s[t] / s[t + 1];
                        break;
                    case 6:/* 栈顶元素的奇偶判断 */
                        s[t] = s[t] % 2;
                        break;
                    case 8:/* 次栈顶项与栈顶项是否相等 */
                        t = t - 1;
                        s[t] = (s[t] == s[t + 1]);
                        break;
                    case 9:/* 次栈顶项与栈顶项是否不等 */
                        t = t - 1;
                        s[t] = (s[t] != s[t + 1]);
                        break;
                    case 10:/* 次栈顶项是否小于栈顶项 */
                        t = t - 1;
                        s[t] = (s[t] < s[t + 1]);
                        break;
                    case 11:/* 次栈顶项是否大于等于栈顶项 */
                        t = t - 1;
                        s[t] = (s[t] >= s[t + 1]);
                        break;
                    case 12:/* 次栈顶项是否大于栈顶项 */
                        t = t - 1;
                        s[t] = (s[t] > s[t + 1]);
                        break;
                    case 13: /* 次栈顶项是否小于等于栈顶项 */
                        t = t - 1;
                        s[t] = (s[t] <= s[t + 1]);
                        break;
                    case 14:/* 栈顶值输出 */
                        printf("%d", s[t]);
                        t = t - 1;
                        break;
                    case 15:/* 输出换行符 */
                        printf("\n");
                        break;
                    case 16:/* 读入一个输入置于栈顶 */
                        t = t + 1;
                        printf("输入一个无符号整数?");
                        int number;
                        scanf("%d",&number);
                        s[t]=number;
                        //scanf("%d", &(s[t]));
                        break;
                }
                break;
            case lod:   /* 取相对当前过程的数据基地址为a的内存的值到栈顶 */
                t = t + 1;
                s[t] = s[base(i.l,s,b) + i.a];
                break;
            case sto:   /* 栈顶的值存到相对当前过程的数据基地址为a的内存 */
                s[base(i.l, s, b) + i.a] = s[t];
                t = t - 1;
                break;
            case cal:   /* 调用子过程 */
                s[t + 1] = base(i.l, s, b); /* 将父过程基地址入栈，即建立静态链 */
                s[t + 2] = b;   /* 将本过程基地址入栈，即建立动态链 */
                s[t + 3] = p;   /* 将当前指令指针入栈，即保存返回地址 */
                b = t + 1;  /* 改变基地址指针值为新过程的基地址 */
                p = i.a;    /* 跳转 */
                break;
            case ini:   /* 在数据栈中为被调用的过程开辟a个单元的数据区 */
                t = t + i.a;
                break;
            case jmp:   /* 直接跳转 */
                p = i.a;
                break;
            case jpc:   /* 条件跳转 */
                if (s[t] == 0)
                    p = i.a;
                t = t - 1;
                break;
            case red:
                 t = t + 1;
                    printf("?\n");
                    int number;
                    scanf("%d", &(s[base(i.l, s, b) + i.a]));
                        break;
        }
    } while (p != 0);
    printf("End!\n");
}
void Readin()
{
    ifstream fin("Pcode.txt");
    string s; int cx;
    fin>>cx;
    for(int i =0;i<cx;i++)
    {    fin>>s>>code[i].l>>code[i].a;
        //cout << "Read from file: " << s <<"\t"<<code[i].l<<"\t"<<code[i].a<< endl;
        code[i].f=(fct)turn(s);
    }

}
int main()
{
    Readin();
    printf("!\n");
    //int a;
//scanf("%d",&a);
    Interprete();
    system("pause");
    return 0;
}
