using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleBonifacio;
using TeleBonifacio.gen;

namespace ATCAtualizeitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            INI cINI = new INI();
            string URL = cINI.ReadString("FTP", "URL", "");
            string user = Cripto.Decrypt(cINI.ReadString("FTP", "user", ""));
            string senha = Cripto.Decrypt(cINI.ReadString("FTP", "pass", ""));
            FTP cFPT = new FTP(URL, user, senha);
            // Apagar o arquivo localmente se houve
            // Baixar o novo arquivo
            if (cFPT.Download(@"/public_html/public/entregas/TeleBonifacio.exe"))
            {

            }
            Environment.Exit(0);
        }
    }
}
