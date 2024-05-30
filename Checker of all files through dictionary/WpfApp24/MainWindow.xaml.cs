using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp24
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            slovo = new List<string>();
            createFol = new DirectoryInfo("Checked Files");
            createFol.Create();
        }

        FolderBrowserDialog openDialog;
        System.Windows.Forms.OpenFileDialog openFileDialog;
        List<string> slovo;
        string filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        DirectoryInfo createFol;
        int buff = 0;
        int counterOfFiles = 0;
        int pos = 0;
        int counterFiles = 0;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            openDialog = new FolderBrowserDialog();
            btpa.IsEnabled = true;
            btre.IsEnabled = true;
            btstop.IsEnabled = true;
            btstart.IsEnabled = false;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await Task.Run(() => DirSize(new DirectoryInfo(openDialog.SelectedPath)));
                await Task.Run(() => FindFiles(openDialog.SelectedPath));
                await Task.Run(Update);
            }
        }

        public async void DirSize(DirectoryInfo d)
        {
            if (pos == -1)
            {
                Thread.CurrentThread.Abort();
            }
            if (pos == 1)
            {
                Thread.CurrentThread.Suspend();
            }
            if (pos == 2)
            {
                Thread.CurrentThread.Resume();
            }
            try
            {
                FileInfo[] fis = d.GetFiles();
                buff += fis.Length;
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                   await Task.Run(() => DirSize(di));
                }
            }
            catch { }
        }

        public void Update()
        {
            while (true)
            {
                if (pos == -1)
                {
                    break;
                }
                if (pos == 1)
                {
                    Thread.CurrentThread.Suspend();
                }
                if (pos == 2)
                {
                    Thread.CurrentThread.Resume();
                }
                if (Dispatcher.Invoke(() => pb.Value == 100) && counterOfFiles == buff)
                {
                    break;
                }
                if (counterOfFiles != 0 && buff != 0)
                {
                    Dispatcher.Invoke(() => pb.Value = (double)((double)((double)counterOfFiles / (double)buff) * 100));
                }
                Dispatcher.Invoke(() => tbLeft.Text = counterOfFiles.ToString());
                Dispatcher.Invoke(() => tbRight.Text = buff.ToString());
                Dispatcher.Invoke(() => tb.Text = counterFiles.ToString());

            }
        }
        private async void FindFiles(string path)
        {
            if (pos == -1)
            {
                Thread.CurrentThread.Abort();
            }
            if (pos == 1)
            {
                Thread.CurrentThread.Suspend();
            }
            if (pos == 2)
            {
                Thread.CurrentThread.Resume();
            }
            try
            {
                    string[] files = Directory.GetFiles(path);
                    string[] dirs = Directory.GetDirectories(path);


                foreach (string dir in dirs)
                {
                    await Task.Run(() => FindFiles(dir));
                }
                foreach (string file in files)
                {
                    string[] strs = file.Split('\\');
                    int counter = 0;
                    if (strs[strs.Length - 1].Split('.').Length > 1 ? strs[strs.Length - 1].Split('.')[1] == "txt" ? true : false : false)
                    {
                        counter = await Task.Run(() => CheckFiles(file));
                    }
                    lock (this)
                    {
                        counterOfFiles += 1;
                    }
                    if (counter != 0)
                    {
                        lock (this)
                        {
                            counterFiles++;
                            string str;
                            using (FileStream fs = new FileStream("report.txt", FileMode.OpenOrCreate))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    str = sr.ReadToEnd();
                                }
                            }
                            using (FileStream fs = new FileStream("report.txt", FileMode.OpenOrCreate))
                            {
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    sw.WriteLine(str);
                                    sw.WriteLine(file);
                                    sw.WriteLine(new FileInfo(file).Length);
                                    sw.WriteLine(counter);
                                    sw.WriteLine("-------------------------------------------");
                                }
                            }
                        }
                    }
                    counter = 0;
                }
            }
            catch { }
        }

        private async Task<int> CheckFiles(string file)
        {
            if (pos == -1)
            {
                Thread.CurrentThread.Abort();
            }
            if (pos == 1)
            {
                Thread.CurrentThread.Suspend();
            }
            if (pos == 2)
            {
                Thread.CurrentThread.Resume();
            }
            int counter = 0;
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (sr.Peek() > 0)
                        {
                            string str = await sr.ReadLineAsync();
                            string[] strss = str.Split(' ');
                            for (int i = 0; i < strss.Length; i++)
                            {
                                for (int j = 0;  j < slovo.Count;  j++)
                                {
                                    if (strss[i] == slovo[j])
                                    {
                                        counter++;
                                    }
                                }
                            }
                        }
                    }
                }
                if(counter > 0)
                {
                    string i = string.Empty;
                    while (true)
                    {
                        try
                        {
                            lock (this)
                            {
                                File.Copy(file, createFol.FullName + "\\" + $"{i}" + System.IO.Path.GetFileName(file));
                                break;
                            }
                        }
                        catch
                        {
                            i += "1";
                        }
                    }
                    await Task.Run(() => ChangeFile(createFol.FullName + "\\" + $"{i}" + System.IO.Path.GetFileName(file)));
                }
                return counter;
            }
            catch
            {
                return 0;
            }
        }

        private void BGW_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = filter;

            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(openFileDialog.OpenFile()))
                {
                    string strInColumn = string.Empty;
                    while(sr.Peek() > 0)
                    {
                        strInColumn += sr.ReadLine();
                        strInColumn += " ";
                    }
                    string[] str = strInColumn.Split(' ');
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (!String.IsNullOrWhiteSpace(str[i]))
                        {
                            slovo.Add(str[i]);
                        }
                    }
                    
                }
            }
        }

        private async void ChangeFile(string path)
        {
            if(pos == -1)
            {
                Thread.CurrentThread.Abort();
            }
            if (pos == 1)
            {
                Thread.CurrentThread.Suspend();
            }
            if (pos == 2)
            {
                Thread.CurrentThread.Resume();
            }
            string file = string.Empty;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.Peek() > 0)
                    {
                        file += await sr.ReadLineAsync();
                        file += "\n";
                    }
                }
            }
            for (int i = 0; i < slovo.Count; i++)
            {
                file = file.Replace(slovo[i], "*******");
            }
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write))
            { 
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(file);
                }
            }
        }

        private void Stop_Thread(object sender, RoutedEventArgs e)
        {
            pos = -1;
            btstart.IsEnabled = false;
            btpa.IsEnabled = false;
            btre.IsEnabled = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            pos = 1;
            btre.IsEnabled = true;
            btpa.IsEnabled = false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            pos = 2;
            btpa.IsEnabled = true;
            btre.IsEnabled = false;
        }
    }
}
