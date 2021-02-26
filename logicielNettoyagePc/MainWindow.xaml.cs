using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace logicielNettoyagePc
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string version = "1.0.0";
        //dossiers à nettoyer
        public DirectoryInfo winTamp; //DirectoryInfo est une classe qui permet de récupérer le poids des fichiers. Elle a d'autres fonctionnalités
        public DirectoryInfo appTamp;
        public MainWindow()
        {
            InitializeComponent();
            //Définir le chemin vers les deux dossiers winTamp et appTamp
            winTamp = new DirectoryInfo(@"C:\windows\Temp"); //dossier temporaire windows
            appTamp = new DirectoryInfo(System.IO.Path.GetTempPath()); //fichier temporaire des appli. GetTempPath() permet de retourner le bon chemin de fichiers peu importe où il se trouve dans le pc
            checkActu();
        }

        public void checkActu()
        {
            string url = "http://localhost/dev/logicielNettoyage/actu.txt";
            using (WebClient client = new WebClient())
            {
                string actu = client.DownloadString(url);
                if(actu != String.Empty)
                {
                    actuTxt.Content = actu;
                    actuTxt.Visibility = Visibility.Visible;
                    bandeau.Visibility = Visibility.Visible;
                }
            }
        }

        public void checkVersion()
        {
            string url = "http://localhost/dev/logicielNettoyage/version.txt";
            using (WebClient client = new WebClient())
            {
                string v = client.DownloadString(url);
                if(version != v)
                {
                    MessageBox.Show("Une mise à jour est disponible !", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Votre logiciel est à jour !", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        //Calculer le poids des dossiers
        public long DirSize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) + dir.GetDirectories().Sum(di => DirSize(di)); //GetFiles() va récupérer tous les fichiers dans le dossier. Sum() et nous être retourné
        }

        //Vider un dossier
        public void CleartempData(DirectoryInfo di) //prend en paramètre un dossier
        {
            foreach (FileInfo file in di.GetFiles()) //boucle foreach pour chaque fichier dans le dossier
                {
                    try
                    //try car peut planter dans le cas où on n'a pas les autorisations nécessaires pour supprimer un fichier windows
                    {
                        file.Delete(); 
                        Console.WriteLine(file.FullName); //indique le fichier supprimé
                        //totalRemovedFiles++;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    Console.WriteLine(dir.FullName);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void btn_MAJ_Click(object sender, RoutedEventArgs e)
        {
            checkVersion();
        }

        private void btn_clean_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Nettoyage en cours ...");
            btn_clean.Content = "NETTOYAGE EN COURS";

            Clipboard.Clear();

            try
            {
                CleartempData(winTamp);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
            try
            {
                CleartempData(appTamp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }

            btn_clean.Content = "NETTOYAGE TERMINE";
            titre.Content = "Nettoyage effectué !";
            espace.Content = "O Mb";

        }

        private void btn_hist_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Todo : créer page historique !", "Historique", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_web_Click(object sender, RoutedEventArgs e)
        {
            try //si l'utilisateur n'a pas de navigateur, on catch l'erreur (envoi d'un message d'erreur) sinon try
            {
                Process.Start(new ProcessStartInfo("https://alexandre-de-sousa.com") //Pour ouvrir un lien url dans le navigateur
                {
                    UseShellExecute = true //ouvre avec le navigateur de l'utilisateur
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
            
        }

        private void btn_analyse_Click(object sender, RoutedEventArgs e)
        {
            analyseFolders();
        }

        public void analyseFolders()
        {
            Console.WriteLine("Début de l'analyse...");
            long totalSize = 0;

            try
            {
                totalSize += DirSize(winTamp) / 1000000; //totalSize est égal à lui même + la taille du dossier temp de windows divisé par 1million (car retourné en octets) 
                totalSize += DirSize(appTamp) / 1000000;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Impossible d'analyser les dossiers :" + ex.Message);
            }
            

            espace.Content = totalSize + "Mb"; //on affiche le nombre de méga que contient l'espace analysé
            titre.Content = "Analyse effectuée !";
            date.Content = DateTime.Today; //retourne le jour actuel
        }
    }
}
