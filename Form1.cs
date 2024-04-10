using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TeleBonifacio;
using TeleBonifacio.gen;

namespace ATCAtualizeitor
{
    public partial class Form1 : Form
    {
        private FTP cFPT;
        private BackgroundWorker worker;

        public Form1()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int value = e.ProgressPercentage;
            if (value >= progressBar1.Minimum && value <= progressBar1.Maximum)
            {
                progressBar1.Value = value;
                this.Text = "Atualizador " + value.ToString() + " %";
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            INI cINI = new INI();
            string URL = cINI.ReadString("FTP", "URL", "");
            string user = Cripto.Decrypt(cINI.ReadString("FTP", "user", ""));
            string senha = Cripto.Decrypt(cINI.ReadString("FTP", "pass", ""));
            cFPT = new FTP(URL, user, senha);
            string nmPrograma = "TeleBonifacio.exe";
            string Pasta = @"/public_html/public/entregas/";
            string tamanho = cINI.ReadString("Config", "tamanho", "");
            long tamanhoTotalArquivo = 0;
            if (tamanho.Length>0)
            {
                tamanhoTotalArquivo = long.Parse(tamanho);
            }            
            if (cFPT.Download(Pasta, nmPrograma, worker, tamanhoTotalArquivo))
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
                Process.Start(arquivoDestino);
                this.Invoke(new MethodInvoker(delegate { this.WindowState = FormWindowState.Minimized; }));
                long bytesReceived = cFPT.bytesReceived;
                cINI.WriteString("Config", "tamanho", bytesReceived.ToString());
                int versaoFtp = cFPT.LerVersaoDoFtp();
                string versaoNovaStr = $"{versaoFtp / 100}.{(versaoFtp / 10) % 10}.{versaoFtp % 10}";
                string VersaoAnterior = cINI.ReadString("Config", "VersaoAtual", "");
                if (VersaoAnterior.Length>0)
                {
                    cINI.WriteString("Config", "VersaoAnterior", versaoNovaStr);
                }
                cINI.WriteString("Config", "VersaoAtual", versaoNovaStr);
                System.Threading.Thread.Sleep(1000);
                e.Result = true;
            }
            else
            {
                e.Result = false;
            }
        }

    }
}
