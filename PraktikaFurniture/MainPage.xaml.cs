using PraktikaFurniture.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System;
using Xceed.Words.NET;
using System.Diagnostics;
using System.IO;
using Xceed.Document.NET;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace PraktikaFurniture
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            EmptyPanel.Visibility = Visibility.Hidden;
            PriceyRadioBttn.IsChecked = true;
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            UpdateListView();
        }

        private void UpdateListView()
        {
            var currentProducts = JewelryStoreDbContext.dbContext.Jewelries.ToList();

            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) == true)
                SearchTextBox.Text = "";
            else
                currentProducts = currentProducts.Where(u => u.Name.ToLower().Contains(SearchTextBox.Text.ToLower()) ||
                                                                            u.Metal.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();

            if (CheapRadioBttn.IsChecked == true)
                currentProducts = currentProducts.OrderBy(p => p.Price).ToList();
            else
                currentProducts = currentProducts.OrderByDescending(p => p.Price).ToList();

            ListViewProducts.ItemsSource = currentProducts;

            EmptyPanel.Visibility = ListViewProducts.HasItems ? Visibility.Hidden : Visibility.Visible;
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateListView();
        private void CheapRadioBttn_Checked(object sender, RoutedEventArgs e) => UpdateListView();
        private void PriceyRadioBttn_Checked(object sender, RoutedEventArgs e) => UpdateListView();
        private void DropFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            PriceyRadioBttn.IsChecked = true;
            UpdateListView();
        }

        private void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            string exception = ex.Message + "\n" + ex.GetBaseException() + "\n" + ex.InnerException + "\n" + ex.Source;
            SendMessage(exception);
            MessageBox.Show("Сообщение об ошибке отправлено в поддержку!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c taskkill /f /im \"{AppDomain.CurrentDomain.FriendlyName + ".exe"}\" && timeout /t 1 && {Process.GetCurrentProcess().MainModule.FileName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                });
            }
            catch (Exception exс) { MessageBox.Show(exс.Message); }
        }
        private static void SendMessage(string exception)
        {
            string smtpServer = "smtp.mail.ru";
            int smtpPort = 587;
            string smtpUsername = "ivanovapractika_errors@mail.ru";
            string smtpPassword = "95kDHZ?zEp?Ab7c8?Pcm?euq".Replace("?", "");

            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(smtpUsername);
                    mailMessage.Subject = "В приложении возникла ошибка";
                    mailMessage.Body = exception;

                    try
                    {
                        smtpClient.Send(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Message sending error: {ex.Message}");
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            try
            {
                using (var templateDoc = DocX.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("PraktikaFurniture.doc-template.docx")))
                {
                    var rand = new Random(); string docNumber = rand.Next(100, 1000).ToString();
                    ReplaceKeywordWithValue(templateDoc, "[doc-number]", docNumber);
                    ReplaceKeywordWithValue(templateDoc, "[at-date]", DateTime.UtcNow.ToString("f"));

                    int sumQuant = 0;
                    foreach (var item in ListViewProducts.SelectedItems) sumQuant += (item as Jewelry).StockQuantity;
                    ReplaceKeywordWithValue(templateDoc, "[sum-quantity]", sumQuant.ToString());
                    decimal sumPrice = 0;
                    foreach (var item in ListViewProducts.SelectedItems) sumPrice += (item as Jewelry).Price;
                    ReplaceKeywordWithValue(templateDoc, "[sum-price]", sumPrice.ToString());

                    var reportTable = templateDoc.Tables[1];
                    var rowPattern = reportTable.Rows[1];
                    int counter = 1;
                    foreach(var item in ListViewProducts.SelectedItems)
                    {
                        AddItemToTable(counter, reportTable, rowPattern, item as Jewelry);
                        counter++;
                    }
                    rowPattern.Remove();
                    templateDoc.SaveAs($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Возвратная накладная No_{docNumber}");

                    MessageBox.Show("Документ успешно создан. Вы можете найти его на рабочем столе.", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Ошибка при создании документа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }
        private static void AddItemToTable(int number, Table table, Row rowPattern, Jewelry product)
        {
            var newItem = table.InsertRow(rowPattern, table.RowCount - 1);

            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%NUMBER%", NewValue = number.ToString() });
            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%NAME%", NewValue = product.Name });
            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%METAL%", NewValue = product.Metal });
            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%GEMSTONE%", NewValue = product.Gemstone });
            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%QUANTITY%", NewValue = product.StockQuantity.ToString() });
            newItem.ReplaceText(new StringReplaceTextOptions() { SearchValue = "%PRICE%", NewValue = product.Price.ToString() });
        }
        private void ReplaceKeywordWithValue(DocX document, string keyword, string value)
        {
            foreach (var paragraph in document.Paragraphs)
            {
                if (paragraph.Text.Contains(keyword))
                {
                    paragraph.ReplaceText(keyword, value);
                }
            }
        }
    }
}
