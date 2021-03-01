using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace InstaParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null)
            {
                string name = textBox1.Text;

                var url = "https://www.instagram.com/" + name + "/";
                var headSectionHtml = DownloadHtmlHeadSection(url);

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(headSectionHtml);

                var metaTags = htmlDoc.DocumentNode.Descendants()
                                      .Where(el => el.NodeType == HtmlNodeType.Element && el.Name == "meta");

                string content = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']").Attributes["content"].Value;

                string[] contents = content.Split(' ');

                label1.Text = name;

                label2.Text = "Подписчики: " + contents[0];

                label3.Text = "Подписки: " + contents[2];

                label4.Text = "Кол - во публикаций: " + contents[4];

                pictureBox1.ImageLocation = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").Attributes["content"].Value;

            }
            else
            {
                MessageBox.Show("Введите имя пользователя!");
            }
        }


        private static string DownloadHtmlHeadSection(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        StringBuilder headSection = new StringBuilder();

                        bool headSectionReading = false;

                        FixedSizedQueue<char> buffer = new FixedSizedQueue<char>(7);

                        while (!streamReader.EndOfStream)
                        {
                            char @char = (char)streamReader.Read();

                            buffer.Enqueue(@char);

                            string bufStr = buffer.ToString().Trim();

                            if (bufStr.Contains("<head>") && !headSectionReading)
                            {
                                headSectionReading = true;
                                headSection.Append("<head>");
                                continue;
                            }

                            if (bufStr == "</head>" && headSectionReading)
                            {
                                return headSection.ToString();
                            }

                            if (headSectionReading)
                                headSection.Append(@char);
                        }

                        return null;
                    }
                }
            }
        }
    }
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size > 0 ? size : throw new ArgumentOutOfRangeException(nameof(size));
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            while (Count > Size && TryDequeue(out T overflow)) ;
        }

        public override string ToString() => string.Join("", this.Select(el => el.ToString()));
    }
}

