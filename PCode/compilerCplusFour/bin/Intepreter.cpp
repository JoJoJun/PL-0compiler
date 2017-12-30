/*ר�ŵĽ��ͳ���C#������������Pcode.txt��bin�ļ����£�����cpp����ͳһ�ļ�����
�������н��ͳ�����ΪC# winform�޷�ʵ�ֶ������*/
#include<cstdio>
#include<iostream>
#include<cstring>
#include<string>
#include<stdlib.h>
#include<fstream>
using namespace std;
const int cxmax = 200;//���������������
const int stacksize = 500;//����ʱ�����������ջΪ500
const int symnum = 37;//������
const int fucnum = 11;//ָ����
int s[stacksize];
string mnemonic[fucnum];
/* ���� */
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

/* ���������ṹ */
struct instruction
{
    fct f; /* ���������ָ�� */
	 int l;      /* ���ò���������Ĳ�β� */
	 int a;      /* ����f�Ĳ�ͬ����ͬ */
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
    int b = 1; /* ָ���ַ */
    int t = 0; /* ջ��ָ�� */
    instruction i;
    printf("Start inteprete!\n");
     s[0] = 0; /* s[0]���� */
    s[1] = 0; /* �������������ϵ��Ԫ����Ϊ0 */
    s[2] = 0;
    s[3] = 0;
    do {
        i = code[p];    /* ����ǰָ�� */
        p = p + 1;
        switch (i.f)
        {
            case lit:   /* ������a��ֵȡ��ջ�� */
                t = t + 1;
                s[t] = i.a;
                break;
            case opr:   /* ��ѧ���߼����� */
                switch (i.a)
                {
                    case 0:  /* �������ý����󷵻� */
                        t = b - 1;
                        p = s[t + 3];
                        b = s[t + 2];
                        break;
                    case 1: /* ջ��Ԫ��ȡ�� */
                        s[t] = - s[t];
                        break;
                    case 2: /* ��ջ�������ջ���������ջԪ�أ����ֵ��ջ */
                        t = t - 1;
                        s[t] = s[t] + s[t + 1];
                        break;
                    case 3:/* ��ջ�����ȥջ���� */
                        t = t - 1;
                        s[t] = s[t] - s[t + 1];
                        break;
                    case 4:/* ��ջ�������ջ���� */
                        t = t - 1;
                        s[t] = s[t] * s[t + 1];
                        break;
                    case 5:/* ��ջ�������ջ���� */
                        t = t - 1;
                        s[t] = s[t] / s[t + 1];
                        break;
                    case 6:/* ջ��Ԫ�ص���ż�ж� */
                        s[t] = s[t] % 2;
                        break;
                    case 8:/* ��ջ������ջ�����Ƿ���� */
                        t = t - 1;
                        s[t] = (s[t] == s[t + 1]);
                        break;
                    case 9:/* ��ջ������ջ�����Ƿ񲻵� */
                        t = t - 1;
                        s[t] = (s[t] != s[t + 1]);
                        break;
                    case 10:/* ��ջ�����Ƿ�С��ջ���� */
                        t = t - 1;
                        s[t] = (s[t] < s[t + 1]);
                        break;
                    case 11:/* ��ջ�����Ƿ���ڵ���ջ���� */
                        t = t - 1;
                        s[t] = (s[t] >= s[t + 1]);
                        break;
                    case 12:/* ��ջ�����Ƿ����ջ���� */
                        t = t - 1;
                        s[t] = (s[t] > s[t + 1]);
                        break;
                    case 13: /* ��ջ�����Ƿ�С�ڵ���ջ���� */
                        t = t - 1;
                        s[t] = (s[t] <= s[t + 1]);
                        break;
                    case 14:/* ջ��ֵ��� */
                        printf("%d", s[t]);
                        t = t - 1;
                        break;
                    case 15:/* ������з� */
                        printf("\n");
                        break;
                    case 16:/* ����һ����������ջ�� */
                        t = t + 1;
                        printf("����һ���޷�������?");
                        int number;
                        scanf("%d",&number);
                        s[t]=number;
                        //scanf("%d", &(s[t]));
                        break;
                }
                break;
            case lod:   /* ȡ��Ե�ǰ���̵����ݻ���ַΪa���ڴ��ֵ��ջ�� */
                t = t + 1;
                s[t] = s[base(i.l,s,b) + i.a];
                break;
            case sto:   /* ջ����ֵ�浽��Ե�ǰ���̵����ݻ���ַΪa���ڴ� */
                s[base(i.l, s, b) + i.a] = s[t];
                t = t - 1;
                break;
            case cal:   /* �����ӹ��� */
                s[t + 1] = base(i.l, s, b); /* �������̻���ַ��ջ����������̬�� */
                s[t + 2] = b;   /* �������̻���ַ��ջ����������̬�� */
                s[t + 3] = p;   /* ����ǰָ��ָ����ջ�������淵�ص�ַ */
                b = t + 1;  /* �ı����ַָ��ֵΪ�¹��̵Ļ���ַ */
                p = i.a;    /* ��ת */
                break;
            case ini:   /* ������ջ��Ϊ�����õĹ��̿���a����Ԫ�������� */
                t = t + i.a;
                break;
            case jmp:   /* ֱ����ת */
                p = i.a;
                break;
            case jpc:   /* ������ת */
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
