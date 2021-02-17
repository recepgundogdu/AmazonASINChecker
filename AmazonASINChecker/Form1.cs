using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmazonASINChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            string[] asins = textBox1.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in asins)
            {
                Task.Run(() =>
                {
                    string html = "";
                    string product = item;
                    string price1 = "";
                    string price2 = "";

                    var hasRow = dataGridView1.Rows.OfType<DataGridViewRow>().Where(x => (string)x.Cells["ASIN"].Value == product).ToList().Count > 0;
                    var price1Error = dataGridView1.Rows.OfType<DataGridViewRow>().Where(x => (string)x.Cells["ASIN"].Value == product && (string)x.Cells["amazon.com"].Value == "HATA!").ToList().Count > 0;
                    var price2Error = dataGridView1.Rows.OfType<DataGridViewRow>().Where(x => (string)x.Cells["ASIN"].Value == product && (string)x.Cells["amazon.ca"].Value == "HATA!").ToList().Count > 0;
                    var index = hasRow ? dataGridView1.Rows.OfType<DataGridViewRow>().Where(x => (string)x.Cells["ASIN"].Value == product).ToList()[0].Index : -1;


                    if (!hasRow || price1Error || price2Error)
                    {
                        if (!hasRow || price1Error)
                        {

                            html = GetHtml("https://www.amazon.com/s?k=" + product);

                            try
                            {
                                price1 = html.Split(new[] { "<span class=\"a-offscreen\">" }, StringSplitOptions.None)[1].Split('<')[0];
                            }
                            catch
                            {
                                price1 = "HATA!";
                            }
                        }
                        if (!hasRow || price2Error)
                        {
                            html = GetHtml("https://www.amazon.ca/s?k=" + product);
                            try
                            {
                                price2 = html.Split(new[] { "<span class=\"a-offscreen\">" }, StringSplitOptions.None)[1].Split('<')[0];
                            }
                            catch
                            {
                                price2 = "HATA!";
                            }
                        }
                        Invoke(new Action(() =>
                        {
                            if (index != -1)
                            {
                                DataGridViewRow newDataRow = dataGridView1.Rows[index];
                                if (price1Error)
                                {
                                    newDataRow.Cells[1].Value = price1;
                                }
                                if (price2Error)
                                {
                                    newDataRow.Cells[2].Value = price2;
                                }
                            }
                            else
                            {
                                DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                                row.Cells[0].Value = product;
                                row.Cells[1].Value = price1;
                                row.Cells[2].Value = price2;
                                dataGridView1.Rows.Add(row);

                            }
                        }));

                    }
                    Invoke(new Action(() =>
                    {
                        try
                        {

                            var loadSize = 100 / asins.Length;
                            progressBar1.Value += progressBar1.Value + loadSize > 100 ? 100 : loadSize;
                            if (asins.ToList().IndexOf(item) == asins.Length - 1)
                            {
                                progressBar1.Value = 100;
                            }
                        }
                        catch
                        {
                            progressBar1.Value = 100;
                        }
                    }));
                });

            }



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("ASIN", "ASIN");
            dataGridView1.Columns.Add("amazon.com", "amazon.com");
            dataGridView1.Columns.Add("amazon.ca", "amazon.ca");
        }
        string GetHtml(string url)
        {
            string result = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; …) Gecko/20100101 Firefox/55.0");
                    wc.Encoding = Encoding.UTF8;
                    result = wc.DownloadString(url  + "&ref=" + new Random().Next(0,999999));
                }
            }
            catch
            {
            }
            return result;
        }
    }
}
