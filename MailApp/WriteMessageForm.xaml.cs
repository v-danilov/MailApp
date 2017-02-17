using System;

using System.Windows;

using System.Windows.Media;

using System.Net.Mail;
using Microsoft.Win32;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media.Animation;


namespace MailApp
{
    /// <summary>
    /// Interaction logic for WriteMessageForm.xaml
    /// </summary>
    /// 
    public partial class WriteMessageForm : Window
    {
        private String username;
        private String password;
        private String filename;
        private String domen;

        public WriteMessageForm(String _user, String _pass, String _domen)
        {
            InitializeComponent();
            username = _user;
            password = _pass;
            domen = _domen;
        }

        public WriteMessageForm(String _user, String _pass, String _domen, AE.Net.Mail.MailMessage email )
        {
            InitializeComponent();
            username = _user;
            password = _pass;
            domen = _domen;
            addressBox.Text = email.From.User + "@" + email.From.Host;
            subjectBox.Text = "RE: " + email.Subject;
            bodyBox.Text = email.Body + "____________________________________________________\n";

        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient client = choose_Client(domen);
            
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(username + domen, password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;


            
           
            MailMessage email = new MailMessage();
            string address = addressBox.Text;
            try
            {
                {
                    email.To.Add(address);
                    email.Subject = subjectBox.Text;
                    email.Body = bodyBox.Text;
                    email.From = new MailAddress(username + domen);

                    if (!String.IsNullOrEmpty(filename))
                    {
                        Attachment attachment = new Attachment(filename, MediaTypeNames.Application.Octet);
                        email.Attachments.Add(attachment);
                    }

                    sendBtn.Visibility = Visibility.Hidden;
                    arc.Visibility = Visibility.Visible;
                    Storyboard board = this.FindResource("SpinRing") as Storyboard;
                    //board.Begin(sendBtn, true);

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += (o, args) => sendMes(client, email);
                    bw.RunWorkerCompleted += (o, args) => bgWorker_RunWorkerCompleted(o, args);
                    bw.RunWorkerAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                addressBox.BorderBrush = Brushes.DarkRed;
            }
            

        }

        
        
        private void attachBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            Nullable<bool> result = filedialog.ShowDialog();
            if (result == true)
            {
                // Open document
                filename = filedialog.FileName;
            }
            //Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet);
        }

        public SmtpClient choose_Client(String _domen)
        {
            switch (domen)
            {
                case "@outlook.com":
                    return new SmtpClient("smtp.live.com", 587);
                   

                case "@live.com":
                    return new SmtpClient("smtp.live.com", 587);


                case "@gmail.com":
                    return new SmtpClient("smtp.gmail.com", 587);


                case "@mail.ru":
                    return new SmtpClient("smtp.mail.ru", 587);


                case "@yandex.ru":
                    return new SmtpClient("smtp.yandex.ru", 587);

                default:
                    return null;
            }
        }


        private void sendMes(SmtpClient sc, MailMessage mes)
        {
            
                sc.Send(mes);
                sc.Dispose();
             
                
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Storyboard board = this.FindResource("SpinRing") as Storyboard;
            sendBtn.Visibility = Visibility.Visible;
            arc.Visibility = Visibility.Hidden;
            //board.Stop(sendBtn);

            if (e.Error != null)
            {
                Console.WriteLine(e.Error.Message + "\n" + e.Error.StackTrace);
                addressBox.BorderBrush = Brushes.DarkRed;
            }
            else
            {
                MessageBox.Show("Email succesful sent");
            }

            
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
