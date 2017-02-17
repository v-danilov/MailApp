using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using AE.Net.Mail;
using System.IO;
using System.ComponentModel;
using System.Threading;
using HtmlAgilityPack;
using System.Text;
using System.Windows.Media.Animation;

namespace MailApp
{
    /// <summary>
    /// Interaction logic for MailWindow.xaml
    /// </summary>
    public partial class MailWindow : Window
    {
        private String username;
        private String password;
        private ImapClient ic;
        private String domen;

        //Массив писем
        MailMessage[] messages;

        //Форма нового сообщения
        WriteMessageForm wmf;

        //Лист для обновления почты в отдельном потоке
        public List<String> MyList = new List<String>();

        //Кол-во писем в почте
        int max_position = 0;

        //Текущее диапазон открытых писем
        int current_position = 0;

        //Объект для конролирования многопоточности
        private static readonly object locker = new object();


        public MailWindow(String _user, String _pass, String _domen, ImapClient _ic)
        {
            InitializeComponent();
            username = _user;
            password = _pass;
            ic = _ic;
            domen = _domen;
        }


        //Обработчик "обновления"
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {

            lock (locker)
            {
                listBox.Items.Clear();
                progressBar.Visibility = Visibility.Visible;

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (o, args) => background_Update();
                bw.RunWorkerCompleted += (o, args) => MethodToUpdateControl();
                bw.RunWorkerAsync();
                
            }
            
        }

        //Обработчик контейнера с письмами
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {    
            if (listBox.SelectedItem != null)
            {
                int count = messages.Length - 1;
                replyBtn.IsEnabled = true;
                deleteBtn.IsEnabled = true;
                
                int index = listBox.SelectedIndex;
                MailMessage selected_mes = messages[count - index];



                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(selected_mes.Body);

                var root = doc.DocumentNode;
                var sb = new StringBuilder();
                foreach (var node in root.DescendantNodesAndSelf())
                {
                    if (!node.HasChildNodes)
                    {
                        string text = node.InnerText;
                        if (!string.IsNullOrEmpty(text))
                            sb.AppendLine(text.Trim());
                    }
                }

                fromBlock.Text = selected_mes.From.DisplayName
                + "  (" + selected_mes.From.User
                + "@" + selected_mes.From.Host + ")";

                subjectBlock.Text = selected_mes.Subject;

                bodyBlock.Text = sb.ToString();


                if(selected_mes.Attachments.Count > 0)
                {
                    downloadAttach.Visibility = Visibility.Visible;
                    downloadAttach.IsEnabled = true;
                }
                else
                {
                    downloadAttach.Visibility = Visibility.Hidden;
                    downloadAttach.IsEnabled = false;
                }
                
                Console.WriteLine(selected_mes.Attachments.Count());
            }
        }


        //Обработчик "нового письма"
        private void writeBtn_Click(object sender, RoutedEventArgs e)
        {      
            wmf = new WriteMessageForm(username, password, domen);
            wmf.Show();
        }



        //Обработчик выхода из аккаунта
        private void logOutBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow logInForm = new MainWindow();
            ic.Dispose();
            this.Close();
            logInForm.Show();
            
        }
        
        //Обработчик кнопки загрузки вложений
        private void downloadAttach_Click(object sender, RoutedEventArgs e)
        {
            int count = messages.Length - 1;
            int index = listBox.SelectedIndex;
            MailMessage msg = messages[count - index];

            foreach (Attachment att in msg.Attachments)
            {
                string fName;
                fName = att.Filename;
                string path = @"C:\Mail Attachments\";
                if (!Directory.Exists(path))
                {

                    Directory.CreateDirectory(path);
                }
                att.Save(Path.Combine(path, fName));
                Console.WriteLine(fName);
                System.Diagnostics.Process.Start(path);
            }

        }


        //Обработчик "ответа"
        private void replyBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = listBox.SelectedIndex;
            MailMessage selected_mes = messages[index];
            wmf = new WriteMessageForm(username, password, domen, selected_mes);
            wmf.Show();



        }


        //Обработчик "удаления"
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = messages.Length - 1;
            int index = listBox.SelectedIndex;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (o, args) => deleteMes(ic, count, index);
            bw.RunWorkerCompleted += (o, args) => bgWorker_RunWorkerCompleted(sender, args, e);
            bw.RunWorkerAsync();
           
              
            fromBlock.Text = "";
            subjectBlock.Text = "";
            bodyBlock.Text = "";
                  
        }


        //Функция удаления сообщения
        private void deleteMes(ImapClient ic, int count, int index)
        {           
                MailMessage msg = messages[count - index];
                List<Lazy<MailMessage>> message_to_delete = ic.SearchMessages(SearchCondition.UID(msg.Uid)).ToList();
                ic.DeleteMessage(message_to_delete.First().Value);
                ic.Expunge();
 
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs ea, RoutedEventArgs e) {

            updateBtn_Click(sender, e);
   
        }
            


        //Функция листания вперед
        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (current_position + 10 >= max_position - 10)
            {
                current_position = max_position - 10;
                nextBtn.IsEnabled = false;
                updateBtn_Click(sender, e);
            }
            else
            {
                current_position = current_position + 10;
                nextBtn.IsEnabled = true;
                updateBtn_Click(sender, e);
            }
            prevBtn.IsEnabled = true;

            
        }

        //Функция листания назад
        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (current_position <= 0)
            {
                prevBtn.IsEnabled = false;
            }
            else
            {
                current_position = current_position - 10;
                updateBtn_Click(sender, e);
                prevBtn.IsEnabled = true;
            }
            nextBtn.IsEnabled = true;
        }

        //Функция обновления содержимого
        private void background_Update()
        {

            lock (locker)
            {
                ic.SelectMailbox("INBOX");
                int message_count = ic.GetMessageCount();
                max_position = message_count;
                Console.WriteLine(message_count);
                MyList.Clear();

                if (domen != "@live.com")
                {
                    messages = ic.GetMessages(message_count - current_position - 10,
                                                message_count - current_position,
                                                false);

                    foreach (MailMessage mes in messages.Reverse())
                    {
                        MyList.Add(mes.From.DisplayName
                                          + "\n" + mes.Subject
                                          + "\n" + mes.Date);
                    }
                   

                }
                else
                {

                    messages = ic.GetMessages(message_count - current_position - 10,
                                                 message_count - current_position - 1,
                                                 false);

                    foreach (MailMessage mes in messages.Reverse())
                    {
                        MyList.Add(mes.From.DisplayName
                                          + "\n" + mes.Subject
                                          + "\n" + mes.Date);
                    }

                    MailMessage mm;
                }
            }
        }


        //Функция отображения обновленного контента
        private void MethodToUpdateControl()
        {
            listBox.Items.Clear();
            foreach (String str in MyList)
            {
                listBox.Items.Add(str);
                progressBar.Visibility = Visibility.Hidden;
            }
        }


        private void Form_FormClosin(object sender, CancelEventArgs e)
        {
            MainWindow logInForm = new MainWindow();
            ic.Dispose();
            //this.Close();
            logInForm.Show();
        }
    }
}
