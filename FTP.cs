using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeleBonifacio
{
    public class FTP
    {
        private int _tamanhoConteudo = 0;
        private int Tot = 0;
        private string ftpIPServidor = "";
        private string ftpUsuarioID = "";
        private string ftpSenha = "";
        private string Erro = "";
        private ProgressBar ProgressBar1= null;
        public long bytesReceived = 0;
        private string Mensagem = "";

        public FTP(string ftpIPServidor, string ftpUsuarioID, string ftpSenha)
        {
            this.ftpIPServidor = ftpIPServidor;
            this.ftpUsuarioID = ftpUsuarioID;
            this.ftpSenha = ftpSenha;
        }

        public FTP()
        {

        }

        public int LerVersaoDoFtp()
        {
            string caminhoArquivo = "/public_html/public/entregas/versao.txt";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.ftpIPServidor + caminhoArquivo));
            request.Credentials = new NetworkCredential(this.ftpUsuarioID, this.ftpSenha);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UsePassive = true;
            FtpWebResponse response;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new Exception("Erro ao conectar ao servidor FTP: " + ex.Message);
            }
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string info = reader.ReadToEnd();
            string[] lines = info.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string versaoTexto = lines[0];
            if (lines.Length > 1)
            {
                this.Mensagem = lines[1];
            }
            reader.Close();
            responseStream.Close();
            response.Close();
            int versaoNumero = int.Parse(versaoTexto.Replace(".", ""));
            return versaoNumero;
        }

        public string retMensagem()
        {
            return this.Mensagem;
        }

        //public int LerVersaoDoFtp()
        //{
        //    string caminhoArquivo = "/public_html/public/entregas/versao.txt";
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + this.ftpIPServidor + caminhoArquivo));
        //    request.Credentials = new NetworkCredential(this.ftpUsuarioID, this.ftpSenha);
        //    request.Method = WebRequestMethods.Ftp.DownloadFile;
        //    request.UsePassive = true;
        //    FtpWebResponse response;
        //    try
        //    {
        //        response = (FtpWebResponse)request.GetResponse();
        //    }
        //    catch (WebException ex)
        //    {
        //        throw new Exception("Erro ao conectar ao servidor FTP: " + ex.Message);
        //    }
        //    Stream responseStream = response.GetResponseStream();
        //    StreamReader reader = new StreamReader(responseStream);
        //    string versaoTexto = reader.ReadToEnd();
        //    reader.Close();
        //    responseStream.Close();
        //    response.Close();
        //    int versaoNumero = int.Parse(versaoTexto.Replace(".",""));
        //    return versaoNumero;
        //}

        public string getErro()
        {
            return this.Erro;
        }

        public bool Download(string nmPasta, string nomeArquivoLocal, BackgroundWorker worker, long tamanhoTotalArquivo)
        {
            string Suri = "ftp://" + this.ftpIPServidor + @"/" + nmPasta + @"/" + nomeArquivoLocal;
            FtpWebRequest requisicaoFTP;
            requisicaoFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(Suri));
            requisicaoFTP.Credentials = new NetworkCredential(this.ftpUsuarioID, this.ftpSenha);
            requisicaoFTP.KeepAlive = false;
            requisicaoFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            requisicaoFTP.UseBinary = true;
            bool ret = false;
            try
            {
                FtpWebResponse respDown = (FtpWebResponse)requisicaoFTP.GetResponse();
                // tamanhoTotalArquivo = respDown.ContentLength;
                Stream responseStream = respDown.GetResponseStream();
                using (FileStream fileStream = File.Create(nomeArquivoLocal))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;                    
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        bytesReceived += bytesRead;
                        if (tamanhoTotalArquivo>0)
                        {                            
                            int progresso = (int)((bytesReceived * 100) / tamanhoTotalArquivo);
                            worker.ReportProgress(progresso);
                        }
                    }
                }
                respDown.Close();
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }
            return ret;
        }

    }
}


