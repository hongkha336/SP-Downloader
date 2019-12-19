using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SPADownloader
{
    public partial class frmMain : Form
    {
        private const String SRC_PARTTERN = "src=\"";
        private const String HREF_PARTERN = "href=\"";
        private const String DATA_IMAGE = "data-img=\"";
        private const String DATA_BG = "data-background=\"";
        private const String SRCSET_PARTERN = "srcset=\"";
        private const String URL_PARTERN = "url(\"";
        private const String LAZY_PARTERN = "data-lazy-interchange=\"";

        private String MAIN_CONTENT = String.Empty;
        private Boolean isDownloadResource = true;
        private List<String> _org = null;
        private String CRIGHT = "<!-- THIS PAGE WAS DOWNLOAD BY SP DOWNLOADER -->\n  <!-- hongkha336@gmail.com -->\n";
        private static Uri BaseUri = null;
     
        public frmMain()
        {
            InitializeComponent();
        }


        private async void analysiswithHttpClientAsync()
        {
            //var client = new RestClient("https://www.plannedparenthood.org/");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.GET);
            //IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);

            var client = new RestClient(txtURL.Text);
            var request = new RestRequest( Method.GET);
            IRestResponse response = client.Execute(request);
            MAIN_CONTENT = response.Content;
        }

      

        private void analysis()
        {
            String encodeType = String.Empty;
            comboBox1.Invoke((MethodInvoker)(() => encodeType = comboBox1.Text.Trim()));
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = false));
            lbStatus.Invoke((MethodInvoker)(() => lbStatus.Text = "Status: Download " + txtURL.Text));
            try
            {
                using (var wb = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    wb.UseDefaultCredentials = true;

                    wb.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    var response = wb.DownloadData(txtURL.Text);

                    switch (encodeType)
                    {
                        case "UTF-8":
                            {
                                var htmlCode = Encoding.UTF8.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        case "ASCII":
                            {
                                var htmlCode = Encoding.ASCII.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        case "Unicode":
                            {
                                var htmlCode = Encoding.Unicode.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        default:
                            {
                                var htmlCode = Encoding.Default.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }

                    }

                }
        
            }
            catch
            {
                //analysiswithHttpClientAsync();
                var client = new RestClient(txtURL.Text);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                MAIN_CONTENT = response.Content;
            }
            dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Clear()));
            int i = 0;
            _org = new List<string>();
            scanLazyInterchange();
            scanResource(MAIN_CONTENT, URL_PARTERN);
            scanResource(MAIN_CONTENT, SRC_PARTTERN);
            scanResource(MAIN_CONTENT, HREF_PARTERN);
            scanResource(MAIN_CONTENT, DATA_IMAGE);
            scanResource(MAIN_CONTENT, DATA_BG);
            scanResource(MAIN_CONTENT, SRCSET_PARTERN);
        
            for (i = 0; i < _org.Count; i++)
            {
                dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Add((i + 1).ToString(), _org[i], "Wait")));
            }
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = true));
            lbdownload.Invoke((MethodInvoker)(() =>
               lbdownload.Text = "Download progess: " + 0 + "/" + _org.Count
            ));
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() =>
            {

                analysis();

            });
            t.Start();

        }

       

        private void openFolder(string folderPath)
        {
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }



        private void replaceCommon()
        {
            replaceLazyInterchange();
            MAIN_CONTENT = replaceTagContent(SRC_PARTTERN, MAIN_CONTENT, txtURL.Text);
            MAIN_CONTENT = replaceTagContent(HREF_PARTERN, MAIN_CONTENT, txtURL.Text);
            MAIN_CONTENT = replaceTagContent(DATA_IMAGE, MAIN_CONTENT, txtURL.Text);
            MAIN_CONTENT = replaceTagContent(DATA_BG, MAIN_CONTENT, txtURL.Text);
            MAIN_CONTENT = replaceTagContent(SRCSET_PARTERN, MAIN_CONTENT,txtURL.Text);
            MAIN_CONTENT = replaceTagContent(URL_PARTERN, MAIN_CONTENT, txtURL.Text);
            MessageBox.Show("Download finished");
        }
        private void BtnSaveFile_Click(object sender, EventArgs e)
        {
 
            Thread t = new Thread(() =>
             {




                 analysis();


                 if (!isDownloadResource)
                 {
                     replaceCommon();
                     Common.SaveFile(textBox1.Text, textBox2.Text, MAIN_CONTENT);
                     DialogResult rs = MessageBox.Show("Save " + textBox1.Text + " successfully! Open folder download?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                     if (rs == DialogResult.Yes)
                     {
                         openFolder(textBox2.Text);
                     }
                 }
                 else
                 {

                     // MessageBox.Show("Download resources...");
                     btnSaveFile.Invoke((MethodInvoker)(() => btnSaveFile.Enabled = false));
                     downloadResource();
                     Common.SaveFile(textBox1.Text, textBox2.Text, MAIN_CONTENT);
                     DialogResult rs = MessageBox.Show("Save " + textBox1.Text + " successfully! Open folder download?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                     if (rs == DialogResult.Yes)
                     {
                         openFolder(textBox2.Text);
                     }

                     btnSaveFile.Invoke((MethodInvoker)(() => btnSaveFile.Enabled = true));
                 }
             });

            t.Start();
        }


        private String replaceTagContent(String mSrcPattern, String orgContent, String concatPattern)
        {
            //progress - pattern
            if (concatPattern.EndsWith("/"))
                concatPattern = concatPattern.Substring(0, concatPattern.Length - 1);

            int orgContentLen = orgContent.Length;
            String newContent = String.Empty;
            String srcPattern = mSrcPattern/*"src=\""*/;
            //include https b'c https start with http. Exclude
            String httpPartern = "http";
            while (orgContent.Length > 0)
            {
                int indexOfSrc = orgContent.IndexOf(srcPattern);
                //found index
                if (indexOfSrc != -1)
                {
                    String subContent = orgContent.Substring(0, indexOfSrc + srcPattern.Length);
                    newContent = newContent + subContent;
                    orgContent = orgContent.Substring(subContent.Length, orgContent.Length - subContent.Length);
                    if (!orgContent.StartsWith(httpPartern))
                    {
                        if (!orgContent.StartsWith("//"))
                        {
                            if (!orgContent.StartsWith("/"))
                            {
                                orgContent = "/" + orgContent;
                            }
                            orgContent = concatPattern + orgContent;
                        }
                        else
                        {
                            orgContent = "https:" + orgContent;
                        }
                    }
                }
                else
                {
                    //Cannot found anyindex
                    newContent = newContent + orgContent;
                    orgContent = String.Empty;
                }
            }
            return newContent;
        }
        private void scanResource(String orgContent, String pt )
        {   
            //progress - pattern
            int orgContentLen = orgContent.Length;
            String srcPattern = pt;
            //include https b'c https start with http. Exclude
            String httpPartern = "http";

            while (orgContent.Length > 0)
            {
                int indexOfSrc = orgContent.IndexOf(pt);
                srcPattern = pt;
                //found index
                if (indexOfSrc != -1)
                {
                    String subContent = orgContent.Substring(0, indexOfSrc + srcPattern.Length);
                    orgContent = orgContent.Substring(subContent.Length, orgContent.Length - subContent.Length);
                    if (!orgContent.StartsWith(httpPartern) && !orgContent.StartsWith("#"))
                    {
                        String endTagPattern = "\"";
                        int endTagIndex = orgContent.IndexOf(endTagPattern);
                        if (endTagIndex != -1)
                        {
                            String src = orgContent.Substring(0, endTagIndex );
                            orgContent = orgContent.Substring(src.Length, orgContent.Length - src.Length);
                            if (!src.StartsWith("/"))
                                src = "/" + src;

                            int fristIndex = src.IndexOf(" ");
                            int lastSpaceIndex = src.LastIndexOf(" ");
                            // first == last mean no space OR 1 space.
                            if (fristIndex == lastSpaceIndex)
                            {
                                //remove the content beforspace
                                if (fristIndex != -1)
                                    src = src.Substring(0, fristIndex).Trim() ;
                                _org.Add(removeQuestionPattern(src));
                            }
                        }
                    }

                }
                else
                {
                    //Cannot found anyindex
                    orgContent = String.Empty;
                }
            }
        
            lbContentLen.Invoke((MethodInvoker)(() => lbContentLen.Text = "Resourse count: " + _org.Count));


        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            textBox2.Text = AppDomain.CurrentDomain.BaseDirectory;

            List<String> encodingType = new List<string>
            {
                "UTF-8","ASCII","Unicode",
            };

            comboBox1.DataSource = encodingType;
        }
        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        { 
                isDownloadResource = !isDownloadResource;
        }


        private String getAbsoPath(String Path)
        {
            Path = Path.Trim();
            String orgPath = String.Empty;
            if (Path.StartsWith("//") || Path.Length<2)
                return String.Empty;
            int lastIndex = Path.LastIndexOf("/");
            if(lastIndex !=-1)
            {
                orgPath = Path.Substring(0, lastIndex);
            }
            return orgPath;
        }

        private String getFileName(string Path)
        {
            Path = Path.Trim();
            String orgPath = String.Empty;
            if ( Path.Length < 2)
                return String.Empty;
            int lastIndex = Path.LastIndexOf("/");
            if (lastIndex != -1)
            {
                orgPath = Path.Substring(lastIndex+1, Path.Length- lastIndex);
            }
            return orgPath;
        }


        private String removeQuestionPattern(String url)
        {
            int lastindex = url.LastIndexOf("?");
            if (lastindex != -1)
                return url.Substring(0, lastindex);
            return url;
        }

        private String GetFileNameFromUrl(String url)
        {
            if (url.EndsWith("/"))
            {
               url = url.Substring(0, url.Length - 1);
            }
            int lasttag = url.LastIndexOf(url);
            if(lasttag !=-1)
            {
                String name = url.Substring(lasttag, url.Length - lasttag);
                name =name.Replace("/", String.Empty);
                return name;
            }
            else
            {
                return "unknowfile" + DateTime.Now.Millisecond;
            }
        }



        private void downloadResource()
        {
            String urlPath = txtURL.Text;
            int lastIndex = urlPath.LastIndexOf("/");
            if(lastIndex != -1)
            {
                urlPath = urlPath.Substring(0, lastIndex);
            }
            
            String rootPath = textBox2.Text;
            if (urlPath.EndsWith("/"))
                urlPath = urlPath.Substring(0, urlPath.Length - 1);
            if (rootPath.EndsWith("\\"))
                rootPath = rootPath.Substring(0, rootPath.Length - 1);
            int index = 0;
            foreach (String str in _org)
            {
                dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows[index].Cells[2].Value = "Pending"));
                dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Rows[index].Cells[2].Style.BackColor = Color.MediumOrchid));

                lbdownload.Invoke((MethodInvoker)(() =>
                    lbdownload.Text = "Download progess: " + (index + 1).ToString() + "/" + _org.Count
                ));
             
                string mstr = str;
                if (!str.StartsWith("/"))
                    mstr = "/" + str;
                String CONTENT = String.Empty;
                String path = getAbsoPath(mstr);
                if(path.Trim().Length>0)
                {
                  
                    String logicalPath = rootPath + path;
                    String fileName = rootPath + mstr;
            
                    String subUrl = urlPath + mstr;

                    lbStatus.Invoke((MethodInvoker)(() => lbStatus.Text = "Status: Download " + subUrl));
                    try
                    {
                        if (!Directory.Exists(logicalPath))
                        {
                            Directory.CreateDirectory(logicalPath);
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        using (WebClient myWebClient = new WebClient())
                        {
                            //myWebClient.DownloadFileAsync(new Uri(subUrl) , fileName);
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            myWebClient.UseDefaultCredentials = true;
                            myWebClient.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                            myWebClient.DownloadFile(subUrl, fileName);
                        }
                       // Common.SaveFile(fileName, CONTENT);
                        dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows[index].Cells[2].Value = "OK"
                            
                            ));
                        dataGridView1.Invoke((MethodInvoker)(() =>
                          dataGridView1.Rows[index].Cells[2].Style.BackColor = Color.Green

                        ));
                      
                    }
                    catch (Exception ex)
                    {
                        if(ex.Message.ToLower().Contains("access"))
                        dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows[index].Cells[2].Value = "Access denied"));
                        else dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows[index].Cells[2].Value = "404"));
                        dataGridView1.Invoke((MethodInvoker)(() =>
                        dataGridView1.Rows[index].Cells[2].Style.BackColor = Color.Red

                      ));
                    }
                    
                }
                else
                {
                    dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows[index].Cells[2].Value = "Skip"));
                    dataGridView1.Invoke((MethodInvoker)(() => dataGridView1.Rows[index].Cells[2].Style.BackColor = Color.Aqua));
                }
                index++;

            }
        }


        private Boolean IsExistedInList(String str, List<String> listStr)
        {
            foreach(String s in listStr)
            {
                if (str.Equals(s))
                    return true;
            }
            return false;
        }

        private void replaceLazyInterchange()
        {

            //String URLDOMAIN = txtURL.Text;
            ////progressURL_DOMAIN
            //int ix = URLDOMAIN.LastIndexOf("/");
            //if(ix != -1)
            //{
            //    URLDOMAIN =  URLDOMAIN.Substring(0, ix);
            //}

            //if(!URLDOMAIN.EndsWith("/"))
            //{
            //    URLDOMAIN = URLDOMAIN + "/";
            //}

            ////progressURL_DOMAIN

            //String temp_content = MAIN_CONTENT;
            //int index = temp_content.IndexOf(LAZY_PARTERN);
            //while (index != -1)
            //{
            //    //check is that start with [ //right
            //    temp_content = temp_content.Substring(index + LAZY_PARTERN.Length, temp_content.Length - index - LAZY_PARTERN.Length).Trim();
            //    int dotIndex = temp_content.IndexOf("\"");
            //    // check is that end with ] //right
            //    String org_lazyContent = temp_content.Substring(0, dotIndex);
            //    //progress with orgLazyContent
            //    //remove [ ] 
            //    //org_lazyContent = org_lazyContent.Substring(1, org_lazyContent.Length - 1);
            //    //org_lazyContent = org_lazyContent.Substring(0, org_lazyContent.Length - 1);

            //    //now,split by '['
            //    String[] orgURL = org_lazyContent.Split('[');
            //    //this is the main result
            //    String progressResult = String.Empty;
            //    foreach (String url in orgURL)
            //    { 
            //        if (url.Equals(String.Empty))
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            progressResult = String.Empty;
            //            String[] sUrl = url.Split(',');
            //            for (int i = 0; i < sUrl.Length; i++)
            //            {

            //                if (i == 0)
            //                {
            //                    progressResult = progressResult + "[" + URLDOMAIN + sUrl[0];
            //                }
            //                else
            //                {
            //                    progressResult = progressResult + "," + URLDOMAIN + sUrl[i];
            //                }
            //            }
            //            MAIN_CONTENT = MAIN_CONTENT.Replace(org_lazyContent, progressResult);
            //        }

            //    }

           
            //    //next round
            //    temp_content = temp_content.Substring(org_lazyContent.Length, temp_content.Length - org_lazyContent.Length);
            //    index = temp_content.IndexOf(LAZY_PARTERN);
                

            //}

        }

        private void scanLazyInterchange()
        {
            String URLDOMAIN = txtURL.Text;
            //progressURL_DOMAIN
            String temp_content = MAIN_CONTENT;
            int index = temp_content.IndexOf(LAZY_PARTERN);
            while (index != -1)
            {
                //check is that start with [ //right
                temp_content = temp_content.Substring(index + LAZY_PARTERN.Length, temp_content.Length - index - LAZY_PARTERN.Length).Trim();
                int dotIndex = temp_content.IndexOf("\"");
                // check is that end with ] //right
                String org_lazyContent = temp_content.Substring(0, dotIndex);
                //progress with orgLazyContent
                //remove [ ] 
                org_lazyContent = org_lazyContent.Substring(1, org_lazyContent.Length - 1);
                org_lazyContent = org_lazyContent.Substring(0, org_lazyContent.Length - 1);

                //now,split by '['
                String[] orgURL = org_lazyContent.Split('[');
                //this is the main result
                String progressResult = String.Empty;
                foreach (String url in orgURL)
                {
                    String[] sUrl = url.Split(',');
                    _org.Add(removeQuestionPattern(sUrl[0]));

                }
                //next round
                index = temp_content.IndexOf(LAZY_PARTERN);
            }
        }

    

        /// <summary>
        /// The first start string must NOT be the interchaneg parttern
        /// </summary>
        /// <returns></returns>
        //private String getFristLazyInterchange()
        //{
        //  //Start with [
           
        //}


        private void BtnAnaHC_Click(object sender, EventArgs e)
        {
            String encodeType = String.Empty;
            comboBox1.Invoke((MethodInvoker)(() => encodeType = comboBox1.Text.Trim()));
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = false));
            lbStatus.Invoke((MethodInvoker)(() => lbStatus.Text = "Status: Download " + txtURL.Text));
            analysiswithHttpClientAsync();
            dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Clear()));
            int i = 0;
            _org = new List<string>();
            scanLazyInterchange();
            scanResource(MAIN_CONTENT, SRC_PARTTERN);
            scanResource(MAIN_CONTENT, HREF_PARTERN);
            scanResource(MAIN_CONTENT, DATA_IMAGE);
            scanResource(MAIN_CONTENT, DATA_BG);
            scanResource(MAIN_CONTENT, SRCSET_PARTERN);
            for (i = 0; i < _org.Count; i++)
            {
                dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Add((i + 1).ToString(), _org[i], "Wait")));
            }
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = true));
            lbdownload.Invoke((MethodInvoker)(() =>
               lbdownload.Text = "Download progess: " + 0 + "/" + _org.Count
            ));
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BtnAnaAuto_Click(object sender, EventArgs e)
        {
            String encodeType = String.Empty;
            comboBox1.Invoke((MethodInvoker)(() => encodeType = comboBox1.Text.Trim()));
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = false));
            lbStatus.Invoke((MethodInvoker)(() => lbStatus.Text = "Status: Download " + txtURL.Text));
            try
            {
                lbMethod.Invoke((MethodInvoker)(() => lbMethod.Text ="Method: WB"));
                using (var wb = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    wb.UseDefaultCredentials = true;

                    wb.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    var response = wb.DownloadData(txtURL.Text);

                    switch (encodeType)
                    {
                        case "UTF-8":
                            {
                                var htmlCode = Encoding.UTF8.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        case "ASCII":
                            {
                                var htmlCode = Encoding.ASCII.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        case "Unicode":
                            {
                                var htmlCode = Encoding.Unicode.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }
                        default:
                            {
                                var htmlCode = Encoding.Default.GetString(response);
                                MAIN_CONTENT = htmlCode.ToString();
                                MAIN_CONTENT = CRIGHT + MAIN_CONTENT;
                                break;
                            }

                    }

                }
            }
            catch
            {
                lbMethod.Invoke((MethodInvoker)(() => lbMethod.Text = "Method: HC"));
                analysiswithHttpClientAsync();
            }
            dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Clear()));
            int i = 0;
            _org = new List<string>();
            scanLazyInterchange();
            scanResource(MAIN_CONTENT, SRC_PARTTERN);
            scanResource(MAIN_CONTENT, HREF_PARTERN);
            scanResource(MAIN_CONTENT, DATA_IMAGE);
            scanResource(MAIN_CONTENT, DATA_BG);
            scanResource(MAIN_CONTENT, SRCSET_PARTERN);
            for (i = 0; i < _org.Count; i++)
            {
                dataGridView1.Invoke((MethodInvoker)(() => this.dataGridView1.Rows.Add((i + 1).ToString(), _org[i], "Wait")));
            }
            btnDownload.Invoke((MethodInvoker)(() => btnDownload.Enabled = true));
            lbdownload.Invoke((MethodInvoker)(() =>
               lbdownload.Text = "Download progess: " + 0 + "/" + _org.Count
            ));
        }
    }
  
}
