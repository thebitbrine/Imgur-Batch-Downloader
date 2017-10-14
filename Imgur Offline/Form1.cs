using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImgurSharp;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Imgur_Offline
{
    public partial class Form1 : Form
    {
        public Imgur imgur = new Imgur("7295e8ecd50bb1a");
        ImgurAlbum album;
        ImgurImage image;


        int totalCount = 0;
        int alreadyDone = 0;
        int albumCount = 0;
        int mediaCount = 0;
        int duplicates = 0;

        bool albumOnly = false;
        bool autoMode = false;
        bool running = false;

        int minImageCount = 0;
        int autoModeSec = 0;

        List<string> allIds = new List<string>();

        public Form1()
        {
            InitializeComponent();
            if(File.Exists("allIds.txt"))
            allIds = File.ReadAllLines("allIds.txt").ToList<string>();
        }

        public void FetchAll()
        {

            ThreadStart A = new ThreadStart(extractLinks);
            Thread B = new Thread(A);
            B.IsBackground = true;
            B.Start();
            running = true;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            //Json();
            //Downloader("A8CFv");
            //getRequest();
            //altDownloader(Path.GetFileName(new Uri("http://imgur.com/gallery/cSJ9m").AbsolutePath));


            FetchAll();

        }

        public void altDownloader(string mediaID)
        {
            //string galleryId = "";
            //string galleryURL = "";
            //int fileExtPos = galleryURL.LastIndexOf("/");
            //if (fileExtPos >= 0)
            //    galleryId = galleryURL.Substring(fileExtPos, galleryURL.Length);

            string response = getRequest("https://api.imgur.com/3/album/" + mediaID + "/images");

            response = response.Replace("//", "/");
            response = response.Replace(@"\/", "/");
            List<string> galleryLinks = GetLinks(response);

            if (galleryLinks.Count >= minImageCount)
            {
                for (int i = 0; i < galleryLinks.Count; i++)
                {
                    using (var client = new WebClient())
                    {

                        string filename = Path.GetFileName(new Uri(galleryLinks[i]).AbsolutePath);

                        string fileNameX = "";
                        int fileExtPos = filename.LastIndexOf(".");
                        if (fileExtPos >= 0)
                            fileNameX = filename.Substring(0, fileExtPos);



                        if (!allIds.Contains(fileNameX) && galleryLinks[i].Contains("imgur.com"))
                        {
                            Directory.CreateDirectory(@"Imgur\" + mediaID);
                            allIds.Add(fileNameX);

                            Directory.CreateDirectory(@"Imgur\" + mediaID);
                            client.DownloadFile(galleryLinks[i], @"Imgur\" + mediaID + @"\[" + (i + 1) + "] " + fileNameX + ".jpg");


                            //string[] Info = getAlbumInfo(mediaID);
                            //if(!File.Exists(Info[0]) && Info[0] != "")
                            //    File.WriteAllText(@"Imgur\" + mediaID + @"\" + Info[0] + ".txt", Info[1]);

                            totalCount++;
                            File.WriteAllLines("allIds.txt", allIds);
                        }
                    }
                }
            }

        }


        public string[] getAlbumInfo(string albumCode)
        {
            string[] fetchedData = { "null", "null" };
            string infoResponse = getRequest("https://api.imgur.com/3/gallery/album/" + albumCode);

            fetchedData[0] = getBetween(infoResponse, "\"title\":", ",").Replace(@"\","").Replace("\"","");
            fetchedData[1] = getBetween(infoResponse, "\"description\":", ",").Replace(@"\", "").Replace("\"", "");

            if (fetchedData[1] == "null")
                fetchedData[1] = "";

            return fetchedData;
        }





        public async void Downloader(string mediaId)
        {
                album = await imgur.GetAlbum(mediaId);
                for (int i = 0; i < album.ImagesCount; i++)
                {
                    using (var client = new WebClient())
                    {

                        string filename = Path.GetFileName(new Uri(album.Images[i].Link).AbsolutePath);

                        if (!allIds.Contains(album.Images[i].Id))
                        {
                            Directory.CreateDirectory(@"Imgur\" + mediaId);
                            allIds.Add(album.Images[i].Id);
                            client.DownloadFile(album.Images[i].Link, @"Imgur\" + mediaId + @"\" + album.Images[i].Id + ".jpg");
                            File.WriteAllLines("allIds.txt", allIds);
                        }
                    }
                }
            }

        


        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }


        public string getRequest(string url)
        {
                string html = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Headers["authorization"] = "Client-ID 7295e8ecd50bb1a";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
                return html;
        }


        public void extractLinks()
        {
            string json = getRequest(@"https://api.imgur.com/3/gallery/hot/viral/0.json");
            json = json.Replace("//", "/");
            json = json.Replace(@"\/","/");
            List<string> links = GetLinks(json);
            for (int i = 0; i < links.Count; i++)
            {
                if (links[i].Contains("imgur.com") && links[i].EndsWith(".jpg") || links[i].EndsWith(".mp4") || links[i].EndsWith(".png"))
                {
                    using (var clientImage = new WebClient())
                    {
                        string filename = Path.GetFileName(new Uri(links[i]).AbsolutePath);
                        Directory.CreateDirectory("Imgur");

                        string fileNameX = "";
                        int fileExtPos = filename.LastIndexOf(".");
                        if (fileExtPos >= 0)
                        fileNameX = filename.Substring(0, fileExtPos);



                        if (albumOnly == false && !File.Exists(@"Imgur\" + filename) && !allIds.Contains(fileNameX))
                        {
                            allIds.Add(fileNameX);
                            clientImage.DownloadFile(links[i], @"Imgur\" + filename);
                            totalCount++;
                            File.WriteAllLines("allIds.txt", allIds);
                        }
                    }
                }
                else
                {
                    if (links[i].Contains("imgur.com") && !links[i].EndsWith(".gif") && !links[i].EndsWith(".gifv") )
                    {
                        string filename = Path.GetFileName(new Uri(links[i]).AbsolutePath);
                        altDownloader(filename);

                    }
                }
            }

            CheckList();
        }

        public void CheckList()
        {

            List<string> Backup = allIds;
            File.WriteAllLines("allIds.txt", allIds);
            running = false;

        }



        public static List<string> GetLinks(string message)
        {
            List<string> list = new List<string>();
            Regex urlRx = new Regex(@"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", RegexOptions.IgnoreCase);

            MatchCollection matches = urlRx.Matches(message);
            foreach (Match match in matches)
            {
                list.Add(match.Value);
            }
            return list;
        }

        public class AlbumZ
        {
            public string id { get; set; }
            public string link { get; set; }
            public bool in_gallery { get; set; }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                albumOnly = true;
            else
                albumOnly = false;
        }

        public int Eyy = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!File.Exists("allIds.txt"))
            {
                allIds.Clear();
            }

            if (Eyy != allIds.Count)
            {
                listBox1.Items.Clear();
                Eyy = allIds.Count();
                for (int i = 0; i < allIds.Count; i++)
                {
                    listBox1.Items.Add(allIds[i].ToString());
                }
                listBox1.SelectedIndex = listBox1.Items.Count-1;
            }
            else
            {
                //for (int i = 0; i < allIds.Count; i++)
                //{
                //    listBox1.Items.Add(allIds[i].ToString());
                //}
            }


            if (autoMode == true && running == false && DateTime.Now.Second == autoModeSec)
                FetchAll();
            

            label2.Text = totalCount + "";
            minImageCount = (int)numericUpDown1.Value;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                autoMode = true;
                button1.Enabled = false;
                autoModeSec = DateTime.Now.Second;
            }
            else
            {
                autoMode = false;
                button1.Enabled = true;
            }
        }
    }
}
