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
        private string arquivoDestino = @"C:\Entregas\TeleBonifacio.exe";

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
            Loga("Acionando programa em "+ arquivoDestino);
            Process.Start(arquivoDestino);
            Loga("Fechando atualizador");
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
            long tamanhoTotalArquivo = tamanho.Length > 0 ? long.Parse(tamanho) : 0;
            if (cFPT.Download(Pasta, nmPrograma, worker, tamanhoTotalArquivo))
            {
                long bytesReceived = cFPT.bytesReceived;
                cINI.WriteString("Config", "tamanho", bytesReceived.ToString());
                string pastaAtual = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string pastaPrograma = Path.Combine(pastaAtual, "..");
                string PastaDoEntregas = cINI.ReadString("Atualizacao", "Programa", "");                
                string pastaBackup = Path.Combine(PastaDoEntregas, "Bak");
                Loga("pastaAtual : " + pastaAtual);
                Loga("pastaPrograma : " + pastaPrograma);
                Loga("pastaBackup : " + pastaBackup);
                if (!Directory.Exists(pastaBackup))
                {
                    Directory.CreateDirectory(pastaBackup);
                }
                string arquivoLocal = Path.Combine(pastaAtual, nmPrograma);
                string PastaDestino = cINI.ReadString("Config", "Programa", "");
                string arquivoBackup = Path.Combine(pastaBackup, nmPrograma);
                // arquivoDestino = Path.Combine(PastaDestino, nmPrograma);
                Loga("arquivoLocal : " + arquivoLocal);
                Loga("arquivoDestino : " + arquivoDestino);
                Loga("arquivoBackup : " + arquivoBackup);
                int versaoFtp = cFPT.LerVersaoDoFtp();
                string versaoNovaStr = $"{versaoFtp / 100}.{(versaoFtp / 10) % 10}.{versaoFtp % 10}";
                string VersaoAnterior = cINI.ReadString("Config", "VersaoAtual", "");
                if (VersaoAnterior.Length > 0)
                {
                    cINI.WriteString("Atualizador", "VersaoAnterior", versaoNovaStr);
                }
                cINI.WriteString("Config", "VersaoAtual", versaoNovaStr);
                File.Copy(arquivoDestino, arquivoBackup, true);
                File.Copy(arquivoLocal, arquivoDestino, true);
                System.Threading.Thread.Sleep(100);
                Loga("Executar programa em " + arquivoDestino);
                // Process.Start(arquivoDestino);
                this.Invoke(new MethodInvoker(delegate { this.WindowState = FormWindowState.Minimized; }));
                System.Threading.Thread.Sleep(1000);
                e.Result = true;
            }
            else
            {
                e.Result = false;
            }
        }

        //private bool EstaRodandoNoVisualStudio()
        //{
        //    string processoAtual = Process.GetCurrentProcess().ProcessName.ToLower();
        //    return processoAtual.Contains("devenv");
        //}

        private void Loga(string message)
        {
            string logFilePath = @"C:\Entregas\Atualizador.txt";
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

    }
}
