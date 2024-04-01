using System;
using System.IO;
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
            string nmPrograma = "TeleBonifacio.exe";
            string Pasta = @"/public_html/public/entregas/";
            if (cFPT.Download(Pasta, nmPrograma))
            {
                string pastaprog = cINI.ReadString("Config", "Programa", "");
                string pastaBak = pastaprog + @"\Bak";
                if (!Directory.Exists(pastaBak))
                {
                    Directory.CreateDirectory(pastaBak);
                }
                string pastaLocal = AppDomain.CurrentDomain.BaseDirectory;
                string arquivoLocal = Path.Combine(pastaLocal, nmPrograma);
                string arquivoDestino = Path.Combine(pastaprog, nmPrograma);
                string nmArqBak = pastaprog + @"\Bak" + @"\" + nmPrograma;
                File.Copy(arquivoDestino, nmArqBak, true);
                File.Copy(arquivoLocal, arquivoDestino, true);
            }
            Environment.Exit(0);
        }
    }
}
