using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFS_help
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private TFSmodule module = new TFSmodule();
        private List<WorkItemDefination> workItems = new List<WorkItemDefination>();
        private List<string> areaPaths;
        private List<string> iterationsPaths;
        private WorkItem parentWorkItem;

        public MainWindow()
        {
            InitializeComponent();
            this.DataGrid.ItemsSource = workItems;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight > 600)
            {
                DataGrid.MaxHeight = this.ActualHeight * 0.5;
            }
            else
            {
                DataGrid.MaxHeight = 300;
            }

            if(this.ActualWidth>1000)
            {
                TextBoxArea.MaxWidth = 400;
                TextBoxIteration.MaxWidth = 400;
            }
            else
            {
                TextBoxArea.MaxWidth = 300;
                TextBoxIteration.MaxWidth = 300;
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            module.Connect();
            if (module.ConnectionState)
            {
                ConnectionStatus.Text = "Online";
                areaPaths = module.GetNodeCollection(TFSmodule.NodeCollectionType.AreaPaths);
                iterationsPaths = module.GetNodeCollection(TFSmodule.NodeCollectionType.Iterations);

                TextBlockID.Foreground = Brushes.Black;
                TextBlockType.Foreground = Brushes.Black;
                TextBoxID.IsEnabled = true;


                this.AreaColumn.ItemsSource = areaPaths;
                this.IterationColumn.ItemsSource = iterationsPaths;
                this.AssignedToColumn.ItemsSource = module.GetUsersList();
            }
            else
            {
                ConnectionStatus.Text = "Offline";
            }
        }

        private void TextBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (TextBoxID.Text != string.Empty)
                {
                    try
                    {
                        parentWorkItem = module.FindWorkItem(int.Parse(TextBoxID.Text));
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
                if (parentWorkItem != null)
                {
                    ChangeTypeInfo(parentWorkItem.Type);

                    TextBoxArea.Text = parentWorkItem.AreaPath;
                    TextBoxIteration.Text = parentWorkItem.IterationPath;
                    TextBoxTitle.Text = parentWorkItem.Title;
                    TextBoxDescription.Text = parentWorkItem.Description;
                    TextBoxAssignedTo.Text = parentWorkItem.Fields["System.AssignedTo"].Value.ToString();

                    ComboBoxType.ItemsSource = GetChildWorkItemTypes(parentWorkItem.Type.Name);
                    if (ComboBoxType.ItemsSource != null)
                    {
                        DataGrid.IsEnabled = true;
                        ComboBoxType.IsEnabled = true;
                        TextBlockType.IsEnabled = true;
                        ButtonAdd.IsEnabled = true;
                        TextBlockType.Foreground = Brushes.Black;
                    }
                    else
                    {
                        DataGrid.IsEnabled = false;
                        ComboBoxType.IsEnabled = false;
                        ButtonAdd.IsEnabled = false;
                        TextBlockType.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8C8C8"));
                        MessageBox.Show("Can not add child elements for this work item type.");
                    }
                    ParentTypeName.Visibility = Visibility.Visible;
                    TypesColor.Visibility = Visibility.Visible;
                    ParentInfo.Visibility = Visibility.Visible;
                    StackPanelType.Visibility = Visibility.Visible;

                }
            }
        }

        List<string> GetChildWorkItemTypes(string type)
        {
            switch (type)
            {
                case "Feature":
                case "Issue":
                    return new List<string> { "Requirement", "Bug" };
                case "Requirement":
                case "Bug":
                    return new List<string> { "Task" };
                default:
                    return null;
            }
        }

        void ChangeTypeInfo(WorkItemType type)
        {
            switch (type.Name)
            {
                case "Task":
                    TypesColor.Background = Brushes.Yellow;
                    break;
                case "Bug":
                    TypesColor.Background = Brushes.Red;
                    break;
                case "Requirement":
                    TypesColor.Background = Brushes.LightBlue;
                    break;
                case "Issue":
                    TypesColor.Background = Brushes.Purple;
                    break;
                case "Feature":
                    TypesColor.Background = Brushes.DarkMagenta;
                    break;
                default:
                    TypesColor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F4F4"));
                    break;
            }
            ParentTypeName.Text = type.Name.ToUpper();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Empty;
            bool result = module.AddWorkItems(out message, parentWorkItem, workItems, ComboBoxType.Text);
            if (result)
            {
                DataGrid.ItemsSource = null;
                DataGrid.Items.Clear();
                workItems.Clear();
                DataGrid.ItemsSource = workItems;
            }
            MessageBox.Show(message);
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Still in work.");
        }
    }
}
