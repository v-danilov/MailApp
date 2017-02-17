using System;
using System.Windows;
using System.Net.Mail;
using System.Net;
using AE.Net.Mail;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MailApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            String username = loginBox.Text;
            String password = passwordBox.Password;
           

            SmtpClient client = new SmtpClient();
            NetworkCredential basicCredential = new NetworkCredential(username, password);
            string domen = comboBox.Text;
            EnterAnimation_BeginStoryboard.Storyboard.Begin(login, true);
            try
            {

                ImapClient ic;
                switch (domen)
                {
                    case "@outlook.com":
                        ic = new ImapClient("imap-mail.outlook.com", username + domen, password, AuthMethods.Login, 993, true);
                        break;

                    case "@live.com":
                        ic = new ImapClient("imap-mail.outlook.com", username + domen, password, AuthMethods.Login, 993, true);
                        break;

                    case "@gmail.com":
                        ic = new ImapClient("imap.gmail.com", username + domen, password, AuthMethods.Login, 993, true);
                        break;

                    case "@mail.ru":
                        ic = new ImapClient("imap.mail.ru", username + domen, password, AuthMethods.Login, 993, true);
                        break;

                    case "@yandex.ru":
                        ic = new ImapClient("imap.yandex.ru", username + domen, password, AuthMethods.Login, 993, true);
                        break;

                    default:
                        ic = null;
                        throw new MissingMemberException("Missing domen name");
                     
                }
               
                
                MailWindow mw = new MailWindow(username, password, domen ,ic);
                //ic.Disconnect();
                //EnterAnimation_BeginStoryboard.Storyboard.Stop(login);
                this.Close();
                mw.Show();
               
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message + "\n" + exc.StackTrace);
                passwordBox.Password = "";
                passwordBox.BorderBrush = Brushes.Red;
                loginBox.BorderBrush = Brushes.Red;
                EnterAnimation_BeginStoryboard.Storyboard.Stop(login);

            }

            
        }

        private void loginBox_GotFocus(object sender, RoutedEventArgs e)
        {
            loginBox.BorderBrush = Brushes.White;
        }

        private void passwordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordBox.BorderBrush = Brushes.White;
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void go_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
