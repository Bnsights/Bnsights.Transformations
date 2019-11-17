using Bnsights.Transformations.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Bnsights.Transformations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txtConfigPath.Text = Settings.Default["WebConfigPath"].ToString();
            txtTransPath.Text = Settings.Default["TransformationPath"].ToString();
            txtConfigPath.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(TxtConfigPath_MouseUp), true);
            txtTransPath.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(TxtTransPath_MouseUp), true);

        }

        private void BtnBrowseConfigPath_Click(object sender, RoutedEventArgs e)
        {
            var filePath = BrowseFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                txtConfigPath.Text = filePath;
                txtConfigPath.ToolTip = filePath;
            }
        }
        private void BtnBrowseTransPath_Click(object sender, RoutedEventArgs e)
        {
            var filePath = BrowseFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                txtTransPath.Text = filePath;
                txtTransPath.ToolTip = filePath;
            }
        }
        private void TxtConfigPath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnBrowseConfigPath_Click(sender, e);
        }
        private void TxtTransPath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BtnBrowseTransPath_Click(sender, e);
        }

        private void BtnTransform_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTransPath.Text) || string.IsNullOrEmpty(txtConfigPath.Text))
            {
                MessageBox.Show("Both files are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!System.IO.File.Exists(txtConfigPath.Text))
            {
                MessageBox.Show("Invalid Web.Config file path.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!System.IO.File.Exists(txtTransPath.Text))
            {
                MessageBox.Show("Invalid Transformation file path.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                RemoveReadOnlyAttribute(txtConfigPath.Text);
                if (TransformWebConfig(txtConfigPath.Text, txtTransPath.Text))
                {
                    MessageBox.Show("Transformation Successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Transformation Failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Settings.Default["WebConfigPath"] = txtConfigPath.Text;
                Settings.Default["TransformationPath"] = txtTransPath.Text;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transformation Failed\r\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private string BrowseFile()
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            string filePath = null;
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    filePath = fileDialog.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    //txtConfigPath.Text = null;
                    //txtConfigPath.ToolTip = null;
                    break;
            }
            return filePath;
        }
        private bool TransformWebConfig(string webConfigPath, string transformationFilePath)
        {
            var config = new Microsoft.Web.XmlTransform.XmlTransformableDocument();
            config.PreserveWhitespace = true;
            config.Load(webConfigPath);
            var transformation = new Microsoft.Web.XmlTransform.XmlTransformation(transformationFilePath);

            if (transformation.Apply(config))
            {
                config.Save(webConfigPath);
                return true;
            }
            else
            {
                return false;
            }
        }
        private static void RemoveReadOnlyAttribute(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                attributes = attributes & ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attributes);
            }
        }


    }
}
