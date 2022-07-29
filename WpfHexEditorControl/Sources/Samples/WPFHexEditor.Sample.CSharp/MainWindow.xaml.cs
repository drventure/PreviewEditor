﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2020
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST FOR TESTING THE HEXEDITOR... DO NOT WATCH THE CODE LOL ;) 
//////////////////////////////////////////////

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.CharacterTable;
using WpfHexaEditor.Dialog;

namespace WPFHexaEditorExample
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            //FORCE CULTURE
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en");

            InitializeComponent();
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            #region Create file dialog
            var fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true
            };

            if (fileDialog.ShowDialog() == null || !File.Exists(fileDialog.FileName)) return;
            #endregion

            #region if file already open do not open again
            foreach (TabItem ti in FileTab.Items)
                if (ti.ToolTip.ToString() == fileDialog.FileName)
                {
                    ti.IsSelected = true;
                    return;
                }
            #endregion

            #region Open multiple file and add tabs
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            foreach (string file in fileDialog.FileNames)
                FileTab.Items.Add(new TabItem
                {
                    Header = Path.GetFileName(file),
                    ToolTip = file
                });

            FileTab.SelectedIndex = FileTab.Items.Count - 1;
            #endregion

            Application.Current.MainWindow.Cursor = null;
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.SubmitChanges();

            Application.Current.MainWindow.Cursor = null;
        }

        private void CloseFileMenu_Click(object sender, RoutedEventArgs e) => CloseFile();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => HexEdit.CloseProvider();

        private void ExitMenu_Click(object sender, RoutedEventArgs e) => Close();

        private void CopyHexaMenu_Click(object sender, RoutedEventArgs e) => HexEdit.CopyToClipboard(CopyPasteMode.HexaString);

        private void CopyStringMenu_Click(object sender, RoutedEventArgs e) => HexEdit.CopyToClipboard();

        private void DeleteSelectionMenu_Click(object sender, RoutedEventArgs e) => HexEdit.DeleteSelection();

        private void GOPosition_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(PositionText.Text, out var position))
                HexEdit.SetPosition(position, 1);
            else
                MessageBox.Show("Enter long value.");

            ViewMenu.IsSubmenuOpen = false;
        }

        private void PositionText_TextChanged(object sender, TextChangedEventArgs e) =>
            GoPositionButton.IsEnabled = long.TryParse(PositionText.Text, out var _);

        private void UndoMenu_Click(object sender, RoutedEventArgs e) => HexEdit.Undo();

        private void RedoMenu_Click(object sender, RoutedEventArgs e) => HexEdit.Redo();

        private void SetBookMarkButton_Click(object sender, RoutedEventArgs e) => HexEdit.SetBookMark();

        private void DeleteBookmark_Click(object sender, RoutedEventArgs e) => HexEdit.ClearScrollMarker(ScrollMarker.Bookmark);

        private void FindAllSelection_Click(object sender, RoutedEventArgs e) => HexEdit.FindAllSelection(true);

        private void SelectAllButton_Click(object sender, RoutedEventArgs e) => HexEdit.SelectAll();

        private void CTableASCIIButton_Click(object sender, RoutedEventArgs e)
        {
            HexEdit.TypeOfCharacterTable = CharacterTableType.Ascii;
            CTableAsciiButton.IsChecked = true;
            CTableTblButton.IsChecked = false;
            CTableTblDefaultAsciiButton.IsChecked = false;
        }

        private void CTableTBLButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == null) return;
            if (!File.Exists(fileDialog.FileName)) return;

            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.LoadTblFile(fileDialog.FileName);
            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile;
            CTableAsciiButton.IsChecked = false;
            CTableTblButton.IsChecked = true;
            CTableTblDefaultAsciiButton.IsChecked = false;

            Application.Current.MainWindow.Cursor = null;
        }

        private void CTableTBLDefaultASCIIButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile;
            HexEdit.LoadDefaultTbl();

            Application.Current.MainWindow.Cursor = null;
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();

            if (fileDialog.ShowDialog() is not null)
                HexEdit.SubmitChanges(fileDialog.FileName, true);
        }

        private void CTableTblDefaultEBCDICButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile;
            HexEdit.LoadDefaultTbl(DefaultCharacterTableType.EbcdicWithSpecialChar);

            Application.Current.MainWindow.Cursor = null;
        }

        private void CTableTblDefaultEBCDICNoSPButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile;
            HexEdit.LoadDefaultTbl(DefaultCharacterTableType.EbcdicNoSpecialChar);

            Application.Current.MainWindow.Cursor = null;
        }

        private void FindMenu_Click(object sender, RoutedEventArgs e) =>
            new FindWindow(HexEdit, HexEdit.GetSelectionByteArray())
            {
                Owner = this
            }.Show();

        private void ReplaceMenu_Click(object sender, RoutedEventArgs e) =>
            new FindReplaceWindow(HexEdit, HexEdit.GetSelectionByteArray())
            {
                Owner = this
            }.Show();

        private void ReverseSelection_Click(object sender, RoutedEventArgs e) => HexEdit.ReverseSelection();

        private void FileTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not TabControl tc) return;
            if (tc.SelectedValue is not TabItem ti) return;

            //Set the tag of last selected ta to currentstate
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabItem lastSelectedTabItem)
                lastSelectedTabItem.Tag = HexEdit.CurrentState;

            //Change loaded file and update the current state
            var filename = ti.ToolTip.ToString();
            if (!File.Exists(filename)) return;

            HexEdit.FileName = filename;

            //Setstate 
            if (ti.Tag is XDocument doc)
            {
                HexEdit.CurrentState = doc;
                SetReadOnlyMenu.IsChecked = HexEdit.ReadOnlyMode;
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e) => CloseFile();

        private void CloseFile()
        {
            if (FileTab.SelectedIndex == -1) return;

            HexEdit.CloseProvider();
            FileTab.Items.RemoveAt(FileTab.SelectedIndex);
        }

        private void CloseAllFileMenu_Click(object sender, RoutedEventArgs e)
        {
            FileTab.Items.Clear();
            HexEdit.CloseProvider();
        }

        private void SetReadOnlyMenu_Click(object sender, RoutedEventArgs e) =>
            HexEdit.ReadOnlyMode = SetReadOnlyMenu.IsChecked;
    }
}