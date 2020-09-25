using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Xml;
using System.Threading;

namespace Bigcommerce_Viamente_Call_Em_All
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        public class RemoveNameSpace
        {

            public static string RemoveAllNamespaces(string xmlDocument)
            {
                XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

                return xmlDocumentWithoutNs.ToString();
            }

            private static XElement RemoveAllNamespaces(XElement xmlDocument)
            {
                if (!xmlDocument.HasElements)
                {
                    XElement xElement = new XElement(xmlDocument.Name.LocalName);
                    xElement.Value = xmlDocument.Value;
                    return xElement;
                }
                return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
            }

        }

        string API_URI_acll_Em_all_saging = "http://staging-api.call-em-all.com/webservices/ceaapi_v3-2-13.asmx?wsdl";
        string API_URI_acll_Em_all_live = "http://staging-api.call-em-all.com/webservices/ceaapi_v3-2-13.asmx?wsdl";

        class task_data
        {
            public string order_id { get; set; }
            public string c_name { get; set; }
            public string c_email { get; set; }
            public string c_phone { get; set; }
            public string total_cost { get; set; }
            public string shipping_address { get; set; }
            public bool oid_status { get; set; }
      

        }
        class result_data
        {
            public string order_id { get; set; }
            public string status { get; set; }
        }

        private delegate void UpdateProgressBarDelegate(
  System.Windows.DependencyProperty dp, Object value);

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            if (added_order.Items.IsEmpty ==false && Route_name.Text != ""  )
            {


                ProgressBar1.Minimum = 0;

                ProgressBar1.Value = 0;
                ProgressBar1.Maximum = added_order.Items.Count;
                double p_value = 0;
                UpdateProgressBarDelegate updatePbDelegate =
                new UpdateProgressBarDelegate(ProgressBar1.SetValue);

                List<task_data> task_list = new List<task_data>();

                if (added_order.Items.Count > 0)
                {
                    // List<result_data> rdata = new List<result_data>();

                    process_status.Content = "Please wait while we creating route  ....";
                    Import_viamente.Content = "Wait...";
                    Import_viamente.IsEnabled = false;
                    if (MessageBox.Show("Are you sure want to proceed?", "Warning!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        for (int cnt = 0; cnt < added_order.Items.Count; cnt++)
                        {
                            p_value += 1;
                            Dispatcher.Invoke(updatePbDelegate,
                                                        System.Windows.Threading.DispatcherPriority.Background,
                                                        new object[] { ProgressBar.ValueProperty, p_value });
                            task_data tsata = new task_data();
                            tsata.order_id = ((ListBoxItem)added_order.Items[cnt]).Uid;

                            try
                            {

                                WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/" + ((ListBoxItem)added_order.Items[cnt]).Uid);
                                HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                                httpreq_order_count.Method = "GET";
                                httpreq_order_count.ContentType = "text/xml; charset=utf-8";

                                httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();

                                StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                                string result_order = rdr_product_count.ReadToEnd();
                                //textBox1.Text = result_order;
                                bool order_send = false;

                                if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                                {
                                    XDocument doc_orders = XDocument.Parse(result_order);
                                    foreach (XElement order_data in doc_orders.Descendants("order"))
                                    {



                                        // MessageBox.Show(tsata.order_id);
                                        //  tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                                        tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString() + " " + order_data.Element("billing_address").Element("last_name").Value.ToString();
                                        tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);

                                        tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                                        //  MessageBox.Show("shiipping_Addes");

                                        WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                                        HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                                        httpreq_shipping_count.Method = "GET";
                                        httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                                        httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                        HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                                        StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                                        string result_shipping = rdr_shipping_count.ReadToEnd();
                                        if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                                        {
                                            XDocument doc_shippings = XDocument.Parse(result_shipping);
                                            foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                            {
                                                tsata.shipping_address = order_shipping.Element("street_1").Value.ToString() + " " + order_shipping.Element("street_2").Value.ToString();
                                                tsata.shipping_address += " , " + order_shipping.Element("city").Value.ToString() + " , " + order_shipping.Element("state").Value.ToString() + " " + order_shipping.Element("zip").Value.ToString() + " , " + order_shipping.Element("country").Value.ToString();
                                                tsata.oid_status = true;

                                                break;

                                            }
                                        }


                                    }
                                }




                            }

                            catch (Exception ex)
                            {
                                // MessageBox.Show(ex.Message.ToString());

                                //  rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = ex.Message.ToString() });

                                tsata.oid_status = false;


                            }


                            task_list.Add(tsata);
                        }

                        display_result.ItemsSource = task_list;

                        /// Import data into Viamente
                        /// 

                        if (task_list.Count() > 0)
                        {
                            string order_import_json = "";
                            int order_count = 0;

                            try
                            {
                                foreach (task_data single_task in task_list)
                                {
                                    if (single_task.oid_status)
                                    {
                                        if (order_count > 0)
                                        {
                                            order_import_json += ",{\"name\": \"" + single_task.c_name + "\",\"serviceTimeMin\": 5 ,\"location\": {\"address\": \"" + single_task.shipping_address + "\"},\"customFields\": {\"orderID\": \"" + single_task.order_id + "\",\"phoneNumber\": \"" + single_task.c_phone + "\"}}";
                                        }
                                        else
                                        {
                                            order_import_json += "{\"name\": \"" + single_task.c_name + "\",\"serviceTimeMin\": 5 ,\"location\": {\"address\": \"" + single_task.shipping_address + "\"},\"customFields\": {\"orderID\": \"" + single_task.order_id + "\",\"phoneNumber\": \"" + single_task.c_phone + "\"}}";
                                        }

                                        order_count++;
                                    }
                                   
                                }


                                if (order_import_json != "")
                                {

                                    // API call

                                    WebRequest req_viamente = WebRequest.Create("https://vrp.viamente.com/api/vrp/v2/routeplans?key="+Viamente_apiKey.Text);
                                    HttpWebRequest httpreq_viamente = (HttpWebRequest)req_viamente;
                                    httpreq_viamente.Method = "POST";

                                    httpreq_viamente.ContentType = "application/json";
                                    // httpreq_mandrill.Headers.Add("Authorization", "Basic " + asana_APIKey.Text);
                                    Stream str_viamente = httpreq_viamente.GetRequestStream();
                                    StreamWriter strwriter_viamente = new StreamWriter(str_viamente, Encoding.ASCII);


                                    string soaprequest_viamente = "{\"name\": \"" + Route_name.Text + "\",\"orders\": [" + order_import_json + "]}";
                                    //MessageBox.Show(soaprequest_mandrill);

                                    strwriter_viamente.Write(soaprequest_viamente.ToString());
                                    strwriter_viamente.Close();
                                    HttpWebResponse res_viamentel = (HttpWebResponse)httpreq_viamente.GetResponse();
                                    if (res_viamentel.StatusCode == HttpStatusCode.OK || res_viamentel.StatusCode == HttpStatusCode.Accepted)
                                    {
                                        StreamReader rdr_viamente = new StreamReader(res_viamentel.GetResponseStream());
                                        string result_viamente = rdr_viamente.ReadToEnd();
                                        MessageBox.Show("Route Created with Orders");
                                        process_status.Content = "Route Created with Orders";
                                    }

                                }
                            }
                            catch (Exception exx)
                            {
                                MessageBox.Show(exx.Message);
                                process_status.Content = exx.Message;

                            }
                        }
                        else
                        {
                            process_status.Content = "";
                            Import_viamente.IsEnabled = true;

                            Import_viamente.Content = "No Valid Order to import ";
                        }

                        display_result.ItemsSource = task_list;
                        
                        Import_viamente.IsEnabled = true;

                        Import_viamente.Content = "Import Order Into Viamente";
                    }
                    else
                    {
                        process_status.Content = "";
                        Import_viamente.IsEnabled = true;

                        Import_viamente.Content = "Import Order Into Viamente";
                    }

                }
                else
                {
                    MessageBox.Show("Please Enter  at least one order ID ");
                }
            }
            else
            {
                MessageBox.Show("Please input a Route Name & order ID to process");
            }
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Route_name.Text = "";

            added_order.Items.Clear();
            display_result.Items.Clear();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            if (oid.Text != "")
            {
                bool add_able = false;

                for (int i = 0; i < added_order.Items.Count; i++)
                {
                    if (((ListBoxItem)added_order.Items[i]).Uid == oid.Text)
                    {
                        add_able = true;
                        MessageBox.Show("This Order ID is already added ");
                        break;
                    }

                }
                if (!add_able)
                {

                    try
                    {
                        task_data tsata = new task_data();
                        tsata.order_id = oid.Text;

                        WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/" + oid.Text);
                        HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                        httpreq_order_count.Method = "GET";
                        httpreq_order_count.ContentType = "text/xml; charset=utf-8";

                        httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                        HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();

                        StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                        string result_order = rdr_product_count.ReadToEnd();
                        //textBox1.Text = result_order;
                        bool order_send = false;

                        if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                        {
                           // MessageBox.Show(result_order);
                            XDocument doc_orders = XDocument.Parse(result_order);
                            foreach (XElement order_data in doc_orders.Descendants("order"))
                            {



                                // MessageBox.Show(tsata.order_id);
                                //  tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                                tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString();
                                tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);

                                tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                                //  MessageBox.Show("shiipping_Addes");

                                WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                                HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                                httpreq_shipping_count.Method = "GET";
                                httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                                httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                                StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                                string result_shipping = rdr_shipping_count.ReadToEnd();
                                if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                                {
                                    XDocument doc_shippings = XDocument.Parse(result_shipping);
                                    foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                    {
                                        tsata.shipping_address = order_shipping.Element("street_1").Value.ToString() + " " + order_shipping.Element("street_2").Value.ToString();
                                        tsata.shipping_address += " , " + order_shipping.Element("city").Value.ToString() + " , " + order_shipping.Element("state").Value.ToString() + " " + order_shipping.Element("zip").Value.ToString() + " , " + order_shipping.Element("country").Value.ToString();
                                        tsata.oid_status = true;

                                        break;

                                    }
                                }


                            }

                            if (tsata.oid_status && tsata.c_name != "" && tsata.c_phone != "" && tsata.shipping_address != "")
                            {
                                added_order.Items.Add(new ListBoxItem { Content = oid.Text, Uid = oid.Text, ToolTip = oid.Text });
                                oid.Text = "";
                            }
                            else
                            {
                                MessageBox.Show("Order ID dont have customer name Or Address or phone Number . Application cant process such order ID");
                            }
                        }

                        else
                        {
                            MessageBox.Show("Order ID not found . Please check");
                        }




                    }

                    catch (Exception ex)
                    {
                         MessageBox.Show(ex.Message.ToString());

                        //  rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = ex.Message.ToString() });

                       // tsata.oid_status = false;


                    }

                   
                }
            }

            Cursor = Cursors.Arrow;
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (added_order.SelectedIndex >= 0)
            {
                added_order.Items.RemoveAt(added_order.SelectedIndex);
            }
        }

        private void added_order_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
           

            
            


        }

        private void button115_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {

           // MessageBox.Show("Working");
            Cursor = Cursors.Wait;
           
            try
            {
                
                getAudio.Content = "Please wait ..";
               // getAudio.IsEnabled = false;

               // Thread.Sleep(2000);
                string API_URI_acll_Em_all_endpoint = "";
                if (call_test.IsChecked == true)
                {
                    API_URI_acll_Em_all_endpoint = API_URI_acll_Em_all_saging;
                }
                else
                {
                    API_URI_acll_Em_all_endpoint = API_URI_acll_Em_all_live;
                }

                WebRequest req_call_em = WebRequest.Create(API_URI_acll_Em_all_endpoint);
                HttpWebRequest httpreq_call_em = (HttpWebRequest)req_call_em;
                httpreq_call_em.Method = "POST";

                httpreq_call_em.ContentType = "text/xml; charset=utf-8";
                // httpreq_mandrill.Headers.Add("Authorization", "Basic " + asana_APIKey.Text);
                Stream str_call_em = httpreq_call_em.GetRequestStream();
                StreamWriter strwriter_call_em = new StreamWriter(str_call_em, Encoding.ASCII);


                string soaprequest_call_em = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://call-em-all.com/\"><SOAP-ENV:Body><ns1:GetAudioLib><ns1:myRequest><ns1:username>" + callEm_apiKey1.Text + "</ns1:username><ns1:pin>" + callEm_Password.Text + "</ns1:pin></ns1:myRequest></ns1:GetAudioLib></SOAP-ENV:Body></SOAP-ENV:Envelope>";
                //MessageBox.Show(soaprequest_mandrill);

                strwriter_call_em.Write(soaprequest_call_em.ToString());
                strwriter_call_em.Close();
                HttpWebResponse res_call_em = (HttpWebResponse)httpreq_call_em.GetResponse();
                if (res_call_em.StatusCode == HttpStatusCode.OK || res_call_em.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader rdr_call_em = new StreamReader(res_call_em.GetResponseStream());
                    string result_call_em = rdr_call_em.ReadToEnd();

                    string Notificationdata = RemoveNameSpace.RemoveAllNamespaces(result_call_em);

                    //MessageBox.Show(Notificationdata);

                    

                    XDocument doc_audio_list = XDocument.Parse(Notificationdata);

                    XElement error_code = doc_audio_list.Descendants("errorCode").First();

                    if (Convert.ToInt32(error_code.Value.ToString()) == 0)
                    {
                        foreach (XElement single_audio in doc_audio_list.Descendants("GetAudioLibDetailData"))
                        {
                            //MessageBox.Show("loop");
                           // MessageBox.Show(single_audio.Element("audioDescription").Value.ToString());

                           // add into list

                            recorder_audio.Items.Add(new ListBoxItem { Content = single_audio.Element("audioDescription").Value.ToString(), Uid = single_audio.Element("audioID").Value.ToString(), ToolTip = single_audio.Element("userComment").Value.ToString() });
                            //.Items.Add(new ListBoxItem { Content = oid.Text, Uid = oid.Text, ToolTip = oid.Text });
                        }
                    }
                    else
                    {
                        MessageBox.Show("API Login Error");
                    }

                    

              
                   
                }

            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            getAudio.Content = "Get Recorded Audio";
            Cursor = Cursors.Arrow;

        }

        private void importCallEm_Click(object sender, RoutedEventArgs e)
        {
            if (added_order.Items.Count > 0 && recorder_audio.SelectedIndex >= 0)
            {
                // Process
               // MessageBox.Show("Processing");



                if (added_order.Items.IsEmpty == false && broadcastName.Text != "")
                    {


                        ProgressBar1.Minimum = 0;

                        ProgressBar1.Value = 0;
                        ProgressBar1.Maximum = added_order.Items.Count;
                        double p_value = 0;
                        UpdateProgressBarDelegate updatePbDelegate =
                        new UpdateProgressBarDelegate(ProgressBar1.SetValue);

                        List<task_data> task_list = new List<task_data>();

                        if (added_order.Items.Count > 0)
                        {
                            // List<result_data> rdata = new List<result_data>();

                            process_status.Content = "Please wait while we creating Broadcast List ....";
                            importCallEm.Content = "Wait...";
                            importCallEm.IsEnabled = false;
                            if (MessageBox.Show("Are you sure want to proceed?", "Warning!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                for (int cnt = 0; cnt < added_order.Items.Count; cnt++)
                                {
                                    p_value += 1;
                                    Dispatcher.Invoke(updatePbDelegate,
                                                                System.Windows.Threading.DispatcherPriority.Background,
                                                                new object[] { ProgressBar.ValueProperty, p_value });
                                    task_data tsata = new task_data();
                                    tsata.order_id = ((ListBoxItem)added_order.Items[cnt]).Uid;

                                    try
                                    {

                                        WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/" + ((ListBoxItem)added_order.Items[cnt]).Uid);
                                        HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                                        httpreq_order_count.Method = "GET";
                                        httpreq_order_count.ContentType = "text/xml; charset=utf-8";

                                        httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                        HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();

                                        StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                                        string result_order = rdr_product_count.ReadToEnd();
                                        //textBox1.Text = result_order;
                                        bool order_send = false;

                                        if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                                        {
                                            XDocument doc_orders = XDocument.Parse(result_order);
                                            foreach (XElement order_data in doc_orders.Descendants("order"))
                                            {



                                                // MessageBox.Show(tsata.order_id);
                                                //  tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                                                tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString() + " " + order_data.Element("billing_address").Element("last_name").Value.ToString();
                                                tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);

                                                tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                                                //  MessageBox.Show("shiipping_Addes");

                                                WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                                                HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                                                httpreq_shipping_count.Method = "GET";
                                                httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                                                httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                                HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                                                StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                                                string result_shipping = rdr_shipping_count.ReadToEnd();
                                                if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                                                {
                                                    XDocument doc_shippings = XDocument.Parse(result_shipping);
                                                    foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                                    {
                                                        tsata.shipping_address = order_shipping.Element("street_1").Value.ToString() + " " + order_shipping.Element("street_2").Value.ToString();
                                                        tsata.shipping_address += " , " + order_shipping.Element("city").Value.ToString() + " , " + order_shipping.Element("state").Value.ToString() + " " + order_shipping.Element("zip").Value.ToString() + " , " + order_shipping.Element("country").Value.ToString();
                                                        tsata.oid_status = true;

                                                        break;

                                                    }
                                                }


                                            }
                                        }




                                    }

                                    catch (Exception ex)
                                    {
                                        // MessageBox.Show(ex.Message.ToString());

                                        //  rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = ex.Message.ToString() });

                                        tsata.oid_status = false;


                                    }


                                    task_list.Add(tsata);
                                }

                                display_result.ItemsSource = task_list;

                               

                                if (task_list.Count() > 0)
                                {
                                    string phone_numbers_import = "";
                                    int order_count = 0;

                                    try
                                    {
                                        foreach (task_data single_task in task_list)
                                        {
                                            if (single_task.oid_status)
                                            {
                                                if (order_count > 0)
                                                {
                                                    phone_numbers_import += "," + single_task.c_phone;
                                                }
                                                else
                                                {
                                                    phone_numbers_import += single_task.c_phone;
                                                }

                                                order_count++;
                                            }

                                        }


                                        if (phone_numbers_import != "")
                                        {

                                            // API call create broadcast list in Call-Em


                string API_URI_acll_Em_all_endpoint = "";
                if (call_test.IsChecked == true)
                {
                    API_URI_acll_Em_all_endpoint = API_URI_acll_Em_all_saging;
                }
                else
                {
                    API_URI_acll_Em_all_endpoint = API_URI_acll_Em_all_live;
                }

                WebRequest req_call_em = WebRequest.Create(API_URI_acll_Em_all_endpoint);
                HttpWebRequest httpreq_call_em = (HttpWebRequest)req_call_em;
                httpreq_call_em.Method = "POST";

                httpreq_call_em.ContentType = "text/xml; charset=utf-8";
                // httpreq_mandrill.Headers.Add("Authorization", "Basic " + asana_APIKey.Text);
                Stream str_call_em = httpreq_call_em.GetRequestStream();
                StreamWriter strwriter_call_em = new StreamWriter(str_call_em, Encoding.ASCII);


                //string soaprequest_call_em = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://call-em-all.com/\"><SOAP-ENV:Body><ns1:ExtCreateBroadcast><ns1:myRequest><ns1:username>" + callEm_apiKey1.Text + "</ns1:username><ns1:pin>" + callEm_Password.Text + "</ns1:pin><ns1:broadcastName>" + broadcastName.Text + "</ns1:broadcastName><ns1:broadcastType>1</ns1:broadcastType><ns1:commaDelimitedPhoneNumbers>" + phone_numbers_import + "</ns1:commaDelimitedPhoneNumbers><ns1:messageID>" + ((ListBoxItem)recorder_audio.Items[recorder_audio.SelectedIndex]).Uid + "</ns1:messageID><ns1:phoneNumberSource>3</ns1:phoneNumberSource><ns1:launchDateTime>03/18/2016 10:10:10 AM</ns1:launchDateTime></ns1:myRequest></ns1:ExtCreateBroadcast></SOAP-ENV:Body></SOAP-ENV:Envelope>";
                string soaprequest_call_em = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://call-em-all.com/\"><SOAP-ENV:Body><ns1:ExtCreateBroadcast><ns1:myRequest><ns1:username>" + callEm_apiKey1.Text + "</ns1:username><ns1:pin>" + callEm_Password.Text + "</ns1:pin><ns1:broadcastName>" + broadcastName.Text + "</ns1:broadcastName><ns1:broadcastType>1</ns1:broadcastType><ns1:commaDelimitedPhoneNumbers>" + phone_numbers_import + "</ns1:commaDelimitedPhoneNumbers><ns1:messageID>" + ((ListBoxItem)recorder_audio.Items[recorder_audio.SelectedIndex]).Uid + "</ns1:messageID><ns1:phoneNumberSource>3</ns1:phoneNumberSource></ns1:myRequest></ns1:ExtCreateBroadcast></SOAP-ENV:Body></SOAP-ENV:Envelope>";
                //MessageBox.Show(soaprequest_mandrill);

                strwriter_call_em.Write(soaprequest_call_em.ToString());
                strwriter_call_em.Close();
                HttpWebResponse res_call_em = (HttpWebResponse)httpreq_call_em.GetResponse();
                if (res_call_em.StatusCode == HttpStatusCode.OK || res_call_em.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader rdr_call_em = new StreamReader(res_call_em.GetResponseStream());
                    string result_call_em = rdr_call_em.ReadToEnd();

                    string Notificationdata = RemoveNameSpace.RemoveAllNamespaces(result_call_em);

                    //MessageBox.Show(Notificationdata);



                    XDocument doc_audio_list = XDocument.Parse(Notificationdata);

                    XElement error_code = doc_audio_list.Descendants("errorCode").First();

                    if (Convert.ToInt32(error_code.Value.ToString()) == 0)
                    {
                        MessageBox.Show("Done!");
                    }
                    else
                    {
                        MessageBox.Show("API Login Error");
                    }
                }

                                        

                                        }
                                    }
                                    catch (Exception exx)
                                    {
                                        MessageBox.Show(exx.Message);
                                        process_status.Content = exx.Message;

                                    }
                                }
                                else
                                {
                                    process_status.Content = "";
                                    importCallEm.IsEnabled = true;

                                    importCallEm.Content = "No Valid Order to import ";
                                }
                                process_status.Content = "Done!";
                                display_result.ItemsSource = task_list;

                                importCallEm.IsEnabled = true;

                                importCallEm.Content = "Import Into Call-em-all";
                            }
                            else
                            {
                                process_status.Content = "";
                                importCallEm.IsEnabled = true;

                                importCallEm.Content = "Import Into Call-em-all";
                            }

                        }
                        else
                        {
                            MessageBox.Show("Please Enter  at least one order ID ");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please input a Broadcast Name & order ID to process");
                    }
                



            }
            else
            {
                MessageBox.Show("Add BC Orders & Select Audio to assign ");
            }
        }
    }
}
